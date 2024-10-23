using Unity.Mathematics;

namespace Beast.Radar.InputControl
{
    public static class InputControlExtensions
    {
        public static void UpdateValue(this InputValueControl<int> control, int touchCount)
        {
            switch (math.abs(touchCount))
            {
                case 0 when control.State == EButtonState.Default:
                    return;
                
                case 0 when control.State == EButtonState.Pressed:
                    control.EndPress(touchCount);
                    break;
                
                case > 0 when control.State == EButtonState.Default:
                    control.StartPress(touchCount);
                    break;
                
                case > 0 when control.State == EButtonState.Pressed:
                    control.PerformPress(touchCount);
                    break;
            }
        }

        public static void UpdateValue(this InputValueControl<float> control, float value)
        {
            switch (math.abs(value))
            {
                case 0 when control.State == EButtonState.Default:
                    return;
                
                case 0 when control.State == EButtonState.Pressed:
                    control.EndPress(value);
                    break;
              
                case > 0 when control.State == EButtonState.Default:
                    control.StartPress(value);
                    break;
                
                case > 0 when control.State == EButtonState.Pressed:
                    control.PerformPress(value);
                    break;
            }
        }
        
        public static void UpdateValue(this InputValueControl<float2> control, int touchCount, float2 value)
        {
            switch (touchCount)
            {
                case 0 when control.State == EButtonState.Default:
                    return;

                case 0 when control.State == EButtonState.Pressed:
                    control.EndPress(value);
                    control.SetValue(float2.zero);
                    break;

                case 1 when control.State == EButtonState.Default:
                    control.StartPress(value);
                    break;

                case 1 when control.State == EButtonState.Pressed:
                    control.PerformPress(value);
                    break;
            }
        }

        public static void UpdateValue(this InputValueControl<float2> control, float2 value)
        {
            bool isPress = !value.Equals(float2.zero);
            switch (isPress)
            {
                case false when control.State == EButtonState.Default:
                    return;

                case false when control.State == EButtonState.Pressed:
                    control.EndPress(value);
                    control.SetValue(float2.zero);
                    break;

                case true when control.State == EButtonState.Default:
                    control.StartPress(value);
                    break;

                case true when control.State == EButtonState.Pressed:
                    control.PerformPress(value);
                    break;
            }
        }
    }
}