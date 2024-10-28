using System;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.UI
{
    partial class WindowManager
    {
        private struct WindowData
        {
            public Type dataType;
            public string windowID;
            public object dataContainer;
            public Func<UniTask> openWindow;
            public Func<UniTask> closeWindow;
        }
    }
}