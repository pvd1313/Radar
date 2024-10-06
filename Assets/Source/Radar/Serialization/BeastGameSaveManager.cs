using System;
using System.IO;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Serialization;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Radar.Serialization
{
    public class BeastGameSaveManager : IBeastGameSaveManager
    {
        private readonly BeastGameModel _gameModel;
        private readonly IBeastGameCycle _gameCycle;
        private readonly BeastGameSaveConfig _config;

        public BeastGameSaveManager(
            BeastGameModel gameModel, 
            IBeastGameCycle gameCycle,
            BeastGameSaveConfig config)
        {
            _gameModel = gameModel;
            _gameCycle = gameCycle;
            _config = config;
        }

        public async UniTask<BeastInitializationResult> LoadDefaultSave()
        {
            try
            {
                _gameModel.CopyFrom(_config.defaultSave);

                return await _gameCycle.Execute(this, BeastCycleEventID.GameLoading);
            }
            catch (Exception e)
            {
                BDebug.LogException(this, e, "Failed to load default game save");
                
                return BeastInitializationResult.Failed();
            }
        }

        public async UniTask<BeastInitializationResult> Load(string folderName, string fileName)
        {
            try
            {
                string filePath = InternalGetOrCreateFilePath(folderName, fileName);
                
                using FileStream fileStream = new (filePath, FileMode.Open);
                BeastBinaryReader binaryReader = new (fileStream);

                binaryReader.Read(_gameModel);
                
                fileStream.Close();
                
                BeastInitializationResult result = await _gameCycle.Execute(this, BeastCycleEventID.GameLoading);
                
                BDebug.LogInfo(this, $"Loaded game state from '{filePath}'");

                return result;
            }
            catch (Exception e)
            {
                BDebug.LogException(this, e, $"Failed to load game state '{folderName}/{fileName}'");
                
                return BeastInitializationResult.Failed();
            }
        }

        public async UniTask<BeastInitializationResult> Save(string folderName, string fileName)
        {
            try
            {
                BeastInitializationResult result = await _gameCycle.Execute(this, BeastCycleEventID.GameSaving);

                if (result.status != EBeastInitializationStatus.Success)
                {
                    return result;
                }
                
                string filePath = InternalGetOrCreateFilePath(folderName, fileName);
                
                using FileStream fileStream = new (filePath, FileMode.Create);
                BeastBinaryWriter binaryWriter = new BeastBinaryWriter(fileStream);

                binaryWriter.Write(_gameModel);

                fileStream.Flush();
                fileStream.Close();
                
                BDebug.LogInfo(this, $"Saved game state to '{filePath}'");

                return result;
            }
            catch (Exception e)
            {
                BDebug.LogException(this, e, $"Failed to save game state '{folderName}/{fileName}'");
                
                return BeastInitializationResult.Failed();
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