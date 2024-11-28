using Beast.Claw.GameLoop;
using Beast.Claw.Resources;
using Beast.Radar.Player;
using Cysharp.Threading.Tasks;
using Noesis;
using UnityEngine;

namespace Beast.Radar.UI.Graphy
{
    public class GraphyWindowNoesis : Actor, IGraphyWindow
    {
        private readonly PlayerPreference _preference;
        private readonly UIConfig _uiConfig;
        private readonly INoesisWindowManager _noesisWindowManager;

        private GraphyView _view;

        public GraphyWindowNoesis(
            PlayerPreference preference,
            UIConfig uiConfig,
            INoesisWindowManager noesisWindowManager)
        {
            _preference = preference;
            _uiConfig = uiConfig;
            _noesisWindowManager = noesisWindowManager;
        }

        public async void Open()
        {
            if (_view == null)
            {
                _uiConfig.graphyAsset.Instantiate(out _view);
                _noesisWindowManager.Attach(_view, 100);
            }
            else
            {
                _view.Visibility = Visibility.Visible;
                
            }
        }

        public void Close()
        {
            if (_view != null)
            {
                _view.Visibility = Visibility.Collapsed;
            }
        }

        protected override UniTask _warmup(ConcurrentContext arg)
        {
            if (_preference.isShowGraphy)
            {
                Open();
            }

            return UniTask.CompletedTask;
        }
    }
}