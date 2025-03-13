#ifndef DEPTHNORMALS_HLSL_INCLUDED
#define DEPTHNORMALS_HLSL_INCLUDED



// float3 _WorldSpaceCameraPos; // Builtin
void ReconstructWorldPositionFromDepth_float(float2 UV, float Depth, out float3 WorldPos){
    float4 clipPos = float4(UV*2 - 1, Depth, 1); // Move UV from center to bottom-left
    clipPos.y = -clipPos.y; // Y is flipped in game/camera
    float4 viewPos = mul(unity_CameraInvProjection, clipPos);
    viewPos /= viewPos.w;
    WorldPos = mul(unity_CameraToWorld, viewPos).xyz;
}

void ReconstructWorldPositionFromDepth_half(half2 UV, half Depth, out half3 WorldPos){
    half4 clipPos = half4(UV*2 - 1, Depth, 1); // Move UV from center to bottom-left
    clipPos.y = -clipPos.y; // Y is flipped in game/camera
    half4 viewPos = mul(unity_CameraInvProjection, clipPos);
    viewPos /= viewPos.w;
    WorldPos = mul(unity_CameraToWorld, viewPos).xyz;
}

void SampleDepth_float(float2 UV, out float Depth)
{
#if SHADERGRAPH_PREVIEW
    Depth = 1;
#else
    Depth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, UV).r;
#endif
}

void SampleDepth_half(half2 UV, out half Depth)
{
#if SHADERGRAPH_PREVIEW
    Depth = 1;
#else
    Depth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, UV).r;
#endif
}


void ReconstructNormals_float(float2 UV, float Depth, out float3 Normal){
#if SHADERGRAPH_PREVIEW
    Normal = float3(0, 0, 1);
#else
    float3 position;
    ReconstructWorldPositionFromDepth_half(UV, Depth, position);
    Normal = normalize(cross(ddx(position), ddy(position)));
#endif
}

void ReconstructNormals_half(half2 UV, half Depth, out half3 Normal){
#if SHADERGRAPH_PREVIEW
    Normal = half3(0, 0, 1);
#else
    float3 position;
    ReconstructWorldPositionFromDepth_half(UV, Depth, position);
    Normal = normalize(cross(ddx(position), ddy(position)));
#endif
}

#endif