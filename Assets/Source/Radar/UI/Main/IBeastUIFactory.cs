namespace Beast.Radar.UI
{
    public interface IBeastUIFactory
    {
        BeastCanvasView CreateCanvas<TOwner>(TOwner owner);

        void Create<TOwner, TView>(TOwner owner, string prefabID, out TView newView) where TView : BeastUIView;
    }
}