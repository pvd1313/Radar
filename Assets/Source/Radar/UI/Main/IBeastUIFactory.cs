namespace Beast.Radar.UI
{
    public interface IBeastUIFactory
    {
        public BeastCanvasView NewCanvas(string canvasPrefabID);

        public BeastUIWindowView NewWindow(string panelPrefabID);

    }
}