using UnityEngine;

namespace Shared {
public static class GPUDrawPrimitive {
	public static void DrawPrimitive(Mesh mesh, Material material, MaterialPropertyBlock mPropertyBlock, Vector3 center, Vector3 size, int count){
		RenderParams rp = new RenderParams(material);
		rp.worldBounds = new Bounds(center, size);
        rp.matProps = mPropertyBlock;
	    Graphics.RenderMeshPrimitives(rp, mesh, 0, count);
	}
}}
