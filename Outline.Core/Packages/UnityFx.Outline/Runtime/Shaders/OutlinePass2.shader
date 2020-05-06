// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

// Renders outline based on a texture produces by 'UnityF/Outline/RenderColor' output.
// Modified version of 'Custom/Post Outline' shader taken from https://willweissman.wordpress.com/tutorials/shaders/unity-shaderlab-object-outlines/.
Shader "UnityFx/Outline/VPassBlend"
{
	Properties
	{
		_Width("Outline thickness (in pixels)", Range(1, 32)) = 5
		_Intensity("Outline intensity", Range(0.1, 100)) = 2
		_Color("Outline color", Color) = (1, 0, 0, 1)
	}

	HLSLINCLUDE

		#include "OutlineCommon.hlsl"

		CBUFFER_START(UnityPerMaterial)
			float _Intensity;
			int _Width;
			float4 _Color;
		CBUFFER_END

		UNITY_DECLARE_TEX2D(_MainTex);
		float2 _MainTex_TexelSize;
		UNITY_DECLARE_TEX2D(_MaskTex);
		float _GaussSamples[32];

		float4 frag(v2f_img i) : SV_Target
		{
			if (UNITY_SAMPLE_TEX2D(_MaskTex, i.uv).r > 0)
			{
				discard;
			}

			float TX_y = _MainTex_TexelSize.y;
			float intensity;
			int n = _Width;

			for (int k = -n; k <= _Width; k += 1)
			{
				intensity += UNITY_SAMPLE_TEX2D(_MainTex, i.uv + float2(0, k * TX_y)).r * _GaussSamples[abs(k)];
			}

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

		Blend SrcAlpha OneMinusSrcAlpha

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

		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			ENDHLSL
		}
	}
}
