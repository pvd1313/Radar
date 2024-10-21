namespace Beast.Claw.Editor.Drawers
{
    public class Float2Task : IValidatable
    {
        public Label mainLabel;
        public Label label0;
        public Label label1;
        public Style mainLabelStyle;
        public Style labelStyle0;
        public Style labelStyle1;
        public Style valueStyle0;
        public Style valueStyle1;
        public float value0;
        public float value1;

        public void SetStyle(UIStyleSet styleSet)
        {
            mainLabelStyle = styleSet.label;
            labelStyle0 = styleSet.label;
            labelStyle1 = styleSet.label;
            valueStyle0 = styleSet.field;
            valueStyle1 = styleSet.field;
        }
        
        public string Validate()
        {
            if (mainLabel.hasLabel && !mainLabelStyle.hasStyle)
            {
                return "Has main label, but has no style for it";
            }

            if (label0.hasLabel && !labelStyle0.hasStyle)
            {
                return "Has label0, but has no style for it";
            }

            if (label1.hasLabel && !labelStyle1.hasStyle)
            {
                return "Has label1, but has no style for it";
            }

            if (!valueStyle0.hasStyle)
            {
                return "valueStyle0 not set";
            }

            if (!valueStyle1.hasStyle)
            {
                return "valueStyle1 not set";
            }

            return null;
        }
    }
}