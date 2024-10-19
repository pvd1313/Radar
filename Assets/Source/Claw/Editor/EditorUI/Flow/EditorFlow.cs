using System;
using System.Collections.Generic;
using Beast.Claw.Debug;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor
{
    public partial class EditorFlow : IEditorFlow
    {
        public GUIStyle CurrentStyle => _customStyle;
        
        private readonly Stack<int> _commandStackSize = new();
        private readonly Stack<CommandUndo> _commands = new();
        private readonly List<GUILayoutOption> _options = new();
        private readonly Empty _emptyFlow = new ();
        private readonly GUILayoutOptionArrayPool _layoutOptionPool = new();

        private bool _validationFailed;
        private GUIStyle _customStyle;

        private EditorUI _createEditorUI()
        {
            GUILayoutOption[] layoutOptions = Array.Empty<GUILayoutOption>();
            
            if (_options.Count > 0)
            {
                layoutOptions = _layoutOptionPool.GetOrCreate(_options.Count);
                _options.CopyTo(layoutOptions);
            }
            
            return new EditorUI
            {
                flow = this,
                // TODO: https://risenbeast.youtrack.cloud/issue/R-48/Editor-flow-garbage-at-layout-options
                //     : Not optimal, will produce garbage.
                layoutOptions = layoutOptions
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

        private void _pushUndoWithType(ECommandType type)
        {
            CommandUndo commandUndo = default;
            commandUndo.type = type;
            
           _pushUndo(commandUndo);
        }

        private void _pushUndo(CommandUndo commandUndo)
        {
            int stackSize = _commandStackSize.Pop();
            _commandStackSize.Push(stackSize + 1);
            _commands.Push(commandUndo);
        }

        private void _undoLastCommands(ref EditorUIResult result)
        {
            int stackSize = _commandStackSize.Pop();
            for (int i = 0; i < stackSize; i++)
            {
                _undo(_commands.Pop(), ref result);
            }
        }
    }
}