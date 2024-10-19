using System;
using System.Runtime.InteropServices;
using Beast.Claw.Collection;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor
{
    public partial class EditorFlow
    {
        private enum ECommandType
        {
            None,
            BeginVertical,
            BeginHorizontal,
            SetIndent,
            ScrollView,
            LayoutOption,
            DisabledGroup,
            ChangeCheck,
            GUIStyle,
            LabelWidth,
            FieldWidth
        }
        
        private struct CommandUndo
        {
            public ECommandType type;
            public object reference;
            public StructValue value;

            public override string ToString()
            {
                return $"(type: 'ECommandType.{type}'; ref: {reference}); value: {value})";
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct StructValue
        {
            [FieldOffset(0)] public int asInt;
            [FieldOffset(0)] public float asFloat;

            public override string ToString()
            {
                return $"(asInt: {asInt}; asFloat: {asFloat})";
            }
        }
        
        private void _undo(CommandUndo commandUndo, ref EditorUIResult result)
        {
            switch (commandUndo.type)
            {
                case ECommandType.BeginVertical:
                    EditorGUILayout.EndVertical();
                    break;
                
                case ECommandType.BeginHorizontal:
                    EditorGUILayout.EndHorizontal();
                    break;
                
                case ECommandType.SetIndent:
                    EditorGUI.indentLevel = commandUndo.value.asInt;
                    break;
                
                case ECommandType.LayoutOption:
                    _options.Pop();
                    break;
                
                case ECommandType.ScrollView:
                    EditorGUILayout.EndScrollView();
                    break;
                
                case ECommandType.DisabledGroup:
                    EditorGUI.EndDisabledGroup();
                    break;
                
                case ECommandType.ChangeCheck:
                    result.changesDetected |= EditorGUI.EndChangeCheck();
                    break;
                
                case ECommandType.GUIStyle:
                    _customStyle = (GUIStyle)commandUndo.reference;
                    break;
                
                case ECommandType.LabelWidth:
                    EditorGUIUtility.labelWidth = commandUndo.value.asFloat;
                    break;
                
                case ECommandType.FieldWidth:
                    EditorGUIUtility.fieldWidth = commandUndo.value.asFloat;
                    break;
                
                default:
                    throw new InvalidOperationException("Unknown command encountered: " + commandUndo);
            }
        }
    }
}