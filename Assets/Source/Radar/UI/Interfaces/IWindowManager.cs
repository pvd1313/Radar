using Beast.Claw.Common;
using Noesis;

namespace Beast.Radar.UI
{
    public interface IWindowManager
    {
        public string WindowID { get; }
        
        public WindowOpener Open { get; }
        
        void Attach(UIElement uiElement);
        
        void Attach(UIElement uiElement, int index);
        
        void Register<TData>(IWindow<TData> window, string windowID) where TData : struct;
        
        IWindowHandle<TData> GetHandle<TData>(IController owner, string windowID) where TData : struct;
    }
}