using UnityEngine;

namespace Beast.Claw.Editor.Automation.ChangelogTool
{
    public class ChangelogToolAsset : ToolAsset
    {
        public ChangelogToolConfig Config => _config;
        
        [SerializeField] private ChangelogToolConfig _config;
    }
}