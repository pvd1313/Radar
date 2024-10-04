using Beast.Claw.Common;
using Unity.Mathematics;
using UnityEditor;

namespace Beast.Claw.Editor.Window
{
    public class BeastLogWindow : BeastEditorWindow
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

            BeastEditorUI ui = new ()
            {
                success = true,
                niceLabelsEnabled = false
            };
            
            foreach ((string id, float value) in BLog.StateStorage.ReadAllFloat())
            {
                ui.FloatField(id, value);
            }

            foreach ((string id, int value) in BLog.StateStorage.ReadAllInt())
            {
                ui.IntField(id, value);
            }

            foreach ((string id, float2 value) in BLog.StateStorage.ReadAllFloat2())
            {
                ui.Float2Field(id, value);
            }
            
            foreach ((string id, float3 value) in BLog.StateStorage.ReadAllFloat3())
            {
                ui.Float3Field(id, value);
            }
            
            foreach ((string id, float4 value) in BLog.StateStorage.ReadAllFloat4())
            {
                ui.Float4Field(id, value);
            }
            
            foreach ((string id, string value) in BLog.StateStorage.ReadAllString())
            {
                ui.StringField(id, value);
            }
            
            Repaint();
        }
    }
}