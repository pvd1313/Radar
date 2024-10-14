using System;
using System.Collections.Generic;
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
            AddIndent,
            ScrollView,
            LayoutOption,
            DisabledGroup,
            ChangeCheck,
        }

        public bool uiErrorsEncountered;
        
        private readonly Stack<int> _commandStackSize = new();
        private readonly Stack<ECommandType> _commands = new();
        private readonly List<GUILayoutOption> _options = new();

        private void _push(ECommandType command)
        {
            int stackSize = _commandStackSize.Pop();
            _commandStackSize.Push(stackSize + 1);
            _commands.Push(command);
        }

        private void _undoLastCommands(ref EditorUIResult result)
        {
            int stackSize = _commandStackSize.Pop();
            for (int i = 0; i < stackSize; i++)
            {
                _undo(_commands.Pop(), ref result);
            }
        }

        private void _undo(ECommandType command, ref EditorUIResult result)
        {
            switch (command)
            {
                case ECommandType.BeginVertical:
                    EditorGUILayout.EndVertical();
                    break;
                
                case ECommandType.BeginHorizontal:
                    EditorGUILayout.EndHorizontal();
                    break;
                
                case ECommandType.AddIndent:
                    EditorGUI.indentLevel--;
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
                
                default:
                    throw new InvalidOperationException("Unknown command encountered: " + command);
            }
        }
    }
}