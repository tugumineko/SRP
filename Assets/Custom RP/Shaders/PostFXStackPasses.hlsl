#ifndef CUSTOM_POST_FX_PASSES_INCLUDED
#define CUSTOM_POST_FX_PASSES_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"
#include "../ShaderLibrary/Common.hlsl"

TEXTURE2D(_PostFXSource);
TEXTURE2D(_PostFXSource2);
SAMPLER(sampler_linear_clamp);

float4 _PostFXSource_TexelSize;// float4(1 / width, 1 / height, width, height)

float4 GetSource(float2 screenUV)
{
    return SAMPLE_TEXTURE2D_LOD(_PostFXSource, sampler_linear_clamp, screenUV,0);
}

float4 GetSource2(float2 screenUV)
{
    return SAMPLE_TEXTURE2D_LOD(_PostFXSource2, sampler_linear_clamp, screenUV,0);
}

float4 GetSourceTexelSize()
{
    return _PostFXSource_TexelSize;
}

float4 GetSourceBicubic(float2 screenUV)
{
    return SampleTexture2DBicubic(
        TEXTURE2D_ARGS(_PostFXSource, sampler_linear_clamp), screenUV, _PostFXSource_TexelSize.zwxy,1.0,0.0
        );
}

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float2 screenUV : VAR_SCREEN_UV;
};

Varyings DefaultPassVertex(uint vertexID : SV_VertexID)
{
    Varyings output;
    output.positionCS = float4(
        vertexID <= 1 ? -1.0 : 3.0,
        vertexID == 1 ? 3.0 : -1.0,
        0.0, 1.0
        );
    output.screenUV = float2(
        vertexID <= 1 ? 0.0 : 2.0,
        vertexID == 1 ? 2.0 : 0.0
    );
    if (_ProjectionParams.x < 0.0)
    {
        output.screenUV.y = 1.0 - output.screenUV.y;
    }
    return output;
}

bool _BloomBicubicUpsampling;
float4 _BloomThreshold;
float _BloomIntensity;

float4 CopyPassFragment(Varyings input) : SV_TARGET {
    return GetSource(input.screenUV);
}

float3 ApplyBloomThreshold(float3 color)
{
    float brightness = Max3(color.r, color.g, color.b);
    float soft = brightness + _BloomThreshold.y;
    soft = clamp(soft,0.0,_BloomThreshold.z);
    soft = soft * soft * _BloomThreshold.w;
    float contribution = max(soft,brightness - _BloomThreshold.x);
    contribution /= max(brightness, 0.00001);
    return color * contribution;
}

float4 BloomAddPassFragment(Varyings input) : SV_TARGET {
    float3 lowRes;
    if (_BloomBicubicUpsampling)
    {
        lowRes = GetSourceBicubic(input.screenUV).rgb;
    }
    else
    {
        lowRes = GetSource(input.screenUV).rgb;
    }
    float3 highRes = GetSource2(input.screenUV).rgb;
    return float4(lowRes * _BloomIntensity + highRes,1.0);
}

float4  BloomScatterPassFragment(Varyings input) : SV_TARGET {
    float3 lowRes;
    if (_BloomBicubicUpsampling)
    {
        lowRes = GetSourceBicubic(input.screenUV).rgb;
    }
    else
    {
        lowRes = GetSource(input.screenUV).rgb;
    }
    float3 highRes = GetSource2(input.screenUV).rgb;
    return float4(lerp(highRes,lowRes,_BloomIntensity),1.0);
}

float4 BloomScatterFinalPassFragment(Varyings input) : SV_TARGET {
    float3 lowRes;
    if (_BloomBicubicUpsampling)
    {
        lowRes = GetSourceBicubic(input.screenUV).rgb;
    }
    else
    {
        lowRes = GetSource(input.screenUV).rgb;
    }
    float3 highRes = GetSource2(input.screenUV).rgb;
    lowRes += highRes - ApplyBloomThreshold(highRes);
    return float4(lerp(highRes,lowRes,_BloomIntensity),1.0);
}

float4 BloomHorizontalPassFragment(Varyings input) : SV_TARGET {
    float3 color = 0.0;
    float offsets[] = {
        -4.0,-3.0,-2.0,-1.0,0.0,1.0,2.0,3.0,4.0
    };
    float weights[] = {
        0.01621622, 0.05405405, 0.12162162, 0.19459459, 0.22702703,
        0.19459459, 0.12162162, 0.05405405, 0.01621622
    };
    for (int i = 0 ; i< 9;i++)
    {
        float offset = offsets[i] * 2.0 * GetSourceTexelSize().x;
        color += GetSource(input.screenUV + float2(offset,0.0)).rgb * weights[i];
    }
    return float4(color,1.0);
}

float4 BloomVerticalPassFragment(Varyings input) : SV_TARGET {
    float3 color = 0.0;
    float offsets[] = {
        -3.23076923, -1.38461538, 0.0, 1.38461538, 3.23076923
    };
    float weights[] = {
        0.07027027, 0.31621622, 0.22702703, 0.31621622, 0.07027027
    };
    for (int i = 0; i < 5; i++) {
        float offset = offsets[i] * GetSourceTexelSize().y;
        color += GetSource(input.screenUV + float2(0.0, offset)).rgb * weights[i];
    }
    return float4(color,1.0);
}

float4 BloomPrefilterPassFragment(Varyings input) : SV_TARGET {
    float3 color = ApplyBloomThreshold(GetSource(input.screenUV).rgb);
    return float4(color,1.0);
}



float4 BloomPrefilterFirefliesPassFragment(Varyings input) : SV_TARGET {
    float3 color = 0.0;
    float weightSum = 0.0;
    float2 offsets[] = {
        float2(0.0, 0.0),
        float2(-1.0, -1.0), float2(-1.0, 1.0), float2(1.0, -1.0), float2(1.0, 1.0)
    };
    for (int i = 0 ; i< 5;i++)
    {
        float3 c = GetSource(input.screenUV + offsets[i] * GetSourceTexelSize().xy * 2.0).rgb;
        c = ApplyBloomThreshold(c);
        float w = 1 / (Luminance(c) + 1);
        color += c * w;
        weightSum += w;
    }
    color /= weightSum;
    return float4(color, 1.0);
}

//--------------------------------------------------------------

int _DiscreteLevel;
float _ReduceColorGrayScale; 

float4 ReduceColorPassFragment(Varyings input) : SV_TARGET {
    float3 color = GetSource(input.screenUV).rgb;
    float brightness = Luminance(GetSource(input.screenUV).rgb);
    brightness = floor(brightness * 256 / (256 / (uint)_DiscreteLevel)) * (256 / (uint)_DiscreteLevel) / 256;  //这个地方不能合并或者化简
    float3 grayColor = float3(brightness,brightness,brightness);
    color.r = floor(GetSource(input.screenUV).rgb.r * 256 / (256 / (uint)_DiscreteLevel)) * (256 / (uint)_DiscreteLevel) / 256;
    color.g = floor(color.g * 256 / (256 / (uint)_DiscreteLevel)) * (256 / (uint)_DiscreteLevel) / 256;
    color.b = floor(color.b * 256 / (256 / (uint)_DiscreteLevel)) * (256 / (uint)_DiscreteLevel) / 256;
    return float4(lerp(color,grayColor,_ReduceColorGrayScale),1.0);
}

//-------------------------------------------------------------

#define DECREASE_SCALE 6
#define BAYER_ORDER 2
#define DITHER_BAYER_SETUP DitherBayer2x2

#if defined(_BAYER4)
#define DECREASE_SCALE 4
#define BAYER_ORDER 4
#define DITHER_BAYER_SETUP DitherBayer4x4
#elif defined(_BAYER8)
#define DECREASE_SCALE 2
#define BAYER_ORDER 8
#define DITHER_BAYER_SETUP DitherBayer8x8
#endif

float _DitherBayerGrayScale;

float DitherBayer2x2(int x, int y, float brightness)
{
    const float dither[4] = {
        0, 2,
        3, 1
    };
    int r = y * 2 + x;
    return step(dither[r],brightness);
}

float DitherBayer4x4(int x, int y, float brightness)
{
    const float dither[16] = {
        0, 8, 2, 10,
        12, 4, 14, 6,
        3, 11, 1, 9,
        15, 7, 13, 5
    };
    int r = y * 4 + x;
    return step(dither[r],brightness);
}

float DitherBayer8x8(int x, int y, float brightness)
{
    const float dither[64] = {
        1, 49, 13, 61, 4, 52, 16, 64,
        33, 17, 45, 29, 36, 20, 48, 32,
        9, 57, 5, 53, 12, 60, 8, 56,
        41, 25, 37, 21, 44, 28, 40, 24,
        3, 51, 15, 63, 2, 50, 14, 62,
        35, 19, 47, 31, 34, 18, 46, 30,
        11, 59, 7, 55, 10, 58, 6, 54,
        43, 27, 39, 23, 42, 26, 38, 22        
    };
    int r = y * 8 + x;
    return step(dither[r],brightness);
}

float4 DitherBayerPassFragment(Varyings input) : SV_TARGET {
    float2 screenPos = input.screenUV * GetSourceTexelSize().zw;
    float3 color = GetSource(input.screenUV).rgb;
    int brightness  = (uint)(Luminance(color) * 256) >> DECREASE_SCALE;
    int colorR = (uint)(color.r * 256) >> DECREASE_SCALE;
    int colorG = (uint)(color.g * 256) >> DECREASE_SCALE;
    int colorB = (uint)(color.b * 256) >> DECREASE_SCALE;

    float gray = DITHER_BAYER_SETUP(screenPos.x % BAYER_ORDER, screenPos.y % BAYER_ORDER, brightness);
    float r = DITHER_BAYER_SETUP(screenPos.x % BAYER_ORDER, screenPos.y % BAYER_ORDER, colorR);
    float g = DITHER_BAYER_SETUP(screenPos.x % BAYER_ORDER, screenPos.y % BAYER_ORDER, colorG);
    float b = DITHER_BAYER_SETUP(screenPos.x % BAYER_ORDER, screenPos.y % BAYER_ORDER, colorB);

    float3 grayColor = float3(gray,gray,gray);
    color = float3(r,g,b);

    return float4(lerp(color,grayColor,_DitherBayerGrayScale),1.0);
    
}

//--------------------------------------------------------------------

float _HalftoneTileSizeInverse;

float DrawTilingDisc(float2 uv, float2 tiling, float scale, float offset)
{
    float2 uvTiled = uv * tiling;
    float row = floor(uvTiled.y);
    uvTiled.x += row * offset;
    uvTiled = frac(uvTiled);
    uvTiled -= 0.5;
    float sdf = length(uvTiled);
    return AntialiasingStep(sdf, scale);

}

float4 HalftonePassFragment(Varyings input) : SV_TARGET {
    float2 tileNum = GetSourceTexelSize().zw  * _HalftoneTileSizeInverse;

    float2 uvTiled = input.screenUV * tileNum;
    float row = floor(uvTiled.y);
    float attenuation = _HalftoneTileSizeInverse > 0.0333 ?   0 : saturate(1.0 - _HalftoneTileSizeInverse);
    uvTiled.x += row *  attenuation;
    float2 mosaicUV = ceil(uvTiled) / tileNum;
    float3 mosaicColor = GetSource(mosaicUV).rgb;

    float grey = Luminance(GetSource(mosaicUV).rgb);
    float round = DrawTilingDisc(input.screenUV,tileNum,0.4 * grey + 0.15 + _HalftoneTileSizeInverse, attenuation);
    
    return float4( round * mosaicColor ,1.0);
}

//---------------------------------------------------------------------

float4 ToneMappingACESPassFragment(Varyings input) : SV_TARGET {
    float4 color = GetSource(input.screenUV);
    color.rgb = min(color.rgb,60.0);
    color.rgb = AcesTonemap(unity_to_ACES(color.rgb));
    return color;
}

float4 ToneMappingNeutralPassFragment(Varyings input) : SV_TARGET {
    float4 color = GetSource(input.screenUV);
    color.rgb = min(color.rgb,60.0);
    color.rgb = NeutralTonemap(color.rgb);
    return color;
}

float4 ToneMappingReinhardPassFragment(Varyings input) : SV_TARGET {
    float4 color = GetSource(input.screenUV);
    color.rgb = min(color.rgb,60.0);
    color.rgb /= color.rgb + 1.0;
    return color;
}

#endif