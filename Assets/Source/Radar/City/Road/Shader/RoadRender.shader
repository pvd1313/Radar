Shader "Beast/Radar/RoadRender"
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

            #include "Assets/Source/Radar/Graphic/Radar.cginc"

            struct VertexInput
            {
                float3 vertex : POSITION;
                float3 normal : NORMAL;
            };

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
            float2 _ScreenSize;
            float _ClipValue;
            
            sampler2D _Texture;
            float _LineWidth;
            float _TextureContrast;
            
            FragmentInput vertex(VertexInput input, uint vertexID : SV_VertexID)
            {
                FragmentInput output;
                
                BeastWorld2ClipMatrix world2Clip;
                world2Clip.projectionScale = _ProjectionScale;
                world2Clip.renderCenter = _RenderCenter;
                world2Clip.renderDistance = _RenderDistance;
                world2Clip.viewMatrix0 = _ViewMatrix0;
                world2Clip.viewMatrix1 = _ViewMatrix1;
                world2Clip.screenSize = _ScreenSize;
                world2Clip.clipValue = _ClipValue;

                float2 leftPoint = input.vertex.xy;
                float2 centerPoint = float2(input.vertex.z, input.normal.x);
                float2 rightPoint = input.normal.yz;

                float4 left = beast_world_to_clip_pos(world2Clip, leftPoint, 0);
                float4 center = beast_world_to_clip_pos(world2Clip, centerPoint, 0);
                float4 right = beast_world_to_clip_pos(world2Clip, rightPoint, 0);

                float2 dir0 = normalize(center.xy - left.xy);
                float2 dir1 = normalize(right.xy - center.xy);
                float2 bisector = normalize(dir0 + dir1);
                float2 perp = float2(bisector.y * -1.0, bisector.x);
                float miterScale = 1.0 / max(0.01, dot(perp, float2(dir1.y * -1.0, dir1.x)));

                float bottomOrTop01 = vertexID % 2u;

                float2 offset = perp * (lerp(-1, +1, bottomOrTop01) * _LineWidth * miterScale);

                float screenAspect = _ScreenSize.x / _ScreenSize.y;
                offset.x /= screenAspect;
                center.xy += offset;

                float2 uv;
                uv.y = bottomOrTop01;
                uv.x = saturate(vertexID % 4 - 1);

                output.positionCS = center;
                output.uv = uv;

                return output;
            }

            float4 fragment(FragmentInput input) : SV_Target
            {
                float4 color = tex2D(_Texture, input.uv);

                color = (color - 0.5f) * _TextureContrast + 0.5f; 
                
                return color;
            }
            
            ENDCG
        }
    }
}