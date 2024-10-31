Shader "Beast/Radar/Terrain/Bake"
{
    SubShader
    {
        // Rendered to buffer
        cull off
        zwrite off
        ztest off

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

            sampler2D _Gradient;
            float _BiomeSize01;
            float2 _Seed;
            float2 _Amount;
            float _Quality01;
            float2 _TextureSize;
            sampler2D _GradientTexture;
            
            FragmentInput vertex(VertexInput input)
            {
                FragmentInput output;
                output.positionCS = UnityObjectToClipPos(input.positionOS);
                output.uv = input.quadUV;
                
                return output;
            }

            float fbm(float2 uv, float quality, float seed, float amount)
            {
                amount = lerp(0.25f, 0.4f, amount + 0.3f);
                
                float fbm = beast_fbm(uv, 0.65f, quality, seed);
                fbm = pow(amount, 1.5f) * fbm;
                fbm += pow(amount, 0.75f) * (1 - fbm);

                return tex2D(_GradientTexture, float2(fbm, 0.5f));
            }

            float4 fragment(FragmentInput input) : SV_Target
            {
                float quality = lerp(7, 14, _Quality01);

                float2 scaledUV = input.uv * lerp(1, 6, 1 - _BiomeSize01);
                float r = fbm(scaledUV, quality, _Seed.x, _Amount.x);
                float g = fbm(scaledUV, quality, _Seed.y, _Amount.y);

                // Currenly simple border, in future we will have erosion.
                r = beast_rect(input.uv, r, 1, 4.0f / _TextureSize);

                return float4(r, g, 0, 0);
            }
            
            ENDCG
        }
    }
}