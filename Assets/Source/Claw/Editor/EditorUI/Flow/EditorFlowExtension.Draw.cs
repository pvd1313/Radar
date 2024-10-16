using System;

namespace Beast.Claw.Editor
{
    partial class EditorFlowExtension
    {
        public static EditorUIResult Button(this IEditorFlow flow, string label, Action action)
        {
            return flow.Draw(ui => ui.Button(label, action));
        }

        public static EditorUIResult Button(this IEditorFlow flow, string label, Action<EditorUI> action)
        {
            return flow.Draw(ui => ui.Button(label, action));
        }

        public static EditorUIResult Label(this IEditorFlow flow, string label)
        {
            return flow.Draw(ui => ui.Label(label));
        }

        public static EditorUIResult WarningBox(this IEditorFlow flow, string message)
        {
            return flow.Draw(ui => ui.WarningBox(message));
        }
        
        public static EditorUIResult InfoBox(this IEditorFlow flow, string message)
        {
            return flow.Draw(ui => ui.InfoBox(message));
        }
    }
}