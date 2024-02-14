using Meshes.ProceduralMeshes.Streams;

using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace Meshes.ProceduralMeshes.Generators {

public struct FlatHexagonGrid : IMeshGenerator {

	const int VERTICES_PER_QUAD = 7;
	const int TRIANGLES_PER_QUAD = 6;

	public int Resolution { get; set; }
	public int ResolutionSqrd => Resolution*Resolution;

	public int VertexCount => VERTICES_PER_QUAD * ResolutionSqrd;
	public int IndexCount => 3*TRIANGLES_PER_QUAD * ResolutionSqrd;
	public int JobLength => Resolution;
	public Bounds Bounds => new Bounds(
		Vector3.zero,
		new Vector3(
			0.75f + 0.25f / Resolution,
			0f,
			(Resolution > 1 ? 0.5f + 0.25f / Resolution : 0.5f) * sqrt(3f)
		)
	);

	public void Execute<S>(int x, S stream) where S : struct, IMeshStream {
		int vIndex = Resolution*x*VERTICES_PER_QUAD;
		int tIndex = Resolution*x*TRIANGLES_PER_QUAD;

		float height = sqrt(3f) / 4f;
		float2 offset = 0f;
		if(Resolution > 1) {
			float xOffset = (x&1) == 0 ? 0.5f : 1.5f;
			offset.x = -0.375f * (Resolution-1);
			offset.y = (xOffset-Resolution)*height;
		}

		for(int z = 0; z < Resolution; z++){
			float2 center = (float2(0.75f*x, 2f*height*z) + offset)/Resolution;
			float4 xCoordinates = center.x + float4(-0.5f, -0.25f, 0.25f, 0.5f)/Resolution;
			float2 zCoordinates = center.y + float2(height, -height)/Resolution;

			Vertex template = new Vertex();
			template.normal.y = 1f;
			template.tangent.xw = float2(1f, -1f);

			template.position.xz = center;
			template.texCoord0 = 0.5f;
			stream.SetVertex(vIndex+0, template);

			template.position.x = xCoordinates.x;
			template.texCoord0.x = 0f;
			stream.SetVertex(vIndex+1, template);

			template.position.x = xCoordinates.y;
			template.position.z = zCoordinates.x;
			template.texCoord0 = float2(0.25f, 0.5f + height);
			stream.SetVertex(vIndex+2, template);

			template.position.x = xCoordinates.z;
			template.texCoord0.x = 0.75f;
			stream.SetVertex(vIndex+3, template);

			template.position.x = xCoordinates.w;
			template.position.z = center.y;
			template.texCoord0 = float2(1f, 0.5f);
			stream.SetVertex(vIndex+4, template);

			template.position.x = xCoordinates.z;
			template.position.z = zCoordinates.y;
			template.texCoord0 = float2(0.75f, 0.5f - height);
			stream.SetVertex(vIndex+5, template);

			template.position.x = xCoordinates.y;
			template.texCoord0.x = 0.25f;
			stream.SetVertex(vIndex+6, template);

			// Sum vIndex here to keep theses indices relative to current quad
			stream.SetTriangle(tIndex+0, vIndex + int3(0, 1, 2));
			stream.SetTriangle(tIndex+1, vIndex + int3(0, 2, 3));
			stream.SetTriangle(tIndex+2, vIndex + int3(0, 3, 4));
			stream.SetTriangle(tIndex+3, vIndex + int3(0, 4, 5));
			stream.SetTriangle(tIndex+4, vIndex + int3(0, 5, 6));
			stream.SetTriangle(tIndex+5, vIndex + int3(0, 6, 1));

			vIndex += VERTICES_PER_QUAD;
			tIndex += TRIANGLES_PER_QUAD;
		}
	}
}}
