using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

/* Following basic tutorial from unity's package 
https://docs.unity3d.com/Packages/com.unity.postprocessing@3.0/manual/Writing-Custom-Effects.html
*/

namespace AiroShaderLibrary {
	// The [PostProcess()] attribute tells Unity that this class holds post-processing data. 
	// The first parameter links the settings to a renderer. 
	// The second parameter creates the injection point for the effect. 
	// The third parameter is the menu entry for the effect. 
	[PostProcess(typeof(GrayscaleRenderer), PostProcessEvent.AfterStack, "AiroLibrary/PostProcess/Grayscale")]
	[Serializable]
	public sealed class Grayscale : PostProcessEffectSettings
	{
		[Range(0f, 1f), Tooltip("Grayscale effect intensity.")]
		public FloatParameter blend = new FloatParameter { value = 0.5f };
	}
}
