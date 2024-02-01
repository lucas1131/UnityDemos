using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

using static Noise.Noise;

namespace Noise {

public class NoiseVisualization : AbstractVisualization {

	public enum NoiseType { 
		Value,
		ValueTurbulence,

		Perlin,
		PerlinTurbulence,
		
		SimplexValue,
		SimplexTurbulenceValue,

		VoronoiWorleyF1,
		VoronoiWorleyF2,
		VoronoiWorleyCell,
		VoronoiWorleyF2MinusF1,

		VoronoiChebychevF1,
		VoronoiChebychevF2,
		VoronoiChebychevCell,
		VoronoiChebychevF2MinusF1,

		VoronoiManhattanF1,
		VoronoiManhattanF2,
		VoronoiManhattanCell,
		VoronoiManhattanF2MinusF1,
	}

	public enum Noises { Lattice1D, Lattice2D, Lattice3D }

	static Noise.ScheduleDelegate[,] noiseJobs = {
		/* Value noise */
		{
			NoiseJob<Lattice1D<LatticeNormal,  Value>>.ScheduleParallel,
			NoiseJob<Lattice1D<LatticeTilling, Value>>.ScheduleParallel,
			NoiseJob<Lattice2D<LatticeNormal,  Value>>.ScheduleParallel,
			NoiseJob<Lattice2D<LatticeTilling, Value>>.ScheduleParallel,
			NoiseJob<Lattice3D<LatticeNormal,  Value>>.ScheduleParallel,
			NoiseJob<Lattice3D<LatticeTilling, Value>>.ScheduleParallel,
		},
		{
			NoiseJob<Lattice1D<LatticeNormal,  Turbulence<Value>>>.ScheduleParallel,
			NoiseJob<Lattice1D<LatticeTilling, Turbulence<Value>>>.ScheduleParallel,
			NoiseJob<Lattice2D<LatticeNormal,  Turbulence<Value>>>.ScheduleParallel,
			NoiseJob<Lattice2D<LatticeTilling, Turbulence<Value>>>.ScheduleParallel,
			NoiseJob<Lattice3D<LatticeNormal,  Turbulence<Value>>>.ScheduleParallel,
			NoiseJob<Lattice3D<LatticeTilling, Turbulence<Value>>>.ScheduleParallel,
		},

		/* Perlin noise*/
		{
			NoiseJob<Lattice1D<LatticeNormal,  Perlin>>.ScheduleParallel,
			NoiseJob<Lattice1D<LatticeTilling, Perlin>>.ScheduleParallel,
			NoiseJob<Lattice2D<LatticeNormal,  Perlin>>.ScheduleParallel,
			NoiseJob<Lattice2D<LatticeTilling, Perlin>>.ScheduleParallel,
			NoiseJob<Lattice3D<LatticeNormal,  Perlin>>.ScheduleParallel,
			NoiseJob<Lattice3D<LatticeTilling, Perlin>>.ScheduleParallel,
		},
		{
			NoiseJob<Lattice1D<LatticeNormal,  Turbulence<Perlin>>>.ScheduleParallel,
			NoiseJob<Lattice1D<LatticeTilling, Turbulence<Perlin>>>.ScheduleParallel,
			NoiseJob<Lattice2D<LatticeNormal,  Turbulence<Perlin>>>.ScheduleParallel,
			NoiseJob<Lattice2D<LatticeTilling, Turbulence<Perlin>>>.ScheduleParallel,
			NoiseJob<Lattice3D<LatticeNormal,  Turbulence<Perlin>>>.ScheduleParallel,
			NoiseJob<Lattice3D<LatticeTilling, Turbulence<Perlin>>>.ScheduleParallel,
		},

		/* Simplex value noise - duplicated entries for simplicity here */
		{
			NoiseJob<Simplex1D<Value>>.ScheduleParallel,
			NoiseJob<Simplex1D<Value>>.ScheduleParallel,
			NoiseJob<Simplex2D<Value>>.ScheduleParallel,
			NoiseJob<Simplex2D<Value>>.ScheduleParallel,
			NoiseJob<Simplex3D<Value>>.ScheduleParallel,
			NoiseJob<Simplex3D<Value>>.ScheduleParallel,
		},
		{
			NoiseJob<Simplex1D<Turbulence<Value>>>.ScheduleParallel,
			NoiseJob<Simplex1D<Turbulence<Value>>>.ScheduleParallel,
			NoiseJob<Simplex2D<Turbulence<Value>>>.ScheduleParallel,
			NoiseJob<Simplex2D<Turbulence<Value>>>.ScheduleParallel,
			NoiseJob<Simplex3D<Turbulence<Value>>>.ScheduleParallel,
			NoiseJob<Simplex3D<Turbulence<Value>>>.ScheduleParallel,
		},

		/* Voronoi-Worley Noise (Euclidean distance) */
		{
			NoiseJob<Voronoi1D<LatticeNormal,  Worley, F1>>.ScheduleParallel,
			NoiseJob<Voronoi1D<LatticeTilling, Worley, F1>>.ScheduleParallel,
			NoiseJob<Voronoi2D<LatticeNormal,  Worley, F1>>.ScheduleParallel,
			NoiseJob<Voronoi2D<LatticeTilling, Worley, F1>>.ScheduleParallel,
			NoiseJob<Voronoi3D<LatticeNormal,  Worley, F1>>.ScheduleParallel,
			NoiseJob<Voronoi3D<LatticeTilling, Worley, F1>>.ScheduleParallel,
		},
		{
			NoiseJob<Voronoi1D<LatticeNormal,  Worley, F2>>.ScheduleParallel,
			NoiseJob<Voronoi1D<LatticeTilling, Worley, F2>>.ScheduleParallel,
			NoiseJob<Voronoi2D<LatticeNormal,  Worley, F2>>.ScheduleParallel,
			NoiseJob<Voronoi2D<LatticeTilling, Worley, F2>>.ScheduleParallel,
			NoiseJob<Voronoi3D<LatticeNormal,  Worley, F2>>.ScheduleParallel,
			NoiseJob<Voronoi3D<LatticeTilling, Worley, F2>>.ScheduleParallel,
		},
		{
			NoiseJob<Voronoi1D<LatticeNormal,  Worley, Cell>>.ScheduleParallel,
			NoiseJob<Voronoi1D<LatticeTilling, Worley, Cell>>.ScheduleParallel,
			NoiseJob<Voronoi2D<LatticeNormal,  Worley, Cell>>.ScheduleParallel,
			NoiseJob<Voronoi2D<LatticeTilling, Worley, Cell>>.ScheduleParallel,
			NoiseJob<Voronoi3D<LatticeNormal,  Worley, Cell>>.ScheduleParallel,
			NoiseJob<Voronoi3D<LatticeTilling, Worley, Cell>>.ScheduleParallel,
		},
		{
			NoiseJob<Voronoi1D<LatticeNormal,  Worley, F2MinusF1>>.ScheduleParallel,
			NoiseJob<Voronoi1D<LatticeTilling, Worley, F2MinusF1>>.ScheduleParallel,
			NoiseJob<Voronoi2D<LatticeNormal,  Worley, F2MinusF1>>.ScheduleParallel,
			NoiseJob<Voronoi2D<LatticeTilling, Worley, F2MinusF1>>.ScheduleParallel,
			NoiseJob<Voronoi3D<LatticeNormal,  Worley, F2MinusF1>>.ScheduleParallel,
			NoiseJob<Voronoi3D<LatticeTilling, Worley, F2MinusF1>>.ScheduleParallel,
		},

		/* Voronoi-Chebychev Noise (chess-like distance) */
		/* Note that 1D Chebychev and Worley are the same, so to reduce compilation redundancy, we just
		use one of them everywhere */
		{
			NoiseJob<Voronoi1D<LatticeNormal,  Worley,    F1>>.ScheduleParallel,
			NoiseJob<Voronoi1D<LatticeTilling, Worley,    F1>>.ScheduleParallel,
			NoiseJob<Voronoi2D<LatticeNormal,  Chebychev, F1>>.ScheduleParallel,
			NoiseJob<Voronoi2D<LatticeTilling, Chebychev, F1>>.ScheduleParallel,
			NoiseJob<Voronoi3D<LatticeNormal,  Chebychev, F1>>.ScheduleParallel,
			NoiseJob<Voronoi3D<LatticeTilling, Chebychev, F1>>.ScheduleParallel,
		},
		{
			NoiseJob<Voronoi1D<LatticeNormal,  Worley,    F2>>.ScheduleParallel,
			NoiseJob<Voronoi1D<LatticeTilling, Worley,    F2>>.ScheduleParallel,
			NoiseJob<Voronoi2D<LatticeNormal,  Chebychev, F2>>.ScheduleParallel,
			NoiseJob<Voronoi2D<LatticeTilling, Chebychev, F2>>.ScheduleParallel,
			NoiseJob<Voronoi3D<LatticeNormal,  Chebychev, F2>>.ScheduleParallel,
			NoiseJob<Voronoi3D<LatticeTilling, Chebychev, F2>>.ScheduleParallel,
		},
		{
			NoiseJob<Voronoi1D<LatticeNormal,  Worley,    Cell>>.ScheduleParallel,
			NoiseJob<Voronoi1D<LatticeTilling, Worley,    Cell>>.ScheduleParallel,
			NoiseJob<Voronoi2D<LatticeNormal,  Chebychev, Cell>>.ScheduleParallel,
			NoiseJob<Voronoi2D<LatticeTilling, Chebychev, Cell>>.ScheduleParallel,
			NoiseJob<Voronoi3D<LatticeNormal,  Chebychev, Cell>>.ScheduleParallel,
			NoiseJob<Voronoi3D<LatticeTilling, Chebychev, Cell>>.ScheduleParallel,
		},
		{
			NoiseJob<Voronoi1D<LatticeNormal,  Worley,    F2MinusF1>>.ScheduleParallel,
			NoiseJob<Voronoi1D<LatticeTilling, Worley,    F2MinusF1>>.ScheduleParallel,
			NoiseJob<Voronoi2D<LatticeNormal,  Chebychev, F2MinusF1>>.ScheduleParallel,
			NoiseJob<Voronoi2D<LatticeTilling, Chebychev, F2MinusF1>>.ScheduleParallel,
			NoiseJob<Voronoi3D<LatticeNormal,  Chebychev, F2MinusF1>>.ScheduleParallel,
			NoiseJob<Voronoi3D<LatticeTilling, Chebychev, F2MinusF1>>.ScheduleParallel,
		},

		/* Voronoi-Manhattan Noise (rook-like distance) */
		/* Note that 1D Manhattan and Worley    are the same, so to reduce compilation redundancy, we just
		use one of them everywhere */
		{
			NoiseJob<Voronoi1D<LatticeNormal,  Worley,    F1>>.ScheduleParallel,
			NoiseJob<Voronoi1D<LatticeTilling, Worley,    F1>>.ScheduleParallel,
			NoiseJob<Voronoi2D<LatticeNormal,  Manhattan, F1>>.ScheduleParallel,
			NoiseJob<Voronoi2D<LatticeTilling, Manhattan, F1>>.ScheduleParallel,
			NoiseJob<Voronoi3D<LatticeNormal,  Manhattan, F1>>.ScheduleParallel,
			NoiseJob<Voronoi3D<LatticeTilling, Manhattan, F1>>.ScheduleParallel,
		},
		{
			NoiseJob<Voronoi1D<LatticeNormal,  Worley,    F2>>.ScheduleParallel,
			NoiseJob<Voronoi1D<LatticeTilling, Worley,    F2>>.ScheduleParallel,
			NoiseJob<Voronoi2D<LatticeNormal,  Manhattan, F2>>.ScheduleParallel,
			NoiseJob<Voronoi2D<LatticeTilling, Manhattan, F2>>.ScheduleParallel,
			NoiseJob<Voronoi3D<LatticeNormal,  Manhattan, F2>>.ScheduleParallel,
			NoiseJob<Voronoi3D<LatticeTilling, Manhattan, F2>>.ScheduleParallel,
		},
		{
			NoiseJob<Voronoi1D<LatticeNormal,  Worley,    Cell>>.ScheduleParallel,
			NoiseJob<Voronoi1D<LatticeTilling, Worley,    Cell>>.ScheduleParallel,
			NoiseJob<Voronoi2D<LatticeNormal,  Manhattan, Cell>>.ScheduleParallel,
			NoiseJob<Voronoi2D<LatticeTilling, Manhattan, Cell>>.ScheduleParallel,
			NoiseJob<Voronoi3D<LatticeNormal,  Manhattan, Cell>>.ScheduleParallel,
			NoiseJob<Voronoi3D<LatticeTilling, Manhattan, Cell>>.ScheduleParallel,
		},
		{
			NoiseJob<Voronoi1D<LatticeNormal,  Worley,    F2MinusF1>>.ScheduleParallel,
			NoiseJob<Voronoi1D<LatticeTilling, Worley,    F2MinusF1>>.ScheduleParallel,
			NoiseJob<Voronoi2D<LatticeNormal,  Manhattan, F2MinusF1>>.ScheduleParallel,
			NoiseJob<Voronoi2D<LatticeTilling, Manhattan, F2MinusF1>>.ScheduleParallel,
			NoiseJob<Voronoi3D<LatticeNormal,  Manhattan, F2MinusF1>>.ScheduleParallel,
			NoiseJob<Voronoi3D<LatticeTilling, Manhattan, F2MinusF1>>.ScheduleParallel,
		},
	}; 

	static readonly int noiseId = Shader.PropertyToID("_Noise");

	[SerializeField] Noises noiseSelector;
	[SerializeField] NoiseType type;
	[SerializeField] bool tiling = false;
	[SerializeField] Settings noiseSettings = Settings.Default;
	[SerializeField] SpaceTRS domain = new SpaceTRS { scale = 1f };

    NativeArray<float4> noise;
    ComputeBuffer noiseBuffer;
	MaterialPropertyBlock propertyBlock;

	protected override void EnableVisualization(int dataLength, MaterialPropertyBlock propertyBlock){
		noise = new NativeArray<float4>(dataLength, Allocator.Persistent);
		noiseBuffer = new ComputeBuffer(dataLength*4, sizeof(float));

		propertyBlock.SetBuffer(noiseId, noiseBuffer);
	}

	protected override void DisableVisualization(){
		noise.Dispose();
		noiseBuffer.Release();
		noiseBuffer = null;
	}

	protected override void UpdateVisualization(NativeArray<float3x4> positions, int resolution, Unity.Jobs.JobHandle job){
		var noiseDimension = 2 * (int)noiseSelector + (tiling ? 1 : 0);
		var noiseDelegate = noiseJobs[(int) type, noiseDimension];
		noiseDelegate(positions, noise, domain, resolution, noiseSettings, job).Complete();
		noiseBuffer.SetData(noise.Reinterpret<float>(4 * sizeof(float)));
	}
}}
