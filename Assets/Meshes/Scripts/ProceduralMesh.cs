using Meshes.ProceduralMeshes.Generators;
using Meshes.ProceduralMeshes.Streams;
using UnityEngine;
using UnityEngine.Rendering;

using Meshes.ProceduralMeshes;

namespace Meshes {

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralMesh : MonoBehaviour {

	Mesh mesh;

	void Awake(){
		mesh = new Mesh{ name = $"Procedural mesh ({name})" };
		GenerateMesh();
		GetComponent<MeshFilter>().mesh = mesh;
	}

	void GenerateMesh(){

	}
}}
