Shader "Beast/Radar/Terrain/BiomeRender"
{
    SubShader
    {
        // Rendered to buffer.
        cull off
        zwrite off
        ztest off

        Pass
        {
            CGPROGRAM

            #pragma multi_compile NOISE_SAMPLER_LOD_0 NOISE_SAMPLER_LOD_1

            #pragma target 2.0
            
            #pragma vertex vertex
            #pragma fragment fragment

            #include "UnityCG.cginc"
            #include "Assets/Source/Radar/Graphic/Radar.cginc"

            struct VertexInput
            {
                float4 positionOS : POSITION;
                float2 quadUV : TEXCOORD0;
            };

            struct FragmentInput
            {
                float4 positionCS : SV_POSITION;
                float2 mainUV : TEXCOORD0;
                float2 noiseUV : TEXCOORD1;
            };

            sampler2D _TerrainTexture;
            float2 _TerrainTextureSize;

            sampler2D _NoiseMap;
            float2 _NoiseMapSize;

            sampler2D _IndexMap;

            sampler2D _DebugGridTexture;
            float2 _DebugGridTextureSize;
            
            float2 _OutputSize;
            float2 _OutputCenter;

            float _NoiseSamplerLOD;
            
            #define NOISE_POWER_0 float2(1, 1)
            #define NOISE_SCALE_0 16.0

            #define NOISE_POWER_1 float2(0.15, 0.15)
            #define NOISE_SCALE_1 64.0

            float2 screen_to_world_uv(float2 uv, float2 mapSize)
            {
                return uv * (_OutputSize.y / mapSize) + _OutputCenter / mapSize;
            }

            FragmentInput vertex(VertexInput input)
            {
                FragmentInput output;
                
                output.positionCS = UnityObjectToClipPos(input.positionOS);

                float2 clipUV = beast_quad_to_clip_uv(input.quadUV, _OutputSize);

                output.mainUV = screen_to_world_uv(clipUV, _TerrainTextureSize);
                output.noiseUV = screen_to_world_uv(clipUV, _NoiseMapSize);

                return output;
            }

            float2 sample_noise(float2 noiseUV)
            {
                float2 noiseRG = tex2D(_NoiseMap, noiseUV).rg;
                float2 noise = beast_unpack_normal_2d(noiseRG);

                return noise / _TerrainTextureSize;
            }

            float4 noise_shift(float2 mainUV, float2 noiseUV, float noisePower)
            {
                float2 noise0 = sample_noise(noiseUV * NOISE_SCALE_0) * NOISE_POWER_0;
                float2 noise1 = sample_noise(noiseUV * NOISE_SCALE_1) * NOISE_POWER_1;
                
                mainUV = mainUV + (noise0 + noise1) * noisePower * 1;
                
                return tex2D(_TerrainTexture, mainUV);
            }

            float4 fragment(FragmentInput input) : SV_Target
            {
#ifdef NOISE_SAMPLER_LOD_0
                float lodValue = 1 - _NoiseSamplerLOD;

                float4 terrainColorRGBA = noise_shift(input.mainUV, input.noiseUV, lodValue);

#endif

#ifdef NOISE_SAMPLER_LOD_1
                float4 terrainColorRGBA = tex2D(_TerrainTexture, input.mainUV);
#endif

                float2 indexMapUV = terrainColorRGBA.rg;

                float indexColorR = tex2D(_IndexMap, indexMapUV).r;

                // In future we also have to process blending colors.
                uint tileIndex = (uint)floor(indexColorR * 255.0f + 0.5f);

                return float4(f16tof32(tileIndex), 0, 0, 0);
            }
            
            ENDCG
        }
    }
}