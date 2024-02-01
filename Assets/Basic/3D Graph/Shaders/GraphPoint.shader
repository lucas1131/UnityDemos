Shader "Custom/Graph/Point"{

	Properties {
		_Smoothness ("Smoothness", Range(0, 1)) = 0.5
	}
	
	SubShader {
		CGPROGRAM

		#pragma surface ConfigureSurface Standard fullforwardshadows
		#pragma target 3.0 // minimum shader target level quality

		float _Smoothness;

		struct Input {
			float3 worldPos; 
		};

		void ConfigureSurface (Input input, inout SurfaceOutputStandard surface){
			surface.Smoothness = _Smoothness;
			surface.Albedo = saturate(input.worldPos*0.4 + 0.5);
		}

		ENDCG
	}

	FallBack "Diffuse"
}
