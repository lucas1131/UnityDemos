using Unity.Mathematics;

using static Unity.Mathematics.math;

namespace Noise {

public static partial class Noise {

	public interface ILattice {
		LatticeSpan4 GetLatticeSpan4(float4 coordinates, int frequency);
		int4 CorrectPointOffsetStep(int4 points, int frequency);
	}

	public struct LatticeSpan4 {
		public int4 p0;
		public int4 p1;
		public float4 g0;
		public float4 g1;
		public float4 t;
	}

	public struct LatticeNormal : ILattice {
		public LatticeSpan4 GetLatticeSpan4(float4 coordinates, int frequency){
			coordinates *= frequency;
			float4 points = floor(coordinates);
			LatticeSpan4 span;
			span.p0 = (int4) points;
			span.p1 = span.p0 + 1;
			span.g0 = coordinates - span.p0;
			span.g1 = span.g0 - 1f;
			span.t = coordinates - points; // floating part of span.p0
			span.t = span.t*span.t*span.t * (span.t * (span.t * 6f - 15f) + 10f); 
			return span;
		}
		
		public int4 CorrectPointOffsetStep(int4 points, int frequency) => points;
	}
	
	public struct LatticeTilling : ILattice {
		public LatticeSpan4 GetLatticeSpan4(float4 coordinates, int frequency){
			coordinates *= frequency;
			float4 points = floor(coordinates);
			LatticeSpan4 span;
			span.p0 = (int4) points;
			span.g0 = coordinates - span.p0;
			span.g1 = span.g0 - 1f;

			span.p0 -= (int4) ceil(points/frequency) * frequency;
			span.p0 = select(span.p0, span.p0+frequency, span.p0 < 0);
			span.p1 = span.p0 + 1;
			span.p1 = select(span.p1, 0, span.p1 == frequency);

			span.t = coordinates - points; // floating part of span.p0
			span.t = span.t*span.t*span.t * (span.t * (span.t * 6f - 15f) + 10f); 
			return span;
		}
		
		public int4 CorrectPointOffsetStep(int4 points, int frequency){
			points = select(points, 0, points == frequency);
			points = select(points, frequency-1, points == -1);
			return points;
		}
	}

	public struct Lattice1D<L, G> : INoise 
		where L : struct, ILattice 
		where G : struct, IGradient { 

		public float4 GetNoise4(float4x3 points, SmallXXHash4 hash, int frequency){
			var gradient = default(G);
			var lattice = default(L);

			LatticeSpan4 x = lattice.GetLatticeSpan4(points.c0, frequency);
			float4 noise1d = lerp(gradient.Evaluate(hash.Eat(x.p0), x.g0), gradient.Evaluate(hash.Eat(x.p1), x.g1), x.t);
			return gradient.EvaluateAfterInterpolation(noise1d);
		}
	}

	public struct Lattice2D<L, G> : INoise 
		where L : struct, ILattice 
		where G : struct, IGradient { 

		public float4 GetNoise4(float4x3 points, SmallXXHash4 hash, int frequency){
			var gradient = default(G);
			var lattice = default(L);

			LatticeSpan4 x = lattice.GetLatticeSpan4(points.c0, frequency);
			LatticeSpan4 z = lattice.GetLatticeSpan4(points.c2, frequency);

			SmallXXHash4 h0 = hash.Eat(x.p0);
			SmallXXHash4 h1 = hash.Eat(x.p1);

			float4 noise1d0 = lerp(
				gradient.Evaluate(h0.Eat(z.p0), x.g0, z.g0), 
				gradient.Evaluate(h0.Eat(z.p1), x.g0, z.g1), 
				z.t);
			float4 noise1d1 = lerp(
				gradient.Evaluate(h1.Eat(z.p0), x.g1, z.g0), 
				gradient.Evaluate(h1.Eat(z.p1), x.g1, z.g1), 
				z.t);

			float4 noise2d = lerp(noise1d0, noise1d1, x.t);
			return gradient.EvaluateAfterInterpolation(noise2d);
		}
	}

	public struct Lattice3D<L, G> : INoise 
		where L : struct, ILattice 
		where G : struct, IGradient { 

		public float4 GetNoise4(float4x3 points, SmallXXHash4 hash, int frequency){
			var gradient = default(G);
			var lattice = default(L);

			LatticeSpan4 x = lattice.GetLatticeSpan4(points.c0, frequency);
			LatticeSpan4 y = lattice.GetLatticeSpan4(points.c1, frequency);
			LatticeSpan4 z = lattice.GetLatticeSpan4(points.c2, frequency);

			SmallXXHash4 h0 = hash.Eat(x.p0);
			SmallXXHash4 h1 = hash.Eat(x.p1);
			SmallXXHash4 h00 = h0.Eat(y.p0); 
			SmallXXHash4 h01 = h0.Eat(y.p1);
			SmallXXHash4 h10 = h1.Eat(y.p0); 
			SmallXXHash4 h11 = h1.Eat(y.p1);

			float4 noise2d0 = lerp(
				lerp(
					gradient.Evaluate(h00.Eat(z.p0), x.g0, y.g0, z.g0), 
					gradient.Evaluate(h00.Eat(z.p1), x.g0, y.g0, z.g1), 
					z.t), // noise1dXX
				lerp(
					gradient.Evaluate(h01.Eat(z.p0), x.g0, y.g1, z.g0), 
					gradient.Evaluate(h01.Eat(z.p1), x.g0, y.g1, z.g1), 
					z.t), // noise1dXX
				y.t
			);

			float4 noise2d1 = lerp(
				lerp(
					gradient.Evaluate(h10.Eat(z.p0), x.g1, y.g0, z.g0), 
					gradient.Evaluate(h10.Eat(z.p1), x.g1, y.g0, z.g1), 
					z.t), // noise1dXX
				lerp(
					gradient.Evaluate(h11.Eat(z.p0), x.g1, y.g1, z.g0), 
					gradient.Evaluate(h11.Eat(z.p1), x.g1, y.g1, z.g1), 
					z.t), // noise1dXX
				y.t
			);

			float4 noise3d = lerp(noise2d0, noise2d1, x.t);
			return gradient.EvaluateAfterInterpolation(noise3d);
		}
	}
}}
