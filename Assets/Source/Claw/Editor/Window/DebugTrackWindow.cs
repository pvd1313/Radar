using Beast.Claw.Common;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor.Window
{
    public class DebugTrackWindow : EditorWindow
    {
        [SerializeField] private bool _clearOnEnterPlayerMode;

        private bool _isPlaying;
        
        protected override void Initialize()
        {
            
        }

        protected override void Dispose()
        {
            
        }

        protected override void OnInspectorGUI()
        {
            if (_isPlaying != Application.isPlaying)
            {
                _isPlaying = Application.isPlaying;

                if (_isPlaying && _clearOnEnterPlayerMode)
                {
                    Clear();
                }
            }
            
            EditorGUIUtility.labelWidth = 200;

            new EditorUI
            {
                success = true,
                niceLabelsEnabled = true
            }
                .Horizontal(() =>
            {
                UI.Button("Clear", Clear);
                _clearOnEnterPlayerMode = UI.CheckBox(nameof(_clearOnEnterPlayerMode), _clearOnEnterPlayerMode);
            });
            
            EditorGUIUtility.labelWidth = 500;
            
            EditorUI ui = new ()
            {
                success = true,
                niceLabelsEnabled = false
            };
            
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