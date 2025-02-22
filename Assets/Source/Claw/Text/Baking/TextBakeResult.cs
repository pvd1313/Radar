using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Text.Baking
{
    public struct TextBakeResult
    {
        public Texture2D texture;
        public int2 origin;
        public int2 size;
    }
}