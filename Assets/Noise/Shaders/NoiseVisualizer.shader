Shader "Custom/Noise/NoiseVisualizer"{

	SubShader {
		CGPROGRAM

		#pragma surface surf Standard fullforwardshadows addshadow
		#pragma instancing_options assumeuniformscaling procedural:procedural_instancing
		#pragma editor_sync_compilation // useful for procedural instancing
		#pragma target 4.5 // minimum shader target level

		#include "NoiseVisualizer.hlsl"

		struct Input {
			float3 worldPos;
		};

		void surf (Input input, inout SurfaceOutputStandard surface){
			surface.Albedo = GetColor();
		}

		ENDCG
	}

	FallBack "Diffuse"
}
