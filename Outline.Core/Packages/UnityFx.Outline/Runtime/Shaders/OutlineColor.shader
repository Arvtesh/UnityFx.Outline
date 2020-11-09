// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

// Renders everything with while color.
// Modified version of 'Custom/DrawSimple' shader taken from https://willweissman.wordpress.com/tutorials/shaders/unity-shaderlab-object-outlines/.
Shader "Hidden/UnityFx/OutlineColor"
{
	HLSLINCLUDE

		#include "UnityCG.cginc"

		UNITY_DECLARE_TEX2D(_MainTex);
		float _Cutoff;

		v2f_img vert(appdata_img v)
		{
			v2f_img o;
			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_INITIALIZE_OUTPUT(v2f_img, o);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv = v.texcoord;

			return o;
		}

		half4 frag() : SV_Target
		{
			return 1;
		}

		half4 frag_clip(v2f_img i) : SV_Target
		{
			UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

			half4 c = UNITY_SAMPLE_TEX2D(_MainTex, i.uv);
			clip(c.a - _Cutoff);
			return 1;
		}

	ENDHLSL

	SubShader
	{
		Cull Off
		ZWrite Off
		ZTest LEqual
		Lighting Off

		Pass
		{
			Name "Opaque"

			HLSLPROGRAM

			#pragma multi_compile_instancing
			#pragma vertex vert
			#pragma fragment frag

			ENDHLSL
		}

		Pass
		{
			Name "Transparent"

			HLSLPROGRAM

			#pragma multi_compile_instancing
			#pragma vertex vert
			#pragma fragment frag_clip

			ENDHLSL
		}
	}
}
