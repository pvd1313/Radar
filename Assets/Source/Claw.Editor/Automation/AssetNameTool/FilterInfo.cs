using System;
using Beast.Claw.Text;
using UnityEngine;

namespace Beast.Claw.Editor.Automation.AssetNameTool
{
    [Serializable]
    internal struct FilterInfo
    {
        [SerializeField] public bool enabled;
        [SerializeField] public RegexPattern scannerFilter;
        [SerializeField] public RegexPatternSubstitution nameConverter;
    }
}