#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
 StructuredBuffer<uint> _Hashes;
 StructuredBuffer<float3> _Positions;
 StructuredBuffer<float3> _Normals;
#endif

float4 _Config;

void procedural_instancing () {
	#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
	 float scale = _Config.y;
	 float3 displacement = _Config.z * ((1.0 / 255.0) * (_Hashes[unity_InstanceID] >> 24) - 0.5);
	 displacement *= _Normals[unity_InstanceID];
	 

	 unity_ObjectToWorld = 0.0;
	 unity_ObjectToWorld._m03_m13_m23_m33 = float4(_Positions[unity_InstanceID], 1.0);
	 unity_ObjectToWorld._m03_m13_m23 += displacement;
	 unity_ObjectToWorld._m00_m11_m22 = scale;
	#endif 
}

float3 GetColor(){
	#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
	 uint hash = _Hashes[unity_InstanceID];
	 return (1.0/255.0) * float3(
	 	hash & 0xFF,
	 	(hash >>  8) & 0xFF,
	 	(hash >> 16) & 0xFF
	 );
	#else
	 return 1.0;
	#endif
}

void ShaderGraphFunction_float(float3 In, out float3 Out, out float3 Color){
	Out = In;
	Color = GetColor();
}

void ShaderGraphFunction_half(half3 In, out half3 Out, out half3 Color){
	Out = In;
	Color = GetColor();
}
