// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

// Renders outline based on a texture produces by 'UnityF/Outline/RenderColor' output.
// Modified version of 'Custom/Post Outline' shader taken from https://willweissman.wordpress.com/tutorials/shaders/unity-shaderlab-object-outlines/.
Shader "UnityFx/Outline/HPass"
{
	Properties
	{
		_Width("Outline Thickness", Range(1, 32)) = 5
		_MainTex("Mask Texture", 2D) = "white"{}
	}

	SubShader
	{
		ZWrite Off
		ZTest Always
		Lighting Off

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;

			// <SamplerName>_TexelSize is a float2 that says how much screen space a texel occupies.
			float2 _MainTex_TexelSize;
			int _Width;

			struct v2f
			{
				float4 pos : POSITION;
				float2 uvs : TEXCOORD0;
			};

			v2f vert(appdata_base v)
			{
				v2f o;

				// Despite the fact that we are only drawing a quad to the screen, Unity requires us to multiply vertices by our MVP matrix, presumably to keep things working when inexperienced people try copying code from other shaders.
				o.pos = UnityObjectToClipPos(v.vertex);

				// Also, we need to fix the UVs to match our screen space coordinates.
				o.uvs = ComputeScreenPos(o.pos);

				return o;
			}

			half frag(v2f i) : COLOR
			{
				float TX_x = _MainTex_TexelSize.x;
				float colorIntensityInRadius;
				int n = _Width;
				float n2 = (float)n / 2;

				for (int k = 0; k < n; k += 1)
				{
					colorIntensityInRadius += tex2D(_MainTex, i.uvs.xy + float2((k - n2) * TX_x, 0)).r / n;
				}

				return colorIntensityInRadius;
			}

			ENDCG
		}
	}
}
