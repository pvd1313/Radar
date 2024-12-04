using Beast.Claw.Common;
using Beast.Claw.GameLoop;
using Beast.Claw.Graphic;
using Beast.Claw.Graphy.Modules;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Claw.Graphy
{
    public class RadarGraphyManager : Actor, IGraphyManager, IReady 
    {
        public FpsMonitor FpsMonitor { get; private set; }
        public FpsRender FpsRender { get; private set;}
        public InformationMonitor InformationMonitor { get; private set; }
        public AudioMonitor AudioMonitor { get; private set; }
        public AudioRender AudioRender { get; private set; }

        public bool IsReady { get; private set; }
        
        private readonly GraphyConfig _config;
        private readonly ICamera _camera;

        private bool _active = true;
        private bool _focused = true;
        

        protected RadarGraphyManager(GraphyConfig config, ICamera camera)
        {
            _config = config;
            _camera = camera;
        }
        
        protected override UniTask _warmup(ConcurrentContext context)
        {
            FpsMonitor = new FpsMonitor();
            FpsMonitor.Initialize();
         
            FpsRender = new FpsRender(_config, FpsMonitor);
            FpsRender.Initialize();

            InformationMonitor = new InformationMonitor();
            InformationMonitor.Initialize();

            AudioMonitor = new AudioMonitor(_config, _camera.Camera.GetComponent<AudioListener>());
            AudioMonitor.Initialize();
           
            AudioRender = new AudioRender(_config, AudioMonitor);
            AudioRender.Initialize();
            
            Initialize();

            return UniTask.CompletedTask;
        }

        protected override void _tick(BlockingContext context)
        {
            if (!IsReady)
            {
                return;
            }
            
            FpsMonitor.Update();
            FpsRender.Update();
            InformationMonitor.Update();
            AudioMonitor.Update();
            AudioRender.Update();
        }
        
        protected override void _dispose(BlockingContext obj)
        {
            FpsRender.Dispose();
            AudioRender.Dispose();
        }

        public void SetModuleMode(EModuleType moduleType, EModuleState moduleState)
        {
            /*switch (moduleType)
            {
                case EModuleType.FPS:
                    m_fpsManager.SetState(moduleState);
                    break;

                case EModuleType.RAM:
                    m_ramManager.SetState(moduleState);
                    break;

                case EModuleType.AUDIO:
                    m_audioManager.SetState(moduleState);
                    break;

                case EModuleType.ADVANCED:
                    m_advancedData.SetState(moduleState);
                    break;
            }*/
        }

        public void ToggleModes()
        {
            /*if ((int)m_modulePresetState >= Enum.GetNames(typeof(EModulePreset)).Length - 1)
            {
                m_modulePresetState = 0;
            }
            else
            {
                m_modulePresetState++;
            }

            SetPreset(m_modulePresetState);*/
        }

        public void SetPreset(EModulePreset modulePreset)
        {
            //m_modulePresetState = modulePreset;

            /*  switch (m_modulePresetState)
              {
                  case EModulePreset.FPS_BASIC:
                      m_fpsManager.SetState(EModuleState.BASIC);
                      m_ramManager.SetState(EModuleState.OFF);
                      m_audioManager.SetState(EModuleState.OFF);
                      m_advancedData.SetState(EModuleState.OFF);
                      break;
  
                  case EModulePreset.FPS_TEXT:
                      m_fpsManager.SetState(EModuleState.TEXT);
                      m_ramManager.SetState(EModuleState.OFF);
                      m_audioManager.SetState(EModuleState.OFF);
                      m_advancedData.SetState(EModuleState.OFF);
                      break;
  
                  case EModulePreset.FPS_FULL:
                      m_fpsManager.SetState(EModuleState.FULL);
                      m_ramManager.SetState(EModuleState.OFF);
                      m_audioManager.SetState(EModuleState.OFF);
                      m_advancedData.SetState(EModuleState.OFF);
                      break;
  
                  case EModulePreset.FPS_TEXT_RAM_TEXT:
                      m_fpsManager.SetState(EModuleState.TEXT);
                      m_ramManager.SetState(EModuleState.TEXT);
                      m_audioManager.SetState(EModuleState.OFF);
                      m_advancedData.SetState(EModuleState.OFF);
                      break;
  
                  case EModulePreset.FPS_FULL_RAM_TEXT:
                      m_fpsManager.SetState(EModuleState.FULL);
                      m_ramManager.SetState(EModuleState.TEXT);
                      m_audioManager.SetState(EModuleState.OFF);
                      m_advancedData.SetState(EModuleState.OFF);
                      break;
  
                  case EModulePreset.FPS_FULL_RAM_FULL:
                      m_fpsManager.SetState(EModuleState.FULL);
                      m_ramManager.SetState(EModuleState.FULL);
                      m_audioManager.SetState(EModuleState.OFF);
                      m_advancedData.SetState(EModuleState.OFF);
                      break;
  
                  case EModulePreset.FPS_TEXT_RAM_TEXT_AUDIO_TEXT:
                      m_fpsManager.SetState(EModuleState.TEXT);
                      m_ramManager.SetState(EModuleState.TEXT);
                      m_audioManager.SetState(EModuleState.TEXT);
                      m_advancedData.SetState(EModuleState.OFF);
                      break;
  
                  case EModulePreset.FPS_FULL_RAM_TEXT_AUDIO_TEXT:
                      m_fpsManager.SetState(EModuleState.FULL);
                      m_ramManager.SetState(EModuleState.TEXT);
                      m_audioManager.SetState(EModuleState.TEXT);
                      m_advancedData.SetState(EModuleState.OFF);
                      break;
  
                  case EModulePreset.FPS_FULL_RAM_FULL_AUDIO_TEXT:
                      m_fpsManager.SetState(EModuleState.FULL);
                      m_ramManager.SetState(EModuleState.FULL);
                      m_audioManager.SetState(EModuleState.TEXT);
                      m_advancedData.SetState(EModuleState.OFF);
                      break;
  
                  case EModulePreset.FPS_FULL_RAM_FULL_AUDIO_FULL:
                      m_fpsManager.SetState(EModuleState.FULL);
                      m_ramManager.SetState(EModuleState.FULL);
                      m_audioManager.SetState(EModuleState.FULL);
                      m_advancedData.SetState(EModuleState.OFF);
                      break;
  
                  case EModulePreset.FPS_FULL_RAM_FULL_AUDIO_FULL_ADVANCED_FULL:
                      m_fpsManager.SetState(EModuleState.FULL);
                      m_ramManager.SetState(EModuleState.FULL);
                      m_audioManager.SetState(EModuleState.FULL);
                      m_advancedData.SetState(EModuleState.FULL);
                      break;
  
                  case EModulePreset.FPS_BASIC_ADVANCED_FULL:
                      m_fpsManager.SetState(EModuleState.BASIC);
                      m_ramManager.SetState(EModuleState.OFF);
                      m_audioManager.SetState(EModuleState.OFF);
                      m_advancedData.SetState(EModuleState.FULL);
                      break;
  
                  default:
                      BDebug.LogError(this, "SetPreset - Tried to set a preset that is not supported.");
                      break;
              }*/
        }

        public void ToggleActive()
        {
            if (!_active)
            {
                Enable();
            }
            else
            {
                Disable();
            }
        }

        public void Enable()
        {
            /*if (!m_active)
            {
                if (m_initialized)
                {
                    m_fpsManager.RestorePreviousState();
                    m_ramManager.RestorePreviousState();
                    m_audioManager.RestorePreviousState();
                    m_advancedData.RestorePreviousState();

                    m_active = true;
                }
                else
                {
                    Init();
                }
            }*/
        }

        public void Disable()
        {
            /*if (m_active)
            {
                m_fpsManager.SetState(EModuleState.OFF);
                m_ramManager.SetState(EModuleState.OFF);
                m_audioManager.SetState(EModuleState.OFF);
                m_advancedData.SetState(EModuleState.OFF);

                m_active = false;
            }*/
        }
        
        private void Initialize()
        {
            /*m_fpsManager.SetState(_graphyConfig.fpsModuleState);
            m_ramManager.SetState(_graphyConfig.ramModuleState);
            m_audioManager.SetState(_graphyConfig.audioModuleState);
            m_advancedData.SetState(_graphyConfig.advancedModuleState);
            
            m_initialized = true;*/

            IsReady = true;
        }

        private void UpdateAllParameters()
        {
            /*m_fpsManager.UpdateParameters();
            m_ramManager.UpdateParameters();
            m_audioManager.UpdateParameters();
            m_advancedData.UpdateParameters();*/
        }

        private void RefreshAllParameters()
        {
            /*m_fpsManager.RefreshParameters();
            m_ramManager.RefreshParameters();
            m_audioManager.RefreshParameters();
            m_advancedData.RefreshParameters();*/
        }



    }
}
