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

            // #define DEBUG_DRAW_TILES_ONE_SECOND

            struct FragmentInput
            {
                float4 positionCS : SV_POSITION;
                float2 tileUV : TEXCOORD0;
                float2 noiseUV : TEXCOORD1;
            };

            float2 _GridSize;
            
            sampler2D _TileAtlas;
            float2 _TileAtlasSize;
            float2 _TileCenter;
            float _TileAtlasPPU;

            sampler2D _WorldTexture;
            float2 _WorldTextureSize;
            float2 _WorldTextureOffset;
            
            float2 _RenderSize;
            float2 _RenderOffset01;
            float _RenderTileHeight;
            
            sampler2D _NoiseTexture;
            float2 _NoiseTextureSize;
            float2 _NoiseTextureOffset;

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

                float2 gridScale = float2(_RenderTileHeight, _RenderTileHeight);
                gridScale.x *= 1 / (aspect * 0.5);

                input.vertex *= _RenderTileHeight * 4;
                input.vertex.x /= aspect;

                input.vertex.x += _RenderOffset01.x * 2 - 1;
                input.vertex.y += _RenderOffset01.y * 2 - _RenderTileHeight - 1;

                // Fix transparent lines glitch.
                // input.vertex.y += sign(input.vertex.y) * (1 / _RenderSize.y);
                // input.vertex.x += sign(input.vertex.x) * (1 / _RenderSize.x);

                uint gridSizeX = (uint)_GridSize.x;
                uint innerGridSizeX = (uint)_GridSize.x - 1;
                uint mainY = floor(instanceID / (gridSizeX + innerGridSizeX));
                uint innerX = instanceID - mainY * (innerGridSizeX + gridSizeX);
                bool oddCheck = innerX / gridSizeX >= 1;
                
                if (oddCheck)
                {
                    // Inner grid tiles:
                    // 0  0  0
                    //  1  1
                    // 0  0  0
                    innerX -= _GridSize.x;
                    input.vertex.x += innerX * gridScale.x * 2 + gridScale.x;
                    input.vertex.y += mainY * gridScale.y * 2 + gridScale.y;
                }
                else
                {
                    // Outer grid tiles:
                    // 1  1  1
                    //  0  0
                    // 1  1  1
                    input.vertex.x += floor((instanceID - mainY * innerGridSizeX) % gridSizeX) * gridScale.x * 2;
                    input.vertex.y += mainY * gridScale.y * 2;
                }

                input.vertex.y *= -1 * _ProjectionParams.x;

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