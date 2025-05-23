﻿#ifndef CMSA_VERTEXUTILS_INCLUDED
#define CMSA_VERTEXUTILS_INCLUDED
#include "CMSAUtils.cginc"

float _FxInner;
float _FxOuter;
fixed3 _Color;

struct v2f {
	float4 vertex : SV_POSITION;
	float2 sPos : TEXCOORD0;
	fixed a : TEXCOORD1;
	UNITY_VERTEX_OUTPUT_STEREO
};

v2f vert (appdata i) {
	v2f o;
	UNITY_SETUP_INSTANCE_ID(i);
    UNITY_INITIALIZE_OUTPUT(v2f, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	
	o.vertex = i.vertex;
	float p = (_FxInner * i.uv.x) + (_FxOuter * i.uv.y);
	o.a = 1-(i.uv.x);
	o.vertex.xy = lerp(o.vertex.xy, 0, p);
	o.vertex.z = UNITY_NEAR_CLIP_VALUE;
	o.vertex.xy *= 2.4;

	// Reproject for asymmetric FOV
	o.vertex.xy -= mul(UNITY_MATRIX_P, half4(0, 0, CLIP_FAR, 1));

	o.sPos = ComputeNonStereoScreenPos(o.vertex);
	return o;
}

inline fixed4 fragBody(v2f v, fixed a){
	#if TUNNEL_SKYBOX
		half4 vPos = screenCoords(v.sPos);
		return fixed4(sampleSkybox(vPos) * _Color, a);
	#else
		return fixed4(_Color, a);
	#endif
}
#endif