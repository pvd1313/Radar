Shader "Beast/IsometricRender"
{
    Properties
    {
        
    }
    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Off

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
                float2 vertexUV : TEXCOORD0;
                float2 solverUV : TEXCOORD1;
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

                float2 gridScale = float2(_OutputTileHeight, _OutputTileHeight) * 2.0f;
                gridScale.x *= 1.0f / (aspect * 0.5f);

                input.vertex.y = input.vertex.y * (1 - _RenderYCrop) - _RenderYCrop * 0.5;
                input.texcoord.y *= _RenderYCrop;

                input.vertex *= _OutputTileHeight * 4.0f;
                input.vertex.x /= aspect;

                input.vertex.x += _OutputOffset.x * 2.0f - 1.0f;
                input.vertex.y -= _OutputOffset.y * 2.0f - _OutputTileHeight - 1.0f;

                // Fix transparent lines glitch.
                // input.vertex.y += sign(input.vertex.y) * (1 / _RenderSize.y);
                // input.vertex.x += sign(input.vertex.x) * (1 / _RenderSize.x);

                BeastIsometricTile tile = beast_isometric_tile(instanceID, _TileGridSize.x);
                float2 screenOffset = beast_isometric_screen_offset(tile, _TileGridSize.x);
                
                input.vertex.x += screenOffset.x * gridScale.x;
                input.vertex.y += screenOffset.y * gridScale.y;

                input.vertex.y *= _ProjectionParams.x;

                output.vertexUV = input.texcoord.xy * (1.0f / _TileAtlasTileCount);

                float solverX = instanceID % (uint) _SolverSize;
                float solverY = instanceID / (uint) _SolverSize;
                float solverTexelSize = 1.0f / float2(_SolverSize, _SolverSize);
                output.solverUV = float2(solverX, solverY) * solverTexelSize + solverTexelSize * 0.5f;
                
                output.positionCS = float4(input.vertex.x, input.vertex.y, UNITY_NEAR_CLIP_VALUE, 1.0f);

                return output;
            }

            float4 fragment(FragmentInput input) : SV_Target
            {
                float2 solverRG = tex2D(_SolverTexture, input.solverUV);

                uint tileIndex = f32tof16(solverRG.r);
                float tileX = tileIndex % (uint) _TileAtlasTileCount.x;
                float tileY = tileIndex / (uint) _TileAtlasTileCount.x;
                float2 tileUVShift = float2(tileX, tileY) * (1.0f / _TileAtlasTileCount);
                float2 tileUV = input.vertexUV + tileUVShift;
                
                float4 atlasColor = tex2D(_TileAtlas, input.vertexUV + tileUVShift);

                clip(atlasColor.r + atlasColor.g + atlasColor.b - 0.01f);

                float noisePower = solverRG.g;
                float t = saturate(_TileAtlasPPU / (_OutputSize.y * _OutputTileHeight * 4));
                float2 centerUV = tileUVShift + _TileBlendCenter / _TileAtlasTileCount;
                centerUV = lerp(tileUV, centerUV, t * 0.5f + t * noisePower * 0.5f);
                
                float4 centerColor = tex2D(_TileAtlas, centerUV);
                
                return lerp(atlasColor, centerColor, t);
            }
            
            ENDCG
        }
    }
}