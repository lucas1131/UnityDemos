#ifndef TONEMAPOPERATOR_HLSL
#define TONEMAPOPERATOR_HLSL

void HablePartial_float(float A, float B, float C, float D, float E, float F, float3 ColorIn, out float3 ColorOut){
	ColorOut = ( (ColorIn*(ColorIn*A + B*C) + D*E) / (ColorIn*(ColorIn*A + B) + D*F) ) - E/F;
}

void HablePartial_half(half A, half B, half C, half D, half E, half F, half3 ColorIn, out half3 ColorOut){
	ColorOut = ( (ColorIn*(ColorIn*A + B*C) + D*E) / (ColorIn*(ColorIn*A + B) + D*F) ) - E/F;
}

void ACESFitRttOdt_float(float3 ColorIn, out float3 ColorOut){
	float3 x = ColorIn * (ColorIn + 0.0245786f) - 0.000090537f;
	float3 y = ColorIn * (0.983729f*ColorIn + 0.4329510f) + 0.238081f;
	ColorOut = x/y;
}

void ACESFitRttOdt_half(half3 ColorIn, out half3 ColorOut){
	half3 x = ColorIn * (ColorIn + 0.0245786h) - 0.000090537h;
	half3 y = ColorIn * (0.983729h*ColorIn + 0.4329510h) + 0.238081h;
	ColorOut = x/y;
}

void ACESApprox_float(float3 ColorIn, out float3 ColorOut){
    ColorIn *= 0.6f;
    float a = 2.51f;
    float b = 0.03f;
    float c = 2.43f;
    float d = 0.59f;
    float e = 0.14f;
    ColorOut = clamp((ColorIn*(a*ColorIn+b))/(ColorIn*(c*ColorIn+d)+e), 0.0f, 1.0f);
}

void ACESApprox_half(half3 ColorIn, out half3 ColorOut){
    ColorIn *= 0.6f;
    half a = 2.51f;
    half b = 0.03f;
    half c = 2.43f;
    half d = 0.59f;
    half e = 0.14f;
    ColorOut = clamp((ColorIn*(a*ColorIn+b))/(ColorIn*(c*ColorIn+d)+e), 0.0f, 1.0f);
}

#endif