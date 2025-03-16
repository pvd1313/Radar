#ifndef RADAR_SHADER_GRAPH_CG_INCLUDE
#define RADAR_SHADER_GRAPH_CG_INCLUDE

#include "Assets/Source/Radar/Graphic/Radar.cginc"

void shadergraph_rect_float(in float2 UV,
                     in float4 RGBA,
                     in float4 RectColor,
                     in float2 LineWidth,
                     out float4 Out)
{
    if (UV.x < LineWidth.x ||
        UV.y < LineWidth.y ||
        UV.x > 1 - LineWidth.x ||
        UV.y > 1 - LineWidth.y)
    {
        Out = RectColor;
    }
    else
    {
        Out = RGBA;
    }
}

void shadergraph_hash_float(in float2 UV,
                      in float seed,
                      out float2 RG)
{
    RG = beast_int_hash(UV, seed);
}


void shadergraph_noise_float(in float2 UV,
                      in float seed,
                      out float R)
{
    R = beast_noise(UV, seed);
}

void shadergraph_fbm_float(in float2 UV,
                    in float Turbulence,
                    in float Quality,
                    in float Seed,
                    out float R)
{
    R = beast_fbm(UV, Turbulence, Quality, Seed);
}

#endif