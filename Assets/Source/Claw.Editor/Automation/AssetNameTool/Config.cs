using System.Collections.Generic;
using UnityEngine;

namespace Beast.Claw.Editor.Automation.AssetNameTool
{
    [System.Serializable]
    internal class Config
    {
        public IReadOnlyList<FilterInfo> Folders => _folders;
        
        [SerializeField] private List<FilterInfo> _folders;
    }
}