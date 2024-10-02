using Beast.Radar.UI.Panels.Base;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.UI.Canvas
{
    public class CanvasProvider 
    {
        private const float FadeDuration = 0.5f; 
        
        private readonly CanvasView _view;

        public CanvasProvider(CanvasView canvasView)
        {
            _view = canvasView;
        }
        
        public void AddChildPanel(BView bView, bool asLastSibling)
        {
            bView.transform.SetParent(_view.body, false);
            
            if (asLastSibling)
            {
                bView.transform.SetAsLastSibling();
            }
            else
            {
                bView.transform.SetAsFirstSibling();
            }
        }

        public void ShowBlackOverlay()
        {
            Fade(0f, 1f, FadeDuration).Forget();
        }

        public void HideBlackOverlay()
        {
            Fade(1f, 0f, FadeDuration).Forget();
        }
        
        private async UniTask Fade(float startAlpha, float endAlpha, float duration)
        {
            float elapsedTime = 0f;
            _view.blackOverlay.alpha = startAlpha;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                _view.blackOverlay.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
                await UniTask.Yield();
            }
            
            _view.blackOverlay.alpha = endAlpha;
            _view.blackOverlay.interactable = endAlpha > 0;
            _view.blackOverlay.blocksRaycasts = endAlpha > 0;
        }
    }
}