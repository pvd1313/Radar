#ifndef RADAR_CG_INCLUDE
#define RADAR_CG_INCLUDE

struct BeastIsometricTile
{
    uint mainY;
    uint innerX;
    float isInner01;
};

// 0 1 2
//  3 4
// 5 6 7
inline BeastIsometricTile beast_isometric_tile(uint instanceID, uint gridSizeX)
{
    BeastIsometricTile output;

    uint innerGridSizeX = gridSizeX - 1u;

    output.mainY = floor(instanceID / (gridSizeX + innerGridSizeX));
    output.innerX = instanceID - output.mainY * (innerGridSizeX + gridSizeX);
    output.isInner01 = output.innerX / gridSizeX >= 1u;

    return output;
}

inline float2 beast_quad_to_clip_uv(float2 quadUV, float2 screenSize)
{
    float aspect = screenSize.x / screenSize.y;

    return (quadUV - 0.5f) * float2(aspect, 1);
}

inline float2 beast_isometric_world_offset(BeastIsometricTile tile, uint gridSizeX)
{
    tile.innerX -= gridSizeX * tile.isInner01;

    float2 output = 0;

    // TODO: Pack this.
    if (tile.isInner01)
    {
        output.x -= tile.mainY;
        output.y -= tile.mainY;
        output.x += tile.innerX;
        output.y -= tile.innerX;
        output.y -= 1;
    }
    else
    {
        output.x -= tile.mainY;
        output.y -= tile.mainY;
        output.x += tile.innerX;
        output.y -= tile.innerX;
    }

    return output;
}

inline float beast_dither_cs(float2 positionCS, float alpha)
{
    const float BAYER_MATRIX[16] =
    {
        1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
        13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
        4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
        16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
    };

    int index = int(positionCS.x) % 4 * 4 + int(positionCS.y) % 4;
    
    return alpha - BAYER_MATRIX[index];
}

inline float4 beast_tex2Dlod(sampler2D text, float2 uv, float lod)
{
    float4 uvLod = float4(uv.x, uv.y, 0, lod);
    return tex2Dlod(text, uvLod);
}

float4 beast_half2_to_rgba(half half0, half half1)
{
    uint u0 = f32tof16(half0);
    uint u1 = f32tof16(half1);
    
    float r = (u0 >> 8) & 0xFF;
    float g = u0 & 0xFF;
    float b = (u1 >> 8) & 0xFF;
    float a = u1 & 0xFF;

    return float4(r, g, b, a) / 255.0;
}

float beast_half2_to_float(half half0, half half1)
{
    uint mainUInt = f32tof16(half0);
    
    float mainFloat = mainUInt >= 32767u ? (mainUInt - 32767.0f) * -1.0f : mainUInt;
    
    return mainFloat + (float)half1;
}

inline float2 beast_isometric_screen_offset(BeastIsometricTile tile, uint gridSizeX)
{
    float2 output = 0;

    // TODO: Pack this.
    if (tile.isInner01)
    {
        tile.innerX -= gridSizeX;
        output.y = tile.mainY * -1.0f - 0.5f;
        output.x = tile.innerX + 0.5f;
    }
    else
    {
        output.y = tile.mainY * -1.0f;
        output.x = tile.innerX;
    }

    return output;
}

inline float2 beast_tile_xy_from_index(uint tileIndex, float2 tileCount)
{
    float x = tileIndex % (uint) tileCount.x;
    float y = tileIndex / (uint) tileCount.x;

    return float2(x, y);
}

inline float2 beast_unpack_normal_2d(float2 color)
{
    return (color - 0.5) * 2;
}

// Ported : v.fuchadzhy@risenbeast.com
// Source : https://www.notion.so/ShaderToy-2D-Clouds-10ca45ace92d805ea7abccb3823037d2
//        : https://www.shadertoy.com/view/4tdSWr
float2 beast_hash(float2 uv, float seed)
{
    uv = float2(dot(uv, float2(127.1, 311.7)), dot(uv, float2(269.5, 183.3)));
    return -1.0 + 2.0 * frac(sin(uv * seed) * 43758.5453123);
}

// Ported : v.fuchadzhy@risenbeast.com
// Source : https://www.notion.so/ShaderToy-2D-Clouds-10ca45ace92d805ea7abccb3823037d2
//        : https://www.shadertoy.com/view/4tdSWr
float beast_noise(float2 uv, float seed)
{
    const float K1 = 0.366025404; // (sqrt(3) - 1) / 2;
    const float K2 = 0.211324865; // (3 - sqrt(3)) / 6;
    
    float2 i = floor(uv + (uv.x + uv.y) * K1);	
    float2 a = uv - i + (i.x + i.y) * K2;
    float2 o = (a.x > a.y ) ? float2(1.0, 0.0) : float2(0.0, 1.0);
    float2 b = a - o + K2;
    float2 c = a - 1.0 + 2.0 * K2;
    float3 h = max(float3(0.5, 0.5, 0.5) - float3(dot(a, a), dot(b, b), dot(c, c)), 0.0);
    float3 n = float3(
        dot(a, beast_hash(i, seed)),
        dot(b, beast_hash(i + o, seed)),
        dot(c, beast_hash(i + 1, seed)));
    
    return dot(h * h * h * h * n, 70.0);
}

// Ported : v.fuchadzhy@risenbeast.com
// Source : https://www.notion.so/ShaderToy-2D-Clouds-10ca45ace92d805ea7abccb3823037d2
//        : https://www.shadertoy.com/view/4tdSWr
float beast_fbm(float2 uv,
                float turbulence,
                float quality,
                float seed)
{
    const float2x2 trans = float2x2(1.6, 1.2, -1.2, 1.6);
    
    float color = 0.0;
    float power = 1.0;
    
    for (int i = 0; i < quality; i++)
    {
        color += beast_noise(uv, seed) * power;
        uv = mul(trans, uv);
        power *= turbulence;
    }

    return color;
}

float beast_rect(float2 uv,
                 float color,
                 float RectColor,
                 float2 LineWidth)
{
    if (uv.x < LineWidth.x ||
        uv.y < LineWidth.y ||
        uv.x > 1 - LineWidth.x ||
        uv.y > 1 - LineWidth.y)
    {
        return  RectColor;
    }
    
    return color;
}

// half beast_encode_RG(float r, float g)
// {
//     return (half)(r * 255.0) + (half)(g * 255.0 / 256.0);
// }
//
// float2 beast_decode_RG(half x)
// {
//     float a = frac(x) * 255.0 / 255.0;
//     float b = floor(x * 256.0) / 255.0;
//     
//     return float2(b, a); 
// }
//
// // float2 beast_decode_RG(half x)
// // {
// //     return float2(floor(x) / 255.0, (x - floor(x)) * 256.0 / 255.0); 
// // }
//
// void shadergraph_encode_RG_float(in float R, in float G, out half Result)
// {
//     Result = beast_encode_RG(R, G);
// }
//
// void shadergraph_decode_RG_float(in half X, out float2 Result)
// {
//     Result = beast_decode_RG(X);
// }

void shadergraph_epsilon_step_float(in float X, out float Y)
{
    float epsilon = 1e-6;
    float almostEpsilon = step(epsilon, abs(X));

    Y = X + (1 - almostEpsilon) * epsilon + (1 - almostEpsilon) * sign(X);
}

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