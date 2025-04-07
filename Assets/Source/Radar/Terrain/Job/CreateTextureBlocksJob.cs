using Beast.Claw.Jobs;
using Unity.Burst;
using Unity.Collections;

namespace Beast.Radar.Terrain
{
    [BurstCompile]
    public struct CreateTextureBlocksJob<TValue, TConverter> : IMultiJob 
        where TValue : unmanaged
        where TConverter : unmanaged, IShortTextureConverter<TValue>
    {
        [ReadOnly]
        public NativeArray<short> inputTexture;

        public int inputTextureSize;
        
        [WriteOnly, NativeDisableParallelForRestriction] 
        public NativeArray<TValue> outputTextureBlocks;

        public int outputBlockSize;
     
        public void Execute(int blockIndex)
        {
            TConverter converter = default;
            
            int blockCount = inputTextureSize / outputBlockSize;
            int outputOffset = blockIndex * outputBlockSize * outputBlockSize;

            int inputX = blockIndex % blockCount * outputBlockSize;
            int inputY = blockIndex / blockCount * outputBlockSize;

            for (int pixel = 0; pixel < outputBlockSize * outputBlockSize; pixel++)
            {
                int x = inputX + pixel % outputBlockSize;
                int y = inputY + pixel / outputBlockSize;
                int inputOffset = y * inputTextureSize + x;
                outputTextureBlocks[outputOffset + pixel] = converter.Convert(inputTexture[inputOffset]);
            }
        }
    }
}