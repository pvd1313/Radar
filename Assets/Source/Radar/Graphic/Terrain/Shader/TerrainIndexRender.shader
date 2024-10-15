Shader "Beast/Radar/TerrainIndexRender"
{
    Properties
    {
        
    }
    SubShader
    {
        cull off
        zwrite off
        ztest off

        Pass
        {
            CGPROGRAM

            #pragma target 2.0
            
            #pragma vertex vertex
            #pragma fragment fragment

            #include "UnityCG.cginc"
            #include "Assets/Source/Radar/Graphic/Main/RadarCG.cginc"

            struct VertexInput
            {
                float4 positionOS : POSITION;
                float2 screenUV : TEXCOORD0;
            };

            struct FragmentInput
            {
                float2 mainUV : TEXCOORD0;
                float2 noiseUV : TEXCOORD1;
                float4 positionCS : SV_POSITION;
            };

            sampler2D _TerrainTexture;
            float2 _TerrainTextureSize;

            sampler2D _NoiseMap;
            float2 _NoiseMapSize;

            sampler2D _DebugGridTexture;
            float2 _DebugGridTextureSize;
            
            float _OutputAspect;
            float _OutputHeight;
            float2 _OutputCenter;

            #define MAP_RENDER_PPU 100;
            #define NOISE_OFFSET_0 float2(8, 8)
            #define NOISE_POWER_0 float2(0.3, 0.2)
            #define NOISE_OFFSET_1 float2(16, 16)
            #define NOISE_POWER_1 float2(0.1, 0.075)

            float2 screen_to_world_uv(float2 uv, float2 mapSize)
            {
                return uv * (_OutputHeight / mapSize) + _OutputCenter / mapSize;
            }

            FragmentInput vertex(VertexInput input)
            {
                FragmentInput output;
                
                output.positionCS = UnityObjectToClipPos(input.positionOS);

                float2 uv = (input.screenUV - 0.5) * float2(_OutputAspect, 1);

                output.mainUV = screen_to_world_uv(uv, _TerrainTextureSize);
                
                output.noiseUV = screen_to_world_uv(uv, _NoiseMapSize);

                // Will not work with index mapping. We have to implement new solution for grid.
// #ifdef DRAW_DEBUG_GRID_TEXTURE
//                 output.gridUV = screen_to_world_uv(uv, _DebugGridTextureSize);
//                 output.gridUV *= MAP_RENDER_PPU;
// #endif

                return output;
            }

            float4 noise_shift(float2 mainUV, float2 noiseUV, float2 offset, float2 power)
            {
                float2 noiseRG = tex2D(_NoiseMap, noiseUV * offset).rg;
                float2 noise = beast_unpack_normal_2d(noiseRG) * power;
                mainUV = mainUV + noise * offset / _TerrainTextureSize;
                
                return tex2D(_TerrainTexture, mainUV).r;
            }

            // There will be more complex or index map texture in future.
            uint terrain_color_to_tile_index(float terrainColor)
            {
                // 0 .. 1 just for now. 0 - grass. 1 - water.
                return saturate(1.0f - terrainColor);
            }

            float4 fragment(FragmentInput input) : SV_Target
            {
                float4 color0 = noise_shift(input.mainUV, input.noiseUV, NOISE_OFFSET_0, NOISE_POWER_0);
                float4 color1 = noise_shift(input.mainUV, input.noiseUV * 1.5, NOISE_OFFSET_1, NOISE_POWER_1);

                float4 color = saturate((color0 + color1) * 900);

                uint tileIndex = terrain_color_to_tile_index(color.r);

                return float4(f16tof32(tileIndex), 0, 0, 0);

                // Will not work with index mapping. We have to implement new solution for grid.
// #ifdef DRAW_DEBUG_GRID_TEXTURE
//                 float gridColor = tex2D(_DebugGridTexture, input.gridUV).r;
//                 float gridTexelSize = _OutputHeight / _DebugGridTextureSize.y;
//                 color = lerp(color, gridColor, 0.25 * saturate(1 - gridTexelSize));
// #endif
            }
            
            ENDCG
        }
    }
}