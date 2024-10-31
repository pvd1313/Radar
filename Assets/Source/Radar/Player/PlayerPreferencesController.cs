using System;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Claw.Serialization;
using Beast.Radar.Serialization;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.Player
{
    public class PlayerPreferencesController : IGameCycleListener
    {
        private const string FOLDER_NAME = "preferences";
        private const string FILE_NAME = "player";

        private readonly IFileManager _fileManager;
        private readonly PlayerPreferences _preferences;

        public PlayerPreferencesController(IFileManager fileManager, PlayerPreferences preferences)
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
            bool fileExists = _fileManager.Exists(FOLDER_NAME, FILE_NAME);
            if (!fileExists)
            {
                return UniTask.CompletedTask;
            }

            using StreamData stream = _fileManager.OpenToRead(FOLDER_NAME, FILE_NAME);

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
            using StreamData stream = _fileManager.OpenToWrite(FOLDER_NAME, FILE_NAME);

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