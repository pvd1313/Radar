namespace Beast.Radar
{
    public static class BeastCycleEventID
    {
        // Initialization
        public static string Warmup = "warmup";
        public static string Boot = "boot";
        public static string GameLoading = "game_loading";
        public static string GameSaving = "game_saving";
        public static string Dispose = "dispose";
        
        // Updates
        public static string FrameBegin = "frame_begin";
        public static string FrameEnd = "frame_end";
        public static string FixedUpdate = "fixed_update";
        public static string GizmosDraw = "gizmos_draw";
    }
}