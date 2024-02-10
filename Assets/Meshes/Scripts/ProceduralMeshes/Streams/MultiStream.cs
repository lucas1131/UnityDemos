using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

using static Unity.Mathematics.math;

namespace Meshes.ProceduralMeshes.Streams {

public struct MultiStream : IMeshStream {

    [NativeDisableContainerSafetyRestriction]
    NativeArray<float3> positions;
    [NativeDisableContainerSafetyRestriction]
    NativeArray<float3> normals;
    [NativeDisableContainerSafetyRestriction]
    NativeArray<float4> tangents;
    [NativeDisableContainerSafetyRestriction]
    NativeArray<float2> texCoords0;
    [NativeDisableContainerSafetyRestriction]
    NativeArray<TriangleUInt16> triangles;

    public void Setup(Mesh.MeshData meshData, Bounds bounds, int vertexCount, int indexCount) {
        /* Vertex att stream:
            An array with attributes for 4 vertices in the follwing format
            PPPP NNNN TTTT XXXX
            where
                - P is the vertex position
                - N is the vertex normal
                - T is the vertex tangent
                - X is the vertex texture coordinates (uv coordinates)
        */
        int vertexAttributesCount = 4;
        SetupVertexAttributes(meshData, vertexAttributesCount, vertexCount);

        meshData.SetIndexBufferParams(indexCount, IndexFormat.UInt16);

        meshData.subMeshCount = 1;
        SetupSubMesh(meshData, 0, 0, indexCount, bounds, vertexCount);

        positions = meshData.GetVertexData<float3>(0);
        normals = meshData.GetVertexData<float3>(1);
        tangents = meshData.GetVertexData<float4>(2);
        texCoords0 = meshData.GetVertexData<float2>(3);
        triangles = meshData.GetIndexData<ushort>().Reinterpret<TriangleUInt16>(sizeof(ushort));
    }

#region Setup helpers
    void SetupVertexAttributes(Mesh.MeshData meshData, int vertexAttributesCount, int vertexCount, VertexAttributeFormat format=VertexAttributeFormat.Float32){
        var attributes = new NativeArray<VertexAttributeDescriptor>(
            vertexAttributesCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        attributes[0] = new VertexAttributeDescriptor(dimension: 3, stream: 0);
        attributes[1] = new VertexAttributeDescriptor(
            VertexAttribute.Normal, dimension: 3, stream: 1);
        attributes[2] = new VertexAttributeDescriptor(
            VertexAttribute.Tangent, format, dimension: 4, stream: 2);
        attributes[3] = new VertexAttributeDescriptor(
            VertexAttribute.TexCoord0, format, dimension: 2, stream: 3);

        meshData.SetVertexBufferParams(vertexCount, attributes);
        attributes.Dispose();
    }

    void SetupSubMesh(Mesh.MeshData meshData, int subMeshIndex, int start, int indexCount, Bounds bounds, int vertexCount){
        var descriptor = new SubMeshDescriptor(start, indexCount){
            bounds = bounds,
            vertexCount = vertexCount,
        };

        meshData.SetSubMesh(
            subMeshIndex,
            descriptor,
            MeshUpdateFlags.DontRecalculateBounds |
            MeshUpdateFlags.DontValidateIndices
        );
    }
#endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetVertex(int index, Vertex vertex) {
        positions[index] = vertex.position;
        normals[index] = vertex.normal;
        tangents[index] = vertex.tangent;
        texCoords0[index] = vertex.texCoord0;
    }

    public void SetTriangle(int index, int3 triangle) => triangles[index] = triangle;
}}
