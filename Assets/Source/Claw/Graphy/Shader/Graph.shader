Shader "Beast/Claw/Graphy/Graph"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap("Pixel snap", Float) = 0

        _GoodColor("Good Color", Color) = (1,1,1,1)
        _CautionColor("Caution Color", Color) = (1,1,1,1)
        _CriticalColor("Critical Color", Color) = (1,1,1,1)

        _GoodThreshold("Good Threshold", Float) = 0.5
        _CautionThreshold("Caution Threshold", Float) = 0.25
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vertex
            #pragma fragment fragment
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile FULL

            #include "UnityCG.cginc"

            struct ApplicationDataType
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VertexToFragment
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            fixed4 _Color;

            VertexToFragment vertex(ApplicationDataType IN)
            {
                VertexToFragment OUT;

                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_OUTPUT(VertexToFragment, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif

                return OUT;
            }

            sampler2D _MainTex;
            sampler2D _AlphaTex;
            float _AlphaSplitEnabled;

            fixed4 sampleSpriteTexture(float2 uv)
            {
                fixed4 color = tex2D(_MainTex, uv);

                #if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
                if (_AlphaSplitEnabled)
                    color.a = tex2D(_AlphaTex, uv).r;
                #endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED

                return color;
            }

            fixed4 _GoodColor;
            fixed4 _CautionColor;
            fixed4 _CriticalColor;

            fixed _GoodThreshold;
            fixed _CautionThreshold;

            uniform float Average;

            // NOTE: The size of this array can break compatibility with some older GPUs
            // This shader is pretty much equal to GraphStandard.shader but with a smaller Array size.
#ifdef FULL
            uniform float GraphValues[512];
#else
             uniform float GraphValues[128];
#endif
            
            uniform float GraphValues_Length;

            fixed4 fragment(VertexToFragment IN) : SV_Target
            {
                fixed4 color = IN.color;

                const fixed x_coord = IN.texcoord.x;
                const fixed y_coord = IN.texcoord.y;

                const float graph_value = GraphValues[floor(x_coord * GraphValues_Length)];

                // Define the width of each element of the graph
                const float increment = 1.0f / (GraphValues_Length - 1);

                // Assign the corresponding color
                if (graph_value > _GoodThreshold)
                {
                    color *= _GoodColor;
                }
                else if (graph_value > _CautionThreshold)
                {
                    color *= _CautionColor;
                }
                else
                {
                    color *= _CriticalColor;
                }

                // Point coloring
                if (graph_value - y_coord > increment * 4)
                {
                    //color.a = yCoord * graphValue * 0.3;
                    color.a *= y_coord * 0.3 / graph_value;
                }

                // Set as transparent the part on top of the current point value
                if (y_coord > graph_value)
                {
                    color.a = 0;
                }

                // Average white bar
                if (y_coord < Average && y_coord > Average - 0.02)
                {
                    color = fixed4(1, 1, 1, 1);
                }

                // CautionColor bar
                if (y_coord < _CautionThreshold && y_coord > _CautionThreshold - 0.02)
                {
                    color = _CautionColor;
                }

                // GoodColor bar
                if (y_coord < _GoodThreshold && y_coord > _GoodThreshold - 0.02)
                {
                    color = _GoodColor;
                }

                // Fade the alpha of the sides of the graph
                if (x_coord < 0.03)
                {
                    color.a *= 1 - (0.03 - x_coord) / 0.03;
                }
                else if (x_coord > 0.97)
                {
                    color.a *= (1 - x_coord) / 0.03;
                }
                
                color.rgb *= color.a;
                
                return color;
            }
            ENDCG
        }
    }
}