namespace Beast.Radar.World
{
    public interface IWorldManager
    {
        WorldData GetCurrentWorld();
        
        void Clean();
        
        void AddWorld(WorldData worldData);
    }
}