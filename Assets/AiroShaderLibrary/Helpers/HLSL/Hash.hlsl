#ifndef HASHHELPERS_HLSL_INCLUDED
#define HASHHELPERS_HLSL_INCLUDED

void Hash21_float(float2 In, out float Out){
	uint2 v = (uint2) (int2) round(In);
	v.y ^= 1103515245U;
	v.x += v.y;
	v.x *= v.y;
	v.x ^= v.x >> 5u;
	v.x *= 0x27d4eb2du;
	Out = (v.x >> 8) * (1.0 / float(0x00ffffff));
}

void Hash23_float(float2 In, out float3 Out){
	float3 v = float3 (
		dot(In.xyx, float3(127.1, 311.7, 74.7)),
		dot(In.yxx, float3(269.5, 183.3, 246.1)),
		dot(In.yyx, float3(113.5, 271.9, 124.6))
	);
	Out = frac(sin(v)*43758.5453123);
}

#endif