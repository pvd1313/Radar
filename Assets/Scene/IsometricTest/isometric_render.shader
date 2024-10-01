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
                float2 mainUV : TEXCOORD0;
                float2 noiseUV : TEXCOORD1;
            };

            float2 _GridSize;
            
            sampler2D _TileTexture;
            float2 _TileCenter;

            
            float2 _RenderSize;
            float _VertexSize;
            sampler2D _NoiseTexture;
            float2 _NoiseTextureSize;
            

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

                float2 gridScale = _VertexSize * 0.25;
                gridScale.x *= 1 / (aspect * 0.5);

                input.vertex *= _VertexSize;
                input.vertex.x /= aspect;

                // Fix transparent lines glitch.
                //float texelSize = ;
                input.vertex.y += sign(input.vertex.y) * (1 / _RenderSize.y);
                input.vertex.x += sign(input.vertex.x) * (1 / _RenderSize.x);

                uint gridSizeX = (uint)_GridSize.x;
                uint innerGridSizeX = (uint)_GridSize.x - 1;
                uint mainY = floor(instanceID / (gridSizeX + innerGridSizeX));
                uint innerX = instanceID - mainY * (innerGridSizeX + gridSizeX);
                bool oddCheck = innerX / gridSizeX >= 1;

                input.vertex.xy -= 1;
                input.vertex.y -= gridScale.y;
                
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

                output.mainUV = input.texcoord.xy;
                output.positionCS = float4(input.vertex.x, input.vertex.y, UNITY_NEAR_CLIP_VALUE, 1);

                return output;
            }

            float4 fragment(FragmentInput input) : SV_Target
            {
                float4 tileColor = tex2D(_TileTexture, input.mainUV);

                clip(tileColor.r + tileColor.g + tileColor.b - 0.01);
                
                float t = saturate(64 / (_RenderSize.y * _VertexSize));
                
                float4 noiseColor = tex2D(_NoiseTexture, input.noiseUV);
                
                float2 centerUV = lerp(input.mainUV, _TileCenter, t * 0.5 + t * noiseColor.r * 0.5);
                
                float4 centerColor = tex2D(_TileTexture, centerUV);

                tileColor = lerp(tileColor, centerColor, t);
                
                return tileColor;
            }
            
            ENDCG
        }
    }
}