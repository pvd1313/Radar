using System;
using System.IO;
using Beast.Claw;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Claw.Serialization;
using Cysharp.Threading.Tasks;

using BinaryReader = Beast.Claw.Serialization.BinaryReader;
using BinaryWriter = Beast.Claw.Serialization.BinaryWriter;

namespace Beast.Radar.Serialization
{
    public class SaveManager : ISaveManager
    {
        public const string DEFAULT_FILE = "default";
        public const string DEFAULT_FOLDER = "default";
        
        private readonly GameModel _gameModel;
        private readonly IGameCycle _gameCycle;
        private readonly IntegratedSave _library;
        private readonly IFileManager _fileManager;

        public SaveManager(
            GameModel gameModel, 
            IGameCycle gameCycle,
            IntegratedSave library,
            IFileManager fileManager)
        {
            _gameModel = gameModel;
            _gameCycle = gameCycle;
            _library = library;
            _fileManager = fileManager;
        }
        
        public async UniTask<bool> Exists(string folderName, string fileName)
        {
            return await _fileManager.Exists(folderName, fileName);
        }

        public async UniTask<OperationResult> Load(GameModel model)
        {
            model.ValidateNotNullThrowException(nameof(model));
            
            try
            {
                MemoryStream memory = new ();
                
                BinaryWriter writer = new (memory);
                model.Serialize(writer);

                memory.Position = 0;
                BinaryReader reader = new (memory);
                _gameModel.Deserialize(reader);

                await memory.DisposeAsync();
                
                return await _gameCycle.Execute(this, OperationID.GAME_LOAD);
            }
            catch (Exception e)
            {
                BDebug.LogException(this, e, "Failed to load default game save");
                
                return OperationResult.Failed();
            }
        }

        public async UniTask<OperationResult> Load(string folderName, string fileName)
        {
            try
            {
                StreamData streamData = await _fileManager.OpenToRead(folderName, fileName);
                
                BinaryReader binaryReader = new (streamData.stream);

                binaryReader.Read(_gameModel);

                await _fileManager.Close(streamData);
                
                OperationResult result = await _gameCycle.Execute(this, OperationID.GAME_LOAD);
                
                BDebug.LogInfo(this, $"Loaded game state from '{streamData.path}'");

                return result;
            }
            catch (Exception e)
            {
                BDebug.LogException(this, e, $"Failed to load game state '{folderName}/{fileName}'");
                
                return OperationResult.Failed();
            }
        }

        public async UniTask<OperationResult> Save(string folderName, string fileName)
        {
            try
            {
                OperationResult result = await _gameCycle.Execute(this, OperationID.GAME_SAVE);
                if (result.status != EOperationStatus.Success)
                {
                    return result;
                }

                StreamData stream = await _fileManager.OpenToWrite(folderName, fileName);
                BinaryWriter writer = new (stream.stream);

                writer.Write(_gameModel);

                await _fileManager.Close(stream);
                
                BDebug.LogInfo(this, $"Saved game state to '{stream.path}'");

                return result;
            }
            catch (Exception e)
            {
                BDebug.LogException(this, e, $"Failed to save game state '{folderName}/{fileName}'");
                
                return OperationResult.Failed();
            }
        }
    }
}