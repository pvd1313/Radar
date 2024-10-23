Shader "Beast/Radar/Terrain/TileSolver"
{
    SubShader
    {
        // Rendered to buffer.
        cull off
        zwrite off
        ztest off

        Pass
        {
            CGPROGRAM

            #pragma target 2.0
            
            #pragma vertex vertex
            #pragma fragment fragment

            #include "UnityCG.cginc"
            #include "Assets/Source/Radar/Graphic/RadarCG.cginc"

            struct VertexInput
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct FragmentInput
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            };
            
            float2 _TileGridSize;

            sampler2D _TileAtlasConfig;
            float2 _TileAtlasTileCount;
            
            sampler2D _TerrainTexture;
            float2 _TerrainUVOffset;
            float2 _TerrainTexelSize;
            
            float _SolverTextureSize;

            FragmentInput vertex(VertexInput input)
            {
                FragmentInput output;
                output.positionCS = UnityObjectToClipPos(input.positionOS);
                output.uv = input.uv;
                
                return output;
            }

            float4 fragment(FragmentInput input) : SV_Target
            {
                uint solverX = floor(input.uv.x * _SolverTextureSize);
                uint solverY = floor(input.uv.y * _SolverTextureSize);
                uint instanceID = solverY * (uint)_SolverTextureSize + solverX;

                BeastIsometricTile tile = beast_isometric_tile(instanceID, _TileGridSize.x);
                float2 offset = beast_isometric_world_offset(tile, _TileGridSize.x);

                float terrainR = tex2D(_TerrainTexture, _TerrainUVOffset + offset * _TerrainTexelSize).r;

                // Currently we are just passing the same tile index.
                // In future we will have to process adjusted tile variations.
                uint tileIndex = f32tof16(terrainR);
                float2 tileXY = beast_tile_xy_from_index(tileIndex, _TileAtlasTileCount);
                float2 configTexelSize = 1.0f / _TileAtlasTileCount;
                float2 configRG = tex2D(_TileAtlasConfig, tileXY * configTexelSize + configTexelSize * 0.5f);
                
                return float4(f16tof32(tileIndex), 0, configRG.r, configRG.g);
            }
            
            ENDCG
        }
    }
}