Shader "Beast/IsometricSolver"
{
    Properties
    {
        
    }
    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

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

            // Rename to tile grid size
            float2 _GridSize;
            
            // Rename to terrain
            sampler2D _WorldTexture;
            float2 _WorldTextureSize;
            float2 _WorldTextureOffset;
            
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

                BeastIsometricTile tile = beast_isometric_tile(instanceID, _GridSize.x);
                float2 offset = beast_isometric_world_offset(tile, _GridSize.x);
                offset += _WorldTextureOffset;

                return tex2D(_WorldTexture, offset / _WorldTextureSize);
                
                return instanceID / (_GridSize.x * _GridSize.y + (_GridSize.x - 1) * (_GridSize.y - 1));
            }
            ENDCG
        }
    }
}