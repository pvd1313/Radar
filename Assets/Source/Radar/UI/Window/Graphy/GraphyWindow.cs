using Beast.Claw.GameLoop;
using Beast.Claw.Resources;
using Beast.Radar.Player;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Radar.UI.Graphy
{
    public class GraphyWindow : Actor, IGraphyWindow
    {
        private readonly PlayerPreference _preference;
        private readonly UIConfig _uiConfig;
        private readonly IResourceManager _resources;

        private GameObject _view;

        public GraphyWindow(
            PlayerPreference preference,
            UIConfig uiConfig,
            IResourceManager resources)
        {
            _preference = preference;
            _uiConfig = uiConfig;
            _resources = resources;
        }

        public async void Open()
        {
            if (_view == null)
            {
                _view = await _resources.InstantiateGameObject(this, _uiConfig.graphy);
            }
            else
            {
                _view.SetActive(true);
            }
        }

        public void Close()
        {
            if (_view != null)
            {
                _view.SetActive(false);
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