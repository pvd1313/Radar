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
        [SerializeField] private bool _clearOnEnterPlayerMode;
        [SerializeField] private float2 _scrollPosition;
        [SerializeField] private UnitySerializableDictionary<string, bool> _folds;
        [SerializeField] private GUILayoutOption[] _orderButtonWidth;
        [SerializeField] private GUILayoutOption[] _controlButtonWidth;
        [SerializeField] private List<string> _drawOrder;
        
        private bool _isPlaying;
        
        protected override void Initialize()
        {
            _folds = new UnitySerializableDictionary<string, bool>();
            _orderButtonWidth = new[] { GUILayout.Width(20) };
            _controlButtonWidth = new[] { GUILayout.Width(60) };
            _drawOrder = new List<string>();
        }

        protected override void OnInspectorGUI(EditorUI ui)
        {
            if (_isPlaying != Application.isPlaying)
            {
                _isPlaying = Application.isPlaying;

                if (_isPlaying && _clearOnEnterPlayerMode)
                {
                    BDebug.ClearTrackStorage();
                }
            }

            ui.WithOption(_controlButtonWidth).Horizontal(ui =>
            {
                ui.Button("Clear", ui => BDebug.ClearTrackStorage());
                ui.Button("Open", ui => _setAllFolds(true));
                ui.Button("Close", ui => _setAllFolds(false));
                ui.Button("Reset", _reset);
                ui.CheckBox("Play Mode Clear", ref _clearOnEnterPlayerMode);
            });

            foreach (string storageName in BDebug.TrackStorages.Keys)
            {
                if (_folds.TryAdd(storageName, false))
                {
                    _drawOrder.Add(storageName);
                }
            }

            List<string> newDrawOrder = _drawOrder;
            
            ui.VerticalScroll(ref _scrollPosition, ui =>
            {
                for (int i = 0; i < _drawOrder.Count; i++)
                {
                    string storageName = _drawOrder[i];

                    _folds.TryGetValue(storageName, out bool foldState);

                    ui.Horizontal(ui =>
                    {
                        foldState = EditorGUILayout.Foldout(foldState, storageName);

                        ui.WithOption(_orderButtonWidth, ui =>
                        {
                            ui.Button("↑", ui =>
                            {
                                newDrawOrder = new List<string>(_drawOrder);
                                newDrawOrder.TrySwap(i, i - 1);
                            });
                            ui.Button("↓", ui =>
                            {
                                newDrawOrder = new List<string>(_drawOrder);
                                newDrawOrder.TrySwap(i, i + 1);
                            });
                        });
                    });

                    _folds[storageName] = foldState;
                    
                    if (foldState)
                    {
                        ui.IndentLevel(1, ui =>
                        {
                            BDebug.TrackStorages.TryGetValue(storageName, out DebugTrackStorage trackStorage);
                            _trackStorageUI(ui, trackStorage);
                            GUILayout.Space(8);
                        });
                    }
                }
            });

            _drawOrder = newDrawOrder;
            
            Repaint();
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
                ui.Float2Field(id, value);
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

        private void _reset(EditorUI ui)
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
            
        }
    }
}