using Beast.Claw.Common;
using Unity.Mathematics;
using UnityEditor;

namespace Beast.Claw.Editor.Window
{
    public class DebugTrackWindow : EditorWindow
    {
        protected override void Initialize()
        {
            
        }

        protected override void Dispose()
        {
            
        }

        protected override void OnInspectorGUI()
        {
            EditorGUIUtility.labelWidth = 400;

            EditorUI ui = new ()
            {
                success = true,
                niceLabelsEnabled = false
            };
            
            ui.Button("Clear", Clear);
            
            foreach ((string id, float value) in BDebug.StateStorage.ReadAllFloat())
            {
                ui.FloatField(id, value);
            }

            foreach ((string id, int value) in BDebug.StateStorage.ReadAllInt())
            {
                ui.IntField(id, value);
            }

            foreach ((string id, float2 value) in BDebug.StateStorage.ReadAllFloat2())
            {
                ui.Float2Field(id, value);
            }
            
            foreach ((string id, float3 value) in BDebug.StateStorage.ReadAllFloat3())
            {
                ui.Float3Field(id, value);
            }
            
            foreach ((string id, float4 value) in BDebug.StateStorage.ReadAllFloat4())
            {
                ui.Float4Field(id, value);
            }
            
            foreach ((string id, string value) in BDebug.StateStorage.ReadAllString())
            {
                ui.StringField(id, value);
            }
            
            Repaint();
        }

        private void Clear()
        {
            BDebug.StateStorage.Clear();
        }
    }
}