using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Noise { 
public static class MathExtensions {
	// Manually vectorized matrix multiplication
	public static float4x3 TransformVectors(this float3x4 trs, float4x3 pos, float w=1f){
		return float4x3(
			trs.c0.x*pos.c0 + trs.c1.x*pos.c1 + trs.c2.x*pos.c2 + trs.c3.x*w,
			trs.c0.y*pos.c0 + trs.c1.y*pos.c1 + trs.c2.y*pos.c2 + trs.c3.y*w,
			trs.c0.z*pos.c0 + trs.c1.z*pos.c1 + trs.c2.z*pos.c2 + trs.c3.z*w
		);
	}

	public static float3x4 Get3x4(this float4x4 matrix){
		return float3x4(matrix.c0.xyz, matrix.c1.xyz, matrix.c2.xyz, matrix.c3.xyz);
	}
}}
