using Beast.Radar.UI.Panels.Base;

namespace Beast.Radar.UI.Factory
{
    public interface IUIFactory
    {
        public T CreatePanel<T>() where T : BView;
    }
}