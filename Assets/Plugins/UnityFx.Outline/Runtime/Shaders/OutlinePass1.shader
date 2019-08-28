// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

// Renders outline based on a texture produces by 'UnityF/Outline/RenderColor' output.
// Modified version of 'Custom/Post Outline' shader taken from https://willweissman.wordpress.com/tutorials/shaders/unity-shaderlab-object-outlines/.
Shader "UnityFx/Outline/HPass"
{
	Properties
	{
		_Width("Outline thickness (in pixels)", Range(1, 32)) = 5
	}

	SubShader
	{
		ZWrite Off
		ZTest Always
		Lighting Off

		Pass
		{
			CGPROGRAM

			#pragma multi_compile _MODE_SOLID _MODE_BLURRED
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MaskTex;
			float2 _MaskTex_TexelSize;
			int _Width;

#if _MODE_BLURRED
			float _GaussSamples[32];
#endif

			struct v2f
			{
				float4 pos : POSITION;
				float2 uvs : TEXCOORD0;
			};

			v2f vert(appdata_base v)
			{
				v2f o;

				o.pos = UnityObjectToClipPos(v.vertex);
				o.uvs = ComputeScreenPos(o.pos);

				return o;
			}

			half frag(v2f i) : COLOR
			{
				int n = _Width * 2 - 1;

				float TX_x = _MaskTex_TexelSize.x;
				float intensity;
				float n2 = _Width;

				for (int k = -n2; k <= n2; k += 1)
				{
#if _MODE_BLURRED
					intensity += tex2D(_MaskTex, i.uvs.xy + float2(k * TX_x, 0)).r * _GaussSamples[abs(k)];
#else
					intensity += tex2D(_MaskTex, i.uvs.xy + float2(k * TX_x, 0)).r;
#endif
				}

				return intensity;
			}

			ENDCG
		}
	}
}
