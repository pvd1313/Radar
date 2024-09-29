Shader "Beast/IsometricRender3"
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

            float _GridSize;
            float3 _CameraDirection;

            sampler2D _TileTexture;
            float2 _RenderSize;
            float _VertexSize;
            sampler2D _NoiseTexture;
            float2 _NoiseTextureSize;

            FragmentInput vertex(appdata_base input, uint instanceID : SV_InstanceID)
            {
                FragmentInput output;

                float aspect = _RenderSize.x / _RenderSize.y;

                float2 gridScale = _VertexSize * 0.25;
                gridScale.x *= 1 / (aspect * 0.5);

                input.vertex *= _VertexSize;
                input.vertex.x /= aspect;

                float texelSize = 1 / _RenderSize.y;
                input.vertex += sign(input.vertex) * texelSize;

                input.vertex.y -= gridScale * _GridSize * aspect * 0.5;

                float gridMod = floor(instanceID % _GridSize);
                float gridDiv = floor(instanceID / _GridSize);
                input.vertex.x += gridMod * gridScale.x - gridDiv * gridScale.x;
                input.vertex.y += gridDiv * gridScale.y + gridMod * gridScale.y;

                output.noiseUV = float2(gridMod, gridDiv) / _NoiseTextureSize;

                input.vertex.y *= -1 * _ProjectionParams.x;

                output.mainUV = input.texcoord.xy;
                output.positionCS = float4(input.vertex.x, input.vertex.y, UNITY_NEAR_CLIP_VALUE, 1);

                return output;
            }

            float4 fragment(FragmentInput input) : SV_Target
            {
                float FLOAT_EPSILON = 0.01;
                
                float4 mainColor = tex2D(_TileTexture, input.mainUV);

                clip(mainColor.r + mainColor.g + mainColor.b - FLOAT_EPSILON);
                
                float t = saturate(64 / (_RenderSize.y * _VertexSize));

                float4 noiseColor = tex2D(_NoiseTexture, input.noiseUV);

                float2 centerUV = lerp(input.mainUV, float2(0.5, 0.5), t * 0.5 + t * noiseColor.r * 0.5);

                float4 centerColor = tex2D(_TileTexture, centerUV);

                mainColor = lerp(mainColor, centerColor, t);
                
                return mainColor;
            }
            
            ENDCG
        }
    }
}