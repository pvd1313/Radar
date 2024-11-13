using Beast.Claw.UI;
using Noesis;

namespace Beast.Radar.UI
{
    public class LoadingScreenView : BeastControl
    {
        public ProgressBar progressBar;
        public TextBlock progressText;
        public TextBlock textLog;
        public Grid grid;

        public override void BindElements()
        {
            _find("progress_bar", out progressBar);
            _find("progress_text", out progressText);
            _find("text_log", out textLog);
            _find("grid", out grid);
        }
    }
}
