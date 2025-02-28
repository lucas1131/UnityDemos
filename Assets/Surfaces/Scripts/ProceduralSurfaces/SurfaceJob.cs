using Meshes.ProceduralMeshes.Streams;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


using static Unity.Mathematics.math;

namespace Surfaces.ProceduralSurfaces {

[BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
public struct SurfaceJob : IJobFor {

	struct Vertex4 {
		public SingleStream.Stream0 v0, v1, v2, v3;
	}

	NativeArray<Vertex4> vertexData;

	public void Execute (int i) {
		Vertex4 vertex = vertexData[i];
		vertex.v0.position.y = abs(vertex.v0.position.x);
		vertex.v1.position.y = abs(vertex.v1.position.x);
		vertex.v2.position.y = abs(vertex.v2.position.x);
		vertex.v3.position.y = abs(vertex.v3.position.x);
		vertexData[i] = vertex;
	}

	public static JobHandle ScheduleParallel(Mesh.MeshData meshData, int resolution, JobHandle dependency) =>
		new SurfaceJob() {
			vertexData = meshData.GetVertexData<SingleStream.Stream0>().Reinterpret<Vertex4>(12*4)
		}.ScheduleParallel(meshData.vertexCount/4, resolution, dependency);
}}

