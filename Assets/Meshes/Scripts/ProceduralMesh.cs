using Meshes.ProceduralMeshes.Generators;
using Meshes.ProceduralMeshes.Streams;
using UnityEngine;
using UnityEngine.Rendering;

using Meshes.ProceduralMeshes;

namespace Meshes {

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralMesh : MonoBehaviour {

	static MeshJobScheduleDelegate[] jobs = {
		MeshJob<SquareGrid, SingleStream>.ScheduleParallel,
		MeshJob<SharedSquareGrid, SingleStream>.ScheduleParallel,
		MeshJob<SharedTriangleGrid, SingleStream>.ScheduleParallel,
		MeshJob<PointyHexagonGrid, SingleStream>.ScheduleParallel,
		MeshJob<FlatHexagonGrid, SingleStream>.ScheduleParallel,
		MeshJob<UVSphere, SingleStream>.ScheduleParallel,
		MeshJob<CubeSphere, SingleStream>.ScheduleParallel,
		MeshJob<SharedCubeSphere, PositionStream>.ScheduleParallel,
		MeshJob<OctaSphere, PositionStream>.ScheduleParallel,
	};

	public enum MeshType {
		SquareGrid,
		SharedSquareGrid,
		SharedTriangleGrid,
		PointyHexagonGrid,
		FlatHexagonGrid,
		UVSphere,
		CubeSphere,
		SharedCubeSphere,
		OctaSphere,
	};

	public enum MaterialMode { Grid, LatLon, Cube }

	[System.Flags]
	public enum GizmoMode {
		Nothing   = 0b0000,
		Vertices  = 0b0001,
		Normals   = 0b0010,
		Tangents  = 0b0100,
		Triangles = 0b1000,
	}

	[System.Flags]
	public enum MeshOptimizationMode {
		Nothing         = 0b00,
		ReorderIndices  = 0b01,
		ReorderVertices = 0b10,
	}


	static int rippleId = Shader.PropertyToID("_Ripple");
	static int speedId = Shader.PropertyToID("_Speed");
	static int amplitudeId = Shader.PropertyToID("_Amplitude");
	static int periodId = Shader.PropertyToID("_Period");

	[SerializeField, Range(1, 50)] int resolution = 1;
	[SerializeField] MeshType meshType;
	[SerializeField] MeshOptimizationMode meshOptimization;
	[SerializeField] GizmoMode gizmos;
	[SerializeField, Range(0.01f, 0.2f)] float gizmosScale = 0.02f;
	[SerializeField] MaterialMode material;
	[SerializeField] Material[] materials;

	[Header("Rippling")]
	[SerializeField] bool enableRippling = false;
	[SerializeField, Range(0f, 2f)] float speed = 1f;
	[SerializeField, Range(0f, 4f)] float amplitude = 0.2f;
	[SerializeField, Range(0f, 10f)] float period = 1f;

	Mesh mesh;

	[System.NonSerialized] Vector3[] vertices;
	[System.NonSerialized] Vector3[] normals;
	[System.NonSerialized] Vector4[] tangents;
	[System.NonSerialized] int[] triangles;

	void Awake(){
		mesh = new Mesh { name = $"Procedural mesh ({name})" };
		GetComponent<MeshFilter>().mesh = mesh;
		GenerateMesh();
	}

	void OnValidate(){
		#if UNITY_EDITOR
         if(!Application.isPlaying) {
             return;
         }
        #endif

		if(enabled){
			if(mesh != null){
				GenerateMesh();
			}

			vertices = null;
			normals = null;
			tangents = null;
			triangles = null;

			GetComponent<MeshRenderer>().material = materials[(int) material];
			UpdateShaderParams();
		}
	}

	void GenerateMesh(){
		Mesh.MeshDataArray dataArray = Mesh.AllocateWritableMeshData(1);
		Mesh.MeshData meshData = dataArray[0];

		jobs[(int) meshType](mesh, meshData, resolution, default).Complete();

		Mesh.ApplyAndDisposeWritableMeshData(dataArray, mesh);

		if (meshOptimization == MeshOptimizationMode.ReorderIndices) {
			mesh.OptimizeIndexBuffers();
		}
		else if (meshOptimization == MeshOptimizationMode.ReorderVertices) {
			mesh.OptimizeReorderVertexBuffer();
		}
		else if (meshOptimization != MeshOptimizationMode.Nothing) {
			mesh.Optimize();
		}
	}

	void UpdateShaderParams(){
		var material = GetComponent<MeshRenderer>().material;
		int enabled = enableRippling ? 1 : 0;
		material.SetInteger(rippleId, enabled);
		material.SetFloat(speedId, speed);
		material.SetFloat(amplitudeId, amplitude);
		material.SetFloat(periodId, period);
	}

	void OnDrawGizmos(){
		if (gizmos == GizmoMode.Nothing || mesh == null) {
			return;
		}

		bool drawVertices = (gizmos & GizmoMode.Vertices) != 0;
		bool drawNormals = (gizmos & GizmoMode.Normals) != 0;
		bool drawTangents = (gizmos & GizmoMode.Tangents) != 0;
		bool drawTriangles = (gizmos & GizmoMode.Triangles) != 0;

		if (vertices == null || enableRippling) {
			vertices = mesh.vertices;
		}

		if (drawNormals && normals == null) {
			drawNormals = mesh.HasVertexAttribute(VertexAttribute.Normal);
			if(drawNormals) {
				normals = mesh.normals;
			} else {
				Debug.LogWarning("Mesh has no 'Normal' attribute! Nothing will show up when drawing normals gizmos");
			}
		}

		if (drawTangents && tangents == null) {
			drawTangents = mesh.HasVertexAttribute(VertexAttribute.Tangent);
			if(drawTangents) {
				tangents = mesh.tangents;
			} else {
				Debug.LogWarning("Mesh has no 'Tangent' attribute! Nothing will show up when drawing tangents gizmos");
			}
		}

		if (drawTriangles && triangles == null) {
			triangles = mesh.triangles;
		}

		Transform t = transform;
		float scale = Unity.Mathematics.math.cmax(t.lossyScale);
		scale /= (resolution/10) + 1;
		scale *= gizmosScale;
		for (int i = 0; i < vertices.Length; i++) {
			Vector3 position = t.TransformPoint(vertices[i]);
			if (drawVertices) {
				Gizmos.color = Color.cyan;
				Gizmos.DrawSphere(position, scale);
			}
			if (drawNormals) {
				Gizmos.color = Color.green;
				Gizmos.DrawRay(position, t.TransformDirection(normals[i]) * 10f*scale);
			}
			if (drawTangents) {
				Gizmos.color = Color.red;
				Gizmos.DrawRay(position, t.TransformDirection(tangents[i]) * 10f*scale);
			}
		}

		if (drawTriangles) {
			float colorStep = 1f / (triangles.Length - 3);
			for (int i = 0; i < triangles.Length; i += 3) {
				float c = i*colorStep;
				Gizmos.color = new Color(c, 0f, c);

				Vector3 pos = vertices[triangles[i]] + vertices[triangles[i+1]] + vertices[triangles[i+2]];
				pos *= (1f / 3f);
				Gizmos.DrawSphere(t.TransformPoint(pos), scale);
			}
		}
	}
}}
