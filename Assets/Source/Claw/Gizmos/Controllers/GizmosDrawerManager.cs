using System;
using System.Collections.Generic;
using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.Gizmos
{
    public class GizmosDrawerManager : IGizmosDrawerManager, IGameCycleListener
    {
        private struct DrawerInfo
        {
            public IGizmosDrawer drawer;
            public GizmosHandle handle;
        }
        
        private readonly List<IGizmosDrawer> _initialDrawers;
        private readonly IGizmosManager _gizmosHandleFactory;

        private Dictionary<IGizmosDrawer, DrawerInfo> _drawers;

        public GizmosDrawerManager(List<IGizmosDrawer> initialDrawers, IGizmosManager gizmosHandles)
        {
            _initialDrawers = initialDrawers;
            _gizmosHandleFactory = gizmosHandles;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(OperationID.WARMUP, _warmup);
            handle.ListenUpdate(UpdateID.TICK, _tick);
        }

        private UniTask<OperationResult> _warmup(OperationContext context)
        {
            _drawers = new Dictionary<IGizmosDrawer, DrawerInfo>();
            
            foreach (IGizmosDrawer drawer in _initialDrawers)
            {
                AddDrawer(drawer);
            }
            return OperationResult.SuccessSyncronous();
        }

        private void _tick(UpdateContext context)
        {
            foreach (DrawerInfo info in _drawers.Values)
            {
                info.drawer.Draw(info.handle); 
            }
        }

        public void AddDrawer(IGizmosDrawer drawer)
        {
            if (_drawers.ContainsKey(drawer))
            {
                throw new InvalidOperationException($"Drawer ({drawer.GetType()}) was already added");
            }  
            
            _drawers.Add(drawer, new DrawerInfo
            {
                drawer = drawer,
                handle = _gizmosHandleFactory.GetHandle(drawer, drawer.GizmosColor)
            });
        }

        public void RemoveDrawer(IGizmosDrawer drawer)
        {
            if (!_drawers.Remove(drawer))
            {
                throw new InvalidOperationException($"Drawer ({drawer.GetType()}) was not added to be removed");
            } 
        }
    }
}