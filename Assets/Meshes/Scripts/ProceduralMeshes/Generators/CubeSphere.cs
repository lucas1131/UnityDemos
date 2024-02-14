using Meshes.ProceduralMeshes.Streams;

using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace Meshes.ProceduralMeshes.Generators {

public struct CubeSphere : IMeshGenerator {

	struct Side {
		public int id;
		public float3 uvOrigin;
		public float3 uVector;
		public float3 vVector;
	}

	const int VERTICES_PER_QUAD = 4;
	const int TRIANGLES_PER_QUAD = 2;
	const int CUBE_QUADS = 6;

	public int Resolution { get; set; }
	public int ResolutionSqrd => Resolution*Resolution;

	public int VertexCount => CUBE_QUADS * VERTICES_PER_QUAD * ResolutionSqrd;
	public int IndexCount => CUBE_QUADS * TRIANGLES_PER_QUAD * ResolutionSqrd * 3;
	public int JobLength => CUBE_QUADS * Resolution;
	public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(2f, 2f, 2f));

	public void Execute<S>(int idx, S stream) where S : struct, IMeshStream {

		int u = idx / CUBE_QUADS;
		Side side = GetSide(idx - 6 * u);
		int vIndex = Resolution*(Resolution*side.id + u)*VERTICES_PER_QUAD;
		int tIndex = Resolution*(Resolution*side.id + u)*TRIANGLES_PER_QUAD;

		// Displace side origin
		float3 uA = side.uvOrigin + side.uVector * u/Resolution;
		float3 uB = side.uvOrigin + side.uVector * (u+1)/Resolution;
		float3 pA = CubeToSphere(uA);
		float3 pB = CubeToSphere(uB);

		Vertex template = new Vertex();
		template.tangent = float4(normalize(pB - pA), -1f);

		for(int v = 1; v <= Resolution; v++){
			// Side 4 corners
			float3 pC = CubeToSphere(uA + side.vVector * v/Resolution);
			float3 pD = CubeToSphere(uB + side.vVector * v/Resolution);

			template.position = pA;
			template.normal = normalize(cross(pC - pA, template.tangent.xyz));
			stream.SetVertex(vIndex+0, template);

			template.position = pB;
			template.normal = normalize(cross(pD - pB, template.tangent.xyz));
			template.texCoord0 = float2(1f, 0f);
			stream.SetVertex(vIndex+1, template);

			template.tangent.xyz = normalize(pD - pC);
			template.position = pC;
			template.normal = normalize(cross(pC - pA, template.tangent.xyz));
			template.texCoord0 = float2(0f, 1f);
			stream.SetVertex(vIndex+2, template);

			template.position = pD;
			template.normal = normalize(cross(pD - pB, template.tangent.xyz));
			template.texCoord0 = float2(1f, 1f);
			stream.SetVertex(vIndex+3, template);

			// Sum vIndex here to keep theses indices relative to current quad
			stream.SetTriangle(tIndex+0, vIndex + int3(0, 2, 1));
			stream.SetTriangle(tIndex+1, vIndex + int3(1, 2, 3));

			vIndex += VERTICES_PER_QUAD;
			tIndex += TRIANGLES_PER_QUAD;

			// Reuse these two points for the next side
			pA = pC;
			pB = pD;
		}
	}

	static float3 CubeToSphere(float3 p) => p * sqrt(1f - ((p*p).yxx + (p*p).zzy)/2f + (p*p).yxx * (p*p).zzy/3f);

	static Side GetSide(int id) => id switch {
		0 => new Side { // Front side
			id = id,
			uvOrigin = -1f,
			uVector = 2f * right(),
			vVector = 2f * up(),
		},
		1 => new Side { // Right side
			id = id,
			uvOrigin = float3(1f, -1f, -1f),
			uVector = 2f * forward(),
			vVector = 2f * up(),
		},
		2 => new Side { // Down side
			id = id,
			uvOrigin = -1f,
			uVector = 2f * forward(),
			vVector = 2f * right(),
		},
		3 => new Side { // Back side
			id = id,
			uvOrigin = float3(-1f, -1f, 1f),
			uVector = 2f * up(),
			vVector = 2f * right(),
		},
		4 => new Side { // Left side
			id = id,
			uvOrigin = -1f,
			uVector = 2f * up(),
			vVector = 2f * forward(),
		},
		5 => new Side { // Up side
			id = id,
			uvOrigin = float3(-1f, 1f, -1f),
			uVector = 2f * right(),
			vVector = 2f * forward(),
		},
		_ => new Side { // Shouldn't happen but defaults to Front side
			id = id,
			uvOrigin = -1f,
			uVector = 2f * right(),
			vVector = 2f * up(),
		}
	};
}}
