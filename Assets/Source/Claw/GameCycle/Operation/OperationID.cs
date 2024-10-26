namespace Beast.Claw.GameCycle
{
    public static class OperationID
    {
        public const string PREFERENCES_LOAD = "preferences_load";
        public const string PREFERENCES_SAVE = "preferences_save";
        
        public const string WARMUP = "warmup";
        public const string DISPOSE = "dispose";
        
        public const string GAME_LOAD = "game_load";
        public const string GAME_SAVE = "game_save";
        
        public const string TICK = "tick";
        public const string RENDER = "render";
        public const string FIXED_UPDATE = "fixed_update";
        public const string GIZMOS_DRAW = "gizmos_draw";
    }
}