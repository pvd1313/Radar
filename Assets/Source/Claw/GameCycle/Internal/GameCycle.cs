using System;
using System.Collections.Generic;
using Beast.Claw.Debug;
using Beast.Claw.Profiling;
using Cysharp.Threading.Tasks;
using Zenject;

namespace Beast.Claw.GameCycle
{
    public partial class GameCycle : IDisposable
    {
        // private readonly Proxy _proxy;
        private readonly GameCycleConfig _config;
        // private readonly ProfilerConfig _profilerConfig;
        private readonly DiContainer _container;

        // private HashSet<object> _brokenUpdates;
        // private Dictionary<object, int> _updateErrorCount;
        // private bool _allUpdateIsBroken;
        
        // private Dictionary<string, HashSet<object>> _updateHashset;
        // private Dictionary<string, HashSet<object>> _operationHashset;

        // private Dictionary<string, HashSet<object>> _singleFiredObjects;

        private Dictionary<string, Runner> _runners;

        // private Dictionary<string, Profiler> _profilers;
        // private bool _profilersInitialized;
        // private Dictionary<string, List<string>> _updateObjectNames;
        // private Dictionary<string, List<string>> _operationObjectNames;
        // private ProfilerTreeStringBuilder _profilerTreeStringBuilder;
        
        private HashSet<object> _registeredListeners;

        private bool _initialized;
        private bool _isRegisteringListeners;
        // private Dictionary<string, OperationInfo> _operationEvents;
        // private HashSet<string> _updateEvents;

        public GameCycle (Proxy proxy, GameCycleConfig config, DiContainer container)
        {
            _config = config;
            _container = container;

            proxy.Inject(this);
        }
        
        private void _initialize(GameCycleInitialization input)
        {
            if (_initialized)
            {
                throw new InvalidOperationException("Already initialized");
            }
            
            input.ValidateThrowException(nameof(input));
            
            _initialized = true;

            _registeredListeners = new HashSet<object>();
            _runners = new Dictionary<string, Runner>();

            foreach (OperationInfo operation in input.operations)
            {
                _runners[operation.id] = new Runner(this, _config, operation);
            }
        }

        private void _registerListeners()
        {
            _isRegisteringListeners = true;
            
            Handle handle = new (this);

            List<IGameCycleListener> listeners = _container.TryResolve<List<IGameCycleListener>>();
            if (listeners == null)
            {
                throw new Exception("Failed to resolve listeners");
            }
            
            foreach (IGameCycleListener subscriber in listeners)
            {
                if (_registeredListeners.Contains(subscriber))
                {
                    continue;
                }

                try
                {
                    handle.currentListener = subscriber;
                    subscriber.Register(handle);
                }
                catch (Exception e)
                {
                    BDebug.LogException(this, e, $"Exception during registration ({subscriber.GetType()})");

                    throw;
                }
                finally
                {
                    _registeredListeners.Add(subscriber);
                }
            }
            
            _isRegisteringListeners = false;
        }

        // private void _tryInitializeProfiling()
        // {
        //     if (_profilersInitialized)
        //     {
        //         return;
        //     }
        //
        //     _profilerTreeStringBuilder = new ProfilerTreeStringBuilder();
        //     _profilers = new Dictionary<string, Profiler>();
        //     
        //     _operationObjectNames = new Dictionary<string, List<string>>();
        //     foreach (string eventID in _operationEvents)
        //     {
        //         _operationObjectNames[eventID] = _operationListeners[eventID]
        //             .Select(listener => listener.obj.GetType().Name).ToList();
        //         
        //         Profiler profiler = new (_profilerConfig);
        //         profiler.Initialize();
        //         _profilers[eventID] = profiler;
        //     }
        //
        //     _updateObjectNames = new Dictionary<string, List<string>>();
        //     foreach (string eventID in _updateEvents)
        //     {
        //         _updateObjectNames[eventID] = _updateListeners[eventID]
        //             .Select(listener => listener.obj.GetType().Name).ToList();
        //         
        //         Profiler profiler = new (_profilerConfig);
        //         profiler.Initialize();
        //         _profilers[eventID] = profiler;
        //     }
        //     
        //     _proxy.InitializeProfilers();
        //     
        //     _profilersInitialized = true;
        // }

        private async UniTask<OperationResult> _execute<TOwner>(TOwner owner, string eventID) 
            where TOwner : class
        {
            eventID.ValidateNotEmptyOrNullThrowException(nameof(eventID));
            owner.ValidateNotNullThrowException(nameof(owner));
            
            if (_isRegisteringListeners)
            {
                throw new InvalidOperationException("Tried to invoke operation during registration");
            }

            if (!_runners.TryGetValue(eventID, out Runner runner))
            {
                throw new InvalidOperationException($"Tried to invoke invalid operation '{eventID}'");
            }

            return await runner.ExecuteConcurrent();
            
            // OperationResult result = OperationResult.Success();
            //
            // HashSet<object> alreadyFired = null;
            // if (operationInfo.executedOncePerObject)
            // {
            //     alreadyFired = _singleFiredObjects[eventID];
            // }
            //
            // // bool profilingActive = _profilerConfig.AnyProfilingActive;
            // // Profiler.Handle handle = Profiler.Handle.Disabled;
            // // Profiler profiler = null;
            // // List<string> objNames = null;
            // //
            // // if (profilingActive)
            // // {
            // //     _tryInitializeProfiling();
            // //     profiler = _profilers[eventID];
            // //     profiler.FetchConfig();
            // //     objNames = _operationObjectNames[eventID];
            // // }
            //
            // List<OperationListener> listeners = _operationListeners[eventID];
            //
            // for (int listenerIndex = 0; listenerIndex < listeners.Count; listenerIndex++)
            // {
            //     OperationListener listener = listeners[listenerIndex];
            //
            //     if (operationInfo.executedOncePerObject && !alreadyFired.Add(listener.obj))
            //     {
            //         continue;
            //     }
            //     
            //     // string sampleName = null;
            //     
            //     // if (profilingActive)
            //     // {
            //     //     handle = profiler.GetHandle(listener.obj);
            //     //     sampleName = objNames[listenerIndex];
            //     //
            //     //     handle.BeginSample(sampleName);
            //     // }
            //
            //     try
            //     {
            //         OperationContext context = default;
            //         result = await listener.action.Invoke(context);
            //     }
            //     catch (Exception e)
            //     {
            //         BDebug.LogException(owner, e, $"Exception during {eventID} ({listener.obj.GetType()})");
            //         break;
            //     }
            //     finally
            //     {
            //         // if (profilingActive)
            //         // {
            //         //     handle.EndSample(sampleName);
            //         // }
            //     }
            //
            //     if (result.status != EOperationStatus.Success)
            //     {
            //         BDebug.LogError(owner, $"Failed to {eventID} ({listener.obj.GetType()})");
            //         break;
            //     }
            // }
            //
            // // if (_profilerConfig.enableClawProfiling)
            // // {
            // //     profiler.RebuildTree();
            // //
            // //     if (_profilerConfig.logOperationProfiling)
            // //     {
            // //         string info = _profilerTreeStringBuilder.ToString(profiler.Reader, eventID);
            // //     
            // //         BDebug.StaticLogInfo(info);
            // //     }
            // // }
            //
            // return result;
        }

        private void _update<TOwner>(TOwner owner, string eventID) where TOwner : class
        {
            eventID.ValidateNotEmptyOrNullThrowException(nameof(eventID));
            owner.ValidateNotNullThrowException(nameof(owner));
            
            // if (_allUpdateIsBroken)
            // {
            //     return;
            // }
            
            if (_isRegisteringListeners)
            {
                throw new InvalidOperationException("Tried to invoke update during subscription");
            }

            if (!_runners.TryGetValue(eventID, out Runner runner))
            {
                throw new InvalidOperationException($"Tried to invoke invalid update '{eventID}'");
            }
            
            runner.ExecuteBlocking();
            
            // if (!_updateEvents.Contains(eventID))
            // {
            //     
            // }
            //
            // bool hasBrokenLogic = _brokenUpdates.Count > 0;
            //
            // // bool profilingActive = _profilerConfig.AnyProfilingActive;
            // // Profiler.Handle handle = Profiler.Handle.Disabled;
            // // Profiler profiler = null;
            // // List<string> objNames = null;
            // //
            // // if (profilingActive)
            // // {
            // //     _tryInitializeProfiling();
            // //     profiler = _profilers[eventID];
            // //     profiler.FetchConfig();
            // //     objNames = _updateObjectNames[eventID];
            // // }
            //
            // List<UpdateListener> updateListeners = _updateListeners[eventID];
            //
            // for (int listenerIndex = 0; listenerIndex < updateListeners.Count; listenerIndex++)
            // {
            //     UpdateListener listener = updateListeners[listenerIndex];
            //     
            //     if (hasBrokenLogic && _brokenUpdates.Contains(listener.obj))
            //     {
            //         continue;
            //     }
            //     
            //     // string sampleName = null;
            //     
            //     // if (profilingActive)
            //     // {
            //     //     handle = profiler.GetHandle(listener.obj);
            //     //     sampleName = objNames[listenerIndex];
            //     //         
            //     //     handle.BeginSample(sampleName);
            //     // }
            //
            //     try
            //     {
            //         OperationContext context = default;
            //         listener.action.Invoke(context);
            //     }
            //     catch (Exception e)
            //     {
            //         ProcessUpdatingException(e, eventID, listener.obj, out bool canContinueExecution);
            //
            //         if (!canContinueExecution)
            //         {
            //             _allUpdateIsBroken = true;
            //
            //             return;
            //         }
            //     }
            //     finally
            //     {
            //         // if (profilingActive)
            //         // {
            //         //     handle.EndSample(sampleName);
            //         // }
            //     }
            // }
            //
            // // if (_profilerConfig.enableClawProfiling)
            // // {
            // //     profiler.RebuildTree();
            // // }
        }
        
        // private void ProcessUpdatingException(
        //     Exception e, 
        //     string eventName, 
        //     object logic, 
        //     out bool canContinueExecution)
        // {
        //     BDebug.LogException(this, e, logic.GetType() + "." + eventName + "()");
        //     
        //     // _updateErrorCount.TryGetValue(logic, out int errorCount);
        //     // errorCount++;
        //     // _updateErrorCount[logic] = errorCount;
        //     //
        //     // if (errorCount > _config.updateExceptionThreshold)
        //     // {
        //     //     _brokenUpdates.Add(logic);
        //     // }
        //     //
        //     // if (_config.exceptionStopsUpdates)
        //     // {
        //     //     canContinueExecution = false;
        //     // }
        //     // else
        //     // {
        //     //     canContinueExecution = true;
        //     // }
        // }

        public void Dispose()
        {
            // if (_profilersInitialized)
            // {
            //     foreach (Profiler profiler in _profilers.Values)
            //     {
            //         profiler.Dispose();
            //     }
            //     
            //     _profilers.Clear();
            //     
            //     _profilersInitialized = false;
            // }
        }
    }
}