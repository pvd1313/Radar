using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Graphic
{
    public struct DrawMeshParameters
    {
        public Mesh mesh;
        public Camera targetCamera;
        public float4x4 matrix;
        public Material material;
        public int layer;
        public int subMeshIndex;
        public MaterialPropertyBlock materialProperties;
    }
}