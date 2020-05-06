// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

// Renders outline based on a texture produces by 'UnityF/Outline/RenderColor' output.
// Modified version of 'Custom/Post Outline' shader taken from https://willweissman.wordpress.com/tutorials/shaders/unity-shaderlab-object-outlines/.
Shader "UnityFx/Outline/HPass"
{
	Properties
	{
		_Width("Outline thickness (in pixels)", Range(1, 32)) = 5
	}

	HLSLINCLUDE

		#include "OutlineCommon.hlsl"

		CBUFFER_START(UnityPerMaterial)
			int _Width;
		CBUFFER_END

		UNITY_DECLARE_TEX2D(_MainTex);
		float2 _MainTex_TexelSize;
		float _GaussSamples[32];

		float frag(v2f_img i) : SV_Target
		{
			float TX_x = _MainTex_TexelSize.x;
			float intensity;
			int n = _Width;

			for (int k = -n; k <= n; k += 1)
			{
				intensity += UNITY_SAMPLE_TEX2D(_MainTex, i.uv + float2(k * TX_x, 0)).r * _GaussSamples[abs(k)];
			}

			return intensity;
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
			HLSLPROGRAM

			#pragma target 3.5
			#pragma vertex vert_vid
			#pragma fragment frag

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
			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			ENDHLSL
		}
	}
}
