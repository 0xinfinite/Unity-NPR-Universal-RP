
#ifndef UNIVERSAL_STYLIZED_REALTIME_LIGHTS_INCLUDED
#define UNIVERSAL_STYLIZED_REALTIME_LIGHTS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/AmbientOcclusion.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LightCookie/LightCookie.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Clustering.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"

//// Abstraction over Light shading data.
//struct Light
//{
//    half3   direction;
//    half3   color;
//    float   distanceAttenuation; // full-float precision required on some platforms
//    half    shadowAttenuation;
//    uint    layerMask;
//};

// WebGL1 does not support the variable conditioned for loops used for additional lights
//#if !defined(_USE_WEBGL1_LIGHTS) && defined(UNITY_PLATFORM_WEBGL) && !defined(SHADER_API_GLES3)
//    #define _USE_WEBGL1_LIGHTS 1
//    #define _WEBGL1_MAX_LIGHTS 8
//#else
//    #define _USE_WEBGL1_LIGHTS 0
//#endif
//
//#if USE_FORWARD_PLUS && defined(LIGHTMAP_ON) && defined(LIGHTMAP_SHADOW_MIXING)
//#define FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK if (_AdditionalLightsColor[lightIndex].a > 0.0h) continue;
//#else
//#define FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK
//#endif
//
//#if USE_FORWARD_PLUS
//    #define LIGHT_LOOP_BEGIN(lightCount) { \
//    uint lightIndex; \
//    ClusterIterator _urp_internal_clusterIterator = ClusterInit(inputData.normalizedScreenSpaceUV, inputData.positionWS, 0); \
//    [loop] while (ClusterNext(_urp_internal_clusterIterator, lightIndex)) { \
//        lightIndex += URP_FP_DIRECTIONAL_LIGHTS_COUNT; \
//        FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK
//    #define LIGHT_LOOP_END } }
//#elif !_USE_WEBGL1_LIGHTS
//    #define LIGHT_LOOP_BEGIN(lightCount) \
//    for (uint lightIndex = 0u; lightIndex < lightCount; ++lightIndex) {
//
//    #define LIGHT_LOOP_END }
//#else
//    // WebGL 1 doesn't support variable for loop conditions
//    #define LIGHT_LOOP_BEGIN(lightCount) \
//    for (int lightIndex = 0; lightIndex < _WEBGL1_MAX_LIGHTS; ++lightIndex) { \
//        if (lightIndex >= (int)lightCount) break;
//
//    #define LIGHT_LOOP_END }
//#endif

///////////////////////////////////////////////////////////////////////////////
//                        Attenuation Functions                               /
///////////////////////////////////////////////////////////////////////////////

float remap(float In, float2 InMinMax, float2 OutMinMax)
{
    return OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
}

// Matches Unity Vanilla HINT_NICE_QUALITY attenuation
// Attenuation smoothly decreases to light range.
float StylizedDistanceAttenuation(float distance, float2 distanceAttenuation)
{
    //return distanceAttenuation.x;
    //return 1;
    // We use a shared distance attenuation for additional directional and puctual lights
    // for directional lights attenuation will be 1
    float lightAtten = saturate(remap(distance, float2(distanceAttenuation.x * distanceAttenuation.y, distanceAttenuation.x), float2(1, 0))); //distance > distanceAttenuation.x * 0.75
        //? 
        ///*cos((distance - distanceAttenuation.x - 1) * 3.1415 * 0.5) */ : 1;//rcp(distanceSqr);
    //float2 distanceAttenuationFloat = float2(distanceAttenuation);

    //// Use the smoothing factor also used in the Unity lightmapper.
    //half factor = half(distanceSqr * distanceAttenuationFloat.x);
    //half smoothFactor = saturate(half(1.0) - factor * factor);
    //smoothFactor = smoothFactor * smoothFactor;

    return lightAtten;// *smoothFactor;
}
//
//half AngleAttenuation(half3 spotDirection, half3 lightDirection, half2 spotAttenuation)
//{
//    // Spot Attenuation with a linear falloff can be defined as
//    // (SdotL - cosOuterAngle) / (cosInnerAngle - cosOuterAngle)
//    // This can be rewritten as
//    // invAngleRange = 1.0 / (cosInnerAngle - cosOuterAngle)
//    // SdotL * invAngleRange + (-cosOuterAngle * invAngleRange)
//    // SdotL * spotAttenuation.x + spotAttenuation.y
//
//    // If we precompute the terms in a MAD instruction
//    half SdotL = dot(spotDirection, lightDirection);
//    half atten = saturate(SdotL * spotAttenuation.x + spotAttenuation.y);
//    return atten * atten;
//}

///////////////////////////////////////////////////////////////////////////////
//                      Light Abstraction                                    //
///////////////////////////////////////////////////////////////////////////////

//Light GetMainLight()
//{
//    Light light;
//    light.direction = half3(_MainLightPosition.xyz);
//#if USE_FORWARD_PLUS
//#if defined(LIGHTMAP_ON) && defined(LIGHTMAP_SHADOW_MIXING)
//    light.distanceAttenuation = _MainLightColor.a;
//#else
//    light.distanceAttenuation = 1.0;
//#endif
//#else
//    light.distanceAttenuation = unity_LightData.z; // unity_LightData.z is 1 when not culled by the culling mask, otherwise 0.
//#endif
//    light.shadowAttenuation = 1.0;
//    light.color = _MainLightColor.rgb;
//
//    light.layerMask = _MainLightLayerMask;
//
//    return light;
//}



//Light GetMainLight(float shadowCastOffset)
//{
//    Light light = GetMainLight();
//
//    //shadowCoord.z += shadowCastOffset;
//    
//    light.shadowAttenuation = MainLightRealtimeShadow(shadowCoord);
//    return light;
//}

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Custom/Projection.hlsl"

TEXTURE2D(_MainCharacterShadowmap);
SAMPLER(sampler_MainCharacterShadowmap);
float4x4 _MainCharacterMatrix;


float3 MainCharacterLightShadow(float3 positionWS) {

    float4 projUV = ProjectUVFromWorldPos(_MainCharacterMatrix, positionWS);
    float2 uv = ProjectionUVToTex2DUV(projUV);

    if (ClipUVBoarder(uv)) {
        return -1;
    }
    if (ClipBackProjection(projUV)) {
        return -1;
    }

    float dfp = DepthFromProjection(projUV);
    float dfd = DepthFromDepthmap(_MainCharacterShadowmap, sampler_MainCharacterShadowmap, projUV, 1);

    return float3(uv.x, uv.y ,1 - ClipProjectionShadow(dfp, dfd, -0.0001));
}

Light GetMainLight(float3 positionWS, half4 shadowMask, float shadowCastOffset)
{
    Light light = GetMainLight();

    float3 virtualWorldPos = positionWS + (light.direction * shadowCastOffset);

    float4 shadowCoord = TransformWorldToShadowCoord(virtualWorldPos);

    light.shadowAttenuation = MainLightShadow(shadowCoord, positionWS, shadowMask, _MainLightOcclusionProbes);

#if defined(MAIN_CHARACTER_SHADOW_ON)
    float3 detailShadow = MainCharacterLightShadow(virtualWorldPos);
    if (detailShadow.z > -0.1) {

        light.shadowAttenuation = min(light.shadowAttenuation, detailShadow.z);
      
    }
#endif

    #if defined(_LIGHT_COOKIES)
        real3 cookieColor = SampleMainLightCookie(positionWS);
        light.color *= cookieColor;
    #endif

    return light;
}

Light GetMainLight(InputData inputData, half4 shadowMask, AmbientOcclusionFactor aoFactor, float shadowCastOffset)
{
    Light light = GetMainLight(inputData.positionWS, shadowMask, shadowCastOffset);

    #if defined(_SCREEN_SPACE_OCCLUSION) && !defined(_SURFACE_TYPE_TRANSPARENT)
    if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_AMBIENT_OCCLUSION))
    {
        light.color *= aoFactor.directAmbientOcclusion;
    }
    #endif

    return light;
}
//
//// Fills a light struct given a perObjectLightIndex
Light GetStylizedAdditionalPerObjectLight(int perObjectLightIndex, float3 positionWS)
{
    // Abstraction over Light input constants
#if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
    float4 lightPositionWS = _AdditionalLightsBuffer[perObjectLightIndex].position;
    half3 color = _AdditionalLightsBuffer[perObjectLightIndex].color.rgb;
    half4 distanceAndSpotAttenuation = _AdditionalLightsBuffer[perObjectLightIndex].attenuation;
    half4 spotDirection = _AdditionalLightsBuffer[perObjectLightIndex].spotDirection;
    uint lightLayerMask = _AdditionalLightsBuffer[perObjectLightIndex].layerMask;
#else
    float4 lightPositionWS = _AdditionalLightsPosition[perObjectLightIndex];
    half3 color = _AdditionalLightsColor[perObjectLightIndex].rgb;
    half4 distanceAndSpotAttenuation = _AdditionalLightsAttenuation[perObjectLightIndex];       //Edited GetPunctualLightDistanceAttenuation() from com.unity.render-pipelines.universal\Runtime\UniversalRenderPipelineCore.cs 
    half4 spotDirection = _AdditionalLightsSpotDir[perObjectLightIndex];
    uint lightLayerMask = asuint(_AdditionalLightsLayerMasks[perObjectLightIndex]);
#endif

    // Directional lights store direction in lightPosition.xyz and have .w set to 0.0.
    // This way the following code will work for both directional and punctual lights.
    float3 lightVector = lightPositionWS.xyz - positionWS * lightPositionWS.w;
    float distanceSqr = max(dot(lightVector, lightVector), HALF_MIN);

    float distance = length(lightPositionWS.xyz - positionWS);

    half3 lightDirection = half3(lightVector * rsqrt(distanceSqr));
    // full-float precision required on some platforms
    float attenuation = StylizedDistanceAttenuation(distance, distanceAndSpotAttenuation)
        * AngleAttenuation(spotDirection.xyz, lightDirection, distanceAndSpotAttenuation.zw);

    Light light;
    light.direction = lightDirection;
    light.distanceAttenuation = attenuation;
    light.shadowAttenuation = 1.0; // This value can later be overridden in GetAdditionalLight(uint i, float3 positionWS, half4 shadowMask)
    light.color = color;
    light.layerMask = lightLayerMask;

    return light;
}

//uint GetPerObjectLightIndexOffset()
//{
//#if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
//    return uint(unity_LightData.x);
//#else
//    return 0;
//#endif
//}
//
//// Returns a per-object index given a loop index.
//// This abstract the underlying data implementation for storing lights/light indices
//int GetPerObjectLightIndex(uint index)
//{
///////////////////////////////////////////////////////////////////////////////////////////////
//// Structured Buffer Path                                                                   /
////                                                                                          /
//// Lights and light indices are stored in StructuredBuffer. We can just index them.         /
//// Currently all non-mobile platforms take this path :(                                     /
//// There are limitation in mobile GPUs to use SSBO (performance / no vertex shader support) /
///////////////////////////////////////////////////////////////////////////////////////////////
//#if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
//    uint offset = uint(unity_LightData.x);
//    return _AdditionalLightsIndices[offset + index];
//
///////////////////////////////////////////////////////////////////////////////////////////////
//// UBO path                                                                                 /
////                                                                                          /
//// We store 8 light indices in half4 unity_LightIndices[2];                                 /
//// Due to memory alignment unity doesn't support int[] or float[]                           /
//// Even trying to reinterpret cast the unity_LightIndices to float[] won't work             /
//// it will cast to float4[] and create extra register pressure. :(                          /
///////////////////////////////////////////////////////////////////////////////////////////////
//#elif !defined(SHADER_API_GLES)
//    // since index is uint shader compiler will implement
//    // div & mod as bitfield ops (shift and mask).
//
//    // TODO: Can we index a float4? Currently compiler is
//    // replacing unity_LightIndicesX[i] with a dp4 with identity matrix.
//    // u_xlat16_40 = dot(unity_LightIndices[int(u_xlatu13)], ImmCB_0_0_0[u_xlati1]);
//    // This increases both arithmetic and register pressure.
//    //
//    // NOTE: min16float4 bug workaround.
//    // Take the "vec4" part into float4 tmp variable in order to force float4 math.
//    // It appears indexing half4 as min16float4 on DX11 can fail. (dp4 {min16f})
//    float4 tmp = unity_LightIndices[index / 4];
//    return int(tmp[index % 4]);
//#else
//    // Fallback to GLES2. No bitfield magic here :(.
//    // We limit to 4 indices per object and only sample unity_4LightIndices0.
//    // Conditional moves are branch free even on mali-400
//    // small arithmetic cost but no extra register pressure from ImmCB_0_0_0 matrix.
//    half indexHalf = half(index);
//    half2 lightIndex2 = (indexHalf < half(2.0)) ? unity_LightIndices[0].xy : unity_LightIndices[0].zw;
//    half i_rem = (indexHalf < half(2.0)) ? indexHalf : indexHalf - half(2.0);
//    return int((i_rem < half(1.0)) ? lightIndex2.x : lightIndex2.y);
//#endif
//}
//
//// Fills a light struct given a loop i index. This will convert the i
//// index to a perObjectLightIndex
//Light GetAdditionalLight(uint i, float3 positionWS)
//{
//#if USE_FORWARD_PLUS
//    int lightIndex = i;
//#else
//    int lightIndex = GetPerObjectLightIndex(i);
//#endif
//    return GetAdditionalPerObjectLight(lightIndex, positionWS);
//}

Light GetAdditionalLight(uint i, float3 positionWS, half4 shadowMask, float shadowCastOffset)
{
#if USE_FORWARD_PLUS
    int lightIndex = i;
#else
    int lightIndex = GetPerObjectLightIndex(i);
#endif
    Light light = GetStylizedAdditionalPerObjectLight(lightIndex, positionWS);

#if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
    half4 occlusionProbeChannels = _AdditionalLightsBuffer[lightIndex].occlusionProbeChannels;
#else
    half4 occlusionProbeChannels = _AdditionalLightsOcclusionProbes[lightIndex];
#endif
   
    positionWS += light.direction * shadowCastOffset;
    light.shadowAttenuation = AdditionalLightShadow(lightIndex, positionWS, light.direction, shadowMask, occlusionProbeChannels);
#if defined(_LIGHT_COOKIES)
    real3 cookieColor = SampleAdditionalLightCookie(lightIndex, positionWS);
    light.color *= cookieColor;
#endif

    return light;
}

Light GetAdditionalLight(uint i, InputData inputData, half4 shadowMask, AmbientOcclusionFactor aoFactor, float shadowCastOffset)
{
    Light light = GetAdditionalLight(i, inputData.positionWS, shadowMask, shadowCastOffset);

    #if defined(_SCREEN_SPACE_OCCLUSION) && !defined(_SURFACE_TYPE_TRANSPARENT)
    if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_AMBIENT_OCCLUSION))
    {
        light.color *= aoFactor.directAmbientOcclusion;
    }
    #endif

    return light;
}
//
//int GetAdditionalLightsCount()
//{
//#if USE_FORWARD_PLUS
//    // Counting the number of lights in clustered requires traversing the bit list, and is not needed up front.
//    return 0;
//#else
//    // TODO: we need to expose in SRP api an ability for the pipeline cap the amount of lights
//    // in the culling. This way we could do the loop branch with an uniform
//    // This would be helpful to support baking exceeding lights in SH as well
//    return int(min(_AdditionalLightsCount.x, unity_LightData.y));
//#endif
//}
//
//half4 CalculateShadowMask(InputData inputData)
//{
//    // To ensure backward compatibility we have to avoid using shadowMask input, as it is not present in older shaders
//    #if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
//    half4 shadowMask = inputData.shadowMask;
//    #elif !defined (LIGHTMAP_ON)
//    half4 shadowMask = unity_ProbesOcclusion;
//    #else
//    half4 shadowMask = half4(1, 1, 1, 1);
//    #endif
//
//    return shadowMask;
//}

#endif
