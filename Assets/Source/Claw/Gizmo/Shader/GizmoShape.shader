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
            #include "Assets/Source/Radar/Graphic/Radar.cginc"

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
            float2 _DataTextureSize;
            float2 _ScreenSize;
            
            FragmentInput vertex(appdata_base input, uint instanceID : SV_InstanceID)
            {
                FragmentInput output;

                half v0, v1, v2, v3, v4, v5, v6, v7;
                beast_get_data_vector(_DataTexture, _DataTextureSize, instanceID * 2 + 0, v0, v1, v2, v3);
                beast_get_data_vector(_DataTexture, _DataTextureSize, instanceID * 2 + 1, v4, v5, v6, v7);
                
                BeastWorld2ClipMatrix world2Clip;
                world2Clip.projectionScale = _ProjectionScale;
                world2Clip.renderCenter = _RenderCenter;
                world2Clip.renderDistance = _RenderDistance;
                world2Clip.viewMatrix0 = _ViewMatrix0;
                world2Clip.viewMatrix1 = _ViewMatrix1;
                world2Clip.screenSize = _ScreenSize;

                // World point, with screen based size.
                if (v0 == 0)
                {
                    float posX = beast_half2_to_float(v4, v5);
                    float posY = beast_half2_to_float(v6, v7);
                    float2 size = input.vertex.xy * (v1 / _ScreenSize * 2);
                    float4 screenSize = float4(size.x, size.y, 0, 0);
                    float2 worldPos = float2(posX, posY);
                    
                    output.positionCS = beast_world_to_clip_pos(world2Clip, worldPos, 0) + screenSize;
                    output.color = beast_half2_to_rgba(v2, v3);
                }
                // World point, with world based size.
                else
                {
                    float posX = beast_half2_to_float(v4, v5);
                    float posY = beast_half2_to_float(v6, v7);
                    float2 size = float2(input.vertex.x, input.vertex.y) * v1;
                    float2 worldPos = float2(posX, posY);
                    
                    output.positionCS = beast_world_to_clip_pos(world2Clip, worldPos + size, 0);
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