namespace Beast.Radar.UI
{
    public interface IBeastUIFactory
    {
        BeastCanvasView NewCanvas<TOwner>(TOwner owner);

        TView NewWindow<TOwner, TView>(TOwner owner, string prefabID) where TView : BeastUIView;
    }
}