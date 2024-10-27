using System.Collections.Generic;

namespace Beast.Radar.UI
{
    public static class WindowID
    {
        public static readonly string None = "none";
        public static readonly string MainHUD = "hud_window";
        public static readonly string Debug = "debug_window";
        public static readonly string Pause = "pause_window";
        public static readonly string MapGeneration = "map_generation_window";
        public static readonly string LoadingScreen = "loading_window";

        public static string[] WindowIndex = new[]
        {
            MainHUD,
            Debug,
            Pause,
            MapGeneration,
            LoadingScreen,
        };
    }
}