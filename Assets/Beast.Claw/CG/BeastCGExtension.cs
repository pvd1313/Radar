using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.CG
{
    public static class BeastCGExtension
    {
        public static float2 WidthHeight(this Texture2D texture2D)
        {
            return new float2(texture2D.width, texture2D.height);
        }
    }
}