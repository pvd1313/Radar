using Noesis;

namespace Beast.Radar.UI.Xaml
{
    /// <summary>
    /// Interaction logic for LoadingScreen.xaml
    /// </summary>
    public class LoadingScreenView : UserControl
    {
        public LoadingScreenView()
        {
            NoesisUnity.LoadComponent(this);
        }
    }
}
