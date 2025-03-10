#ifndef SIMPLEBLUR_HLSL_INCLUDED
#define SIMPLEBLUR_HLSL_INCLUDED

void BlurShadows_float(float3 WorldPos, float BlurSize, out float BlurredShadow)
{
#if SHADERGRAPH_PREVIEW
    BlurredShadow = 1.0f;
#else
    // Kernel size for blur (Adjust for performance vs. quality)
    const int KERNEL_SIZE = 5;
    const float WEIGHTS[5] = { 0.15, 0.2, 0.3, 0.2, 0.15 };

    // const int KERNEL_SIZE = 7;
    // const float WEIGHTS[7] = { 0.1, 0.15, 0.15, 0.2, 0.15, 0.15, 0.1 };

    // Horizontal Blur Pass
    float horizontalBlur = 0.0;
    for (int i = -KERNEL_SIZE / 2; i <= KERNEL_SIZE / 2; i++)
    {
        float4 shadowCoord = TransformWorldToShadowCoord(WorldPos + float4(i*BlurSize, 0, 0, 0));
        Light mainLight = GetMainLight(shadowCoord);
        horizontalBlur += mainLight.shadowAttenuation * WEIGHTS[i + KERNEL_SIZE / 2];
    }

    // Vertical Blur Pass
    float verticalBlur = 0.0;
    for (int i = -KERNEL_SIZE / 2; i <= KERNEL_SIZE / 2; i++)
    {
        float4 shadowCoord = TransformWorldToShadowCoord(WorldPos + float4(0, i*BlurSize, 0, 0));
        Light mainLight = GetMainLight(shadowCoord);
        verticalBlur += mainLight.shadowAttenuation * WEIGHTS[i + KERNEL_SIZE / 2];
    }

    BlurredShadow = horizontalBlur;
    BlurredShadow = verticalBlur;
    BlurredShadow = (horizontalBlur + verticalBlur) * 0.5;

    // const int KERNEL_SIZE = 5;
    // const float WEIGHTS[5][5] = {
    //     { 0.00390625, 0.015625, 0.0234375, 0.015625, 0.00390625 },
    //     { 0.015625,   0.0625,   0.09375,   0.0625,   0.015625 },
    //     { 0.0234375,  0.09375,  0.140625,  0.09375,  0.0234375 },
    //     { 0.015625,   0.0625,   0.09375,   0.0625,   0.015625 },
    //     { 0.00390625, 0.015625, 0.0234375, 0.015625, 0.00390625 }
    // };
    // float gaussianBlur = 0.0f;
    // for (int i = -KERNEL_SIZE / 2; i <= KERNEL_SIZE / 2; i++)
    // {
    //     for (int j = -KERNEL_SIZE / 2; j <= KERNEL_SIZE / 2; j++){
    //         float4 shadowCoord = TransformWorldToShadowCoord(WorldPos + float4(i*BlurSize, j*BlurSize, 0, 0));
    //         Light mainLight = GetMainLight(shadowCoord);
    //         gaussianBlur += mainLight.shadowAttenuation * WEIGHTS[i + KERNEL_SIZE / 2][j + KERNEL_SIZE / 2];
    //     }
    // }
    // BlurredShadow = gaussianBlur;
#endif
}

void BlurShadows_half(half3 WorldPos, half BlurSize, out half BlurredShadow)
{
#if SHADERGRAPH_PREVIEW
    BlurredShadow = 1.0f;
#else
    // Kernel size for blur (Adjust for performance vs. quality)
    const int KERNEL_SIZE = 5;
    const half WEIGHTS[5] = { 0.15, 0.2, 0.3, 0.2, 0.15 };

    // const int KERNEL_SIZE = 7;
    // const half WEIGHTS[7] = { 0.1, 0.15, 0.15, 0.2, 0.15, 0.15, 0.1 };

    // Horizontal Blur Pass
    half horizontalBlur = 0.0;
    for (int i = -KERNEL_SIZE / 2; i <= KERNEL_SIZE / 2; i++)
    {
        half4 shadowCoord = TransformWorldToShadowCoord(WorldPos + half4(i*BlurSize, 0, 0, 0));
        Light mainLight = GetMainLight(shadowCoord);
        horizontalBlur += mainLight.shadowAttenuation * WEIGHTS[i + KERNEL_SIZE / 2];
    }

    // Vertical Blur Pass
    half verticalBlur = 0.0;
    for (int i = -KERNEL_SIZE / 2; i <= KERNEL_SIZE / 2; i++)
    {
        half4 shadowCoord = TransformWorldToShadowCoord(WorldPos + half4(0, i*BlurSize, 0, 0));
        Light mainLight = GetMainLight(shadowCoord);
        verticalBlur += mainLight.shadowAttenuation * WEIGHTS[i + KERNEL_SIZE / 2];
    }
    
    BlurredShadow = horizontalBlur;
    BlurredShadow = verticalBlur;
    BlurredShadow = (horizontalBlur + verticalBlur) * 0.5;

    // const int KERNEL_SIZE = 5;
    // const half WEIGHTS[5][5] = {
    //     { 0.00390625, 0.015625, 0.0234375, 0.015625, 0.00390625 },
    //     { 0.015625,   0.0625,   0.09375,   0.0625,   0.015625 },
    //     { 0.0234375,  0.09375,  0.140625,  0.09375,  0.0234375 },
    //     { 0.015625,   0.0625,   0.09375,   0.0625,   0.015625 },
    //     { 0.00390625, 0.015625, 0.0234375, 0.015625, 0.00390625 }
    // };
    // half gaussianBlur = 0.0f;
    // for (int i = -KERNEL_SIZE / 2; i <= KERNEL_SIZE / 2; i++)
    // {
    //     for (int j = -KERNEL_SIZE / 2; j <= KERNEL_SIZE / 2; j++){
    //         half4 shadowCoord = TransformWorldToShadowCoord(WorldPos + float4(i*BlurSize, j*BlurSize, 0, 0));
    //         Light mainLight = GetMainLight(shadowCoord);
    //         gaussianBlur += mainLight.shadowAttenuation * WEIGHTS[i + KERNEL_SIZE / 2][j + KERNEL_SIZE / 2];
    //     }
    // }
    // BlurredShadow = gaussianBlur;
#endif
}
#endif