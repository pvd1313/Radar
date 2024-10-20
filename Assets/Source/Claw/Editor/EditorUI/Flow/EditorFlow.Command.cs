using System;
using Beast.Claw.Debug;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor
{
    partial class EditorFlow
    {
        public void Close()
        {
            if (_isFatal)
            {
                return;
            }
            
            if (_layoutOptionCount > 0 || 
                _commands.Count > 0 || 
                _commandStackSize.Count > 0)
            {
                BDebug.LogError(this, "Flow is not clear");
            }

            _reset();
        }

        public void ExitGUIException()
        {
            if (_isFatal)
            {
                return;
            }
            
            _reset();
        }
        
        public IEditorFlow Begin()
        {
            if (_isFatal)
            {
                new EditorUI(this).ErrorBox(_fatalMessage);
                
                return _emptyFlow;
            }
            
            _commandStackSize.Push(0);

            return this;
        }

        public void StagedBeginDraw(out EditorUI ui, out EditorUIResult result)
        {
            result = default;
            ui = new EditorUI(this);
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
            _recordUndoWithType(EUndoCommandType.DisabledGroup);
            
            EditorGUI.BeginDisabledGroup(!isEnabled);

            return this;
        }

        public IEditorFlow Vertical()
        {
            _recordUndoWithType(EUndoCommandType.BeginVertical);
            
            EditorGUILayout.BeginVertical();

            return this;
        }

        public IEditorFlow Horizontal()
        {
            _recordUndoWithType(EUndoCommandType.BeginHorizontal);
            
            EditorGUILayout.BeginHorizontal();

            return this;
        }

        public IEditorFlow Option(GUILayoutOption option)
        {
            option.ValidateNotNullThrowException(nameof(option));
            
            if (_layoutOptionCount >= FATAL_LAYOUT_OPTION_COUNT)
            {
                throw new InvalidOperationException("Too many layout options.");
            }
            
            _recordUndoWithType(EUndoCommandType.LayoutOption);

            GUILayoutOption[] currentLayout = _layoutOptions[_layoutOptionCount];
            GUILayoutOption[] newLayout = _layoutOptions[_layoutOptionCount + 1];
            Array.Copy(currentLayout, newLayout, _layoutOptionCount);
            newLayout[^1] = option;

            _layoutOptionCount++;

            return this;
        }

        public IEditorFlow Scroll(ref float2 scrollPosition)
        {
            _recordUndoWithType(EUndoCommandType.ScrollView);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            return this;
        }

        public IEditorFlow Spy()
        {
            _recordUndoWithType(EUndoCommandType.ChangeCheck);
            
            EditorGUI.BeginChangeCheck();

            return this;
        }
        
        public IEditorFlow Indent(int indent)
        {
            if (indent < 0)
            {
                throw new ArgumentException("Indent can not be less than zero");
            }

            _recordUndo(new CommandUndo
            {
                type = EUndoCommandType.SetIndent,
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
            
            _recordUndo(new CommandUndo
            {
                type = EUndoCommandType.GUIStyle,
                reference = _currentStyle
            });

            _currentStyle = style;

            return this;
        }

        public IEditorFlow LabelWidth(float labelWidth)
        {
            if (labelWidth < 0)
            {
                throw new ArgumentException("Label width can no be less than zero");
            }
            
            _recordUndo(new CommandUndo
            {
                type = EUndoCommandType.LabelWidth,
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

            // 0 seems to be not working. Looks like there is some inner checks in Unity.
            if (fieldWidth == 0)
            {
                fieldWidth = float.Epsilon;
            }
            
            _recordUndo(new CommandUndo
            {
                type = EUndoCommandType.FieldWidth,
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