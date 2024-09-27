Shader "Beast/Radar/WorldMapRenderShader"
{
    Properties
    {
        _WorldTexture ("_WorldTexture", 2D) = "white" { }
        _WorldTextureSize ("_WorldTextureSize", Vector) = (0, 0, 0, 0)
        
        _ScreenAspect ("_ScreenAspect", Float) = 1
        _ViewDistance ("_ViewDistance", Float) = 128
        _CameraPosition ("_CameraPosition", Vector) = (0, 0, 0, 0)
        
        _WorldNoiseTexture ("_WorldNoiseTexture", 2D) = "black" { }
        _WorldNoiseTextureSize ("_WorldNoiseTextureSize", Vector) = (0, 0, 0, 0)
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

                output.mainUV = input.screenUV - float2(0.5f, 0.5f);
                output.mainUV.x *= _ScreenAspect;
                output.mainUV *= _ViewDistance / _WorldTextureSize;
                output.mainUV += _CameraPosition / _WorldTextureSize;

                output.noiseUV = input.screenUV - float2(0.5f, 0.5f);
                output.noiseUV.x *= _ScreenAspect;
                output.noiseUV *= _ViewDistance / _WorldNoiseTextureSize;
                output.noiseUV += _CameraPosition / _WorldNoiseTextureSize;

                //output.noiseUV = input.screenUV - float2(0.5f, 0.5f);


                
                
                return output;
            }

            float2 unpack_normal(float2 color)
            {
                return (color - 0.5f) * 2;
            }

            float4 fragment(FragmentInput input) : SV_Target
            {
                float2 noise = unpack_normal(tex2D(_WorldNoiseTexture, input.noiseUV).rg);
                noise *= 4 / _WorldTextureSize;

                float color = tex2D(_WorldTexture, input.mainUV + noise);
                
                return float4(saturate(color * 90), 0, 0, 0);
            }

          
            
            ENDCG
        }
    }
}