#ifndef UNIVERSAL_CUSTOM_LIGHTING_INCLUDED
#define UNIVERSAL_CUSTOM_LIGHTING_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/CustomizeExample/FogOfWar/FogOfWar.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"


Light CustomizeLight(Light prevLight, InputData inputData)
{
    Light light = prevLight;
    light.shadowAttenuation *= FogOfWars(inputData.positionWS+inputData.normalWS * 0.25f, 0);
    
    return light;
}

#endif
