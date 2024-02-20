using Meshes.ProceduralMeshes.Streams;

using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace Meshes.ProceduralMeshes.Generators {

public struct IcoSphere : IMeshGenerator {

	struct Strip {
		public int id;
		public float3 lowerLeftCorner;
		public float3 lowerRightCorner;
		public float3 upperLeftCorner;
		public float3 upperRightCorner;
	}

	const int TRIANGLES_PER_QUAD = 2;
	const int NUMBER_OF_RHOMBUSES = 5;

	public int Resolution { get; set; }
	public int ResolutionV => 2*Resolution;
	public int ResolutionSqrd => Resolution*Resolution;
	public int EffectiveResolutionSqrd => ResolutionV*Resolution;

	public int VertexCount => NUMBER_OF_RHOMBUSES * EffectiveResolutionSqrd + 2;
	public int IndexCount => NUMBER_OF_RHOMBUSES * TRIANGLES_PER_QUAD * EffectiveResolutionSqrd * 3;
	public int JobLength => NUMBER_OF_RHOMBUSES * Resolution;
	public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(2f, 2f, 2f));

	public void Execute<S>(int idx, S stream) where S : struct, IMeshStream {

		int u = idx / NUMBER_OF_RHOMBUSES;
		Strip strip = GetStrip(idx - NUMBER_OF_RHOMBUSES * u);
		int vIndex = ResolutionV*(Resolution*strip.id + u) + 2;
		int tIndex = ResolutionV*(Resolution*strip.id + u)*TRIANGLES_PER_QUAD;

		bool isFirstColumn = u == 0;

		int4 quad = int4(
			vIndex, // x
			isFirstColumn ? 0 : vIndex - ResolutionV, // y
			isFirstColumn
				? strip.id == 0
					? (NUMBER_OF_RHOMBUSES-1)*EffectiveResolutionSqrd + 2
					: vIndex - ResolutionV * (Resolution + u)
				: vIndex - ResolutionV + 1, // z
			vIndex + 1 // w
		);

		u++;

		float3 columnBottomDir = strip.lowerRightCorner - down();
		float3 columnBottomStart = down() + columnBottomDir * u/Resolution;
		float3 columnBottomEnd = strip.lowerLeftCorner + columnBottomDir * u/Resolution;


		float3 ringColumnMidLowerDir = strip.upperRightCorner - strip.lowerLeftCorner;
		float3 ringColumnMidLowerStart = strip.lowerRightCorner + ringColumnMidLowerDir * ((float) u/Resolution - 1f);
		float3 ringColumnMidLowerEnd = strip.lowerLeftCorner + ringColumnMidLowerDir * u/Resolution;


		float3 ringColumnMidUpperDir = strip.upperRightCorner - strip.lowerLeftCorner;
		float3 ringColumnMidUpperStart = strip.lowerLeftCorner + ringColumnMidUpperDir * u/Resolution;
		float3 ringColumnMidUpperEnd = strip.upperLeftCorner + ringColumnMidUpperDir * u/Resolution;


		float3 columnTopDir = up() - strip.upperLeftCorner;
		float3 columnTopStart = strip.upperRightCorner + columnTopDir * ((float) u/Resolution - 1f);
		float3 columnTopEnd = strip.upperLeftCorner + columnTopDir * u/Resolution;

		// Displace strip origin
		Vertex template = new Vertex();

		if(strip.id == 0){
			template.position = down(); // South pole
			stream.SetVertex(0, template);
			template.position = up(); // North pole
			stream.SetVertex(1, template);
		}

		template.position = StripToSphere(columnBottomStart);
		stream.SetVertex(vIndex, template);
		vIndex++;

		for(int v = 1; v < ResolutionV; v++){
			if(v <= Resolution-u){
				template.position = StripToSphere(lerp(columnBottomStart, columnBottomEnd, (float) v/Resolution));
			} else if(v < Resolution){
				template.position = StripToSphere(lerp(ringColumnMidLowerStart, ringColumnMidLowerEnd, (float) v/Resolution));
			} else if(v < ResolutionV - u){
				template.position = StripToSphere(lerp(ringColumnMidUpperStart, ringColumnMidUpperEnd, (float) v/Resolution - 1f));
			} else {
				template.position = StripToSphere(lerp(columnTopStart, columnTopEnd, (float) v/Resolution - 1f));
			}

			stream.SetVertex(vIndex, template);

			stream.SetTriangle(tIndex+0, quad.xyz);
			stream.SetTriangle(tIndex+1, quad.xzw);

			quad.y = quad.z;
			quad += int4(1, 0, isFirstColumn && v <= Resolution-u ? ResolutionV : 1, 1);

			vIndex++;
			tIndex += TRIANGLES_PER_QUAD;
		}

		if(isFirstColumn && strip.id == 0){
			quad.w = quad.z+1;
		}
		if(!isFirstColumn){
			quad.z = EffectiveResolutionSqrd*(strip.id == 0 ? NUMBER_OF_RHOMBUSES : strip.id) - Resolution + u + 1;
		}

		quad.w = u < Resolution ? quad.z + 1 : 1;

		stream.SetTriangle(tIndex+0, quad.xyz);
		stream.SetTriangle(tIndex+1, quad.xzw);
	}

	// static float3 StripToSphere(float3 p) => p; // For debugging
	static float3 StripToSphere(float3 p) => normalize(p);

	static float3 GetCorner(int id, int ySign) {
		float cornerRingScale = 0.4f * sqrt(5f);
		sincos(
			0.2f*PI*id,
			out float sine,
			out float cosine
		);
		return float3(sine, ySign*0.5f, -cosine)*cornerRingScale;
	}

	// Make sure to use constant values when creating strip so burst use these structs as constant values
	static Strip GetStrip(int id) => id switch {
		0 => CreateStrip(0),
		1 => CreateStrip(1),
		2 => CreateStrip(2),
		3 => CreateStrip(3),
		_ => CreateStrip(4),
	};

	static Strip CreateStrip(int id) => new Strip {
		id = id,
		lowerLeftCorner  = GetCorner(2*id, -1),
		lowerRightCorner = GetCorner(id == 4 ? 0 : 2*id + 2, -1),
		upperLeftCorner  = GetCorner(id == 0 ? 9 : 2*id - 1, 1),
		upperRightCorner = GetCorner(2*id + 1, 1)
	};
}}
