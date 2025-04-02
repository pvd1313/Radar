Shader "Beast/Radar/PixelPlot"
{
    SubShader
    {
        // Rendered to buffer.
        cull off
        zwrite false
        ztest false

        Pass
        {
            CGPROGRAM

            #pragma target 2.0
            
            #pragma vertex vertex
            #pragma fragment fragment

            #include "UnityCG.cginc"
            #include "Assets/Source/Radar/Graphic/Radar.cginc"

            struct FragmentInput
            {
                float4 positionCS : SV_POSITION;
            };


            float _PixelColor;

            sampler2D _DataTexture;
            float2 _DataTextureSize;
            
            FragmentInput vertex(appdata_base input, uint instanceID : SV_InstanceID)
            {
                FragmentInput output;

                half v0, v1, v2, v3;
                beast_get_data_vector(_DataTexture, _DataTextureSize, instanceID, v0, v1, v2, v3);

                uint x = f32tof16(v0);
                uint y = f32tof16(v1);

                float4 position = float4(x, y, 0, 0) + input.vertex;

                output.positionCS = UnityObjectToClipPos(position);

                return output;
            }

            float4 fragment(FragmentInput input) : SV_Target
            {
                return float4(f16tof32(_PixelColor), 0, 0, 0);
            }
            
            ENDCG
        }
    }
}