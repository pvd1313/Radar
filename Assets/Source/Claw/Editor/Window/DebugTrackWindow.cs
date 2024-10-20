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
        private struct TypeInfo
        {
            public string fullName;
            public string niceShortName;
            public bool isInitialized;
            public MonoScript script;
            public bool hasScript;
        }
        
        [SerializeField] private bool _playModeClear;
        [SerializeField] private float2 _scrollPosition;
        [SerializeField] private UnitySerializableDictionary<string, bool> _folds = new();
        [SerializeField] private List<string> _drawOrder = new();
        [SerializeField] private int _updateTickRate = 3;
        [SerializeField] private bool _isSharpMode;
        [SerializeField] private bool _editorFetch;

        private Dictionary<Type, TypeInfo> _typeInfo;
        private bool _isPlaying;
        private DateTime _lastUpdateTime;
        private Dictionary<string, Type> _fullNameToType;
        
        private GUILayoutOption _smallButton;
        private GUILayoutOption _csButtonWidth;
        private GUILayoutOption _csButtonHeight;
        private GUILayoutOption _tickRateButton;
        private GUIStyle _csText;
        private GUIStyle _csButton;
        private GUIContent _csIcon;
        private GUIContent _infoIcon;

        protected override void Warmup()
        {
            EditorApplication.update += _onEditorApplicationUpdate;
            
            _lastUpdateTime = DateTime.Now;
            _typeInfo = new Dictionary<Type, TypeInfo>();
            _fullNameToType = new Dictionary<string, Type>();
            
            _tickRateButton = GUILayout.Width(UIStyle.SIZE_TWO_CHAR);
            _smallButton = GUILayout.Width(UIStyle.SIZE_ONE_CHAR);
            _csButtonWidth = GUILayout.Width(UIStyle.SIZE_ONE_CHAR);
            _csButtonHeight = GUILayout.Height(UIStyle.SIZE_ONE_CHAR);
            
            _csIcon = EditorGUIUtility.IconContent(UIStyle.ICON_CSHARP_SCRIPT);
            _infoIcon = EditorGUIUtility.IconContent(UIStyle.ICON_CONSOLE_INFO);

            _csButton = new GUIStyle(GUI.skin.button)
            {
                padding = new RectOffset(0, 0, 0, 0),
                imagePosition = ImagePosition.ImageOnly
            };

            _csText = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
            };
        }
        
        protected override void Reset()
        {
            _drawOrder.Clear();
            _folds.Clear();
            _typeInfo.Clear();
            _fullNameToType.Clear();
        }

        private void _onEditorApplicationUpdate()
        {
            if (!EditorApplication.isPlaying && !_editorFetch)
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

                if (_isPlaying && _playModeClear)
                {
                    BDebug.ClearTrackStorage();
                }
            }

            flow.Horizontal(ui =>
            {
                const string FETCH_EDITOR = "EditorFetch";
                const string PLAY_MODE_CLEAR = "PlayModeClear";

                flow.LabelWidth(FETCH_EDITOR).Draw(ui => ui.CheckBoxLeft(FETCH_EDITOR, ref _editorFetch));
                flow.LabelWidth(PLAY_MODE_CLEAR).Draw(ui => ui.CheckBoxLeft(PLAY_MODE_CLEAR, ref _playModeClear));
                
                ui.FlexibleSpace();
            });
            
            flow.Horizontal(ui =>
            {
                flow.Option(_csButtonHeight, _csButtonWidth)
                    .Style(_csButton)
                    .Button( _isSharpMode ? _csIcon : _infoIcon, () =>
                    {
                        _isSharpMode = !_isSharpMode;
                    });
                
                const string FETCH_TICK_RATE_LABEL = "FetchTickRate";
                
                flow.LabelWidth(FETCH_TICK_RATE_LABEL)
                    .FieldWidth("XXX")
                    .Draw(ui =>
                {
                    ui.IntField(FETCH_TICK_RATE_LABEL, ref _updateTickRate);
                });

                flow.Option(_tickRateButton).Draw(ui =>
                {
                    ui.Button("3", () => _updateTickRate = 3);
                    ui.Button("10", () => _updateTickRate = 10);
                    ui.Button("60", () => _updateTickRate = 60);
                });
                
                ui.FlexibleSpace();
            });

            flow.Horizontal(ui =>
            {
                // ui.Button("Reset", _reset);
                ui.Button("Clear", BDebug.ClearTrackStorage);
                ui.Button("Open", () => _setAllFolds(true));
                ui.Button("Close", () => _setAllFolds(false));
            });
            
            flow.Draw(ui => ui.Space(UIStyle.SIZE_MINOR_SPACE));

            foreach (Type type in BDebug.TrackStorage.Keys)
            {
                TypeInfo typeInfo = _getOrCreateTypeInfo(type);

                _fullNameToType[typeInfo.fullName] = type;
                
                if (_folds.TryAdd(typeInfo.fullName, false))
                {
                    _drawOrder.Add(typeInfo.fullName);
                }
            }

            (int a, int b) swapIndex = default;

            flow.Vertical().Scroll(ref _scrollPosition, ui =>
            {
                for (int i = 0; i < _drawOrder.Count; i++)
                {
                    string storageFullName = _drawOrder[i];

                    _folds.TryGetValue(storageFullName, out bool foldState);
                    _fullNameToType.TryGetValue(storageFullName, out Type storageType);
                    _typeInfo.SafeTryGetValueOrDefault(storageType, out TypeInfo storageInfo);

                    flow.Horizontal(ui =>
                    {
                        if (storageInfo.isInitialized)
                        {
                            ui.Foldout(storageInfo.niceShortName, ref foldState);
                        }
                        else
                        {
                            ui.Foldout(storageFullName, ref foldState);
                        }

                        flow.Option(_smallButton, ui =>
                        {
                            ui.Button(UIStyle.CHAR_ARROW_UP, () => swapIndex = (i, i - 1));
                            ui.Button(UIStyle.CHAR_ARROW_DOWN, () => swapIndex = (i, i + 1));
                        });
                    });
                    
                    _folds[storageFullName] = foldState;

                    flow.If(foldState, ui =>
                    {
                        _fullNameToType.TryGetValue(storageFullName, out Type storageType);

                        flow.If(storageInfo.isInitialized && _isSharpMode).Horizontal(ui =>
                        {
                            flow.Enabled(storageInfo.hasScript)
                                .Style(_csButton)
                                .Option(_csButtonWidth, _csButtonHeight)
                                .Button(_csIcon, () =>
                            {
                                BAsset.TryOpenAsset(storageInfo.script);
                            });

                            flow.Enabled(false).Style(_csText).Option(_csButtonHeight).PassiveLabel(storageInfo.fullName);
                        });

                        BDebug.TrackStorage.SafeTryGetValueOrDefault(storageType, out DebugTrackStorage storage);

                        flow.Tab(ui =>
                        {
                            _trackStorageUI(ui, storage);
                        });
                        
                        ui.Space(UIStyle.SIZE_MINOR_SPACE);
                    });
                }
            });

            _drawOrder.TrySwap(swapIndex.a, swapIndex.b);
        }

        private void _trackStorageUI(FlowUI ui, DebugTrackStorage storage)
        {
            if (storage == null)
            {
                ui.PassiveLabel("-- NO DATA --");
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
                ui.Float3FieldTwoLine(id, value);
            }
            
            foreach ((string id, float4 value) in storage.ReadAllFloat4())
            {
                ui.Float4FieldTwoLine(id, value);
            }
            
            foreach ((string id, string value) in storage.ReadAllString())
            {
                ui.StringField(id, value);
            }
        }

        private void _setAllFolds(bool isOpen)
        {
            foreach (string id in _folds.Keys.ToList())
            {
                _folds[id] = isOpen;
            }
        }

        private TypeInfo _getOrCreateTypeInfo(Type type)
        {
            if (_typeInfo.TryGetValue(type, out TypeInfo info))
            {
                return info;
            }

            bool hasScript = BAsset.TryFindScript(type, out MonoScript script);

            return _typeInfo.InsertAndReturn(type, new TypeInfo
            {
                niceShortName = ObjectNames.NicifyVariableName(type.Name),
                fullName = type.FullName,
                script = script,
                hasScript = hasScript,
                isInitialized = true
            });
        }
        
        protected override void Dispose()
        {
            EditorApplication.update -= _onEditorApplicationUpdate;
        }
    }
}