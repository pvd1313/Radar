using Beast.Claw.Math;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Beast.Radar.Sprite
{
    [BurstCompile]
    public struct SpriteRenderPrepareJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<ArchetypeChunk> inChunks;
        [ReadOnly] public NativeArray<int> inWriteOffset;
        [ReadOnly] public ComponentTypeHandle<PositionComponent> positionHandle;
        [ReadOnly] public ComponentTypeHandle<SpriteComponent> spriteHandle;
        [ReadOnly] public NativeList<SpriteInfo> spritesData;
        
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<ushort> outData;
        [WriteOnly] public NativeArray<int> outEntityCounters;
        
        public void Execute(int chunkIndex)
        {
            ArchetypeChunk chunk = inChunks[chunkIndex];
            NativeArray<PositionComponent> positions = chunk.GetNativeArray(ref positionHandle);
            NativeArray<SpriteComponent> sprites = chunk.GetNativeArray(ref spriteHandle);

            int writeOffset = inWriteOffset[chunkIndex] * SpriteRender.HALF_PER_SPRITE;
            int spriteCount = 0;

            for (int i = 0; i < chunk.Count; i++)
            {
                float2 position = positions[i].value;
                ushort spriteID = sprites[i].value;

                SpriteInfo sprite = spritesData[spriteID];

                BConvert.FloatToHalf2(position.x, out ushort px0, out ushort px1);
                BConvert.FloatToHalf2(position.y, out ushort py0, out ushort py1);

                outData[writeOffset + 0] = px0;
                outData[writeOffset + 1] = px1;
                outData[writeOffset + 2] = py0;
                outData[writeOffset + 3] = py1;
                outData[writeOffset + 4] = BConvert.FloatToHalf(sprite.offset.x);
                outData[writeOffset + 5] = BConvert.FloatToHalf(sprite.offset.y);
                outData[writeOffset + 6] = BConvert.FloatToHalf(sprite.size.x);
                outData[writeOffset + 7] = BConvert.FloatToHalf(sprite.size.y);
                
                spriteCount++;
                writeOffset += SpriteRender.HALF_PER_SPRITE;
            }

            outEntityCounters[chunkIndex] = spriteCount;
        }
    }
}