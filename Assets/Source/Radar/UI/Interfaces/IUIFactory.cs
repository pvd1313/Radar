namespace Beast.Radar.UI
{
    public interface IUIFactory
    {
        CanvasView CreateCanvas<TOwner>(TOwner owner);

        void Create<TOwner, TView>(TOwner owner, string prefabID, out TView newView) where TView : UIView;
    }
}