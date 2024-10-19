using System;
using Object = UnityEngine.Object;

namespace Beast.Claw.Editor
{
    partial class EditorFlowExtension
    {
        public static EditorUIResult PassiveLabel(this IEditorFlow anyFlow, string label)
        {
            if (anyFlow is EditorFlow flow)
            {
                flow.StagedBeginDraw(out EditorUI ui, out EditorUIResult result);

                try
                {
                    ui.PassiveLabel(label);
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
        
        public static EditorUIResult WarningBox(this IEditorFlow anyFlow, string message)
        {
            if (anyFlow is EditorFlow flow)
            {
                flow.StagedBeginDraw(out EditorUI ui, out EditorUIResult result);

                try
                {
                    ui.WarningBox(message);
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
        
        public static EditorUIResult InfoBox(this IEditorFlow anyFlow, string message)
        {
            if (anyFlow is EditorFlow flow)
            {
                flow.StagedBeginDraw(out EditorUI ui, out EditorUIResult result);

                try
                {
                    ui.InfoBox(message);
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

        public static EditorUIResult AssetField<TAsset>(this IEditorFlow anyFlow, string label, ref TAsset asset)
            where TAsset : Object
        {
            if (anyFlow is EditorFlow flow)
            {
                flow.StagedBeginDraw(out EditorUI ui, out EditorUIResult result);

                try
                {
                    ui.AssetField(label, ref asset);
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