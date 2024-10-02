using System.Linq;
using Beast.Claw.Common;
using Beast.Radar.UI;
using Beast.Radar.UI.Data;

namespace Beast.Radar
{
    public class UILibrary: BeastLibrary
    {
        public CanvasPrefabData[] canvases;
        public PanelPrefabData[] panels;
        public OverlayPrefabData[] overlays;

        public UICanvasView GetCanvasPrefab(string id)
        {
            UICanvasView view = canvases.FirstOrDefault(d => d.ID == id).view;
           
            if (view == null)
            {
                throw new BeastException($"UI library cannot find a canvas prefab with id #{id}");
            }
            
            return view;
        }

        public UIPanelView GetPanelPrefab(string id)
        {
            UIPanelView view = panels.FirstOrDefault(d => d.ID == id).view;
           
            if (view == null)
            {
                throw new BeastException($"UI library cannot find a panel prefab with id #{id}");
            }
            
            return view;
        }
        
        public UIOverlayView GetOverlayPrefab(string id)
        {
            UIOverlayView view = overlays.FirstOrDefault(d => d.ID == id).view;
           
            if (view == null)
            {
                throw new BeastException($"UI library cannot find a overlay prefab with id #{id}");
            }
            
            return view;
        }
    }
}