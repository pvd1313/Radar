Shader "Beast/Claw/Gizmo/Shape"
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
                float4 color : COLOR;
            };

            sampler2D _DataTexture;
            float _DataTextureSize;
            float2 _ScreenSize;

            float4 GetColor(uint index)
            {
                float texelSize = 1.0f / _DataTextureSize;
                
                uint x = index % (uint) _DataTextureSize;
                uint y = index / (uint) _DataTextureSize;
                
                float2 uv = float2(x, y) * texelSize + texelSize * 0.5f;

                return beast_tex2Dlod(_DataTexture, uv, 0);
            }
            
            FragmentInput vertex(appdata_base input, uint instanceID : SV_InstanceID)
            {
                FragmentInput output;
                
                float4 set0 = GetColor(instanceID * 2 + 0);
                float4 set1 = GetColor(instanceID * 2 + 1);

                uint shapeType = (uint) floor(set0.x * 255.0f + 0.5f);

                // World point, with screen based size.
                if (shapeType == 0)
                {
                    float2 posXY = set0.yz * 65535;
                    float pixelSize = set0.w * 255.0f;
                    float2 size = input.vertex.xy * (pixelSize / _ScreenSize * 2);
                    float4 vertex = float4(size.x, size.y, 0, 0);
                    float3 worldPos = float3(posXY.x, 0, posXY.y);
                    
                    output.positionCS = UnityWorldToClipPos(worldPos) + vertex;
                    output.color = beast_float_to_rgba8(set1.x);
                }
                // World point, with world based size.
                else if (shapeType == 1)
                {
                    float2 posXY = set0.yz * 65535;
                    float3 size = float3(input.vertex.x, 0, input.vertex.y) * (set0.w * 65535);
                    float3 worldPos = float3(posXY.x, 0, posXY.y);
                    
                    output.positionCS = UnityWorldToClipPos(worldPos + size);
                    output.color = 1.0f;
                }

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