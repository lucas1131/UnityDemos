using Meshes.ProceduralMeshes.Streams;

using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

using static Unity.Mathematics.math;

namespace Meshes.ProceduralMeshes.Generators {
public interface IMeshGenerator {

	int Resolution { get; set; }
	int VertexCount { get; }
	int IndexCount { get; }
	int JobLength { get; }
	Bounds Bounds { get; }

	void Execute<S>(int index, S stream) where S : struct, IMeshStream;
}}
