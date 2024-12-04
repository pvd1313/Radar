using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Graphic;
using UnityEngine;

namespace Beast.Claw.Graphy
{
    public class GraphyConfig : ScriptableConfig
    {
        public Shader shader;
        public RenderTextureData renderTextureData;
        public EMode graphyMode = EMode.FULL;
        
        
        [Space, Header("Fps")]
        public EModuleState fpsModuleState = EModuleState.FULL;
        [Space]
        public Color goodFpsColor = new Color32(118, 212, 58, 255);
        public int goodFpsThreshold = 60;
        [Space]
        public Color cautionFpsColor = new Color32(243, 232, 0, 255);
        public int cautionFpsThreshold = 30;
        [Space]
        public Color criticalFpsColor = new Color32(220, 41, 30, 255);
        [Space]
        [Range(10, 300)] public int fpsGraphResolution = 150;
        [Range(1, 200)] public int fpsTextUpdateRate = 3; // 3 updates per sec.
        
        
        [Space, Header("Ram")]
        public EModuleState ramModuleState = EModuleState.FULL;
        public Color allocatedRamColor = new Color32(255, 190, 60, 255);
        public Color reservedRamColor = new Color32(205, 84, 229, 255);
        public Color monoRamColor = new Color(0.3f, 0.65f, 1f, 1);
        [Range(10, 300)] public int ramGraphResolution = 150;
        [Range(1, 200)] public int ramTextUpdateRate = 3; // 3 updates per sec.
        
        
        [Space, Header("Audio")]
        public EModuleState audioModuleState = EModuleState.FULL;
        public Color audioColor = Color.white;
        [Range(10, 300)] public int audioGraphResolution = 81;
        [Range(1, 200)] public int audioTextUpdateRate = 3; // 3 updates per sec.
        public FFTWindow FFTWindow = FFTWindow.Blackman;
        [Tooltip("Must be a power of 2 and between 64-8192")]
        public int spectrumSize = 512;

        
        [Space, Header("Advanced")]
        public EModuleState advancedModuleState = EModuleState.FULL;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}