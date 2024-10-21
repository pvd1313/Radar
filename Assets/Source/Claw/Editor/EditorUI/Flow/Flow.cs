using System;
using System.Collections.Generic;
using Beast.Claw.Collection;
using Beast.Claw.Debug;
using Beast.Claw.Editor.Drawers;
using Beast.Claw.Editor.Fields;
using Beast.Claw.Math;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor
{
    public partial class Flow : IFlow
    {
        private const int TEMP_ARRAY_CAPACITY = 8;
        private const string FATAL_MESSAGE = ": Flow terminated and not able to process commands until domain reload.";
        private const int MAX_DRAW_CALL_COUNT = 1024;
        private const int MAX_COMMAND_CALL_COUNT = 1024;
        
        private readonly Stack<int> _commandStackSize = new();
        private readonly Stack<CommandUndo> _commands = new();
        private readonly Empty _emptyFlow = new ();
        private readonly TempArray<GUILayoutOption> _layoutTempArray = new(TEMP_ARRAY_CAPACITY);

        private int _flowDepth;
        private bool _validationFailed;
        private GUIStyle _currentStyle;
        private int _layoutOptionCount;
        private int _drawCallCount;
        private int _commandCallCount;
        private bool _isFatal;
        private string _fatalMessage;

        public Flow()
        {
            rectUI = new RectUI();
            
            rectUI.BindRect<Float2Rect, Float2Task>();
            rectUI.BindField<Float2Field, float2>();
            rectUI.BindField<FloatMinMaxField, FloatMinMax>();
        }

        private void _handleException(ref FlowUIResult uiResult, Exception exception)
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

        private void _recordUndoWithType(EUndoCommandType type)
        {
            CommandUndo commandUndo = default;
            commandUndo.type = type;
            
           _recordUndo(commandUndo);
        }

        private void _recordUndo(CommandUndo commandUndo)
        {
            if (_commandCallCount >= MAX_COMMAND_CALL_COUNT)
            {
                _fatal("Max command call count reached");
            }
            
            int stackSize = _commandStackSize.Pop();
            _commandStackSize.Push(stackSize + 1);
            _commands.Push(commandUndo);
            _commandCallCount++;
        }

        private void _undoLastCommands(ref FlowUIResult result)
        {
            int stackSize = _commandStackSize.Pop();
            for (int i = 0; i < stackSize; i++)
            {
                _undo(_commands.Pop(), ref result);
            }
        }

        private void _fatal(string reason)
        {
            FlowUIResult result = default;

            _undoLastCommands(ref result);
            
            _drawCallCount = 0;
            _commandCallCount = 0;
            
            _isFatal = true;
            _fatalMessage = reason + FATAL_MESSAGE;
            
            throw new Exception(_fatalMessage);
        }

        private void _reset()
        {
            _drawCallCount = 0;
            _commandCallCount = 0;
            _currentStyle = null;
            _commands.Clear();
            _commandStackSize.Clear();
            _layoutOptionCount = 0;
        }
    }
}