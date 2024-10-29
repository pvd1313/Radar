namespace Beast.Radar.UI
{
    public interface IUIFactory
    {
        void Create<TOwner, TView>(TOwner owner, string prefabID, out TView newView) where TView : UIView;
    }
}