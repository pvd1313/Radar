using System;
using Beast.Claw.Attributes;
using Beast.Claw.Debug;
using Beast.Claw.GameLoop;
using Beast.Claw.Serialization;
using Beast.Radar.Serialization;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.Player
{
    public class PlayerPreferenceSerializer : Actor
    {
        [Required] private readonly IFileManager _fileManager;
        [Required] private readonly PlayerPreference _preference;

        protected override UniTask _preferencesLoad(ConcurrentContext context)
        {
            bool fileExists = _fileManager.Exists(FolderName.PREFERENCES, FileName.PLAYER_PREFERENCES);
            if (!fileExists)
            {
                return UniTask.CompletedTask;
            }

            using StreamData stream = _fileManager.OpenToRead(FolderName.PREFERENCES, FileName.PLAYER_PREFERENCES);

            try
            {
                BinaryReader reader = new (stream.stream);

                _preference.Deserialize(reader);
            }
            catch (Exception e)
            {
                BDebug.LogException(e);
            }

            return UniTask.CompletedTask;
        }

        protected override void _preferencesSave(BlockingContext context)
        {
            using StreamData stream = _fileManager.OpenToWrite(FolderName.PREFERENCES, FileName.PLAYER_PREFERENCES);

            try
            {
                BinaryWriter writer = new (stream.stream);
                
                _preference.Serialize(writer);
                
                stream.stream.Flush();
            }
            catch (Exception e)
            {
                BDebug.LogException(e);
            }
        }
        
    }
}