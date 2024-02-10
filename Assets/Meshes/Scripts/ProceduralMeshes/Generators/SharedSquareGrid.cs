using Meshes.ProceduralMeshes.Streams;

using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace Meshes.ProceduralMeshes.Generators {

public struct SharedSquareGrid : IMeshGenerator {

	const int TRIANGLES_PER_QUAD = 2;

	public int Resolution { get; set; }
	public int ResolutionSqrd => Resolution*Resolution;

	public int VertexCount => (Resolution+1)*(Resolution+1);
	public int IndexCount => 3*TRIANGLES_PER_QUAD * ResolutionSqrd;
	public int JobLength => Resolution+1;
	public Bounds Bounds => new Bounds(
		Vector3.zero,
		new Vector3(1f, 0f, 1f)
	);

	public void Execute<S>(int zOffset, S stream) where S : struct, IMeshStream {
		int vIndex = (Resolution+1)*zOffset;
		int tIndex = (zOffset-1) * 2*Resolution;

		Vertex template = new Vertex();
		template.normal.y = 1f;
		template.tangent.xw = float2(1f, -1f);

		template.position.x = -0.5f;
		template.position.z = (float) zOffset/Resolution - 0.5f;
		template.texCoord0.y = (float) zOffset/Resolution;
		stream.SetVertex(vIndex, template);
		vIndex++;

		for(int xOffset = 1; xOffset <= Resolution; xOffset++){

			template.position.x = (float) xOffset/Resolution - 0.5f;
			template.texCoord0.x = (float) xOffset/Resolution;
			stream.SetVertex(vIndex, template);

			if(zOffset > 0) {
				int quadPoint0x0Relative = -Resolution-2; // 0
				int quadPoint0x1Relative = -Resolution-1; // 1
				int quadPoint1x0Relative = -1;            // 2
				int quadPoint1x1Relative = 0;             // 3
				stream.SetTriangle(tIndex+0, vIndex + int3(quadPoint0x0Relative, quadPoint1x0Relative, quadPoint0x1Relative));
				stream.SetTriangle(tIndex+1, vIndex + int3(quadPoint0x1Relative, quadPoint1x0Relative, quadPoint1x1Relative));
			}

			vIndex++;
			tIndex += TRIANGLES_PER_QUAD;
		}
	}
}}
