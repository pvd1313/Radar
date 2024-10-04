using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameCycle
{
    public class BeastGameCycleLogic : Zenject.IInitializable
    {
        private readonly BeastGameCycleConfig _config;
        private readonly List<IBeastInitialize> _init;
        private readonly List<IBeastFrameBegin> _frameBegin;
        private readonly List<IBeastFixedUpdate> _fixedUpdate;
        private readonly List<IBeastFrameEnd> _frameEnd;

        private HashSet<object> _brokenLogic;
        private Dictionary<object, int> _errors;
        private BeastUpdateHandlerMono _beastUpdateHandlerMono;

        public BeastGameCycleLogic (
            BeastGameCycleConfig config,
            List<IBeastInitialize> init, 
            List<IBeastFrameBegin> frameBegin,
            List<IBeastFixedUpdate> fixedUpdate,
            List<IBeastFrameEnd> frameEnd)
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
            
            foreach (IBeastInitialize controller in _init)
            {
                BeastInitializationResult result = await controller.Initialize();

                if (result.status != EBeastInitializationStatus.Success)
                {
                    BLog.Error(this, $"Failed to initialize {controller.GetType()}");
                     
                    return;
                }
            }
            
            _beastUpdateHandlerMono = BeastScene.NewGameObject(this).AddComponent<BeastUpdateHandlerMono>();
            _beastUpdateHandlerMono.FrameBegin += OnFrameBegin;
            _beastUpdateHandlerMono.FixedUpdated += OnFixedBeastUpdate;
            _beastUpdateHandlerMono.FrameEnd += OnFrameEnd;
        }
        
        private void OnFrameBegin()
        {
            bool hasBrokenLogic = _brokenLogic.Count > 0;
            
            foreach (IBeastFrameBegin logic in _frameBegin)
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

        private void OnFixedBeastUpdate()
        {
            bool hasBrokenLogic = _brokenLogic.Count > 0;
            
            foreach (IBeastFixedUpdate logic in _fixedUpdate)
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
            
            foreach (IBeastFrameEnd logic in _frameEnd)
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
            BLog.Exception(this, e, logic.GetType() + "." + methodName + "()");
            
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
                
                _beastUpdateHandlerMono.FrameBegin -= OnFrameBegin;
                _beastUpdateHandlerMono.FixedUpdated -= OnFixedBeastUpdate;
                _beastUpdateHandlerMono.FrameEnd -= OnFrameEnd;
            }
            else
            {
                canContinueExecution = true;
            }
        }
    }
}