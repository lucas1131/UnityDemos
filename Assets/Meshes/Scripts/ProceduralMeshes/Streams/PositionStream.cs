using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

using static Unity.Mathematics.math;

namespace Meshes.ProceduralMeshes.Streams {

public struct PositionStream : IMeshStream {

    [NativeDisableContainerSafetyRestriction]
    NativeArray<float3> stream0;
    [NativeDisableContainerSafetyRestriction]
    NativeArray<TriangleUInt16> triangles;

    public void Setup(Mesh.MeshData meshData, Bounds bounds, int vertexCount, int indexCount) {
        int vertexAttributesCount = 1;
        SetupVertexAttributes(meshData, vertexAttributesCount, vertexCount);

        meshData.SetIndexBufferParams(indexCount, IndexFormat.UInt16);

        meshData.subMeshCount = 1;
        SetupSubMesh(meshData, 0, 0, indexCount, bounds, vertexCount);

        stream0 = meshData.GetVertexData<float3>();
        triangles = meshData.GetIndexData<ushort>().Reinterpret<TriangleUInt16>(sizeof(ushort));
    }

#region Setup helpers
    void SetupVertexAttributes(Mesh.MeshData meshData, int vertexAttributesCount, int vertexCount, VertexAttributeFormat format=VertexAttributeFormat.Float32){
        var attributes = new NativeArray<VertexAttributeDescriptor>(
            vertexAttributesCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        attributes[0] = new VertexAttributeDescriptor(dimension: 3);
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
    public void SetVertex(int index, Vertex vertex) => stream0[index] = vertex.position;
    public void SetTriangle(int index, int3 triangle) => triangles[index] = triangle;
}}
