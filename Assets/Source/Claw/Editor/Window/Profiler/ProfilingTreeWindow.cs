namespace Beast.Claw.Editor.Window.Profiler
{
    public class ProfilingTreeWindow : UnityEditor.EditorWindow
    {
        // private Stopwatch _stopwatch;
        // private List<ProfilerNode> _nodes;
        //
        // private int _offset;
        // private int _yScroll;
        // private ProfilingNodesView _view;

        private void OnGUI()
        {
            // var profilingManager = SceneContextUtil.TryResolve<I>();
            // if (profilingManager is null)
            // {
            //     return;
            // }
            //
            // var currentEvent = Event.current;
            // if (currentEvent.isScrollWheel)
            // {
            //     _yScroll = (int) currentEvent.delta.y;
            //     return;
            // }
            //
            // _view ??= new ProfilingNodesView();
            //
            // _stopwatch ??= new Stopwatch();
            // _stopwatch.Reset();
            // _stopwatch.Start();
            //
            // DrawTreeJobSafe(profilingManager.Reader);
            //
            // _stopwatch.Stop();
            //
            // SpaceDebug.LogState("DrawTree ms", _stopwatch.ElapsedTicks / (float) Stopwatch.Frequency * 1000);
            
            // Repaint();
        }

        // private void DrawTreeJobSafe(ProfilingTreeReader tree)
        // {
        //     var currentEvent = Event.current;
        //
        //     if (currentEvent.type == EventType.Layout)
        //     {
        //         _nodes ??= new List<ProfilingNode>();
        //         var displayCount = position.height / 20;
        //         tree.Read(_offset,  (int) Math.Ceiling(displayCount), _nodes, out var totalNodeCount);
        //
        //         var lastNode = Math.Max(0, totalNodeCount - (int) Math.Floor(displayCount));
        //         _offset = Mathf.Clamp(_offset + _yScroll, 0, lastNode);
        //         _yScroll = 0;
        //     }
        //
        //     var rect = EditorGUILayout.GetControlRect(false);
        //     
        //     _view.OnGUI(rect, _nodes);
        // }
    }
}