using Cysharp.Threading.Tasks;

namespace Beast.Radar.UI
{
    partial class WindowManager
    {
        private class Handle<TData> : IWindowHandle<TData> where TData : struct
        {
            private readonly WindowManager _manager;
            private readonly string _windowID;

            public Handle(WindowManager manager, string windowID)
            {
                _manager = manager;
                _windowID = windowID;
            }

            public async UniTask Open(TData data)
            {
                await _manager._open(_windowID, data);
            }
        }
    }
}