#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
 StructuredBuffer<float3> _Positions;
#endif

float _PointScale;
float _Step;

// Not working properly for URP dunno why
void procedural_instancing () {
	#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
	 float scale = _PointScale;
	 float3 position = _Positions[unity_InstanceID];
	 position *= 8;
	 position.y += 10.5;

	 /*
			transform matrix
			  0   1   2   3
			-----------------
		0	|s.x| 0 | 0 |p.x|
		1	| 0 |s.y| 0 |p.y|
		2	| 0 | 0 |s.z|p.z|
		3	| 0 | 0 | 0 | 1 | <- m[3, 3]
			-----------------
	 */
	 unity_ObjectToWorld = 0.0;
	 unity_ObjectToWorld._m03_m13_m23_m33 = float4(position, 1.0);
	 unity_ObjectToWorld._m00_m11_m22 = _Step*scale; // scale diagonal
	#endif 
}

// Not working properly for URP dunno why
// void procedural_instancing () {
// 	#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
// 	 float3 position = _Positions[unity_InstanceID];
// 	 unity_ObjectToWorld = 0.0;
// 	 unity_ObjectToWorld._m03_m13_m23_m33 = float4(position, 1.0);
// 	 unity_ObjectToWorld._m00_m11_m22 = _Step;
// 	#endif 
// }

void ShaderGraphFunction_float(float3 In, out float3 Out){
	Out = In;
}

void ShaderGraphFunction_half(half3 In, out half3 Out){
	Out = In;
}
