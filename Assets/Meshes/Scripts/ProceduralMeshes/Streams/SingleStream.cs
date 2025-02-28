using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

using static Unity.Mathematics.math;

namespace Meshes.ProceduralMeshes.Streams {

public struct SingleStream : IMeshStream {

    [StructLayout(LayoutKind.Sequential)]
    public struct Stream0 {
        public float3 position;
        public float3 normal;
        public float4 tangent;
        public float2 texCoord0;
    }

    [NativeDisableContainerSafetyRestriction]
    NativeArray<Stream0> stream0;
    [NativeDisableContainerSafetyRestriction]
    NativeArray<TriangleUInt16> triangles;

    public void Setup(Mesh.MeshData meshData, Bounds bounds, int vertexCount, int indexCount) {
        /* Vertex att stream:
            An array with attributes for 4 vertices in the follwing format
            PNTX PNTX PNTX PNTX
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

        stream0 = meshData.GetVertexData<Stream0>();
        triangles = meshData.GetIndexData<ushort>().Reinterpret<TriangleUInt16>(sizeof(ushort));
    }

#region Setup helpers
    void SetupVertexAttributes(Mesh.MeshData meshData, int vertexAttributesCount, int vertexCount, VertexAttributeFormat format=VertexAttributeFormat.Float32){
        var attributes = new NativeArray<VertexAttributeDescriptor>(
            vertexAttributesCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        attributes[0] = new VertexAttributeDescriptor(dimension: 3);
        attributes[1] = new VertexAttributeDescriptor(
            VertexAttribute.Normal, dimension: 3);
        attributes[2] = new VertexAttributeDescriptor(
            VertexAttribute.Tangent, format, dimension: 4);
        attributes[3] = new VertexAttributeDescriptor(
            VertexAttribute.TexCoord0, format, dimension: 2);

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
    public void SetVertex(int index, Vertex vertex) => stream0[index] = new Stream0 {
        position = vertex.position,
        normal = vertex.normal,
        tangent = vertex.tangent,
        texCoord0 = vertex.texCoord0,
    };

    public void SetTriangle(int index, int3 triangle) => triangles[index] = triangle;
}}
