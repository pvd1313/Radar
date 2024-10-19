using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
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

        public static EditorUIResult Tab(this IEditorFlow flow, Action<EditorUI> action)
        {
            return flow.Indent(EditorGUI.indentLevel + 1).Draw(action);
        }
        
        public static IEditorFlow Tab(this IEditorFlow flow)
        {
            return flow.Indent(EditorGUI.indentLevel + 1);
        }
        
        public static IEditorFlow LabelWidth(this IEditorFlow flow, SmartLabel label)
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
        
        public static EditorUIResult LabelWidth(this IEditorFlow flow, SmartLabel label, Action<EditorUI> action)
        {
            return flow.LabelWidth(label).Draw(action);
        }

        public static IEditorFlow FieldWidth(this IEditorFlow flow, SmartLabel label)
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

        public static EditorUIResult FieldWidth(this IEditorFlow flow, SmartLabel label, Action<EditorUI> action)
        {
            return flow.FieldWidth(label).Draw(action);
        }
        
        public static EditorUIResult AddTab(this IEditorFlow flow, int indent, Action<EditorUI> action)
        {
            return flow.Indent(EditorGUI.indentLevel + indent).Draw(action);
        }
        
        public static EditorUIResult SetTab(this IEditorFlow flow, int newIndent, Action<EditorUI> action)
        {
            return flow.Indent(newIndent).Draw(action);
        }

        public static EditorUIResult Option(this IEditorFlow flow, GUILayoutOption option, Action<EditorUI> action)
        {
            return flow.Option(option).Draw(action);
        }

        public static IEditorFlow Option(this IEditorFlow flow, IEnumerable<GUILayoutOption> options)
        {
            foreach (GUILayoutOption option in options)
            {
                flow = flow.Option(option);
            }

            return flow;
        }
        
        public static IEditorFlow Option(this IEditorFlow flow, GUILayoutOption a, GUILayoutOption b)
        {
            return flow.Option(a).Option(b);
        }
        
        public static IEditorFlow Option(this IEditorFlow flow, GUILayoutOption a, GUILayoutOption b, GUILayoutOption c)
        {
            return flow.Option(a).Option(b).Option(c);
        }
        
        public static EditorUIResult Option(this IEditorFlow flow, IEnumerable<GUILayoutOption> options, Action<EditorUI> action)
        {
            foreach (GUILayoutOption option in options)
            {
                flow = flow.Option(option);
            }

            return flow.Draw(action);
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