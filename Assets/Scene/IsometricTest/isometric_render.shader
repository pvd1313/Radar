Shader "Beast/IsometricRender"
{
    Properties
    {
        
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
        }
        
        LOD 100

        Pass
        {
            CGPROGRAM

            #pragma target 2.0
            
            #pragma vertex vertex
            #pragma fragment fragment

            #include "UnityCG.cginc"

            struct FragmentInput
            {
                // float2 mainUV : TEXCOORD0;
                float cameraDot : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            };

            float _GridSize;
            float3 _CameraDirection;

            FragmentInput vertex(appdata_base input, uint instanceID : SV_InstanceID)
            {
                FragmentInput output;

                float gridScale = 2;

                input.vertex.xz -= gridScale * _GridSize * 0.5;

                input.vertex.y += 0.5;
                input.vertex.x += floor(instanceID / _GridSize) * gridScale;
                input.vertex.z += floor(instanceID % _GridSize) * gridScale;
                
                output.positionCS = UnityObjectToClipPos(input.vertex);
                output.cameraDot = saturate(dot(_CameraDirection, -input.normal));

                return output;
            }

            float4 fragment(FragmentInput input) : SV_Target
            {
                return input.cameraDot;
            }
            
            ENDCG
        }
    }
}