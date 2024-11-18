using System.Collections.Generic;
using Beast.Claw.Editor.Window;
using Beast.Claw.Text;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Editor.Validation
{
    public class ValidationWindow : EditorWindow
    {
        [SerializeField] private float2 _scrollPosition;
        [SerializeField] private string _searchPattern;

        private StringWithFormat _searchPatternLowerCase;
        private CSButtonView _csButton;
        
        protected override void Warmup()
        {
            _searchPatternLowerCase = new StringWithFormat(_searchPattern ?? string.Empty, EStringFormat.LowerCase);
            _csButton = new CSButtonView(_flow);
        }

        protected override void Reset()
        {
            
        }

        protected override void DrawUI()
        {
            _flow.Begin().Draw(ui =>
            {
                IReadOnlyCollection<ValidationMethod> allMethods = ValidationManager.AllMethods;
                
                _flow.Begin().Horizontal(ui =>
                {
                    const string EXTRA_INFO = "Method Display";

                    _flow.Begin().FieldWidth(UIStyle.SIZE_ONE_CHAR).LabelWidth(EXTRA_INFO, ui =>
                    {
                        bool extraInfoEnabled = ValidationPreference.MethodDisplayEnabled;
                        ui.CheckBoxLeft(EXTRA_INFO, ref extraInfoEnabled);
                        ValidationPreference.MethodDisplayEnabled.Value = extraInfoEnabled;
                    });
                    
                    const string TOTAL = "Total";

                    _flow.Begin().LabelWidth(TOTAL).Draw(ui =>
                    {
                        ui.IntField(TOTAL, allMethods.Count);
                    });
                });
                
                const string SEARCH = "Search";

                _flow.Begin().LabelWidth(SEARCH, ui =>
                {
                    ui.StringField(SEARCH, ref _searchPattern);
                        
                    _searchPatternLowerCase.Update(_searchPattern);
                });

                _flow.Begin().Vertical().Scroll(ref _scrollPosition, ui =>
                {
                    bool hasPattern = !_searchPatternLowerCase.Value.IsNullOrEmpty();
                    
                    foreach (ValidationMethod method in allMethods)
                    {
                        bool canDisplay = !hasPattern || method.typeFullNameLowerCase.Contains(_searchPatternLowerCase);

                        _flow.Begin().If(canDisplay).Horizontal().Draw(ui =>
                        {
                            _csButton.DrawAsScriptLink(method.script, method.hasScript);
                            
                            ui.StringField(Label.NoLabel, method.fullNameWithMethod);
                        });
                    }
                });
            });
        }

        protected override void Dispose()
        {
            
        }
    }
}