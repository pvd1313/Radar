Shader "Beast/Radar/Terrain/Gizmos/Quad"
{
    SubShader
    {
        // Rendered to buffer.
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

            struct FragmentInput
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
            };

            sampler2D _RGBA32Texture;
            sampler2D _RGFloatTexture;
            float _TextureSize;

            FragmentInput vertex(appdata_base input, uint instanceID : SV_InstanceID)
            {
                FragmentInput output;

                uint x = instanceID % (uint) _TextureSize;
                uint y = instanceID / (uint) _TextureSize;

                float texelSize = 1.0f / _TextureSize;
                float2 uv = float2(x, y) * texelSize + texelSize * 0.5f;

                float4 rgba32 = beast_tex2Dlod(_RGBA32Texture, uv, 0);
                float2 rgFloat = beast_tex2Dlod(_RGFloatTexture, uv, 0);

                float2 posXY = rgFloat * 65535;

                float3 vertex = float3(input.vertex.x, 0, input.vertex.y) * 12;
                float3 quadPos = float3(posXY.x, 0, posXY.y);
                
                output.positionCS = UnityWorldToClipPos(quadPos + vertex);
                
                output.color = 1;

                return output;
            }

            float4 fragment(FragmentInput input) : SV_Target
            {
                return input.color;
            }
            
            ENDCG
        }
    }
}