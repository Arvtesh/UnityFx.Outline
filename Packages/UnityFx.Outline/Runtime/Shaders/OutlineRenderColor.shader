// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

// Renders everything with while color.
// Modified version of 'Custom/DrawSimple' shader taken from https://willweissman.wordpress.com/tutorials/shaders/unity-shaderlab-object-outlines/.
Shader "UnityFx/Outline/RenderColor"
{
	SubShader
	{
		Cull Off
		ZWrite Off
		ZTest Always
		Lighting Off

		Pass
		{
			HLSLPROGRAM

			#pragma vertex Vert
			#pragma fragment Frag
			#include "UnityCG.cginc"

			struct v2f
			{
				float4 pos: POSITION;
			};

			v2f Vert(v2f i)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(i.pos);
				return o;
			}

			half4 Frag(): COLOR0
			{
				return half4(1, 1, 1, 1);
			}

			ENDHLSL
		}
	}
}
