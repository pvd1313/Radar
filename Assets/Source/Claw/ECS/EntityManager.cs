using Beast.Claw.GameLoop;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;

namespace Beast.Claw.ECS
{
    public class EntityManager : Actor, IEntityManager
    {
        private World _world;
        
        protected override UniTask _warmup(ConcurrentContext context)
        {
            _world = new World("Claw", WorldFlags.None);
            
            return UniTask.CompletedTask;
        }

        public void AddComponent<T>(NativeArray<Entity> entities) where T : unmanaged, IComponentData
        {
            _world.EntityManager.AddComponent<T>(entities);
        }

        public void AddComponent<T>(EntityQuery query) where T : unmanaged, IComponentData
        {
            _world.EntityManager.AddComponent<T>(query);
        }
        
        public void DestroyEntity(NativeSlice<Entity> entities)
        {
            _world.EntityManager.DestroyEntity(entities);
        }

        public void DestroyEntity(NativeArray<Entity> entities)
        {
            _world.EntityManager.DestroyEntity(entities);
        }

        public void DestroyEntity(EntityQuery query)
        {
            _world.EntityManager.DestroyEntity(query);
        }

        public void DestroyEntity(Entity entity)
        {
            _world.EntityManager.DestroyEntity(entity);
        }

        public NativeArray<Entity> GetAllEntities(Allocator allocator)
        {
            return _world.EntityManager.GetAllEntities(allocator);
        }

        public EntityQuery CreateEntityQuery(params ComponentType[] requiredComponents)
        {
            return _world.EntityManager.CreateEntityQuery(requiredComponents);
        }

        public EntityQuery CreateEntityQuery(params EntityQueryDesc[] queryDesc)
        {
            return _world.EntityManager.CreateEntityQuery(queryDesc);
        }

        public ComponentTypeHandle<T> GetComponentTypeHandle<T>(bool isReadOnly)
        {
            return _world.EntityManager.GetComponentTypeHandle<T>(isReadOnly);
        }

        public EntityTypeHandle GetEntityTypeHandle()
        {
            return _world.EntityManager.GetEntityTypeHandle();
        }

        public EntityArchetype CreateArchetype(params ComponentType[] types)
        {
            return _world.EntityManager.CreateArchetype(types);
        }

        public NativeArray<Entity> CreateEntity(EntityArchetype archetype, int entityCount, Allocator allocator)
        {
            return _world.EntityManager.CreateEntity(archetype, entityCount, allocator);
        }

        public Entity CreateEntity(params ComponentType[] types)
        {
            return _world.EntityManager.CreateEntity(types);
        }

        public Entity CreateEntity(EntityArchetype archetype)
        {
            return _world.EntityManager.CreateEntity(archetype);
        }

        public void SetComponentData<T>(Entity entity, T componentData) where T : unmanaged, IComponentData
        {
            _world.EntityManager.SetComponentData(entity, componentData);
        }

        public bool CheckExists(Entity entity)
        {
            return _world.EntityManager.Exists(entity);
        }

        public T GetComponentData<T>(Entity entity) where T : unmanaged, IComponentData
        {
            return _world.EntityManager.GetComponentData<T>(entity);
        }

        protected override void _dispose(BlockingContext obj)
        {
            _world.EntityManager.DestroyAndResetAllEntities();
        }
    }
}