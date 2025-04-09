using System.Collections.Generic;
using System.IO;
using System.Linq;
using Beast.Claw.Attributes;
using Beast.Claw.Editor.IO;
using UnityEngine;

namespace Beast.Claw.Editor.Automation.NamespaceTool
{
    internal class NamespaceToolAsset : ToolAsset
    {
        [SerializeField] private Config _config;

        [Button]
        private void ScanFoldersAndLog()
        {
            ConsoleUtil.ClearLog();
            
            FolderScanner folderScanner = new();
            HashSet<FolderInfo> folders = folderScanner.Scan(_config);

            foreach (FolderInfo info in folders)
            {
                string regex = _config.FolderFilters[info.regexId].filter.pattern;
                string message = $"'{info.name}' by regex '{regex}' ({info.regexId})";
                UnityEngine.Debug.Log(message);
            }
            
            UnityEngine.Debug.Log($"Total folders: {folders.Count};");
        }

        [Button]
        private void ScanAssembliesAndLog()
        {
            ConsoleUtil.ClearLog();

            AssemblyUtil assemblyUtil = new();
            List<AssemblyInfo> assemblies = assemblyUtil.Scan(_config);

            foreach (AssemblyInfo assembly in assemblies)
            {
                string regex = _config.AssemblyFilters[assembly.regexId].filter.pattern;
                string fileName = assemblyUtil.AssemblyToFileName(assembly.name);
                string message = $"'{assembly.name}' -> '{fileName}' by regex '{regex}' ({assembly.regexId}):";
                
                UnityEngine.Debug.Log(message);
                
                foreach (string folder in assembly.folders)
                {
                    UnityEngine.Debug.Log($"\t{folder}");
                }
            }

            int folderCount = assemblies.SelectMany(a => a.folders).Count();
            UnityEngine.Debug.Log($"Total assemblies: {assemblies.Count}; Total folders: {folderCount}");
        }

        [Button]
        private void OverrideProjDotSettings()
        {
            string[] oldFiles = Directory.GetFiles(EditorPath.ProjectRoot, "*.csproj.DotSettings");
            foreach (string file in oldFiles)
            {
                File.Delete(file);
            }
            
            FolderScanner folderScanner = new();
            AssemblyUtil assemblyUtil = new();
            DotSettingsWriter dotSettingsWriter = new();
            HashSet<FolderInfo> folders = folderScanner.Scan(_config);
            HashSet<string> folderNames = new(folders.Select(f => f.name));
            List<AssemblyInfo> assemblies = assemblyUtil.Scan(_config);
            foreach (AssemblyInfo assembly in assemblies)
            {
                string assemblyPath = assemblyUtil.AssemblyToFileName(assembly.name);
                IEnumerable<string> foldersToSkip = assembly.folders.Where(f => folderNames.Contains(f));
                dotSettingsWriter.Write(assemblyPath, foldersToSkip);
            }

            UnityEngine.Debug.Log("Done. Don't forget to restart Rider.");
        }

        [Button]
        private void RegexHelp()
        {
            Application.OpenURL("https://regex101.com/");
        }
    }
}