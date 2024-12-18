using Beast.Claw.UI;
using Noesis;

namespace Beast.Radar.UI
{
    public class MainMenuView : BeastControl
    {
        public Button button_discord;
        public Button button_youtube;
        public Button button_continue;
        public Button button_newGame;
        public Button button_options;
        public Button button_about;

        private StackPanel _scroll_items;
        private Style _versionStyle;
        private Style _descriptionStyle;

        public override void BindElements()
        {
            _versionStyle = (Style)Resources["Style.Text.Active.Header"];
            _descriptionStyle = (Style)Resources["Style.Text.Light.Normal"];
            
            _find("button_discord", out button_discord);
            _find("button_youtube", out button_youtube);
            _find("button_continue", out button_continue);
            _find("button_new_game", out button_newGame);
            _find("button_options", out button_options);
            _find("button_about", out button_about);
            _find("stack_updates", out _scroll_items);
            
            _find("grid_window", out Grid grid_window);
            _updateSafeArea(grid_window);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="version"></param>
        /// <param name="description">max 55 chars in line</param>
        public void AddVersionDescription(string version, string description)
        {
            _scroll_items.Children.Add(new TextBlock()
            {
                Text = version,
                Style = _versionStyle
            });
            
            _scroll_items.Children.Add(new TextBlock()
            {
                Text = description,
                Style = _descriptionStyle
            });
        }
    }
}