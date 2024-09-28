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

            struct VertexInput
            {
                float4 positionOS : POSITION;
                float2 screenUV : TEXCOORD0;
            };

            struct FragmentInput
            {
                float2 mainUV : TEXCOORD0;
                float2 noiseUV : TEXCOORD1;
                float4 positionCS : SV_POSITION;
            };

            sampler2D _WorldTexture;
            float2 _WorldTextureSize;
            float _RenderAspect;
            float _ViewDistance;
            float2 _CameraPosition;
            sampler2D _WorldNoiseTexture;
            float2 _WorldNoiseTextureSize;

            FragmentInput vertex(VertexInput input)
            {
                FragmentInput output;
                
                output.positionCS = UnityObjectToClipPos(input.positionOS);

                output.mainUV = input.screenUV - 0.5;
                output.mainUV.x *= _RenderAspect;
                output.mainUV *= _ViewDistance / _WorldTextureSize;
                output.mainUV += _CameraPosition / _WorldTextureSize;
                
                output.noiseUV = input.screenUV - 0.5;
                output.noiseUV.x *= _RenderAspect;
                output.noiseUV *= _ViewDistance / _WorldNoiseTextureSize;
                output.noiseUV += _CameraPosition / _WorldNoiseTextureSize;

                return output;
            }

            float2 unpack_normal(float2 color)
            {
                return (color - float2(0.5, 0.5)) * float2(2, 2);
            }

            float4 fragment(FragmentInput input) : SV_Target
            {
                float2 NoiseShift = float2(8, 8);
                float2 noisePower = float2(0.3, 0.2);
                
                float2 noiseRG = tex2D(_WorldNoiseTexture, input.noiseUV * NoiseShift).rg;
                float2 texelSize = _WorldTextureSize / _ViewDistance;
                float2 noise = unpack_normal(noiseRG) * noisePower;

                input.mainUV = input.mainUV + noise * NoiseShift / _WorldTextureSize;
                input.mainUV = float2 (input.mainUV / texelSize) * texelSize;
                
                float color = tex2D(_WorldTexture, input.mainUV);
                
                return float4(color, 0, 0, 0);
            }
            
            ENDCG
        }
    }
}