#ifndef UNIVERSAL_FOGOFWAR_INCLUDED
#define UNIVERSAL_FOGOFWAR_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

//#if defined(SHADER_API_MOBILE) && (defined(SHADER_API_GLES) || defined(SHADER_API_GLES30))
//    #define MAX_FOW_COUNT 16
//#elif defined(SHADER_API_MOBILE) || (defined(SHADER_API_GLCORE) && !defined(SHADER_API_SWITCH)) || defined(SHADER_API_GLES) || defined(SHADER_API_GLES3) // Workaround because SHADER_API_GLCORE is also defined when SHADER_API_SWITCH is
//    #define MAX_FOW_COUNT 32
//#else
    #define MAX_FOW_COUNT 32
//#endif


TEXTURE2D_SHADOW(_FogOfWarmapAtlas);
float4x4 _FogOfWarMatrices[MAX_FOW_COUNT];
float4 _FogOfWarParams[MAX_FOW_COUNT]; // Per-custom shadow data
float4 _FogOfWarParams2[MAX_FOW_COUNT];
float4 _FogOfWarPositions[MAX_FOW_COUNT];
float4      _FogOfWarmapSize; // (xy: 1/width and 1/height, zw: width and height)
int         _FogOfWarCount;
float4      _FogOfWarOffset0; // xy: offset0, zw: offset1
float4      _FogOfWarOffset1; // xy: offset2, zw: offset3
float _FogOfWarFeatureStrength;

//#if !_USE_WEBGL
//#define FOGOFWAR_LOOP_BEGIN(FogOfWarCount) \
//    for (uint shadowIndex = 0u; shadowIndex < FogOfWarCount; ++shadowIndex) {

//#define FOGOFWAR_LOOP_END }
//#else
//// WebGL 1 doesn't support variable for loop conditions
//#define FOGOFWAR_LOOP_BEGIN(FogOfWarCount) \
//    for (int shadowIndex = 0; shadowIndex < _WEBGL1_MAX_SHADOWS; ++shadowIndex) { \
//        if (shadowIndex >= (int)FogOfWarCount) break;

//#define FOGOFWAR_LOOP_END }
//#endif

//struct ShadowSamplingData
//{
//    half4 shadowOffset0;
//    half4 shadowOffset1;
//    half4 shadowOffset2;
//    half4 shadowOffset3;
//    float4 shadowmapSize;
//    half softShadowQuality;
//};
//SAMPLER_CMP(sampler_LinearClampCompare);

ShadowSamplingData GetFogOfWarSamplingData(int index)
{
    ShadowSamplingData shadowSamplingData = (ShadowSamplingData)0;

#if defined(_CUSTOM_CLIPPING) || defined(_CUSTOM_LIGHTING)
    // shadowOffsets are used in SampleShadowmapFiltered for low quality soft shadows.
    shadowSamplingData.shadowOffset0 = _FogOfWarOffset0;
    shadowSamplingData.shadowOffset1 = _FogOfWarOffset1;

    // shadowmapSize is used in SampleShadowmapFiltered otherwise.
    shadowSamplingData.shadowmapSize = _FogOfWarmapSize;
    shadowSamplingData.softShadowQuality = _FogOfWarParams[index].y;
#endif
    return shadowSamplingData;
}

half4 GetFogOfWarParams(int index)
{
#if defined(_CUSTOM_CLIPPING) || defined(_CUSTOM_LIGHTING)

    return _FogOfWarParams[index];

#else
    // Same defaults as set in AdditionalLightsShadowCasterPass.cs
    return half4(0, 0, 0, -1);
#endif
}

half4 GetFogOfWarParams2(int index)
{
#if defined(_CUSTOM_CLIPPING) || defined(_CUSTOM_LIGHTING)

    return _FogOfWarParams2[index];

#else
    // Same defaults as set in AdditionalLightsShadowCasterPass.cs
    return half4(0, 0, 0, 100);
#endif
}

float4 GetCustomFogOfWarPosition(int index)
{
    return _FogOfWarPositions[index];
}
//
//half remap(half x, half in_min, half in_max, half out_min, half out_max)
//{
//    return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
//}

int GetCustomFogOfWarCount() {
    return _FogOfWarCount;
}

half GetCustomFogOfWarFalloff(half2 shadowCoord,
    half2 areaX, half2 areaY,
    half threshold) {
    return min(
        min(saturate(remap(shadowCoord.x, areaX.x, areaX.x + threshold, 0, 1)),
            saturate(remap(shadowCoord.x, areaX.y - threshold, areaX.y, 1, 0)))
        ,
        min(saturate(remap(shadowCoord.y, areaY.x, areaY.x + threshold, 0, 1)),
            saturate(remap(shadowCoord.y, areaY.y - threshold, areaY.y, 1, 0)))
    );
}
half GetCustomFogOfWarFalloff(half3 shadowCoord,
    half2 areaX, half2 areaY,
    half threshold) {
    return min(
        min(
            min(saturate(remap(shadowCoord.x, areaX.x, areaX.x + threshold, 0, 1)),
                saturate(remap(shadowCoord.x, areaX.y - threshold, areaX.y, 1, 0)))
            ,
            min(saturate(remap(shadowCoord.y, areaY.x, areaY.x + threshold, 0, 1)),
                saturate(remap(shadowCoord.y, areaY.y - threshold, areaY.y, 1, 0)))
        )
        ,
        min(saturate(remap(shadowCoord.z, 0, threshold, 0, 1)),
            saturate(remap(shadowCoord.z, 1, 1 - threshold, 0, 1)))
    );
}

float GetRawDepth(float2 uv)
{
    return SampleSceneDepth(uv.xy).r;
}

half FogOfWar(int index, float3 positionWS, float depthBias = 0, float clipOverFarPlane = 1.0) {
    ShadowSamplingData shadowSamplingData = GetFogOfWarSamplingData(index);

    half4 shadowParams = GetFogOfWarParams(index);
    half4 shadowParams2 = GetFogOfWarParams2(index);

    float4 lightPos = GetCustomFogOfWarPosition(index);
    float3 lightVector = lightPos.xyz - positionWS * lightPos.w;

    float distanceSqr = max(dot(lightVector, lightVector), HALF_MIN);
    half3 lightDirection = half3(lightVector * rsqrt(distanceSqr));
    half farClipPlane = shadowParams2.w;
    
    float4 shadowCoord = mul(_FogOfWarMatrices[index], float4(positionWS + lightDirection * shadowParams.z * (length(lightPos - positionWS) / farClipPlane)
    + lightDirection * depthBias * (length(lightPos - positionWS) / farClipPlane)    , 1.0));

    //shadowCoord.x += shadowParams2.x/ shadowCoord.w;
    //shadowCoord.y += shadowParams2.y/ shadowCoord.w;

    half shadowTileScale = 1.0 / (half)GetCustomFogOfWarCount();

    half4 offsetAndScale = half4(shadowParams2.x * shadowParams2.z, shadowParams2.x * shadowParams2.z + shadowParams2.z, 
    shadowParams2.y * shadowParams2.z, shadowParams2.y * shadowParams2.z + shadowParams2.z);

    
    float falloff = GetCustomFogOfWarFalloff(half3(shadowCoord.x / shadowCoord.w, shadowCoord.y / shadowCoord.w, shadowCoord.z), half2(offsetAndScale.x, offsetAndScale.y/*offsetAndScale.y*/),
        half2(offsetAndScale.z, offsetAndScale.w/*offsetAndScale.w*/),
        shadowParams.w * shadowTileScale * 0.25);

    //falloff *= (shadowCoord.z / shadowCoord.w)>0?1:0;
    //float zpw = (shadowCoord.z / shadowCoord.w);
    //float forwardBlend = saturate( zpw* farClipPlane); // ;
    //return falloff;
    //falloff *= shadowCoord.z > 0 ? 1 : 0;
    //falloff *= saturate(remap(zpw, 0.0, 0.001, 0.0, 1.0));
    //float rawDepth = GetRawDepth(shadowCoord.xy);
    //return Linear01Depth(rawDepth, _ZBufferParams);
        
    //float depth = SAMPLE_TEXTURE2D(TEXTURE2D_X_FLOAT(_FogOfWarmapAtlas), shadowCoord.xy).r;
    
    //return depth;
    
    //return shadowCoord.w / farClipPlane;
    //saturate(remap((shadowCoord.w / farClipPlane), 0.0, 1.0, 0.9, 1.0));
    
    return lerp(
    lerp(0, SampleShadowmap(TEXTURE2D_ARGS(_FogOfWarmapAtlas, sampler_LinearClampCompare), shadowCoord, shadowSamplingData, shadowParams, true), falloff)
    //* saturate(shadowCoord.w)
    , 1, saturate(remap(shadowCoord.w / farClipPlane, 0.75, 1.0, 0.0, 1.0)) * (1 - clipOverFarPlane)) * saturate(shadowCoord.w * 100);
}

half FogOfWars(float depthBias, float3 positionWS, float clipOverFarPlane = 1)
{
    int fogOfWarCount = GetCustomFogOfWarCount();
    half attenuation = 0;
    //FOGOFWAR_LOOP_BEGIN(shadowsCount)
    for (uint shadowIndex = 0u; shadowIndex < fogOfWarCount; ++shadowIndex)
    {
        attenuation += FogOfWar((int)shadowIndex, positionWS, depthBias, clipOverFarPlane);
    }
    //FOGOFWAR_LOOP_END
        /* for (int shadowIndex = 0; shadowIndex < shadowsCount; ++shadowIndex) {
             attenuation *= CustomFogOfWar(shadowIndex, positionWS);
         }*/
        return attenuation;
}


half FogOfWars(float3 positionWS, float clipOverFarPlane = 1)
{
    int fogOfWarCount = GetCustomFogOfWarCount();
    half attenuation = 0;
    //FOGOFWAR_LOOP_BEGIN(shadowsCount)
    for (uint shadowIndex = 0u; shadowIndex < fogOfWarCount; ++shadowIndex)
    {
        attenuation += FogOfWar((int) shadowIndex, positionWS, 0, clipOverFarPlane);
    }
    //FOGOFWAR_LOOP_END
        /* for (int shadowIndex = 0; shadowIndex < shadowsCount; ++shadowIndex) {
             attenuation *= CustomFogOfWar(shadowIndex, positionWS);
         }*/

    return lerp(1, attenuation, _FogOfWarFeatureStrength);
}

#endif
