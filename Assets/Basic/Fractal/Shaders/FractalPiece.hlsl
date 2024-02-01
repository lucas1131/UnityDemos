#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
 StructuredBuffer<float3x4> _Matrices;
#endif

float _PointScale;
float4 _FractalColor1, _FractalColor2;
float4 _SequenceValues;

// Not working properly for URP dunno why
void procedural_instancing () {
	#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
	 float scale = _PointScale;
	 float3x4 m = _Matrices[unity_InstanceID];
	 unity_ObjectToWorld._m00_m01_m02_m03 = m._m00_m01_m02_m03;
	 unity_ObjectToWorld._m10_m11_m12_m13 = m._m10_m11_m12_m13;
	 unity_ObjectToWorld._m20_m21_m22_m23 = m._m20_m21_m22_m23;
	 unity_ObjectToWorld._m30_m31_m32_m33 = float4(0.0, 0.0, 0.0, 1.0);
	#endif 
}

float4 GetFractalColor(){
	#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
	 float4 color;
	 color.rgb = lerp(_FractalColor1.rgb, _FractalColor2.rgb, frac(unity_InstanceID * _SequenceValues.x + _SequenceValues.y));
	 color.a   = lerp(_FractalColor1.a  , _FractalColor2.a,   frac(unity_InstanceID * _SequenceValues.z + _SequenceValues.w));
	 return color;
	#else
	 return _FractalColor1;
	#endif
}

void ShaderGraphFunction_float(float3 In, out float3 Out, out float4 FractalColor){
	Out = In;
	FractalColor = GetFractalColor();
}

void ShaderGraphFunction_half(half3 In, out half3 Out, out half4 FractalColor){
	Out = In;
	FractalColor = GetFractalColor();
}
