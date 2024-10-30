using System;
using System.IO;
using UnityEngine;

namespace Beast.Radar.Serialization
{
    public class FileManager : IFileManager
    {
        public bool Exists(string folderName, string fileName)
        {
            string filePath = _getOrCreateFilePath(folderName, fileName);

            return File.Exists(filePath);
        }
        
        public StreamData OpenToRead(string folderName, string fileName)
        {
            string filePath = _getOrCreateFilePath(folderName, fileName);

            if (!File.Exists(filePath))
            {
                throw new InvalidOperationException($"No file found at path '{filePath}'");
            }
            
            return new StreamData
            {
                stream = new FileStream(filePath, FileMode.Open),
                path = filePath,
            };
        }
        
        public StreamData OpenToWrite(string folderName, string fileName)
        {
            string filePath = _getOrCreateFilePath(folderName, fileName);
            
            return new StreamData
            {
                stream = new FileStream(filePath, FileMode.Create),
                path = filePath,
            };
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