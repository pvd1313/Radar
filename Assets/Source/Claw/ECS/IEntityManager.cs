using Unity.Collections;
using Unity.Entities;

namespace Beast.Claw.ECS
{
    public interface IEntityManager
    {
        EntityQuery CreateEntityQuery(params ComponentType[] requiredComponents);
        ComponentTypeHandle<T> GetComponentTypeHandle<T>(bool isReadOnly);
        EntityTypeHandle GetEntityTypeHandle();
        EntityArchetype CreateArchetype(params ComponentType[] types);
        NativeArray<Entity> CreateEntity(EntityArchetype archetype, int entityCount, Allocator allocator);
        void SetComponentData<T>(Entity entity, T componentData) where T : unmanaged, IComponentData;
        T GetComponentData<T>(Entity entity) where T : unmanaged, IComponentData;
        Entity CreateEntity(params ComponentType[] types);
        void DestroyEntity(Entity entity);
        void DestroyEntity(NativeSlice<Entity> entities);
        void DestroyEntity(NativeArray<Entity> entities);
        NativeArray<Entity> GetAllEntities(Allocator allocator);
        Entity CreateEntity(EntityArchetype archetype);
        EntityQuery CreateEntityQuery(params EntityQueryDesc[] queryDesc);
        void DestroyEntity(EntityQuery query);
        bool CheckExists(Entity entity);
        void AddComponent<T>(NativeArray<Entity> entities) where T : unmanaged, IComponentData;
        void AddComponent<T>(EntityQuery query) where T : unmanaged, IComponentData;
    }
}