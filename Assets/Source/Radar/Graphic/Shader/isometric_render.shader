Shader "Beast/IsometricRender"
{
    Properties
    {
        
    }
    SubShader
    {
        cull off
        zwrite on
        ztest on

        Pass
        {
            CGPROGRAM

            #pragma target 1.0
            
            #pragma vertex vertex
            #pragma fragment fragment

            #include "UnityCG.cginc"
            #include "Assets/Source/Claw/Graphic/BeastCG.cginc"
            
            // #define DEBUG_DRAW_TILES_ONE_SECOND

            struct FragmentInput
            {
                float4 positionCS : SV_POSITION;
                half2 mainUV : TEXCOORD0;
                half2 centerUV : TEXCOORD1;
                fixed t : TEXCOORD2;
            };

            float2 _TileGridSize;
            
            sampler2D _TileAtlas;
            float2 _TileAtlasWidthHeight;
            float2 _TileAtlasTileCount;
            float2 _TileBlendCenter;
            float _TileAtlasPPU;
            
            float _RenderYCrop;

            sampler2D _SolverTexture;
            float _SolverSize;
            
            float2 _OutputSize;
            float2 _OutputOffset;
            float _OutputTileHeight;
            
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
                
                float aspect = _OutputSize.x / _OutputSize.y;

                // Y crop, reduce rasterizer overlload. Will be obtained from config texture in future.
                input.vertex.y = input.vertex.y * (1 - _RenderYCrop) - _RenderYCrop * 0.5;
                input.texcoord.y *= 1 - _RenderYCrop;

                // Scale vertex to desired size.
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

                // Get tile data from solver.
                float2 vertexUV = input.texcoord.xy * (1.0f / _TileAtlasTileCount);
                float solverX = instanceID % (uint) _SolverSize;
                float solverY = instanceID / (uint) _SolverSize;
                float solverTexelSize = 1.0f / float2(_SolverSize, _SolverSize);
                float2 solverUV = float2(solverX, solverY) * solverTexelSize + solverTexelSize * 0.5f;
                float2 solverRG = tex2Dlod(_SolverTexture, float4(solverUV.x, solverUV.y, 0, 0));
                uint tileIndex = f32tof16(solverRG.r);
                float tileX = tileIndex % (uint) _TileAtlasTileCount.x;
                float tileY = tileIndex / (uint) _TileAtlasTileCount.x;
                float2 tileUVShift = float2(tileX, tileY) * (1.0f / _TileAtlasTileCount);
                float2 tileUV = vertexUV + tileUVShift;
                output.mainUV = tileUV;

                // Second UV to reduce glitches when tiles are small. Blend center will be obtained from tile config in future.
                float noisePower = solverRG.g;
                float t = saturate(_TileAtlasPPU / (_OutputSize.y * _OutputTileHeight * 4));
                float2 centerUV = tileUVShift + _TileBlendCenter / _TileAtlasTileCount;
                output.centerUV = lerp(tileUV, centerUV, t * 0.5f + t * noisePower * 0.5f);

                output.t = t;
                
                output.positionCS = float4(input.vertex.x, input.vertex.y, UNITY_NEAR_CLIP_VALUE, 1.0f);

                return output;
            }

            float4 fragment(FragmentInput input) : SV_Target
            {
                half3 mainColor = tex2D(_TileAtlas, input.mainUV);

                clip(mainColor.r + mainColor.g + mainColor.b - 0.5f);

                half3 centerColor = tex2D(_TileAtlas, input.centerUV);
                
                mainColor = lerp(mainColor, centerColor, input.t);
                
                return half4(mainColor.r, mainColor.g, mainColor.b, 1);
            }
            
            ENDCG
        }
    }
}