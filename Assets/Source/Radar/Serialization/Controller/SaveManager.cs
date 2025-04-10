using System;
using System.IO;
using Beast.Claw.Attributes;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameLoop;
using Beast.Claw.ProgressTracker;
using Beast.Claw.Serialization;
using Beast.Claw.Validation;
using Beast.Radar.UI;
using Cysharp.Threading.Tasks;

using BinaryReader = Beast.Claw.Serialization.BinaryReader;
using BinaryWriter = Beast.Claw.Serialization.BinaryWriter;

namespace Beast.Radar.Serialization
{
    public class SaveManager : Controller, ISaveManager
    {
        [Required] private readonly GameModel _gameModel;
        [Required] private readonly IGameLoop _gameLoop;
        [Required] private readonly IFileManager _fileManager;
        [Required] private readonly IWindowManager _windowManager;

        public bool Exists(string folderName, string fileName)
        {
            return _fileManager.Exists(folderName, fileName);
        }

        public async UniTask<EActionStatus> LoadAsync(GameModel model, ESaveManagerFlags flags)
        {
            model.ValidateNotNullThrowException(nameof(model));
            
            try
            {
                IProgressTracker progressTracker = _displayOperationWindow(flags);
                
                MemoryStream memory = new();
                
                BinaryWriter writer = new (memory);
                model.Serialize(writer);

                memory.Position = 0;
                BinaryReader reader = new (memory);
                _gameModel.Deserialize(reader);

                await memory.DisposeAsync();
                
                EActionStatus status = await _gameLoop.ExecuteConcurrent(this, ActionID.GAME_LOAD, progressTracker);

                await _finishOperation(progressTracker, status, flags);
                
                return status;
            }
            catch (Exception e)
            {
                BDebug.LogException(e);
                
                return EActionStatus.Failed;
            }
        }

        public async UniTask<EActionStatus> LoadAsync(string folderName, string fileName, ESaveManagerFlags flags)
        {
            try
            {
                IProgressTracker progressTracker = _displayOperationWindow(flags);
                
                await using StreamData streamData = _fileManager.OpenToRead(folderName, fileName);
                
                BinaryReader binaryReader = new (streamData.stream);

                binaryReader.ReadClass(_gameModel);

                EActionStatus status = await _gameLoop.ExecuteConcurrent(this, ActionID.GAME_LOAD, progressTracker);
                
                BDebug.LogInfo(this, $"Loaded game state from '{streamData.path}'");

                await _finishOperation(progressTracker, status, flags);

                return status;
            }
            catch (Exception e)
            {
                BDebug.LogException(e);
                
                return EActionStatus.Failed;
            }
        }

        private async UniTask _finishOperation(
            IProgressTracker progressTracker, 
            EActionStatus status, 
            ESaveManagerFlags flags)
        {
            if ((flags & ESaveManagerFlags.Silent) != 0)
            {
                return;
            }
            
            if (status == EActionStatus.Success)
            {
                if ((flags & ESaveManagerFlags.AutoSkipWindow) != 0)
                {
                    await _windowManager.Open.HUD();
                }
                else
                {
                    await progressTracker.Next("Waiting for player input");
                }
                
                return;
            }

            // TODO : https://risenbeast.youtrack.cloud/issue/R-125/Error-Info-popup-window-for-saving-loading
            
            await _windowManager.Open.MainMenu();
        }

        private IProgressTracker _displayOperationWindow(ESaveManagerFlags flags)
        {
            if ((flags & ESaveManagerFlags.Silent) != 0)
            {
                return null;
            }
            
            ProgressTracker tracker = new();

            _windowManager.Open.LoadingScreen(new LoadingWindowInput
            {
                nextAction = () => _windowManager.Open.HUD(),
                clearLastProgress = true,
                currentProgress = 0,
                progressTracker = tracker
            });

            return tracker;
        }

        public EActionStatus Save(string folderName, string fileName)
        {
            try
            {
                try
                {
                    _gameLoop.ExecuteBlocking(this, ActionID.GAME_SAVE);
                }
                catch (Exception e)
                {
                    BDebug.LogException(e);
                    
                    // TODO : https://risenbeast.youtrack.cloud/issue/R-125/Error-Info-popup-window-for-saving-loading
                    
                    return EActionStatus.Failed;
                }

                using StreamData stream = _fileManager.OpenToWrite(folderName, fileName);
                BinaryWriter writer = new (stream.stream);

                writer.Write(_gameModel);
                
                stream.stream.Flush();

                BDebug.LogInfo(this, $"Saved game state to '{stream.path}'");

                return EActionStatus.Success;
            }
            catch (Exception e)
            {
                BDebug.LogException(e);
                
                // TODO : https://risenbeast.youtrack.cloud/issue/R-125/Error-Info-popup-window-for-saving-loading

                return EActionStatus.Failed;
            }
        }
    }
}