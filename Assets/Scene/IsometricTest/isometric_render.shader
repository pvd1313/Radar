Shader "Beast/IsometricRender"
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
            #include "Assets/Source/Claw/CG/BeastCG.cginc"

            
            // #define DEBUG_DRAW_TILES_ONE_SECOND

            struct FragmentInput
            {
                float4 positionCS : SV_POSITION;
                float2 tileUV : TEXCOORD0;
                float2 noiseUV : TEXCOORD1;
            };

            // Rename to tile grid size
            float2 _GridSize;
            
            sampler2D _TileAtlas;
            float2 _TileAtlasSize;
            float2 _TileCenter;
            float _TileAtlasPPU;

            // Rename to terrain
            sampler2D _WorldTexture;
            float2 _WorldTextureSize;
            float2 _WorldTextureOffset;

            // Rename to output
            float2 _RenderSize;
            float2 _RenderOffset01;
            float _RenderTileHeight;
            
            sampler2D _NoiseTexture;
            float2 _NoiseTextureSize;
            float2 _NoiseTextureOffset;

            float4 _LayoutMatrix;

            // GRID 3x2:
            // 0 1 2
            //  3 4
            // 5 6 7
            FragmentInput vertex(appdata_base input, uint instanceID : SV_InstanceID)
            {
                FragmentInput output;

#ifdef DEBUG_DRAW_TILES_ONE_SECOND
                if (instanceID > ((_GridSize.x * _GridSize.y) + (_GridSize.x - 1) * (_GridSize.y - 1)) * frac(_Time.y))
                {
                    output.positionCS = 0;
                    return output;
                }
#endif
                
                float aspect = _RenderSize.x / _RenderSize.y;

                float2 gridScale = float2(_RenderTileHeight, _RenderTileHeight) * 2.0f;
                gridScale.x *= 1.0f / (aspect * 0.5f);

                input.vertex *= _RenderTileHeight * 4.0f;
                input.vertex.x /= aspect;

                input.vertex.x += _RenderOffset01.x * 2.0f - 1.0f;
                input.vertex.y -= _RenderOffset01.y * 2.0f - _RenderTileHeight - 1.0f;

                // Fix transparent lines glitch.
                // input.vertex.y += sign(input.vertex.y) * (1 / _RenderSize.y);
                // input.vertex.x += sign(input.vertex.x) * (1 / _RenderSize.x);

                BeastIsometricTile tile = beast_isometric_tile(instanceID, _GridSize.x);
                float2 screenOffset = beast_isometric_screen_offset(tile, _GridSize.x);
                
                input.vertex.x += screenOffset.x * gridScale.x;
                input.vertex.y += screenOffset.y * gridScale.y;

                input.vertex.y *= _ProjectionParams.x;

                output.noiseUV = instanceID / (_GridSize.x * _GridSize.y + (_GridSize.x - 1) * (_GridSize.y - 1));

                output.tileUV = input.texcoord.xy;
                output.positionCS = float4(input.vertex.x, input.vertex.y, UNITY_NEAR_CLIP_VALUE, 1);

                return output;
            }

            float4 fragment(FragmentInput input) : SV_Target
            {
                float4 tileColor = tex2D(_TileAtlas, input.tileUV);
                
                clip(tileColor.r + tileColor.g + tileColor.b - 0.01);
                
                float4 noiseColor = tex2D(_NoiseTexture, input.noiseUV);
                
                float t = saturate(_TileAtlasPPU / (_RenderSize.y * _RenderTileHeight));
                
                float2 centerUV = lerp(input.tileUV, _TileCenter, t * 0.5 + t * noiseColor.r * 0.5);
                
                float4 centerColor = tex2D(_TileAtlas, centerUV);
                
                tileColor = lerp(tileColor, centerColor, t);
                
                return tileColor;
            }
            
            ENDCG
        }
    }
}