Shader "Beast/Radar/TerrainColorRender"
{
    Properties
    {
        
    }
    SubShader
    {
        cull off
        zwrite off
        ztest on

        Pass
        {
            CGPROGRAM

            #pragma target 1.0
            
            #pragma vertex vertex
            #pragma fragment fragment

            #include "UnityCG.cginc"
            #include "Assets/Source/Claw/Graphic/BeastCG.cginc"

            struct VertexInput
            {
                float4 positionOS : POSITION;
                float2 screenUV : TEXCOORD0;
            };

            struct FragmentInput
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            };

            sampler2D _TerrainTexture;
            float2 _TerrainTextureSize;

            float4 _UVMatrix0;
            float4 _UVMatrix1;
            float4x4 _ComplexMatrix;
            
            float _OutputAspect;
            float _OutputHeight;
            float2 _OutputCenter;

            float2 screen_to_world_uv(float2 uv, float2 mapSize)
            {
                return uv * (_OutputHeight / mapSize) + _OutputCenter / mapSize;
            }

            FragmentInput vertex(VertexInput input)
            {
                FragmentInput output;
                
                output.positionCS = UnityObjectToClipPos(input.positionOS);

                float2 uv = (input.screenUV - 0.5) * float2(_OutputAspect, 1);

                uv = mul(float2x2(_UVMatrix0), uv);
                uv = mul(float2x2(_UVMatrix1), uv);
                
                output.uv = screen_to_world_uv(uv, _TerrainTextureSize);
                
                return output;
            }

            float4 fragment(FragmentInput input) : SV_Target
            {
                return tex2D(_TerrainTexture, input.uv);
            }
            
            ENDCG
        }
    }
}