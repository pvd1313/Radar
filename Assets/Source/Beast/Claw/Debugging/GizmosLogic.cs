using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.Debugging
{
    public class GizmosLogic : Initializable
    {
        private readonly List<IGizmos> _drawers;

        public GizmosLogic (List<IGizmos> drawers)
        {
            _drawers = drawers;
        }
        
        public async UniTask<InitializationResult> Initialize()
        {
            GizmosMono mono = SceneUtil.NewObjectFor<GizmosLogic>().AddComponent<GizmosMono>();
            mono.GizmosDrawn += OnGizmosDraw;

            return InitializationResult.Success();
        }

        private void OnGizmosDraw()
        {
            foreach (IGizmos entity in _drawers)
            {
                entity.OnDrawGizmos();
            }
        }
    }
}