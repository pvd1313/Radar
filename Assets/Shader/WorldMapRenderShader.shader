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
            float _ScreenAspect;
            float _ViewDistance;
            float2 _CameraPosition;
            sampler2D _WorldNoiseTexture;
            float2 _WorldNoiseTextureSize;

            FragmentInput vertex(VertexInput input)
            {
                FragmentInput output;
                
                output.positionCS = UnityObjectToClipPos(input.positionOS);

                output.mainUV = input.screenUV - 0.5;
                output.mainUV.x *= _ScreenAspect;
                output.mainUV *= _ViewDistance / _WorldTextureSize;
                output.mainUV += _CameraPosition / _WorldTextureSize;
                
                output.noiseUV = input.screenUV - 0.5;
                output.noiseUV.x *= _ScreenAspect;
                output.noiseUV *= _ViewDistance / _WorldNoiseTextureSize;
                output.noiseUV += _CameraPosition / _WorldNoiseTextureSize;

                return output;
            }

            float2 unpack_normal(float2 color)
            {
                return (color - 0.5) * 2;
            }

            float4 fragment(FragmentInput input) : SV_Target
            {
                float difference = _WorldTextureSize / _WorldNoiseTextureSize;
                float2 noiseRG = tex2D(_WorldNoiseTexture, input.noiseUV * difference).xy;
                float2 texelSize = _WorldTextureSize / _ViewDistance;
                float2 noise = unpack_normal(noiseRG) * 0.2;

                input.mainUV = input.mainUV + noise * 8 / _WorldTextureSize;
                input.mainUV = float2 (input.mainUV / texelSize) * texelSize;
                
                float color = tex2D(_WorldTexture, input.mainUV);
                
                return float4(color, 0, 0, 0);
            }
            
            ENDCG
        }
    }
}