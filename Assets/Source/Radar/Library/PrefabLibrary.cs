using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Radar.Cinematic;
using Beast.Radar.InputHandling;
using Beast.Radar.Player;
using Cysharp.Threading.Tasks;

namespace Beast.Radar
{
    public class PrefabLibrary : ScriptableConfig
    {
        public CameraView cameraPrefab;
        public PlayerInputView playerInputPrefab;
        public PlayerView playerViewPrefab;
        public UIPrefabLibrary ui;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield return ui;
        }
    }
}