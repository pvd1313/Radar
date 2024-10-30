using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Radar.Debug;
using Beast.Radar.InputHandling;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class DebugSettingsPanel : IPanel
    {
        private readonly DebugSettingsPanelView _view;
        private readonly ISelectableZoneBuilder _zoneBuilder;
        private readonly IInputSystem _inputSystem;
        private readonly IDebugController _debugController;
        private readonly float[] _timeScales = { 1, 3, 10 };
        
        private SelectablesController _selectablesController;
        private int[] _frameRates;
        
        public DebugSettingsPanel(
            DebugSettingsPanelView view,
            ISelectableZoneBuilder zoneBuilder,
            IInputSystem inputSystem, 
            IDebugController debugController)
        {
            _zoneBuilder = zoneBuilder;
            _inputSystem = inputSystem;
            _debugController = debugController;
            _view = view;
        }

        public void Warmup()
        {
            _initializeFrameRates();
            _configureSelectables();
            _subscribeView();
        }
        
        public UniTask Open(Empty data)
        {
            _view.fpsToggle.SetValueWithoutNotify(_debugController.GetElementState(DebugElementID.FpsCounterID));
            _view.logToggle.SetValueWithoutNotify(_debugController.GetElementState(DebugElementID.LogConsoleID));

            _selectablesController.Enable();
            _view.body.SetActive(true);

            return UniTask.CompletedTask;
        }

        public UniTask Close()
        {
            _view.body.SetActive(false);
            _selectablesController.Disable();
            
            return UniTask.CompletedTask;
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
               zone: ESelectionZone.Navigate,
               selectableGrid: selectables, 
               isLooping: false);

           Dictionary<ESelectionZone, SelectableGridData> grids = new()
           {
               { dpadGrid.Item1, dpadGrid.Item2 }
           };
           
           _selectablesController = new SelectablesController(_inputSystem, grids);
           _selectablesController.Warmup();
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

            for (int i = 0; i < _view.frameRateButtons.Length; i++)
            {
                int index = i;
                _view.frameRateButtons[i].Clicked += () =>
                {
                    Application.targetFrameRate = _frameRates[index];
                    BDebug.LogInfo(this, $"Target frame rate = {_frameRates[index]}");
                };
            }

            _view.fpsToggle.Changed += on => _debugController.SetElementState(DebugElementID.LogConsoleID, on);
            _view.logToggle.Changed += on => _debugController.SetElementState(DebugElementID.FpsCounterID, on);
        }
    }
}