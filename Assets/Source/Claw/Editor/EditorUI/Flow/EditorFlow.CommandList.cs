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

            if (ui.layoutOptions.Length > 0)
            {
                _layoutOptionPool.Dispose(ui.layoutOptions);
                ui.layoutOptions = null;
            }
            
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
            _pushUndoWithType(ECommandType.DisabledGroup);
            
            EditorGUI.BeginDisabledGroup(!isEnabled);

            return this;
        }

        public IEditorFlow Vertical()
        {
            _pushUndoWithType(ECommandType.BeginVertical);
            
            EditorGUILayout.BeginVertical();

            return this;
        }

        public IEditorFlow Horizontal()
        {
            _pushUndoWithType(ECommandType.BeginHorizontal);
            
            EditorGUILayout.BeginHorizontal();

            return this;
        }

        public IEditorFlow Option(GUILayoutOption option)
        {
            _pushUndoWithType(ECommandType.LayoutOption);

            _options.Add(option);

            return this;
        }

        public IEditorFlow Scroll(ref float2 scrollPosition)
        {
            _pushUndoWithType(ECommandType.ScrollView);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            return this;
        }

        public IEditorFlow Spy()
        {
            _pushUndoWithType(ECommandType.ChangeCheck);
            
            EditorGUI.BeginChangeCheck();

            return this;
        }
        
        public IEditorFlow Indent(int indent)
        {
            if (indent < 0)
            {
                throw new ArgumentException("Indent can not be less than zero");
            }

            _pushUndo(new CommandUndo
            {
                type = ECommandType.SetIndent,
                value = new StructValue
                {
                    asInt = EditorGUI.indentLevel
                }
            });

            EditorGUI.indentLevel = indent;

            return this;
        }

        public IEditorFlow Style(GUIStyle style)
        {
            style.ValidateNotNullThrowException(nameof(style));
            
            _pushUndo(new CommandUndo
            {
                type = ECommandType.GUIStyle,
                reference = _customStyle
            });

            _customStyle = style;

            return this;
        }

        public IEditorFlow LabelWidth(float labelWidth)
        {
            if (labelWidth < 0)
            {
                throw new ArgumentException("Label width can no be less than zero");
            }
            
            _pushUndo(new CommandUndo
            {
                type = ECommandType.LabelWidth,
                value = new StructValue
                {
                    asFloat = EditorGUIUtility.labelWidth
                }
            });

            EditorGUIUtility.labelWidth = labelWidth;

            return this;
        }

        public IEditorFlow FieldWidth(float fieldWidth)
        {
            if (fieldWidth < 0)
            {
                throw new ArgumentException("Field width can no be less than zero");
            }
            
            _pushUndo(new CommandUndo
            {
                type = ECommandType.FieldWidth,
                value = new StructValue
                {
                    asFloat = EditorGUIUtility.fieldWidth
                }
            });

            EditorGUIUtility.fieldWidth = fieldWidth;

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