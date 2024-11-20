using System;
using Beast.Claw.Editor.UI;
using UnityEditor;
using UnityEngine;

using EditorWindow = Beast.Claw.Editor.UI.EditorWindow;

namespace Beast.Claw.Editor
{
    [Serializable]
    public class RepaintTickRateBar
    {
        [SerializeField] private EditorWindow _owner;
        [SerializeField] private int _tickRate = 3;
        [SerializeField] private bool _editorFetch;
        
        private DateTime _lastUpdateTime;
        private GUILayoutOption _buttonWidth;

        public RepaintTickRateBar(EditorWindow owner)
        {
            _owner = owner;
        }
        
        public void Warmup()
        {
            EditorApplication.update += _update;
            
            _buttonWidth = GUILayout.Width(UIStyle.SIZE_TWO_CHAR);
        }

        private void _update()
        {
            if (!EditorApplication.isPlaying && !_editorFetch)
            {
                return;
            }
            
            DateTime now = DateTime.Now;
            if ((now - _lastUpdateTime).TotalSeconds >= 1f / _tickRate)
            {
                _lastUpdateTime = now;
                _owner.Repaint();
            }
        }
        
        public void Draw(IFlow flow)
        {
            const string EDITOR_FETCH = "EFetch";

            flow.Begin().LabelWidth(EDITOR_FETCH).Draw(ui =>
            {
                ui.CheckBoxRight(EDITOR_FETCH, ref _editorFetch);
            });
            
            const string TICK_RATE = "TickRate";

            flow.Begin().LabelWidth(TICK_RATE).FieldWidth("XXX").Draw(ui =>
            {
                ui.IntField(TICK_RATE, ref _tickRate);
            });
            
            flow.Begin().Option(_buttonWidth).Draw(ui =>
            {
                ui.Button("3", () => _tickRate = 3);
                ui.Button("10", () => _tickRate = 10);
                ui.Button("60", () => _tickRate = 60);
            });
        }

        public void Dispose()
        {
            EditorApplication.update -= _update;
        }
    }
}