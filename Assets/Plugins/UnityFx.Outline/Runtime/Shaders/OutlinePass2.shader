// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
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
			#include "UnityCG.cginc"

			UNITY_DECLARE_TEX2D(_MaskTex);
			float2 _MaskTex_TexelSize;
			UNITY_DECLARE_TEX2D(_HPassTex);
			float2 _HPassTex_TexelSize;
			float4 _Color;
			float _Intensity;
			int _Width;
			float _GaussSamples[32];

			struct v2f
			{
				float4 pos : POSITION;
				float2 uvs : TEXCOORD0;
			};

			v2f vert(appdata_base v)
			{
				v2f o;

				//o.pos = UnityObjectToClipPos(v.vertex);

				o.pos = float4(v.vertex.xy, 0.0, 1.0);
				o.uvs = ComputeScreenPos(o.pos);

				return o;
			}

			float4 frag(v2f i) : COLOR
			{
				if (UNITY_SAMPLE_TEX2D(_MaskTex, i.uvs.xy).r > 0)
				{
					discard;
				}

				float TX_y = _MaskTex_TexelSize.y;
				float intensity;
				int n = _Width;

				for (int k = -n; k <= _Width; k += 1)
				{
					intensity += UNITY_SAMPLE_TEX2D(_HPassTex, i.uvs.xy + float2(0, k * TX_y)).r * _GaussSamples[abs(k)];
				}

				intensity = _Intensity > 99 ? step(0.01, intensity) : intensity * _Intensity;
				return float4(_Color.rgb, saturate(_Color.a * intensity));
			}

			ENDHLSL
		}
	}
}
