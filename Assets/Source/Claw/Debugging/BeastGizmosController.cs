using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.Debugging
{
    public class BeastGizmosController : IBeastInitialize
    {
        private readonly List<IBeastGizmo> _drawers;
        private BeastGizmosHandlerMono _mono;
        
        public BeastGizmosController (List<IBeastGizmo> drawers)
        {
            _drawers = drawers;
        }
        
        public UniTask<BeastInitializationResult> Initialize()
        {
            BeastGizmosHandlerMono beastGizmosHandlerMono = BeastScene.NewGameObject(this).AddComponent<BeastGizmosHandlerMono>();
            beastGizmosHandlerMono.GizmosDrawn += OnGizmosDraw;

            return BeastInitializationResult.SuccessNotAsync();
        }

        private void OnGizmosDraw()
        {
            foreach (IBeastGizmo entity in _drawers)
            {
                try
                {
                    entity.OnDrawGizmos();
                }
                catch (Exception e)
                {
                    _mono.GizmosDrawn -= OnGizmosDraw;
                    throw new BeastException(e.ToString());
                }
            }
        }
    }
}