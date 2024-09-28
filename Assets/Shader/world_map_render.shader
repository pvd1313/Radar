Shader "Beast/Radar/WorldMapRenderShader"
{
    Properties
    {
        
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
        }
        
        LOD 100

        Pass
        {
            CGPROGRAM

            #pragma target 2.0
            
            #pragma vertex vertex
            #pragma fragment fragment

            #include "UnityCG.cginc"

            #define DEBUG_GRID_MAP

            struct VertexInput
            {
                float4 positionOS : POSITION;
                float2 screenUV : TEXCOORD0;
            };

            struct FragmentInput
            {
                float2 mainUV : TEXCOORD0;
                float2 noiseUV : TEXCOORD1;

#ifdef DEBUG_GRID_MAP
                float2 gridUV : TEXCOORD2;
#endif
                
                float4 positionCS : SV_POSITION;
            };

            sampler2D _MainMap;
            float2 _MainMapSize;

            sampler2D _NoiseMap;
            float2 _NoiseMapSize;

            sampler2D _GridMap;
            float2 _GridMapSize;
            
            float _RenderAspect;
            float _RenderSizeY;
            float2 _RenderCenter;

            #define MAP_RENDER_PPU 100;
            #define NOISE_OFFSET_0 float2(8, 8)
            #define NOISE_POWER_0 float2(0.3, 0.2)
            #define NOISE_OFFSET_1 float2(16, 16)
            #define NOISE_POWER_1 float2(0.1, 0.075)
            
            inline float2 unpack_normal_2d(float2 color)
            {
                return (color - 0.5) * 2;
            }

            float2 screen_to_world_uv(float2 uv, float2 mapSize)
            {
                return uv * (_RenderSizeY / mapSize) + _RenderCenter / mapSize;
            }

            FragmentInput vertex(VertexInput input)
            {
                FragmentInput output;
                
                output.positionCS = UnityObjectToClipPos(input.positionOS);

                float2 uv = (input.screenUV - 0.5) * float2(_RenderAspect, 1);

                output.mainUV = screen_to_world_uv(uv, _MainMapSize);
                output.noiseUV = screen_to_world_uv(uv, _NoiseMapSize);

#ifdef DEBUG_GRID_MAP
                output.gridUV = screen_to_world_uv(uv, _GridMapSize);
                output.gridUV *= MAP_RENDER_PPU;
#endif

                return output;
            }

            float4 noise_shift(float2 mainUV, float2 noiseUV, float2 offset, float2 power)
            {
                float2 noiseRG = tex2D(_NoiseMap, noiseUV * offset).rg;
                float2 noise = unpack_normal_2d(noiseRG) * power;
                mainUV = mainUV + noise * offset / _MainMapSize;
                
                return tex2D(_MainMap, mainUV).r;
            }

            float4 fragment(FragmentInput input) : SV_Target
            {
                float4 color0 = noise_shift(input.mainUV, input.noiseUV, NOISE_OFFSET_0, NOISE_POWER_0);
                float4 color1 = noise_shift(input.mainUV, input.noiseUV * 1.5, NOISE_OFFSET_1, NOISE_POWER_1);

                float4 color = saturate((color0 + color1) * 90);

#ifdef DEBUG_GRID_MAP
                float gridColor = tex2D(_GridMap, input.gridUV).r;
                float gridTexelSize = _RenderSizeY / _GridMapSize.y;
                color = lerp(color, gridColor, 0.25 * saturate(1 - gridTexelSize));
#endif
                
                return color;
            }
            
            ENDCG
        }
    }
}