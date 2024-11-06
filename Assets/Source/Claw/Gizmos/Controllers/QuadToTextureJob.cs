using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Beast.Claw.Gizmos
{
    [BurstCompile]
    public struct QuadToTextureJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<GizmosQuad> quads;

        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<byte> rgba32Texture;
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<float> rgFloatTexture;
        
        public void Execute(int quadIndex)
        {
            GizmosQuad quad = quads[quadIndex];

            int rgba32Offset = quadIndex * 4;

            rgba32Texture[rgba32Offset + 0] = (byte)quad.color.x;
            rgba32Texture[rgba32Offset + 1] = (byte)quad.color.y;
            rgba32Texture[rgba32Offset + 2] = (byte)quad.color.z;
            rgba32Texture[rgba32Offset + 3] = (byte)quad.color.w;

            int rgFloatOffset = quadIndex * 2;

            rgFloatTexture[rgFloatOffset + 0] = quad.center.x / ushort.MaxValue;
            rgFloatTexture[rgFloatOffset + 1] = quad.center.y / ushort.MaxValue;
        }
    }
}