#ifndef OUTLINE_COMMON_INCLUDED
#define OUTLINE_COMMON_INCLUDED

#include "UnityCG.cginc"

#if SHADER_TARGET < 35

v2f_img vert(appdata_img v)
{
	v2f_img o;
	UNITY_INITIALIZE_OUTPUT(v2f_img, o);
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	o.pos = float4(v.vertex.xy, UNITY_NEAR_CLIP_VALUE, 1);
	o.uv = ComputeScreenPos(o.pos);

	return o;
}

#else

struct appdata_vid
{
	uint vertexID : SV_VertexID;
};

v2f_img vert_vid(appdata_vid v)
{
	v2f_img o;
	UNITY_INITIALIZE_OUTPUT(v2f_img, o);
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	// Generate a triangle in homogeneous clip space, s.t.
	// v0 = (-1, -1, 1), v1 = (3, -1, 1), v2 = (-1, 3, 1).
	float2 uv = float2((v.vertexID << 1) & 2, v.vertexID & 2);
	o.pos = float4(uv * 2 - 1, UNITY_NEAR_CLIP_VALUE, 1);

#if UNITY_UV_STARTS_AT_TOP
	o.uv = half2(uv.x, 1 - uv.y);
#else
	o.uv = uv;
#endif

	return o;
}

#endif

#endif	// OUTLINE_COMMON_INCLUDED
