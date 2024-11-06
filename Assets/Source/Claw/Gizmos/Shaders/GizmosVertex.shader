Shader "Beast/Claw/Gizmos/Vertex"
{
    SubShader
    {
        Pass
        {
            zwrite off
            ztest off
            
            CGPROGRAM
            
            #pragma vertex vertex
            #pragma fragment fragment

            #pragma target 2.0
            
            #include "UnityCG.cginc"

            struct VertexInput
            {
                float4 vertexOS : POSITION;
                fixed4 color : COLOR;
            };

            struct FragmenInput
            {
                fixed4 color : COLOR;
                float4 vertexCS : SV_POSITION;
            };

            FragmenInput vertex (VertexInput input)
            {
                FragmenInput output;
                
                output.vertexCS = UnityObjectToClipPos(input.vertexOS);
                output.color = input.color;
                
                return output;
            }
            
            fixed4 fragment (FragmenInput input) : SV_Target
            {
                return input.color;
            }
            
            ENDCG
        }
    }
}