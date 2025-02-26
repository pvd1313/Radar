using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Text.Baking
{
    public struct TextBakeResult
    {
        public Texture texture;
        public int2 offset;
        public int2 size;
    }
}