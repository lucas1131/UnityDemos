using Unity.Mathematics;

using static Unity.Mathematics.math;

namespace Noise {

public static partial class Noise {

	public interface IGradient {
		float4 Evaluate(SmallXXHash4 hash, float4 x);
		float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y);
		float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y, float4 z);
		float4 EvaluateCombined(float4 value);
	}

	public static class BaseGradients {
		public static float4 Line(SmallXXHash4 hash, float4 x){
			float4 signSelector = select(-x, x, ((uint4) hash & 1 << 8) == 0);
			return signSelector * (hash.Float01A() + 1f);
		}

		static float4x2 SquareVectors(SmallXXHash4 hash){
			float4x2 gradient;
			gradient.c0 = hash.Float01A()*2f - 1f;
			gradient.c1 = 0.5f - abs(gradient.c0);
			gradient.c0 -= floor(gradient.c0 + 0.5f);
			return gradient;
		}

		static float4x3 OctahedronVectors(SmallXXHash4 hash){
			float4x3 gradient;
			gradient.c0 = hash.Float01A()*2f - 1f;
			gradient.c1 = hash.Float01D()*2f - 1f;
			gradient.c2 = 0.5f - abs(gradient.c0) - abs(gradient.c1);
			float4 offset = max(-gradient.c2, 0f);

			gradient.c0 += select(-offset, offset, gradient.c0 < 0f);
			gradient.c1 += select(-offset, offset, gradient.c1 < 0f);

			return gradient;
		}

		public static float4 Square(SmallXXHash4 hash, float4 x, float4 y){
			float4x2 g = SquareVectors(hash);
			return (x*g.c0 + y*g.c1);
		}

		public static float4 Circle(SmallXXHash4 hash, float4 x, float4 y){
			float4x2 g = SquareVectors(hash);
			float4 radius = rsqrt(g.c0*g.c0 + g.c1*g.c1);
			return (x*g.c0 + y*g.c1) * radius;
		}

		public static float4 Octahedron(SmallXXHash4 hash, float4 x, float4 y, float4 z){
			float4x3 g = OctahedronVectors(hash);
			return (x*g.c0 + y*g.c1 + z*g.c2);
		}

		public static float4 Sphere(SmallXXHash4 hash, float4 x, float4 y, float4 z){
			float4x3 g = OctahedronVectors(hash);
			float4 radius = rsqrt(g.c0*g.c0 + g.c1*g.c1 + g.c2*g.c2);
			return (x*g.c0 + y*g.c1 + z*g.c2) * radius;
		}
	}

	public struct Value : IGradient {
		public float4 Evaluate(SmallXXHash4 hash, float4 x) => hash.Float01A()*2f - 1f;
		public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y) => hash.Float01A()*2f - 1f;
		public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y, float4 z) => hash.Float01A()*2f - 1f;
		public float4 EvaluateCombined(float4 value) => value;
	}

	public struct Perlin : IGradient {
		public float4 Evaluate(SmallXXHash4 hash, float4 x) => BaseGradients.Line(hash, x) * 2f;
		public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y) =>
			BaseGradients.Square(hash, x, y) * (2f / 0.53528f); // Square gradient maximum
		public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y, float4 z) =>
			BaseGradients.Octahedron(hash, x, y, z) * (2f / 0.56290f); // Octahedron gradient maximum
	
		public float4 EvaluateCombined(float4 value) => value;
	}

	public struct Turbulence<G> : IGradient where G : struct, IGradient {
		public float4 Evaluate (SmallXXHash4 hash, float4 x) => default(G).Evaluate(hash, x);
		public float4 Evaluate (SmallXXHash4 hash, float4 x, float4 y) => default(G).Evaluate(hash, x, y);
		public float4 Evaluate (SmallXXHash4 hash, float4 x, float4 y, float4 z) => default(G).Evaluate(hash, x, y, z);
		public float4 EvaluateCombined (float4 value) => abs(default(G).EvaluateCombined(value));
	}

	public struct Simplex : IGradient {
		public float4 Evaluate(SmallXXHash4 hash, float4 x) =>
			BaseGradients.Line(hash, x) * (32f/27f);
		public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y) =>
			BaseGradients.Circle(hash, x, y) * (5.832f/sqrt(2f));
		public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y, float4 z) =>
			BaseGradients.Sphere(hash, x, y, z) * (1024f/(125f*sqrt(3f)));

		public float4 EvaluateCombined(float4 value) => value;
	}
}}
