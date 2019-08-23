// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

// Renders outline based on a texture produces by 'UnityF/Outline/RenderColor' output.
// Modified version of 'Custom/Post Outline' shader taken from https://willweissman.wordpress.com/tutorials/shaders/unity-shaderlab-object-outlines/.

// TODO: Blur the outline.
Shader "UnityFx/Outline/PostProcess"
{
	Properties
	{
		_Color("Outline Color", Color) = (1, 0, 0, 1)
		_Width("Outline Thickness", Range(1, 30)) = 5
		_MainTex("Main Texture", 2D) = "white"{}
	}

	SubShader
	{
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;

			// <SamplerName>_TexelSize is a float2 that says how much screen space a texel occupies.
			float2 _MainTex_TexelSize;
			float4 _Color;
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

			half4 frag(v2f i) : COLOR
			{
				// If something already exists underneath the fragment, discard the fragment.
				if (tex2D(_MainTex, i.uvs.xy).r > 0)
				{
					discard;
				}

				// Split texel size into smaller words.
				float TX_x = _MainTex_TexelSize.x;
				float TX_y = _MainTex_TexelSize.y;

				// And a final intensity that increments based on surrounding intensities.
				float colorIntensityInRadius;

				int n = _Width;
				float n2 = (float)n / 2;

				// For every iteration we need to do horizontally.
				for (int k = 0; k < n; k += 1)
				{
					// For every iteration we need to do vertically.
					for (int j = 0; j < n; j += 1)
					{
						// Increase our output color by the pixels in the area.
						colorIntensityInRadius += tex2D(_MainTex, i.uvs.xy + float2((k - n2) * TX_x, (j - n2) * TX_y)).r;
					}
				}

				return colorIntensityInRadius * _Color;
			}

			ENDCG
		}
	}
}
