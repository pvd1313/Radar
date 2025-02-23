using System.Collections.Generic;
using Beast.Claw.Attributes;
using Beast.Claw.ECS;
using Beast.Claw.GameLoop;
using Beast.Claw.Graphic;
using Beast.Claw.Text.Baking;
using Beast.Radar.Sprite;
using Beast.Radar.World;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using TerrainData = Beast.Radar.Terrain.TerrainData;

namespace Beast.Radar.City
{
    public class CityIconManager : Actor
    {
        [Required] private readonly IEntityManager _entityManager;
        [Required] private readonly ICitySpawner _citySpawner;
        [Required] private readonly IWorldManager _worldManager;
        [Required] private readonly ISpriteAtlas _spriteAtlas;
        [Required] private readonly CityIconConfig _config;
        [Required] private readonly ICameraTransform _camera;
        
        private TerrainData _bakedForTerrain;
        private EntityArchetype _iconArchetype;
        private EntityQuery _iconQuery;
        private NativeArray<Entity> _icons;
        private TextBaker _textBaker;
        private Dictionary<Texture2D, ushort> _textureID;

        private ushort _majorCitySpriteID;
        private ushort _minorCitySpriteID;
        private ushort _dotSpriteID;
        
        protected override UniTask _warmup(ConcurrentContext context)
        {
            ComponentType[] iconComponents = {
                typeof(PositionComponent),
                typeof(SpriteComponent),
                typeof(CityComponent)
            };
            
            _iconArchetype = _entityManager.CreateArchetype(iconComponents);
            _iconQuery = _entityManager.CreateEntityQuery(iconComponents);

            _textureID = new Dictionary<Texture2D, ushort>();
            foreach (CityIconLod iconLod in _config.iconLods)
            {
                _tryAddTextureToAtlas(iconLod.minorCityIcon);
                _tryAddTextureToAtlas(iconLod.majorCityIcon);
            }

            _textBaker = new TextBaker(_config.textConfig);
            
            _textBaker.Bake("Maggyletean - Диспот");
            
            return UniTask.CompletedTask;
        }

        private void _tryAddTextureToAtlas(Texture2D texture)
        {
            if (texture == null)
            {
                return;
            }

            if (_textureID.TryGetValue(texture, out ushort id))
            {
                return;
            }

            _textureID[texture] = _spriteAtlas.CreateSprite(new SpriteSetup
            {
                sourceTexture = texture
            });
        }

        protected override UniTask _gameLoad(ConcurrentContext context)
        {
            TerrainData terrain = _worldManager.GetCurrentWorld().terrain;

            if (terrain.IsSame(_bakedForTerrain))
            {
                return UniTask.CompletedTask;
            }

            if (_icons.IsCreated)
            {
                _entityManager.DestroyEntity(_icons);
                _icons.Dispose();
            }

            NativeList<CityInfo> cities = _citySpawner.Cities;

            _icons = _entityManager.CreateEntity(_iconArchetype, cities.Length, Allocator.Persistent);

            for (int i = 0; i < cities.Length; i++)
            {
                CityInfo city = cities[i];
                Entity entity = _icons[i];
                float2 position = city.position + new float2(0.5f, 0.5f);
                
                _entityManager.SetComponentData(entity, new PositionComponent(position));
                _entityManager.SetComponentData(entity, new SpriteComponent(_spriteAtlas.EmptySpriteID));
                _entityManager.SetComponentData(entity, new CityComponent((ushort)i));
            }
            
            return UniTask.CompletedTask;
        }
        
        protected override void _tick(BlockingContext context)
        {
            NativeArray<ArchetypeChunk> chunks = _iconQuery.ToArchetypeChunkArray(Allocator.TempJob);

            CityIconSwitchJob iconSwitchJob = new()
            {
                inChunks = chunks,
                positionHandle = _entityManager.GetComponentTypeHandle<PositionComponent>(true),
                cityHandle = _entityManager.GetComponentTypeHandle<CityComponent>(true),
                spriteHandle = _entityManager.GetComponentTypeHandle<SpriteComponent>(false),
                inIconSwitchLod = _resolveIconLod(_camera.ViewDistance),
                inCities = _citySpawner.Cities,
                inCameraViewBounds = _camera.GetViewBounds()
            };

            iconSwitchJob.Schedule(chunks.Length, 2).Complete();

            chunks.Dispose();
        }

        private CityIconSwitchJobLod _resolveIconLod(float viewDistance)
        {
            foreach (CityIconLod iconLod in _config.iconLods)
            {
                if (iconLod.viewDistance.Contains(viewDistance))
                {
                    return new CityIconSwitchJobLod
                    {
                        majorCityIconID = _getTextureID(iconLod.majorCityIcon),
                        minorCityIconID = _getTextureID(iconLod.minorCityIcon)
                    };
                }
            }

            return default;
        }

        private ushort _getTextureID(Texture2D texture)
        {
            if (texture == null || !_textureID.TryGetValue(texture, out ushort id))
            {
                return 0;
            }

            return id;
        }

        protected override void _dispose(BlockingContext obj)
        {
            if (_icons.IsCreated)
            {
                _icons.Dispose();
            }
            
            _iconQuery.Dispose();
            _textBaker.Dispose();
        }
    }
}