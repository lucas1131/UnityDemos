using Unity.Mathematics;

using static Unity.Mathematics.math;

namespace Noise {

public static partial class Noise {

	static float4x3 UpdateVoronoiMinima(float4x3 minima, float4 distances, float4 cell) {
		// New second global minima, update just second minima
		float4 secondaryMinima = select(minima.c1, distances, distances < minima.c1); 
		
		// New global minima, move old minima to second-minima and update
		bool4 isMinimum = distances < minima.c0;

		// Finally update values
		minima.c1 = select(secondaryMinima, minima.c0, isMinimum);
		minima.c0 = select(minima.c0, distances, isMinimum);
		minima.c2 = select(minima.c2, cell, isMinimum);

		return minima;
	}

	static float4 GetDistance(float4 x, float4 y) => sqrt(x*x + y*y);
	static float4 GetDistance(float4 x, float4 y, float4 z) => sqrt(x*x + y*y + z*z);

	public struct Voronoi1D<L, D, F> : INoise 
		where L : struct, ILattice
		where D : struct, IVoronoiDistance
		where F : struct, IVoronoiFunction { 

		public float4 GetNoise4(float4x3 points, SmallXXHash4 hash, int frequency){
			var lattice = default(L);
			var distanceFunc = default(D);
			var function = default(F);

			LatticeSpan4 x = lattice.GetLatticeSpan4(points.c0, frequency);

			float4x3 minima = 100f;
			for (int u = -1; u <= 1; u++) {
				int4 correctedX = lattice.CorrectPointOffsetStep(x.p0+u, frequency);
				SmallXXHash4 hx = hash.Eat(correctedX);
				float4 dist = distanceFunc.GetDistance(hx.Float01A()+u - x.g0);
				float4 cell = hx.Float01D();
				minima = UpdateVoronoiMinima(minima, dist, cell);
			}

			float4 noise1d = function.Evaluate(distanceFunc.Finalize1D(minima));
			return noise1d;
		}
	}

	public struct Voronoi2D<L, D, F> : INoise 
		where L : struct, ILattice
		where D : struct, IVoronoiDistance
		where F : struct, IVoronoiFunction { 

		public float4 GetNoise4(float4x3 points, SmallXXHash4 hash, int frequency){
			var lattice = default(L);
			var distanceFunc = default(D);
			var function = default(F);

			LatticeSpan4 x = lattice.GetLatticeSpan4(points.c0, frequency);
			LatticeSpan4 z = lattice.GetLatticeSpan4(points.c2, frequency);

			float4x3 minima = 100f;
			for (int u = -1; u <= 1; u++) {
				int4 correctedX = lattice.CorrectPointOffsetStep(x.p0+u, frequency);
				SmallXXHash4 hx = hash.Eat(correctedX);
				float4 xOffset = u - x.g0;

				for (int v = -1; v <= 1; v++) {
					int4 correctedZ = lattice.CorrectPointOffsetStep(z.p0+v, frequency);
					SmallXXHash4 hxz = hx.Eat(correctedZ);
					float4 zOffset = v - z.g0;

					float4 cell = hxz.GetBitsAsFloat01(24, 6);
					float4 dist = distanceFunc.GetDistance(
						hxz.GetBitsAsFloat01(0, 6)+xOffset, 
						hxz.GetBitsAsFloat01(6, 6)+zOffset
					);
					
					minima = UpdateVoronoiMinima(minima, dist, cell);
					dist = distanceFunc.GetDistance(
						hxz.GetBitsAsFloat01(12, 6)+xOffset, 
						hxz.GetBitsAsFloat01(18, 6)+zOffset
					);
					
					minima = UpdateVoronoiMinima(minima, dist, cell);
				}
			}

		float4 noise2d = function.Evaluate(distanceFunc.Finalize2D(minima));
			return noise2d;
		}
	}

	public struct Voronoi3D<L, D, F> : INoise 
		where L : struct, ILattice
		where D : struct, IVoronoiDistance
		where F : struct, IVoronoiFunction { 

		public float4 GetNoise4(float4x3 points, SmallXXHash4 hash, int frequency){
			var lattice = default(L);
			var distanceFunc = default(D);
			var function = default(F);

			LatticeSpan4 x = lattice.GetLatticeSpan4(points.c0, frequency);
			LatticeSpan4 y = lattice.GetLatticeSpan4(points.c1, frequency);
			LatticeSpan4 z = lattice.GetLatticeSpan4(points.c2, frequency);

			float4x3 minima = 100f;
			for (int u = -1; u <= 1; u++) {
				int4 correctedX = lattice.CorrectPointOffsetStep(x.p0+u, frequency);
				SmallXXHash4 hx = hash.Eat(correctedX);
				float4 xOffset = u - x.g0;

				for (int v = -1; v <= 1; v++) {
					int4 correctedY = lattice.CorrectPointOffsetStep(y.p0+v, frequency);
					SmallXXHash4 hxy = hx.Eat(correctedY);
					float4 yOffset = v - y.g0;

					for (int w = -1; w <= 1; w++) {
						int4 correctedZ = lattice.CorrectPointOffsetStep(z.p0+w, frequency);
						SmallXXHash4 hxyz = hxy.Eat(correctedZ);
						float4 zOffset = w - z.g0;

						float4 cell = hxyz.GetBitsAsFloat01(24, 4);
						float4 dist = distanceFunc.GetDistance(
							hxyz.GetBitsAsFloat01(0, 4) + xOffset, 
							hxyz.GetBitsAsFloat01(4, 4) + yOffset, 
							hxyz.GetBitsAsFloat01(8, 4) + zOffset
						);
						minima = UpdateVoronoiMinima(minima, dist, cell);

						dist = distanceFunc.GetDistance(
							hxyz.GetBitsAsFloat01(12, 4) + xOffset, 
							hxyz.GetBitsAsFloat01(16, 4) + yOffset, 
							hxyz.GetBitsAsFloat01(20, 4) + zOffset
						);
						
						minima = UpdateVoronoiMinima(minima, dist, cell);
					}
				}
			}

			float4 noise3d = function.Evaluate(distanceFunc.Finalize3D(minima));
			return noise3d;
		}
	}
}}
