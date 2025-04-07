using Beast.Claw.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Beast.Radar.Terrain
{
    [BurstCompile]
    public struct CreateAveragePixelCountTextureJob : IMultiJob
    {
        [ReadOnly] public TextureWithBlocks<TerrainCell> sourceTexture;

        [WriteOnly, NativeDisableParallelForRestriction] 
        public NativeArray<TerrainCell> outputTexture;

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

            int outputCellSizeSqr = outputCellSize * outputCellSize;
            int outputCellSizeSqrHalf = outputCellSizeSqr / 2;
            
            for (int cellIndex = 0; cellIndex < cellsPerBlockSqr; cellIndex++)
            {
                ByteArray16 pixelCount = default;

                int2 cellXY;
                cellXY.x = cellIndex % cellsPerBlock;
                cellXY.y = cellIndex / cellsPerBlock;
                int2 cellXYAbs = blockXYAbs + cellXY * outputCellSize;

                int maxColorIndex = 255;

                for (int pixelIndex = 0; pixelIndex < outputCellSizeSqr; pixelIndex++)
                {
                    int2 pixelXY;
                    pixelXY.x = pixelIndex % outputCellSize;
                    pixelXY.y = pixelIndex / outputCellSize;

                    int innerOffset = sourceTexture.GetInnerOffset(cellXYAbs + pixelXY);
                    
                    byte pixelColor = (byte) sourceTexture.texture[blockOffset + innerOffset].Biome;
                    
                    byte count = pixelCount.GetByte(pixelColor);

                    count++;

                    if (count >= outputCellSizeSqrHalf)
                    {
                        maxColorIndex = pixelColor;
                        break;
                    }
                    
                    pixelCount.SetByte(pixelColor, count);
                }

                if (maxColorIndex == 255)
                {
                    maxColorIndex = 0;
                    byte maxColorCount = pixelCount.GetByte(0);
                    for (int i = 1; i < 16; i++)
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
                
                outputTexture[outputOffset] = new TerrainCell((ETerrainBiome) maxColorIndex, ETerrainFeature.None);
            }
        }
    }
}