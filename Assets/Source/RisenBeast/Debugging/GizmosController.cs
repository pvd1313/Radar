using System.Collections;
using System.Collections.Generic;
using RisenBeast.Common;
using UnityEngine;

namespace RisenBeast.Debugging
{
    public class GizmosController
    {
        private readonly List<IGizmosDrawer> _gizmosDrawers;

        public GizmosController (List<IGizmosDrawer> gizmosDrawers)
        {
            _gizmosDrawers = gizmosDrawers;
        }
        
        public IEnumerator Initialize()
        {
            GizmosBehaviour behaviour = SceneUtil.NewObjectFor<GizmosController>().AddComponent<GizmosBehaviour>();
            behaviour.GizmosDrawn += OnGizmosDraw;
            
            yield break;
        }

        private void OnGizmosDraw()
        {
            foreach (IGizmosDrawer entity in _gizmosDrawers)
            {
                entity.OnDrawGizmos();
            }
        }
    }
}