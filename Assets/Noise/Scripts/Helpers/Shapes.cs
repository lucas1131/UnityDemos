using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace Noise {
public class Shapes {

	public delegate JobHandle ScheduleDelegate(NativeArray<float3x4> positions, NativeArray<float3x4> normals, int resolution, float4x4 trs, JobHandle dependsOn);

	public static readonly float4x3 up = float4x3(0f, 1f, 0f);
	public static float4x2 IndexToUV4(int t, float resolution, float invResolution){
		float4 idx4 = 4f*t + float4(0f, 1f, 2f, 3f);
		float bias = 0.00001f;
		float4x2 uv;

		uv.c1 = floor(invResolution * idx4 + bias);
		uv.c0 = (idx4 - uv.c1 * resolution + 0.5f)*invResolution;
		uv.c1 = invResolution * (uv.c1 + 0.5f);

		return uv;
	}


	public interface IShape {
		public Point4x3 GetPoint(int t, float resolution, float invResolution);
	}


	public struct Point4x3 {
		// dont forget that matrices here are transposed already
		public float4x3 positions;
		public float4x3 normals;
	}


	public struct Plane : IShape {
		public Point4x3 GetPoint(int t, float resolution, float invResolution){
			float4x2 uv = IndexToUV4(t, resolution, invResolution);
			return new Point4x3 {
				positions = float4x3(uv.c0 - 0.5f, 0f, uv.c1 - 0.5f),
				normals = up,
			};
		}
	}

	public struct UVSphere : IShape {
		public Point4x3 GetPoint(int t, float resolution, float invResolution){
			float4x2 uv = IndexToUV4(t, resolution, invResolution);

			float r = 0.5f;
			float4 s = r * sin(PI * uv.c1);

			Point4x3 point;
			point.positions.c0 = s * sin(2f * PI * uv.c0);
			point.positions.c1 = r * cos(PI * uv.c1);
			point.positions.c2 = s * cos(2f * PI * uv.c0);

			point.normals = point.positions;
			return point;
		}
	}

	public struct OctahedronSphere : IShape {
		public Point4x3 GetPoint(int t, float resolution, float invResolution){
			float4x2 uv = IndexToUV4(t, resolution, invResolution);


			Point4x3 point;
			point.positions.c0 = uv.c0 - 0.5f;
			point.positions.c1 = uv.c1 - 0.5f;
			point.positions.c2 = 0.5f - abs(point.positions.c0) - abs(point.positions.c1);
			float4 offset = max(-point.positions.c2, 0f);
			point.positions.c0 += select(-offset, offset, point.positions.c0 < 0f);
			point.positions.c1 += select(-offset, offset, point.positions.c1 < 0f);

			float4 scale = 0.5f * rsqrt(
				point.positions.c0 * point.positions.c0 +
				point.positions.c1 * point.positions.c1 +
				point.positions.c2 * point.positions.c2
			);

			point.positions.c0 *= scale;
			point.positions.c1 *= scale;
			point.positions.c2 *= scale;
			point.normals = point.positions;
			return point;
		}
	}

	public struct Torus : IShape {
		public Point4x3 GetPoint(int t, float resolution, float invResolution){
			float4x2 uv = IndexToUV4(t, resolution, invResolution);

			float r1 = 0.375f;
			float r2 = 0.125f;
			float4 s = r1 + r2 * cos(2f * PI * uv.c1);

			Point4x3 point;
			point.positions.c0 = s  * sin(2f * PI * uv.c0);
			point.positions.c1 = r2 * sin(2f * PI * uv.c1);
			point.positions.c2 = s  * cos(2f * PI * uv.c0);

			point.normals = point.positions;
			point.normals.c0 -= r1 * sin(2f * PI * uv.c0);
			point.normals.c2 -= r1 * cos(2f * PI * uv.c0);
			return point;
		}
	}


	[BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously=true)]
	public struct ShapeJob<S> : IJobFor where S : struct, IShape {

		[WriteOnly] NativeArray<float3x4> positions;
		[WriteOnly] NativeArray<float3x4> normals;
		public float resolution;
		public float invResolution;
		public float3x4 positionTRS;
		public float3x4 normalTRS;

		public void Execute(int idx){
			Point4x3 point = default(S).GetPoint(idx, resolution, invResolution);
    		float4x3 pointTransform = float4x3(point.positions.c0, point.positions.c1, point.positions.c2);
			positions[idx] = transpose(positionTRS.TransformVectors(pointTransform, 1f));

			float4x3 pointNormal = float4x3(point.normals.c0, point.normals.c1, point.normals.c2);
			float3x4 normal4 = transpose(normalTRS.TransformVectors(pointNormal, 0f));
			normals[idx] = float3x4(
				normalize(normal4.c0),
				normalize(normal4.c1),
				normalize(normal4.c2),
				normalize(normal4.c3)
			);
		}

		public static JobHandle ScheduleParallel(
			NativeArray<float3x4> positions, 
			NativeArray<float3x4> normals, 
			int resolution, 
			float4x4 trs, 
			JobHandle dependsOn) => new ShapeJob<S> {
				positions = positions,
				resolution = resolution,
				invResolution = 1f / resolution,
				positionTRS = trs.Get3x4(),
				normalTRS = transpose(inverse(trs)).Get3x4(), // Transpose-Inverse-Matrix from the TRS matrix
				normals = normals,
			}.ScheduleParallel(positions.Length, resolution, dependsOn);
	}
}}
