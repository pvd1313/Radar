using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Beast.Radar.Terrain
{
    [BurstCompile]
    public struct CreateAveragePixelCountTextureJob : IJobParallelFor
    {
        [ReadOnly] public TextureWithBlocks sourceTexture;

        [WriteOnly, NativeDisableParallelForRestriction] 
        public NativeArray<byte> outputTexture;

        public int outputCellSize;
        
        public void Execute(int blockIndex)
        {
            int cellsPerBlock = sourceTexture.blockSize / outputCellSize;
            int cellsPerBlockSqr = cellsPerBlock * cellsPerBlock;

            int2 blockXY;
            blockXY.x = blockIndex % sourceTexture.blockCountX;
            blockXY.y = blockIndex / sourceTexture.blockCountX;
            int2 blockXYAbs = blockXY * sourceTexture.blockSize;
            int blockOffset = sourceTexture.GetBlockOffset(blockXYAbs);

            int cellHalfPixelCount = outputCellSize * outputCellSize / 2;
            
            for (int cellIndex = 0; cellIndex < cellsPerBlockSqr; cellIndex++)
            {
                ByteArray16 pixelCount = default;

                int2 cellXY;
                cellXY.x = cellIndex % cellsPerBlock;
                cellXY.y = cellIndex / cellsPerBlock;
                int2 cellXYAbs = blockXYAbs + cellXY * outputCellSize;

                int maxColorIndex = 255;

                for (int pixelIndex = 0; pixelIndex < outputCellSize; pixelIndex++)
                {
                    int2 pixelXY;
                    pixelXY.x = pixelIndex % outputCellSize;
                    pixelXY.y = pixelIndex / outputCellSize;

                    int innerOffset = sourceTexture.GetInnerOffset(cellXYAbs + pixelXY);
                    
                    byte pixelColor = sourceTexture.texture[blockOffset + innerOffset];
                    
                    byte count = pixelCount.GetByte(pixelColor);

                    count++;
                    
                    pixelCount.SetByte(pixelColor, (byte)(count + 1));

                    if (count >= cellHalfPixelCount)
                    {
                        maxColorIndex = pixelColor;
                        break;
                    }
                }

                if (maxColorIndex == 255)
                {
                    maxColorIndex = 0;
                    byte maxColorCount = pixelCount.GetByte(0);
                    for (int i = 0; i < 16; i++)
                    {
                        byte colorCount = pixelCount.GetByte(i);
                        if (colorCount > maxColorCount)
                        {
                            maxColorCount = colorCount;
                            maxColorIndex = i;
                        }
                    }
                }

                int outputCellX = blockXY.x * cellsPerBlock + cellXY.x;
                int outputCellY = blockXY.y * cellsPerBlock + cellXY.y;
                int outputOffset = outputCellY * (sourceTexture.blockCountX * cellsPerBlock) + outputCellX;
                
                outputTexture[outputOffset] = (byte)maxColorIndex;
            }
        }
    }
}