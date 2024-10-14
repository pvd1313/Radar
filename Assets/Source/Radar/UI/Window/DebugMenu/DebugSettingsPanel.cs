using System;
using System.Collections.Generic;
using Beast.Claw.Common;
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

        private int[] _frameRates;

        private SelectablesController _selectablesController;

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
            InitializeFrameRates();
            ConfigureSelectables();
            SubscribeView();
            
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

        private void ConfigureSelectables()
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
               selectionsGrid: selectables, 
               isLooping: false);

            _selectablesController = new SelectablesController(_input, new Dictionary<ESelectionZone, SelectableGridData>
            {
                { dpadGrid.Item1, dpadGrid.Item2 }
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