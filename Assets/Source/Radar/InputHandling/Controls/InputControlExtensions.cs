using Unity.Mathematics;

namespace Beast.Radar.InputHandling
{
    public static class InputControlExtensions
    {
        
        public static void UpdateValue(this InputValueControl<EInputOptionButton> control, EInputOptionButton value)
        {
            switch (value)
            {
                case EInputOptionButton.None when control.State is EButtonState.Default:
                default:
                    return;
                
                case EInputOptionButton.First when control.State is EButtonState.Default:
                case EInputOptionButton.Second when control.State is EButtonState.Default:
                case EInputOptionButton.Third when control.State is EButtonState.Default:
                case EInputOptionButton.Fourth when control.State is EButtonState.Default:
                case EInputOptionButton.Back when control.State is EButtonState.Default:
                case EInputOptionButton.Start when control.State is EButtonState.Default:
                    control.StartPress(value);
                    break;
                
                case EInputOptionButton.None when control.State is EButtonState.Pressed:
                    control.EndPress(control.Value);
                    control.SetValue(EInputOptionButton.None);
                    return;
                
                case EInputOptionButton.First when control.State is EButtonState.Pressed:
                case EInputOptionButton.Second when control.State is EButtonState.Pressed:
                case EInputOptionButton.Third when control.State is EButtonState.Pressed:
                case EInputOptionButton.Fourth when control.State is EButtonState.Pressed:
                case EInputOptionButton.Back when control.State is EButtonState.Pressed:
                case EInputOptionButton.Start when control.State is EButtonState.Pressed:
                    control.PerformPress(value);
                    break;
                
            }
        }
        
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