using Meshes.ProceduralMeshes.Streams;

using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace Meshes.ProceduralMeshes.Generators {

public struct UVSphere : IMeshGenerator {

	public int Resolution { get; set; }
	int ResolutionU => 4*Resolution;
	int ResolutionV => 2*Resolution;

	public int VertexCount => (ResolutionU+1)*(ResolutionV+1) - 2; // Discount two polar vertices, the columns have polar vertices of their own
	public int IndexCount => 6 * ResolutionU * (ResolutionV-1); // Discount two generate triangles from the poles (remember that the indices were ResolutionV+1 before)
	public int JobLength => ResolutionU+1;
	public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(2f, 2f, 2f));

	public void Execute<S>(int u, S stream) where S : struct, IMeshStream {
		if (u == 0){
			ExecuteSeam(stream);
		} else {
			ExecuteRegular(u, stream);
		}
	}

	public void ExecuteSeam<S>(S stream) where S : struct, IMeshStream {
		Vertex template = new Vertex();
		template.tangent.x = 1f;
		template.tangent.w = -1f;

		for(int v = 1; v < ResolutionV; v++){
			// Add 180 degrees to invert the sign here so we get the correct position
			sincos(PI + PI*v/ResolutionV,
				out template.position.z,
				out template.position.y);

			template.normal = template.position;
			template.texCoord0.y = (float) v/ResolutionV;
			stream.SetVertex(v-1, template);
		}
	}

	public void ExecuteRegular<S>(int u, S stream) where S : struct, IMeshStream {
		int vIndex = (ResolutionV+1)*u - 2;
		int tIndex = (u-1) * 2*(ResolutionV-1);

		// First vertex (south pole) is a bit different
		Vertex template = new Vertex();
		template.position.y = -1f;
		template.normal.y = -1f;
		sincos(2f*PI*(u-0.5f)/ResolutionU,
			out template.tangent.z,
			out template.tangent.x);

		template.tangent.w = -1f;
		template.texCoord0.x = (u - 0.5f)/ResolutionU;
		stream.SetVertex(vIndex, template);

		// Last vertex (north pole) as well
		template.position.y = 1f;
		template.normal.y = 1f;
		template.texCoord0.y = 1f;
		stream.SetVertex(vIndex+ResolutionV, template);
		vIndex++;

		// Degenerate triangles from south pole
		// int quadPoint0x0Relative = -ResolutionV-2; // 0
		// int quadPoint0x1Relative = -ResolutionV-1; // 1
		int quadPoint1x0Relative = -1;             // 2
		int quadPoint1x1Relative = 0;              // 3

		// Compensate for seam pole and apply a shift for the triangle vertices on poles/seam
		int shiftLeft = (u == 1 ? 0 : -1) - ResolutionV; // Represents vertex #1 shifted to the left
		int shiftLeft2 = shiftLeft-1; // Represents vertex #0 shifted to the left
		stream.SetTriangle(tIndex, vIndex + int3(quadPoint1x0Relative, shiftLeft, quadPoint1x1Relative));
		tIndex++;

		float2 unitCircle;
		sincos(2f*PI*u/ResolutionU,
			out unitCircle.x,
			out unitCircle.y);

		// Template for the following vertices
		template.tangent.xz = unitCircle.yx;
		template.texCoord0.x = (float) u/ResolutionU;
		unitCircle.y = -unitCircle.y;

		for(int v = 1; v < ResolutionV; v++){

			// Add 180 degrees to invert the sign here so we get the correct position but have to invert the radius
			sincos(PI + PI*v/ResolutionV,
				out float radius,
				out template.position.y);

			template.position.xz = unitCircle * -radius;
			template.normal = template.position;
			template.texCoord0.y = (float) v/ResolutionV;
			stream.SetVertex(vIndex, template);

			if(v > 1) {
				stream.SetTriangle(tIndex+0, vIndex + int3(shiftLeft2, shiftLeft, quadPoint1x0Relative));
				stream.SetTriangle(tIndex+1, vIndex + int3(quadPoint1x0Relative, shiftLeft, quadPoint1x1Relative));
				tIndex += 2;
			}

			vIndex++;
		}

		// Degenerate north pole triangles
		stream.SetTriangle(tIndex, vIndex + int3(shiftLeft2, quadPoint1x1Relative, quadPoint1x0Relative));
	}
}}
