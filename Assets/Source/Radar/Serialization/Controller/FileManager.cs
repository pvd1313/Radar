using System;
using System.IO;
using Beast.Claw.Debug;
using UnityEngine;

namespace Beast.Radar.Serialization
{
    public class FileManager : IFileManager
    {
        public bool Exists(string folderName, string fileName)
        {
            string filePath = _getOrCreateFilePath(folderName, fileName);

            return _fileExists(filePath);
        }
        
        public StreamData OpenToRead(string folderName, string fileName)
        {
            BProfiler.BeginSample("FileManager.OpenToRead");
            
            try
            {
                string filePath = _getOrCreateFilePath(folderName, fileName);

                if (!_fileExists(filePath))
                {
                    throw new InvalidOperationException($"No file found at path '{filePath}'");
                }

                return new StreamData
                {
                    stream = new FileStream(filePath, FileMode.Open),
                    path = filePath,
                };
            }
            finally
            {
                BProfiler.EndSample();
            }
        }
        
        public StreamData OpenToWrite(string folderName, string fileName)
        {
            BProfiler.BeginSample("FileManager.OpenToWrite");
            
            try
            {
                string filePath = _getOrCreateFilePath(folderName, fileName);
            
                return new StreamData
                {
                    stream = new FileStream(filePath, FileMode.Create),
                    path = filePath,
                };
            }
            finally
            {
                BProfiler.EndSample();
            }
        }

        public void DeleteFolder(string folderName)
        {
            string folderPath = Path.Combine(Application.persistentDataPath, folderName);
            
            Directory.Delete(folderPath, true);
        }

        private static bool _fileExists(string path)
        {
            BProfiler.BeginSample("FileManager._fileExists");

            bool result = File.Exists(path);
            
            BProfiler.EndSample();

            return result;
        }

        private static string _getOrCreateFilePath(string folderName, string fileName)
        {
            try
            {
                BProfiler.BeginSample("FileManager._getOrCreateFilePath");
                
                string folderPath = Path.Combine(Application.persistentDataPath, folderName);

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                return Path.Combine(folderPath, fileName) + ".bin";
            }
            finally
            {
                BProfiler.EndSample();
            }
        }
    }
}