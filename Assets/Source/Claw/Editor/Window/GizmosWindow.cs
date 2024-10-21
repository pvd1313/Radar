using System.Collections.Generic;
using Beast.Claw.Gizmos;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Editor.Window
{
    public class GizmosWindow : EditorWindow
    {
        private struct Data
        {
            public bool isEnable;
            public Color color;
        }

        private float2 _scrollPosition;
        private readonly Dictionary<string, Data> _gizmos = new();

        protected override void Warmup()
        {
            
        }
        
        protected override void Reset()
        {
            
        }
        
        protected override void DrawUI()
        {
            if (GizmosStateStorage.HandleCount == 0)
            {
                flow.InfoBox("No gizmos registered.");
                _gizmos.Clear();
                return;
            }
            
            flow.Vertical().Scroll(ref _scrollPosition, ui =>
            {
                for (ushort i = 0; i < GizmosStateStorage.HandleCount; i++)
                {
                    ushort handleId = i;
                    string handleName = GizmosStateStorage.GetHandleName(i);

                    if (!_gizmos.TryGetValue(handleName, out Data data))
                    {
                        data = new Data
                        {
                            isEnable = GizmosStateStorage.GetHandleEnabled(i),
                            color = GizmosStateStorage.GetHandleColor(i)
                        };
                        
                        _gizmos[handleName] = data;
                    }

                    bool tempIsEnabled = data.isEnable;
                    Color tempColor = data.color;

                    ui.Label(handleName);
                    ui.CheckBoxRight("enabled", ref tempIsEnabled);
                    ui.ColorField("color", ref tempColor);

                    if (tempIsEnabled != data.isEnable)
                    {
                        data.isEnable = tempIsEnabled;
                        GizmosStateStorage.SetHandleEnabled(handleId, tempIsEnabled);
                    }

                    if (tempColor != data.color)
                    {
                        data.color = tempColor;
                        GizmosStateStorage.SetHandleColor(handleId, tempColor);
                    }

                    _gizmos[handleName] = data;
                }
            });
        }

        protected override void Dispose()
        {
            
        }
    }
}