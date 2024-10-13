using System.Collections.Generic;
using System.Linq;
using Beast.Claw.Common;
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
        
        private bool _isPlaying;
        private Dictionary<string, bool> _folds;
        
        protected override void Initialize()
        {
            
        }

        protected override void Dispose()
        {
            
        }

        protected override void OnInspectorGUI()
        {
            if (_isPlaying != Application.isPlaying)
            {
                _isPlaying = Application.isPlaying;

                if (_isPlaying && _clearOnEnterPlayerMode)
                {
                    BDebug.ClearTrackStorage();
                }
            }

            UI.Horizontal(() =>
            {
                UI.Button("Clear", BDebug.ClearTrackStorage);
                UI.Button("Open All", () => _setAllFolds(true));
                UI.Button("Collapse", () => _setAllFolds(false));
                _clearOnEnterPlayerMode = UI.CheckBox("Play Mode Clear", _clearOnEnterPlayerMode);
            });
            
            _folds ??= new Dictionary<string, bool>();

            UI.VerticalScroll(ref _scrollPosition, () =>
            {
                foreach ((string storageName, DebugTrackStorage storage) in BDebug.TrackStorages)
                {
                    _folds.TryGetValue(storageName, out bool foldState);
                    foldState = EditorGUILayout.Foldout(foldState, storageName);
                    _folds[storageName] = foldState;

                    if (foldState)
                    {
                        EditorGUI.indentLevel++;
                        _trackStorageUI(storage);
                        EditorGUI.indentLevel--;
                        GUILayout.Space(8);
                    }
                }
            });
            
            Repaint();
        }

        private void _trackStorageUI(DebugTrackStorage storage)
        {
            foreach ((string id, float value) in storage.ReadAllFloat())
            {
                UI.FloatField(id, value);
            }
            
            foreach ((string id, int value) in storage.ReadAllInt())
            {
                UI.IntField(id, value);
            }
            
            foreach ((string id, float2 value) in storage.ReadAllFloat2())
            {
                UI.Float2Field(id, value);
            }
            
            foreach ((string id, float3 value) in storage.ReadAllFloat3())
            {
                UI.Float3Field(id, value);
            }
            
            foreach ((string id, float4 value) in storage.ReadAllFloat4())
            {
                UI.Float4Field(id, value);
            }
            
            foreach ((string id, string value) in storage.ReadAllString())
            {
                UI.StringField(id, value);
            }
        }

        private void _setAllFolds(bool isOpen)
        {
            foreach (string id in _folds.Keys.ToList())
            {
                _folds[id] = isOpen;
            }
        }
    }
}