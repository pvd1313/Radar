using System;
using Beast.Claw.Attributes.Annotation;
using Beast.Claw.Editor.UI.UI;

namespace Beast.Claw.Editor.UI
{
    [ExtensionClass]
    partial class EditorFlowExtension
    {
        public static FlowUIResult Button(this IFlow anyFlow, Label label, Action action)
        {
            label.ValidateHasLabelThrowException();
            
            if (anyFlow is Flow flow)
            {
                flow.StagedBeginDraw(out FlowUI ui, out FlowUIResult result);

                try
                {
                    ui.Button(label, action);
                }
                catch (Exception e)
                {
                    flow.StagedHandleException(ref ui, ref result, e);
                }
                finally
                {
                    flow.StagedEndDraw(ref ui, ref result);
                }

                return result;
            }

            return default;
        }

        public static FlowUIResult Button(this IFlow anyFlow, Label label, Action<FlowUI> action)
        {
            label.ValidateHasLabelThrowException();
            
            if (anyFlow is Flow flow)
            {
                flow.StagedBeginDraw(out FlowUI ui, out FlowUIResult result);

                try
                {
                    ui.Button(label, action);
                }
                catch (Exception e)
                {
                    flow.StagedHandleException(ref ui, ref result, e);
                }
                finally
                {
                    flow.StagedEndDraw(ref ui, ref result);
                }

                return result;
            }

            return default;
        }
    }
}