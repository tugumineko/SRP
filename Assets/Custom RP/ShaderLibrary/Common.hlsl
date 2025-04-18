#ifndef CUSTOM_COMMON_INCLUDED
#define CUSTOM_COMMON_INCLUDED

#if defined(_SHADOW_MASK_DISTANCE) || defined(_SHADOW_MASK_ALWAYS)
    #define SHADOWS_SHADOWMASK
#endif

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "UnityInput.hlsl"
#define UNITY_MATRIX_M unity_ObjectToWorld
#define UNITY_MATRIX_I_M unity_WorldToObject
#define UNITY_MATRIX_V unity_MatrixV
#define UNITY_MATRIX_VP unity_MatrixVP
#define UNITY_MATRIX_P glstate_matrix_projection

#define UNITY_MATRIX_I_V (float4x4)0
#define UNITY_PREV_MATRIX_I_M (float4x4)0
#define UNITY_PREV_MATRIX_M (float4x4)0

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl" 
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl" 
float Square (float v)
{
    return v * v;
}
     
float DistanceSquared (float3 pA, float3 pB)
{
    return dot(pA - pB, pA - pB);
}

void ClipLOD(float2 positionCS, float fade)
{
    #if defined(LOD_FADE_CROSSFADE)
    float dither = InterleavedGradientNoise(positionCS.xy,0);
    clip(fade + (fade < 0.0 ? dither : -dither));
    #endif
}

float3 DecodeNormal(float4 sample,float scale)
{
    #if defined(UNITY_NO_DX5nm)
        return normalize(UnpackNormalRGB(sample,scale));
    #else
        return normalize(UnpackNormalmapRGorAG(sample,scale));
    #endif
}

float3 NormalTangentToWorld(float3 normalTS,float3 normalWS,float4 tangentWS)
{
    float3x3 tangentToWorld = CreateTangentToWorld(normalWS,tangentWS.xyz,tangentWS.w);
    return TransformTangentToWorld(normalTS,tangentToWorld);
}

real2 Rotate(real2 uv, real2 center, real rotation)
{
    real sin_rad = sin(rotation);
    real cos_rad = cos(rotation);
    real2x2 rotation_matrix = real2x2(cos_rad,sin_rad,-sin_rad,cos_rad);
    //uv -= center;
    uv = mul(rotation_matrix,uv);
    //uv += center;
    return uv;
}

real AntialiasingStep(real a, real b)
{
    real c = b - a;
    return saturate( c / (abs(ddx(c)) + abs(ddy(c)) ));
}

 #endif
