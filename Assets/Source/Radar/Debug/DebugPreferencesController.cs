using System;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Claw.Serialization;
using Beast.Radar.GameCycle;
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
            handle.ListenOperation(this, OperationID.PREFERENCES_LOAD, PreferencesLoad);
            handle.ListenOperation(this, OperationID.PREFERENCES_SAVE, PreferencesSave);
        }

        private async UniTask<OperationResult> PreferencesLoad()
        {
            bool fileExists = await _fileManager.Exists(FOLDER_NAME, FILE_NAME);
            if (!fileExists)
            {
                return OperationResult.Success();
            }

            StreamData stream = await _fileManager.OpenToRead(FOLDER_NAME, FILE_NAME);

            try
            {
                BinaryReader reader = new (stream.stream);

                _preferences.Deserialize(reader);
            }
            catch (Exception e)
            {
                BDebug.LogException(this, e, $"Error during debug preferences load. ({stream.path})");
            }
            finally
            {
                await _fileManager.Close(stream);
            }

            // TODO : https://risenbeast.youtrack.cloud/issue/R-46/Unity-Task-Success
            //      : We should probably return fail.
            //      : But game cycle should be able to ignore that fail as it is not critical.
            return OperationResult.Success();
        }
        
        private async UniTask<OperationResult> PreferencesSave()
        {
            StreamData stream = await _fileManager.OpenToWrite(FOLDER_NAME, FILE_NAME);

            try
            {
                BinaryWriter writer = new (stream.stream);
                
                _preferences.Serialize(writer);
            }
            catch (Exception e)
            {
                BDebug.LogException(this, e, $"Error during debug preferences save. ({stream.path})");
            }
            finally
            {
                await _fileManager.Close(stream);
            }
            
            // TODO : https://risenbeast.youtrack.cloud/issue/R-46/Unity-Task-Success
            return OperationResult.Success();
        }
    }
}