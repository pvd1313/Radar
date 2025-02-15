Shader "Beast/Radar/Sprite"
{
    SubShader
    {
        // Rendered to buffer.
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

            struct FragmentInput
            {
                float4 positionCS : SV_POSITION;
                half2 uv : TEXCOORD0;
            };

            float4 _ViewMatrix0;
            float4 _ViewMatrix1;
            float _ProjectionScale;
            float2 _RenderCenter;
            float _RenderDistance;

            float _AtlasTexelSize;
            sampler2D _AtlasTexture;

            sampler2D _DataTexture;
            float _DataTextureSize;
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
                float posX = beast_half2_to_float(v0, v1);
                float posY = beast_half2_to_float(v2, v3);
                float2 spriteSize = float2(v6, v7);
                float2 size = input.vertex.xy * (spriteSize / _ScreenSize * 2);
                float4 screenSize = float4(size.x, size.y, 0, 0);
                float2 worldPos = float2(posX, posY);

                float2 spriteOffset = float2(v4, v5);
                output.positionCS = beast_world_to_clip_pos(world2Clip, worldPos) + screenSize;
                output.uv = (spriteOffset + spriteSize * input.vertex.xy + spriteSize * 0.5) * _AtlasTexelSize;

                return output;
            }

            float4 fragment(FragmentInput input) : SV_Target
            {
                float3 color = tex2D(_AtlasTexture, input.uv);

                beast_clip_black_color(color);
                
                return float4(color.r, color.g, color.b, 1);
            }
            
            ENDCG
        }
    }
}