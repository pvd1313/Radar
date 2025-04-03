using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace Beast.Claw.Editor.Automation.AssetNameTool
{
    internal class FileNameUtil
    {
        private readonly HashSet<string> _textHash;

        public FileNameUtil()
        {
            _textHash = new HashSet<string>();
        }
        
        public void Rename(IReadOnlyList<FileInfo> files)
        {
            _textHash.Clear();
            
            for (int i = 0; i < files.Count; i++)
            {
                FileInfo file = files[i];
                
                if (!CheckRequiresRenaming(file))
                {
                    _textHash.Add(file.modifiedPath.ToLowerInvariant());
                    continue;
                }
                
                string guid = AssetDatabase.AssetPathToGUID(file.modifiedPath);
                if (!string.IsNullOrEmpty(guid))
                {
                    UnityEngine.Debug.LogError($"Abort. Name '{file.modifiedPath}' is occupied.");
                    return;
                }

                if (!_textHash.Add(file.modifiedPath.ToLowerInvariant()))
                {
                    UnityEngine.Debug.LogError($"Abort. Name '{file.modifiedPath}' will be duplicated.");
                    return;
                }

                string message = AssetDatabase.ValidateMoveAsset(file.originalPath, file.modifiedPath);
                if (!string.IsNullOrEmpty(message))
                {
                    UnityEngine.Debug.LogError($"Abort. ValidateMoveAsset: '{message}'");
                    return;
                }
            }

            for (int i = 0; i < files.Count; i++)
            {
                FileInfo file = files[i];
                if (!CheckRequiresRenaming(file))
                {
                    continue;
                }

                string fileName = Path.GetFileName(file.modifiedPath);
                string message = AssetDatabase.RenameAsset(file.originalPath, fileName);
                if (!string.IsNullOrEmpty(message))
                {
                    UnityEngine.Debug.LogError($"Abort. RenameAsset: '{message}'");
                    return;
                }
                
                UnityEngine.Debug.Log($"Renamed '{file.originalPath}' -> '{file.modifiedPath}'; extraInfo: '{message}'");
            }
        }

        private bool CheckRequiresRenaming(FileInfo fileInfo)
        {
            const StringComparison comparison = StringComparison.InvariantCultureIgnoreCase;

            return string.Compare(fileInfo.originalPath, fileInfo.modifiedPath, comparison) != 0;
        }
    }
}