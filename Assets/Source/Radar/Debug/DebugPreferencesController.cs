using System;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Claw.Serialization;
using Beast.Radar.Serialization;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.Debug
{
    public class DebugPreferencesController : Controller, IGameCycleListener
    {
        private readonly IFileManager _fileManager;
        private readonly DebugPreferences _preferences;

        public DebugPreferencesController(IFileManager fileManager, DebugPreferences preferences)
        {
            _fileManager = fileManager;
            _preferences = preferences;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.PREFERENCES_LOAD, _preferencesLoad);
            handle.ListenBlocking(ActionID.PREFERENCES_SAVE, _preferencesSave);
        }

        private UniTask _preferencesLoad(ConcurrentContext context)
        {
            bool fileExists = _fileManager.Exists(FolderName.PREFERENCES, FileName.DEBUG_PREFERENCES);
            if (!fileExists)
            {
                return UniTask.CompletedTask;
            }

            using StreamData stream = _fileManager.OpenToRead(FolderName.PREFERENCES, FileName.DEBUG_PREFERENCES);

            try
            {
                BinaryReader reader = new (stream.stream);

                _preferences.Deserialize(reader);
            }
            catch (Exception e)
            {
                BDebug.LogException(e);
            }

            return UniTask.CompletedTask;
        }
        
        private void _preferencesSave(BlockingContext context)
        {
            using StreamData stream = _fileManager.OpenToWrite(FolderName.PREFERENCES, FileName.DEBUG_PREFERENCES);

            try
            {
                BinaryWriter writer = new (stream.stream);
                
                _preferences.Serialize(writer);
                
                stream.stream.Flush();
            }
            catch (Exception e)
            {
                BDebug.LogException(e);
            }
        }
    }
}