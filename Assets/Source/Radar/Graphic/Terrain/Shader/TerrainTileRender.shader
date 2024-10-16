Shader "Beast/Radar/Terrain/TileRender"
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

            #pragma target 2.0
            
            #pragma vertex vertex
            #pragma fragment fragment

            #include "UnityCG.cginc"
            #include "Assets/Source/Radar/Graphic/Main/RadarCG.cginc"
            
            // #define DEBUG_DRAW_NOISE_ONLY
            // #define DEBUG_DRAW_TILES_ONE_SECOND

            struct FragmentInput
            {
                float4 positionCS : SV_POSITION;
                half2 mainUV : TEXCOORD0;
                half2 centerUV : TEXCOORD1;
                fixed blendPower : TEXCOORD2;

#ifdef DEBUG_DRAW_NOISE_ONLY
                float noiseColor : TEXCOORD3;
#endif
                
            };

            float2 _TileGridSize;
            
            sampler2D _TileAtlas;
            float2 _TileAtlasWidthHeight;
            float2 _TileAtlasTileCount;
            float _TileAtlasPPU;
            
            sampler2D _SolverTexture;
            float _SolverSize;
            
            float2 _OutputSize;
            float2 _OutputOffset;
            float _OutputTileHeight;
            
            FragmentInput vertex(appdata_base input, uint instanceID : SV_InstanceID)
            {
                FragmentInput output;

#ifdef DEBUG_DRAW_TILES_ONE_SECOND
                float2 gSize = _TileGridSize;
                if (instanceID > (gSize.x * gSize.y + (gSize.x - 1) * (gSize.y - 1)) * frac(_Time.y))
                {
                    output.positionCS = 0;
                    return output;
                }
#endif

                // Get tile data from solver.
                float solverX = instanceID % (uint) _SolverSize;
                float solverY = instanceID / (uint) _SolverSize;
                float solverTexelSize = 1.0f / float2(_SolverSize, _SolverSize);
                float2 solverUV = float2(solverX, solverY) * solverTexelSize + solverTexelSize * 0.5f;
                float4 solverRGBA = tex2Dlod(_SolverTexture, float4(solverUV.x, solverUV.y, 0, 0));

                // Y crop, reduce rasterizer overload.
                float renderYCrop = solverRGBA.b;
                input.vertex.y = input.vertex.y * (1 - renderYCrop) - renderYCrop * 0.5;
                input.texcoord.y *= 1 - renderYCrop;

                // Main UV.
                float2 vertexUV = input.texcoord.xy * (1.0f / _TileAtlasTileCount);
                uint tileIndex = f32tof16(solverRGBA.r);
                float tileX = tileIndex % (uint) _TileAtlasTileCount.x;
                float tileY = tileIndex / (uint) _TileAtlasTileCount.x;
                float2 tileUVShift = float2(tileX, tileY) * (1.0f / _TileAtlasTileCount);
                float2 tileUV = vertexUV + tileUVShift;
                output.mainUV = tileUV;

                // Second UV to reduce glitches when tiles are small.
                float noisePower = solverRGBA.g;
                float blendCenterY = solverRGBA.a;
                float t = saturate(_TileAtlasPPU / (_OutputSize.y * _OutputTileHeight * 4));
                float2 centerUV = tileUVShift + float2(0.5f, blendCenterY) / _TileAtlasTileCount;
                output.centerUV = lerp(tileUV, centerUV, t * 0.5f + t * noisePower * 0.5f);
                output.blendPower = t;

                // Scale vertex to desired size.
                float aspect = _OutputSize.x / _OutputSize.y;
                input.vertex *= _OutputTileHeight * 4.0f;
                input.vertex.x /= aspect;

                // Prevent transparent lines glitches.
                input.vertex.x += sign(input.vertex.x) * (1.0f / _OutputSize.x);
                input.vertex.y += sign(input.vertex.y) * (1.0f / _OutputSize.y);

                // Isometric to screen layout.
                float2 gridScale = float2(_OutputTileHeight, _OutputTileHeight) * 2.0f;
                gridScale.x *= 1.0f / (aspect * 0.5f);
                BeastIsometricTile tile = beast_isometric_tile(instanceID, _TileGridSize.x);
                float2 screenOffset = beast_isometric_screen_offset(tile, _TileGridSize.x);
                input.vertex.x += screenOffset.x * gridScale.x;
                input.vertex.y += screenOffset.y * gridScale.y;

                // Viewport offset. Must be applied after isometric layout to prevent transparent line glitches.
                input.vertex.x += _OutputOffset.x * 2.0f - 1.0f;
                input.vertex.y -= _OutputOffset.y * 2.0f - _OutputTileHeight - 1.0f;

                // Projection flip
                input.vertex.y *= _ProjectionParams.x;
                
                output.positionCS = float4(input.vertex.x, input.vertex.y, UNITY_NEAR_CLIP_VALUE, 1.0f);

#ifdef DEBUG_DRAW_NOISE_ONLY
                output.noiseColor = noisePower;
#endif
                

                return output;
            }

            float4 fragment(FragmentInput input) : SV_Target
            {
#ifdef DEBUG_DRAW_NOISE_ONLY
                return input.noiseColor;
#endif
                
                half3 mainColor = tex2D(_TileAtlas, input.mainUV);

                clip(mainColor.r + mainColor.g + mainColor.b - 0.01f);

                half3 centerColor = tex2D(_TileAtlas, input.centerUV);
                
                mainColor = lerp(mainColor, centerColor, input.blendPower);
                
                return half4(mainColor.r, mainColor.g, mainColor.b, 1);
            }
            
            ENDCG
        }
    }
}