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
		MeshJob<SharedSquareGrid, SingleStream>.ScheduleParallel
	};

	public enum MeshType { SquareGrid, SharedSquareGrid };

	[SerializeField, Range(1, 100)] int resolution = 1;
	[SerializeField] MeshType meshType;

	Mesh mesh;

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

		if(enabled && mesh != null){
			GenerateMesh();
		}
	}

	void GenerateMesh(){
		Mesh.MeshDataArray dataArray = Mesh.AllocateWritableMeshData(1);
		Mesh.MeshData meshData = dataArray[0];

		jobs[(int) meshType](mesh, meshData, resolution, default).Complete();

		Mesh.ApplyAndDisposeWritableMeshData(dataArray, mesh);
	}
}}
