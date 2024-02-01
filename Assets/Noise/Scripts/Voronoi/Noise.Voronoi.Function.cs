using Unity.Mathematics;

using static Unity.Mathematics.math;

namespace Noise {

public static partial class Noise {

	public interface IVoronoiFunction {
		float4 Evaluate(float4x3 distances);
	}

	public struct F1 : IVoronoiFunction {
		public float4 Evaluate(float4x3 distances) => distances.c0;
	}

	public struct F2 : IVoronoiFunction {
		public float4 Evaluate(float4x3 distances) => distances.c1;
	}

	public struct Cell : IVoronoiFunction {
		public float4 Evaluate(float4x3 distances) => distances.c2;
	}

	public struct F2MinusF1 : IVoronoiFunction {
		public float4 Evaluate(float4x3 distances) => distances.c1 - distances.c0;
	}
}}
