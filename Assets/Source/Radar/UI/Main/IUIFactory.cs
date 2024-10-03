namespace Beast.Radar.UI
{
    public interface IUIFactory
    {
        public CanvasView CreateCanvas(string canvasPrefabID);

        public BeastUIWindowView CreateWindowView(string panelPrefabID);

    }
}