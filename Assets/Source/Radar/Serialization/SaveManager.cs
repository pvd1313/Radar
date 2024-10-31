using System;
using System.IO;
using Beast.Claw;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Claw.Serialization;
using Cysharp.Threading.Tasks;

using BinaryReader = Beast.Claw.Serialization.BinaryReader;
using BinaryWriter = Beast.Claw.Serialization.BinaryWriter;

namespace Beast.Radar.Serialization
{
    public class SaveManager : Controller, ISaveManager
    {
        public const string DEFAULT_FILE = "default";
        public const string DEFAULT_FOLDER = "default";
        
        private readonly GameModel _gameModel;
        private readonly IGameCycle _gameCycle;
        private readonly IFileManager _fileManager;

        public SaveManager(
            GameModel gameModel, 
            IGameCycle gameCycle,
            IFileManager fileManager)
        {
            _gameModel = gameModel;
            _gameCycle = gameCycle;
            _fileManager = fileManager;
        }
        
        public bool Exists(string folderName, string fileName)
        {
            return _fileManager.Exists(folderName, fileName);
        }

        public async UniTask<ActionResult> LoadAsync(GameModel model)
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
                
                return await _gameCycle.ExecuteConcurrent(this, ActionID.GAME_LOAD);
            }
            catch (Exception e)
            {
                BDebug.LogException(e);
                
                return ActionResult.Failed();
            }
        }

        public async UniTask<ActionResult> LoadAsync(string folderName, string fileName)
        {
            try
            {
                await using StreamData streamData = _fileManager.OpenToRead(folderName, fileName);
                
                BinaryReader binaryReader = new (streamData.stream);

                binaryReader.Read(_gameModel);

                ActionResult result = await _gameCycle.ExecuteConcurrent(this, ActionID.GAME_LOAD);
                
                BDebug.LogInfo(this, $"Loaded game state from '{streamData.path}'");

                return result;
            }
            catch (Exception e)
            {
                BDebug.LogException(e);
                
                return ActionResult.Failed();
            }
        }

        public void Save(string folderName, string fileName)
        {
            try
            {
                try
                {
                    _gameCycle.ExecuteBlocking(this, ActionID.GAME_SAVE);
                }
                catch (Exception e)
                {
                    BDebug.LogException(e);
                }

                using StreamData stream = _fileManager.OpenToWrite(folderName, fileName);
                BinaryWriter writer = new (stream.stream);

                writer.Write(_gameModel);
                
                stream.stream.Flush();

                BDebug.LogInfo(this, $"Saved game state to '{stream.path}'");
            }
            catch (Exception e)
            {
                BDebug.LogException(e);
            }
        }
    }
}