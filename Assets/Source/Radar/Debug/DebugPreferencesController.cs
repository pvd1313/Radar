using System;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Claw.Serialization;
using Beast.Radar.Serialization;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.Debug
{
    public class DebugPreferencesController : IGameCycleListener
    {
        private const string FOLDER_NAME = "preferences";
        private const string FILE_NAME = "debug";
        
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

        private UniTask<ActionResult> _preferencesLoad(ConcurrentContext context)
        {
            bool fileExists = _fileManager.Exists(FOLDER_NAME, FILE_NAME);
            if (!fileExists)
            {
                return ActionResult.SuccessSyncronous();
            }

            StreamData stream = _fileManager.OpenToRead(FOLDER_NAME, FILE_NAME);

            try
            {
                BinaryReader reader = new (stream.stream);

                _preferences.Deserialize(reader);
            }
            catch (Exception e)
            {
                BDebug.LogException(e);
            }
            finally
            {
                _fileManager.Close(stream);
            }

            // TODO : https://risenbeast.youtrack.cloud/issue/R-46/Unity-Task-Success
            //      : We should probably return fail.
            //      : But game cycle should be able to ignore that fail as it is not critical.
            return ActionResult.SuccessSyncronous();
        }
        
        private void _preferencesSave(BlockingContext context)
        {
            StreamData stream = _fileManager.OpenToWrite(FOLDER_NAME, FILE_NAME);

            try
            {
                BinaryWriter writer = new (stream.stream);
                
                _preferences.Serialize(writer);
            }
            catch (Exception e)
            {
                BDebug.LogException(e);
            }
            finally
            {
                _fileManager.Close(stream);
            }
        }
    }
}