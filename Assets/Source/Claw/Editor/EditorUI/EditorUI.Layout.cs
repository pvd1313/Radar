using System;
using Unity.Mathematics;
using UnityEditor;

namespace Beast.Claw.Editor
{
    partial struct EditorUI
    {
        public EditorUI VerticalEnabledGroup(bool isEnabled, params Action[] actions)
        {
            EditorGUILayout.BeginVertical();
            EditorGUI.BeginDisabledGroup(!isEnabled);

            EditorUI result = ExecuteActions();

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();

            return result;
        }

        public EditorUI EnabledGroup(bool isEnabled, params Action[] actions)
        {
            EditorGUI.BeginDisabledGroup(!isEnabled);

            EditorUI result = ExecuteActions(actions);

            EditorGUI.EndDisabledGroup();

            return result;
        }

        public EditorUI HorizontalEnabledGroup(bool isEnabled, params Action[] actions)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(!isEnabled);

            EditorUI result = ExecuteActions(actions);

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            return result;
        }

        public EditorUI Vertical(params Action[] actions)
        {
            EditorGUILayout.BeginVertical();

            EditorUI result = ExecuteActions(actions);

            EditorGUILayout.EndVertical();

            return result;
        }

        public EditorUI VerticalScroll(ref float2 scrollPosition, params Action[] actions)
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorUI result = ExecuteActions(actions);
            
            EditorGUILayout.EndScrollView();

            return result;
        }

        public EditorUI Horizontal(params Action[] actions)
        {
            EditorGUILayout.BeginHorizontal();

            EditorUI result = ExecuteActions(actions);

            EditorGUILayout.EndHorizontal();

            return result;
        }
    }
}