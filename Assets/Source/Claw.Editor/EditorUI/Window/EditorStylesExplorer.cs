using System.Collections.Generic;
using System.Reflection;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor.Window
{
    public class EditorStylesExplorer : EditorWindow
    {
        private struct StyleData
        {
            public string name;
            public GUIStyle style;
        }
        
        [SerializeField] private float2 _scrollPosition;
        [SerializeField] private Color _colorPicker;
        
        private List<StyleData> _styles;

        protected override void Warmup()
        {
            PropertyInfo[] properties = typeof(EditorStyles).GetProperties(BindingFlags.Public | BindingFlags.Static);

            _styles = new List<StyleData>(properties.Length);

            foreach (PropertyInfo prop in properties)
            {
                object value = prop.GetValue(null);

                if (value is GUIStyle style)
                {
                    _styles.Add(new StyleData
                    {
                        name = prop.Name,
                        style = style
                    });
                }
            }
        }
        
        protected override void Reset()
        {
            
        }

        protected override void DrawUI()
        {
            _flow.Begin().Horizontal().Draw(ui =>
            {
                _flow.Begin().FieldWidth(0).Draw(ui => ui.ColorField(Label.NoLabel, ref _colorPicker));
                string label = "UnityEditor.EditorStyles:";
                _flow.Begin().LabelWidth(label).PassiveLabel(label);
            });

            _flow.Begin().Vertical().Scroll(ref _scrollPosition).Draw(ui =>
            {
                foreach (StyleData data in _styles)
                {
                    _flow.Begin().Style(data.style).PassiveLabel(data.name);
                }
            });
        }

        protected override void Dispose()
        {
            
        }
    }
}