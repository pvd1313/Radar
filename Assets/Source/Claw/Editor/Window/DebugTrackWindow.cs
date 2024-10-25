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
        [SerializeField] private bool _isSharpMode;
        [SerializeField] private bool _showArrayArrows;

        [SerializeField] private RepaintTickRateBar _tickRateBar;

        private Dictionary<Type, TypeInfo> _typeInfo;
        private bool _isPlaying;
        private Dictionary<string, Type> _fullNameToType;
        
        private GUILayoutOption _smallButton;
        private GUILayoutOption _csButtonWidth;
        private GUILayoutOption _csButtonHeight;
        private GUIStyle _csText;
        private GUIStyle _csButton;
        private GUIContent _csIcon;
        private GUIContent _infoIcon;

        public DebugTrackWindow()
        {
            _tickRateBar = new RepaintTickRateBar(this);
        }

        protected override void Warmup()
        {
            _typeInfo = new Dictionary<Type, TypeInfo>();
            _fullNameToType = new Dictionary<string, Type>();
            
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
            
            _tickRateBar.Warmup();
        }
        
        protected override void Reset()
        {
            _drawOrder?.Clear();
            _folds?.Clear();
            _typeInfo?.Clear();
            _fullNameToType?.Clear();
        }

        protected override void DrawUI()
        {
            if (_isPlaying != Application.isPlaying)
            {
                _isPlaying = Application.isPlaying;

                if (_isPlaying && _playModeClear)
                {
                    BDebug.ClearTrackStorage();
                }
            }

            Begin.Horizontal(ui =>
            {
                Begin.Option(_csButtonHeight, _csButtonWidth)
                    .Style(_csButton)
                    .Button( _isSharpMode ? _csIcon : _infoIcon, () =>
                    {
                        _isSharpMode = !_isSharpMode;
                    });
                
                const string PLAY_MODE_CLEAR = "PlayModeClear";

                Begin.LabelWidth(PLAY_MODE_CLEAR)
                    .Draw(ui => ui.CheckBoxRight(PLAY_MODE_CLEAR, ref _playModeClear));
                
                const string ARROWS = UIStyle.CHAR_ARROW_UP + UIStyle.CHAR_ARROW_DOWN;
                Begin.LabelWidthAndButton(ARROWS, () => _showArrayArrows = !_showArrayArrows);
                
                ui.FlexibleSpace();
            });
            
            Begin.Horizontal(ui =>
            {
                _tickRateBar.Draw(_flow);
                
                ui.FlexibleSpace();
            });

            Begin.Horizontal(ui =>
            {
                ui.Button("Clear", BDebug.ClearTrackStorage);
                ui.Button("Open", () => _setAllFolds(true));
                ui.Button("Close", () => _setAllFolds(false));
            });
            
            Begin.Draw(ui => ui.Space(UIStyle.SIZE_MINOR_SPACE));

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

            Begin.Vertical().Scroll(ref _scrollPosition, ui =>
            {
                for (int i = 0; i < _drawOrder.Count; i++)
                {
                    string storageFullName = _drawOrder[i];

                    _folds.TryGetValue(storageFullName, out bool foldState);
                    _fullNameToType.TryGetValue(storageFullName, out Type storageType);
                    _typeInfo.SafeTryGetValueOrDefault(storageType, out TypeInfo storageInfo);

                    Begin.Horizontal(ui =>
                    {
                        if (storageInfo.isInitialized)
                        {
                            ui.Foldout(storageInfo.niceShortName, ref foldState);
                        }
                        else
                        {
                            ui.Foldout(storageFullName, ref foldState);
                        }

                        Begin.If(_showArrayArrows).Option(_smallButton, ui =>
                        {
                            ui.Button(UIStyle.CHAR_ARROW_UP, () => swapIndex = (i, i - 1));
                            ui.Button(UIStyle.CHAR_ARROW_DOWN, () => swapIndex = (i, i + 1));
                        });
                    });
                    
                    _folds[storageFullName] = foldState;

                    Begin.If(foldState, ui =>
                    {
                        _fullNameToType.TryGetValue(storageFullName, out Type storageType);

                        Begin.If(storageInfo.isInitialized && _isSharpMode).Horizontal(ui =>
                        {
                            Begin.Enabled(storageInfo.hasScript)
                                .Style(_csButton)
                                .Option(_csButtonWidth, _csButtonHeight)
                                .Button(_csIcon, () =>
                            {
                                BAsset.TryOpenAsset(storageInfo.script);
                            });

                            Begin.Enabled(false).Style(_csText).Option(_csButtonHeight).PassiveLabel(storageInfo.fullName);
                        });

                        BDebug.TrackStorage.SafeTryGetValueOrDefault(storageType, out DebugTrackStorage storage);

                        Begin.Tab(ui =>
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
                ui.GenericField(id, value);
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
            
            foreach ((string id, bool value) in storage.ReadAllBool())
            {
                bool v = value;
                ui.CheckBoxRight(id, ref v);
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
            _tickRateBar.Dispose();
        }
    }
}