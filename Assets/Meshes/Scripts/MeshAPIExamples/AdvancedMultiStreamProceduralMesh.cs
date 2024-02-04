using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

using static Unity.Mathematics.math;

namespace Meshes {

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class AdvancedMultiStreamProceduralMesh : MonoBehaviour {

	void OnEnable(){
		Vector3 pivot = new Vector3(0.5f, 0.5f);
		var bounds = new Bounds(pivot + transform.position, transform.lossyScale);
		Mesh mesh = new Mesh {
			name = name,
			bounds = bounds,
		};

		Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
		Mesh.MeshData meshData = meshDataArray[0];

		/* Vertex att stream:
			An array with attributes for 4 vertices in the follwing format
			PPPP NNNN TTTT XXXX
			where
				- P is the vertex position
				- N is the vertex normal
				- T is the vertex tangent
				- X is the vertex texture coordinates (uv coordinates)
		*/
		int vertexCount = 4;
		int vertexAttributesCount = 4;
		SetupVertexAttributes(meshData, vertexAttributesCount, vertexCount, VertexAttributeFormat.Float16);
		SetupVertices(meshData);

		int triangleIndexCount = 6;
		SetupTriangles(meshData, triangleIndexCount);

		meshData.subMeshCount = 1;
		SetupSubMesh(meshData, 0, 0, triangleIndexCount, bounds, vertexCount);

		Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
		GetComponent<MeshFilter>().mesh = mesh;
	}

	void SetupVertexAttributes(Mesh.MeshData meshData, int vertexAttributesCount, int vertexCount, VertexAttributeFormat format=VertexAttributeFormat.Float32){
		var attributes = new NativeArray<VertexAttributeDescriptor>(
			vertexAttributesCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

		// Vertex size must be a multiple of 4 bytes, so we can only use halfs for tangent and uv here
		// If we want to use half for normals and positions we need to use 4 dimensions
		// (float16 = 2 bytes * 4 dimensions = 8 bytes). This may help with performance for the gpu
		// as there is less data to transfer but we also need to do a bit more work and will have some
		// unused bytes
		attributes[0] = new VertexAttributeDescriptor(dimension: 3);
		attributes[1] = new VertexAttributeDescriptor(
			VertexAttribute.Normal, dimension: 3, stream: 1);
		attributes[2] = new VertexAttributeDescriptor(
			VertexAttribute.Tangent, format, dimension: 4, stream: 2);
		attributes[3] = new VertexAttributeDescriptor(
			VertexAttribute.TexCoord0, format, dimension: 2, stream: 3);

		meshData.SetVertexBufferParams(vertexCount, attributes);
		attributes.Dispose();
	}

	void SetupVertices(Mesh.MeshData meshData){
		half h0 = half(0f);
		half h1 = half(1f);
		NativeArray<float3> positions = meshData.GetVertexData<float3>();
		positions[0] = 0f + float3(transform.position);
		positions[1] = right() + float3(transform.position);
		positions[2] = up() + float3(transform.position);
		positions[3] = float3(1f, 1f, 0f) + float3(transform.position);

		NativeArray<float3> normals = meshData.GetVertexData<float3>(1);
		normals[0] = normals[1] = normals[2] = normals[3] = back();

		NativeArray<half4> tangents = meshData.GetVertexData<half4>(2);
		tangents[0] = tangents[1] = tangents[2] = tangents[3] = half4(h1, h0, h0, half(-1f));

		NativeArray<half2> texCoords = meshData.GetVertexData<half2>(3);
		texCoords[0] = h0;
		texCoords[1] = half2(h1, h0);
		texCoords[2] = half2(h0, h1);
		texCoords[3] = h1;
	}

	void SetupTriangles(Mesh.MeshData meshData, int triangleIndexCount){
		meshData.SetIndexBufferParams(triangleIndexCount, IndexFormat.UInt16);
		NativeArray<ushort> triangleIndices = meshData.GetIndexData<ushort>();
		triangleIndices[0] = 0;
		triangleIndices[1] = 2;
		triangleIndices[2] = 1;
		triangleIndices[3] = 1;
		triangleIndices[4] = 2;
		triangleIndices[5] = 3;
	}

	void SetupSubMesh(Mesh.MeshData meshData, int subMeshIndex, int start, int indexCount, Bounds bounds, int vertexCount){
		var descriptor = new SubMeshDescriptor(start, indexCount){
			bounds = bounds,
			vertexCount = vertexCount,
		};

		meshData.SetSubMesh(subMeshIndex, descriptor, MeshUpdateFlags.DontRecalculateBounds);
	}
}}
