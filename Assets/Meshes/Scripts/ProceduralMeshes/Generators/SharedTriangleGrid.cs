using Meshes.ProceduralMeshes.Streams;

using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace Meshes.ProceduralMeshes.Generators {

public struct SharedTriangleGrid : IMeshGenerator {

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

	public void Execute<S>(int z, S stream) where S : struct, IMeshStream {
		int vIndex = (Resolution+1)*z;
		int tIndex = (z-1) * 2*Resolution;

		int quadPoint0x0Relative = -Resolution-2; // 0
		int quadPoint1x0Relative = -Resolution-1; // 1
		int quadPoint0x1Relative = -1;            // 2
		int quadPoint1x1Relative = 0;             // 3
		int3 tA = int3(quadPoint0x0Relative, quadPoint0x1Relative, quadPoint1x1Relative);
		int3 tB = int3(quadPoint0x0Relative, quadPoint1x1Relative, quadPoint1x0Relative);

		float xOffset = -0.25f;
		float uOffset = 0f;
		if((z&1) == 1) {
			xOffset = 0.25f;
			uOffset = 0.5f/(Resolution + 0.5f);

 			// Invert triangles orientation
			tA = int3(quadPoint0x0Relative, quadPoint0x1Relative, quadPoint1x0Relative);
			tB = int3(quadPoint1x0Relative, quadPoint0x1Relative, quadPoint1x1Relative);
		}

		xOffset = xOffset/Resolution - 0.5f;

		Vertex template = new Vertex();
		template.normal.y = 1f;
		template.tangent.xw = float2(1f, -1f);
		template.position.x = xOffset;
		template.position.z = (float) z/Resolution - 0.5f;
		template.position.z *= sqrt(3)/2f;
		template.texCoord0.y = template.position.z/(1f + 0.5f/Resolution) + 0.5f;
		stream.SetVertex(vIndex, template);
		vIndex++;

		for(int x = 1; x <= Resolution; x++){

			template.position.x = (float) x/Resolution + xOffset;
			template.texCoord0.x = (float) x/(Resolution + 0.5f) + uOffset;
			stream.SetVertex(vIndex, template);

			if(z > 0) {
				stream.SetTriangle(tIndex+0, vIndex + tA);
				stream.SetTriangle(tIndex+1, vIndex + tB);
			}

			vIndex++;
			tIndex += TRIANGLES_PER_QUAD;
		}
	}
}}
