using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Serialization;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Radar.Serialization
{
    public class BeastGameSaveManager : IBeastInitialize, IBeastGameSaveManager
    {
        private readonly List<IBeastSerializableModule> _modules;
        private readonly BeastGameSaveConfig _config;

        private BeastFileCompressor _fileCompressor;

        public BeastGameSaveManager(List<IBeastSerializableModule> modules, BeastGameSaveConfig config)
        {
            _modules = modules;
            _config = config;
        }

        public UniTask<BeastInitializationResult> Initialize()
        {
            _fileCompressor = new BeastFileCompressor();
            
            return BeastInitializationResult.SuccessNotAsync();
        }
        
        public void Load(string folderName, string fileName)
        {
            try
            {
                string realFilePath = InternalGetOrCreateFilePath(folderName, fileName);
                string tempFilePath = realFilePath + ".temp";

                if (_config.useCompression)
                {
                    _fileCompressor.Decompress(realFilePath, tempFilePath);
                }
                
                using FileStream fileStream = new (_config.useCompression ? tempFilePath : realFilePath, FileMode.Open);
                
                BeastBlobStreamReader blobStream = new (fileStream);
                
                blobStream.ReadHeaders();

                foreach (IBeastSerializableModule module in _modules)
                {
                    try
                    {
                        module.OnModuleDeserialize(blobStream);
                    }
                    catch (Exception e)
                    {
                        string message =
                            $"Error during game state loading '{folderName}/{fileName}'({module.GetType()})";
                        
                        BLog.Exception(this, e, message);
                    }
                }
                
                fileStream.Close();

                if (_config.useCompression)
                {
                    File.Delete(tempFilePath);
                }
                
                BLog.Info(this, $"Loaded game state from '{realFilePath}' compression: {_config.useCompression}");
            }
            catch (Exception e)
            {
                BLog.Exception(this, e, $"Failed to load game state '{folderName}/{fileName}'");
            }
        }

        public void Save(string folderName, string fileName)
        {
            try
            {
                string realFilePath = InternalGetOrCreateFilePath(folderName, fileName);
                string tempFilePath = realFilePath + ".open";
                
                using FileStream fileStream = new (_config.useCompression ? tempFilePath : realFilePath, FileMode.Create);
                BeastBlobStreamWriter blobStream = new (fileStream);
                
                foreach (IBeastSerializableModule module in _modules)
                {
                    try
                    {
                        module.OnModuleSerialize(blobStream);
                    }
                    catch (Exception e)
                    {
                        string message =
                            $"Error during game state saving '{folderName}/{fileName}'({module.GetType()})";
                        
                        BLog.Exception(this, e, message);
                    }
                }

                fileStream.Flush();
                fileStream.Close();

                if (_config.useCompression)
                {
                    _fileCompressor.Compress(tempFilePath, realFilePath);

                    File.Delete(tempFilePath);
                }
                
                BLog.Info(this, $"Saved game state to '{realFilePath}' compression: {_config.useCompression}");
            }
            catch (Exception e)
            {
                BLog.Exception(this, e, $"Failed to save game state '{folderName}/{fileName}'");
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