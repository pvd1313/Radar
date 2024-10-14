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
        
        public EditorFlow Begin()
        {
            _commandStackSize.Push(0);

            return this;
        }

        public EditorUIResult Draw(Action<EditorUI> action)
        {
            EditorUIResult result = new ()
            {
                changesDetected = false,
                errorsEncountered = false,
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

            catch (AssertionException e)
            {
                EditorGUILayout.HelpBox(e.Message, MessageType.Warning);

                result.errorsEncountered = true;
            }

            catch (ValidationException e)
            {
                EditorGUILayout.HelpBox(e.Message, MessageType.Warning);

                result.errorsEncountered = true;
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

                result.errorsEncountered = true;

            }

            _undoLastCommands(ref result);

            result.errorsEncountered |= uiErrorsEncountered;
            
            uiErrorsEncountered = false;

            return result;
        }

        public EditorFlow Enabled(bool isEnabled)
        {
            _push(ECommandType.DisabledGroup);
            
            EditorGUI.BeginDisabledGroup(!isEnabled);

            return this;
        }

        public EditorFlow Vertical()
        {
            _push(ECommandType.BeginVertical);
            
            EditorGUILayout.BeginVertical();

            return this;
        }

        public EditorFlow Horizontal()
        {
            _push(ECommandType.BeginHorizontal);
            
            EditorGUILayout.BeginHorizontal();

            return this;
        }

        public EditorFlow Indent()
        {
            _push(ECommandType.AddIndent);
            
            EditorGUI.indentLevel++;

            return this;
        }

        public EditorFlow Option(GUILayoutOption option)
        {
            _push(ECommandType.LayoutOption);

            _options.Add(option);

            return this;
        }

        public EditorFlow Scroll(ref float2 scrollPosition)
        {
            _push(ECommandType.ScrollView);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            return this;
        }

        public EditorFlow Spy()
        {
            _push(ECommandType.ChangeCheck);
            
            EditorGUI.BeginChangeCheck();

            return this;
        }
    }
}