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
    public struct CityIconSwitchJob : IJobParallelFor
    {
        [ReadOnly] public NativeList<CityInfo> inCities;
        [ReadOnly] public CityIconSwitchJobLod inIconSwitchLod;

        [ReadOnly] public NativeArray<ArchetypeChunk> inChunks;
        [ReadOnly] public ComponentTypeHandle<PositionComponent> positionHandle;
        [ReadOnly] public ComponentTypeHandle<CityComponent> cityHandle;
        [ReadOnly] public Bounds2D inCameraViewBounds;
        
        public ComponentTypeHandle<SpriteComponent> spriteHandle;
        
        public void Execute(int chunkIndex)
        {
            ArchetypeChunk chunk = inChunks[chunkIndex];
            NativeArray<PositionComponent> positions = chunk.GetNativeArray(ref positionHandle);
            NativeArray<SpriteComponent> sprites = chunk.GetNativeArray(ref spriteHandle);
            NativeArray<CityComponent> cities = chunk.GetNativeArray(ref cityHandle);

            for (int entityIndex = 0; entityIndex < chunk.Count; entityIndex++)
            {
                ushort cityID = cities[entityIndex].id;
                byte citySize = inCities[cityID].size;
                float2 position = positions[entityIndex].value;

                ushort targetSpriteIndex = 0;

                if (inCameraViewBounds.Contains(position))
                {
                    if (CitySpawner.MAJOR_CITY_SIZE.Contains(citySize))
                    {
                        targetSpriteIndex = inIconSwitchLod.majorCityIconID;
                    }
                    else
                    {
                        targetSpriteIndex = inIconSwitchLod.minorCityIconID;
                    }
                }

                if (targetSpriteIndex != sprites[entityIndex].value)
                {
                    sprites[entityIndex] = new SpriteComponent(targetSpriteIndex);
                }
            }
        }
    }
}