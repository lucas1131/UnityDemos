Shader "Custom/Noise/HashVisualizer"{
	SubShader {

		CGPROGRAM

		#pragma surface surf Standard fullforwardshadows addshadow
		#pragma instancing_options assumeuniformscaling procedural:procedural_instancing
		#pragma editor_sync_compilation // useful for procedural instancing
		#pragma target 4.5 // minimum shader target level

		#include "HashVisualizer.hlsl"

		struct Input {
			float3 worldPos;
		};

		void surf (Input input, inout SurfaceOutputStandard surface){
			surface.Albedo = GetColor();
			surface.Smoothness = 0.8;
		}

		ENDCG
	}

	FallBack "Diffuse"
}
