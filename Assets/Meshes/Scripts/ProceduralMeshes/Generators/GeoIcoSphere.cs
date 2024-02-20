using Meshes.ProceduralMeshes.Streams;

using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

using quaternion = Unity.Mathematics.quaternion;

namespace Meshes.ProceduralMeshes.Generators {

public struct GeoIcoSphere : IMeshGenerator {

	struct Strip {
		public int id;
		public float3 lowerLeftCorner;
		public float3 lowerRightCorner;
		public float3 upperLeftCorner;
		public float3 upperRightCorner;

		public float3 lowerLeftAxis, lowerRightAxis;
		public float3 midLeftAxis, midCenterAxis, midRightAxis;
		public float3 upperLeftAxis, upperRightAxis;

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

	// Keep this as a static property, burst will optimize to a constant. If you make this a member variable,
	// burst will treat this as a variable and not a constant to preserve the struct layout
	static float EdgeRotationAngle => acos(dot(up(), GetCorner(0, 1)));

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

		float3 ringColumnMidLowerDir = strip.upperRightCorner - strip.lowerLeftCorner;
		float3 ringColumnMidLowerStart = strip.lowerRightCorner + ringColumnMidLowerDir * ((float) u/Resolution - 1f);
		float3 ringColumnMidLowerEnd = strip.lowerLeftCorner + ringColumnMidLowerDir * u/Resolution;

		float3 ringColumnMidUpperDir = strip.upperRightCorner - strip.lowerLeftCorner;
		float3 ringColumnMidUpperStart = strip.lowerLeftCorner + ringColumnMidUpperDir * u/Resolution;
		float3 ringColumnMidUpperEnd = strip.upperLeftCorner + ringColumnMidUpperDir * u/Resolution;

		// Displace strip origin
		Vertex template = new Vertex();

		if(strip.id == 0){
			template.position = down(); // South pole
			stream.SetVertex(0, template);
			template.position = up(); // North pole
			stream.SetVertex(1, template);
		}

		template.position = mul(
			quaternion.AxisAngle(strip.lowerRightAxis, EdgeRotationAngle*u/Resolution),
			down()
		);

		template.position = StripToSphere(template.position);
		stream.SetVertex(vIndex, template);
		vIndex++;

		for(int v = 1; v < ResolutionV; v++){
			float height = u+v;
			float edgeAngleScale;
			float faceAngleScale;
			float3 leftAxis, rightAxis;
			float3 leftStart, rightStart;

			if(v <= Resolution - u){
				leftAxis = strip.lowerLeftAxis;
				rightAxis = strip.lowerRightAxis;
				leftStart = down();
				rightStart = down();
				edgeAngleScale = height/Resolution;
				faceAngleScale = v/height;

			} else if(v < Resolution){
				leftAxis = strip.midCenterAxis;
				rightAxis = strip.midRightAxis;
				leftStart = strip.lowerLeftCorner;
				rightStart = strip.lowerRightCorner;
				edgeAngleScale = height/Resolution - 1f;
				faceAngleScale = (Resolution - u)/(ResolutionV - height);

			} else if(v <= ResolutionV - u){
				leftAxis = strip.midLeftAxis;
				rightAxis = strip.midCenterAxis;
				leftStart = strip.lowerLeftCorner;
				rightStart = strip.lowerLeftCorner;
				edgeAngleScale = height/Resolution - 1f;
				faceAngleScale = (v - Resolution)/(height-Resolution);

			} else {
				leftAxis = strip.upperLeftAxis;
				rightAxis = strip.upperRightAxis;
				leftStart = strip.upperLeftCorner;
				rightStart = strip.upperRightCorner;
				edgeAngleScale = height/Resolution - 2f;
				faceAngleScale = (Resolution - u)/(3f*Resolution - height);
			}

			float3 leftPos = mul(quaternion.AxisAngle(leftAxis, EdgeRotationAngle*edgeAngleScale), leftStart);
			float3 rightPos = mul(quaternion.AxisAngle(rightAxis, EdgeRotationAngle*edgeAngleScale), rightStart);

			float3 axis = normalize(cross(rightPos, leftPos));
			float angle = acos(dot(rightPos, leftPos));
			template.position = mul(quaternion.AxisAngle(axis, angle*faceAngleScale), rightPos);

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

	static float3 StripToSphere(float3 p) => p; // For debugging
	// static float3 StripToSphere(float3 p) => normalize(p);

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

	static Strip CreateStrip(int id){
		Strip strip = new Strip {
			id = id,
			lowerLeftCorner  = GetCorner(2*id, -1),
			lowerRightCorner = GetCorner(id == 4 ? 0 : 2*id + 2, -1),
			upperLeftCorner  = GetCorner(id == 0 ? 9 : 2*id - 1, 1),
			upperRightCorner = GetCorner(2*id + 1, 1)
		};

		strip.lowerLeftAxis = normalize(cross(down(), strip.lowerLeftCorner));
		strip.lowerRightAxis = normalize(cross(down(), strip.lowerRightCorner));

		strip.midLeftAxis = normalize(cross(strip.lowerLeftCorner, strip.upperLeftCorner));
		strip.midCenterAxis = normalize(cross(strip.lowerLeftCorner, strip.upperRightCorner));
		strip.midRightAxis = normalize(cross(strip.lowerRightCorner, strip.upperRightCorner));

		strip.upperLeftAxis = normalize(cross(strip.upperLeftCorner, up()));
		strip.upperRightAxis = normalize(cross(strip.upperRightCorner, up()));

		return strip;
	}
}}
