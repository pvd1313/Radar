using System;
using System.Collections.Generic;
using System.Linq;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Serialization;
using Beast.Radar.Serialization;
using Cysharp.Threading.Tasks;
using Profiler = UnityEngine.Profiling.Profiler;

namespace Beast.Radar.World
{
    public class WorldManager : Controller, IWorldManager, IGameCycleListener
    {
        private const string ROOT_FILE_NAME = "all";
        
        private readonly IFileManager _fileManager;
        private readonly GameModel _game;

        private Dictionary<Guid, WorldData> _worlds;
        private bool _isDirty;

        public WorldManager(IFileManager fileManager, GameModel game)
        {
            _fileManager = fileManager;
            _game = game;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
            handle.ListenBlocking(ActionID.GAME_SAVE, _gameSave);
        }

        public void Clean()
        {
            if (_worlds.Count > 0)
            {
                _worlds.Clear();
                _isDirty = true;
            }
        }

        public void AddWorld(WorldData worldData)
        {
            _worlds.TryAdd(worldData.guid, worldData);
            _isDirty = true;
        }
        
        public WorldData GetCurrentWorld()
        {
            if (!_worlds.TryGetValue(_game.world, out WorldData world))
            {
                throw new InvalidOperationException($"World '{world.guid}' was not found.");
            }

            return world;
        }

        private UniTask _warmup(ConcurrentContext arg)
        {
            _worlds = new Dictionary<Guid, WorldData>();

            if (_fileManager.Exists(FolderName.WORLD, ROOT_FILE_NAME))
            {
                using StreamData stream = _fileManager.OpenToRead(FolderName.WORLD, ROOT_FILE_NAME);
                BinaryReader reader = new (stream.stream);

                WorldData[] worldArray = reader.ReadArray<WorldData>();

                foreach (WorldData world in worldArray)
                {
                    _worlds[world.guid] = world;
                }
            }
            
            return UniTask.CompletedTask;
        }
        
        private void _gameSave(BlockingContext context)
        {
            if (!_isDirty)
            {
                return;
            }

            _isDirty = false;

            Profiler.BeginSample("_worlds.Values.ToArray();");
            WorldData[] array = _worlds.Values.ToArray();
            Profiler.EndSample();
            
            using StreamData stream = _fileManager.OpenToWrite(FolderName.WORLD, ROOT_FILE_NAME);
            BinaryWriter writer = new (stream.stream);
            
            Profiler.BeginSample("writer.WriteArray(array);");
            writer.WriteArray(array);
            Profiler.EndSample();
        }
    }
}