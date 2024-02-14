using Meshes.ProceduralMeshes.Streams;

using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace Meshes.ProceduralMeshes.Generators {

public struct SharedCubeSphere : IMeshGenerator {

	struct Side {
		public int id;
		public float3 uvOrigin;
		public float3 uVector;
		public float3 vVector;
		public int seamStep;

		public bool TouchesMinimumPole => (id & 1) == 0;
	}

	const int TRIANGLES_PER_QUAD = 2;
	const int CUBE_QUADS = 6;

	public int Resolution { get; set; }
	public int ResolutionSqrd => Resolution*Resolution;

	public int VertexCount => CUBE_QUADS * ResolutionSqrd + 2;
	public int IndexCount => CUBE_QUADS * TRIANGLES_PER_QUAD * ResolutionSqrd * 3;
	public int JobLength => CUBE_QUADS * Resolution;
	public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(2f, 2f, 2f));

	public void Execute<S>(int idx, S stream) where S : struct, IMeshStream {

		int u = idx / CUBE_QUADS;
		Side side = GetSide(idx - CUBE_QUADS * u);
		int vIndex = Resolution*(Resolution*side.id + u) + 2; // Vertices 0 and 1 are poles hardcoded below
		int tIndex = Resolution*(Resolution*side.id + u)*TRIANGLES_PER_QUAD;
		bool isFirstColumn = u == 0;

		u++;

		// Displace side origin
		float3 pStart = side.uvOrigin + side.uVector * u/Resolution;
		Vertex template = new Vertex();

		// Two polar vertices
		if(idx == 0){
			template.position = -sqrt(1f/3f);
			stream.SetVertex(0, template);
			template.position = sqrt(1f/3f);
			stream.SetVertex(1, template);
		}

		template.position = CubeToSphere(pStart);
		stream.SetVertex(vIndex, template);

		// South pole seam/triangle
		int3 triangle = GenerateFirstTriangle(isFirstColumn, side, vIndex);
		stream.SetTriangle(tIndex, triangle);

		vIndex++;
		tIndex++;

		// Z offset variations
		/*
			same logic as below
			if(v == Resolution-1){
				if(isFirstColumn && side.TouchesMinimumPole) {
					triangle.z += Resolution;
				} else if(!isFirstColumn && !side.TouchesMinimumPole) {
					triangle.z += Resolution * ((side.seamStep + 1) * Resolution - u) + u;
				} else {
					triangle.z += (side.seamStep + 1) * ResolutionSqrd - Resolution + 1;
				}
			} else {
				triangle.z += Resolution;
			}
		*/
		int zOffset = isFirstColumn && side.TouchesMinimumPole ? Resolution : 1;
		int zLastOffset =
			isFirstColumn && side.TouchesMinimumPole
			? Resolution
			: !isFirstColumn && !side.TouchesMinimumPole
				? Resolution * ((side.seamStep + 1) * Resolution - u) + u
				: (side.seamStep + 1) * ResolutionSqrd - Resolution + 1;

		for(int v = 1; v < Resolution; v++){
			template.position = CubeToSphere(pStart + side.vVector * v/Resolution);
			stream.SetVertex(vIndex, template);

			triangle.x += 1;
			triangle.y = triangle.z;
			triangle.z += (v == Resolution-1) ? zLastOffset : zOffset;
			stream.SetTriangle(tIndex+0, int3(triangle.x-1, triangle.y, triangle.x));
			stream.SetTriangle(tIndex+1, triangle);

			vIndex++;
			tIndex += TRIANGLES_PER_QUAD;
		}

		// North pole seam/triangle
		triangle = int3(
			triangle.x,
			triangle.z,
			side.TouchesMinimumPole
				? triangle.z + Resolution
				: u == Resolution
					? 1
					: triangle.z + 1
		);
		stream.SetTriangle(tIndex, triangle);
	}

	int3 GenerateFirstTriangle(bool isFirstColumn, Side side, int vIndex){
		int3 triangle;

		/* Both of these are the same logic and neither are easy to read, pick your poison
		if(isFirstColumn){
			if(side.TouchesMinimumPole){
				triangle = int3(vIndex, 0, vIndex + side.seamStep*ResolutionSqrd);
			} else {
				triangle = vIndex + int3(0, -Resolution, -Resolution+1);
			}
		} else {
			triangle = vIndex + int3(0, -Resolution, Resolution == 1 ? side.seamStep : -Resolution+1);
		}
		*/

		// Using this because its a bit easier for the compiler to optimize ternary operators compared to ifs
		// although the final code is the same in both cases
		triangle = int3(
			vIndex,
			isFirstColumn && side.TouchesMinimumPole ? 0 : vIndex - Resolution,
			vIndex +
			(
				isFirstColumn
				? side.TouchesMinimumPole
					? side.seamStep * ResolutionSqrd
					: Resolution == 1
						? side.seamStep
						: -Resolution + 1
				: -Resolution + 1
			)
		);

		return triangle;
	}

	static float3 CubeToSphere(float3 p) => p * sqrt(1f - ((p*p).yxx + (p*p).zzy)/2f + (p*p).yxx * (p*p).zzy/3f);

	static Side GetSide(int id) => id switch {
		0 => new Side { // Front side
			id = id,
			uvOrigin = -1f,
			uVector = 2f * right(),
			vVector = 2f * up(),
			seamStep = 4,
		},
		1 => new Side { // Right side
			id = id,
			uvOrigin = float3(1f, -1f, -1f),
			uVector = 2f * forward(),
			vVector = 2f * up(),
			seamStep = 4,
		},
		2 => new Side { // Down side
			id = id,
			uvOrigin = -1f,
			uVector = 2f * forward(),
			vVector = 2f * right(),
			seamStep = -2,
		},
		3 => new Side { // Back side
			id = id,
			uvOrigin = float3(-1f, -1f, 1f),
			uVector = 2f * up(),
			vVector = 2f * right(),
			seamStep = -2,
		},
		4 => new Side { // Left side
			id = id,
			uvOrigin = -1f,
			uVector = 2f * up(),
			vVector = 2f * forward(),
			seamStep = -2,
		},
		5 => new Side { // Up side
			id = id,
			uvOrigin = float3(-1f, 1f, -1f),
			uVector = 2f * right(),
			vVector = 2f * forward(),
			seamStep = -2,
		},
		_ => new Side { // Shouldn't happen but defaults to Front side
			id = id,
			uvOrigin = -1f,
			uVector = 2f * right(),
			vVector = 2f * up(),
			seamStep = 4,
		}
	};
}}
