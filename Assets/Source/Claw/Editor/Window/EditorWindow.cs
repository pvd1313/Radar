using Beast.Claw.Attributes;
using UnityEngine;

namespace Beast.Claw.Editor.Window
{
    [FancyAsset]
    public abstract class EditorWindow : UnityEditor.EditorWindow
    {
        public EditorFlow flow => _flow.Begin();

        private EditorFlow _flow;

        protected abstract void InspectorUI();
        
        protected abstract void Dispose();
        
        private void OnGUI()
        {
            _flow ??= new EditorFlow();

            try
            {
                InspectorUI();
                
                _flow.CheckFlowIsFinished();
            }
            catch (ExitGUIException)
            {
                _flow.TerminateFlow();
            }
        }

        private void OnDestroy()
        {
            Dispose();
        }
    }
}