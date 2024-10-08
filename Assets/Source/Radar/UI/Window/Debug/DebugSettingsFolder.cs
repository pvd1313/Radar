using System.Collections.Generic;
using System.Linq;
using Beast.Claw.Common;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class DebugSettingsFolder: IFolder
    {
        public Dictionary<ESelectionZone, SelectableGridData> Selections { get; private set; }
        private static readonly float[] TimeScales = { 1, 3, 10 };
        
        private readonly ISelectionZoneBuilder _zoneBuilder;
        private readonly DebugSettingsFolderView _view;
        
        public DebugSettingsFolder(DebugSettingsFolderView view, ISelectionZoneBuilder zoneBuilder)
        {
            _zoneBuilder = zoneBuilder;
            _view = view;
        }
        
        public void Warmup()
        {
            SelectableGridData dpadGrid = _zoneBuilder.Build(
                _view.timeScaleButtons.Select(b => b.Selectable).ToArray(), 
                new int2(1, 3),
                isLooping: false);
        
            Selections = new Dictionary<ESelectionZone, SelectableGridData>()
            {
                { ESelectionZone.DPad , dpadGrid}
            };
            
            Subscribe();
        }

        public void Dispose()
        {
        }
        
        public void Open()
        {
            _view.body.SetActive(true);    
        }

        public void Close()
        {
            _view.body.SetActive(false);
        }

        private void Subscribe()
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