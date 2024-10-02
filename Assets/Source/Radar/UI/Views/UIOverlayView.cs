using Beast.Claw.Common;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class UIOverlayView: BeastMono
    {
        public RectTransform rect;
        
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _fadeDuration = 0.5f; 

        private bool _isHidden = true;
        
        public void Show()
        {
            Fade(0f, 1f, _fadeDuration).Forget();
        }

        public void Hide()
        {
            if (_isHidden)
            {
                return;
            }
            Fade(1f, 0f, _fadeDuration).Forget();
        }
        
        private async UniTask Fade(float startAlpha, float endAlpha, float duration)
        {
            float elapsedTime = 0f;
            _canvasGroup.alpha = startAlpha;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
                await UniTask.Yield();
            }
            
            _canvasGroup.alpha = endAlpha;
            _canvasGroup.interactable = endAlpha > 0;
            _canvasGroup.blocksRaycasts = endAlpha > 0;
            _isHidden = endAlpha == 0;
        }
    }
}