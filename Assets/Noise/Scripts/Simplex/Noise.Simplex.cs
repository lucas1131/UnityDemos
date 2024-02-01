using Unity.Mathematics;

using static Unity.Mathematics.math;

namespace Noise {

public static partial class Noise {

	public struct Simplex1D<G> : INoise where G : struct, IGradient {

		static float4 Kernel(SmallXXHash4 hash, float4 latticeX0, float4x3 positions){
			float4 x = positions.c0 - latticeX0;
			float4 falloff = 1f - x*x;
			falloff = falloff*falloff*falloff; // Cube this function so its C2-continuous
			return falloff * default(G).Evaluate(hash, x);
		}

		public float4 GetNoise4(float4x3 positions, SmallXXHash4 hash, int frequency) {
			var gradient = default(G);

			positions *= frequency;
			int4 latticeX0 = (int4) floor(positions.c0);
			int4 latticeX1 = latticeX0 + 1;
			return gradient.EvaluateCombined(
				Kernel(hash.Eat(latticeX0), latticeX0, positions) +
				Kernel(hash.Eat(latticeX1), latticeX1, positions)
			);
		}
	}

	public struct Simplex2D<G> : INoise where G : struct, IGradient {

		public float4 GetNoise4(float4x3 positions, SmallXXHash4 hash, int frequency) {
			return default(G).EvaluateCombined(0f);
		}
	}

	public struct Simplex3D<G> : INoise where G : struct, IGradient {

		public float4 GetNoise4(float4x3 positions, SmallXXHash4 hash, int frequency) {
			return default(G).EvaluateCombined(0f);
		}
	}
}}
