using Beast.Claw.Attributes;
using Beast.Claw.ECS;
using Beast.Claw.GameLoop;
using Beast.Radar.Sprite;
using Beast.Radar.Terrain;
using Beast.Radar.World;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Beast.Radar.City
{
    public class CitySpriteManager : Actor
    {
        [Required] private readonly IEntityManager _entityManager;
        [Required] private readonly ICitySpawner _citySpawner;
        [Required] private readonly IWorldManager _worldManager;
        [Required] private readonly ISpriteAtlas _spriteAtlas;
        [Required] private readonly CitySpriteConfig _config;
        
        private TerrainData _bakedForTerrain;
        private EntityArchetype _iconArchetype;
        private ushort _citySpriteID;
        private NativeArray<Entity> _icons;
        
        protected override UniTask _warmup(ConcurrentContext context)
        {
            _iconArchetype = _entityManager.CreateArchetype(
                typeof(PositionComponent),
                typeof(SpriteComponent));

            _citySpriteID = _spriteAtlas.CreateSprite(new SpriteSetup
            {
                displayViewDistance = _config.sprites[0].displayViewDistance,
                sourceTexture = _config.sprites[0].icon
            });
            
            return UniTask.CompletedTask;
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
                Entity icon = _icons[i];
                float2 position = cities[i].position + new float2(0.5f, 0.5f);
                
                _entityManager.SetComponentData(icon, new PositionComponent(position));
                _entityManager.SetComponentData(icon, new SpriteComponent(_citySpriteID));
            }
            
            return UniTask.CompletedTask;
        }

        protected override void _dispose(BlockingContext obj)
        {
            if (_icons.IsCreated)
            {
                _icons.Dispose();
            }
        }
    }
}