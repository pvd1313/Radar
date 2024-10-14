using System;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Editor
{
    public static class EditorFlowExtension
    {
        public static EditorUIResult Vertical(this EditorFlow flow, Action<EditorUI> action)
        {
            return flow.Vertical().Draw(action);
        }
        
        public static EditorUIResult Horizontal(this EditorFlow flow, Action<EditorUI> action)
        {
            return flow.Horizontal().Draw(action);
        }

        public static EditorUIResult Enabled(this EditorFlow flow, bool isEnabled, Action<EditorUI> action)
        {
            return flow.Enabled(isEnabled).Draw(action);
        }

        public static EditorUIResult Scroll(this EditorFlow flow, ref float2 position, Action<EditorUI> action)
        {
            return flow.Scroll(ref position).Draw(action);
        }

        public static EditorUIResult Indent(this EditorFlow flow, Action<EditorUI> action)
        {
            return flow.Indent().Draw(action);
        }

        public static EditorUIResult Option(this EditorFlow flow, GUILayoutOption option, Action<EditorUI> action)
        {
            return flow.Option(option).Draw(action);
        }

        public static EditorUIResult Spy(this EditorFlow flow, Action<EditorUI> action)
        {
            return flow.Spy().Draw(action);
        }

        public static EditorUIResult Button(this EditorFlow flow, string label, Action action)
        {
            return flow.Draw(ui => ui.Button(label, action));
        }
        
        public static EditorUIResult Button(this EditorFlow flow, string label, Action<EditorUI> action)
        {
            return flow.Draw(ui => ui.Button(label, action));
        }
    }
}