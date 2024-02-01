#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
 StructuredBuffer<float> _Noise;
 StructuredBuffer<float3> _Positions;
 StructuredBuffer<float3> _Normals;
#endif

float4 _Config;

void procedural_instancing () {
	#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
	 float scale = _Config.y;
	 float3 displacement = _Config.z * _Noise[unity_InstanceID] * _Normals[unity_InstanceID];

	 unity_ObjectToWorld = 0.0;
	 unity_ObjectToWorld._m03_m13_m23_m33 = float4(_Positions[unity_InstanceID], 1.0);
	 unity_ObjectToWorld._m03_m13_m23 += displacement;
	 unity_ObjectToWorld._m00_m11_m22 = scale;
	#endif 
}

float3 GetColor(){
	#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
	 float noise = _Noise[unity_InstanceID];
	 return noise < 0.0 ? float3(-noise, 0.0, 0.0) : noise;
	 // return float3(noise, noise/2.0, noise/3.0);
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
