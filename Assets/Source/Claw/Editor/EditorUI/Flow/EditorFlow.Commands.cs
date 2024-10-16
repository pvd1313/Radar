using System;
using Beast.Claw.Debug;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor
{
    partial class EditorFlow
    {
        public void CheckFlowIsFinished()
        {
            if (_options.Count > 0 || _commands.Count > 0 || _commandStackSize.Count > 0)
            {
                BDebug.LogError(this, "Flow is not clear");
            }
        }

        public void TerminateFlow()
        {
            for (int i = 0; i < 100 && _commandStackSize.Count > 0; i++)
            {
                EditorUIResult result = default;
                
                _undoLastCommands(ref result);
            }
        }
        
        public IEditorFlow Begin()
        {
            _commandStackSize.Push(0);

            return this;
        }

        public EditorUIResult Draw(Action<EditorUI> action)
        {
            EditorUIResult result = new ()
            {
                changesDetected = false,
                hasErrors = false,
            };

            try
            {
                action.Invoke(new EditorUI
                {
                    flow = this,
                    layoutOptions = _options.ToArray(),
                    niceLabelsEnabled = true
                });
            }

            catch (ExitGUIException)
            {
                throw;
            }

            catch (Exception e)
            {
                EditorGUILayout.HelpBox($"Unhandled exception:", MessageType.Error);

                EditorGUILayout.LabelField(e.Message);

                EditorGUILayout.TextArea(e.StackTrace);

                BDebug.StaticLogException(e);

                result.hasErrors = true;

            }
            finally
            {
                _undoLastCommands(ref result);
                
                result.hasErrors |= uiErrorsEncountered;
            
                uiErrorsEncountered = false;
            }
            
            return result;
        }

        public IEditorFlow Enabled(bool isEnabled)
        {
            _push(ECommandType.DisabledGroup);
            
            EditorGUI.BeginDisabledGroup(!isEnabled);

            return this;
        }

        public IEditorFlow Vertical()
        {
            _push(ECommandType.BeginVertical);
            
            EditorGUILayout.BeginVertical();

            return this;
        }

        public IEditorFlow Horizontal()
        {
            _push(ECommandType.BeginHorizontal);
            
            EditorGUILayout.BeginHorizontal();

            return this;
        }

        public IEditorFlow Indent()
        {
            _push(ECommandType.AddIndent);
            
            EditorGUI.indentLevel++;

            return this;
        }

        public IEditorFlow Option(GUILayoutOption option)
        {
            _push(ECommandType.LayoutOption);

            _options.Add(option);

            return this;
        }

        public IEditorFlow Scroll(ref float2 scrollPosition)
        {
            _push(ECommandType.ScrollView);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            return this;
        }

        public IEditorFlow Spy()
        {
            _push(ECommandType.ChangeCheck);
            
            EditorGUI.BeginChangeCheck();

            return this;
        }

        public IEditorFlow If(bool condition)
        {
            if (!condition)
            {
                EditorUIResult uiResult = default;
                
                _undoLastCommands(ref uiResult);

                return _emptyFlow;
            }

            return this;
        }
    }
}