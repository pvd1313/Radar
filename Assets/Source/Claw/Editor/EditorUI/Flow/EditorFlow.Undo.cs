using System;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor
{
    public partial class EditorFlow
    {
        private enum EUndoCommandType
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
            public EUndoCommandType type;
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
        
        private void _undo(CommandUndo command, ref EditorUIResult result)
        {
            switch (command.type)
            {
                case EUndoCommandType.BeginVertical:
                    EditorGUILayout.EndVertical();
                    break;
                
                case EUndoCommandType.BeginHorizontal:
                    EditorGUILayout.EndHorizontal();
                    break;
                
                case EUndoCommandType.SetIndent:
                    EditorGUI.indentLevel = command.value.asInt;
                    break;
                
                case EUndoCommandType.LayoutOption:
                    _layoutOptionCount--;
                    break;
                
                case EUndoCommandType.ScrollView:
                    EditorGUILayout.EndScrollView();
                    break;
                
                case EUndoCommandType.DisabledGroup:
                    EditorGUI.EndDisabledGroup();
                    break;
                
                case EUndoCommandType.ChangeCheck:
                    result.changesDetected |= EditorGUI.EndChangeCheck();
                    break;
                
                case EUndoCommandType.GUIStyle:
                    _currentStyle = (GUIStyle)command.reference;
                    break;
                
                case EUndoCommandType.LabelWidth:
                    EditorGUIUtility.labelWidth = command.value.asFloat;
                    break;
                
                case EUndoCommandType.FieldWidth:
                    EditorGUIUtility.fieldWidth = command.value.asFloat;
                    break;
                
                default:
                    throw new InvalidOperationException("Unknown command encountered: " + command);
            }
        }
    }
}