using System;
using Beast.Claw.Attributes;
using Beast.Claw.Attributes.Annotation;
using Beast.Claw.Editor.UI.UI;
using Object = UnityEngine.Object;

namespace Beast.Claw.Editor.UI
{
    partial class EditorFlowExtension
    {
        public static FlowUIResult PassiveLabel(this IFlow anyFlow, string label)
        {
            if (anyFlow is Flow flow)
            {
                flow.StagedBeginDraw(out FlowUI ui, out FlowUIResult result);

                try
                {
                    ui.Label(label);
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
        
        public static FlowUIResult WarningBox(this IFlow anyFlow, string message)
        {
            if (anyFlow is Flow flow)
            {
                flow.StagedBeginDraw(out FlowUI ui, out FlowUIResult result);

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
        
        public static FlowUIResult InfoBox(this IFlow anyFlow, string message)
        {
            if (anyFlow is Flow flow)
            {
                flow.StagedBeginDraw(out FlowUI ui, out FlowUIResult result);

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

        [NoCustomStyle]
        public static FlowUIResult AssetField<TAsset>(this IFlow anyFlow, [LabelCanBeEmpty] Label label, ref TAsset asset)
            where TAsset : Object
        {
            if (anyFlow is Flow flow)
            {
                flow.StagedBeginDraw(out FlowUI ui, out FlowUIResult result);

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