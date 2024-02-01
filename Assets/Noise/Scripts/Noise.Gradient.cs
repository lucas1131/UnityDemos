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

	public struct Value : IGradient {
		public float4 Evaluate(SmallXXHash4 hash, float4 x) => hash.Float01A()*2f - 1f;
		public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y) => hash.Float01A()*2f - 1f;
		public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y, float4 z) => hash.Float01A()*2f - 1f;
		public float4 EvaluateCombined(float4 value) => value;
	}

	public struct Perlin : IGradient {
		public float4 Evaluate(SmallXXHash4 hash, float4 x) {
			float4 binarySelector = select(-x, x, ((uint4) hash & 1 << 8) == 0);
			return 2f * binarySelector * (hash.Float01A() + 1f);
		}

		public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y) {
			float4 gx = hash.Float01A()*2f - 1f;
			float4 gy = 0.5f - abs(gx);
			gx -= floor(gx + 0.5f);
			return (x*gx + y*gy) * (2f / 0.53528f); // Square gradients maximum
		}

		public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y, float4 z) {
			float4 gx = hash.Float01A()*2f - 1f;
			float4 gy = hash.Float01D()*2f - 1f;
			float4 gz = 0.5f - abs(gx) - abs(gy);
			float4 offset = max(-gz, 0f);

			gx += select(-offset, offset, gx < 0f);
			gy += select(-offset, offset, gy < 0f);

			return (x*gx + y*gy) * (2f / 0.56290f); // Octahedron gradients maximum
		}
	
		public float4 EvaluateCombined(float4 value) => value;
	}

	public struct Turbulence<G> : IGradient where G : struct, IGradient {
		public float4 Evaluate (SmallXXHash4 hash, float4 x) => default(G).Evaluate(hash, x);
		public float4 Evaluate (SmallXXHash4 hash, float4 x, float4 y) => default(G).Evaluate(hash, x, y);
		public float4 Evaluate (SmallXXHash4 hash, float4 x, float4 y, float4 z) => default(G).Evaluate(hash, x, y, z);
		public float4 EvaluateCombined (float4 value) => abs(default(G).EvaluateCombined(value));
	}
}}
