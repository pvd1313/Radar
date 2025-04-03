using System;
using System.Collections.Generic;
using Beast.Claw.Editor.IO;
using UnityEngine;

namespace Beast.Claw.Editor.Automation.ProjectStructureTool
{
    [Serializable]
    internal class Config
    {
        public EditorFolderPath ScanPath => _scanPath;
        public EditorFilePath ExportPath => _exportPath;
        public IReadOnlyList<FilterInfo> Blacklist => _blacklist;

        [SerializeField] private EditorFolderPath _scanPath;
        [SerializeField] private EditorFilePath _exportPath;
        [SerializeField] private List<FilterInfo> _blacklist;
    }
}