using System;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Editor
{
    public interface IEditorFlow
    {
        EditorUIResult Draw(Action<EditorUI> action);
        IEditorFlow Enabled(bool isEnabled);
        IEditorFlow Vertical();
        IEditorFlow Horizontal();
        IEditorFlow Indent();
        IEditorFlow Option(GUILayoutOption option);
        IEditorFlow Scroll(ref float2 scrollPosition);
        IEditorFlow Spy();

        IEditorFlow If(bool condition);
    }
}