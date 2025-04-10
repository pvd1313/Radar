using System;

namespace Beast.Radar.Serialization
{
    [Flags]
    public enum ESaveManagerFlags
    {
        None,
        Silent = 1,
        AutoSkipWindow = 2,
    }
}