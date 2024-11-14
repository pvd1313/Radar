using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Claw.Mathematics;
using Beast.Radar.City;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine.Device;

namespace Beast.Radar.UI
{
    public class CityViewController : Controller, IGameCycleListener, IReady
    {
        public bool IsReady { get; private set; }

        private readonly IWorldViewInstance _worldView;
        private readonly ICitySpawner _citySpawner;
        private readonly ICameraTransform _camera;

        private readonly CorePrefabLibrary _prefabLibrary;
        private readonly UIConfig _uiConfig;
        private readonly WorldViewConfig _worldViewConfig;

        private UIPool<CityView> _cityPool;
        private int2 _screenSize;

        public CityViewController(
            WorldViewConfig worldViewConfig,
            ICitySpawner citySpawner,
            ICameraTransform camera,
            IWorldViewInstance worldView,
            CorePrefabLibrary prefabLibrary,
            UIConfig uiConfig)
        {
            _worldViewConfig = worldViewConfig;
            _worldView = worldView;
            _citySpawner = citySpawner;
            _camera = camera;
            _prefabLibrary = prefabLibrary;
            _uiConfig = uiConfig;
        }

        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
            handle.ListenConcurrent(ActionID.GAME_LOAD, _gameLoad);
            handle.ListenBlocking(ActionID.TICK, _tick);
        }

        private UniTask _warmup(ConcurrentContext arg)
        {
            _screenSize = new int2(Screen.width, Screen.height);

            _cityPool = new UIPool<CityView>(_prefabLibrary.city, _worldView.Instance.cityRoot);

            return UniTask.CompletedTask;
        }

        private UniTask _gameLoad(ConcurrentContext arg)
        {
            foreach (CityInfo city in _citySpawner.Cities)
            {
                CityView view = _cityPool.GetNext();
                view._image.SetSprite(_uiConfig.citySprites[city.size]);
                view.transform.position = _camera.WorldToScreen(city.position).XY0();
            }

            IsReady = true;
            return UniTask.CompletedTask;
        }

        private void _tick(BlockingContext obj)
        {
            if (!IsReady)
            {
                return;
            }

            for (int i = 0; i < _citySpawner.Cities.Length; i++)
            {
                CityInfo city = _citySpawner.Cities[i];
                CityView view = _cityPool.GetByIndex(i);
          
                float visibleZoom = _worldViewConfig.cityVisibilityZoomLevels[city.size];

                if (_camera.Zoom01 <= visibleZoom)
                {
                    view.transform.localScale = new float3(2 - _camera.Zoom01);
                    view.transform.position = _camera.WorldToScreen(city.position).XY0();
                 
                    if (!view.gameObject.activeSelf)
                    {
                        _cityPool.Enable(view);
                    }
                }
                else if (view.gameObject.activeSelf)
                {
                    _cityPool.Disable(view);
                }
            }
        }
    }
}