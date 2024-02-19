using Meshes.ProceduralMeshes.Streams;

using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace Meshes.ProceduralMeshes.Generators {

public struct IcoSphere : IMeshGenerator {

	struct Rhombus {
		public int id;
		public float3 leftCorner;
		public float3 rightCorner;
	}

	const int TRIANGLES_PER_QUAD = 2;
	const int NUMBER_OF_RHOMBUSES = 4;
	const int SOUTH_POLE_INDEX = 0;
	const int NORTH_POLE_INDEX = 4;

	public int Resolution { get; set; }
	public int ResolutionSqrd => Resolution*Resolution;

	public int VertexCount => NUMBER_OF_RHOMBUSES * ResolutionSqrd + 2;
	public int IndexCount => NUMBER_OF_RHOMBUSES * TRIANGLES_PER_QUAD * ResolutionSqrd * 3;
	public int JobLength => NUMBER_OF_RHOMBUSES * Resolution;
	public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(2f, 2f, 2f));

	public void Execute<S>(int idx, S stream) where S : struct, IMeshStream {

		int u = idx / NUMBER_OF_RHOMBUSES;
		Rhombus rhombus = GetRhombus(idx - NUMBER_OF_RHOMBUSES * u);
		int vIndex = Resolution*(Resolution*rhombus.id + u) + 2;
		int tIndex = Resolution*(Resolution*rhombus.id + u)*TRIANGLES_PER_QUAD;

		bool isFirstColumn = u == 0;
		// int4 quad = 0;
		// if (rhombus.id == 0) {
		// 	quad.x = vIndex;
		// 	quad.y = isFirstColumn ? 0 : vIndex - Resolution;
		// 	quad.z = isFirstColumn ? 8 : vIndex - Resolution + 1;
		// 	quad.w = vIndex + 1;
		// }
		// else if (isFirstColumn) {
		// 	quad.x = vIndex;
		// 	quad.y = rhombus.id;
		// 	quad.z = vIndex - Resolution * (Resolution + u);
		// 	quad.w = vIndex + 1;
		// } else quad = 0;

		int4 quad = int4(
			vIndex, // x
			isFirstColumn ? 0 : vIndex - Resolution, // y
			isFirstColumn
				? rhombus.id == 0
					? 3*ResolutionSqrd + 2
					: vIndex - Resolution * (Resolution + u)
				: vIndex - Resolution + 1, // z
			vIndex + 1 // w
		);

		u++;

		float3 columnBottomDir = rhombus.rightCorner - down();
		float3 columnBottomStart = down() + columnBottomDir * u/Resolution;
		float3 columnBottomEnd = rhombus.leftCorner + columnBottomDir * u/Resolution;

		float3 columnTopDir = up() - rhombus.leftCorner;
		float3 columnTopStart = rhombus.rightCorner + columnTopDir * ((float) u/Resolution - 1f);
		float3 columnTopEnd = rhombus.leftCorner + columnTopDir * u/Resolution;

		// Displace rhombus origin
		Vertex template = new Vertex();

		if(rhombus.id == 0){
			template.position = down(); // South pole
			stream.SetVertex(0, template);
			template.position = up(); // North pole
			stream.SetVertex(1, template);
		}

		template.position = RhombusToSphere(columnBottomStart);
		stream.SetVertex(vIndex, template);
		vIndex++;

		for(int v = 1; v < Resolution; v++){
			if(v <= Resolution-u){
				template.position = RhombusToSphere(lerp(columnBottomStart, columnBottomEnd, (float) v/Resolution));
			} else {
				template.position = RhombusToSphere(lerp(columnTopStart, columnTopEnd, (float) v/Resolution));
			}

			stream.SetVertex(vIndex, template);

			stream.SetTriangle(tIndex+0, quad.xyz);
			stream.SetTriangle(tIndex+1, quad.xzw);

			quad.y = quad.z;
			quad += int4(1, 0, isFirstColumn ? Resolution : 1, 1);

			vIndex++;
			tIndex += TRIANGLES_PER_QUAD;
		}

		if(isFirstColumn && rhombus.id == 0){
			quad.w = quad.z+1;
		}
		if(!isFirstColumn){
			quad.z = ResolutionSqrd*(rhombus.id == 0 ? 4 : rhombus.id) - Resolution + u + 1;
		}

		quad.w = u < Resolution ? quad.z + 1 : 1;

		stream.SetTriangle(tIndex+0, quad.xyz);
		stream.SetTriangle(tIndex+1, quad.xzw);
	}

	static float3 RhombusToSphere(float3 p) => p; // For debugging
	// static float3 RhombusToSphere(float3 p) => normalize(p);

	static float3 GetCorner(int id) {
		sincos(
			0.4f*PI*id,
			out float sine,
			out float cosine
		);
		return float3(sine, 0, -cosine);
	}

	static Rhombus GetRhombus(int id) => id switch {
		0 => new Rhombus {
			id = id,
			leftCorner = GetCorner(0),
			rightCorner = GetCorner(1)
		},
		1 => new Rhombus {
			id = id,
			leftCorner = GetCorner(1),
			rightCorner = GetCorner(2)
		},
		2 => new Rhombus {
			id = id,
			leftCorner = GetCorner(2),
			rightCorner = GetCorner(3)
		},
		3 => new Rhombus {
			id = id,
			leftCorner = GetCorner(3),
			rightCorner = GetCorner(4)
		},
		_ => new Rhombus {
			id = id,
			leftCorner = GetCorner(4),
			rightCorner = GetCorner(0)
		}
	};
}}
