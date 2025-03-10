#ifndef MAINLIGHT_HLSL_INCLUDED
#define MAINLIGHT_HLSL_INCLUDED

void MainLight_float(float3 WorldPos, out float3 Direction, out float3 Color, out float ShadowAttenuation){
#if SHADERGRAPH_PREVIEW || !defined(UNIVERSAL_PIPELINE_CORE_INCLUDED)
	Direction = float3(-0.7f, 0.7f, -0.7f);
	Color = float3(1.0f, 1.0f, 1.0f);
	ShadowAttenuation = 1.0f;
#else
	float4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
	Light mainLight = GetMainLight(shadowCoord);
	Direction = mainLight.direction;
	Color = mainLight.color;
	ShadowAttenuation = mainLight.shadowAttenuation;
#endif
}

void MainLight_half(half3 WorldPos, out half3 Direction, out half3 Color, out half ShadowAttenuation){

#if SHADERGRAPH_PREVIEW || !defined(UNIVERSAL_PIPELINE_CORE_INCLUDED)
	Direction = half3(-0.7h, 0.7h, -0.7h);
	Color = half3(1.0h, 1.0h, 1.0h);
	ShadowAttenuation = 1.0h;
#else
	half4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
	Light mainLight = GetMainLight(shadowCoord);
	Direction = mainLight.direction;
	Color = mainLight.color;
	ShadowAttenuation = mainLight.shadowAttenuation;
#endif
}


void AdditionalLights_float(
	float SpecularPower, 
	float3 WorldPos, 
	float3 WorldNormal, 
	float3 WorldView, 
	float MainDiffuse, 
	float3 MainSpecular, 
	float3 MainColor,
	out float Diffuse, 
	out float3 Specular, 
	out float3 Color)
{
	Diffuse = MainDiffuse;
	Specular = MainSpecular;
	Color = (MainDiffuse + MainSpecular) * MainColor;

#if !SHADERGRAPH_PREVIEW
	
	uint pixelLightCount = GetAdditionalLightsCount();
	LIGHT_LOOP_BEGIN(pixelLightCount)
		lightIndex = GetPerObjectLightIndex(lightIndex);
		Light light = GetAdditionalPerObjectLight(lightIndex, WorldPos);
		light.shadowAttenuation = AdditionalLightRealtimeShadow(lightIndex, WorldPos, light.direction);
		float attenuation = light.distanceAttenuation * light.shadowAttenuation;

		float lightIncidence = saturate(dot(WorldNormal, light.direction));
		float lightDiffuse = attenuation * lightIncidence;
		float3 lightSpecular = LightingSpecular(lightDiffuse, light.direction, WorldNormal, WorldView, 1, SpecularPower); // hardcoded 1 is ambient occlusion

		Diffuse += lightDiffuse;
		Specular += lightSpecular;
		Color += (lightDiffuse + lightSpecular) * light.color;
	LIGHT_LOOP_END

	float brightness = Diffuse + dot(Specular, float3(0.333f, 0.333f, 0.333f));
	Color = brightness <= 0 ? MainColor : Color/brightness;

#endif
}

void AdditionalLights_half(
	half SpecularPower, 
	half3 WorldPos, 
	half3 WorldNormal, 
	half3 WorldView, 
	half MainDiffuse, 
	half3 MainSpecular, 
	half3 MainColor,
	out half Diffuse, 
	out half3 Specular, 
	out half3 Color)
{
	Diffuse = MainDiffuse;
	Specular = MainSpecular;
	Color = (MainDiffuse + MainSpecular) * MainColor;

#if !SHADERGRAPH_PREVIEW
	
	uint pixelLightCount = GetAdditionalLightsCount();
	LIGHT_LOOP_BEGIN(pixelLightCount)
		lightIndex = GetPerObjectLightIndex(lightIndex);
		Light light = GetAdditionalPerObjectLight(lightIndex, WorldPos);
		light.shadowAttenuation = AdditionalLightRealtimeShadow(lightIndex, WorldPos, light.direction);
		half attenuation = light.distanceAttenuation * light.shadowAttenuation;

		half lightIncidence = saturate(dot(WorldNormal, light.direction));
		half lightDiffuse = attenuation * lightIncidence;
		half3 lightSpecular = LightingSpecular(lightDiffuse, light.direction, WorldNormal, WorldView, 1, SpecularPower); // hardcoded 1 is ambient occlusion

		Diffuse += lightDiffuse;
		Specular += lightSpecular;
		Color += (lightDiffuse + lightSpecular) * light.color;
	LIGHT_LOOP_END

	half brightness = Diffuse + dot(Specular, half3(0.333f, 0.333f, 0.333f));
	Color = brightness <= 0 ? MainColor : Color/brightness;

#endif
}


#endif
