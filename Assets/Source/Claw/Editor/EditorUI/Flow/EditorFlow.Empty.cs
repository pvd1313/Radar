using System;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Editor
{
    partial class EditorFlow
    {
        public class Empty : IEditorFlow
        {
            public EditorUIResult Draw(Action<EditorUI> action)
            {
                return new EditorUIResult
                {
                    changesDetected = false,
                    hasErrors = false
                };
            }
            
            public IEditorFlow Enabled(bool isEnabled) => this;
            public IEditorFlow Vertical() => this;
            public IEditorFlow Horizontal() => this;
            public IEditorFlow Indent() => this;
            public IEditorFlow Option(GUILayoutOption option) => this;
            public IEditorFlow Scroll(ref float2 scrollPosition) => this;
            public IEditorFlow Spy() => this;
            public IEditorFlow If(bool condition) => this;
        }
    }
}