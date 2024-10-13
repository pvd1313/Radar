using System;
using Beast.Claw.Attributes;
using UnityEngine;

namespace Beast.Claw.Editor.Window
{
    [FancyAsset]
    public abstract class EditorWindow : UnityEditor.EditorWindow
    {
        private EditorUI _ui;
        
        protected abstract void Initialize();

        protected abstract void Dispose();
        
        protected abstract void OnInspectorGUI(EditorUI ui);

        protected EditorWindow()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            Dispose();
        }

        private void OnGUI()
        {
            _ui = new EditorUI
            {
                success = true,
                niceLabelsEnabled = true,
                layoutOptions = Array.Empty<GUILayoutOption>(),
            };
            
            _ui.TryCatch(OnInspectorGUI, out bool _);
        }
    }
}