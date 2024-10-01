using System;
using Beast.Radar.UI.Enums;
using Beast.Radar.UI.Panels.Base;

namespace Beast.Radar.UI.Panels.Hud
{
    public class HudProvider : BProvider
    {
        public override bool ShowsBlackOverlay => false;
        
        private readonly HudView _view;

        public HudProvider(HudView hudView)
        {
            _view = hudView;
        }

        public void Initialize()
        {
            SubscribeToEvents(true);
        }
        
        public void Dispose()
        {
            SubscribeToEvents(false);
        }
        
        public override void OpenPanel(Action onCompleted = null)
        {
            onCompleted?.Invoke();
        }

        public override void ClosePanel(Action onCompleted = null)
        {
            onCompleted?.Invoke();
        }

        private void SubscribeToEvents(bool flag)
        {
            if (flag)
            {
                _view.pauseButton.Pressed += OnPauseButtonPressed;
            }
            else
            {
                _view.pauseButton.Pressed -= OnPauseButtonPressed;
            }
        }

        private void OnPauseButtonPressed()
        {
            InvokeOtherPanelCall(EPanelType.Pause);
        }
    }
}