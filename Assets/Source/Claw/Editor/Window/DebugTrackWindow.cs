using System;
using System.Collections.Generic;
using System.Linq;
using Beast.Claw.Collection;
using Beast.Claw.Debug;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor.Window
{
    public class DebugTrackWindow : EditorWindow
    {
        private readonly GUILayoutOption _orderButtonWidth = GUILayout.Width(EditorUI.OneCharButtonSize);
        private readonly GUILayoutOption _tickrateButtonWidth = GUILayout.Width(EditorUI.TwoCharButtonSize);
        
        [SerializeField] private bool _clearOnEnterPlayerMode;
        [SerializeField] private float2 _scrollPosition;
        [SerializeField] private UnitySerializableDictionary<string, bool> _folds = new();
        [SerializeField] private List<string> _drawOrder = new();
        [SerializeField] private int _updateTickRate = 3;
        
        private bool _isPlaying;
        private DateTime _lastUpdateTime;

        public DebugTrackWindow()
        {
            EditorApplication.update += _onEditorApplicationUpdate;
            _lastUpdateTime = DateTime.Now;
        }

        private void _onEditorApplicationUpdate()
        {
            if (!EditorApplication.isPlaying)
            {
                return;
            }
            
            DateTime now = DateTime.Now;
            if ((now - _lastUpdateTime).TotalSeconds >= 1f / _updateTickRate)
            {
                _lastUpdateTime = now;
                Repaint();
            }
        }

        protected override void InspectorUI()
        {
            if (_isPlaying != Application.isPlaying)
            {
                _isPlaying = Application.isPlaying;

                if (_isPlaying && _clearOnEnterPlayerMode)
                {
                    BDebug.ClearTrackStorage();
                }
            }
            
            flow.Horizontal(ui =>
            {
                ui.FlexibleSpace();
                
                float labelWidthBak = EditorGUIUtility.labelWidth;
                
                EditorGUIUtility.labelWidth = 100;

                ui.IntField("Fetch TickRate", ref _updateTickRate);
                
                EditorGUIUtility.labelWidth = labelWidthBak;

                flow.Option(_tickrateButtonWidth).Draw(ui =>
                {
                    ui.Button("3", () => _updateTickRate = 3);
                    ui.Button("10", () => _updateTickRate = 10);
                    ui.Button("60", () => _updateTickRate = 60);
                });
                
                ui.CheckBoxLeft("PlayMode Clear", ref _clearOnEnterPlayerMode);
            });

            flow.Horizontal(ui =>
            {
                ui.Button("Reset", _reset);
                ui.Button("Clear", BDebug.ClearTrackStorage);
                ui.Button("Open", () => _setAllFolds(true));
                ui.Button("Close", () => _setAllFolds(false));
            });
            
            flow.Draw(ui => ui.Space(EditorUI.MinorSpace));

            foreach (string storageName in BDebug.TrackStorages.Keys)
            {
                if (_folds.TryAdd(storageName, false))
                {
                    _drawOrder.Add(storageName);
                }
            }

            (int a, int b) swapIndex = default;

            flow.Vertical().Scroll(ref _scrollPosition, ui =>
            {
                for (int i = 0; i < _drawOrder.Count; i++)
                {
                    string storageName = _drawOrder[i];

                    _folds.TryGetValue(storageName, out bool foldState);

                    flow.Horizontal(ui =>
                    {
                        ui.Foldout(storageName, ref foldState);

                        flow.Option(_orderButtonWidth, ui =>
                        {
                            ui.Button(EditorUI.CharUpArrow, () => swapIndex = (i, i - 1));
                            ui.Button(EditorUI.CharDownArrow, () => swapIndex = (i, i + 1));
                        });
                    });

                    _folds[storageName] = foldState;

                    flow.If(foldState).Indent(ui =>
                    {
                        BDebug.TrackStorages.TryGetValue(storageName, out DebugTrackStorage trackStorage);
                        _trackStorageUI(ui, trackStorage);
                        ui.Space(EditorUI.MinorSpace);
                    });
                }
            });

            _drawOrder.TrySwap(swapIndex.a, swapIndex.b);
        }

        private void _trackStorageUI(EditorUI ui, DebugTrackStorage storage)
        {
            if (storage == null)
            {
                ui.Label("-- NO DATA --");
                return;
            }
            
            foreach ((string id, float value) in storage.ReadAllFloat())
            {
                ui.FloatField(id, value);
            }
            
            foreach ((string id, int value) in storage.ReadAllInt())
            {
                ui.IntField(id, value);
            }
            
            foreach ((string id, float2 value) in storage.ReadAllFloat2())
            {
                ui.Float2FieldOneLine(id, value);
            }
            
            foreach ((string id, float3 value) in storage.ReadAllFloat3())
            {
                ui.Float3Field(id, value);
            }
            
            foreach ((string id, float4 value) in storage.ReadAllFloat4())
            {
                ui.Float4Field(id, value);
            }
            
            foreach ((string id, string value) in storage.ReadAllString())
            {
                ui.StringField(id, value);
            }
        }

        private void _reset()
        {
            _drawOrder.Clear();
            _folds.Clear();
        }

        private void _setAllFolds(bool isOpen)
        {
            foreach (string id in _folds.Keys.ToList())
            {
                _folds[id] = isOpen;
            }
        }
        
        protected override void Dispose()
        {
            EditorApplication.update -= _onEditorApplicationUpdate;
        }
    }
}