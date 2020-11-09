// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

// Renders outline based on a texture produced with 'UnityF/OutlineColor'.
// Modified version of 'Custom/Post Outline' shader taken from https://willweissman.wordpress.com/tutorials/shaders/unity-shaderlab-object-outlines/.
Shader "Hidden/UnityFx/Outline.URP"
{
	HLSLINCLUDE

		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

		TEXTURE2D_X(_MaskTex);
		SAMPLER(sampler_MaskTex);

		TEXTURE2D_X(_MainTex);
		SAMPLER(sampler_MainTex);
		float2 _MainTex_TexelSize;

		float4 _Color;
		float _Intensity;
		int _Width;
		float _GaussSamples[32];

		struct Varyings
		{
			float4 positionCS : SV_POSITION;
			float2 uv : TEXCOORD0;
			UNITY_VERTEX_OUTPUT_STEREO
		};

#if SHADER_TARGET < 35 || _USE_DRAWMESH

		struct Attributes
		{
			float4 positionOS : POSITION;
			float2 uv : TEXCOORD0;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		Varyings VertexSimple(Attributes input)
		{
			Varyings output = (Varyings)0;

			UNITY_SETUP_INSTANCE_ID(input);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

			float4 pos = TransformObjectToHClip(input.positionOS.xyz);

			output.positionCS = float4(pos.xy, UNITY_NEAR_CLIP_VALUE, 1);
			output.uv = ComputeScreenPos(output.positionCS).xy;

			return output;
		}

#else

		struct Attributes
		{
			uint vertexID : SV_VertexID;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		Varyings VertexSimple(Attributes input)
		{
			Varyings output = (Varyings)0;

			UNITY_SETUP_INSTANCE_ID(input);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

			output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
			output.uv = GetFullScreenTriangleTexCoord(input.vertexID);

			return output;
		}

#endif

		float CalcIntensity(float2 uv, float2 offset)
		{
			float intensity = 0;

			// Accumulates horizontal or vertical blur intensity for the specified texture position.
			// Set offset = (tx, 0) for horizontal sampling and offset = (0, ty) for vertical.
			for (int k = -_Width; k <= _Width; ++k)
			{
				intensity += SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, uv + k * offset).r * _GaussSamples[abs(k)];
			}

			return intensity;
		}

		float4 FragmentH(Varyings i) : SV_Target
		{
			UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

			float2 uv = UnityStereoTransformScreenSpaceTex(i.uv);
			float intensity = CalcIntensity(uv, float2(_MainTex_TexelSize.x, 0));
			return float4(intensity, intensity, intensity, 1);
		}

		float4 FragmentV(Varyings i) : SV_Target
		{
			UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

			float2 uv = UnityStereoTransformScreenSpaceTex(i.uv);

			if (SAMPLE_TEXTURE2D_X(_MaskTex, sampler_MaskTex, uv).r > 0)
			{
				// TODO: Avoid discard/clip to improve performance on mobiles.
				discard;
			}

			float intensity = CalcIntensity(uv, float2(0, _MainTex_TexelSize.y));
			intensity = _Intensity > 99 ? step(0.01, intensity) : intensity * _Intensity;
			return float4(_Color.rgb, saturate(_Color.a * intensity));
		}

	ENDHLSL

	// SM3.5+
	SubShader
	{
		Tags{ "RenderPipeline" = "UniversalPipeline" }

		Cull Off
		ZWrite Off
		ZTest Always
		Lighting Off

		Pass
		{
			Name "HPass"

			HLSLPROGRAM

			#pragma target 3.5
			#pragma multi_compile_instancing
			#pragma shader_feature_local _USE_DRAWMESH
			#pragma vertex VertexSimple
			#pragma fragment FragmentH

			ENDHLSL
		}

		Pass
		{
			Name "VPassBlend"
			Blend SrcAlpha OneMinusSrcAlpha

			HLSLPROGRAM

			#pragma target 3.5
			#pragma multi_compile_instancing
			#pragma shader_feature_local _USE_DRAWMESH
			#pragma vertex VertexSimple
			#pragma fragment FragmentV

			ENDHLSL
		}
	}

	// SM2.0
	SubShader
	{
		Tags { "RenderPipeline" = "UniversalPipeline" }

		Cull Off
		ZWrite Off
		ZTest Always
		Lighting Off

		Pass
		{
			Name "HPass"

			HLSLPROGRAM

			#pragma vertex VertexSimple
			#pragma fragment FragmentH

			ENDHLSL
		}

		Pass
		{
			Name "VPassBlend"
			Blend SrcAlpha OneMinusSrcAlpha

			HLSLPROGRAM

			#pragma vertex VertexSimple
			#pragma fragment FragmentV

			ENDHLSL
		}
	}
}
