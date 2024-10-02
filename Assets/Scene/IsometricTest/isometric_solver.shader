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

            float4 fragment(FragmentInput input) : SV_Target
            {
                uint x = floor(input.uv.x * _OutputTextureSize);
                uint y = floor(input.uv.y * _OutputTextureSize);
                uint instanceID = y * (uint)_OutputTextureSize + x;

                BeastIsometricTile tile = beast_isometric_tile(instanceID, _TileGridSize.x);
                float2 offset = beast_isometric_world_offset(tile, _TileGridSize.x);
                offset += _TerrainOffset;

                return tex2D(_TerrainTexture, offset / _TerrainSize);
                
                return instanceID / (_TileGridSize.x * _TileGridSize.y + (_TileGridSize.x - 1) * (_TileGridSize.y - 1));
            }
            ENDCG
        }
    }
}