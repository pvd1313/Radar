Shader "Beast/Radar/Terrain/ColorRender"
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

            #include "UnityCG.cginc"
            #include "Assets/Source/Radar/Graphic/Radar.cginc"

            struct VertexInput
            {
                float4 positionOS : POSITION;
                float2 quadUV : TEXCOORD0;
            };

            struct FragmentInput
            {
                float4 positionCS : SV_POSITION;
                float2 worldUV : TEXCOORD0;
            };

            sampler2D _TerrainBiomeTexture;
            float2 _TerrainBiomeTextureSize;

            sampler2D _TileAtlasColorTexture;
            float2 _TileAtlasTileCount;

            float4 _UVMatrix0;
            float4 _UVMatrix1;
            
            float2 _OutputSize;
            float2 _OutputCenter;

            float2 screen_to_world_uv(float2 uv, float2 mapSize)
            {
                return uv * (_OutputSize.y / mapSize) + _OutputCenter / mapSize;
            }

            FragmentInput vertex(VertexInput input)
            {
                FragmentInput output;
                
                output.positionCS = UnityObjectToClipPos(input.positionOS);

                float2 clipUV = beast_quad_to_clip_uv(input.quadUV, _OutputSize);

                clipUV = mul(float2x2(_UVMatrix0), clipUV);
                clipUV = mul(float2x2(_UVMatrix1), clipUV);
                
                output.worldUV = screen_to_world_uv(clipUV, _TerrainBiomeTextureSize);
                
                return output;
            }

            float4 fragment(FragmentInput input) : SV_Target
            {
                float terrainR = tex2D(_TerrainBiomeTexture, input.worldUV).r;
                uint tileIndex = f32tof16(terrainR);
                float2 tileXY = beast_tile_xy_from_index(tileIndex, _TileAtlasTileCount);
                float2 atlasTexelSize = 1.0f / _TileAtlasTileCount;
                float4 tileColor = tex2D(_TileAtlasColorTexture, tileXY * atlasTexelSize + atlasTexelSize * 0.5f);
                
                return tileColor;
            }
            
            ENDCG
        }
    }
}