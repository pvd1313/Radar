using Beast.Claw.Attributes;
using UnityEditor;

namespace Beast.Claw.Editor.Window
{
    [FancyAsset]
    public abstract class BeastEditorWindow : EditorWindow
    {
        protected BeastEditorUI UI { get; set; }

        protected abstract void Initialize();

        protected abstract void Dispose();
        
        protected abstract void OnInspectorGUI();

        protected BeastEditorWindow()
        {
            UI = new BeastEditorUI
            {
                success = true,
                niceLabelsEnabled = true
            };
            
            Initialize();
        }

        private void OnDestroy()
        {
            Dispose();
        }

        private void OnGUI()
        {
            UI.TryCatch(OnInspectorGUI, out bool _);
        }
    }
}