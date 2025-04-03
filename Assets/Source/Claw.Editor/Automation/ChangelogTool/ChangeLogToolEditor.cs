#if ODIN_INSPECTOR

using System;
using System.Collections.Generic;
using System.Linq;
using Beast.Claw.Text;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using StringBuilder = System.Text.StringBuilder;

namespace Beast.Claw.Editor.Automation.ChangelogTool
{
    [CustomEditor(typeof(ChangelogToolAsset))]
    public class ChangeLogToolEditor : OdinEditor
    {
        [SerializeField] private string _inputText;
        [SerializeField] private string _outputText;
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            _inputText = EditorGUILayout.TextArea(_inputText, GUILayout.MaxHeight(400));

            if (GUILayout.Button("Convert"))
            {
                _outputText = Convert(_inputText, ((ChangelogToolAsset) target).Config);
            }

            _outputText = EditorGUILayout.TextArea(_outputText, GUILayout.MaxHeight(400));
        }

        private string Convert(string source, ChangelogToolConfig config)
        {
            string[] lines = source.Split(new [] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder builder = new();
            IReadOnlyList<RegexPattern> blacklist = config.Blacklist;
            IReadOnlyList<RegexPatternSubstitution> converters = config.Converters;
            
            foreach (string line in lines)
            {
                if (blacklist.Any(bp => bp.IsMatch(line)))
                {
                    continue;
                }

                string converted = line;
                foreach (RegexPatternSubstitution converter in converters)
                {
                    converted = converter.Replace(converted);
                }
                
                builder.Append(converted);
                builder.Append(Environment.NewLine);
            }

            return builder.ToString();
        }
    }
}

#endif