using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Radar.Cinematic;
using Beast.Radar.UI;

namespace Beast.Radar
{
    public class EssentialPrefabLibrary : ScriptableConfig
    {
        public CameraView camera;
        public LegacyCanvasView canvas;
        public NoesisMonoRender noesisRender;
        public NoesisXaml canvasXaml;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}