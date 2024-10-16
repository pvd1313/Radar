using System;
using System.Collections.Generic;
using Beast.Claw.Collection;
using Beast.Claw.Debug;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor
{
    public partial class EditorFlow : IEditorFlow
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

        private readonly Stack<int> _commandStackSize = new();
        private readonly Stack<ECommandType> _commands = new();
        private readonly List<GUILayoutOption> _options = new();
        private readonly Empty _emptyFlow = new ();

        private bool _validationFailed;
        private bool _niceLabelsEnabled = false;

        private EditorUI _createEditorUI()
        {
            return new EditorUI
            {
                flow = this,
                // TODO: https://risenbeast.youtrack.cloud/issue/R-48/Editor-flow-garbage-at-layout-options
                //     : Not optimal, will produce garbage.
                layoutOptions = _options.ToArray(),
            };
        }

        private void _handleException(ref EditorUIResult uiResult, Exception exception)
        {
            if (exception is ExitGUIException)
            {
                throw exception;
            }
            
            EditorGUILayout.HelpBox($"Unhandled exception:", MessageType.Error);

            EditorGUILayout.LabelField(exception.Message);

            EditorGUILayout.TextArea(exception.StackTrace);

            BDebug.StaticLogException(exception);
        }

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