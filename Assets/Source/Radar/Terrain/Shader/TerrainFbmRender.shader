Shader "Beast/Radar/Terrain/FbmRender"
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

            float fbm_amount(float fbm, float amount)
            {
                amount = lerp(0.25f, 0.4f, amount + 0.3f);
                
                fbm = pow(amount, 1.5f) * fbm;
                fbm += pow(amount, 0.75f) * (1 - fbm);

                return tex2D(_GradientTexture, float2(fbm, 0.5f));
            }

            float4 fragment(FragmentInput input) : SV_Target
            {
                float quality = lerp(7, 14, _Quality01);

                _Amount.x = lerp(0.5f, 1, _Amount.x);
                _Amount.y = lerp(0.25f, 1, _Amount.y);

                float2 scaledUV = input.uv * lerp(2, 4, 1 - _BiomeSize01);
                float fbm0 = beast_fbm(scaledUV, 0.65f, quality, _Seed.x);
                float fbm1 = beast_fbm(scaledUV, 0.65f, quality, _Seed.y);
                
                float r = fbm_amount(fbm0, _Amount.x);
                float g = fbm_amount(fbm1, _Amount.y);

                // Water border.
                float2 uvCentered = input.uv * 2.0f - 1.0f;
                float falloff = pow(length(uvCentered), 4.0f + _Amount.x * 8);
                r = beast_rect(input.uv, r, 1, 4.0f / _TextureSize);
                r = saturate(r + falloff * (1 - fbm1) + falloff * 0.1f * (1 - fbm0));

                return float4(r, g, 0, 0);
            }
            
            ENDCG
        }
    }
}