using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class LoadingTaskText : UIView, IUIPoolable
    {
        [SerializeField] private UIText _uiText;
        [SerializeField] private float _dotsAnimationCooldownSec = 0.5f; 
        private string _task;
        
        private bool _isRunning;
        private CancellationTokenSource _cancellationTokenSource;
        
        [Button]
        public async void SetTask(string task)
        {
            _task = task;
            _uiText.SetText(_task);
            await _startUpdatingDots();
        }

        [Button]
        public void Completed(string duration)
        {
            _stopUpdatingDots();
            _uiText.SetText($"{_task}... [{duration}]");
        }

        public void Enable()
        {
            gameObject.SetActive(true);
        }

        public void Disable()
        {
            gameObject.SetActive(false);
        }

        private void _stopUpdatingDots()
        {
            _isRunning = false;
        }

        private async UniTask _startUpdatingDots()
        {
            _isRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();
            int dotCount = 1;
            
            while (_isRunning)
            {
                _uiText.SetText($"{_task}{new string('.', dotCount)}");
                dotCount = dotCount % 3 + 1; 
                await UniTask.Delay((int)(_dotsAnimationCooldownSec * 1000), cancellationToken: _cancellationTokenSource.Token); 
            }
            
            _cancellationTokenSource.Cancel(); 
        }
    }
}