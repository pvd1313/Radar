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
        [SerializeField] private UnitySerializableDictionary<string, bool> _folds = new();
        [SerializeField] private GUILayoutOption _orderButtonWidth = GUILayout.Width(EditorUI.OneCharButtonSize);
        [SerializeField] private List<string> _drawOrder = new();
        
        private bool _isPlaying;

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

                EditorGUIUtility.labelWidth = 150;
                
                ui.CheckBox("Enter PlayMode Clear", ref _clearOnEnterPlayerMode);

                EditorGUIUtility.labelWidth = labelWidthBak;
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
            
        }
    }
}