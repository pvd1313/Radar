using System;
using System.Collections.Generic;
using Beast.Claw.Collections;
using Beast.Claw.GameLoop;
using Beast.Claw.Profiling;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Editor.Window.Profiler
{
    public class ProfilerWindow : EditorWindow
    {
        [SerializeField] private RepaintTickRateBar _tickRateBar;
        [SerializeField] private string _selectedProfilerName;
        [SerializeField] private UnitySerializableDictionary<string, int> _profilerNameToIndex;
        [SerializeField] private float2[] _elementSizes;
        [SerializeField] private float2[] _elementPositions;
        [SerializeField] private int _selectedProfilerIndex;
        [SerializeField] private float2 _scrollPosition;

        private Style _buttonStyleNormal;
        private Style _labelStyle;
        private Style _buttonStyleSelected;
        private InjectionReference<IGameLoop> _gameCycle;
        private ProfilerTreeGUI _treeView;
        private List<ProfilerNode> _nodesBuffer;

        private string _lastDrawnProfilerName;

        public ProfilerWindow()
        {
            _tickRateBar = new RepaintTickRateBar(this);
            _profilerNameToIndex = new UnitySerializableDictionary<string, int>();
            _nodesBuffer = new List<ProfilerNode>();
            _selectedProfilerIndex = -1;
        }
        
        protected override void Warmup()
        {
            _gameCycle = new InjectionReference<IGameLoop>();
            _treeView = new ProfilerTreeGUI();
            _buttonStyleNormal = _rectUI.Style.Button;
            _labelStyle = _rectUI.Style.Label;
            _buttonStyleSelected = _rectUI.Style.Button.Clone();
            _buttonStyleSelected.Scheme.normal.textColor = Color.yellow;
            _tickRateBar.Warmup();
        }

        protected override void Reset()
        {
            _profilerNameToIndex.Clear();
        }

        protected override void DrawUI()
        {
            _flow.Begin().Horizontal().Draw(ui =>
            {
                _tickRateBar.Draw(_flow);
                
                ui.FlexibleSpace();
            });

            if (_gameCycle.Reference == null)
            {
                _flow.Begin().InfoBox("GameCycle not found.");
                
                return;
            }

            if (_gameCycle.Reference.AllOperations.Count == 0)
            {
                _flow.Begin().InfoBox("No operations found in GameCycle");
            }

            _tryRebuildProfilers();

            _flow.Begin().Draw(ui =>
            {
                FlexGrid flex = new (new FlexGrid.Input
                {
                    elementSizes = new Span<float2>(_elementSizes),
                    outputPositions = new Span<float2>(_elementPositions),
                    screenSize = position.size
                });

                Rect uiRect = ui.AllocateRect(flex.TotalSize);

                IProfiler selectedProfiler = null;

                foreach (string operationName in _gameCycle.Reference.AllOperations)
                {
                    int profilerIndex = _profilerNameToIndex[operationName];
                    Style buttonStyle = _buttonStyleNormal;

                    IProfiler profiler = _gameCycle.Reference.TryGetProfiler(operationName);

                    if (profilerIndex == _selectedProfilerIndex)
                    {
                        selectedProfiler = profiler;
                        buttonStyle = _buttonStyleSelected;
                    }

                    Rect buttonRect = flex[profilerIndex].AddPosition(uiRect.position);
                    
                    if (_rectUI.Button(buttonRect, operationName, buttonStyle))
                    {
                        selectedProfiler = profiler;
                        _selectedProfilerIndex = profilerIndex;
                    }
                }

                _flow.Begin().If(selectedProfiler != null).Scroll(ref _scrollPosition).Draw(ui =>
                {
                    ProfilerTreeReader reader = selectedProfiler.Reader;

                    selectedProfiler.Reader.Read(0, reader.NodeCount, _nodesBuffer, out _);

                    Rect rect = ui.AllocateRect(new float2(position.width - 15, 20 * _nodesBuffer.Count));
                    rect.width -= 4;
                    
                    _treeView.OnGUI(rect, _nodesBuffer);
                });
            });
        }

        private void _tryRebuildProfilers()
        {
            bool needsRebuild = false;
            
            foreach (string profilerName in _gameCycle.Reference.AllOperations)
            {
                if (!_profilerNameToIndex.ContainsKey(profilerName))
                {
                    needsRebuild = true;
                    break;
                }
            }

            if (!needsRebuild)
            {
                return;
            }

            int profilerCount = _gameCycle.Reference.AllOperations.Count;
            _profilerNameToIndex.Clear();

            _selectedProfilerIndex = -1;
            _elementSizes = new float2[profilerCount];
            _elementPositions = new float2[profilerCount];
            int i = 0;
            
            foreach (string profilerName in _gameCycle.Reference.AllOperations)
            {
                _elementSizes[i] = _labelStyle.CalcSize(profilerName) + new float2(10, 5);
                _profilerNameToIndex[profilerName] = i;

                i++;
            }
        }

        protected override void Dispose()
        {
            _tickRateBar.Dispose();
        }
    }
}