using System.Collections.Generic;
using Beast.Claw.Text;
using UnityEngine;

namespace Beast.Claw.Editor.Automation.ChangelogTool
{
    [System.Serializable]
    public class ChangelogToolConfig
    {
        public IReadOnlyList<RegexPattern> Blacklist => _blacklist;
        public IReadOnlyList<RegexPatternSubstitution> Converters => _converters;
        
#if ODIN_INSPECTOR
[Sirenix.OdinInspector.TableList]
#endif
        [SerializeField] private List<RegexPattern> _blacklist;
        
#if ODIN_INSPECTOR
[Sirenix.OdinInspector.TableList]
#endif
        [SerializeField] private List<RegexPatternSubstitution> _converters;
    }
}