using Meshes.ProceduralMeshes.Streams;

using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace Meshes.ProceduralMeshes.Generators {

public struct PointyHexagonGrid : IMeshGenerator {

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
			(Resolution > 1 ? 0.5f + 0.25f / Resolution : 0.5f) * sqrt(3f),
			0f,
			0.75f + 0.25f / Resolution
		)
	);

	public void Execute<S>(int z, S stream) where S : struct, IMeshStream {
		int vIndex = Resolution*z*VERTICES_PER_QUAD;
		int tIndex = Resolution*z*TRIANGLES_PER_QUAD;

		float height = sqrt(3f) / 4f;
		float2 offset = 0f;
		if(Resolution > 1) {
			float xOffset = (z&1) == 0 ? 0.5f : 1.5f;
			offset.x = (xOffset-Resolution)*height;
			offset.y = -0.375f * (Resolution-1);
		}

		for(int x = 0; x < Resolution; x++){
			float2 center = (float2(2f*height*x, 0.75f*z) + offset)/Resolution;
			float2 xCoordinates = center.x + float2(-height, height)/Resolution;
			float4 zCoordinates = center.y + float4(-0.5f, -0.25f, 0.25f, 0.5f)/Resolution;

			Vertex template = new Vertex();
			template.normal.y = 1f;
			template.tangent.xw = float2(1f, -1f);

			template.position.xz = center;
			template.texCoord0 = 0.5f;
			stream.SetVertex(vIndex+0, template);

			template.position.z = zCoordinates.x;
			template.texCoord0.y = 0f;
			stream.SetVertex(vIndex+1, template);

			template.position.x = xCoordinates.x;
			template.position.z = zCoordinates.y;
			template.texCoord0 = float2(0.5f - height, 0.25f);
			stream.SetVertex(vIndex+2, template);

			template.position.z = zCoordinates.z;
			template.texCoord0.y = 0.75f;
			stream.SetVertex(vIndex+3, template);

			template.position.x = center.x;
			template.position.z = zCoordinates.w;
			template.texCoord0 = float2(0.5f, 1f);
			stream.SetVertex(vIndex+4, template);

			template.position.x = xCoordinates.y;
			template.position.z = zCoordinates.z;
			template.texCoord0 = float2(0.5f + height, 0.75f);
			stream.SetVertex(vIndex+5, template);

			template.position.z = zCoordinates.y;
			template.texCoord0.y = 0.25f;
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
