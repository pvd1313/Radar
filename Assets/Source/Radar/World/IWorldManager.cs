using System;
using Beast.Claw.Serialization;

namespace Beast.Radar.World
{
    public interface IWorldManager
    {
        WorldData GetCurrentWorld();
        
        void Clean();
        
        void AddWorld(WorldData worldData);

        bool CacheExists(Guid worldGuid, string cacheName);
        
        IBinaryReader StartCacheReading(Guid worldGuid, string cacheId);
        
        IBinaryWriter StartCacheWriting(Guid worldGuid, string cacheId);
    }
}