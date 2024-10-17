Shader "Beast/Radar/Terrain/TileBlit"
{
    SubShader
    {
        // Rendered to camera.
        cull off
        zwrite on
        ztest on

        Pass
        {
            CGPROGRAM

            #pragma target 2.0
            
            #pragma vertex vertex
            #pragma fragment fragment

            #include "UnityCG.cginc"
            #include "Assets/Source/Radar/Graphic/RadarCG.cginc"

            struct VertexInput
            {
                float4 positionOS : POSITION;
                float2 quadUV : TEXCOORD0;
            };

            struct FragmentInput
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTexture;
            float _Alpha;
            float2 _OutputSize;

            FragmentInput vertex(VertexInput input)
            {
                FragmentInput output;
                output.positionCS = UnityObjectToClipPos(input.positionOS);
                output.uv = input.quadUV;
                
                return output;
            }

            float4 fragment(FragmentInput input) : SV_Target
            {
                float dither = beast_dither_cs(input.positionCS, _Alpha);
                
                clip(dither);
                
                return tex2D(_MainTexture, input.uv);
            }
            
            ENDCG
        }
    }
}