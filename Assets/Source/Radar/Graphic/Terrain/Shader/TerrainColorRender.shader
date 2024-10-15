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

            #pragma target 2.0
            
            #pragma vertex vertex
            #pragma fragment fragment

            #include "UnityCG.cginc"
            #include "Assets/Source/Radar/Graphic/Main/RadarCG.cginc"

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

            sampler2D _TerrainIndexTexture;
            float2 _TerrainIndexTextureSize;

            sampler2D _TileAtlasColorTexture;
            float2 _TileAtlasTileCount;

            float4 _UVMatrix0;
            float4 _UVMatrix1;
            
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
                
                output.uv = screen_to_world_uv(uv, _TerrainIndexTextureSize);
                
                return output;
            }

            float4 fragment(FragmentInput input) : SV_Target
            {
                uint tileIndex = f32tof16(tex2D(_TerrainIndexTexture, input.uv).r);
                float2 tileXY = beast_tile_xy_from_index(tileIndex, _TileAtlasTileCount);
                float2 atlasTexelSize = 1.0f / _TileAtlasTileCount;
                float4 tileColor = tex2D(_TileAtlasColorTexture, tileXY * atlasTexelSize + atlasTexelSize * 0.5f);
                
                return tileColor;
            }
            
            ENDCG
        }
    }
}