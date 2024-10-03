using System;
using System.IO;
using System.IO.Compression;
using Beast.Claw.Common;
using Newtonsoft.Json;

namespace Beast.Radar.GameSave
{
    public class FileDataHandler
    {
        public void Save<TData>(TData savedData, string dirPath, string fileName)
        {
            string fullPath = Path.Combine(dirPath, fileName + ".gz"); 
            try
            {
                Directory.CreateDirectory
                (
                    Path.GetDirectoryName(fullPath) 
                    ?? throw new BeastException("Incorrect save path")
                );

                string dataToStore = JsonConvert.SerializeObject(savedData, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

                using FileStream stream = new FileStream(fullPath, FileMode.Create);
                using GZipStream compressionStream = new GZipStream(stream, CompressionMode.Compress);
                using StreamWriter writer = new StreamWriter(compressionStream);

                writer.Write(dataToStore);
            }
            catch (Exception e)
            {
                throw new BeastException("Error occurred when trying to save data to file: " + fullPath + "\n" + e);
            }
        }

        public TData Load<TData>(string dirPath, string fileName)
        {
            string fullPath = Path.Combine(dirPath, fileName + ".gz");

            if (!File.Exists(fullPath))
            {
                return default;
            }
            try
            {
                using FileStream stream = new FileStream(fullPath, FileMode.Open);
                using GZipStream decompressionStream = new GZipStream(stream, CompressionMode.Decompress);
                using StreamReader reader = new StreamReader(decompressionStream);

                string dataToLoad = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<TData>(dataToLoad);
            }
            catch (Exception e)
            {
                throw new BeastException("Error occurred when trying to load data from file: " + fullPath + "\n" + e);
            }
        }
    }
}
