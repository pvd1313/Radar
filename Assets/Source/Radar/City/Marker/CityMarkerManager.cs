using System.Collections.Generic;
using Beast.Claw.Attributes;
using Beast.Claw.Common;
using Beast.Claw.ECS;
using Beast.Claw.GameLoop;
using Beast.Claw.Graphic;
using Beast.Claw.Jobs;
using Beast.Claw.Text.Baking;
using Beast.Radar.Sprite;
using Beast.Radar.World;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using TerrainData = Beast.Radar.Terrain.TerrainData;

namespace Beast.Radar.City
{
    public class CityMarkerManager : Actor
    {
        [Required] private readonly IEntityManager _entityManager;
        [Required] private readonly ICitySpawner _citySpawner;
        [Required] private readonly ICityNameGenerator _cityName;
        [Required] private readonly IWorldManager _worldManager;
        [Required] private readonly ISpriteAtlas _spriteAtlas;
        [Required] private readonly CityMarkerConfig _config;
        [Required] private readonly ICameraTransform _camera;

        private TerrainData _bakedForTerrain;
        private EntityArchetype _iconArchetype;
        private EntityArchetype _labelArchetype;
        private EntityQuery _markerQuery;
        private NativeArray<Entity> _icons;
        private NativeArray<Entity> _labels;
        private TextBaker _textBaker;
        private Dictionary<Texture2D, ushort> _textureID;

        protected override UniTask _warmup(ConcurrentContext context)
        {
            ComponentType[] iconComponents =
            {
                typeof(PositionComponent),
                typeof(SpriteComponent),
                typeof(CityIDComponent)
            };

            ComponentType[] labelComponents =
            {
                typeof(PositionComponent),
                typeof(SpriteComponent),
                typeof(CityIDComponent),
                typeof(CityLabelComponent),
                typeof(SpriteOffsetComponent)
            };

            _iconArchetype = _entityManager.CreateArchetype(iconComponents);
            _labelArchetype = _entityManager.CreateArchetype(labelComponents);
            _markerQuery = _entityManager.CreateEntityQuery(iconComponents);

            _textureID = new Dictionary<Texture2D, ushort>();
            foreach (CityMarkerLod iconLod in _config.iconLods)
            {
                _tryAddTextureToAtlas(iconLod.minorIcon);
                _tryAddTextureToAtlas(iconLod.majorIcon);
            }

            _textBaker = new TextBaker(_config.textConfig);

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
                sourceTexture = texture,
                textureSize = texture.Size(),
                textureOffset = 0
            });
        }

        protected override UniTask _gameLoad(ConcurrentContext context)
        {
            TerrainData terrain = _worldManager.GetCurrentWorld().terrain;

            if (terrain.IsSame(_bakedForTerrain))
            {
                return UniTask.CompletedTask;
            }

            _disposeEntities();

            NativeSlice<CityInfo> cities = _citySpawner.Cities;

            _icons = _entityManager.CreateEntity(_iconArchetype, cities.Length, Allocator.Persistent);
            _labels = _entityManager.CreateEntity(_labelArchetype, cities.Length, Allocator.Persistent);

            for (int i = 0; i < cities.Length; i++)
            {
                CityInfo city = cities[i];
                float2 position = city.position + new float2(0.5f, 0.5f);
                
                Entity icon = _icons[i];
                _entityManager.SetComponentData(icon, new PositionComponent(position));
                _entityManager.SetComponentData(icon, new CityIDComponent((ushort)i));

                Entity label = _labels[i];
                _entityManager.SetComponentData(label, new PositionComponent(position));
                _entityManager.SetComponentData(label, new CityIDComponent((ushort)i));
            }

            return UniTask.CompletedTask;
        }

        protected override void _tick(BlockingContext context)
        {
            NativeArray<ArchetypeChunk> chunks = _markerQuery.ToArchetypeChunkArray(Allocator.TempJob);

            CityMarkerJob markerJob = new()
            {
                inChunks = chunks,
                positionHandle = _entityManager.GetComponentTypeHandle<PositionComponent>(true),
                cityIDHandle = _entityManager.GetComponentTypeHandle<CityIDComponent>(true),
                spriteHandle = _entityManager.GetComponentTypeHandle<SpriteComponent>(false),
                spriteOffsetHandle = _entityManager.GetComponentTypeHandle<SpriteOffsetComponent>(false),
                entityHandle = _entityManager.GetEntityTypeHandle(),
                inMarkerLod = _resolveLod(_camera.ViewDistance),
                inCities = _citySpawner.Cities,
                inCameraViewBounds = _camera.GetViewBounds(),
                inSprites = _spriteAtlas.Sprites,
                outLabelCommands =
                    BNative.TempJobArray<CityLabelCommand>(chunks.Length * CityMarkerJob.MAX_COMMAND_COUNT),
                outLabelCommandCounters = BNative.TempJobArray<byte>(chunks.Length)
            };

            markerJob.ExecuteOn(chunks);

            for (int i = 0; i < markerJob.outLabelCommandCounters.Length; i++)
            {
                int cmdCount = markerJob.outLabelCommandCounters[i];
                int cmdOffset = i * CityMarkerJob.MAX_COMMAND_COUNT;

                for (int j = 0; j < cmdCount; j++)
                {
                    CityLabelCommand command = markerJob.outLabelCommands[cmdOffset + j];

                    _applyLabelCommand(command);
                }
            }

            chunks.Dispose();
            markerJob.outLabelCommands.Dispose();
            markerJob.outLabelCommandCounters.Dispose();
        }

        private void _applyLabelCommand(CityLabelCommand command)
        {
            switch (command.commandType)
            {
                case ECityLabelCommandType.CreateLabel:
                    Profiler.BeginSample("CreateLabel()");
                    _setupLabel(command);
                    Profiler.EndSample();
                    break;

                case ECityLabelCommandType.ReleaseLabel:
                    Profiler.BeginSample("ReleaseLabel()");
                    _spriteAtlas.ReleaseSprite(command.spriteID);
                    _entityManager.SetComponentData(command.targetEntity, new SpriteComponent());
                    Profiler.EndSample();
                    break;

                case ECityLabelCommandType.ChangeLabelResolution:
                    Profiler.BeginSample("ChangeLabelResolution()");
                    _spriteAtlas.ReleaseSprite(command.spriteID);
                    _setupLabel(command);
                    Profiler.EndSample();
                    break;
            }
        }

        private void _setupLabel(CityLabelCommand command)
        {
            string cityName = _cityName.GetCityName(command.cityID);
            TextBakeResult bakedText = _textBaker.Bake(cityName, command.labelSize);

            ushort spriteID = _spriteAtlas.CreateSprite(new SpriteSetup
            {
                sourceTexture = bakedText.texture,
                textureSize = bakedText.size,
                textureOffset = bakedText.offset
            });

            _entityManager.SetComponentData(command.targetEntity, new SpriteComponent(spriteID));
        }

        private CityMarkerLodNative _resolveLod(float viewDistance)
        {
            foreach (CityMarkerLod lod in _config.iconLods)
            {
                if (lod.viewDistance.Contains(viewDistance))
                {
                    return new CityMarkerLodNative
                    {
                        majorCityIconID = _getTextureID(lod.majorIcon),
                        minorCityIconID = _getTextureID(lod.minorIcon),
                        minorLabelSize = lod.minorLabelSize,
                        majorLabelSize = lod.majorLabelSize,
                        minorLabelOffset = lod.minorLabelOffset,
                        majorLabelOffset = lod.majorLabelOffset,
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
            _disposeEntities();

            _markerQuery.Dispose();
            _textBaker.Dispose();
        }

        private void _disposeEntities()
        {
            if (_icons.IsCreated)
            {
                _entityManager.DestroyEntity(_icons);
                _entityManager.DestroyEntity(_labels);

                _icons.Dispose();
                _labels.Dispose();
            }
        }
    }
}