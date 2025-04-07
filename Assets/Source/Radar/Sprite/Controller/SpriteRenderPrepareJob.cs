using Beast.Claw.Jobs;
using Beast.Claw.Math;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Beast.Radar.Sprite
{
    [BurstCompile]
    public struct SpriteRenderPrepareJob : IMultiJob
    {
        [ReadOnly] public NativeArray<ArchetypeChunk> inChunks;
        [ReadOnly] public NativeArray<int> inWriteOffset;
        [ReadOnly] public ComponentTypeHandle<PositionComponent> positionHandle;
        [ReadOnly] public ComponentTypeHandle<SpriteComponent> spriteHandle;
        [ReadOnly] public NativeSlice<SpriteInfo> spritesData;
        [ReadOnly] public ComponentTypeHandle<SpriteOffsetComponent> spriteOffsetHandle;
        
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<ushort> outData;
        [WriteOnly] public NativeArray<int> outEntityCounters;
        
        public void Execute(int chunkIndex)
        {
            ArchetypeChunk chunk = inChunks[chunkIndex];
            NativeArray<PositionComponent> positions = chunk.GetNativeArray(ref positionHandle);
            NativeArray<SpriteComponent> sprites = chunk.GetNativeArray(ref spriteHandle);
            NativeArray<SpriteOffsetComponent> spriteOffsets = chunk.GetNativeArray(ref spriteOffsetHandle);

            bool hasScreenOffset = chunk.Has<SpriteOffsetComponent>();

            int writeOffset = inWriteOffset[chunkIndex] * SpriteRender.HALF_PER_SPRITE;
            int spriteCount = 0;

            for (int entityID = 0; entityID < chunk.Count; entityID++)
            {
                ushort spriteID = sprites[entityID].value;

                if (spriteID == 0)
                {
                    continue;
                }
                
                float2 position = positions[entityID].value;

                SpriteInfo sprite = spritesData[spriteID];
                
                int2 screenOffset = int2.zero;

                if (hasScreenOffset)
                {
                    screenOffset = spriteOffsets[entityID].value;
                }
                
                BConvert.FloatToHalf2(position.x, out ushort px0, out ushort px1);
                BConvert.FloatToHalf2(position.y, out ushort py0, out ushort py1);

                outData[writeOffset + 0] = px0;
                outData[writeOffset + 1] = px1;
                outData[writeOffset + 2] = py0;
                outData[writeOffset + 3] = py1;
                outData[writeOffset + 4] = BConvert.FloatToHalf(sprite.atlasOffset.x);
                outData[writeOffset + 5] = BConvert.FloatToHalf(sprite.atlasOffset.y);
                outData[writeOffset + 6] = BConvert.UByte2ToHalf(sprite.size.x, sprite.size.y);
                outData[writeOffset + 7] = BConvert.SByte2ToHalf(screenOffset.x, screenOffset.y);
                
                spriteCount++;
                writeOffset += SpriteRender.HALF_PER_SPRITE;
            }

            outEntityCounters[chunkIndex] = spriteCount;
        }
    }
}