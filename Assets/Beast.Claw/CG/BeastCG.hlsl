#ifndef BEAST_CG_INCLUDE
#define BEAST_CG_INCLUDE

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

void BeastNoise_float(in float2 UV,
                      in float seed,
                      out float R)
{
    R = beast_noise(UV, seed);
}

void BeastFBM_float(in float2 UV,
                    in float Turbulence,
                    in float Quality,
                    in float Seed,
                    out float R)
{
    R = beast_fbm(UV, Turbulence, Quality, Seed);
}

#endif