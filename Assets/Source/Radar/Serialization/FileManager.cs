using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Radar.Serialization
{
    public class FileManager : IFileManager
    {
        public UniTask<bool> Exists(string folderName, string fileName)
        {
            string filePath = _getOrCreateFilePath(folderName, fileName);

            bool fileFound = File.Exists(filePath);
            
            return UniTask.FromResult(fileFound);
        }
        
        public UniTask<StreamData> OpenToRead(string folderName, string fileName)
        {
            string filePath = _getOrCreateFilePath(folderName, fileName);

            if (!File.Exists(filePath))
            {
                throw new InvalidOperationException($"No file found at path '{filePath}'");
            }
            
            return UniTask.FromResult(new StreamData
            {
                stream = new FileStream(filePath, FileMode.Open),
                path = filePath,
                isReading = true
            });
        }
        
        public UniTask<StreamData> OpenToWrite(string folderName, string fileName)
        {
            string filePath = _getOrCreateFilePath(folderName, fileName);
            
            return UniTask.FromResult(new StreamData
            {
                stream = new FileStream(filePath, FileMode.Create),
                path = filePath,
                isReading = false
            });
        }

        public UniTask Close(StreamData stream)
        {
            if (!stream.isReading)
            {
                stream.stream.Flush();
            }
            
            stream.stream.Close();
            stream.stream.Dispose();

            return UniTask.CompletedTask;
        }
        
        private static string _getOrCreateFilePath(string folderName, string fileName)
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