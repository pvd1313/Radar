Shader "Beast/Radar/WorldMapRenderShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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

            sampler2D _MainTex;

            FragmentInput vertex(VertexInput input)
            {
                FragmentInput o;
                o.positionCS = UnityObjectToClipPos(input.positionOS);
                o.uv = input.uv;
                return o;
            }

            float4 fragment(FragmentInput input) : SV_Target
            {
                return tex2D(_MainTex, input.uv);
            }
            
            ENDCG
        }
    }
}