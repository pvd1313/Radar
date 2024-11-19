using System;
using Beast.Claw.Common;
using Beast.Claw.Validation;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor
{
    partial class Flow
    {
        public IFlow Begin()
        {
            if (_isFatal)
            {
                new FlowUI(this, _rectUI).ErrorBox(_fatalMessage);
                
                return _emptyFlow;
            }
            
            _commandStackSize.Push(0);

            return this;
        }

        public FlowUIResult Draw(Action<FlowUI> action)
        {
            StagedBeginDraw(out FlowUI ui, out FlowUIResult result);

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

        public IFlow Enabled(bool isEnabled)
        {
            _recordUndoWithType(EUndoCommandType.DisabledGroup);
            
            EditorGUI.BeginDisabledGroup(!isEnabled);

            return this;
        }

        public IFlow Vertical()
        {
            _recordUndoWithType(EUndoCommandType.BeginVertical);
            
            EditorGUILayout.BeginVertical();

            return this;
        }

        public IFlow Horizontal()
        {
            _recordUndoWithType(EUndoCommandType.BeginHorizontal);
            
            EditorGUILayout.BeginHorizontal();

            return this;
        }

        public IFlow Option(GUILayoutOption option)
        {
            option.ValidateNotNullThrowException(nameof(option));
            
            if (_layoutOptionCount >= TEMP_ARRAY_CAPACITY)
            {
                throw new InvalidOperationException("Too many layout options.");
            }
            
            _recordUndoWithType(EUndoCommandType.LayoutOption);

            GUILayoutOption[] currentLayout = _layoutTempArray[_layoutOptionCount];
            GUILayoutOption[] newLayout = _layoutTempArray[_layoutOptionCount + 1];
            Array.Copy(currentLayout, newLayout, _layoutOptionCount);
            newLayout[^1] = option;

            _layoutOptionCount++;

            return this;
        }

        public IFlow Scroll(ref float2 scrollPosition)
        {
            _recordUndoWithType(EUndoCommandType.ScrollView);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            return this;
        }

        public IFlow Spy()
        {
            _recordUndoWithType(EUndoCommandType.ChangeCheck);
            
            EditorGUI.BeginChangeCheck();

            return this;
        }
        
        public IFlow Indent(int indent)
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

        public IFlow Style(GUIStyle style)
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

        public IFlow LabelWidth(float labelWidth)
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

        public IFlow FieldWidth(float fieldWidth)
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

        public IFlow If(bool condition)
        {
            if (!condition)
            {
                FlowUIResult uiResult = default;
                
                _undoLastCommands(ref uiResult);

                return _emptyFlow;
            }

            return this;
        }
    }
}