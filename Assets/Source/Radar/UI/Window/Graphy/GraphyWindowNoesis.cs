using Beast.Claw.Attributes;
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
        [Required] private readonly PlayerPreference _preference;
        [Required] private readonly UIConfig _uiConfig;
        [Required] private readonly INoesisWindowManager _noesisWindowManager;

        private GraphyView _view;

        public void Open()
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