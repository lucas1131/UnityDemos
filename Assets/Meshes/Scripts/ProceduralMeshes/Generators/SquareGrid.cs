using Meshes.ProceduralMeshes.Streams;

using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace Meshes.ProceduralMeshes.Generators {

public struct SquareGrid : IMeshGenerator {

	const int VERTICES_PER_QUAD = 4;
	const int TRIANGLES_PER_QUAD = 2;

	public int Resolution { get; set; }
	public int ResolutionSqrd => Resolution*Resolution;

	public int VertexCount => VERTICES_PER_QUAD * ResolutionSqrd;
	public int IndexCount => 3*TRIANGLES_PER_QUAD * ResolutionSqrd;
	public int JobLength => Resolution;
	public Bounds Bounds => new Bounds(
		Vector3.zero,
		new Vector3(1f, 0f, 1f)
	);

	public void Execute<S>(int z, S stream) where S : struct, IMeshStream {
		int vIndex = Resolution*z*VERTICES_PER_QUAD;
		int tIndex = Resolution*z*TRIANGLES_PER_QUAD;

		for(int x = 0; x < Resolution; x++){
			float2 xCoordinates = float2(x, x+1f)/Resolution - 0.5f;
			float2 zCoordinates = float2(z, z+1f)/Resolution - 0.5f;

			Vertex template = new Vertex();
			template.normal.y = 1f;
			template.tangent.xw = float2(1f, -1f);

			template.position.x = xCoordinates.x;
			template.position.z = zCoordinates.x;
			stream.SetVertex(vIndex+0, template);

			template.position.x = xCoordinates.y;
			template.texCoord0 = float2(1f, 0f);
			stream.SetVertex(vIndex+1, template);

			template.position.x = xCoordinates.x;
			template.position.z = zCoordinates.y;
			template.texCoord0 = float2(0f, 1f);
			stream.SetVertex(vIndex+2, template);

			template.position.x = xCoordinates.y;
			template.texCoord0 = float2(1f, 1f);
			stream.SetVertex(vIndex+3, template);

			// Sum vIndex here to keep theses indices relative to current quad
			stream.SetTriangle(tIndex+0, vIndex + int3(0, 2, 1));
			stream.SetTriangle(tIndex+1, vIndex + int3(1, 2, 3));

			vIndex += VERTICES_PER_QUAD;
			tIndex += TRIANGLES_PER_QUAD;
		}
	}
}}
