using System;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Editor
{
    public static partial class EditorFlowExtension
    {
        public static EditorUIResult Vertical(this IEditorFlow flow, Action<EditorUI> action)
        {
            return flow.Vertical().Draw(action);
        }
        
        public static EditorUIResult Horizontal(this IEditorFlow flow, Action<EditorUI> action)
        {
            return flow.Horizontal().Draw(action);
        }

        public static EditorUIResult Enabled(this IEditorFlow flow, bool isEnabled, Action<EditorUI> action)
        {
            return flow.Enabled(isEnabled).Draw(action);
        }

        public static EditorUIResult Scroll(this IEditorFlow flow, ref float2 position, Action<EditorUI> action)
        {
            return flow.Scroll(ref position).Draw(action);
        }

        public static EditorUIResult Indent(this IEditorFlow flow, Action<EditorUI> action)
        {
            return flow.Indent().Draw(action);
        }

        public static EditorUIResult Option(this IEditorFlow flow, GUILayoutOption option, Action<EditorUI> action)
        {
            return flow.Option(option).Draw(action);
        }

        public static EditorUIResult Spy(this IEditorFlow flow, Action<EditorUI> action)
        {
            return flow.Spy().Draw(action);
        }

        public static EditorUIResult If(this IEditorFlow flow, bool condition, Action<EditorUI> action)
        {
            return flow.If(condition).Draw(action);
        }
    }
}