using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace Surfaces.ProceduralSurfaces {

[BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
public struct SurfaceJob : IJobFor {

	NativeArray<float3> positions;

	public void Execute (int i) {
		float3 p = positions[i];
		p.y = abs(p.x);
		positions[i] = p;
	}

	public static JobHandle ScheduleParallel(Mesh.MeshData meshData, int resolution, JobHandle dependency) =>
		new SurfaceJob() {
			positions = meshData.GetVertexData<float3>()
		}.ScheduleParallel(meshData.vertexCount, resolution, dependency);
}}

