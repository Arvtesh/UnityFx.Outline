// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

// Renders everything with while color.
// Modified version of 'Custom/DrawSimple' shader taken from https://willweissman.wordpress.com/tutorials/shaders/unity-shaderlab-object-outlines/.
Shader "UnityFx/Outline/RenderColor"
{
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

			struct v2f
			{
				float4 pos: POSITION;
			};

			v2f vert(v2f i)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(i.pos);
				return o;
			}

			half4 frag(): COLOR0
			{
				return half4(1, 1, 1, 1);
			}

			ENDCG
		}
	}
}
