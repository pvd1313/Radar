using UnityEngine;
using Zenject;

namespace Beast.Claw.Editor.Window.Profiler
{
    public static class SceneContextUtil
    {
        private static bool _contextFound;
        private static bool _isPlaying;
        private static SceneContext _context;
        
        public static T TryResolve<T>() where T : class
        {
            if (!Application.isPlaying)
            {
                _isPlaying = false;
                
                return null;
            }
            
            if (!_isPlaying)
            {
                _context = Object.FindObjectOfType<SceneContext>();
                _contextFound = _context != null;
                _isPlaying = true;
            }
            
            if (!_contextFound)
            {
                return null;
            }
            
            return _context.Container.TryResolve<T>();
        }
    }
}