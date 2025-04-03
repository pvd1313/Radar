using Beast.Claw.Text;
using UnityEngine;

namespace Beast.Claw.Editor.Automation.ProjectStructureTool
{
    [System.Serializable]
    internal struct FilterInfo
    {
        [SerializeField] public bool enabled;
        [SerializeField] public RegexPattern filter;
    }
}