#ifndef RADAR_CG_INCLUDE
#define RADAR_CG_INCLUDE

struct BeastIsometricTile
{
    uint mainY;
    uint innerX;
    float isInner01;
};

struct BeastWorld2ClipMatrix
{
    float4 viewMatrix0;
    float4 viewMatrix1;
    float projectionScale;
    float2 renderCenter;
    float renderDistance;
    float2 screenSize;
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

    uint x = uint(positionCS.x) % 4U;
    uint y = uint(positionCS.y) % 4U;
    uint index = y * 4U + x;
    
    return alpha - BAYER_MATRIX[index];
}

inline float4 beast_tex2Dlod(sampler2D text, float2 uv, float lod)
{
    float4 uvLod = float4(uv.x, uv.y, 0, lod);
    return tex2Dlod(text, uvLod);
}

inline void beast_get_data_vector(
    sampler2D dataTexture,
    float2 dataTextureSize,
    uint index,
    out half h0,
    out half h1,
    out half h2,
    out half h3)
{
    float2 texelSize = 1.0 / dataTextureSize;
                
    uint x = index % (uint) dataTextureSize.x;
    uint y = index / (uint) dataTextureSize.x;
                
    float2 uv = float2(x, y) * texelSize + texelSize * 0.5;

    half4 color = beast_tex2Dlod(dataTexture, uv, 0);

    h0 = color.r;
    h1 = color.g;
    h2 = color.b;
    h3 = color.a;
}

float4 beast_half2_to_rgba(half half0, half half1)
{
    float u0 = (float)f32tof16(half0);
    float u1 = (float)f32tof16(half1);
    
    float r = floor(u0 / 128.0);
    float g = u0 - (r * 128.0);
    float b = floor(u1 / 128.0);
    float a = u1 - (b * 128.0);
    return float4(r, g, b, a) / 127.0;
}

inline void beast_clip_black_color(float3 color)
{
    clip(color.r + color.g + color.b - 0.01);
}

float4 beast_world_to_clip_pos(BeastWorld2ClipMatrix mat, float2 worldPos, float2 screenOffset)
{
    float2 screenTexelSize = 1.0 / mat.screenSize;
    
    float aspect = mat.screenSize.x / mat.screenSize.y;

    worldPos = worldPos - mat.renderCenter;
                
    worldPos = mul(float2x2(mat.viewMatrix0), worldPos);
    worldPos = mul(float2x2(mat.viewMatrix1), worldPos);
                
    worldPos /= mat.renderDistance;
    worldPos.x /= aspect;
    worldPos *= mat.projectionScale;

    worldPos += screenTexelSize * screenOffset * 2;

    worldPos.y *= _ProjectionParams.x;
                
    return float4(worldPos.x, worldPos.y, UNITY_NEAR_CLIP_VALUE, 1.0);
}

float beast_half2_to_float(half half0, half half1)
{
    uint mainUInt = f32tof16(half0);
    
    float mainFloat = mainUInt >= 32767U ? (mainUInt - 32767.0) * -1.0 : mainUInt;
    
    return mainFloat + (float)half1;
}

float2 beast_half_to_sbyte2(half value)
{
    uint a = (uint)value;
    uint b = (uint)((value - a) * 128.0 + 0.5);

    float aFloat = a >= 63U ? (a - 63U) * -1.0 : a;
    float bFloat = b >= 63U ? (b - 63U) * -1.0 : b;

    return float2(aFloat, bFloat);
}

float2 beast_half_to_ubyte2(half value)
{
    uint a = (uint)value;
    uint b = (uint)((value - a) * 128.0 + 0.5);

    return float2(a, b);
}

inline float2 beast_isometric_screen_offset(BeastIsometricTile tile, uint gridSizeX)
{
    float2 output = 0;

    // TODO: Pack this.
    if (tile.isInner01)
    {
        tile.innerX -= gridSizeX;
        output.y = tile.mainY * -1.0 - 0.5;
        output.x = tile.innerX + 0.5;
    }
    else
    {
        output.y = tile.mainY * -1.0;
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

// https://www.shadertoy.com/view/llGSzw
float beast_hugo_hash(uint n) 
{
    // integer hash copied from Hugo Elias
    n = (n << 13U) ^ n;
    n = n * (n * n * 15731U + 789221U) + 1376312589U;
    return float(n & uint(0x7fffffffU)) / float(0x7fffffffU);
}

float2 beast_int_hash(float2 uv, float seed)
{
    const uint prime0 = 73856093U;
    const uint prime1 = 19349663U;
    
    uint a = (asuint(uv.x) * prime0) ^ (asuint(uv.y) * prime1) + asuint(seed);
    uint b = (asuint(uv.x) * prime1) ^ (asuint(uv.y) * prime0) + asuint(seed);
    
    return -1.0 + 2.0 * float2(beast_hugo_hash(a), beast_hugo_hash(b));
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
        dot(a, beast_int_hash(i, seed)),
        dot(b, beast_int_hash(i + o, seed)),
        dot(c, beast_int_hash(i + 1, seed)));
    
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

#endif