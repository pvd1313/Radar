using System;

namespace Beast.Claw.Editor
{
    partial class EditorFlowExtension
    {
        public static EditorUIResult Button(this IEditorFlow anyFlow, SmartLabel label, Action action)
        {
            label.ValidateHasLabelThrowException();
            
            if (anyFlow is EditorFlow flow)
            {
                flow.StagedBeginDraw(out EditorUI ui, out EditorUIResult result);

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

        public static EditorUIResult Button(this IEditorFlow anyFlow, SmartLabel label, Action<EditorUI> action)
        {
            label.ValidateHasLabelThrowException();
            
            if (anyFlow is EditorFlow flow)
            {
                flow.StagedBeginDraw(out EditorUI ui, out EditorUIResult result);

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