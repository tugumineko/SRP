﻿#ifndef CUSTOM_BRDF_INCLUDED
#define CUSTOM_BRDF_INCLUDED

#include "Surface.hlsl"
#include "Common.hlsl"
#define MIN_REFLECTIVITY 0.04

struct BRDF
{
    float3 diffuse;
    float3 specular;
    float roughness;
    float perceptualRoughness;
    float fresnel;
};

float OneMinusReflectivity(float metallic)
{
    float range = 1.0 - MIN_REFLECTIVITY;
    return range - metallic * range ;
}

float SpecularStrength(Surface surface, BRDF brdf, Light light)
{
    float3 h = SafeNormalize(light.direction + surface.viewDirection);
    float nh2 = Square(saturate(dot(surface.normal,h)));
    float lh2 = Square(saturate(dot(light.direction,h)));
    float r2 = Square(brdf.roughness);
    float d2 = Square(nh2* (r2 -1.0)+ 1.0001f);
    float normalization = brdf.roughness * 4.0 + 2.0;
    return r2 / (d2 * max(0.1,lh2) * normalization);
}   

float3 DirectBRDF(Surface surface, BRDF brdf, Light light)
{
    // @source https://community.arm.com/cfs-file/__key/communityserver-blogs-components-weblogfiles/00-00-00-20-66/siggraph2015_2D00_mmg_2D00_renaldas_2D00_slides.pdf
    return SpecularStrength(surface, brdf, light) * brdf.specular + brdf.diffuse;
}

// for gi
float3 IndirectBRDF(Surface surface, BRDF brdf, float3 diffuse, float3 specular)
{
    float fresnelStrength = surface.fresnelStrength * Pow4(1.0 - saturate(dot(surface.normal,surface.viewDirection))); // F_0 * (1 - (n * l))^4
    float3 reflection = specular * lerp(brdf.specular,brdf.fresnel,fresnelStrength);
    reflection /= brdf.roughness * brdf.roughness + 1.0;
    return (diffuse * brdf.diffuse + reflection) * surface.occlusion; 
}

BRDF GetBRDF(inout Surface surface, bool applyAlphaDiffuse = false)
{
    BRDF brdf;
    float oneMinusReflectivity  = OneMinusReflectivity(surface.metallic);
    brdf.diffuse = surface.color * oneMinusReflectivity; // diffuse = 0.96 * color * (1 - metallic)
    if (applyAlphaDiffuse)
    {
        brdf.diffuse *= surface.alpha;
    }
    brdf.specular = lerp(MIN_REFLECTIVITY,surface.color,surface.metallic); // (0.04, 0.04, 0.04) * (1 - metallic) + color * metallic
    brdf.perceptualRoughness = PerceptualRoughnessToPerceptualSmoothness(surface.smoothness);
    brdf.roughness = PerceptualRoughnessToRoughness(brdf.perceptualRoughness);
    brdf.fresnel = saturate(surface.smoothness + 1.0 - oneMinusReflectivity); // 0.04 + 0.96 * metallic + smoothness
    
    return brdf;
}


#endif