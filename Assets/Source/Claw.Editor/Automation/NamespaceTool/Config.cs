using System.Collections.Generic;
using Beast.Claw.Editor.IO;
using UnityEngine;

namespace Beast.Claw.Editor.Automation.NamespaceTool
{
    [System.Serializable]
    internal class Config
    {
        public EditorFolderPath ScanPath => _scanPath;
        public IReadOnlyList<FilterInfo> FolderFilters => _folderFilters;
        public IReadOnlyList<FilterInfo> AssemblyFilters => _assemblyFilters;

        [SerializeField] private EditorFolderPath _scanPath;
        
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.TableList]
#endif
        [SerializeField] private List<FilterInfo> _folderFilters;
        
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.TableList]
#endif
        [SerializeField] private List<FilterInfo> _assemblyFilters;
    }
}