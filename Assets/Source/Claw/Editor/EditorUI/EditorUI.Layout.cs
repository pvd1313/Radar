using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

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
        
        public EditorUI WithOption(GUILayoutOption[] options, Action<EditorUI> actions)
        {
            EditorUI ui = this;
            
            ui.layoutOptions = options;

            return ui.ExecuteActions(actions);
        }
        
        public EditorUI WithOption(GUILayoutOption[] options)
        {
            EditorUI ui = this;
            
            ui.layoutOptions = options;
            
            return ui;
        }

        public EditorUI EnabledGroup(bool isEnabled, params Action<EditorUI>[] actions)
        {
            EditorGUI.BeginDisabledGroup(!isEnabled);

            EditorUI result = ExecuteActions(actions);

            EditorGUI.EndDisabledGroup();

            return result;
        }

        public EditorUI HorizontalEnabledGroup(bool isEnabled, params Action<EditorUI>[] actions)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(!isEnabled);

            EditorUI result = ExecuteActions(actions);

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            return result;
        }

        public EditorUI Vertical(params Action<EditorUI>[] actions)
        {
            EditorGUILayout.BeginVertical();

            EditorUI result = ExecuteActions(actions);

            EditorGUILayout.EndVertical();

            return result;
        }

        public EditorUI IndentLevel(int levelDelta, params Action<EditorUI>[] actions)
        {
            EditorGUI.indentLevel += levelDelta;
            
            EditorUI result = ExecuteActions(actions);

            EditorGUI.indentLevel -= levelDelta;

            return result;
        }

        public EditorUI VerticalScroll(ref float2 scrollPosition, params Action<EditorUI>[] actions)
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorUI result = ExecuteActions(actions);
            
            EditorGUILayout.EndScrollView();

            return result;
        }

        public EditorUI Horizontal(params Action<EditorUI>[] actions)
        {
            EditorGUILayout.BeginHorizontal();

            EditorUI result = ExecuteActions(actions);

            EditorGUILayout.EndHorizontal();

            return result;
        }
    }
}