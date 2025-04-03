using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor.Automation.AssetNameTool
{
    internal class FileProcessor
    {
        public void Process(Config config, ICollection<FileInfo> result)
        {
            result.Clear();
            
            string[] assetGUIDs = AssetDatabase.FindAssets("t:ScriptableObject");
            IReadOnlyList<FilterInfo> filters = config.Folders;
            
            for (int i = 0; i < assetGUIDs.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);

                for (int j = 0; j < filters.Count; j++)
                {
                    FilterInfo filter = filters[j];

                    if (!filter.enabled)
                    {
                        continue;
                    }

                    if (!filter.scannerFilter.IsMatch(assetPath))
                    {
                        continue;
                    }

                    ScriptableObject obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
                    string typeName = obj.GetType().ToString();
                    string oldName = Path.GetFileName(assetPath);
                    string newName = filter.nameConverter.Replace(typeName);
                    string assetRoot = assetPath.Substring(0, assetPath.LastIndexOf(oldName));

                    result.Add( new FileInfo
                    {
                        foundByRegexId = j,
                        typeName = typeName,
                        originalPath = assetPath,
                        modifiedPath = assetRoot + newName
                    });
                }
            }
        }
    }
}