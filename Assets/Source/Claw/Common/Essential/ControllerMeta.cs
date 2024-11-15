namespace Beast.Claw.Common
{
    public struct ControllerMeta
    {
        private static int _nextID;
        
        public string name;
        public int guid;

        public static ControllerMeta Create(object owner)
        {
            if (Build.IS_DEVELOPMENT)
            {
                return new ControllerMeta
                {
                    name = owner.GetType().FullName,
                    guid = _nextID++
                };
            }

            return new ControllerMeta
            {
                name = "Controller",
                guid = _nextID++
            };
        }
    }
}