using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.Debugging
{
    public class GizmosLogic : Initializable
    {
        private readonly List<IGizmos> _drawers;
        private GizmosMono _gizmosMono;
        
        public GizmosLogic (List<IGizmos> drawers)
        {
            _drawers = drawers;
        }
        
        public UniTask<InitializationResult> Initialize()
        {
            GizmosMono _gizmosMono = BeastScene.NewGameObject(this).AddComponent<GizmosMono>();
            _gizmosMono.GizmosDrawn += OnGizmosDraw;

            return InitializationResult.SuccessNotAsync();
        }

        private void OnGizmosDraw()
        {
            foreach (IGizmos entity in _drawers)
            {
                try
                {
                    entity.OnDrawGizmos();
                }
                catch (Exception e)
                {
                    _gizmosMono.GizmosDrawn -= OnGizmosDraw;
                    throw new BeastException(e.ToString());
                }
            }
        }
    }
}