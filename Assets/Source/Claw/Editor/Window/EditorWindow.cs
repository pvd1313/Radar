using Beast.Claw.Attributes;

namespace Beast.Claw.Editor.Window
{
    [FancyAsset]
    public abstract class EditorWindow : UnityEditor.EditorWindow
    {
        protected EditorUI UI { get; set; }

        protected abstract void Initialize();

        protected abstract void Dispose();
        
        protected abstract void OnInspectorGUI();

        protected EditorWindow()
        {
            UI = new EditorUI
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