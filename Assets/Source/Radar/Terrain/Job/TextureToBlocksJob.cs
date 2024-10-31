using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Beast.Radar.Terrain
{
    [BurstCompile]
    public struct TextureToBlocksJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<short> inputTexture;

        public int inputTextureSize;
        
        [WriteOnly, NativeDisableParallelForRestriction] 
        public NativeArray<byte> outputTextureBlocks;

        public int outputBlockSize;
     
        public void Execute(int index)
        {
            int blockCount = inputTextureSize / outputBlockSize;
            int outputOffset = index * outputBlockSize * outputBlockSize;

            int inputX = index % blockCount * outputBlockSize;
            int inputY = index / blockCount * outputBlockSize;

            for (int pixel = 0; pixel < outputBlockSize * outputBlockSize; pixel++)
            {
                int x = inputX + pixel % outputBlockSize;
                int y = inputY + pixel / outputBlockSize;
                int inputOffset = y * inputTextureSize + x;
                outputTextureBlocks[outputOffset + pixel] = (byte)inputTexture[inputOffset];
            }
        }
    }
}