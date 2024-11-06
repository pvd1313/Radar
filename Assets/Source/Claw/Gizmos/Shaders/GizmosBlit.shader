Shader "Beast/Claw/Gizmos/Blit"
{
    SubShader
    {
        Pass
        {
            // Rendered to camera
            zwrite on
            ztest on
            cull off
            
            CGPROGRAM
            
            #pragma vertex vertex
            #pragma fragment fragment

            #pragma target 2.0
            
            #include "UnityCG.cginc"

            struct VertexInput
            {
                float4 vertexOS : POSITION;
                half2 uv : TEXCOORD0;
            };

            struct FragmenInput
            {
                half2 uv : TEXCOORD0;
                float4 vertexCS : SV_POSITION;
            };

            FragmenInput vertex (VertexInput input)
            {
                FragmenInput output;
                
                output.vertexCS = UnityObjectToClipPos(input.vertexOS);
                output.uv = input.uv;
                
                return output;
            }

            sampler2D _Texture;
            
            fixed4 fragment (FragmenInput input) : SV_Target
            {
                fixed4 color = tex2D(_Texture, input.uv);

                clip(color.a - 1);
                
                return color;
            }
            
            ENDCG
        }
    }
}