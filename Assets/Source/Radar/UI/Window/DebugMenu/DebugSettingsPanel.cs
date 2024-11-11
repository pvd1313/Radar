using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Radar.Debug;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class DebugSettingsPanel : IPanel
    {
        private readonly DebugSettingsPanelView _view;
        private readonly IDebugController _debugController;
        private readonly float[] _timeScales = { 1, 3, 10 };
        
        private int[] _frameRates;
        
        public DebugSettingsPanel(
            DebugSettingsPanelView view,
            IDebugController debugController)
        {
            _debugController = debugController;
            _view = view;
        }

        public void Warmup()
        {
            _subscribeView();
        }
        
        public UniTask Open(Empty data)
        {
            _view.fpsToggle.SetValueWithoutNotify(_debugController.GetElementState(EDebugFeature.FpsCounter));
            _view.logToggle.SetValueWithoutNotify(_debugController.GetElementState(EDebugFeature.LogConsole));

            _view.body.SetActive(true);

            return UniTask.CompletedTask;
        }

        public UniTask Close()
        {
            _view.body.SetActive(false);
            
            return UniTask.CompletedTask;
        }

        private void _subscribeView()
        {
            for (int i = 0; i < _view.timeScaleButtons.Length; i++)
            {
                int index = i;
                _view.timeScaleButtons[i].Clicked += () =>
                {
                    Time.timeScale = _timeScales[index];
                    BDebug.LogInfo(this, $"Time scale = {_timeScales[index]}");
                };
            }
            
            _view.fpsToggle.Changed += on => _debugController.SetElementState(EDebugFeature.FpsCounter, on);
            _view.logToggle.Changed += on => _debugController.SetElementState(EDebugFeature.LogConsole, on);
        }
    }
}