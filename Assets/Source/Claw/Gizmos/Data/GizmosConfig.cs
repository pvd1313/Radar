using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using UnityEngine;

namespace Beast.Claw.Gizmos
{
    [Serializable]
    public class GizmosConfig : ScriptableConfig
    {
        public Shader blitShader;
        public Shader vertexRender;
        public Mesh quad;

        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}