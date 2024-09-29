Shader "Beast/IsometricRender2"
{
    Properties
    {
        
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
        }
        
        LOD 100

        Pass
        {
            CGPROGRAM

            #pragma target 2.0
            
            #pragma vertex vertex
            #pragma fragment fragment

            #include "UnityCG.cginc"

            struct FragmentInput
            {
                // float2 mainUV : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float order : TEXCOORD2;
            };

            float _GridSize;
            float3 _CameraDirection;

            sampler2D _Texture;

            float2 _RenderSize;

            FragmentInput vertex(appdata_base input, uint instanceID : SV_InstanceID)
            {
                FragmentInput output;

                float aspect = _RenderSize.x / _RenderSize.y;

                float vertexSize = 0.2;
                
                float2 gridScale = float2(0.05 * (1 / (aspect * 0.5)), 0.05);
                

                input.vertex *= vertexSize;
                input.vertex.x /= aspect;

                float texelSize = 1 / _RenderSize.y;
                input.vertex += sign(input.vertex) * texelSize;
                
                // input.vertex.x += sign(input.vertex.x) * pixelSize;
                // input.vertex.y += sign(input.vertex.y) * pixelSize;

                input.vertex.y -= gridScale * _GridSize * aspect * 0.5;

                float gridMod = floor(instanceID % _GridSize);
                float gridDiv = floor(instanceID / _GridSize);
                input.vertex.x += gridMod * gridScale.x - gridDiv * gridScale.x;
                input.vertex.y += gridDiv * gridScale.y + gridMod * gridScale.y;

                output.order = instanceID / (_GridSize * _GridSize);

                output.uv = input.texcoord.xy;

                // float t = 64 / _RenderSize.y;
                //
                // output.uv.x -= 0.5;
                // output.uv.x *= 1 - pow(t, 1);
                // output.uv.x += 0.5;
                //
                // output.uv.y -= 0.5;
                // output.uv.y *= 1 - pow(t, 1);
                // output.uv.y += 0.5;

                // float pixelSize0 = pow(32 / _RenderSize.y, 1 + vertexSize * 2);
                //
                // output.uv.x = clamp(output.uv.x - sign(output.uv.x - 0.5) * pixelSize0, pixelSize0, 1 - pixelSize0);
                // output.uv.y = clamp(output.uv.y - sign(output.uv.y - 0.5) * pixelSize0, pixelSize0, 1 - pixelSize0);
                
                //output.uv += clamp(sign(output.uv - 0.5) * (1 / 64), border);

                output.positionCS = float4(input.vertex.x, input.vertex.y, UNITY_NEAR_CLIP_VALUE, 1);

                return output;
            }

            float4 fragment(FragmentInput input) : SV_Target
            {
                float FLOAT_EPSILON = 0.001;
                
                float4 color = tex2D(_Texture, input.uv);

                //float4 centerColor = tex2D(_Texture, float2(0.5, 0.5));

                //float2 uvTime = abs((input.uv - 0.5) * 2);

                //color = lerp(centerColor, color, uvTime.x * uvTime.y);

                clip(color.r + color.g + color.b - FLOAT_EPSILON);

                //color *= lerp(float4(1, 0, 0, 1), float4(0, 1, 0, 1), input.order);
                
                return input.order;
            }
            
            ENDCG
        }
    }
}