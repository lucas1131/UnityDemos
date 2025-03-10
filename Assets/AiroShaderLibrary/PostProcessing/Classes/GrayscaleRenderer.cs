using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

/* Following basic tutorial from unity's package 
https://docs.unity3d.com/Packages/com.unity.postprocessing@3.0/manual/Writing-Custom-Effects.html
*/

namespace AiroShaderLibrary {
	public sealed class GrayscaleRenderer : PostProcessEffectRenderer<Grayscale>
	{
		public override void Render(PostProcessRenderContext context)
		{
			// Request a PropertySheet for our shader and set the uniform within it.
			PropertySheet sheet = context.propertySheets.Get(Shader.Find("AiroLibrary/PostProcess/Grayscale"));
			sheet.properties.SetFloat("_Blend", settings.blend);
			context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
		}
	}
}
