#ifndef POSTPROCESSBLUR_HLSL_INCLUDED
#define POSTPROCESSBLUR_HLSL_INCLUDED

void SimpleBlurK5_float(float2 UV, float2 PixelSize, UnityTexture2D TextureIn, UnitySamplerState Sampler, out float4 ColorOut){
    const int KERNEL_SIZE = 5;
    const float WEIGHTS[5] = { 0.15, 0.2, 0.3, 0.2, 0.15 };

    // Horizontal Blur Pass
    float4 horizontalBlur = 0.0;
    for (int xOffset = -KERNEL_SIZE/2; xOffset <= KERNEL_SIZE/2; xOffset++)
    {
    	float2 offset = float2(xOffset*PixelSize.x, 0);
        float4 color = SAMPLE_TEXTURE2D(TextureIn.tex, Sampler.samplerstate, UV+offset);
        horizontalBlur += color * WEIGHTS[xOffset + KERNEL_SIZE/2];
    }

    // Vertical Blur Pass
    float4 verticalBlur = 0.0;
    for (int yOffset = -KERNEL_SIZE/2; yOffset <= KERNEL_SIZE/2; yOffset++)
    {
		float2 offset = float2(0, yOffset*PixelSize.y);
        float4 color = SAMPLE_TEXTURE2D(TextureIn.tex, Sampler.samplerstate, UV+offset);
        verticalBlur += color * WEIGHTS[yOffset + KERNEL_SIZE/2];
    }

    ColorOut = (horizontalBlur + verticalBlur) * 0.5;
}

void SimpleBlurK5_half(half2 UV, half2 PixelSize, UnityTexture2D TextureIn, UnitySamplerState Sampler, out half4 ColorOut){
    const int KERNEL_SIZE = 5;
    const float WEIGHTS[5] = { 0.15, 0.2, 0.3, 0.2, 0.15 };

    // Horizontal Blur Pass
    half4 horizontalBlur = 0.0;
    for (int xOffset = -KERNEL_SIZE/2; xOffset <= KERNEL_SIZE/2; xOffset++)
    {
    	half2 offset = half2(xOffset*PixelSize.x, 0);
        half4 color = SAMPLE_TEXTURE2D(TextureIn.tex, Sampler.samplerstate, UV+offset);
        horizontalBlur += color * WEIGHTS[xOffset + KERNEL_SIZE/2];
    }

    // Vertical Blur Pass
    half4 verticalBlur = 0.0;
    for (int yOffset = -KERNEL_SIZE/2; yOffset <= KERNEL_SIZE/2; yOffset++)
    {
		half2 offset = half2(0, yOffset*PixelSize.y);
        half4 color = SAMPLE_TEXTURE2D(TextureIn.tex, Sampler.samplerstate, UV+offset);
        verticalBlur += color * WEIGHTS[yOffset + KERNEL_SIZE/2];
    }

    ColorOut = (horizontalBlur + verticalBlur) * 0.5;
}

void SimpleBlurK7_float(float2 UV, float2 PixelSize, UnityTexture2D TextureIn, UnitySamplerState Sampler, out float4 ColorOut){
    const int KERNEL_SIZE = 7;
    const float WEIGHTS[7] = { 0.11, 0.13, 0.16, 0.2, 0.16, 0.13, 0.11 };

    // Horizontal Blur Pass
    float4 horizontalBlur = 0.0;
    for (int xOffset = -KERNEL_SIZE/2; xOffset <= KERNEL_SIZE/2; xOffset++)
    {
    	float2 offset = float2(xOffset*PixelSize.x, 0);
        float4 color = SAMPLE_TEXTURE2D(TextureIn.tex, Sampler.samplerstate, UV+offset);
        horizontalBlur += color * WEIGHTS[xOffset + KERNEL_SIZE/2];
    }

    // Vertical Blur Pass
    float4 verticalBlur = 0.0;
    for (int yOffset = -KERNEL_SIZE/2; yOffset <= KERNEL_SIZE/2; yOffset++)
    {
		float2 offset = float2(0, yOffset*PixelSize.y);
        float4 color = SAMPLE_TEXTURE2D(TextureIn.tex, Sampler.samplerstate, UV+offset);
        verticalBlur += color * WEIGHTS[yOffset + KERNEL_SIZE/2];
    }

    ColorOut = (horizontalBlur + verticalBlur) * 0.5;
}

void SimpleBlurK7_half(half2 UV, half2 PixelSize, UnityTexture2D TextureIn, UnitySamplerState Sampler, out half4 ColorOut){
    const int KERNEL_SIZE = 7;
    const half WEIGHTS[7] = { 0.11, 0.13, 0.16, 0.2, 0.16, 0.13, 0.11 };

    // Horizontal Blur Pass
    half4 horizontalBlur = 0.0;
    for (int xOffset = -KERNEL_SIZE/2; xOffset <= KERNEL_SIZE/2; xOffset++)
    {
    	half2 offset = half2(xOffset*PixelSize.x, 0);
        half4 color = SAMPLE_TEXTURE2D(TextureIn.tex, Sampler.samplerstate, UV+offset);
        horizontalBlur += color * WEIGHTS[xOffset + KERNEL_SIZE/2];
    }

    // Vertical Blur Pass
    half4 verticalBlur = 0.0;
    for (int yOffset = -KERNEL_SIZE/2; yOffset <= KERNEL_SIZE/2; yOffset++)
    {
		half2 offset = half2(0, yOffset*PixelSize.y);
        half4 color = SAMPLE_TEXTURE2D(TextureIn.tex, Sampler.samplerstate, UV+offset);
        verticalBlur += color * WEIGHTS[yOffset + KERNEL_SIZE/2];
    }

    ColorOut = (horizontalBlur + verticalBlur) * 0.5;
}

void GaussianBlur_float(float2 UV, float2 PixelSize, UnityTexture2D TextureIn, UnitySamplerState Sampler, out float4 ColorOut){
    const int KERNEL_SIZE = 5;
    const float WEIGHTS[5][5] = {
        { 0.00390625, 0.015625, 0.0234375, 0.015625, 0.00390625 },
        { 0.015625,   0.0625,   0.09375,   0.0625,   0.015625 },
        { 0.0234375,  0.09375,  0.140625,  0.09375,  0.0234375 },
        { 0.015625,   0.0625,   0.09375,   0.0625,   0.015625 },
        { 0.00390625, 0.015625, 0.0234375, 0.015625, 0.00390625 }
    };

    float4 gaussianBlur = 0.0f;
    for (int i = -KERNEL_SIZE/2; i <= KERNEL_SIZE/2; i++){
        for (int j = -KERNEL_SIZE/2; j <= KERNEL_SIZE/2; j++){
			float2 offset = float2(i, j)*PixelSize;
        	float4 color = SAMPLE_TEXTURE2D(TextureIn.tex, Sampler.samplerstate, UV+offset);
            gaussianBlur += color * WEIGHTS[i + KERNEL_SIZE/2][j + KERNEL_SIZE/2];
        }
    }

    ColorOut = gaussianBlur;
}

void GaussianBlur_half(half2 UV, half2 PixelSize, UnityTexture2D TextureIn, UnitySamplerState Sampler, out half4 ColorOut){
    const int KERNEL_SIZE = 5;
    const half WEIGHTS[5][5] = {
        { 0.00390625, 0.015625, 0.0234375, 0.015625, 0.00390625 },
        { 0.015625,   0.0625,   0.09375,   0.0625,   0.015625 },
        { 0.0234375,  0.09375,  0.140625,  0.09375,  0.0234375 },
        { 0.015625,   0.0625,   0.09375,   0.0625,   0.015625 },
        { 0.00390625, 0.015625, 0.0234375, 0.015625, 0.00390625 }
    };

    half4 gaussianBlur = 0.0f;
    for (int i = -KERNEL_SIZE/2; i <= KERNEL_SIZE/2; i++){
        for (int j = -KERNEL_SIZE/2; j <= KERNEL_SIZE/2; j++){
			half2 offset = half2(i, j)*PixelSize;
        	half4 color = SAMPLE_TEXTURE2D(TextureIn.tex, Sampler.samplerstate, UV+offset);
            gaussianBlur += color * WEIGHTS[i + KERNEL_SIZE/2][j + KERNEL_SIZE/2];
        }
    }

    ColorOut = gaussianBlur;
}

#endif