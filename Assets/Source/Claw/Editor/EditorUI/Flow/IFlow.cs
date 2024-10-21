using System;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Editor
{
    public interface IFlow
    {
        FlowUIResult Draw(Action<FlowUI> action);
        IFlow Enabled(bool isEnabled);
        IFlow Vertical();
        IFlow Horizontal();
        IFlow Indent(int indent);
        IFlow LabelWidth(float labelWidth);
        IFlow Option(GUILayoutOption option);
        IFlow Scroll(ref float2 scrollPosition);
        IFlow Spy();
        IFlow Style(GUIStyle style);
        IFlow If(bool condition);
        GUIStyle CurrentStyle { get; }
        IFlow FieldWidth(float fieldWidth);
    }
}