using System;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Editor
{
    partial class Flow
    {
        public class Empty : IFlow
        {
            public GUIStyle CurrentStyle => null;

            public FlowUIResult Draw(Action<FlowUI> action)
            {
                return new FlowUIResult
                {
                    changesDetected = false,
                };
            }

            public IFlow Begin() => this;

            public IFlow Enabled(bool isEnabled) => this;
            public IFlow Vertical() => this;
            public IFlow Horizontal() => this;
            public IFlow Indent(int indent) => this;
            public IFlow LabelWidth(float labelWidth) => this;
            public IFlow Indent() => this;
            public IFlow Option(GUILayoutOption option) => this;
            public IFlow Scroll(ref float2 scrollPosition) => this;
            public IFlow Spy() => this;
            public IFlow FieldWidth(float fieldWidth) => this;
            public IFlow Style(GUIStyle style) => this;
            public IFlow If(bool condition) => this;
        }
    }
}