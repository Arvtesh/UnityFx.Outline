// Copyright (C) 2019-2021 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

// Renders outline based on a texture produced with 'UnityF/OutlineColor'.
// Modified version of 'Custom/Post Outline' shader taken from https://willweissman.wordpress.com/tutorials/shaders/unity-shaderlab-object-outlines/.
Shader "Hidden/UnityFx/Outline"
{
	HLSLINCLUDE

		#include "UnityCG.cginc"

		UNITY_DECLARE_SCREENSPACE_TEXTURE(_MaskTex);
		UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
		float2 _MainTex_TexelSize;

		float4 _Color;
		float _Intensity;
		int _Width;
		float _GaussSamples[32];

#if SHADER_TARGET < 35 || _USE_DRAWMESH

		v2f_img vert(appdata_img v)
		{
			v2f_img o;
			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_INITIALIZE_OUTPUT(v2f_img, o);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

			o.pos = float4(v.vertex.xy, UNITY_NEAR_CLIP_VALUE, 1);
			o.uv = ComputeScreenPos(o.pos);

			return o;
		}

#else

		struct appdata_vid
		{
			uint vertexID : SV_VertexID;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		float4 GetFullScreenTriangleVertexPosition(uint vertexID, float z = UNITY_NEAR_CLIP_VALUE)
		{
			// Generates a triangle in homogeneous clip space, s.t.
			// v0 = (-1, -1, 1), v1 = (3, -1, 1), v2 = (-1, 3, 1).
			float2 uv = float2((vertexID << 1) & 2, vertexID & 2);
			return float4(uv * 2 - 1, z, 1);
		}

		v2f_img vert(appdata_vid v)
		{
			v2f_img o;
			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_INITIALIZE_OUTPUT(v2f_img, o);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

			o.pos = GetFullScreenTriangleVertexPosition(v.vertexID);
			o.uv = ComputeScreenPos(o.pos);

			return o;
		}

#endif

		float CalcIntensityN0(float2 uv, float2 offset, int k)
		{
			return UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, uv + k * offset).r * _GaussSamples[k];
		}

		float CalcIntensityN1(float2 uv, float2 offset, int k)
		{
			return UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, uv - k * offset).r * _GaussSamples[k];
		}

		float CalcIntensity(float2 uv, float2 offset)
		{
			float intensity = 0;

			// Accumulates horizontal or vertical blur intensity for the specified texture position.
			// Set offset = (tx, 0) for horizontal sampling and offset = (0, ty) for vertical.
			// 
			// NOTE: Unroll directive is needed to make the method function on platforms like WebGL 1.0 where loops are not supported.
			// If maximum outline width is changed here, it should be changed in OutlineResources.MaxWidth as well.
			//
			[unroll(32)]
			for (int k = 1; k <= _Width; ++k)
			{
				intensity += CalcIntensityN0(uv, offset, k);
				intensity += CalcIntensityN1(uv, offset, k);
			}

			intensity += CalcIntensityN0(uv, offset, 0);
			return intensity;
		}

		float4 frag_h(v2f_img i) : SV_Target
		{
			UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

			float intensity = CalcIntensity(i.uv, float2(_MainTex_TexelSize.x, 0));
			return float4(intensity, intensity, intensity, 1);
		}

		float4 frag_v(v2f_img i) : SV_Target
		{
			UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

			if (UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MaskTex, i.uv).r > 0)
			{
				// TODO: Avoid discard/clip to improve performance on mobiles.
				discard;
			}

			float intensity = CalcIntensity(i.uv, float2(0, _MainTex_TexelSize.y));
			intensity = _Intensity > 99 ? step(0.01, intensity) : intensity * _Intensity;
			return float4(_Color.rgb, saturate(_Color.a * intensity));
		}

	ENDHLSL

	// SM3.5+
	SubShader
	{
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
			#pragma vertex vert
			#pragma fragment frag_h

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
			#pragma vertex vert
			#pragma fragment frag_v

			ENDHLSL
		}
	}

	// SM2.0
	SubShader
	{
		Cull Off
		ZWrite Off
		ZTest Always
		Lighting Off

		Pass
		{
			Name "HPass"

			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag_h

			ENDHLSL
		}

		Pass
		{
			Name "VPassBlend"
			Blend SrcAlpha OneMinusSrcAlpha

			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag_v

			ENDHLSL
		}
	}
}
