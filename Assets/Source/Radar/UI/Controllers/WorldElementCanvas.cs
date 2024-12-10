    using Beast.Claw.Attributes;
    using Beast.Claw.GameLoop;
    using Beast.Claw.Graphic;
    using Cysharp.Threading.Tasks;
    using Noesis;
    using Unity.Mathematics;

    namespace Beast.Radar.UI
    {
        public class WorldElementCanvas : Actor, IWorldElementCanvas
        {
            [Required] private ICameraTransform _camera;
            [Required] private IWindowManager _windowManager;
            
            private Canvas _canvas;

            protected override UniTask _warmup(ConcurrentContext context)
            {
                _canvas = new Canvas();
                
                _windowManager.Attach(_canvas, WindowOrder.WORLD_ELEMENT);
                
                return UniTask.CompletedTask;
            }
            
            public void UpdatePosition(UIElement element, float2 worldPosition, float2 offset)
            {
                float2 viewport = _camera.WorldToViewport(worldPosition);
                
                viewport.x *= _canvas.ActualWidth;
                
                viewport.y *= _canvas.ActualHeight;
                
                Canvas.SetLeft(element, viewport.x - offset.x);
            
                Canvas.SetBottom(element, viewport.y - offset.x);
            }

            public void AddElement(UIElement element)
            {
                _canvas.Children.Add(element);
            }
            
            public void UpdateZIndex(UIElement element, int zIndex)
            {
                if (_canvas.Children.Contains(element))
                {
                    Panel.SetZIndex(element, zIndex);
                }
            }
        }
    }