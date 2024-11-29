using Beast.Claw.Common;
using Beast.Claw.UI;
using Noesis;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class MainHUDView : BeastControl
    {
        public TextBlock textTime;
        public Button buttonSettings;
        public Button autoPathFindingButton;
        public Button buttonMove;
        
        private Thickness _thicknessActive;
        private Thickness _thicknessDisable;
        
        private Cache<int> _playerMovementCache;
        private Cache<bool> _autoPathFindingActiveCache;

        private string _pathIconMove;
        private string _pathIconStand;

        public override void BindElements()
        {
            _find("text_time", out textTime);
            _find("button_settings", out buttonSettings);
            _find("button_path", out autoPathFindingButton);
            _find("button_move", out buttonMove);

            _thicknessActive = new Thickness(1);
            _thicknessDisable= new Thickness(0);
            
            _pathIconMove = (string)FindResource("Path.Icon.Move");
            _pathIconStand = (string)FindResource("Path.Icon.Stand");
        }

        public void SetPlayerMovementState(bool hasTarget, bool isMoving)
        {
            int playerMovement = !hasTarget ? 0 : (isMoving ? 1 : 2);

            if (_playerMovementCache.Update(playerMovement))
            {
                buttonMove.Tag = isMoving ? _pathIconMove : _pathIconStand;
                buttonMove.Visibility = hasTarget ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public void SetAutoPathFindingActive(bool isActive)
        {
            if (_autoPathFindingActiveCache.Update(isActive))
            {
                autoPathFindingButton.BorderThickness = isActive ? _thicknessActive : _thicknessDisable;
            }
        }
    }
}