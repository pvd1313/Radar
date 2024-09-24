using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameCycle
{
    public class GameCycleLogic : Zenject.IInitializable
    {
        private readonly GameCycleConfig _config;
        private readonly List<Initializable> _init;
        private readonly List<IFrameBeginUpdate> _frameBegin;
        private readonly List<IFixedUpdate> _fixedUpdate;
        private readonly List<IFrameEndUpdate> _frameEnd;

        private HashSet<object> _brokenLogic;
        private Dictionary<object, int> _errors;
        private UpdateHandlerMono _updateHandlerMono;

        public GameCycleLogic (
            GameCycleConfig config,
            List<Initializable> init, 
            List<IFrameBeginUpdate> frameBegin,
            List<IFixedUpdate> fixedUpdate,
            List<IFrameEndUpdate> frameEnd)
        {
            _config = config;
            _init = init;
            _frameBegin = frameBegin;
            _fixedUpdate = fixedUpdate;
            _frameEnd = frameEnd;
        }
        
        public void Initialize()
        {
            UniTask.Create(InitializeAsync);
        }

        private async UniTask InitializeAsync()
        {
            _errors = new Dictionary<object, int>();
            _brokenLogic = new HashSet<object>();
            
            foreach (Initializable controller in _init)
            {
                InitializationResult result = await controller.Initialize();

                if (result.status != EInitializationStatus.Success)
                {
                    BeastLog.ErrorFor<GameCycleLogic>($"Failed to initialize {controller.GetType()}");
                     
                    return;
                }
            }
            
            _updateHandlerMono = BeastScene.NewObjectFor<GameCycleLogic>().AddComponent<UpdateHandlerMono>();
            _updateHandlerMono.FrameBegin += OnFrameBegin;
            _updateHandlerMono.FixedUpdated += OnFixedUpdate;
            _updateHandlerMono.FrameEnd += OnFrameEnd;
        }
        
        private void OnFrameBegin()
        {
            bool hasBrokenLogic = _brokenLogic.Count > 0;
            
            foreach (IFrameBeginUpdate logic in _frameBegin)
            {
                if (hasBrokenLogic && _brokenLogic.Contains(logic))
                {
                    continue;
                }
                
                try
                {
                    logic.OnFrameBegin();
                }
                catch (Exception e)
                {
                    ProcessUpdatingException(e, "FrameBeginUpdate", logic, out bool canContinueExecution);

                    if (!canContinueExecution)
                    {
                        break;
                    }
                }
            }
        }

        private void OnFixedUpdate()
        {
            bool hasBrokenLogic = _brokenLogic.Count > 0;
            
            foreach (IFixedUpdate logic in _fixedUpdate)
            {
                if (hasBrokenLogic && _brokenLogic.Contains(logic))
                {
                    continue;
                }
                
                try
                {
                    logic.OnFixedUpdate();
                }
                catch (Exception e)
                {
                    ProcessUpdatingException(e, "FixedUpdate", logic, out bool canContinueExecution);

                    if (!canContinueExecution)
                    {
                        break;
                    }
                }
            }
        }
        
        private void OnFrameEnd()
        {
            bool hasBrokenLogic = _brokenLogic.Count > 0;
            
            foreach (IFrameEndUpdate logic in _frameEnd)
            {
                if (hasBrokenLogic && _brokenLogic.Contains(logic))
                {
                    continue;
                }
                
                try
                {
                    logic.OnFrameEnd();
                }
                catch (Exception e)
                {
                    ProcessUpdatingException(e, "FrameEndUpdate", logic, out bool canContinueExecution);

                    if (!canContinueExecution)
                    {
                        break;
                    }
                }
            }
        }

        private void ProcessUpdatingException(
            Exception e, 
            string methodName, 
            object logic, 
            out bool canContinueExecution)
        {
            BeastLog.ExceptionFor<GameCycleLogic>(e, logic.GetType() + "." + methodName + "()");
            
            _errors.TryGetValue(logic, out int errorCount);
            errorCount++;
            _errors[logic] = errorCount;

            if (errorCount > _config.exceptionThreshold)
            {
                _brokenLogic.Add(logic);
            }

            if (_config.exceptionStopsExecution)
            {
                canContinueExecution = false;
                
                _updateHandlerMono.FrameBegin -= OnFrameBegin;
                _updateHandlerMono.FixedUpdated -= OnFixedUpdate;
                _updateHandlerMono.FrameEnd -= OnFrameEnd;
            }
            else
            {
                canContinueExecution = true;
            }
        }
    }
}