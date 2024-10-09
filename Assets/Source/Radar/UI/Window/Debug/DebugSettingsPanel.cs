using System.Collections.Generic;
using System.Linq;
using Beast.Claw.Common;
using Beast.Radar.InputControl;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class DebugSettingsPanel: IPanel
    {
        private static readonly float[] TimeScales = { 1, 3, 10 };
        
        private readonly DebugSettingsFolderView _view;
        private readonly ISelectionZoneBuilder _zoneBuilder;
        private readonly IUIInputController _input;

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
            SelectableGridData dpadGrid = _zoneBuilder.Build(_view.timeScaleButtons.Select(b => b.Selectable).ToArray(),
                isFillHorizontally: false,
                isLooping: false);

            _selectableController = new SelectableController(_input, new Dictionary<ESelectionZone, SelectableGridData>
            {
                { ESelectionZone.DPad , dpadGrid}
            });

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

        private void SubscribeView()
        {
            for (int i = 0; i < _view.timeScaleButtons.Length; i++)
            {
                int index = i;
                _view.timeScaleButtons[i].Clicked += () => OnTimeScaleButtonClicked(index);
            }
        }
        
        private void OnTimeScaleButtonClicked(int index)
        {
            Time.timeScale = TimeScales[index];
            BDebug.LogInfo(this, $"Time scale = {TimeScales[index]}");
        }
    }
}