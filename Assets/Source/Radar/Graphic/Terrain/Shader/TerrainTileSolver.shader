Shader "Beast/IsometricSolver"
{
    Properties
    {
        
    }
    SubShader
    {
        cull off
        zwrite off
        ztest off

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
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct FragmentInput
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            float2 _TileGridSize;
            
            sampler2D _TerrainTexture;
            float2 _TerrainUVOffset;
            float2 _TerrainTexelSize;

            sampler2D _NoiseTexture;
            float2 _NoiseSize;
            float2 _NoiseUVOffset;
            float _NoiseTexelSize;
            
            float _SolverTextureSize;

            FragmentInput vertex(VertexInput input)
            {
                FragmentInput output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                
                return output;
            }

            // There will be more complex or index map texture in future.
            uint terrain_color_to_tile_index(float terrainColor)
            {
                // 0 .. 1 just for now. 0 - grass. 1 - water.
                return saturate(1 - terrainColor);
            }

            float4 fragment(FragmentInput input) : SV_Target
            {
                uint x = floor(input.uv.x * _SolverTextureSize);
                uint y = floor(input.uv.y * _SolverTextureSize);
                uint instanceID = y * (uint)_SolverTextureSize + x;

                BeastIsometricTile tile = beast_isometric_tile(instanceID, _TileGridSize.x);
                float2 offset = beast_isometric_world_offset(tile, _TileGridSize.x);

                float terrainColor = tex2D(_TerrainTexture, _TerrainUVOffset + offset * _TerrainTexelSize).r;
                uint tileIndex = terrain_color_to_tile_index(terrainColor);

                float noiseColor = tex2D(_NoiseTexture, _NoiseUVOffset + offset * _NoiseTexelSize).r;
                
                return float4(f16tof32(tileIndex), noiseColor, 0, 0);
            }
            ENDCG
        }
    }
}