using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Claw.Gizmos
{
    public class GizmosManager : Controller, IGizmosManager, IGameCycleListener
    {
        public int RenderVersion { get; private set; }
        
        private const int BUFFER_SIZE = 256;
        
        private readonly GizmosConfig _config;
        private readonly ICameraTransform _camera;

        private Material _material;
        private ShapeStorage<Line> _wireLines;
        private ShapeStorage<Rect> _wireRects;
        private ShapeStorage<Polygon> _wirePolygons;
        private ShapeStorage<Square> _wireSquares;
        private ShapeStorage<Square> _screenSquares;

        public GizmosManager(GizmosConfig config, ICameraTransform camera)
        {
            _config = config;
            _camera = camera;
            GizmosStateStorage.Initialize();
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
            handle.ListenBlocking(ActionID.GIZMOS_DRAW, _gizmosDraw);
            handle.ListenBlocking(ActionID.DISPOSE, _dispose);
        }
        
        private UniTask _warmup(ConcurrentContext context)
        {
            _wireLines = new ShapeStorage<Line>(BUFFER_SIZE);
            _wireRects = new ShapeStorage<Rect>(BUFFER_SIZE);
            _wirePolygons = new ShapeStorage<Polygon>(BUFFER_SIZE);
            _wireSquares = new ShapeStorage<Square>(BUFFER_SIZE);
            _screenSquares = new ShapeStorage<Square>(BUFFER_SIZE);
            _material = new Material(_config.Shader);

            return UniTask.CompletedTask;
        }

        public GizmosHandle GetHandle(object owner, string name, Color defaultColor)
        {
            ushort id = GizmosStateStorage.GetOrCreateHandleId(owner.GetType().FullName + "(" + name + ")", defaultColor);
            
            return new GizmosHandle(id, this);
        }

        public GizmosHandle GetHandle(object owner, Color defaultColor)
        {
            ushort id = GizmosStateStorage.GetOrCreateHandleId(owner.GetType().FullName, defaultColor);
            
            return new GizmosHandle(id, this);
        }

        private void _gizmosDraw(BlockingContext context)
        {
            _material.SetPass(0);
            
            GizmosScreenDrawer.BeginDraw(_camera);
            GizmosScreenDrawer.DrawSquares(_screenSquares);
            GizmosScreenDrawer.EndDraw();
            
            _screenSquares.Clear();
            
            GizmosWireDrawer.BeginDraw();
            GizmosWireDrawer.DrawLines(_wireLines);
            GizmosWireDrawer.DrawRects(_wireRects);
            GizmosWireDrawer.DrawPolygons(_wirePolygons);
            GizmosWireDrawer.DrawSquares(_wireSquares);
            GizmosWireDrawer.EndDraw();
            
            _wireLines.Clear();
            _wireRects.Clear();
            _wirePolygons.Clear();
            _wireSquares.Clear();

            RenderVersion++;
        }

        internal void ScheduleLineDraw(Line line) => _wireLines.Add(line);
        internal void ScheduleWireRectDraw(Rect rect) => _wireRects.Add(rect);
        internal void ScheduleWirePolygonDraw(Polygon polygon) => _wirePolygons.Add(polygon);
        internal void ScheduleWireSquareDraw(Square square) => _wireSquares.Add(square);
        internal void ScheduleScreenSquareDraw(Square square) => _screenSquares.Add(square);

        private void _dispose(BlockingContext context)
        {
            _wireLines.Dispose();
            _wireRects.Dispose();
            _wirePolygons.Dispose();
            _wireSquares.Dispose();
            _screenSquares.Dispose();
            GizmosStateStorage.Dispose();
            Object.Destroy(_material);
        }
    }
}