namespace Beast.Radar.UI
{
    public class CityView : UIView, IUIPoolable
    {
        public UIImage _image;
        public void Enable()
        {
            _image.gameObject.SetActive(true);
        }

        public void Disable()
        {
            _image.gameObject.SetActive(false);
        }
    }
}