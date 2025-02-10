using Beast.Claw.UI;
using Noesis;

namespace Beast.Radar.UI
{
    public class MainMenuView : BeastControl
    {
        public Button buttonDiscord;
        public Button buttonYoutube;
        public Button buttonContinue;
        public Button buttonNewGame;
        public Button buttonOptions;
        public Button buttonAbout;

        private StackPanel _scrollItems;
        private Style _versionStyle;
        private Style _descriptionStyle;

        public override void BindElements()
        {
            _versionStyle = (Style)Resources["Style.Text.Active.Header"];
            _descriptionStyle = (Style)Resources["Style.Text.Light.Normal"];
            
            _find("button_discord", out buttonDiscord);
            _find("button_youtube", out buttonYoutube);
            _find("button_continue", out buttonContinue);
            _find("button_new_game", out buttonNewGame);
            _find("button_options", out buttonOptions);
            _find("button_about", out buttonAbout);
            _find("stack_updates", out _scrollItems);
            
            _find("grid_window", out Grid gridWindow);
            _updateSafeArea(gridWindow);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="version"></param>
        /// <param name="description">max 55 chars in line</param>
        public void AddVersionDescription(string version, string description)
        {
            _scrollItems.Children.Add(new TextBlock
            {
                Text = version,
                Style = _versionStyle
            });
            
            _scrollItems.Children.Add(new TextBlock
            {
                Text = description,
                Style = _descriptionStyle
            });
        }
    }
}