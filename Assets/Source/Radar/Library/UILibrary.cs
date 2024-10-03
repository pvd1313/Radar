using System.Linq;
using Beast.Claw.Common;
using Beast.Radar.UI;

namespace Beast.Radar
{
    public class UILibrary: BeastLibrary
    {
        public CanvasPrefabData[] canvases;
        public PanelPrefabData[] panels;

        public CanvasView GetCanvasPrefab(string id)
        {
            CanvasView view = canvases.FirstOrDefault(d => d.ID == id).view;
           
            if (view == null)
            {
                throw new BeastException($"UI library cannot find a canvas prefab with id #{id}");
            }
            
            return view;
        }

        public BeastUIWindowView GetPanelPrefab(string id)
        {
            BeastUIWindowView view = panels.FirstOrDefault(d => d.ID == id).view;
           
            if (view == null)
            {
                throw new BeastException($"UI library cannot find a panel prefab with id #{id}");
            }
            
            return view;
        }
    }
}