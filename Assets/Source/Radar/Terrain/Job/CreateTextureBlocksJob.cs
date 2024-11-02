using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Beast.Radar.Terrain
{
    [BurstCompile]
    public struct CreateTextureBlocksJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<short> inputTexture;

        public int inputTextureSize;
        
        [WriteOnly, NativeDisableParallelForRestriction] 
        public NativeArray<byte> outputTextureBlocks;

        public int outputBlockSize;
     
        public void Execute(int blockIndex)
        {
            int blockCount = inputTextureSize / outputBlockSize;
            int outputOffset = blockIndex * outputBlockSize * outputBlockSize;

            int inputX = blockIndex % blockCount * outputBlockSize;
            int inputY = blockIndex / blockCount * outputBlockSize;

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