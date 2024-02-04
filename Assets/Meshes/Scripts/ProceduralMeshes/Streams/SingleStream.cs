using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

using static Unity.Mathematics.math;

namespace Meshes.ProceduralMeshes.Streams {

public struct SingleStream : IMeshStream {

    [StructLayout(LayoutKind.Sequential)]
    struct Stream0 {
        public float3 position;
        public float3 normal;
        public float4 tangent;
        public float2 texCoord0;
    }

    NativeArray<Stream0> stream0;
    NativeArray<int3> triangles;

    public void Setup(Mesh.MeshData meshData, int vertexCount, int indexCount) {
        /* Vertex att stream:
            An array with attributes for 4 vertices in the follwing format
            PNTX PNTX PNTX PNTX
            where
                - P is the vertex position
                - N is the vertex normal
                - T is the vertex tangent
                - X is the vertex texture coordinates (uv coordinates)
        */
        stream0 = meshData.GetVertexData<Stream0>();
        triangles = meshData.GetVertexData<int>().Reinterpret<int3>(sizeof(int));

        int vertexAttributesCount = 4;
        SetupVertexAttributes(meshData, vertexAttributesCount, vertexCount, VertexAttributeFormat.Float16);
        SetupVertices(meshData);

        int triangleIndexCount = 6;
        SetupTriangles(meshData, triangleIndexCount);

        meshData.subMeshCount = 1;
        var bounds = new Bounds(new Vector3(0.5f, 0.5f), new Vector3(1f, 1f));
        SetupSubMesh(meshData, 0, 0, triangleIndexCount, bounds, vertexCount);
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

    void SetupVertices(Mesh.MeshData meshData){
        half h0 = half(0f);
        half h1 = half(1f);
        NativeArray<Vertex> vertices = meshData.GetVertexData<Vertex>(0);
        Vertex template = new Vertex {
            normal = back(),
            tangent = half4(h1, h0, h0, half(-1f)),
        };

        // Struct is value type so this is passing a copy of our template
        template.position = 0f;
        template.texCoord0 = h0;
        vertices[0] = template;

        template.position = right();
        template.texCoord0 = half2(h1, h0);
        vertices[1] = template;

        template.position = up();
        template.texCoord0 = half2(h0, h1);
        vertices[2] = template;

        template.position = float3(1f, 1f, 0f);
        template.texCoord0 = h1;
        vertices[3] = template;
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
#endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetVertex(int index, Vertex vertex) => new Stream0 {
        position = vertex.position,
        normal = vertex.normal,
        tangent = vertex.tangent,
        texCoord0 = vertex.texCoord0,
    };

    public void SetTriangle(int index, int3 triangle) => triangles[index] = triangle;
}}
