using System;

namespace Beast.Claw.Common
{
    [Serializable]
    public struct ControllerMeta
    {
        private static int _nextID;
        
        public string fullName;
        public string shortName;
        public int guid;

        public static ControllerMeta Create(object owner)
        {
            Type type = owner.GetType();
            
            return new ControllerMeta
            {
                fullName = type.FullName,
                shortName = type.Name,
                guid = _nextID++
            };
        }
    }
}