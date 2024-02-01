Shader "Custom/Graph/Point GPU"{

	Properties {
		_Smoothness ("Smoothness", Range(0, 1)) = 0.5
	}
	
	SubShader {
		CGPROGRAM

		#pragma surface surf Standard fullforwardshadows addshadow
		#pragma instancing_options assumeuniformscaling procedural:procedural_instancing
		#pragma editor_sync_compilation // useful for procedural instancing
		#pragma target 4.5 // minimum shader target level

		#include "GraphPointGPU.hlsl"

		float _Smoothness;

		struct Input {
			float3 worldPos; 
		};

		void surf (Input input, inout SurfaceOutputStandard surface){
			surface.Smoothness = _Smoothness;
			input.worldPos.y -= 10.5;
			surface.Albedo = saturate(input.worldPos*0.07 + 0.3);
		}

		ENDCG
	}

	FallBack "Diffuse"
}
