using System;
using Beast.Claw.Common;

namespace Beast.Radar.UI
{
    partial class WindowManager
    {
        private struct Record
        {
            public IController obtainedBy;
            public string windowID;
            public Type dataType;
        }
    }
}