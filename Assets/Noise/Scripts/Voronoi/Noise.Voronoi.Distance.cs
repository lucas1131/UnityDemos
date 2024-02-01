using Unity.Mathematics;

using static Unity.Mathematics.math;

namespace Noise {

public static partial class Noise {
	public interface IVoronoiDistance {
		float4 GetDistance(float4 x);
		float4 GetDistance(float4 x, float4 y);
		float4 GetDistance(float4 x, float4 y ,float4 z);
		
		float4x3 Finalize1D(float4x3 minima);
		float4x3 Finalize2D(float4x3 minima);
		float4x3 Finalize3D(float4x3 minima);
	}

	public struct Worley : IVoronoiDistance {
		public float4 GetDistance(float4 x) {
			return abs(x);
		}

		public float4 GetDistance(float4 x, float4 y) {
			return x*x + y*y;
		}

		public float4 GetDistance(float4 x, float4 y ,float4 z) {
			return x*x + y*y + z*z;
		}

		public float4x3 Finalize1D(float4x3 minima) => minima;
		public float4x3 Finalize2D(float4x3 minima) {
			minima.c0 = sqrt(min(minima.c0, 1f));
			minima.c1 = sqrt(min(minima.c1, 1f));
			return minima;
		}
		public float4x3 Finalize3D(float4x3 minima) => Finalize2D(minima);
	}

	public struct Chebychev : IVoronoiDistance {
		public float4 GetDistance(float4 x) {
			return abs(x);
		}

		public float4 GetDistance(float4 x, float4 y) {
			return max(abs(x), abs(y));
		}

		public float4 GetDistance(float4 x, float4 y ,float4 z) {
			return max( max(abs(x), abs(y)),  abs(z));
		}

		public float4x3 Finalize1D(float4x3 minima) => minima;
		public float4x3 Finalize2D(float4x3 minima) => minima;
		public float4x3 Finalize3D(float4x3 minima) => minima;
	}

	public struct Manhattan : IVoronoiDistance {
		public float4 GetDistance(float4 x) {
			return abs(x);
		}

		public float4 GetDistance(float4 x, float4 y) {
			return abs(x) + abs(y);
		}

		public float4 GetDistance(float4 x, float4 y ,float4 z) {
			return abs(x) + abs(y) + abs(z);
		}

		public float4x3 Finalize1D(float4x3 minima) => minima;
		public float4x3 Finalize2D(float4x3 minima) => minima/2f;
		public float4x3 Finalize3D(float4x3 minima) => minima/3f;
	}
}}