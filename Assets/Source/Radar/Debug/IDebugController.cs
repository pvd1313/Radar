using System;

namespace Beast.Radar.Debug
{
    public interface IDebugController
    {
        void SetElementState(string elementID, bool isActive);
        bool GetElementState(string elementID);
    }
}