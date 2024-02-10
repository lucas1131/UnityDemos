using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace Meshes.ProceduralMeshes.Streams {
[StructLayout(LayoutKind.Sequential)]
public struct TriangleUInt16 {
    public ushort a;
    public ushort b;
    public ushort c;

    public static implicit operator TriangleUInt16(int3 val) => new TriangleUInt16 {
        a = (ushort) val.x,
        b = (ushort) val.y,
        c = (ushort) val.z
    };
}}
