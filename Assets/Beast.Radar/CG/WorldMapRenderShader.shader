Shader "Beast/Radar/WorldMapRenderShader"
{
    Properties
    {
        _WorldTexture ("_WorldTexture", 2D) = "white" {}
        _WorldTextureSize ("_WorldTextureSize", Vector) = (0, 0, 0, 0)
        _ScreenAspect ("_ScreenAspect", Float) = 1
        _ViewDistance ("_ViewDistance", Float) = 128
        _CameraPosition ("_CameraPosition", Vector) = (0, 0, 0, 0)
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
                float2 uv : TEXCOORD0;
            };

            struct FragmentInput
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            };

            sampler2D _WorldTexture;
            float2 _WorldTextureSize;
            float _ScreenAspect;
            float _ViewDistance;
            float2 _CameraPosition;

            FragmentInput vertex(VertexInput input)
            {
                FragmentInput output;
                
                output.positionCS = UnityObjectToClipPos(input.positionOS);

                output.uv = input.uv - float2(0.5f, 0.5f);
                output.uv.x *= _ScreenAspect;
                
                output.uv *= _ViewDistance / _WorldTextureSize;
                output.uv += _CameraPosition / _WorldTextureSize;
                
                return output;
            }

            float4 fragment(FragmentInput input) : SV_Target
            {
                return tex2D(_WorldTexture, input.uv);
            }
            
            ENDCG
        }
    }
}