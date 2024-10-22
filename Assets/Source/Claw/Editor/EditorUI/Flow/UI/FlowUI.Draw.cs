using System;
using Beast.Claw.Attributes;
using Beast.Claw.Attributes.Annotation;
using Beast.Claw.Math;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor
{
    readonly partial struct FlowUI
    {
        [SupportsCustomStyle]
        public void Foldout([LabelMustBeSet] Label label, ref bool isFold)
        {
            _flow.NotifyDrawCall();
            
            label.ValidateHasLabelThrowException();
            
            if (_flow.TryGetCustomStyle(out GUIStyle style))
            {
                isFold = EditorGUILayout.Foldout(isFold, label.content, style);
            }
            
            isFold = EditorGUILayout.Foldout(isFold, label.content);
        }

        public void GenericField<TValue>(Label label, ref TValue value)
        {
            Rect screen = EditorGUILayout.GetControlRect();

            value = _flow.rectUI.Field(screen, label, value);
        }
        
        public TValue GenericField<TValue>(Label label, TValue value)
        {
            Rect screen = EditorGUILayout.GetControlRect();

            return _flow.rectUI.Field(screen, label, value);
        }

        [NoCustomStyle]
        public void Space(float pixelSize)
        {
            _flow.NotifyDrawCall();
            
            GUILayout.Space(pixelSize);
        }
        
        [NoCustomStyle]
        public void InfoBox(string message)
        {
            _flow.NotifyDrawCall();
            
            EditorGUILayout.HelpBox(message, MessageType.Info);
        }

        [NoCustomStyle]
        public void WarningBox(string message)
        {
            _flow.NotifyDrawCall();
            
            EditorGUILayout.HelpBox(message, MessageType.Warning);
        }
        
        [NoCustomStyle]
        public void ErrorBox(string message)
        {
            _flow.NotifyDrawCall();
            
            EditorGUILayout.HelpBox(message, MessageType.Error);
        }
        
        [SupportsCustomStyle]
        public void Button([LabelMustBeSet] Label label, Action<FlowUI> action)
        {
            _flow.NotifyDrawCall();
            
            label.ValidateHasLabelThrowException();
            action.ValidateNotNullThrowException("label : action");
            
            if (_flow.TryGetCustomStyle(out GUIStyle style))
            {
                if (GUILayout.Button(label.content, style, _flow.CurrentLayout))
                {
                    action.Invoke(this);
                }
            }
            else
            {
                if (GUILayout.Button(label.content, _flow.CurrentLayout))
                {
                    action.Invoke(this);
                }
            }
        }
        
        [SupportsCustomStyle]
        public void Button([LabelMustBeSet] Label label, Action action)
        {
            _flow.NotifyDrawCall();
            
            label.ValidateHasLabelThrowException();
            action.ValidateNotNullThrowException("label : action");

            if (_flow.TryGetCustomStyle(out GUIStyle style))
            {
                if (GUILayout.Button(label.content, style, _flow.CurrentLayout))
                {
                    action.Invoke();
                }
            }
            else
            {
                if (GUILayout.Button(label.content, _flow.CurrentLayout))
                {
                    action.Invoke();
                }
            }
        }

        [NoCustomStyle]
        public float FloatSlider([LabelCanBeEmpty] Label label, FloatMinMax minMax, float value)
        {
            FloatSlider(label, minMax, ref value);

            return value;
        }

        [NoCustomStyle]
        public void FloatSlider([LabelCanBeEmpty] Label label, FloatMinMax minMax, ref float value)
        {
            _flow.NotifyDrawCall();
            
            if (label.hasLabel)
            {
                value = EditorGUILayout.Slider(label.content, value, minMax.min, minMax.max, _flow.CurrentLayout);
            }
            else
            {
                value = EditorGUILayout.Slider(value, minMax.min, minMax.max, _flow.CurrentLayout);
            }
        }
        
        [SupportsCustomStyle]
        public void EnumSingleField<T>([LabelCanBeEmpty] Label label, ref T value) where T : Enum
        {
            _flow.NotifyDrawCall();
            
            if (label.hasLabel)
            {
                if (_flow.TryGetCustomStyle(out GUIStyle style))
                {
                    value = (T)EditorGUILayout.EnumPopup(label.content, value, style, _flow.CurrentLayout);
                }
                else
                {
                    value = (T)EditorGUILayout.EnumPopup(label.content, value, _flow.CurrentLayout);
                }
            }
            else
            {
                if (_flow.TryGetCustomStyle(out GUIStyle style))
                {
                    value = (T)EditorGUILayout.EnumPopup(value, style, _flow.CurrentLayout);
                }
                else
                {
                    value = (T)EditorGUILayout.EnumPopup(value, _flow.CurrentLayout);
                }
            }
        }
        
        [SupportsCustomStyle]
        public void EnumFlagsField<T>([LabelCanBeEmpty] Label label, ref T value) where T : Enum
        {
            _flow.NotifyDrawCall();
            
            if (label.hasLabel)
            {
                if (_flow.TryGetCustomStyle(out GUIStyle style))
                {
                    value = (T)EditorGUILayout.EnumFlagsField(label.content, value, style, _flow.CurrentLayout);
                }
                else
                {
                    value = (T)EditorGUILayout.EnumFlagsField(label.content, value, _flow.CurrentLayout);
                }
            }
            else
            {
                if (_flow.TryGetCustomStyle(out GUIStyle style))
                {
                    value = (T)EditorGUILayout.EnumFlagsField(value, style, _flow.CurrentLayout);
                }
                else
                {
                    value = (T)EditorGUILayout.EnumFlagsField(value, _flow.CurrentLayout);
                }
            }
        }

        [NoCustomStyle]
        public void TexturePreview([NotNull] Texture texture, float height)
        {
            _flow.NotifyDrawCall();
            
            texture.ValidateNotNullThrowException(nameof(texture));
            
            Rect rect = EditorGUILayout.GetControlRect(false, height);
            EditorGUI.DrawPreviewTexture(rect, texture, null, ScaleMode.ScaleToFit);
        }

        [SupportsCustomStyle]
        public void Label([LabelMustBeSet] Label label)
        {
            _flow.NotifyDrawCall();
            
            label.ValidateHasLabelThrowException();

            if (_flow.TryGetCustomStyle(out GUIStyle style))
            {
                EditorGUILayout.LabelField(label.content, style, _flow.CurrentLayout);
            }
            else
            {
                EditorGUILayout.LabelField(label.content);
            }
        }
        
        [SupportsCustomStyle]
        public void CheckBoxRight([LabelCanBeEmpty] Label label, ref bool value)
        {
            _flow.NotifyDrawCall();
            
            if (label.hasLabel)
            {
                if (_flow.TryGetCustomStyle(out GUIStyle style))
                {
                    value = EditorGUILayout.Toggle(label.content, value, style, _flow.CurrentLayout);
                }
                else
                {
                    value = EditorGUILayout.Toggle(label.content, value, _flow.CurrentLayout);
                }
            }
            else
            {
                if (_flow.TryGetCustomStyle(out GUIStyle style))
                {
                    value = EditorGUILayout.Toggle(value, style, _flow.CurrentLayout);
                }
                else
                {
                    value = EditorGUILayout.Toggle(value, _flow.CurrentLayout);
                }
            }
            
        }

        [NoCustomStyle]
        public void CheckBoxLeft([LabelMustBeSet] Label label, ref bool value)
        {
            _flow.NotifyDrawCall();
            
            label.ValidateHasLabelThrowException();

            if (_flow.TryGetCustomStyle(out GUIStyle style))
            {
                value = EditorGUILayout.ToggleLeft(label.content, value, style, _flow.CurrentLayout);
            }
            else
            {
                value = EditorGUILayout.ToggleLeft(label.content, value, _flow.CurrentLayout);
            }
        }

        public void FlexibleSpace()
        {
            _flow.NotifyDrawCall();
            
            GUILayout.FlexibleSpace();
        }
    }
}