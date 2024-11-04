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
        
        IBinaryReader OpenCacheForReading(Guid worldGuid, string cacheId);
        
        IBinaryWriter OpenCacheForWriting(Guid worldGuid, string cacheId);
    }
}