#ifndef UNIVERSAL_FOW_CUSTOM_CLIPPING_INCLUDED
#define UNIVERSAL_FOW_CUSTOM_CLIPPING_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Dither.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/CustomizeExample/FogOfWar/FogOfWar.hlsl"

float CustomClipping(InputData inputData, SurfaceData surfaceData, float4 color, float2 screenPos)
{
    float value = FogOfWars(inputData.positionWS + inputData.normalWS * 0.25f);
    value = value - (0.01f * 1 - value);
    clip(Dithering(value, screenPos));
    return value;
}

#endif
