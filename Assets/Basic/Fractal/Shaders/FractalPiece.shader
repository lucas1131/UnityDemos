Shader "Custom/Fractal/Piece"{

	SubShader {
		CGPROGRAM

		#pragma surface surf Standard fullforwardshadows addshadow
		#pragma instancing_options assumeuniformscaling procedural:procedural_instancing
		#pragma editor_sync_compilation // useful for procedural instancing
		#pragma target 4.5 // minimum shader target level

		#include "FractalPiece.hlsl"

		struct Input {
			float3 worldPos; 
		};

		void surf (Input input, inout SurfaceOutputStandard surface){
			surface.Albedo = GetFractalColor().rgb;
			surface.Smoothness = GetFractalColor().a;
		}

		ENDCG
	}

	FallBack "Diffuse"
}
