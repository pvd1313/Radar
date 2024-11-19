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

            float4 _ViewMatrix0;
            float4 _ViewMatrix1;
            float _ProjectionScale;
            float2 _RenderCenter;
            float _RenderDistance;

            sampler2D _DataTexture;
            float _DataTextureSize;
            float2 _ScreenSize;

            inline void get_vector(uint index, out half h0, out half h1, out half h2, out half h3)
            {
                float texelSize = 1.0f / _DataTextureSize;
                
                uint x = index % (uint) _DataTextureSize;
                uint y = index / (uint) _DataTextureSize;
                
                float2 uv = float2(x, y) * texelSize + texelSize * 0.5f;

                half4 color = beast_tex2Dlod(_DataTexture, uv, 0);

                h0 = color.r;
                h1 = color.g;
                h2 = color.b;
                h3 = color.a;
            }

            float4 world_to_clip_pos(float2 worldPos)
            {
                float aspect = _ScreenSize.x / _ScreenSize.y;

                worldPos = worldPos - _RenderCenter;
                
                worldPos = mul(float2x2(_ViewMatrix0), worldPos);
                worldPos = mul(float2x2(_ViewMatrix1), worldPos);
                
                worldPos /= _RenderDistance;
                worldPos.x /= aspect;
                worldPos *= _ProjectionScale;

                worldPos.y *= _ProjectionParams.x;
                
                return float4(worldPos.x, worldPos.y, UNITY_NEAR_CLIP_VALUE, 1.0f);
            }
            
            FragmentInput vertex(appdata_base input, uint instanceID : SV_InstanceID)
            {
                FragmentInput output;

                half v0, v1, v2, v3, v4, v5, v6, v7;
                get_vector(instanceID * 2 + 0, v0, v1, v2, v3);
                get_vector(instanceID * 2 + 1, v4, v5, v6, v7);
                
                // World point, with screen based size.
                if (v0 == 0)
                {
                    float posX = beast_half2_to_float(v4, v5);
                    float posY = beast_half2_to_float(v6, v7);
                    float2 size = input.vertex.xy * (v1 / _ScreenSize * 2);
                    float4 screenSize = float4(size.x, size.y, 0, 0);
                    float2 worldPos = float2(posX, posY);
                    
                    output.positionCS = world_to_clip_pos(worldPos) + screenSize;
                    output.color = beast_half2_to_rgba(v2, v3);
                }
                // World point, with world based size.
                else
                {
                    float posX = beast_half2_to_float(v4, v5);
                    float posY = beast_half2_to_float(v6, v7);
                    float2 size = float2(input.vertex.x, input.vertex.y) * v1;
                    float2 worldPos = float2(posX, posY);
                    
                    output.positionCS = world_to_clip_pos(worldPos + size);
                    output.color = beast_half2_to_rgba(v2, v3);
                }
                
                return output;
            }

            float4 fragment(FragmentInput input) : SV_Target
            {
                clip(beast_dither_cs(input.positionCS, input.color.a));

                return input.color;
            }
            
            ENDCG
        }
    }
}