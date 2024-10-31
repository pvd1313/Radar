using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.Gizmos
{
    public class GizmosDrawerManager : Controller, IGizmosDrawerManager, IGameCycleListener
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
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
            handle.ListenBlocking(ActionID.TICK, _tick);
        }

        private UniTask _warmup(ConcurrentContext context)
        {
            _drawers = new Dictionary<IGizmosDrawer, DrawerInfo>();
            
            foreach (IGizmosDrawer drawer in _initialDrawers)
            {
                AddDrawer(drawer);
            }

            return UniTask.CompletedTask;
        }

        private void _tick(BlockingContext context)
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