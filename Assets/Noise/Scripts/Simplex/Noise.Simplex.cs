using Unity.Mathematics;

using static Unity.Mathematics.math;

namespace Noise {

public static partial class Noise {

	public struct Simplex1D<G> : INoise where G : struct, IGradient {

		static float4 Kernel(SmallXXHash4 hash, float4 latticeX, float4x3 positions){
			float4 x = positions.c0 - latticeX;
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

		static float4 Kernel(SmallXXHash4 hash, float4 latticeX, float4 latticeZ, float4x3 positions){
			float square2TriangleSkew = 0.211324f; // (3f - sqrt(3f))/6f;

			float4 unskew = (latticeX + latticeZ) * square2TriangleSkew;
			float4 x = positions.c0 - latticeX + unskew;
			float4 z = positions.c2 - latticeZ + unskew;


			float4 falloff = 0.5f - x*x - z*z;
			falloff = falloff*falloff*falloff*8f; // Cube this function so its C2-continuous
			return max(0f, falloff) * default(G).Evaluate(hash, x, z);
		}

		public float4 GetNoise4(float4x3 positions, SmallXXHash4 hash, int frequency) {
			var gradient = default(G);

			positions *= frequency*(1f/sqrt(3f));
			float triangle2SquareSkew = 0.366025f; // (sqrt(3f) - 1f)/2f;
			float4 skew = (positions.c0 + positions.c2) * triangle2SquareSkew;
			float4 skewX = positions.c0 + skew;
			float4 skewZ = positions.c2 + skew;

			int4 latticeX0 = (int4) floor(skewX);
			int4 latticeZ0 = (int4) floor(skewZ);
			int4 latticeX1 = latticeX0 + 1;
			int4 latticeZ1 = latticeZ0 + 1;
			
			bool4 isAboveXZDiagonal = skewX - latticeX0 > skewZ - latticeZ0;
			int4 selectedX = select(latticeX0, latticeX1, isAboveXZDiagonal);
			int4 selectedZ = select(latticeZ1, latticeZ0, isAboveXZDiagonal);

			SmallXXHash4 hx0 = hash.Eat(latticeX0);
			SmallXXHash4 hx1 = hash.Eat(latticeX1);
			SmallXXHash4 selectedH = SmallXXHash4.Select(hx0, hx1, isAboveXZDiagonal);


			return gradient.EvaluateCombined(
				Kernel(hx0.Eat(latticeZ0), latticeX0, latticeZ0, positions) +
				Kernel(hx1.Eat(latticeZ1), latticeX1, latticeZ1, positions) +
				Kernel(selectedH.Eat(selectedZ), selectedX, selectedZ, positions)
			);
		}
	}

	public struct Simplex3D<G> : INoise where G : struct, IGradient {

		public float4 GetNoise4(float4x3 positions, SmallXXHash4 hash, int frequency) {
			return default(G).EvaluateCombined(0f);
		}
	}
}}
