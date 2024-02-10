using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

using static Unity.Mathematics.math;

namespace Meshes.ProceduralMeshes.Streams {
public interface IMeshStream {
	void Setup(Mesh.MeshData meshData, Bounds bounds, int vertexCount, int indexCount);
	void SetVertex(int index, Vertex vertex);
	void SetTriangle(int index, int3 triangle);
}}
