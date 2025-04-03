using System.Collections.Generic;
using Beast.Claw.Editor.IO;
using Beast.Claw.Text;
using UnityEngine;

namespace Beast.Claw.Editor.Automation.FileCopyTool
{
    [System.Serializable]
    public class FileCopyToolConfig
    {
        public EditorFolderPath OutputFolder => _outputFolder;
        public IReadOnlyList<Object> AssetsToCopy => _assetsToCopy;
        public IReadOnlyList<RegexPattern> DeleteFilesPatterns => _deleteFilesPatterns;
        
        [SerializeField] private EditorFolderPath _outputFolder;
        [SerializeField] private List<RegexPattern> _deleteFilesPatterns;
        [SerializeField] private List<Object> _assetsToCopy;
    }
}