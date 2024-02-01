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

			positions *= frequency*0.577350f; // (1f/sqrt(3f));
			float triangle2SquareSkew = 0.366025f; // (sqrt(3f) - 1f)/2f;
			float4 skew = (positions.c0 + positions.c2) * triangle2SquareSkew;
			float4 skewX = positions.c0 + skew;
			float4 skewZ = positions.c2 + skew;

			int4 latticeX0 = (int4) floor(skewX);
			int4 latticeZ0 = (int4) floor(skewZ);
			int4 latticeX1 = latticeX0 + 1;
			int4 latticeZ1 = latticeZ0 + 1;
			
			bool4 isXGreaterThanZ = skewX - latticeX0 > skewZ - latticeZ0;
			int4 selectedX = select(latticeX0, latticeX1, isXGreaterThanZ);
			int4 selectedZ = select(latticeZ1, latticeZ0, isXGreaterThanZ);

			SmallXXHash4 hx0 = hash.Eat(latticeX0);
			SmallXXHash4 hx1 = hash.Eat(latticeX1);
			SmallXXHash4 selectedH = SmallXXHash4.Select(hx0, hx1, isXGreaterThanZ);


			return gradient.EvaluateCombined(
				Kernel(hx0.Eat(latticeZ0), latticeX0, latticeZ0, positions) +
				Kernel(hx1.Eat(latticeZ1), latticeX1, latticeZ1, positions) +
				Kernel(selectedH.Eat(selectedZ), selectedX, selectedZ, positions)
			);
		}
	}

	public struct Simplex3D<G> : INoise where G : struct, IGradient {

		static float4 Kernel(SmallXXHash4 hash, float4 latticeX, float4 latticeY, float4 latticeZ, float4x3 positions){
			float square2TriangleSkew = 1f/6f;

			float4 unskew = (latticeX + latticeY + latticeZ) * square2TriangleSkew;
			float4 x = positions.c0 - latticeX + unskew;
			float4 y = positions.c1 - latticeY + unskew;
			float4 z = positions.c2 - latticeZ + unskew;

			float4 falloff = 0.5f - x*x - y*y - z*z;
			falloff = falloff*falloff*falloff*8f; // Cube this function so its C2-continuous
			return max(0f, falloff) * default(G).Evaluate(hash, x, y, z);
		}

		public float4 GetNoise4(float4x3 positions, SmallXXHash4 hash, int frequency) {
			var gradient = default(G);

			positions *= frequency*0.6f;
			float triangle2SquareSkew = 1f/3f;
			float4 skew = (positions.c0 + positions.c1 + positions.c2) * triangle2SquareSkew;
			float4 skewX = positions.c0 + skew;
			float4 skewY = positions.c1 + skew;
			float4 skewZ = positions.c2 + skew;

			int4 latticeX0 = (int4) floor(skewX);
			int4 latticeY0 = (int4) floor(skewY);
			int4 latticeZ0 = (int4) floor(skewZ);
			int4 latticeX1 = latticeX0 + 1;
			int4 latticeY1 = latticeY0 + 1;
			int4 latticeZ1 = latticeZ0 + 1;
			
			/* Lattice point selection truth table

			|------------------------|
			|X>Y|X>Z|Y>Z|| X | Y | Z |
			|------------------------|
			| T | T | T ||1 1|0 1|0 0|
			| T | T | F ||1 1|0 0|0 1|
			| T | F | T ||0 1|0 0|1 1|
			| T | F | F ||0 1|0 0|1 1|
			| F | T | T ||0 1|1 1|0 0|
			| F | T | F ||0 0|0 1|1 1|
			| F | F | T ||0 0|1 1|0 1|
			| F | F | F ||0 0|0 1|1 1|
			|------------------------|
			*/
			bool4 isXGreaterThanY = skewX - latticeX0 > skewY - latticeY0;
			bool4 isXGreaterThanZ = skewX - latticeX0 > skewZ - latticeZ0;
			bool4 isYGreaterThanZ = skewY - latticeY0 > skewZ - latticeZ0;

			bool4 x0 =   isXGreaterThanY &  isXGreaterThanZ;
			bool4 x1 =   isXGreaterThanY | (isXGreaterThanZ  &  isYGreaterThanZ);
			bool4 y0 =  !isXGreaterThanY &  isYGreaterThanZ;
			bool4 y1 =  !isXGreaterThanY | (isXGreaterThanZ  &  isYGreaterThanZ);
			bool4 z0 =  (isXGreaterThanY & !isXGreaterThanZ) | (!isXGreaterThanY & !isYGreaterThanZ);
			bool4 z1 = !(isXGreaterThanZ &  isYGreaterThanZ);

			int4 selectedX0 = select(latticeX0, latticeX1, x0);
			int4 selectedX1 = select(latticeX0, latticeX1, x1);
			int4 selectedY0 = select(latticeY0, latticeY1, y0);
			int4 selectedY1 = select(latticeY0, latticeY1, y1);
			int4 selectedZ0 = select(latticeZ0, latticeZ1, z0);
			int4 selectedZ1 = select(latticeZ0, latticeZ1, z1);


			SmallXXHash4 hx0 = hash.Eat(latticeX0);
			SmallXXHash4 hx1 = hash.Eat(latticeX1);
			SmallXXHash4 selectedH0 = SmallXXHash4.Select(hx0, hx1, x0);
			SmallXXHash4 selectedH1 = SmallXXHash4.Select(hx0, hx1, x1);


			return gradient.EvaluateCombined(
				Kernel(hx0.Eat(latticeY0).Eat(latticeZ0), latticeX0, latticeY0, latticeZ0, positions) +
				Kernel(hx1.Eat(latticeY1).Eat(latticeZ1), latticeX1, latticeY1, latticeZ1, positions) +
				Kernel(selectedH0.Eat(selectedY0).Eat(selectedZ0), selectedX0, selectedY0, selectedZ0, positions) +
				Kernel(selectedH1.Eat(selectedY1).Eat(selectedZ1), selectedX1, selectedY1, selectedZ1, positions)
			);
		}
	}
}}
