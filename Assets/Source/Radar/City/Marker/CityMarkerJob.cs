using Beast.Claw.Math;
using Beast.Radar.Sprite;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Beast.Radar.City
{
    [BurstCompile]
    public struct CityMarkerJob : IJobParallelFor
    {
        public const int MAX_COMMAND_COUNT = 4;
        
        [ReadOnly] public NativeSlice<CityInfo> inCities;
        [ReadOnly] public CityMarkerLodNative inIconSwitchLodNative;

        [ReadOnly] public NativeArray<ArchetypeChunk> inChunks;
        [ReadOnly] public ComponentTypeHandle<PositionComponent> positionHandle;
        [ReadOnly] public ComponentTypeHandle<CityIDComponent> cityIDHandle;
        [ReadOnly] public Bounds2D inCameraViewBounds;
        [ReadOnly] public EntityTypeHandle entityHandle;
        [ReadOnly] public NativeSlice<SpriteInfo> inSprites;

        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<CityLabelCommand> outLabelCommands;
        [WriteOnly] public NativeArray<byte> outLabelCommandCounters;

        private int _outLabelCommandCount;
        
        public ComponentTypeHandle<SpriteComponent> spriteHandle;
        
        public void Execute(int chunkIndex)
        {
            ArchetypeChunk chunk = inChunks[chunkIndex];
            NativeArray<PositionComponent> positions = chunk.GetNativeArray(ref positionHandle);
            NativeArray<SpriteComponent> sprites = chunk.GetNativeArray(ref spriteHandle);
            NativeArray<CityIDComponent> cities = chunk.GetNativeArray(ref cityIDHandle);
            NativeArray<Entity> entities = chunk.GetNativeArray(entityHandle);

            bool isLabel = chunk.Has<CityLabelComponent>();
            _outLabelCommandCount = 0;

            for (int entityIndex = 0; entityIndex < chunk.Count; entityIndex++)
            {
                ushort cityID = cities[entityIndex].id;
                float2 position = positions[entityIndex].value;
                ushort currentSpriteID = sprites[entityIndex].value;

                bool isMajor = CitySpawner.MAJOR_CITY_SIZE.Contains(inCities[cityID].size);

                if (isLabel)
                {
                    int targetLabelSize = isMajor ? inIconSwitchLodNative.majorLabelSize : inIconSwitchLodNative.minorLabelSize;

                    bool labelMustBeVisible = inCameraViewBounds.Contains(position) && targetLabelSize > 0;
                    
                    if (labelMustBeVisible && currentSpriteID == 0)
                    {
                        _addLabelCommand(chunkIndex, new CityLabelCommand
                        {
                            commandType = ECityLabelCommandType.CreateLabel,
                            targetEntity = entities[entityIndex],
                            labelSize = (byte) targetLabelSize,
                            cityID = cityID
                        });
                        
                        continue;
                    }

                    if (labelMustBeVisible && inSprites[currentSpriteID].size.y != targetLabelSize)
                    {
                        _addLabelCommand(chunkIndex, new CityLabelCommand
                        {
                            commandType = ECityLabelCommandType.ChangeLabelResolution,
                            targetEntity = entities[entityIndex],
                            labelSize = (byte) targetLabelSize,
                            spriteID = currentSpriteID,
                            cityID = cityID
                        });
                    }
                    
                    if (!labelMustBeVisible && currentSpriteID != 0)
                    {
                        _addLabelCommand(chunkIndex, new CityLabelCommand
                        {
                            commandType = ECityLabelCommandType.ReleaseLabel,
                            targetEntity = entities[entityIndex],
                            spriteID = currentSpriteID
                        });
                        
                        continue;
                    }
                }
                else
                {
                    ushort targetSpriteIndex = 0;

                    if (inCameraViewBounds.Contains(position))
                    {
                        targetSpriteIndex = isMajor ? inIconSwitchLodNative.majorCityIconID : inIconSwitchLodNative.minorCityIconID;
                    }

                    if (targetSpriteIndex != currentSpriteID)
                    {
                        sprites[entityIndex] = new SpriteComponent(targetSpriteIndex);
                    }
                }
            }

            outLabelCommandCounters[chunkIndex] = (byte)_outLabelCommandCount;
        }

        private void _addLabelCommand(int chunkIndex, CityLabelCommand command)
        {
            if (_outLabelCommandCount >= MAX_COMMAND_COUNT)
            {
                return;
            }

            int commandIndex = chunkIndex * MAX_COMMAND_COUNT + _outLabelCommandCount;

            outLabelCommands[commandIndex] = command;

            _outLabelCommandCount++;
        }
    }
}