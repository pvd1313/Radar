using System.Collections;
using System.Collections.Generic;
using Beast.Claw.Common;

namespace Beast.Claw.Debugging
{
    public class GizmosLogic
    {
        private readonly List<IGizmos> _gizmosDrawers;

        public GizmosLogic (List<IGizmos> gizmosDrawers)
        {
            _gizmosDrawers = gizmosDrawers;
        }
        
        public IEnumerator Initialize()
        {
            GizmosMono mono = SceneUtil.NewObjectFor<GizmosLogic>().AddComponent<GizmosMono>();
            mono.GizmosDrawn += OnGizmosDraw;
            
            yield break;
        }

        private void OnGizmosDraw()
        {
            foreach (IGizmos entity in _gizmosDrawers)
            {
                entity.OnDrawGizmos();
            }
        }
    }
}