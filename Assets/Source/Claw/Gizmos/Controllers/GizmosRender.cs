using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Claw.Mathematics;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Gizmos
{
    public class GizmosRender : Controller, IGizmosManager, IGameCycleListener
    {
        public int RenderVersion { get; private set; }
        
        private const int BUFFER_SIZE = 256;
        
        private readonly ICameraTransform _camera;
        private readonly List<IGizmosDrawer> _drawControllers;
        private readonly IScreen _screen;
        private readonly GizmosConfig _config;

        private RenderTexture _texture;
        private List<GizmosHandle> _handles;
        private Material _blitMaterial;
        private Material _vertexMaterial;

        private ShapeStorage<Line> _wireLines;
        private ShapeStorage<Rect> _wireRects;
        private ShapeStorage<Polygon> _wirePolygons;
        private ShapeStorage<Square> _wireSquares;
        private ShapeStorage<Square> _screenSquares;

        public GizmosRender(
            ICameraTransform camera, 
            List<IGizmosDrawer> drawers,
            IScreen screen,
            GizmosConfig config)
        {
            _camera = camera;
            _drawControllers = drawers;
            _screen = screen;
            _config = config;

            GizmosStateStorage.Initialize();
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
            handle.ListenBlocking(ActionID.RENDER, _render);
            handle.ListenBlocking(ActionID.DISPOSE, _dispose);
        }
        
        private UniTask _warmup(ConcurrentContext context)
        {
            _handles = new List<GizmosHandle>(_drawControllers.Count);
            
            foreach (IGizmosDrawer drawer in _drawControllers)
            {
                _handles.Add(GetHandle(drawer, drawer.GizmosColor));
            }

            _blitMaterial = BGraphic.CreateMaterial(this, _config.blitShader);
            _vertexMaterial = BGraphic.CreateMaterial(this, _config.vertexRender);
            
            _wireLines = new ShapeStorage<Line>(BUFFER_SIZE);
            _wireRects = new ShapeStorage<Rect>(BUFFER_SIZE);
            _wirePolygons = new ShapeStorage<Polygon>(BUFFER_SIZE);
            _wireSquares = new ShapeStorage<Square>(BUFFER_SIZE);
            _screenSquares = new ShapeStorage<Square>(BUFFER_SIZE);

            return UniTask.CompletedTask;
        }

        public GizmosHandle GetHandle(IController owner, string handleName, Color defaultColor)
        {
            owner.ValidateNotNullThrowException(nameof(owner));
            handleName.ValidateNotEmptyOrNullThrowException(nameof(handleName));
            
            handleName = owner.Meta.name + "_" + handleName;
            
            ushort id = GizmosStateStorage.GetOrCreateHandleId(handleName, defaultColor);
            
            return new GizmosHandle(id, this);
        }

        public GizmosHandle GetHandle(IController owner, Color defaultColor)
        {
            ushort id = GizmosStateStorage.GetOrCreateHandleId(owner.Meta.name, defaultColor);
            
            return new GizmosHandle(id, this);
        }

        private void _render(BlockingContext context)
        {
            if (_texture == null || 
                _texture.width != _screen.Width || 
                _texture.height != _screen.Height)
            {
                BGraphic.TryDispose(ref _texture);
            
                _texture = BGraphic.CreatePermRenderTexture(this, new RenderTextureData
                {
                    format = RenderTextureFormat.ARGB32,
                    texture = new PlanarTextureData
                    {
                        filterMode = FilterMode.Point,
                        wrapMode = TextureWrapMode.Clamp,
                        size = _screen.Size
                    },
                    temp = new TempRenderTextureData
                    {
                        antiAliasing = 1
                    }
                });
            }
            
            for (int i = 0; i < _drawControllers.Count; i++)
            {
                _drawControllers[i].Draw(_handles[i]);
            }

            Graphics.SetRenderTarget(_texture);
            
            GL.Clear(true, true, Color.clear);

            _vertexMaterial.SetPass(0);
            
            GizmosScreenDrawer.BeginDraw(_screen);
            GizmosScreenDrawer.DrawSquares(_camera, _screenSquares);
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

            _blitMaterial.SetTexture("_Texture", _texture);

            Graphics.SetRenderTarget(null);
            
            BGraphic.DrawMeshDelayed(new DrawMeshTask
            {
                material = _blitMaterial,
                matrix = new Float4x4TRS
                {
                    rotation = Quaternion.identity,
                    scale = new float3(_screen.Aspect, 1, 1),
                    translation = new float3(0, 0, RenderOrderZ.Gizmos)
                },
                mesh = _config.quad
            });
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

            BGraphic.TryDispose(ref _texture);
            BGraphic.TryDispose(ref _blitMaterial);
        }
    }
}