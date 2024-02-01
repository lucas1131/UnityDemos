using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using static Shared.GPUDrawPrimitive;

namespace Noise {

public abstract class AbstractVisualization : MonoBehaviour {

	public enum Curves { Plane, UVSphere, OctahedronSphere, Torus }

	static Shapes.ScheduleDelegate[] shapeJobs = {
		Shapes.ShapeJob<Shapes.Plane>.ScheduleParallel,
		Shapes.ShapeJob<Shapes.UVSphere>.ScheduleParallel,
		Shapes.ShapeJob<Shapes.OctahedronSphere>.ScheduleParallel,
		Shapes.ShapeJob<Shapes.Torus>.ScheduleParallel
	};
	
	static readonly int configId = Shader.PropertyToID("_Config");
	static readonly int positionsId = Shader.PropertyToID("_Positions");
	static readonly int normalsId = Shader.PropertyToID("_Normals");
	
	[SerializeField] Mesh mesh;
	[SerializeField] Material material;
	[SerializeField, Range(1, 1024)] int resolution = 16;
	[SerializeField, Range(-5f, 5f)] float displacement = 1f;
	[SerializeField, Range(0.1f, 10f)] float instanceScale = 1f;
	[SerializeField] Curves curve;

	bool isDirty;
    NativeArray<float3x4> positions;
    NativeArray<float3x4> normals;
    ComputeBuffer positionsBuffer;
    ComputeBuffer normalsBuffer;
	MaterialPropertyBlock propertyBlock;
	float3 boundingBoxSize;

	protected abstract void EnableVisualization(int dataLength, MaterialPropertyBlock propertyBlock);
	protected abstract void DisableVisualization();
	protected abstract void UpdateVisualization(NativeArray<float3x4> positions, int resolution, JobHandle job);

	void OnEnable(){
		isDirty = true;

		// Add 1 extra element when resolution is an odd value - this gives us +4 elements
		// so we can support resolutions not multiple of 4, with up to 3 floats of overhead
		int length = resolution*resolution;
		length = length/4 + (length & 1);
		float invResolution = 1f / resolution;
		positions = new NativeArray<float3x4>(length, Allocator.Persistent);
		normals = new NativeArray<float3x4>(length, Allocator.Persistent);

		positionsBuffer = new ComputeBuffer(length*4, sizeof(float)*3); // wtf you mean float3 has no predefined size, its 3 times a float size what??
		normalsBuffer = new ComputeBuffer(length*4, sizeof(float)*3);

		propertyBlock ??= new MaterialPropertyBlock();
		propertyBlock.SetBuffer(positionsId, positionsBuffer);
		propertyBlock.SetBuffer(normalsId, normalsBuffer);
		propertyBlock.SetVector(configId, new Vector4(resolution, instanceScale/resolution, displacement));
		EnableVisualization(length, propertyBlock);
	}

	void OnDisable(){
		positions.Dispose();
		positionsBuffer.Release();
		positionsBuffer = null;

		normals.Dispose();
		normalsBuffer.Release();
		normalsBuffer = null;

		DisableVisualization();
	}

	void OnValidate(){
		if(positionsBuffer != null && enabled){
			OnDisable();
			OnEnable();
		}
	}

	void Update(){
		if(isDirty || transform.hasChanged){
			RecalculateVisualization();
		}
		
		DrawPrimitive(mesh, material, propertyBlock, transform.position, boundingBoxSize, resolution*resolution);
	}

	void RecalculateVisualization(){
		isDirty = false;
		transform.hasChanged = false;

		var shapeJob = shapeJobs[(int) curve](positions, normals, resolution, transform.localToWorldMatrix, default);
		UpdateVisualization(positions, resolution, shapeJob);

		positionsBuffer.SetData(positions.Reinterpret<float3>(4 * 3 * sizeof(float)));
		normalsBuffer.SetData(normals.Reinterpret<float3>(4 * 3 * sizeof(float)));

		boundingBoxSize = float3(2f * cmax(abs(transform.lossyScale)) + displacement);
	}
}}
