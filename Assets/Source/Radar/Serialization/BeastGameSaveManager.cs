using System;
using System.Collections.Generic;
using System.IO;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Radar.Serialization
{
    public class BeastGameSaveManager : IBeastInitialize, IBeastGameSaveManager
    {
        private readonly List<IBeastSerializableModule> _modules;

        public BeastGameSaveManager(List<IBeastSerializableModule> modules)
        {
            _modules = modules;
        }

        public UniTask<BeastInitializationResult> Initialize()
        {
            return BeastInitializationResult.SuccessNotAsync();
        }
        
        public void Load(string folderName, string fileName)
        {
            try
            {
                string filePath = InternalGetOrCreateFilePath(folderName, fileName);
                using FileStream fileStream = new (filePath, FileMode.Open);
                
                // TODO : Can not seek positions in GZipStream.
                // GZipStream zipStream = new (fileStream, CompressionLevel.Fastest);
                
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
                
                BLog.Info(this, $"Loaded game state from '{filePath}'");
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
                string filePath = InternalGetOrCreateFilePath(folderName, fileName);
                using FileStream fileStream = new (filePath, FileMode.Create);
                
                // TODO : Can not seek positions in GZipStream.
                // GZipStream zipStream = new (fileStream, CompressionLevel.Fastest);
                
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
                
                BLog.Info(this, $"Saved game state to '{filePath}'");
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