using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using UnityEngine;

namespace Beast.Claw.Gizmo
{
    [Serializable]
    public class GizmoConfig : ScriptableConfig
    {
        public Shader canvasShader;
        public Shader shapeShader;
        public Mesh quadMesh;
        public int entityTextureMaxSize;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}