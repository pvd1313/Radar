using System;
using System.IO;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Claw.Serialization;
using Beast.Radar.GameCycle;
using Cysharp.Threading.Tasks;
using UnityEngine;

using BinaryReader = Beast.Claw.Serialization.BinaryReader;
using BinaryWriter = Beast.Claw.Serialization.BinaryWriter;

namespace Beast.Radar.Serialization
{
    public class GameSaveManager : IGameSaveManager
    {
        private readonly GameModel _gameModel;
        private readonly IGameCycle _gameCycle;
        private readonly GameSaveConfig _config;

        public GameSaveManager(
            GameModel gameModel, 
            IGameCycle gameCycle,
            GameSaveConfig config)
        {
            _gameModel = gameModel;
            _gameCycle = gameCycle;
            _config = config;
        }

        public async UniTask<OperationResult> LoadDefaultSave()
        {
            try
            {
                _gameModel.CopyFrom(_config.defaultSave);

                return await _gameCycle.Execute(this, OperationID.GameLoading);
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
                string filePath = InternalGetOrCreateFilePath(folderName, fileName);
                
                using FileStream fileStream = new (filePath, FileMode.Open);
                BinaryReader binaryReader = new (fileStream);

                binaryReader.Read(_gameModel);
                
                fileStream.Close();
                
                OperationResult result = await _gameCycle.Execute(this, OperationID.GameLoading);
                
                BDebug.LogInfo(this, $"Loaded game state from '{filePath}'");

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
                OperationResult result = await _gameCycle.Execute(this, OperationID.GameSaving);

                if (result.status != EOperationStatus.Success)
                {
                    return result;
                }
                
                string filePath = InternalGetOrCreateFilePath(folderName, fileName);
                
                using FileStream fileStream = new (filePath, FileMode.Create);
                BinaryWriter binaryWriter = new BinaryWriter(fileStream);

                binaryWriter.Write(_gameModel);

                fileStream.Flush();
                fileStream.Close();
                
                BDebug.LogInfo(this, $"Saved game state to '{filePath}'");

                return result;
            }
            catch (Exception e)
            {
                BDebug.LogException(this, e, $"Failed to save game state '{folderName}/{fileName}'");
                
                return OperationResult.Failed();
            }
        }

        private string InternalGetOrCreateFilePath(string folderName, string fileName)
        {
            string folderPath = Path.Combine(Application.persistentDataPath, folderName);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            return Path.Combine(folderPath, fileName) + ".bin";
        }
    }
}