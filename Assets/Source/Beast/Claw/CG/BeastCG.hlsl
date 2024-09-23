// Ported : v.fuchadzhy@risenbeast.com
// Source : https://www.shadertoy.com/view/4tdSWr

#ifndef BEAST_CG_INCLUDE
#define BEAST_CG_INCLUDE

// float2 toymul(float2x2 m, float x, float y)
// {
//     return float2(m._m00 * x + m._m10 * y, m._m01 * x + m._m11 * y);
// }
//
// float2 toymul(float2x2 m, float2 v)
// {
//     return toymul(m, v.x, v.y);
// }

float2 beast_hash(float2 uv, float seed)
{
    uv = float2(dot(uv, float2(127.1, 311.7)), dot(uv, float2(269.5, 183.3)));
    return -1.0 + 2.0 * frac(sin(uv * seed) * 43758.5453123);
}

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

void BeastFBM_float(in float2 UV,
                    in float Turbulence,
                    in float Quality,
                    in float Seed,
                    out float R)
{
    const float2x2 trans = float2x2(1.6, 1.2, -1.2, 1.6);
    
    float color = 0.0;
    float power = 1.0;
    
    for (int i = 0; i < Quality; i++)
    {
        color += beast_noise(UV, Seed) * power;
        UV = mul(trans, UV);
        power *= Turbulence;
    }

    R = color;
}

// void Cloud_float(in float2 UV,
//             in float Time,
//             in UnityTexture2D Texture,
//             out float4 RGBA)
// {
//     RGBA = 0;
//     
//     //float2 p = fragCoord.xy / iResolution.xy;
//     //loat2 uv = p * float2 (iResolution.x / iResolution.y, 1.0);    
//     // float time = Time * speed;
//     float time = speed;
//     float2 uv = UV;
//     float q = fbm(UV * cloudscale * float2(0.5, 0.5));
//
//     // RGBA.rg = UV;
//     // return;
//
//     
//     RGBA.r = fbm(UV) * 100;
//     return;
//     
//     // Ridged noise shape.
//     float r = 0.0;
//     uv *= cloudscale;
//     uv -= q - time;
//     float weight = 0.8;
//     for (int i = 0; i < 8; i++)
//     {
//         r += abs(weight * noise(uv));
//         uv = toymul(m, uv) + time;
//         weight *= 0.7;
//     }
//
//     
//     RGBA.r = min(1.0, fbm(float2(noise(UV), noise(UV))) * 5.0);
//
//     //RGBA.rg = uv;
//     return;
//     
//     RGBA = 0;
//     RGBA.r = hash(float2(1, 1)).r;
//
//     return;
//     
//     // Noise shape.
//     float f = 0.0;
//     uv = UV;
//     uv *= cloudscale;
//     uv -= q - time;
//     weight = 0.7;
//     for (int i= 0; i < 8; i++)
//     {
//         f += weight * noise( uv );
//         uv = mul(m, uv) + time;
//         weight *= 0.6;
//     }
//     
//     f *= r + f;
//     
//     // Noise colour.
//     float c = 0.0;
//     time = Time * speed * 2.0;
//     uv = UV;
//     uv *= cloudscale * 2.0;
//     uv -= q - time;
//     weight = 0.4;
//     for (int i = 0; i < 7; i++)
//     {
//         c += weight * noise( uv );
//         uv = mul(m, uv) + time;
//         weight *= 0.6;
//     }
//     
//     // Noise ridge colour.
//     float c1 = 0.0;
//     time = Time * speed * 3.0;
//     uv = UV;
//     uv *= cloudscale * 3.0;
//     uv -= q - time;
//     weight = 0.4;
//     for (int i = 0; i < 7; i++)
//     {
//         c1 += abs(weight * noise( uv ));
//         uv = mul(m, uv) + time;
//         weight *= 0.6;
//     }
// 	
//     c += c1;
//     
//     // float3 skyColour = lerp(skycolour2, skycolour1, p.y);
//     float3 skyColour = lerp(skycolour2, skycolour1, 0);
//     float3 cloudColour = float3(1.1, 1.1, 0.9) * clamp(clouddark + cloudlight * c, 0.0, 1.0);
//    
//     f = cloudcover + cloudalpha * f * r;
//     
//     float3 result = lerp(skyColour, clamp(skytint * skyColour + cloudColour, 0.0, 1.0), clamp(f + c, 0.0, 1.0));
//     
//     RGBA = float4(result, 1.0);
// }

#endif