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

            #define WORLD_MAP_RENDER_GRID

            struct VertexInput
            {
                float4 positionOS : POSITION;
                float2 screenUV : TEXCOORD0;
            };

            struct FragmentInput
            {
                float2 mainUV : TEXCOORD0;
                float2 noiseUV : TEXCOORD1;

#ifdef  WORLD_MAP_RENDER_GRID
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

            #define BEAST_WORLD_MAP_RENDER_PPU 100;
            #define BEAST_WORLD_MAP_RENDER_NOISE_SHIFT float2(8, 8)
            #define BEAST_WORLD_MAP_RENDER_NOISE_POWER float2(0.3, 0.2)
            
            inline float2 unpack_normal_2d(float2 color)
            {
                return (color - 0.5) * 2;
            }

            FragmentInput vertex(VertexInput input)
            {
                FragmentInput output;
                
                output.positionCS = UnityObjectToClipPos(input.positionOS);

                float2 uv = (input.screenUV - 0.5) * float2(_RenderAspect, 1);

                output.mainUV = uv * (_RenderSizeY / _MainMapSize) + _RenderCenter / _MainMapSize;
                output.noiseUV = uv * (_RenderSizeY / _NoiseMapSize) + _RenderCenter / _NoiseMapSize;

#ifdef WORLD_MAP_RENDER_GRID
                output.gridUV = uv * (_RenderSizeY / _GridMapSize) + _RenderCenter / _GridMapSize;
                output.gridUV *= BEAST_WORLD_MAP_RENDER_PPU;
#endif

                return output;
            }

            float4 fragment(FragmentInput input) : SV_Target
            {
                float2 NOISE_SHIFT = BEAST_WORLD_MAP_RENDER_NOISE_SHIFT;
                float2 NOISE_POWER = BEAST_WORLD_MAP_RENDER_NOISE_POWER;
                
                float2 noiseRG = tex2D(_NoiseMap, input.noiseUV * NOISE_SHIFT).rg;
                float2 noise = unpack_normal_2d(noiseRG) * NOISE_POWER;

                float2 texelSize = _MainMapSize / _RenderSizeY;
                input.mainUV = input.mainUV + noise * NOISE_SHIFT / _MainMapSize;
                input.mainUV = float2 (input.mainUV / texelSize) * texelSize;
                
                float mainColor = tex2D(_MainMap, input.mainUV).r;

#ifdef WORLD_MAP_RENDER_GRID
                float gridColor = tex2D(_GridMap, input.gridUV).r;
                float gridTexelSize = _RenderSizeY / _GridMapSize.y;
                mainColor = lerp(mainColor, gridColor, 0.25 * saturate(1 - gridTexelSize));
#endif
                
                return mainColor;
            }
            
            ENDCG
        }
    }
}