using System;
using Beast.Claw.Common;
using UnityEngine;

namespace Beast.Radar.UI
{
    public interface IWindowManager
    {
        event Action<string> WindowChanged; 
        event Action<bool> MouseOverUI;
        
        public Transform CanvasTransform { get; }
        public string CurrentWindow { get; }
        
        void Register<TData>(IWindow<TData> window, string windowID) where TData : struct;
        
        IWindowHandle<TData> GetHandle<TData>(IController owner, string windowID) where TData : struct;
    }
}