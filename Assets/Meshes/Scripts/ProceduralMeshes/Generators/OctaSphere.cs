using Meshes.ProceduralMeshes.Streams;

using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace Meshes.ProceduralMeshes.Generators {

public struct OctaSphere : IMeshGenerator {

	struct Rhombus {
		public int id;
		public float3 leftCorner;
		public float3 rightCorner;
	}

	const int TRIANGLES_PER_QUAD = 2;
	const int RHOMBUS_QUADS = 4;
	const int SOUTH_POLE_INDEX = 0;
	const int NORTH_POLE_INDEX = 4;

	public int Resolution { get; set; }
	public int ResolutionSqrd => Resolution*Resolution;

	public int VertexCount => RHOMBUS_QUADS * ResolutionSqrd + 2 * Resolution + 7;
	public int IndexCount => RHOMBUS_QUADS * TRIANGLES_PER_QUAD * ResolutionSqrd * 3;
	public int JobLength => RHOMBUS_QUADS * Resolution + 1;
	public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(2f, 2f, 2f));

	public void Execute<S>(int idx, S stream) where S : struct, IMeshStream {
		if(idx == 0){
			ExecutePolesAndSeam(stream);
		} else {
			ExecuteRegular(idx-1, stream);
		}
	}

	public void ExecuteRegular<S>(int idx, S stream) where S : struct, IMeshStream {

		int u = idx / RHOMBUS_QUADS;
		Rhombus rhombus = GetRhombus(idx - RHOMBUS_QUADS * u);
		int vIndex = Resolution*(Resolution*rhombus.id + u + 2) + 7; // Vertices 0 and 1 are poles hardcoded below
		int tIndex = Resolution*(Resolution*rhombus.id + u)*TRIANGLES_PER_QUAD;
		bool isFirstColumn = u == 0;

		u++;

		float3 columnBottomDir = rhombus.rightCorner - down();
		float3 columnBottomStart = down() + columnBottomDir * u/Resolution;
		float3 columnBottomEnd = rhombus.leftCorner + columnBottomDir * u/Resolution;

		float3 columnTopDir = up() - rhombus.leftCorner;
		float3 columnTopStart = rhombus.rightCorner + columnTopDir * ((float) u/Resolution - 1f);
		float3 columnTopEnd = rhombus.leftCorner + columnTopDir * u/Resolution;

		// Displace rhombus origin
		Vertex template = new Vertex();
		template.position = columnBottomStart;
		stream.SetVertex(vIndex, template);

		int4 quad = 0;
		if (rhombus.id == 0) {
			quad.x = vIndex;
			quad.y = isFirstColumn ? 0 : vIndex - Resolution;
			quad.z = isFirstColumn ? 8 : vIndex - Resolution + 1;
			quad.w = vIndex + 1;
		}
		else if (isFirstColumn) {
			quad.x = vIndex;
			quad.y = rhombus.id;
			quad.z = vIndex - Resolution * (Resolution + u);
			quad.w = vIndex + 1;
		} else quad = 0;

		// int4 quad = int4(
		// 	vIndex, // x
		// 	isFirstColumn ? rhombus.id : vIndex - Resolution, // y
		// 	isFirstColumn
		// 		? rhombus.id == 0
		// 			? 8
		// 			: vIndex - Resolution * (Resolution + u)
		// 		: vIndex - Resolution + 1, // z
		// 	vIndex + 1 // w
		// );

		vIndex++;

		for(int v = 1; v < Resolution; v++){
			if(v <= Resolution-u){
				template.position = lerp(columnBottomStart, columnBottomEnd, (float) v/Resolution);
			} else {
				template.position = lerp(columnTopStart, columnTopEnd, (float) v/Resolution);
			}

			stream.SetVertex(vIndex, template);

			stream.SetTriangle(tIndex+0, quad.xyz);
			stream.SetTriangle(tIndex+1, quad.xzw);

			bool isFirstColumnButNotRhombus = isFirstColumn && rhombus.id != 0;
			quad.y = quad.z;
			quad += int4(1, 0, isFirstColumnButNotRhombus ? Resolution : 1, 1);

			vIndex++;
			tIndex += TRIANGLES_PER_QUAD;
		}

		quad.z = ResolutionSqrd*rhombus.id + Resolution + u + 6;
		quad.w = u < Resolution ? quad.z + 1 : rhombus.id + 4;

		stream.SetTriangle(tIndex+0, quad.xyz);
		stream.SetTriangle(tIndex+1, quad.xzw);
	}

	public void ExecutePolesAndSeam<S>(S stream) where S : struct, IMeshStream {
		Vertex template = new Vertex();
		template.tangent = float4(sqrt(0.5f), 0f, sqrt(0.5f), -1f);
		template.texCoord0.x = 0.125f;

		for(int i = 0; i < 4; i++){

			// South pole
			template.position = down();
			template.normal = down();
			template.texCoord0.y = 0f;
			stream.SetVertex(i, template);

			// North pole
			template.position = up();
			template.normal = up();
			template.texCoord0.y = 1f;
			stream.SetVertex(i+4, template);

			template.tangent.xz = float2(-template.tangent.z, template.tangent.x);
			template.texCoord0.x += 0.25f;
		}

		template.tangent.xz = float2(1f, 0f);
		template.texCoord0.x = 0f;

		for(int v = 1; v < 2*Resolution; v++){
			if(v < Resolution){
				template.position = lerp(down(), back(), (float) v/Resolution);
			} else {
				template.position = lerp(back(), up(), (float) (v-Resolution)/Resolution);
			}

			template.normal = template.position;
			stream.SetVertex(v + 7, template);
		}
	}

	static float3 RhombusToSphere(float3 p) => p * sqrt(1f - ((p*p).yxx + (p*p).zzy)/2f + (p*p).yxx * (p*p).zzy/3f);

	static Rhombus GetRhombus(int id) => id switch {
		0 => new Rhombus {
			id = id,
			leftCorner = back(),
			rightCorner = right()
		},
		1 => new Rhombus {
			id = id,
			leftCorner = right(),
			rightCorner = forward()
		},
		2 => new Rhombus {
			id = id,
			leftCorner = forward(),
			rightCorner = left()
		},
		_ => new Rhombus {
			id = id,
			leftCorner = left(),
			rightCorner = back()
		}
	};
}}
