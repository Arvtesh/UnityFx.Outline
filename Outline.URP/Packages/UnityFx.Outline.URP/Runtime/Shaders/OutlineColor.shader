// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

// Renders everything with while color.
// Modified version of 'Custom/DrawSimple' shader taken from https://willweissman.wordpress.com/tutorials/shaders/unity-shaderlab-object-outlines/.
Shader "Hidden/UnityFx/OutlineColor.URP"
{
	HLSLINCLUDE

		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

		struct Attributes
		{
			float4 positionOS : POSITION;
			float2 uv : TEXCOORD0;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct Varyings
		{
			float4 positionCS : SV_POSITION;
			float2 uv : TEXCOORD0;
		};

		TEXTURE2D_X(_MainTex);
		SAMPLER(sampler_MainTex);

		Varyings VertexSimple(Attributes input)
		{
			Varyings output = (Varyings)0;

			UNITY_SETUP_INSTANCE_ID(input);
			UNITY_TRANSFER_INSTANCE_ID(input, output);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

			VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);

			output.uv = input.uv;
			output.positionCS = vertexInput.positionCS;

			return output;
		}

		half4 FragmentSimple(Varyings input) : SV_Target
		{
			return 1;
		}

		half4 FragmentAlphaTest(Varyings input) : SV_Target
		{
			UNITY_SETUP_INSTANCE_ID(input);
			UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

			half4 c = SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, input.uv);
			AlphaDiscard(c.a, 1);
			return 1;
		}

	ENDHLSL

	SubShader
	{
		Tags { "RenderPipeline"="UniversalRenderPipeline"}

		Cull Off
		ZWrite Off
		ZTest LEqual
		Lighting Off

		Pass
		{
			Name "Opaque"

			HLSLPROGRAM

			#pragma vertex VertexSimple
			#pragma fragment FragmentSimple

			ENDHLSL
		}

		Pass
		{
			Name "Transparent"

			HLSLPROGRAM

			#pragma shader_feature _ALPHATEST_ON
			#pragma vertex VertexSimple
			#pragma fragment FragmentAlphaTest

			ENDHLSL
		}
	}
}
