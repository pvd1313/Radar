Shader "Beast/IsometricSolver"
{
    Properties
    {
        
    }
    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Off

        Pass
        {
            CGPROGRAM

            #pragma target 2.0
            
            #pragma vertex vertex
            #pragma fragment fragment

            #include "UnityCG.cginc"
            #include "Assets/Source/Claw/CG/BeastCG.cginc"

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

            sampler2D _TileAtlas;
            float2 _TileAtlasWidthHeight;
            
            sampler2D _TerrainTexture;
            float2 _TerrainSize;
            float2 _TerrainOffset;
            
            float _OutputTextureSize;

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
                uint x = floor(input.uv.x * _OutputTextureSize);
                uint y = floor(input.uv.y * _OutputTextureSize);
                uint instanceID = y * (uint)_OutputTextureSize + x;

                BeastIsometricTile tile = beast_isometric_tile(instanceID, _TileGridSize.x);
                float2 offset = beast_isometric_world_offset(tile, _TileGridSize.x);
                offset += _TerrainOffset;

                float4 terrainColor = tex2D(_TerrainTexture, offset / _TerrainSize);

                uint tileIndex = terrain_color_to_tile_index(terrainColor.r);

                // TODO: This might be not save to transfer raw float.
                return float4(tileIndex, 0, 0, 0);
            }
            ENDCG
        }
    }
}