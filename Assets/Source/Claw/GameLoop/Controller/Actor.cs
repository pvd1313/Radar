using System;
using System.Reflection;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameLoop
{
    public abstract class Actor : Controller, IGameLoopActor
    {
        private const string BASE_METHOD_CALL = "Actor's base method was subscribed or called.";
        private static readonly Type ACTOR_TYPE = typeof(Actor);
        
        public void Register(IGameLoopHandle handle)
        {
            Type type = GetType();

            if (_isMethodImplemented(type, nameof(_warmup)))
            {
                handle.ListenConcurrent(ActionID.WARMUP, _warmup);
            }
            
            if (_isMethodImplemented(type, nameof(_preferencesLoad)))
            {
                handle.ListenConcurrent(ActionID.PREFERENCES_LOAD, _preferencesLoad);
            }
            
            if (_isMethodImplemented(type, nameof(_gameLoad)))
            {
                handle.ListenConcurrent(ActionID.GAME_LOAD, _gameLoad);
            }

            if (_isMethodImplemented(type, nameof(_tick)))
            {
                handle.ListenBlocking(ActionID.TICK, _tick);
            }
            
            if (_isMethodImplemented(type, nameof(_render)))
            {
                handle.ListenBlocking(ActionID.RENDER, _render);
            }
            
            if (_isMethodImplemented(type, nameof(_fixed)))
            {
                handle.ListenBlocking(ActionID.FIXED_UPDATE, _fixed);
            }

            if (_isMethodImplemented(type, nameof(_gameSave)))
            {
                handle.ListenBlocking(ActionID.GAME_SAVE, _gameSave);
            }

            if (_isMethodImplemented(type, nameof(_preferencesSave)))
            {
                handle.ListenBlocking(ActionID.PREFERENCES_SAVE, _preferencesSave);
            }

            if (_isMethodImplemented(type, nameof(_dispose)))
            {
                handle.ListenBlocking(ActionID.DISPOSE, _dispose);
            }
        }

        private bool _isMethodImplemented(Type type, string methodName)
        {
            BProfiler.BeginSample("Actor._isMethodImplemented");

            try
            {
                MethodInfo methodInfo = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
                
                if (methodInfo == null)
                {
                    return false;
                }

                return methodInfo.DeclaringType != ACTOR_TYPE;
            }
            finally
            {
                BProfiler.EndSample();
            }
        }

        protected virtual UniTask _warmup(ConcurrentContext context)
        {
            throw new InvalidOperationException(BASE_METHOD_CALL);
        }
        
        protected virtual UniTask _preferencesLoad(ConcurrentContext context)
        {
            throw new InvalidOperationException(BASE_METHOD_CALL);
        }
        
        protected virtual UniTask _gameLoad(ConcurrentContext context)
        {
            throw new InvalidOperationException(BASE_METHOD_CALL);
        }
        
        protected virtual void _tick(BlockingContext context)
        {
            throw new InvalidOperationException(BASE_METHOD_CALL);
        }
        
        protected virtual void _fixed(BlockingContext context)
        {
            throw new InvalidOperationException(BASE_METHOD_CALL);
        }
        
        protected virtual void _render(BlockingContext context)
        {
            throw new InvalidOperationException(BASE_METHOD_CALL);
        }
        
        protected virtual void _gameSave(BlockingContext context)
        {
            throw new InvalidOperationException(BASE_METHOD_CALL);
        }
        
        protected virtual void _preferencesSave(BlockingContext context)
        {
            throw new InvalidOperationException(BASE_METHOD_CALL);
        }
        
        protected virtual void _dispose(BlockingContext obj)
        {
            throw new InvalidOperationException(BASE_METHOD_CALL);
        }
    }
}