using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace Noise {

public class HashVisualization : AbstractVisualization {

	static readonly int hashesId = Shader.PropertyToID("_Hashes");

	[SerializeField] uint seed;
	[SerializeField] SpaceTRS domain = new SpaceTRS { scale = 1f };

    NativeArray<uint4> hashes;
    ComputeBuffer hashesBuffer;
	MaterialPropertyBlock propertyBlock;

	protected override void EnableVisualization(int dataLength, MaterialPropertyBlock propertyBlock){
		hashes = new NativeArray<uint4>(dataLength, Allocator.Persistent);
		hashesBuffer = new ComputeBuffer(dataLength*4, sizeof(uint));

		propertyBlock.SetBuffer(hashesId, hashesBuffer);
	}

	protected override void DisableVisualization(){
		hashes.Dispose();
		hashesBuffer.Release();
		hashesBuffer = null;
	}

	protected override void UpdateVisualization(NativeArray<float3x4> positions, int resolution, JobHandle job){
		new HashJob {
			positions = positions,
			hashes = hashes, 
			seed = SmallXXHash.Seed(seed),
			domain = domain.Matrix,
		}.ScheduleParallel(hashes.Length, resolution, job).Complete();

		hashesBuffer.SetData(hashes.Reinterpret<uint>(4 * sizeof(uint)));
	}

	[BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously=true)]
    struct HashJob : IJobFor {

    	[ReadOnly] public NativeArray<float3x4> positions;
    	[WriteOnly] public NativeArray<uint4> hashes;
    	public uint seed;
    	public float3x4 domain;

    	public void Execute(int idx){
    		// make entire colums correspond to x0, x1, x2, x3 | y0, y1, y2, y3 | z0, z1, z2, z3; then the next array element continues from 4 to 7 and so on
    		float4x3 p = domain.TransformVectors(transpose(positions[idx]));
			
			int4 u = (int4) floor(p.c0);
    		int4 v = (int4) floor(p.c1);
    		int4 w = (int4) floor(p.c2);

    		hashes[idx] = SmallXXHash4.Seed(seed).Eat(u).Eat(v).Eat(w);
    	}
    }
}}
