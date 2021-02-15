// Copyright (C) 2019-2021 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

// Renders everything with while color.
// Modified version of 'Custom/DrawSimple' shader taken from https://willweissman.wordpress.com/tutorials/shaders/unity-shaderlab-object-outlines/.
Shader "Hidden/UnityFx/OutlineColor.URP"
{
	HLSLINCLUDE

		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		#include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"

		TEXTURE2D(_MainTex);
		SAMPLER(sampler_MainTex);

		half _Cutoff;

		half4 FragmentSimple(Varyings input) : SV_Target
		{
			return 1;
		}

		half4 FragmentAlphaTest(Varyings input) : SV_Target
		{
			half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
			clip(c.a - _Cutoff);
			return 1;
		}

	ENDHLSL

	SubShader
	{
		Tags { "RenderPipeline" = "UniversalPipeline" }

		Cull Off
		ZWrite Off
		ZTest LEqual
		Lighting Off

		Pass
		{
			Name "Opaque"

			HLSLPROGRAM

			#pragma multi_compile_instancing
			#pragma vertex Vert
			#pragma fragment FragmentSimple

			ENDHLSL
		}

		Pass
		{
			Name "Transparent"

			HLSLPROGRAM

			#pragma multi_compile_instancing
			#pragma vertex Vert
			#pragma fragment FragmentAlphaTest

			ENDHLSL
		}
	}
}
