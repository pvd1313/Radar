using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Radar.InputControl;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class DebugSettingsPanel : IPanel
    {
        private readonly DebugSettingsPanelView _view;
        private readonly ISelectionZoneBuilder _zoneBuilder;
        private readonly IUIInputController _input;
        private readonly float[] _timeScales = { 1, 3, 10 };

        private SelectablesController _selectablesController;
        private int[] _frameRates;
        
        public DebugSettingsPanel(
            DebugSettingsPanelView view,
            ISelectionZoneBuilder zoneBuilder,
            IUIInputController input)
        {
            _zoneBuilder = zoneBuilder;
            _input = input;
            _view = view;
        }

        public void Warmup()
        {
            _initializeFrameRates();
            _configureSelectables();
            _subscribeView();
            
            _selectablesController.Warmup();
        }

        public void Dispose()
        {
            _selectablesController.Dispose();
        }

        public void Open()
        {
            _selectablesController.Enable();
            _view.body.SetActive(true);
        }

        public void Close()
        {
            _view.body.SetActive(false);
            _selectablesController.Disable();
        }

        private void _configureSelectables()
        {
            UISelectable[,] selectables =
            {
                { _view.timeScaleButtons[0].Selectable, _view.frameRateButtons[0].Selectable, },
                { _view.timeScaleButtons[1].Selectable, _view.frameRateButtons[1].Selectable, },
                { _view.timeScaleButtons[2].Selectable, _view.frameRateButtons[2].Selectable, },
                { null, _view.frameRateButtons[3].Selectable, },
                { null, _view.frameRateButtons[4].Selectable, },
            };

           Tuple<ESelectionZone, SelectableGridData> dpadGrid = _zoneBuilder.Build(
               zone: ESelectionZone.DPad,
               selectablesGrid: selectables, 
               isLooping: false);

            _selectablesController = new SelectablesController(_input, new Dictionary<ESelectionZone, SelectableGridData>
            {
                { dpadGrid.Item1, dpadGrid.Item2 }
            });
        }

        private void _initializeFrameRates()
        {
            RefreshRate refreshRate = Screen.currentResolution.refreshRateRatio;
            float screenRate = (float)refreshRate.numerator / refreshRate.denominator;
            _frameRates = new[]
            {
                10, 30, 60, (int)screenRate, 999
            };
        }

        private void _subscribeView()
        {
            for (int i = 0; i < _view.timeScaleButtons.Length; i++)
            {
                int index = i;
                _view.timeScaleButtons[i].Clicked += () => _onTimeScaleButtonClicked(index);
            }

            for (int i = 0; i < _view.frameRateButtons.Length; i++)
            {
                int index = i;
                _view.frameRateButtons[i].Clicked += () => _onFrameRateButtonClicked(index);
            }
        }

        private void _onTimeScaleButtonClicked(int index)
        {
            Time.timeScale = _timeScales[index];
            BDebug.LogInfo(this, $"Time scale = {_timeScales[index]}");
        }

        private void _onFrameRateButtonClicked(int index)
        {
            Application.targetFrameRate = _frameRates[index];
            BDebug.LogInfo(this, $"Target frame rate = {_frameRates[index]}");
        }
    }
}