using Cysharp.Threading.Tasks;

namespace Beast.Radar.UI
{
    public interface IUIFactory
    {
        public UICanvasView CreateCanvas(string canvasID);

        public UIPanelView CreatePanel(string panelID, string canvasID, bool isLastSabling);

        public UIOverlayView CreateOverlay(string overlayID, string canvasID, bool isLastSabling);
    }
}