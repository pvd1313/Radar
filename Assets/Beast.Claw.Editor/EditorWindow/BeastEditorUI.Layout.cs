using System;
using UnityEditor;

namespace Beast.Claw.Editor
{
    partial struct BeastEditorUI
    {
        public BeastEditorUI VerticalEnabledGroup(bool isEnabled, params Action[] actions)
        {
            EditorGUILayout.BeginVertical();
            EditorGUI.BeginDisabledGroup(!isEnabled);

            BeastEditorUI result = ExecuteActions();

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();

            return result;
        }

        public BeastEditorUI EnabledGroup(bool isEnabled, params Action[] actions)
        {
            EditorGUI.BeginDisabledGroup(!isEnabled);

            BeastEditorUI result = ExecuteActions(actions);

            EditorGUI.EndDisabledGroup();

            return result;
        }

        public BeastEditorUI HorizontalEnabledGroup(bool isEnabled, params Action[] actions)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(!isEnabled);

            BeastEditorUI result = ExecuteActions(actions);

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            return result;
        }

        public BeastEditorUI Vertical(params Action[] actions)
        {
            EditorGUILayout.BeginVertical();

            BeastEditorUI result = ExecuteActions(actions);

            EditorGUILayout.EndVertical();

            return result;
        }

        public BeastEditorUI Horizontal(params Action[] actions)
        {
            EditorGUILayout.BeginHorizontal();

            BeastEditorUI result = ExecuteActions(actions);

            EditorGUILayout.EndHorizontal();

            return result;
        }
    }
}