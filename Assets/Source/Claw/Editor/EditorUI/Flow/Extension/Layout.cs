using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor
{
    public static partial class EditorFlowExtension
    {
        public static FlowUIResult Vertical(this IFlow flow, Action<FlowUI> action)
        {
            return flow.Vertical().Draw(action);
        }
        
        public static FlowUIResult Horizontal(this IFlow flow, Action<FlowUI> action)
        {
            return flow.Horizontal().Draw(action);
        }

        public static FlowUIResult Enabled(this IFlow flow, bool isEnabled, Action<FlowUI> action)
        {
            return flow.Enabled(isEnabled).Draw(action);
        }

        public static FlowUIResult Scroll(this IFlow flow, ref float2 position, Action<FlowUI> action)
        {
            return flow.Scroll(ref position).Draw(action);
        }

        public static FlowUIResult Tab(this IFlow flow, Action<FlowUI> action)
        {
            return flow.Indent(EditorGUI.indentLevel + 1).Draw(action);
        }
        
        public static IFlow Tab(this IFlow flow)
        {
            return flow.Indent(EditorGUI.indentLevel + 1);
        }
        
        public static IFlow LabelWidth(this IFlow flow, Label label)
        {
            label.ValidateHasLabelThrowException();

            float labelWidth;
            
            if (flow.CurrentStyle != null)
            {
                labelWidth = flow.CurrentStyle.CalcSize(label.content).x;
            }
            else
            {
                labelWidth = GUI.skin.label.CalcSize(label.content).x;
            }

            return flow.LabelWidth(labelWidth);
        }
        
        public static FlowUIResult LabelWidth(this IFlow flow, Label label, Action<FlowUI> action)
        {
            return flow.LabelWidth(label).Draw(action);
        }

        public static IFlow FieldWidth(this IFlow flow, Label label)
        {
            label.ValidateHasLabelThrowException();
            
            float labelWidth;
            
            if (flow.CurrentStyle != null)
            {
                labelWidth = flow.CurrentStyle.CalcSize(label.content).x;
            }
            else
            {
                labelWidth = GUI.skin.label.CalcSize(label.content).x;
            }

            return flow.FieldWidth(labelWidth);
        }

        public static FlowUIResult FieldWidth(this IFlow flow, Label label, Action<FlowUI> action)
        {
            return flow.FieldWidth(label).Draw(action);
        }
        
        public static FlowUIResult AddTab(this IFlow flow, int indent, Action<FlowUI> action)
        {
            return flow.Indent(EditorGUI.indentLevel + indent).Draw(action);
        }
        
        public static FlowUIResult SetTab(this IFlow flow, int newIndent, Action<FlowUI> action)
        {
            return flow.Indent(newIndent).Draw(action);
        }

        public static FlowUIResult Option(this IFlow flow, GUILayoutOption option, Action<FlowUI> action)
        {
            return flow.Option(option).Draw(action);
        }

        public static IFlow Option(this IFlow flow, IEnumerable<GUILayoutOption> options)
        {
            foreach (GUILayoutOption option in options)
            {
                flow = flow.Option(option);
            }

            return flow;
        }
        
        public static IFlow Option(this IFlow flow, GUILayoutOption a, GUILayoutOption b)
        {
            return flow.Option(a).Option(b);
        }
        
        public static IFlow Option(this IFlow flow, GUILayoutOption a, GUILayoutOption b, GUILayoutOption c)
        {
            return flow.Option(a).Option(b).Option(c);
        }
        
        public static FlowUIResult Option(this IFlow flow, IEnumerable<GUILayoutOption> options, Action<FlowUI> action)
        {
            foreach (GUILayoutOption option in options)
            {
                flow = flow.Option(option);
            }

            return flow.Draw(action);
        }

        public static FlowUIResult Spy(this IFlow flow, Action<FlowUI> action)
        {
            return flow.Spy().Draw(action);
        }

        public static FlowUIResult If(this IFlow flow, bool condition, Action<FlowUI> action)
        {
            return flow.If(condition).Draw(action);
        }
    }
}