#/*ifndef CUSTOM_UNLIT_PASS_INCLUDED
#define CUSTOM_UNLIT_PASS_INCLUDED

#include "../ShaderLibrary/Common.hlsl"

struct Attributes
{
	float3 positionOS : POSITION;
	float2 baseUV : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
	float4 positonCS : POSITION;
	float2 baseUV : VAR_BASE_UV;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings UnlitPassVertex(Attributes input ) {
	Varyings output;
	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_TRANSFER_INSTANCE_ID(input,output);
	float3 positionWS = TransformObjectToWorld(input.positionOS);
	output.positonCS = TransformWorldToHClip(positionWS);
	float4 baseUV_ST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_BaseMap_ST);
	output.baseUV = input.baseUV * baseUV_ST.xy + baseUV_ST.zw;
	return output;
}

float4 UnlitPassFragment(Varyings input) :SV_TARGET
{
	UNITY_SETUP_INSTANCE_ID(input);
	float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,input.baseUV);
	float4 baseColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_BaseColor);
	float4 color = baseColor * baseMap;
	#ifdef _CLIPPING
		clip(color.a - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_Cutoff));
	#endif
	return color;
}

#endif*/

  