using System.Collections.Generic;
using Beast.Claw.Common;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class UIFactory : IUIFactory
    {
        private readonly UILibrary _library;
        private Dictionary<string, UICanvasView> _canvases = new();

        public UIFactory(UILibrary library)
        {
            _library = library;
        }

        public UICanvasView CreateCanvas(string canvasID)
        {
            if (_canvases.ContainsKey(canvasID))
            {
                throw new BeastException($"Canvas with this id #{canvasID} already exists");
            }

            UICanvasView canvasPrefab = _library.GetCanvasPrefab(canvasID);
            UICanvasView view = BeastScene.InstantiateMono(this, canvasPrefab, float3.zero, quaternion.identity);
            _canvases.Add(canvasID, view);
            return view;
        }

        public UIPanelView CreatePanel(string panelID, string canvasID, bool isLastSabling)
        {
            if (!_canvases.ContainsKey(canvasID))
            {
                throw new BeastException($"Factory cannot find a canvas instance with id #{canvasID}");
            }

            UIPanelView panelPrefab = _library.GetPanelPrefab(panelID);
            UIPanelView view = BeastScene.InstantiateMono(this, panelPrefab, float3.zero, quaternion.identity);
            SetParent(view.rect, _canvases[canvasID].rect, isLastSabling);
            return view;
        }

        public UIOverlayView CreateOverlay(string overlayID, string canvasID, bool isLastSabling)
        {
            if (!_canvases.ContainsKey(canvasID))
            {
                throw new BeastException($"Factory cannot find a canvas instance with id #{canvasID}");
            }

            UIOverlayView overlayPrefab = _library.GetOverlayPrefab(overlayID);
            UIOverlayView view = BeastScene.InstantiateMono(this, overlayPrefab, float3.zero, quaternion.identity);
            SetParent(view.rect, _canvases[canvasID].rect, isLastSabling);
            return view;
        }

        private void SetParent(RectTransform child, RectTransform parent, bool isLastSabling)
        {
            child.SetParent(parent, false);
            if (isLastSabling)
            {
                child.SetAsLastSibling();
            }
            else
            {
                child.SetAsFirstSibling();
            }
        }
    }
}