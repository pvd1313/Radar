// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Beast/Claw/TextBaker/Font"
{
    Properties
    {
        _MainTex ("Font Texture", 2D) = "white" {}
        _Color ("Text Color", Color) = (1,1,1,1)
    }

    SubShader {

        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }
        
        Lighting Off Cull Off ZTest Always ZWrite Off
        
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vertex
            #pragma fragment fragment
            #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
            #include "UnityCG.cginc"

            struct VertexInput
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct FragmentInput
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _BackgroundColor;

            FragmentInput vertex (VertexInput input)
            {
                FragmentInput output;
                
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                output.vertex = UnityObjectToClipPos(input.vertex);
                
                output.color = input.color * _Color;
                
                output.texcoord = TRANSFORM_TEX(input.uv, _MainTex);
                
                return output;
            }

            float4 fragment (FragmentInput input) : SV_Target
            {
                float alpha = tex2D(_MainTex, input.texcoord).a;

                alpha = pow(alpha, 0.4);

                float4 col = lerp(_BackgroundColor, input.color, alpha);
                
                return col;
            }
            ENDCG
        }
    }
}