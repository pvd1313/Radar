using System;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Editor
{
    partial class EditorFlow
    {
        public class Empty : IEditorFlow
        {
            public GUIStyle CurrentStyle => null;

            public EditorUIResult Draw(Action<EditorUI> action)
            {
                return new EditorUIResult
                {
                    changesDetected = false,
                };
            }
            
            public IEditorFlow Enabled(bool isEnabled) => this;
            public IEditorFlow Vertical() => this;
            public IEditorFlow Horizontal() => this;
            public IEditorFlow Indent(int indent) => this;
            public IEditorFlow LabelWidth(float labelWidth) => this;
            public IEditorFlow Indent() => this;
            public IEditorFlow Option(GUILayoutOption option) => this;
            public IEditorFlow Scroll(ref float2 scrollPosition) => this;
            public IEditorFlow Spy() => this;
            public IEditorFlow FieldWidth(float fieldWidth) => this;
            public IEditorFlow Style(GUIStyle style) => this;
            public IEditorFlow If(bool condition) => this;
        }
    }
}