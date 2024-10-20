using Beast.Claw.Attributes;
using Beast.Claw.Attributes.Annotation;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;

namespace Beast.Claw.Editor
{
    readonly partial struct EditorUI
    {
        [NoCustomStyle]
        public Color ColorField([CanBeEmpty] SmartLabel label, Color value)
        {
            ColorField(label, ref value);

            return value;
        }
        
        [NoCustomStyle]
        public void ColorField([CanBeEmpty] SmartLabel label, ref Color value)
        {
            _flow.NotifyDrawCall();
            
            if (label.hasLabel)
            {
                value = EditorGUILayout.ColorField(label.content, value, _flow.CurrentLayout);
            }
            else
            {
                value = EditorGUILayout.ColorField(value, _flow.CurrentLayout);
            }
        }

        [CustomStyleSupported]
        public float FloatField([CanBeEmpty] SmartLabel label, float value)
        {
            FloatField(label, ref value);
            
            return value;
        }

        [CustomStyleSupported]
        public void FloatField([CanBeEmpty] SmartLabel label, ref float value)
        {
            _flow.NotifyDrawCall();
            
            if (label.hasLabel)
            {
                if (_flow.TryGetCustomStyle(out GUIStyle style))
                {
                    value = EditorGUILayout.FloatField(label.content, value, style, _flow.CurrentLayout);
                }
                else
                {
                    value = EditorGUILayout.FloatField(label.content, value, _flow.CurrentLayout);
                }
            }
            else
            {
                if (_flow.TryGetCustomStyle(out GUIStyle style))
                {
                    value = EditorGUILayout.FloatField(value, style, _flow.CurrentLayout);
                }
                else
                {
                    value = EditorGUILayout.FloatField(value, _flow.CurrentLayout);
                }
            }
        }

        [NoCustomStyle]
        public float2 Float2FieldTwoLine([MustHaveLabel] SmartLabel label, float2 value)
        {
            Float2FieldTwoLine(label, ref value);

            return value;
        }

        [NoCustomStyle]
        public void Float2FieldTwoLine([MustHaveLabel] SmartLabel label, ref float2 value)
        {
            _flow.NotifyDrawCall();
            
            label.ValidateHasLabelThrowException();

            value = EditorGUILayout.Vector2Field(label.content, value, _flow.CurrentLayout);
        }

        [NoCustomStyle]
        public float2 Float2FieldOneLine([CanBeEmpty] SmartLabel label, float2 value)
        {
            Float2FieldOneLine(label, ref value);

            return value;
        }

        [NoCustomStyle]
        public void Float2FieldOneLine([CanBeEmpty] SmartLabel label, ref float2 value)
        {
            _flow.NotifyDrawCall();
            
            Rect rect = EditorGUILayout.GetControlRect(false);

            if (label.hasLabel)
            {
                rect = EditorGUI.PrefixLabel(rect, GUIUtility.GetControlID(FocusType.Passive), label.content);
            }

            const float LABEL_WIDTH = 15;
            float fieldWidth = (rect.width - LABEL_WIDTH * 2) / 2;

            (Rect w0, Rect w1, Rect w2, Rect w3) = RectSlice.Horizontal(
                rect, LABEL_WIDTH, fieldWidth, LABEL_WIDTH, fieldWidth);
            
            int indentLevelBak = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            
            EditorGUI.BeginDisabledGroup(true);
            GUIContent tempX = EditorGUIUtility.TrTempContent(" x");
            EditorGUI.PrefixLabel(w0, GUIUtility.GetControlID(FocusType.Passive), tempX);
            
            GUIContent tempY = EditorGUIUtility.TrTempContent(" y");
            EditorGUI.PrefixLabel(w2, GUIUtility.GetControlID(FocusType.Passive), tempY);
            EditorGUI.EndDisabledGroup();
            
            value.x = EditorGUI.FloatField(w1, value.x);
            value.y = EditorGUI.FloatField(w3, value.y);

            EditorGUI.indentLevel = indentLevelBak;
        }
        
        [NoCustomStyle]
        public float3 Float3FieldTwoLine([MustHaveLabel] SmartLabel label, float3 value)
        {
            Float3FieldTwoLine(label, ref value);

            return value;
        }

        [NoCustomStyle]
        public void Float3FieldTwoLine([MustHaveLabel] SmartLabel label, ref float3 value)
        {
            _flow.NotifyDrawCall();
            
            label.ValidateHasLabelThrowException();

            value = EditorGUILayout.Vector3Field(label.content, value, _flow.CurrentLayout);
        }
        
        [NoCustomStyle]
        public float4 Float4FieldTwoLine([MustHaveLabel] SmartLabel label, float4 value)
        {
            Float4FieldTwoLine(label, ref value);

            return value;
        }

        [NoCustomStyle]
        public void Float4FieldTwoLine([MustHaveLabel] SmartLabel label, ref float4 value)
        {
            _flow.NotifyDrawCall();
            
            label.ValidateHasLabelThrowException();
            
            value = EditorGUILayout.Vector4Field(label.content, value, _flow.CurrentLayout);
        }

        [CustomStyleSupported]
        public string StringField([CanBeEmpty] SmartLabel label, string value)
        {
            StringField(label, ref value);

            return value;
        }

        [CustomStyleSupported]
        public void StringField([CanBeEmpty] SmartLabel label, ref string value)
        {
            _flow.NotifyDrawCall();
            
            if (label.hasLabel)
            {
                if (_flow.TryGetCustomStyle(out GUIStyle style))
                {
                    value = EditorGUILayout.TextField(label.content, value, style, _flow.CurrentLayout);
                }
                else
                {
                    value = EditorGUILayout.TextField(label.content, value, _flow.CurrentLayout);
                }
            }
            else
            {
                if (_flow.TryGetCustomStyle(out GUIStyle style))
                {
                    value = EditorGUILayout.TextField(value, style, _flow.CurrentLayout);
                }
                else
                {
                    value = EditorGUILayout.TextField(value, _flow.CurrentLayout);
                }
            }
        }
        
        [NoCustomStyle]
        public TObject AssetField<TObject>([CanBeEmpty] SmartLabel label, TObject obj) where TObject : Object
        {
            _flow.NotifyDrawCall();
            
            AssetField(label, ref obj);

            return obj;
        }
        
        [NoCustomStyle]
        public void AssetField<TObject>([CanBeEmpty] SmartLabel label, ref TObject obj) where TObject : Object
        {
            _flow.NotifyDrawCall();
            
            if (label.hasLabel)
            {
                obj = (TObject)EditorGUILayout.ObjectField(label.content, obj, typeof(TObject), false, _flow.CurrentLayout);
            }
            else
            {
                obj = (TObject)EditorGUILayout.ObjectField(obj, typeof(TObject), false, _flow.CurrentLayout);
            }
        }

        [CustomStyleSupported]
        public int IntField([CanBeEmpty] SmartLabel label, int value)
        {
            IntField(label, ref value);
            
            return value;
        }

        [CustomStyleSupported]
        public void IntField([CanBeEmpty] SmartLabel label, ref int value)
        {
            _flow.NotifyDrawCall();
            
            if (label.hasLabel)
            {
                if (_flow.TryGetCustomStyle(out GUIStyle style))
                {
                    value = EditorGUILayout.IntField(label.content, value, style, _flow.CurrentLayout);
                }
                else
                {
                    value = EditorGUILayout.IntField(label.content, value, _flow.CurrentLayout);
                }
            }
            else
            {
                if (_flow.TryGetCustomStyle(out GUIStyle style))
                {
                    value = EditorGUILayout.IntField(value, style, _flow.CurrentLayout);
                }
                else
                {
                    value = EditorGUILayout.IntField(value, _flow.CurrentLayout);
                }
            }
        }

        [NoCustomStyle]
        public int2 Int2FieldTwoLine([MustHaveLabel] SmartLabel label, int2 value)
        {
            _flow.NotifyDrawCall();
            
            Int2FieldTwoLine(label, ref value);

            return value;
        }
        
        [NoCustomStyle]
        public void Int2FieldTwoLine([MustHaveLabel] SmartLabel label, ref int2 value)
        {
            _flow.NotifyDrawCall();
            
            label.ValidateHasLabelThrowException();

            Vector2 vector = new (value.x, value.y);

            vector = EditorGUILayout.Vector2Field(label.content, vector);

            value = new int2((int)vector.x, (int)vector.y);
        }
    }
}