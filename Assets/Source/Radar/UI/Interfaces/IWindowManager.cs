using System;
using Beast.Claw.Common;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.UI
{
    public interface IWindowManager
    {
        event Action<string> WindowChanged; 

        public Transform CanvasTransform { get; }
        public string WindowID { get; }
        
        void Register<TData>(IWindow<TData> window, string windowID) where TData : struct;
        
        IWindowHandle<TData> GetHandle<TData>(IController owner, string windowID) where TData : struct;
    }
}