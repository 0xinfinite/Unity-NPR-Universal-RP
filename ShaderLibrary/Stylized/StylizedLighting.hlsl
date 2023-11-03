#ifndef UNIVERSAL_STYLIZED_LIGHTING_INCLUDED
#define UNIVERSAL_STYLIZED_LIGHTING_INCLUDED

//
//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/BRDF.hlsl"
//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/Debugging3D.hlsl"
//#include "StylizedGlobalIllumination.hlsl"
//#include "StylizedRealtimeLights.hlsl"
//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/AmbientOcclusion.hlsl"
//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
//
//#if defined(LIGHTMAP_ON)
//    #define DECLARE_LIGHTMAP_OR_SH(lmName, shName, index) float2 lmName : TEXCOORD##index
//    #define OUTPUT_LIGHTMAP_UV(lightmapUV, lightmapScaleOffset, OUT) OUT.xy = lightmapUV.xy * lightmapScaleOffset.xy + lightmapScaleOffset.zw;
//    #define OUTPUT_SH(normalWS, OUT)
//#else
//    #define DECLARE_LIGHTMAP_OR_SH(lmName, shName, index) half3 shName : TEXCOORD##index
//    #define OUTPUT_LIGHTMAP_UV(lightmapUV, lightmapScaleOffset, OUT)
//    #define OUTPUT_SH(normalWS, OUT) OUT.xyz = SampleSHVertex(normalWS)
//#endif
//
TEXTURE2D(_WarpMapAtlas);
SAMPLER(sampler_WarpMapAtlas);
int _WarpMapCount;

//float4 _WarpMapAtlas_ST;

///////////////////////////////////////////////////////////////////////////////////
//////                      Lighting Functions                                   //
///////////////////////////////////////////////////////////////////////////////////
////half3 LightingLambert(half3 lightColor, half3 lightDir, half3 normal)
////{
////    half NdotL = saturate(dot(normal, lightDir));
////    return lightColor * NdotL;
////}
////
////half3 LightingSpecular(half3 lightColor, half3 lightDir, half3 normal, half3 viewDir, half4 specular, half smoothness)
////{
////    float3 halfVec = SafeNormalize(float3(lightDir) + float3(viewDir));
////    half NdotH = half(saturate(dot(normal, halfVec)));
////    half modifier = pow(NdotH, smoothness);
////    half3 specularReflection = specular.rgb * modifier;
////    return lightColor * specularReflection;
////}
//
half2 GetWarpMapUVFromAtlas(half NdotL, half warpMapOffset, int count) {
    half y = (1.0 / (half)max(count, 1));
    return half2(NdotL, warpMapOffset + (y * 0.5));
}


//half2 GetDistanceMapUVFromAtlas(half NdotL, int mapIndex, int count) {
//    half y = (1.0 / (half)max(count, 1));
//    return half2(NdotL, (mapIndex/ (half)max(count, 1)) + (y * 0.5));
//}

half4 LightingStylizedBasedDeferred(BRDFData brdfData, BRDFData brdfDataClearCoat,
    half3 lightColor, half3 lightDirectionWS, half lightAttenuation,
    half3 normalWS, half3 viewDirectionWS,
    half clearCoatMask, bool specularHighlightsOff, half warpMapOffset)
{
    half3 NdotL = saturate(dot(normalWS, lightDirectionWS)).xxx;
#if defined(WARPMAP_ATLAS)
    NdotL.r = saturate(SAMPLE_TEXTURE2D(_WarpMapAtlas, sampler_WarpMapAtlas, GetWarpMapUVFromAtlas(NdotL.r, warpMapOffset, _WarpMapCount)).r);
    NdotL.g = saturate(SAMPLE_TEXTURE2D(_WarpMapAtlas, sampler_WarpMapAtlas, GetWarpMapUVFromAtlas(NdotL.g, warpMapOffset, _WarpMapCount)).g);
    NdotL.b = saturate(SAMPLE_TEXTURE2D(_WarpMapAtlas, sampler_WarpMapAtlas, GetWarpMapUVFromAtlas(NdotL.b, warpMapOffset, _WarpMapCount)).b);
#endif
    half3 attenuation = (lightAttenuation * NdotL);
    half3 radiance = lightColor * attenuation;

    half3 brdf = brdfData.diffuse;
#ifndef _SPECULARHIGHLIGHTS_OFF
    [branch] if (!specularHighlightsOff)
    {
        brdf += brdfData.specular * DirectBRDFSpecular(brdfData, normalWS, lightDirectionWS, viewDirectionWS);

#if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
        // Clear coat evaluates the specular a second timw and has some common terms with the base specular.
        // We rely on the compiler to merge these and compute them only once.
        half brdfCoat = kDielectricSpec.r * DirectBRDFSpecular(brdfDataClearCoat, normalWS, lightDirectionWS, viewDirectionWS);

        // Mix clear coat and base layer using khronos glTF recommended formula
        // https://github.com/KhronosGroup/glTF/blob/master/extensions/2.0/Khronos/KHR_materials_clearcoat/README.md
        // Use NoV for direct too instead of LoH as an optimization (NoV is light invariant).
        half NoV = saturate(dot(normalWS, viewDirectionWS));
        // Use slightly simpler fresnelTerm (Pow4 vs Pow5) as a small optimization.
        // It is matching fresnel used in the GI/Env, so should produce a consistent clear coat blend (env vs. direct)
        half coatFresnel = kDielectricSpec.x + kDielectricSpec.a * Pow4(1.0 - NoV);

        brdf = brdf * (1.0 - clearCoatMask * coatFresnel) + brdfCoat * clearCoatMask;
#endif // _CLEARCOAT
    }
#endif // _SPECULARHIGHLIGHTS_OFF

    return half4(brdf * radiance, max(max(attenuation.r, attenuation.g), attenuation.b));
}

half4 LightingStylizedBasedDeferred(BRDFData brdfData, BRDFData brdfDataClearCoat, Light light, half3 normalWS, half3 viewDirectionWS, half clearCoatMask, bool specularHighlightsOff, half warpMapOffset)
{
    return LightingStylizedBasedDeferred(brdfData, brdfDataClearCoat, light.color.rgb* light.color.a, light.direction, light.color.a * light.distanceAttenuation * light.shadowAttenuation, normalWS, viewDirectionWS, clearCoatMask, specularHighlightsOff, warpMapOffset);
}


half4 LightingStylizedBasedDeferred(BRDFData brdfData, Light light, half3 normalWS, half3 viewDirectionWS, bool specularHighlightsOff, half warpMapOffset)
{
    const BRDFData noClearCoat = (BRDFData)0;
    return LightingStylizedBasedDeferred(brdfData, noClearCoat, light, normalWS, viewDirectionWS, 0.0, specularHighlightsOff, warpMapOffset);
}

//half4 LightingStylizedBasedDeferred(BRDFData brdfData, BRDFData brdfDataClearCoat,
//    Light light,
//    half3 normalWS, half3 viewDirectionWS,
//    half clearCoatMask, bool specularHighlightsOff, half warpMapOffset)
//{
//    return LightingStylizedBasedDeferred(brdfData, brdfDataClearCoat,
//        light.color.rgb, light.direction, light.color.a * light.distanceAttenuation * light.shadowAttenuation,
//        normalWS, viewDirectionWS,
//        clearCoatMask, specularHighlightsOff, warpMapOffset);
//
//}

half4 LightingStylizedBased(BRDFData brdfData, BRDFData brdfDataClearCoat,
    half3 lightColor, half3 lightDirectionWS, half lightAttenuation,
    half3 normalWS, half3 viewDirectionWS,
    half clearCoatMask, bool specularHighlightsOff, half warpMapOffset)
{
    half NdotL = saturate(dot(normalWS, lightDirectionWS));
#if defined(WARPMAP_ATLAS)
    NdotL = saturate(SAMPLE_TEXTURE2D(_WarpMapAtlas, sampler_WarpMapAtlas, GetWarpMapUVFromAtlas(NdotL, warpMapOffset, _WarpMapCount)).r);
#endif
    /*half3 radiance = lightColor * (lightAttenuation * NdotL);

    half3 brdf = brdfData.diffuse;*/
    half attenuation = lightAttenuation * NdotL;
    half3 radiance = lightColor;// *attenuation;
    
    half3 brdf = attenuation;// attenuation;//brdfData.diffuse;


#ifndef _SPECULARHIGHLIGHTS_OFF
    [branch] if (!specularHighlightsOff)
    {
        brdf += brdfData.specular * DirectBRDFSpecular(brdfData, normalWS, lightDirectionWS, viewDirectionWS);

#if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
        // Clear coat evaluates the specular a second timw and has some common terms with the base specular.
        // We rely on the compiler to merge these and compute them only once.
        half brdfCoat = kDielectricSpec.r * DirectBRDFSpecular(brdfDataClearCoat, normalWS, lightDirectionWS, viewDirectionWS);

        // Mix clear coat and base layer using khronos glTF recommended formula
        // https://github.com/KhronosGroup/glTF/blob/master/extensions/2.0/Khronos/KHR_materials_clearcoat/README.md
        // Use NoV for direct too instead of LoH as an optimization (NoV is light invariant).
        half NoV = saturate(dot(normalWS, viewDirectionWS));
        // Use slightly simpler fresnelTerm (Pow4 vs Pow5) as a small optimization.
        // It is matching fresnel used in the GI/Env, so should produce a consistent clear coat blend (env vs. direct)
        half coatFresnel = kDielectricSpec.x + kDielectricSpec.a * Pow4(1.0 - NoV);

        brdf = brdf * (1.0 - clearCoatMask * coatFresnel) + brdfCoat * clearCoatMask;
#endif // _CLEARCOAT
    }
#endif // _SPECULARHIGHLIGHTS_OFF

    return half4(brdf * radiance, attenuation); 
}

half4 LightingStylizedBased(BRDFData brdfData, Light light, half3 normalWS, half3 viewDirectionWS, half clearCoatMask, bool specularHighlightsOff, half warpMapOffset)
{
    BRDFData brdfDataClearCoat = (BRDFData)0;
    //    #if defined(DISTANCEATTENUATIONPMAP_ATLAS)
    //    float distanceAttenuation = SAMPLE_TEXTURE2D(_DistanceAttenuationMapAtlas, sampler_DistanceAttenuationMapAtlas, GetDistanceMapUVFromAtlas(light.distanceAttenuation, distAttenOffset, _DistanceAttenuationMapCount)).r;
    //#else
    //    float distanceAttenuation = light.distanceAttenuation;
    //#endif
    return LightingStylizedBased(brdfData, brdfDataClearCoat, light.color.rgb, light.direction, light.color.a * light.distanceAttenuation * light.shadowAttenuation, normalWS, viewDirectionWS, clearCoatMask, specularHighlightsOff, warpMapOffset);
}


half4 LightingStylizedBased(BRDFData brdfData, BRDFData brdfDataClearCoat, Light light, half3 normalWS, half3 viewDirectionWS, half clearCoatMask, bool specularHighlightsOff,  half warpMapOffset)
{
//    #if defined(DISTANCEATTENUATIONPMAP_ATLAS)
//    float distanceAttenuation = SAMPLE_TEXTURE2D(_DistanceAttenuationMapAtlas, sampler_DistanceAttenuationMapAtlas, GetDistanceMapUVFromAtlas(light.distanceAttenuation, distAttenOffset, _DistanceAttenuationMapCount)).r;
//#else
//    float distanceAttenuation = light.distanceAttenuation;
//#endif
    return LightingStylizedBased(brdfData, brdfDataClearCoat, light.color.rgb, light.direction, light.color.a * light.distanceAttenuation * light.shadowAttenuation, normalWS, viewDirectionWS, clearCoatMask, specularHighlightsOff, warpMapOffset);
}

// Backwards compatibility
//half4 LightingStylizedBased(BRDFData brdfData, Light light, half3 normalWS, half3 viewDirectionWS, half warpMapOffset)
//{
//#ifdef _SPECULARHIGHLIGHTS_OFF
//    bool specularHighlightsOff = true;
//#else
//    bool specularHighlightsOff = false;
//#endif
//    const BRDFData noClearCoat = (BRDFData)0;
//    return LightingStylizedBased(brdfData, noClearCoat, light, normalWS, viewDirectionWS, 0.0, specularHighlightsOff, warpMapOffset);
//}
//
//half4 LightingStylizedBased(BRDFData brdfData, half3 lightColor, half3 lightDirectionWS, half lightAttenuation, half3 normalWS, half3 viewDirectionWS, half warpMapOffset)
//{
//    Light light;
//    light.color = half4(lightColor, lightAttenuation);
//    light.direction = lightDirectionWS;
//    light.distanceAttenuation = lightAttenuation;
//    light.shadowAttenuation = 1;
//    return LightingStylizedBased(brdfData, light, normalWS, viewDirectionWS, warpMapOffset);
//}
//
//half4 LightingStylizedBased(BRDFData brdfData, Light light, half3 normalWS, half3 viewDirectionWS, bool specularHighlightsOff, half warpMapOffset)
//{
//    const BRDFData noClearCoat = (BRDFData)0;
//    return LightingStylizedBased(brdfData, noClearCoat, light, normalWS, viewDirectionWS, 0.0, specularHighlightsOff, warpMapOffset);
//}
//
//half4 LightingStylizedBased(BRDFData brdfData, half3 lightColor, half3 lightDirectionWS, half lightAttenuation, half3 normalWS, half3 viewDirectionWS, bool specularHighlightsOff, half warpMapOffset)
//{
//    Light light;
//    light.color = half4( lightColor, lightAttenuation);
//    light.direction = lightDirectionWS;
//    light.distanceAttenuation = lightAttenuation;
//    light.shadowAttenuation = 1;
//    return LightingStylizedBased(brdfData, light, viewDirectionWS, specularHighlightsOff, specularHighlightsOff,  warpMapOffset);
//}

half4 UniversalFragmentNPR(InputData inputData, SurfaceData surfaceData)
{
#if defined(_SPECULARHIGHLIGHTS_OFF)
    bool specularHighlightsOff = true;
#else
    bool specularHighlightsOff = false;
#endif
    BRDFData brdfData;

    // NOTE: can modify "surfaceData"...
    InitializeBRDFData(surfaceData, brdfData);

#if defined(DEBUG_DISPLAY)
    half4 debugColor;

    if (CanDebugOverrideOutputColor(inputData, surfaceData, brdfData, debugColor))
    {
        return debugColor;
    }
#endif

    // Clear-coat calculation...
    BRDFData brdfDataClearCoat = CreateClearCoatBRDFData(surfaceData, brdfData);
    half4 shadowMask = CalculateShadowMask(inputData);
    AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(inputData, surfaceData);
    uint meshRenderingLayers = GetMeshRenderingLayer();
    Light mainLight = GetMainLight(inputData, shadowMask, aoFactor);
    
    // NOTE: We don't apply AO to the GI here because it's done in the lighting calculation below...
    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI);

    LightingData lightingData = CreateLightingData(inputData, surfaceData);

    half distAttenOffset = 0;
    half warpMapOffset = inputData.shadowCoord.w;

    lightingData.giColor = lerp(0, GlobalIllumination(brdfData, brdfDataClearCoat, surfaceData.clearCoatMask,
        inputData.bakedGI, aoFactor.indirectAmbientOcclusion, inputData.positionWS,
        inputData.normalWS, inputData.viewDirectionWS, inputData.normalizedScreenSpaceUV)
        , inputData.shadowCoord.z);
#ifdef _LIGHT_LAYERS
    if (IsMatchingLightLayer(mainLight.layerMask, meshRenderingLayers))
#endif
    {
        lightingData.mainLightColor = LightingStylizedBased(brdfData, brdfDataClearCoat,
            mainLight,
            inputData.normalWS, inputData.viewDirectionWS,
            surfaceData.clearCoatMask, specularHighlightsOff, warpMapOffset);
    }

#if defined(_ADDITIONAL_LIGHTS)
    uint pixelLightCount = GetAdditionalLightsCount();

#if USE_FORWARD_PLUS
    for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
    {
        FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK

            Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);

#ifdef _LIGHT_LAYERS
        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
#endif
        {
            lightingData.additionalLightsColor += LightingStylizedBased(brdfData, brdfDataClearCoat, light,
                inputData.normalWS, inputData.viewDirectionWS,
                surfaceData.clearCoatMask, specularHighlightsOff,  warpMapOffset);
        }
    }
#endif

    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);
    half distAttenOffset = 0;
#ifdef _LIGHT_LAYERS
    if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
#endif
    {
        lightingData.additionalLightsColor += LightingStylizedBased(brdfData, brdfDataClearCoat, light,
            inputData.normalWS, inputData.viewDirectionWS,
            surfaceData.clearCoatMask, specularHighlightsOff, warpMapOffset);
    }
    LIGHT_LOOP_END
#endif

#if defined(_ADDITIONAL_LIGHTS_VERTEX)
        lightingData.vertexLightingColor +=half4( inputData.vertexLighting.rgb * brdfData.diffuse, inputData.vertexLighting.a);
#endif
    
#if REAL_IS_HALF
    // Clamp any half.inf+ to HALF_MAX
    return min(CalculateFinalColor(lightingData, surfaceData.alpha), HALF_MAX);
#else
    return CalculateFinalColor(lightingData, surfaceData.alpha);
#endif
}


//half4 LightingStylizedBased(BRDFData brdfData, BRDFData brdfDataClearCoat,
//    half3 lightColor, half3 lightDirectionWS, half lightAttenuation,
//    half3 normalWS, half3 viewDirectionWS,
//    half clearCoatMask, bool specularHighlightsOff, half warpMapOffset = 0)
//{
//    half NdotL = saturate(dot(normalWS, lightDirectionWS));
//#if defined(WARPMAP_ATLAS)
//    //half4 warpMapOffset = half4(1, 1, 0, 0);//GetWarpMapOffsetFromAtlas(warpMapIndex);
//    NdotL = saturate(SAMPLE_TEXTURE2D(_WarpMapAtlas, sampler_WarpMapAtlas, GetWarpMapUVFromAtlas(NdotL, warpMapOffset, _WarpMapCount) /** warpMapOffset.xy + warpMapOffset.zw*/).r);
//#endif
//    half attenuation = lightAttenuation * NdotL;
//    half3 radiance = lightColor; //* attenuation;
//
//    half3 brdf = attenuation;// attenuation;//brdfData.diffuse;
//#ifndef _SPECULARHIGHLIGHTS_OFF
//    [branch] if (!specularHighlightsOff)
//    {
//        brdf += brdfData.specular * DirectBRDFSpecular(brdfData, normalWS, lightDirectionWS, viewDirectionWS);
//
//#if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
//        // Clear coat evaluates the specular a second timw and has some common terms with the base specular.
//        // We rely on the compiler to merge these and compute them only once.
//        half brdfCoat = kDielectricSpec.r * DirectBRDFSpecular(brdfDataClearCoat, normalWS, lightDirectionWS, viewDirectionWS);
//
//            // Mix clear coat and base layer using khronos glTF recommended formula
//            // https://github.com/KhronosGroup/glTF/blob/master/extensions/2.0/Khronos/KHR_materials_clearcoat/README.md
//            // Use NoV for direct too instead of LoH as an optimization (NoV is light invariant).
//            half NoV = saturate(dot(normalWS, viewDirectionWS));
//            // Use slightly simpler fresnelTerm (Pow4 vs Pow5) as a small optimization.
//            // It is matching fresnel used in the GI/Env, so should produce a consistent clear coat blend (env vs. direct)
//            half coatFresnel = kDielectricSpec.x + kDielectricSpec.a * Pow4(1.0 - NoV);
//
//        brdf = brdf * (1.0 - clearCoatMask * coatFresnel) + brdfCoat * clearCoatMask;
//#endif // _CLEARCOAT
//    }
//#endif // _SPECULARHIGHLIGHTS_OFF
//
//    return half4(brdf * radiance, attenuation);
//}
//
//
//half4 LightingStylizedBased(BRDFData brdfData, BRDFData brdfDataClearCoat, Light light, half3 normalWS, half3 viewDirectionWS, half clearCoatMask, bool specularHighlightsOff, int distAttenOffset, half warpMapOffset = 0)
//{
//#if defined(DISTANCEATTENUATIONPMAP_ATLAS)
//    float distanceAttenuation = SAMPLE_TEXTURE2D(_DistanceAttenuationMapAtlas, sampler_DistanceAttenuationMapAtlas, GetDistanceMapUVFromAtlas(light.distanceAttenuation, distAttenOffset, _DistanceAttenuationMapCount)).r;
//#else
//    float distanceAttenuation = light.distanceAttenuation;
//#endif
//    return LightingStylizedBased(brdfData, brdfDataClearCoat, light.color, light.direction, distanceAttenuation * light.shadowAttenuation, normalWS, viewDirectionWS, clearCoatMask, specularHighlightsOff, warpMapOffset);
//}
//
//half4 LightingStylizedBased(BRDFData brdfData, BRDFData brdfDataClearCoat, Light light, half3 normalWS, half3 viewDirectionWS, half clearCoatMask, bool specularHighlightsOff, half warpMapOffset = 0)
//{
//    return LightingStylizedBased(brdfData, brdfDataClearCoat, light.color, light.direction, light.distanceAttenuation * light.shadowAttenuation, normalWS, viewDirectionWS, clearCoatMask, specularHighlightsOff, warpMapOffset);
//}
//
//// Backwards compatibility
//half4 LightingStylizedBased(BRDFData brdfData, Light light, half3 normalWS, half3 viewDirectionWS)
//{
//    #ifdef _SPECULARHIGHLIGHTS_OFF
//    bool specularHighlightsOff = true;
//#else
//    bool specularHighlightsOff = false;
//#endif
//    const BRDFData noClearCoat = (BRDFData)0;
//    return LightingStylizedBased(brdfData, noClearCoat, light, normalWS, viewDirectionWS, 0.0, specularHighlightsOff);
//}
////half4 LightingStylizedBased(BRDFData brdfData, BRDFData brdfDataClearCoat, Light light, half3 normalWS, half3 viewDirectionWS, half clearCoatMask, bool specularHighlightsOff, GetAdditionalLightDistanceAttenuationOffset(lightIndex), warpMapOffset);
//
//half4 LightingStylizedBased(BRDFData brdfData, half3 lightColor, half3 lightDirectionWS, half lightAttenuation, half3 normalWS, half3 viewDirectionWS)
//{
//    Light light;
//    light.color = lightColor;
//    light.direction = lightDirectionWS;
//    light.distanceAttenuation = lightAttenuation;
//    light.shadowAttenuation   = 1;
//    return LightingStylizedBased(brdfData, light, normalWS, viewDirectionWS);
//}
//
//
//half4 LightingStylizedBased(BRDFData brdfData, Light light, half3 normalWS, half3 viewDirectionWS, bool specularHighlightsOff, int distAttenOffset, half warpMapOffset)
//{
//    const BRDFData noClearCoat = (BRDFData)0;
//    return LightingStylizedBased(brdfData, noClearCoat, light, normalWS, viewDirectionWS, 0.0, specularHighlightsOff, distAttenOffset, warpMapOffset);
//}
//
//half4 LightingStylizedBased(BRDFData brdfData, Light light, half3 normalWS, half3 viewDirectionWS, bool specularHighlightsOff, half warpMapOffset = 0)
//{
//    const BRDFData noClearCoat = (BRDFData)0;
//    return LightingStylizedBased(brdfData, noClearCoat, light, normalWS, viewDirectionWS, 0.0, specularHighlightsOff, _BaseIndexOfDistanceAttenuationMap, warpMapOffset);
//}
//
//half4 LightingStylizedBased(BRDFData brdfData, half3 lightColor, half3 lightDirectionWS, half lightAttenuation, half3 normalWS, half3 viewDirectionWS, bool specularHighlightsOff)
//{
//    Light light;
//    light.color = lightColor;
//    light.direction = lightDirectionWS;
//    light.distanceAttenuation = lightAttenuation;
//    light.shadowAttenuation   = 1;
//    return LightingStylizedBased(brdfData, light, viewDirectionWS, specularHighlightsOff, specularHighlightsOff);
//}
//
//half3 VertexLighting(float3 positionWS, half3 normalWS, float shadowcastOffset)
//{
//    half3 vertexLightColor = half3(0.0, 0.0, 0.0);
//
//#ifdef _ADDITIONAL_LIGHTS_VERTEX
//    uint lightsCount = GetAdditionalLightsCount();
//    LIGHT_LOOP_BEGIN(lightsCount)
//        Light light = GetAdditionalLight(lightIndex, positionWS, shadowcastOffset);
//        half3 lightColor = light.color * light.distanceAttenuation;
//        vertexLightColor += LightingLambert(lightColor, light.direction, normalWS);
//    LIGHT_LOOP_END
//#endif
//
//    return vertexLightColor;
//}
//
//struct StylizedLightingData
//{
//    half3 giColor;
//    half4 mainLightColor;
//    half4 additionalLightsColor;
//    half3 vertexLightingColor;
//    half3 emissionColor;
//    half  lightAttenuation;
//    half  giAdditive;
//    half  giMultiplier;
//    half distanceAttenuationMapOffset;
//};
//
//half4 CalculateIndivisualLightingColor(half4 lightColor, half giAdditive, half giMultiplier, half3 albedo, half3 shadowTint, half3 shadowedColor)
//{
//    return /*lerp( lerp( lerp (pow(albedo, giAdditive), shadowTint, (albedo.r+ albedo.g+ albedo.b)* 0.33333), 0, giMultiplier),
//        lightColor.rgb,
//        lightColor.a);*/
//        //half4(  lerp( shadowTint * lightColor.rgb* giAdditive, lightColor.rgb, lightColor.a), lightColor.a);
//        half4(lerp(0, lightColor.rgb, lightColor.a), lightColor.a);
//}
//
//half CalculateLightStrengthFromColor(half3 color) {
//    return (color.r + color.g + color.b) * 0.3334;
//}
//
//half3 CalculateGIColor(StylizedLightingData lightingData) {
//    return lerp(0, lightingData.giColor, lightingData.giMultiplier);
//}
////
//half4 CalculateStylizedLightingColor(StylizedLightingData lightingData, half3 albedo/*, half3 shadowTint, half shadowedColor*/)
//{
//    half4 lightingColor = 0;
//
//    //if (IsOnlyAOLightingFeatureEnabled())
//    //{
//    //    return half4(lightingData.giColor,0); // Contains white + AO
//    //}
//
//    //if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_GLOBAL_ILLUMINATION))
//    //{
//    //    lightingColor += half4( lightingData.giColor, CalculateLightStrengthFromColor(lightingData.giColor)) * lightingData.giAdditive;
//    //}
//
//    if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_MAIN_LIGHT))
//    {
//        lightingColor += lightingData.mainLightColor;
//    }
//
//    if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_ADDITIONAL_LIGHTS))
//    {
//        lightingColor += lightingData.additionalLightsColor;
//    }
//
//    if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_VERTEX_LIGHTING))
//    {
//        lightingColor += half4(lightingData.vertexLightingColor, CalculateLightStrengthFromColor(lightingData.vertexLightingColor));
//    }
//
//    //lightingColor *= albedo;
//
//    if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_EMISSION))
//    {
//        lightingColor += half4(lightingData.emissionColor, CalculateLightStrengthFromColor(lightingData.emissionColor));
//    }
//
//    return lightingColor;
//}
//
//LightingData StylizedToNormalLightingColor(StylizedLightingData stylized) {
//    LightingData lightingData;
//
//    lightingData.giColor = stylized.giColor;
//    lightingData.emissionColor = stylized.emissionColor;
//    lightingData.vertexLightingColor = stylized.vertexLightingColor;
//    lightingData.mainLightColor = stylized.mainLightColor;
//    lightingData.additionalLightsColor = stylized.additionalLightsColor;
//
//    return lightingData;
//}
//
//half4 CalculateFinalColor(StylizedLightingData lightingData, half alpha, half3 shadowed)
//{
//
//
//    half3 finalColor = CalculateLightingColor(StylizedToNormalLightingColor(lightingData), 1);
//
//    return half4(finalColor, alpha);
//    //half4 lightColor = CalculateLightingColor(lightingData, 1, 0, shadowed);
//    //half3 finalColor = lerp(shadowed, lightColor.rgb, lightColor.a);
//
//    //return half4(finalColor, alpha);
//}
//
//half3 CalculateShadowSideColor(StylizedLightingData lightingData, half3 shadowed, half3 shadowTint) {
//    return lerp(shadowed * shadowTint, lightingData.giColor, lightingData.giMultiplier);
//}
//
//half4 CalculateFinalColor(StylizedLightingData lightingData, half3 albedo, half3 shadowTint, half alpha, float fogCoord, half3 shadowed)
//{
//    #if defined(_FOG_FRAGMENT)
//        #if (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2))
//        float viewZ = -fogCoord;
//        float nearToFarZ = max(viewZ - _ProjectionParams.y, 0);
//        half fogFactor = ComputeFogFactorZ0ToFar(nearToFarZ);
//    #else
//        half fogFactor = 0;
//        #endif
//    #else
//    half fogFactor = fogCoord;
//    #endif
//    half3 giColor = CalculateGIColor(lightingData);
//    half4 lightingColor = CalculateStylizedLightingColor(lightingData, albedo/*, shadowTint, shadowed*/);
//    half3 shadowSideColor = CalculateShadowSideColor(lightingData, shadowed, shadowTint);
//    //lightingColor.rgb *= lightingColor.a;
//    //half3 finalColor = MixFog(lerp(
//    //    /* shadowTint * saturate( lerp(lightingColor.rgb,1, /*CalculateLightStrengthFromColor(lightingColor.rgb)lightingColor.a))*/
//    //    , albedo * lightingColor.rgb, /*((lightingColor.r + lightingColor.g + lightingColor.b) * 0.3333) * */ lightingColor.a), fogFactor);
//    half3 finalColor = half3(
//        lerp(shadowSideColor.r, lightingColor.r * albedo.r + saturate(lerp(shadowSideColor.r, 0, lightingColor.r * lightingColor.a)) , lightingColor.r * lightingColor.a),
//        lerp(shadowSideColor.g, lightingColor.g * albedo.g + saturate(lerp(shadowSideColor.g, 0, lightingColor.g * lightingColor.a)) , lightingColor.g * lightingColor.a),
//        lerp(shadowSideColor.b, lightingColor.b * albedo.b + saturate(lerp(shadowSideColor.b, 0, lightingColor.b * lightingColor.a)) , lightingColor.b * lightingColor.a));
//    
//    finalColor += lerp(0, giColor, lightingData.giAdditive);
//
//    finalColor = MixFog(finalColor, fogFactor);
//
//    return half4(finalColor* alpha, alpha);
//}
//
//StylizedLightingData CreateStylizedLightingData(InputData inputData, SurfaceData  surfaceData)
//{
//    StylizedLightingData lightingData;
//
//    lightingData.giColor = inputData.bakedGI;
//    lightingData.emissionColor = surfaceData.emission;
//    lightingData.vertexLightingColor = 0;
//    lightingData.mainLightColor = 0;
//    lightingData.additionalLightsColor = 0;
//    lightingData.lightAttenuation = 1;
//    lightingData.giAdditive = 1;
//    lightingData.giMultiplier = 1;
//    lightingData.distanceAttenuationMapOffset = _BaseIndexOfDistanceAttenuationMap;
//
//    return lightingData;
//}
//
////half3 CalculateBlinnPhong(Light light, InputData inputData, SurfaceData  surfaceData)
////{
////    half3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
////    half3 lightDiffuseColor = LightingLambert(attenuatedLightColor, light.direction, inputData.normalWS);
////
////    half3 lightSpecularColor = half3(0,0,0);
////    #if defined(_SPECGLOSSMAP) || defined(_SPECULAR_COLOR)
////    half smoothness = exp2(10 * surfaceData.smoothness + 1);
////
////    lightSpecularColor += LightingSpecular(attenuatedLightColor, light.direction, inputData.normalWS, inputData.viewDirectionWS, half4(surfaceData.specular, 1), smoothness);
////    #endif
////
////#if _ALPHAPREMULTIPLY_ON
////    return lightDiffuseColor * surfaceData.albedo * surfaceData.alpha + lightSpecularColor;
////#else
////    return lightDiffuseColor * surfaceData.albedo + lightSpecularColor;
////#endif
////}
//
/////////////////////////////////////////////////////////////////////////////////
////                      Fragment Functions                                   //
////       Used by ShaderGraph and others builtin renderers                    //
/////////////////////////////////////////////////////////////////////////////////
//
//////////////////////////////////////////////////////////////////////////////////
///// PBR lighting...
//////////////////////////////////////////////////////////////////////////////////
//half4 UniversalFragmentPBR(InputData inputData, SurfaceData surfaceData, half3 shadowed, float4 offsets, half3 shadowColor, half warpMapOffset = 0)
//{
//    float shadowcastOffset = offsets.x;
//    float additionalShadowcastOffset = offsets.y;
//    float giAdditive = offsets.z;
//    float giMultiplier = offsets.w;
//
//    #if defined(_SPECULARHIGHLIGHTS_OFF)
//    bool specularHighlightsOff = true;
//    #else
//    bool specularHighlightsOff = false;
//    #endif
//    BRDFData brdfData;
//
//    // NOTE: can modify "surfaceData"...
//    InitializeBRDFData(surfaceData, brdfData);
//
//    #if defined(DEBUG_DISPLAY)
//    half4 debugColor;
//
//    if (CanDebugOverrideOutputColor(inputData, surfaceData, brdfData, debugColor))
//    {
//        return debugColor;
//    }
//    #endif
//
//    // Clear-coat calculation...
//    BRDFData brdfDataClearCoat = CreateClearCoatBRDFData(surfaceData, brdfData);
//    half4 shadowMask = CalculateShadowMask(inputData);
//    AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(inputData, surfaceData);
//    uint meshRenderingLayers = GetMeshRenderingLayer();
//    Light mainLight = GetMainLight(inputData, shadowMask, aoFactor, shadowcastOffset);
//
//    // NOTE: We don't apply AO to the GI here because it's done in the lighting calculation below...
//    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI);
//
//    StylizedLightingData lightingData = CreateStylizedLightingData(inputData, surfaceData);
//    lightingData.giAdditive = giAdditive;
//    lightingData.giMultiplier = giMultiplier;
//
//    lightingData.giColor = GlobalIllumination(brdfData, brdfDataClearCoat, surfaceData.clearCoatMask,
//                                              inputData.bakedGI, aoFactor.indirectAmbientOcclusion, inputData.positionWS,
//                                              inputData.normalWS, inputData.viewDirectionWS, inputData.normalizedScreenSpaceUV);
//#ifdef _LIGHT_LAYERS
//    if (IsMatchingLightLayer(mainLight.layerMask, meshRenderingLayers))
//#endif
//    {
//        lightingData.mainLightColor = LightingStylizedBased(brdfData, brdfDataClearCoat,
//                                                              mainLight,
//                                                              inputData.normalWS, inputData.viewDirectionWS,
//                                                              surfaceData.clearCoatMask, specularHighlightsOff, _BaseIndexOfDistanceAttenuationMap, warpMapOffset);
//    }
//
//    #if defined(_ADDITIONAL_LIGHTS)
//    uint pixelLightCount = GetAdditionalLightsCount();
//
//    #if USE_FORWARD_PLUS
//    for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
//    {
//        FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK
//
//        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor, additionalShadowcastOffset);
//
//#ifdef _LIGHT_LAYERS
//        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
//#endif
//        {
//            lightingData.additionalLightsColor += LightingStylizedBased(brdfData, brdfDataClearCoat, light,
//                                                                          inputData.normalWS, inputData.viewDirectionWS,
//                                                                          surfaceData.clearCoatMask, specularHighlightsOff,  GetAdditionalLightDistanceAttenuationOffset(lightIndex), warpMapOffset);
//        }
//    }
//    #endif
//
//    LIGHT_LOOP_BEGIN(pixelLightCount)
//        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor, additionalShadowcastOffset);
//
//#ifdef _LIGHT_LAYERS
//        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
//#endif
//        {
//            lightingData.additionalLightsColor += LightingStylizedBased(brdfData, brdfDataClearCoat, light,
//                                                                          inputData.normalWS, inputData.viewDirectionWS,
//                                                                          surfaceData.clearCoatMask, specularHighlightsOff, GetAdditionalLightDistanceAttenuationOffset(lightIndex), warpMapOffset);
//        }
//    LIGHT_LOOP_END
//    #endif
//
//    #if defined(_ADDITIONAL_LIGHTS_VERTEX)
//    lightingData.vertexLightingColor += inputData.vertexLighting * brdfData.diffuse;
//    #endif
//
//#if REAL_IS_HALF
//    // Clamp any half.inf+ to HALF_MAX
//    return min(CalculateFinalColor(lightingData, surfaceData.albedo, shadowColor, surfaceData.alpha, inputData.fogCoord, shadowed), HALF_MAX);
//#else
//    return CalculateFinalColor(lightingData, surfaceData.albedo, shadowColor, surfaceData.alpha, inputData.fogCoord, shadowed);
//#endif
//}
//
//// Deprecated: Use the version which takes "SurfaceData " instead of passing all of these arguments...
//half4 UniversalFragmentPBR(InputData inputData, half3 albedo, half metallic, half3 specular,
//    half smoothness, half occlusion, half3 emission, half alpha, float4 offsets, half3 shadowed, half3 shadowColor)
//{
//    SurfaceData  surfaceData;
//
//    surfaceData.albedo = albedo;
//    surfaceData.specular = specular;
//    surfaceData.metallic = metallic;
//    surfaceData.smoothness = smoothness;
//    surfaceData.normalTS = half3(0, 0, 1);
//    surfaceData.emission = emission;
//    surfaceData.occlusion = occlusion;
//    surfaceData.alpha = alpha;
//    surfaceData.clearCoatMask = 0;
//    surfaceData.clearCoatSmoothness = 1;
//
//    /*AdditionalSurfaceData additionalData;
//
//    additionalData.shadowed = shadowed;*/
//
//    return UniversalFragmentPBR(inputData, surfaceData, shadowed, offsets, shadowColor);
//}
//
//////////////////////////////////////////////////////////////////////////////////
///// Phong lighting...
//////////////////////////////////////////////////////////////////////////////////
//half4 UniversalFragmentBlinnPhong(InputData inputData, SurfaceData  surfaceData, float4 offsets)
//{
//    float shadowcastOffset = offsets.x;
//    float additionalShadowcastOffset = offsets.y;
//    float giAdditive = offsets.z;
//    float giMultiplier = offsets.w;
//
//    #if defined(DEBUG_DISPLAY)
//    half4 debugColor;
//
//    if (CanDebugOverrideOutputColor(inputData, surfaceData, debugColor))
//    {
//        return debugColor;
//    }
//    #endif
//
//    uint meshRenderingLayers = GetMeshRenderingLayer();
//    half4 shadowMask = CalculateShadowMask(inputData);
//    AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(inputData, surfaceData);
//    Light mainLight = GetMainLight(inputData, shadowMask, aoFactor, shadowcastOffset);
//
//    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, aoFactor);
//
//    inputData.bakedGI *= surfaceData.albedo;
//
//    StylizedLightingData lightingData = CreateStylizedLightingData(inputData, surfaceData);//LightingData lightingData = CreateLightingData(inputData, surfaceData);
//#ifdef _LIGHT_LAYERS
//    if (IsMatchingLightLayer(mainLight.layerMask, meshRenderingLayers))
//#endif
//    {
//        lightingData.mainLightColor.rgb += CalculateBlinnPhong(mainLight, inputData, surfaceData);
//    }
//
//    #if defined(_ADDITIONAL_LIGHTS)
//    uint pixelLightCount = GetAdditionalLightsCount();
//
//    #if USE_FORWARD_PLUS
//    for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
//    {
//        FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK
//
//        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor, additionalShadowcastOffset);
//#ifdef _LIGHT_LAYERS
//        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
//#endif
//        {
//            lightingData.additionalLightsColor.rgb += CalculateBlinnPhong(light, inputData, surfaceData);
//        }
//    }
//    #endif
//
//    LIGHT_LOOP_BEGIN(pixelLightCount)
//        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor, additionalShadowcastOffset);
//#ifdef _LIGHT_LAYERS
//        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
//#endif
//        {
//            lightingData.additionalLightsColor.rgb += CalculateBlinnPhong(light, inputData, surfaceData);
//        }
//    LIGHT_LOOP_END
//    #endif
//
//    #if defined(_ADDITIONAL_LIGHTS_VERTEX)
//    lightingData.vertexLightingColor += inputData.vertexLighting * surfaceData.albedo;
//    #endif
//
//    return CalculateFinalColor(lightingData, surfaceData.alpha, 0);
//}
//
//// Deprecated: Use the version which takes "SurfaceData " instead of passing all of these arguments...
//half4 UniversalFragmentBlinnPhong(InputData inputData, half3 diffuse, half4 specularGloss, half smoothness, half3 emission, half alpha, half3 normalTS, float4 offsets)
//{
//    SurfaceData  surfaceData;
//
//    surfaceData.albedo = diffuse;
//    surfaceData.alpha = alpha;
//    surfaceData.emission = emission;
//    surfaceData.metallic = 0;
//    surfaceData.occlusion = 1;
//    surfaceData.smoothness = smoothness;
//    surfaceData.specular = specularGloss.rgb;
//    surfaceData.clearCoatMask = 0;
//    surfaceData.clearCoatSmoothness = 1;
//    surfaceData.normalTS = normalTS;
//
//    return UniversalFragmentBlinnPhong(inputData, surfaceData, offsets);
//}
//
//////////////////////////////////////////////////////////////////////////////////
///// Unlit
//////////////////////////////////////////////////////////////////////////////////
//half4 UniversalFragmentBakedLit(InputData inputData, SurfaceData  surfaceData, half3 shadowColor)
//{
//    #if defined(DEBUG_DISPLAY)
//    half4 debugColor;
//
//    if (CanDebugOverrideOutputColor(inputData, surfaceData, debugColor))
//    {
//        return debugColor;
//    }
//    #endif
//
//    AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(inputData, surfaceData);
//    StylizedLightingData lightingData = CreateStylizedLightingData(inputData, surfaceData);//LightingData lightingData = CreateLightingData(inputData, surfaceData);
//
//    if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_AMBIENT_OCCLUSION))
//    {
//        lightingData.giColor *= aoFactor.indirectAmbientOcclusion;
//    }
//
//    return CalculateFinalColor(lightingData, surfaceData.albedo, shadowColor, surfaceData.alpha, inputData.fogCoord, 1);
//}
//
//// Deprecated: Use the version which takes "SurfaceData " instead of passing all of these arguments...
//half4 UniversalFragmentBakedLit(InputData inputData, half3 color, half alpha, half3 normalTS, half3 shadowColor)
//{
//    SurfaceData  surfaceData;
//
//    surfaceData.albedo = color;
//    surfaceData.alpha = alpha;
//    surfaceData.emission = half3(0, 0, 0);
//    surfaceData.metallic = 0;
//    surfaceData.occlusion = 1;
//    surfaceData.smoothness = 1;
//    surfaceData.specular = half3(0, 0, 0);
//    surfaceData.clearCoatMask = 0;
//    surfaceData.clearCoatSmoothness = 1;
//    surfaceData.normalTS = normalTS;
//
//    return UniversalFragmentBakedLit(inputData, surfaceData, shadowColor);
//}
//
#endif
