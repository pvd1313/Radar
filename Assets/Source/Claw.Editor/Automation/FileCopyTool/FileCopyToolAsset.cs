using System.IO;
using System.Linq;
using Beast.Claw.Attributes;
using Beast.Claw.Editor.IO;
using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;

namespace Beast.Claw.Editor.Automation.FileCopyTool
{
    public class FileCopyToolAsset : ToolAsset
    {
        [SerializeField] private FileCopyToolConfig _config;
        
        [Button]
        private void PreviewDeletedFiles()
        {
            string outputFolder = EditorPath.Combine(EditorPath.ProjectRoot, _config.OutputFolder.path);
            string[] files = Directory.GetFiles(outputFolder);
            foreach (string file in files)
            {
                if (_config.DeleteFilesPatterns.Any(p => p.IsMatch(file)))
                {
                    UnityEngine.Debug.Log(file);
                }
            }
        }

        [Button]
        private void PreviewExportedFiles()
        {
            foreach (Object asset in _config.AssetsToCopy)
            {
                GetAssetCopyOperationPath(asset, out string fromPath, out string toPath);
                
                UnityEngine.Debug.Log($"'{fromPath}' -> '{toPath}'");
            }
        }
        
        [Button]
        private void DeleteOldFilesAndExportNewAssets()
        {
            string outputFolder = EditorPath.Combine(EditorPath.ProjectRoot, _config.OutputFolder.path);
            
            string[] files = Directory.GetFiles(outputFolder);
            foreach (string file in files)
            {   
                if (_config.DeleteFilesPatterns.Any(p => p.IsMatch(file)))
                {
                    File.Delete(file);
                }
            }

            foreach (Object asset in _config.AssetsToCopy)
            {
                GetAssetCopyOperationPath(asset, out string fromPath, out string toPath);
                File.Copy(fromPath, toPath, false);
            }
            
            UnityEngine.Debug.Log("Done");
        }

        private void GetAssetCopyOperationPath(Object asset, out string sourcePath, out string outputPath)
        {
            string outputFolder = EditorPath.Combine(EditorPath.ProjectRoot, _config.OutputFolder.path);
            string assetRelativePath = AssetDatabase.GetAssetPath(asset);
            string assetName = Path.GetFileName(assetRelativePath);
            sourcePath = EditorPath.Combine(EditorPath.ProjectRoot, assetRelativePath);
            outputPath = EditorPath.Combine(outputFolder, assetName);
        }
    }
}