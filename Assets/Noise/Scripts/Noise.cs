using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace Noise {

public static partial class Noise {
	
	public delegate JobHandle ScheduleDelegate(NativeArray<float3x4> positions, NativeArray<float4> noise, SpaceTRS domain, int resolution, Settings noiseSettings, JobHandle dependsOn);
	
	public interface INoise {
		float4 GetNoise4(float4x3 positions, SmallXXHash4 hash, int frequency);
	}

	[System.Serializable]
	public struct Settings {
		public uint seed;

		[Min(1)] public int frequency;
		[Tooltip("Frequency scaling for each octave")]
		[Range(2, 4)] public int lacunarity;
		[Range(1, 6)] public int octaves;
		[Range(0.1f, 1f)] public float persistence;

		public static Settings Default => new Settings {
			frequency = 4,
			lacunarity = 2,
			octaves = 1,
			persistence = 0.5f,
		};
	}


	[BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously=true)]
	public struct NoiseJob<N> : IJobFor where N : struct, INoise {

		[ReadOnly] public NativeArray<float3x4> positions;
		[WriteOnly] public NativeArray<float4> noise;
		Settings settings;
		public float3x4 domain;

		public void Execute(int idx){
			SmallXXHash4 hash = SmallXXHash.Seed(settings.seed);
			float4x3 position = domain.TransformVectors(transpose(positions[idx]));
			int frequency = settings.frequency;
			float amplitude = 1f;
			float amplitudeSum = 0f;
			float4 sum = 0f;

			for (int i = 0; i < settings.octaves; i++) {
				sum += default(N).GetNoise4(position, hash+i, frequency) * amplitude;
				amplitudeSum += amplitude;
				frequency *= settings.lacunarity;
				amplitude *= settings.persistence;
			}

			noise[idx] = sum/amplitudeSum;
		}

		public static JobHandle ScheduleParallel(
			NativeArray<float3x4> positions, 
			NativeArray<float4> noise,
			SpaceTRS domain,
			int resolution,
			Settings settings, 
			JobHandle dependsOn) => new NoiseJob<N> {
				positions = positions,
				noise = noise,
				settings = settings,
				domain = domain.Matrix,
			}.ScheduleParallel(positions.Length, resolution, dependsOn);
	}
}}
