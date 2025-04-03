using System.Collections.Generic;
using Beast.Claw.Attributes;
using UnityEngine;

namespace Beast.Claw.Editor.Automation.AssetNameTool
{
    public class AssetNameToolAsset : ToolAsset
    {
        [SerializeField] private Config _config;

        [Button]
        private void ScanAndLog()
        {
            ConsoleUtil.ClearLog();
            
            FileProcessor processor = new();
            List<FileInfo> files = new();
            processor.Process(_config, files);
            
            foreach (FileInfo file in files)
            {
                UnityEngine.Debug.Log($"({file.typeName}) ({file.foundByRegexId}) '{file.originalPath}' -> '{file.modifiedPath}'");
            }
        }

        [Button]
        private void RenameAssets()
        {
            ConsoleUtil.ClearLog();
            
            FileProcessor processor = new();
            FileNameUtil renamer = new();
            List<FileInfo> files = new();
            
            processor.Process(_config, files);
            renamer.Rename(files);
        }
    }
}