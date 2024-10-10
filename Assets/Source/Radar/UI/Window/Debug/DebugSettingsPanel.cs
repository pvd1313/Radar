using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Radar.InputControl;
using UnityEngine;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class DebugSettingsPanel: IPanel
    {
        private readonly DebugSettingsFolderView _view;
        private readonly ISelectionZoneBuilder _zoneBuilder;
        private readonly IUIInputController _input;

        private readonly float[] _timeScales = { 1, 3, 10 };
        private int[] _frameRates;
        
        private SelectableController _selectableController;
        
        public DebugSettingsPanel(
            DebugSettingsFolderView view, 
            ISelectionZoneBuilder zoneBuilder,
            IUIInputController input)
        {
            _zoneBuilder = zoneBuilder;
            _input = input;
            _view = view;
        }
        
        public void Warmup()
        {
            InitializeFrameRates();
            ConfigureSelectables();
            SubscribeView();
        }

        public void Dispose()
        {
        }

        public void Open()
        {
            _selectableController.Enable();
            _view.body.SetActive(true);    
        }

        public void Close()
        {
            _view.body.SetActive(false);
            _selectableController.Disable();
        }

        private void ConfigureSelectables()
        {
            Selectable[][] selectables =
            {
                new[] { _view.timeScaleButtons[0].Selectable, _view.frameRateButtons[0].Selectable, },
                new[] { _view.timeScaleButtons[1].Selectable, _view.frameRateButtons[1].Selectable, },
                new[] { _view.timeScaleButtons[2].Selectable, _view.frameRateButtons[2].Selectable, },
                new[] { null, _view.frameRateButtons[3].Selectable, },
                new[] { null, _view.frameRateButtons[4].Selectable, },
            };

            SelectableGridData dpadGrid = _zoneBuilder.Build(selectables, isLooping: false);

            _selectableController = new SelectableController(_input, new Dictionary<ESelectionZone, SelectableGridData>
            {
                { ESelectionZone.DPad, dpadGrid }
            });
        }

        private void InitializeFrameRates()
        {
            RefreshRate refreshRate = Screen.currentResolution.refreshRateRatio;
            float screenRate = (float)refreshRate.numerator / refreshRate.denominator;
            _frameRates = new[]
            {
                10, 30, 60, (int)screenRate, 999
            };
        }

        private void SubscribeView()
        {
            for (int i = 0; i < _view.timeScaleButtons.Length; i++)
            {
                int index = i;
                _view.timeScaleButtons[i].Clicked += () => OnTimeScaleButtonClicked(index);
            }
            
            for (int i = 0; i < _view.frameRateButtons.Length; i++)
            {
                int index = i;
                _view.frameRateButtons[i].Clicked += () => OnFrameRateButtonClicked(index);
            }
        }

        private void OnTimeScaleButtonClicked(int index)
        {
            Time.timeScale = _timeScales[index];
            BDebug.LogInfo(this, $"Time scale = {_timeScales[index]}");
        }

        private void OnFrameRateButtonClicked(int index)
        {
            Application.targetFrameRate = _frameRates[index];
            BDebug.LogInfo(this, $"Target frame rate = {_frameRates[index]}");
        }
    }
}