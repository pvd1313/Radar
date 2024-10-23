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
        public void Float2FieldTwoLine([LabelMustBeSet] Label label, ref float2 value)
        {
            _flow.NotifyDrawCall();
            
            label.ValidateHasLabelThrowException();

            value = EditorGUILayout.Vector2Field(label.content, value, _flow.CurrentLayout);
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
        public void WindowField<TWindow>([LabelCanBeEmpty] Label label, WindowContainer<TWindow> container) 
            where TWindow : class
        {
            container.ValidateNotNullThrowException(nameof(container));
            
            _flow.NotifyDrawCall();

            // TODO : Optimize active window search (Resources.FindObjectsOfTypeAll)
            //      : https://risenbeast.youtrack.cloud/issue/R-70
            EditorWindow[] editorWindows = Resources.FindObjectsOfTypeAll<EditorWindow>();

            StackIndexMap<TWindow> windowMap = new (stackalloc short[_flow.TempStringArray.Capacity]);
            
            int selectedIndex = windowMap.FindAllWithTypeAndCurrent(editorWindows, container.Window);
            if (selectedIndex == -1)
            {
                selectedIndex = 0;
            }

            GUIContent[] windowNames = _flow.TempContentArray[windowMap.Count + 1];
            windowNames[0] = _flow.TextNone;
            for (int i = 0; i < windowMap.Count; i++)
            {
                windowNames[i + 1] = (windowMap[i] as EditorWindow)?.titleContent ?? GUIContent.none;
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
                if (selectedIndexNew > 0)
                {
                    container.SetReference(windowMap[selectedIndexNew - 1] as Object);
                }
                else
                {
                    container.SetReference(null);
                }
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
    }
}