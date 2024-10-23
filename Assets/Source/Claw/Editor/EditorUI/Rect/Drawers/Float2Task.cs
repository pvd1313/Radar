namespace Beast.Claw.Editor.Drawers
{
    public class Float2Task : IRectDrawTask
    {
        public Label mainLabel;
        public Label label0;
        public Label label1;
        public Label label2;
        public Style mainLabelStyle;
        public Style labelStyle0;
        public Style labelStyle1;
        public Style labelStyle2;
        public Style valueStyle0;
        public Style valueStyle1;
        public float value0;
        public float value1;
        public float fieldLabelSpacing;

        public void SetStyle(UIStyleGroup style)
        {
            mainLabelStyle = style.Label;
            labelStyle0 = style.Label;
            labelStyle1 = style.Label;
            labelStyle2 = style.Label;
            valueStyle0 = style.NumberField;
            valueStyle1 = style.NumberField;
            fieldLabelSpacing = style.MinorSpacing;

            mainLabel = Label.NoLabel;
            label0 = Label.NoLabel;
            label1 = Label.NoLabel;
            label2 = Label.NoLabel;
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