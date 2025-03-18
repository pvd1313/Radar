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
                float3 vertexWS : POSITION;
                float3 normalWS : NORMAL;
                float2 uv : TEXCOORD0;
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
            
            FragmentInput vertex(VertexInput input)
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

                float2 worldPos = input.vertexWS.xy + input.normalWS.xy * 0.5f;
                output.positionCS = beast_world_to_clip_pos(world2Clip, worldPos, 0);
                output.uv = input.uv;

                return output;
            }

            float4 fragment(FragmentInput input) : SV_Target
            {
                return 1;
            }
            
            ENDCG
        }
    }
}