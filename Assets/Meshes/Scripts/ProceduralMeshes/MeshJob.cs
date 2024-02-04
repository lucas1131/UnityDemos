using Meshes.ProceduralMeshes.Generators;
using Meshes.ProceduralMeshes.Streams;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Meshes.ProceduralMeshes {

[BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
public struct MeshJob<G, S> : IJobFor
	where G : struct, IMeshGenerator
	where S : struct, IMeshStream {

	G generator;
	[WriteOnly] S stream;

	public void Execute(int index) => generator.Execute(index, stream);

	public static JobHandle ScheduleParallel(Mesh.MeshData meshData, JobHandle dependency) {
		var job = new MeshJob<G, S>();
		job.stream.Setup(meshData, job.generator.VertexCount, job.generator.IndexCount);
		return job.ScheduleParallel(job.generator.JobLength, 1, dependency);
	}
}}
