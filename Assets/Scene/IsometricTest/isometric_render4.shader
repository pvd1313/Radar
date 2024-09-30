Shader "Beast/IsometricRender4"
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

            struct FragmentInput
            {
                float4 positionCS : SV_POSITION;
                float2 mainUV : TEXCOORD0;
                float2 noiseUV : TEXCOORD1;
            };

            float2 _GridSize;
            float3 _CameraDirection;

            sampler2D _TileTexture;
            float2 _RenderSize;
            float _VertexSize;
            sampler2D _NoiseTexture;
            float2 _NoiseTextureSize;

            // 1 1 0 2 = square, custom size
            // 2 0 2 2 = checker board, custom size
            float4 _LayoutMatrix;

            // GRID 3x2:
            // 1 2 3
            //  4 5
            // 6 7 8

            //   0
            //  y 0 x x x
            // y         x x x x
            //y                 x x x x
            FragmentInput vertex(appdata_base input, uint instanceID : SV_InstanceID)
            {
                FragmentInput output;

                // if (instanceID > ((_GridSize.x * _GridSize.y) + (_GridSize.x - 1) * (_GridSize.y - 1))
                //     * frac(_Time.y))
                // {
                //     output.positionCS = 0;
                //     return output;
                // }

                float aspect = _RenderSize.x / _RenderSize.y;

                float2 gridScale = _VertexSize * 0.25;
                gridScale.x *= 1 / (aspect * 0.5);

                input.vertex *= _VertexSize;
                input.vertex.x /= aspect;
                
                int innerGridSizeX = (int)_GridSize.x - 1;
                int mainY = floor(instanceID / ((int)_GridSize.x + innerGridSizeX));
                int innerX = instanceID - mainY * (innerGridSizeX + (int)_GridSize.x);
                bool oddCheck = innerX / (int)_GridSize.x >= 1;

                input.vertex.xy -= 0.5;
                
                if (oddCheck)
                {
                    innerX -= _GridSize.x;
                    input.vertex.x += innerX * gridScale.x * 2 + gridScale.x;
                    input.vertex.y += mainY * gridScale.y * 2 + gridScale.y;
                }
                else
                {
                    input.vertex.x += floor((int)(instanceID - mainY * innerGridSizeX) % (int)_GridSize.x) * gridScale.x * 2;
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
                float4 mainColor = tex2D(_TileTexture, input.mainUV);

                clip(mainColor.r + mainColor.g + mainColor.b - 0.01);
                
                float t = saturate(64 / (_RenderSize.y * _VertexSize));
                
                float4 noiseColor = tex2D(_NoiseTexture, input.noiseUV);
                
                float2 centerUV = lerp(input.mainUV, float2(0.5, 0.23), t * 0.5 + t * noiseColor.r * 0.5);
                
                float4 centerColor = tex2D(_TileTexture, centerUV);

                mainColor = lerp(mainColor, centerColor, t);
                
                return mainColor;
            }
            
            ENDCG
        }
    }
}