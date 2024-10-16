using System;
using System.Collections.Generic;
using Beast.Claw.Debug;
using Beast.Radar.Debug;
using Beast.Radar.InputControl;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class DebugSettingsPanel : IPanel
    {
        private readonly DebugSettingsPanelView _view;
        private readonly ISelectableZoneBuilder _zoneBuilder;
        private readonly IUIInputController _uiInputController;
        private readonly DebugConfig _debugConfig;
        private readonly float[] _timeScales = { 1, 3, 10 };

        private List<ToggleLazyObjectController> _toggleableObjects;
        private SelectablesController _selectablesController;
        private int[] _frameRates;
        
        public DebugSettingsPanel(
            DebugSettingsPanelView view,
            ISelectableZoneBuilder zoneBuilder,
            IUIInputController uiInputController, 
            DebugConfig debugConfig)
        {
            _zoneBuilder = zoneBuilder;
            _uiInputController = uiInputController;
            _debugConfig = debugConfig;
            _view = view;
        }

        public void Warmup()
        {
            _initializeFrameRates();
            _configureToggles();
            _configureSelectables();
            _subscribeView();
        }

        public void Dispose()
        {
            foreach (ToggleLazyObjectController toggleableObject in _toggleableObjects)
            {
                toggleableObject.Dispose();
            }
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

        private void _initializeFrameRates()
        {
            RefreshRate refreshRate = Screen.currentResolution.refreshRateRatio;
            float screenRate = (float)refreshRate.numerator / refreshRate.denominator;
            _frameRates = new[]
            {
                10, 30, 60, (int)screenRate, 999
            };
        }

        private void _configureToggles()
        {
            _toggleableObjects = new List<ToggleLazyObjectController>()
            {
                new ToggleLazyObjectController(_view.logToggle, _debugConfig.logConsolePrefab),
                new ToggleLazyObjectController(_view.fpsToggle, _debugConfig.fpsCounterPrefab),
            };
            foreach (ToggleLazyObjectController toggleableObject in _toggleableObjects)
            {
                toggleableObject.Warmup();
            }
        }

        private void _configureSelectables()
        {
            UISelectable[,] selectables =
            {
                { _view.timeScaleButtons[0].Selectable, _view.frameRateButtons[0].Selectable, _view.logToggle.Selectable},
                { _view.timeScaleButtons[1].Selectable, _view.frameRateButtons[1].Selectable, _view.fpsToggle.Selectable},
                { _view.timeScaleButtons[2].Selectable, _view.frameRateButtons[2].Selectable, null},
                { null,                                 _view.frameRateButtons[3].Selectable, null},
                { null,                                 _view.frameRateButtons[4].Selectable, null},
            };

           Tuple<ESelectionZone, SelectableGridData> dpadGrid = _zoneBuilder.Build(
               zone: ESelectionZone.DPad,
               selectableGrid: selectables, 
               isLooping: false);

           Dictionary<ESelectionZone, SelectableGridData> grids = new()
           {
               { dpadGrid.Item1, dpadGrid.Item2 }
           };
           
           _selectablesController = new SelectablesController(_uiInputController, grids);
           _selectablesController.Warmup();
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