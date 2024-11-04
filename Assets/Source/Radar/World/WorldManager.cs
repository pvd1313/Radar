using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Serialization;
using Beast.Radar.Serialization;
using Cysharp.Threading.Tasks;
using BinaryReader = Beast.Claw.Serialization.BinaryReader;
using BinaryWriter = Beast.Claw.Serialization.BinaryWriter;
using Profiler = UnityEngine.Profiling.Profiler;

namespace Beast.Radar.World
{
    public class WorldManager : Controller, IWorldManager, IGameCycleListener
    {
        private const string ROOT_FILE_NAME = "all";
        private const string POSTFIX = ".";
        
        private readonly IFileManager _fileManager;
        private readonly GameModel _game;

        private Dictionary<Guid, WorldData> _worlds;
        private Dictionary<Guid, string> _guidToStringDictionary;
        private Dictionary<(Guid, string), string> _cacheIdToFileNameDictionary;
        private StringBuilder _stringBuilder;
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
            
            _fileManager.DeleteFolder(FolderName.WORLD);
        }

        public void AddWorld(WorldData worldData)
        {
            _worlds.TryAdd(worldData.guid, worldData);
            _isDirty = true;
        }

        public bool CacheExists(Guid worldGuid, string cacheName)
        {
            string fileName = _cacheIdToFileName(worldGuid, cacheName);

            return _fileManager.Exists(FolderName.WORLD, fileName);
        }

        public IBinaryReader OpenCacheForReading(Guid worldGuid, string cacheId)
        {
            string fileName = _cacheIdToFileName(worldGuid, cacheId);

            StreamData stream = _fileManager.OpenToRead(FolderName.WORLD, fileName);

            return new BinaryReader(stream.stream);
        }

        public IBinaryWriter OpenCacheForWriting(Guid worldGuid, string cacheId)
        {
            string fileName = _cacheIdToFileName(worldGuid, cacheId);

            StreamData stream = _fileManager.OpenToWrite(FolderName.WORLD, fileName);

            return new BinaryWriter(stream.stream);
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
            _guidToStringDictionary = new Dictionary<Guid, string>();
            _cacheIdToFileNameDictionary = new Dictionary<(Guid, string), string>();
            _stringBuilder = new StringBuilder();

            if (_fileManager.Exists(FolderName.WORLD, ROOT_FILE_NAME))
            {
                using StreamData stream = _fileManager.OpenToRead(FolderName.WORLD, ROOT_FILE_NAME);
                BinaryReader reader = new (stream.stream);

                WorldData[] worldArray = reader.ReadArrayAndLength<WorldData>();

                foreach (WorldData world in worldArray)
                {
                    _worlds[world.guid] = world;
                }
            }
            
            return UniTask.CompletedTask;
        }

        private string _cacheIdToFileName(Guid worldGuid, string cacheName)
        {
            if (_cacheIdToFileNameDictionary.TryGetValue((worldGuid, cacheName), out string name))
            {
                return name;
            }
            
            string guidText = _guidToString(worldGuid);

            _stringBuilder.Clear();
            _stringBuilder.Append(guidText);
            _stringBuilder.Append(POSTFIX);
            _stringBuilder.Append(cacheName);

            name = _stringBuilder.ToString();

            _cacheIdToFileNameDictionary[(worldGuid, cacheName)] = name;

            return name;
        }
        
        private string _guidToString(Guid guid)
        {
            if (_guidToStringDictionary.TryGetValue(guid, out string text))
            {
                return text;
            }

            text = guid.ToString();

            _guidToStringDictionary[guid] = text;

            return text;
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
            writer.WriteArrayAndLength(array);
            Profiler.EndSample();
        }
    }
}