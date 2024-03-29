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
	const int NUMBER_OF_RHOMBUSES = 4;
	const int SOUTH_POLE_INDEX = 0;
	const int NORTH_POLE_INDEX = 4;

	public int Resolution { get; set; }
	public int ResolutionSqrd => Resolution*Resolution;

	public int VertexCount => NUMBER_OF_RHOMBUSES * ResolutionSqrd + 2 * Resolution + 7;
	public int IndexCount => NUMBER_OF_RHOMBUSES * TRIANGLES_PER_QUAD * ResolutionSqrd * 3;
	public int JobLength => NUMBER_OF_RHOMBUSES * Resolution + 1;
	public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(2f, 2f, 2f));

	public void Execute<S>(int idx, S stream) where S : struct, IMeshStream {
		if(idx == 0){
			ExecutePolesAndSeam(stream);
		} else {
			ExecuteRegular(idx-1, stream);
		}
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

			template.tangent.xz = GetTangentXZ(template.tangent.xyz);
			template.texCoord0.x += 0.25f;
		}

		template.tangent.xz = float2(1f, 0f);
		template.texCoord0.x = 0f;


		for(int v = 1; v < 2*Resolution; v++){
			if(v < Resolution){
				template.position = RhombusToSphere(lerp(down(), back(), (float) v/Resolution));
			} else {
				template.position = RhombusToSphere(lerp(back(), up(), (float) (v-Resolution)/Resolution));
			}

			template.texCoord0.y = GetTexCoord0(template.position).y;
			template.normal = template.position;
			stream.SetVertex(v + 7, template);
		}
	}

	public void ExecuteRegular<S>(int idx, S stream) where S : struct, IMeshStream {

		int u = idx / NUMBER_OF_RHOMBUSES;
		Rhombus rhombus = GetRhombus(idx - NUMBER_OF_RHOMBUSES * u);
		int vIndex = Resolution*(Resolution*rhombus.id + u + 2) + 7; // Vertices 0 and 1 are poles hardcoded below
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
			isFirstColumn ? rhombus.id : vIndex - Resolution, // y
			isFirstColumn
				? rhombus.id == 0
					? 8
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
		template.position = RhombusToSphere(columnBottomStart);
		template.normal = template.position;
		template.tangent.xz = GetTangentXZ(template.position);
		template.tangent.w = -1f;
		template.texCoord0 = GetTexCoord0(template.position);
		stream.SetVertex(vIndex, template);
		vIndex++;

		for(int v = 1; v < Resolution; v++){
			if(v <= Resolution-u){
				template.position = RhombusToSphere(lerp(columnBottomStart, columnBottomEnd, (float) v/Resolution));
			} else {
				template.position = RhombusToSphere(lerp(columnTopStart, columnTopEnd, (float) v/Resolution));
			}

			template.normal = template.position;
			template.tangent.xz = GetTangentXZ(template.position);
			template.texCoord0 = GetTexCoord0(template.position);
			stream.SetVertex(vIndex, template);

			stream.SetTriangle(tIndex+0, quad.xyz);
			stream.SetTriangle(tIndex+1, quad.xzw);

			bool isFirstColumnAndNotRhombus = isFirstColumn && rhombus.id != 0;
			quad.y = quad.z;
			quad += int4(1, 0, isFirstColumnAndNotRhombus ? Resolution : 1, 1);

			vIndex++;
			tIndex += TRIANGLES_PER_QUAD;
		}

		if(isFirstColumn && rhombus.id == 0){
			quad.w = quad.z+1;
		}
		quad.z = ResolutionSqrd*rhombus.id + Resolution + u + 6;
		quad.w = u < Resolution ? quad.z + 1 : rhombus.id + 4;

		stream.SetTriangle(tIndex+0, quad.xyz);
		stream.SetTriangle(tIndex+1, quad.xzw);
	}

	static float2 GetTangentXZ(float3 p) => normalize(float2(-p.z, p.x));
	static float3 RhombusToSphere(float3 p) => normalize(p);
	static float2 GetTexCoord0(float3 p) {
		float u = atan2(p.x, p.z)/(-2f*PI) + 0.5f;
		float v = asin(p.y)/PI + 0.5f;
		float2 texCoord = float2(u, v);

		if(texCoord.x < 1e-6){
			texCoord.x = 1f;
		}

		return texCoord;
	}

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
