using Beast.Claw.Attributes;
using Beast.Claw.Attributes.Annotation;
using Beast.Claw.Collection;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Beast.Claw.Editor
{
    readonly partial struct FlowUI
    {
        [NoCustomStyle]
        public Color ColorField([LabelCanBeEmpty] Label label, Color value)
        {
            ColorField(label, ref value);

            return value;
        }
        
        [NoCustomStyle]
        public void ColorField([LabelCanBeEmpty] Label label, ref Color value)
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

        [SupportsCustomStyle]
        public float FloatField([LabelCanBeEmpty] Label label, float value)
        {
            FloatField(label, ref value);
            
            return value;
        }

        [SupportsCustomStyle]
        public void FloatField([LabelCanBeEmpty] Label label, ref float value)
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
        public float2 Float2FieldTwoLine([LabelMustBeSet] Label label, float2 value)
        {
            Float2FieldTwoLine(label, ref value);

            return value;
        }

        [NoCustomStyle]
        public void Float2FieldTwoLine([LabelMustBeSet] Label label, ref float2 value)
        {
            _flow.NotifyDrawCall();
            
            label.ValidateHasLabelThrowException();

            value = EditorGUILayout.Vector2Field(label.content, value, _flow.CurrentLayout);
        }

        [NoCustomStyle]
        public float2 Float2FieldOneLine([LabelCanBeEmpty] Label label, float2 value)
        {
            Float2FieldOneLine(label, ref value);

            return value;
        }

        [NoCustomStyle]
        public void Float2FieldOneLine([LabelCanBeEmpty] Label label, ref float2 value)
        {
            _flow.NotifyDrawCall();
            
            Rect rect = EditorGUILayout.GetControlRect(false);

            if (label.hasLabel)
            {
                rect = EditorGUI.PrefixLabel(rect, GUIUtility.GetControlID(FocusType.Passive), label.content);
            }

            const float LABEL_WIDTH = 15;
            float fieldWidth = (rect.width - LABEL_WIDTH * 2) / 2;

            Rect4 columns = rect.SliceToColumns4(LABEL_WIDTH, fieldWidth, LABEL_WIDTH, fieldWidth);
            
            int indentLevelBak = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            
            EditorGUI.BeginDisabledGroup(true);
            GUIContent tempX = EditorGUIUtility.TrTempContent(" x");
            EditorGUI.PrefixLabel(columns.rect0, GUIUtility.GetControlID(FocusType.Passive), tempX);
            
            GUIContent tempY = EditorGUIUtility.TrTempContent(" y");
            EditorGUI.PrefixLabel(columns.rect2, GUIUtility.GetControlID(FocusType.Passive), tempY);
            EditorGUI.EndDisabledGroup();
            
            value.x = EditorGUI.FloatField(columns.rect1, value.x);
            value.y = EditorGUI.FloatField(columns.rect3, value.y);

            EditorGUI.indentLevel = indentLevelBak;
        }
        
        [NoCustomStyle]
        public float3 Float3FieldTwoLine([LabelMustBeSet] Label label, float3 value)
        {
            Float3FieldTwoLine(label, ref value);

            return value;
        }

        [NoCustomStyle]
        public void Float3FieldTwoLine([LabelMustBeSet] Label label, ref float3 value)
        {
            _flow.NotifyDrawCall();
            
            label.ValidateHasLabelThrowException();

            value = EditorGUILayout.Vector3Field(label.content, value, _flow.CurrentLayout);
        }
        
        [NoCustomStyle]
        public float4 Float4FieldTwoLine([LabelMustBeSet] Label label, float4 value)
        {
            Float4FieldTwoLine(label, ref value);

            return value;
        }

        [NoCustomStyle]
        public void Float4FieldTwoLine([LabelMustBeSet] Label label, ref float4 value)
        {
            _flow.NotifyDrawCall();
            
            label.ValidateHasLabelThrowException();
            
            value = EditorGUILayout.Vector4Field(label.content, value, _flow.CurrentLayout);
        }

        [SupportsCustomStyle]
        public string StringField([LabelCanBeEmpty] Label label, string value)
        {
            StringField(label, ref value);

            return value;
        }

        [SupportsCustomStyle]
        public void StringField([LabelCanBeEmpty] Label label, ref string value)
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
        public TObject AssetField<TObject>([LabelCanBeEmpty] Label label, TObject obj) where TObject : Object
        {
            _flow.NotifyDrawCall();
            
            AssetField(label, ref obj);

            return obj;
        }
        
        [NoCustomStyle]
        public void AssetField<TObject>([LabelCanBeEmpty] Label label, ref TObject obj) where TObject : Object
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

        [SupportsCustomStyle]
        public void WindowField<TWindow>([LabelCanBeEmpty] Label label, ref TWindow window) 
            where TWindow : class
        {
            _flow.NotifyDrawCall();

            EditorWindow[] editorWindows = Resources.FindObjectsOfTypeAll<EditorWindow>();

            StackIndexMap<TWindow> windowMap = new (stackalloc short[_flow.TempStringArray.Capacity]);
            
            int selectedIndex = windowMap.FindAllWithTypeAndCurrent(editorWindows, window);
            if (selectedIndex == -1)
            {
                selectedIndex = 0;
            }

            GUIContent[] windowNames = _flow.TempContentArray[windowMap.Count + 1];
            windowNames[0] = _flow.TextNone;
            for (int i = 0; i < windowMap.Count; i++)
            {
                windowNames[i + 1] = (windowMap[i] as EditorWindow)?.titleContent;
            }

            int selectedIndexNew;
            
            if (label.hasLabel)
            {
                if (_flow.TryGetCustomStyle(out GUIStyle style))
                {
                    selectedIndexNew = EditorGUILayout.Popup(
                        label.content, 
                        selectedIndex, 
                        windowNames, 
                        style, 
                        _flow.CurrentLayout);
                }
                else
                {
                    selectedIndexNew = EditorGUILayout.Popup(label.content,
                        selectedIndex,
                        windowNames,
                        _flow.CurrentLayout);
                }
            }
            else
            {
                if (_flow.TryGetCustomStyle(out GUIStyle style))
                {
                    selectedIndexNew = EditorGUILayout.Popup(
                        selectedIndex, 
                        windowNames, 
                        _flow.CurrentLayout);
                }
                else
                {
                    selectedIndexNew = EditorGUILayout.Popup(
                        selectedIndex, 
                        windowNames, 
                        style,
                        _flow.CurrentLayout);
                }
            }

            if (selectedIndexNew != selectedIndex)
            {
                window = selectedIndexNew > 0 ? windowMap[selectedIndexNew - 1] : null;
            }
        }

        [SupportsCustomStyle]
        public int IntField([LabelCanBeEmpty] Label label, int value)
        {
            IntField(label, ref value);
            
            return value;
        }

        [SupportsCustomStyle]
        public void IntField([LabelCanBeEmpty] Label label, ref int value)
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
        public int2 Int2FieldTwoLine([LabelMustBeSet] Label label, int2 value)
        {
            _flow.NotifyDrawCall();
            
            Int2FieldTwoLine(label, ref value);

            return value;
        }
        
        [NoCustomStyle]
        public void Int2FieldTwoLine([LabelMustBeSet] Label label, ref int2 value)
        {
            _flow.NotifyDrawCall();
            
            label.ValidateHasLabelThrowException();

            Vector2 vector = new (value.x, value.y);

            vector = EditorGUILayout.Vector2Field(label.content, vector);

            value = new int2((int)vector.x, (int)vector.y);
        }
    }
}