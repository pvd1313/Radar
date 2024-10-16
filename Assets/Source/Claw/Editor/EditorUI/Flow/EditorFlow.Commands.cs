using System;
using Beast.Claw.Debug;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor
{
    partial class EditorFlow
    {
        public void NotifyValidationFailed()
        {
            _validationFailed = true;
        }
        
        public string NicifyLabel(string label)
        {
            if (!_niceLabelsEnabled)
            {
                return label;
            }

            return ObjectNames.NicifyVariableName(label);
        }
        
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

        public void StagedBeginDraw(out EditorUI ui, out EditorUIResult result)
        {
            result = default;
            ui = _createEditorUI();
        }
        
        public void StagedHandleException(ref EditorUI ui, ref EditorUIResult result, Exception exception)
        {
            _handleException(ref result, exception);
        }

        public void StagedEndDraw(ref EditorUI ui, ref EditorUIResult result)
        {
            result.validationFailed |= _validationFailed;

            _validationFailed = false;
            
            _undoLastCommands(ref result);
        }

        public EditorUIResult Draw(Action<EditorUI> action)
        {
            StagedBeginDraw(out EditorUI ui, out EditorUIResult result);

            try
            {
                action.Invoke(ui);
            }
            catch (Exception e)
            {
                StagedHandleException(ref ui, ref result, e);
            }
            finally
            {
                StagedEndDraw(ref ui, ref result);
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