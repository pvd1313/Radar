using System.Collections.Generic;
using Beast.Claw.Editor.Serialization;
using Beast.Claw.Editor.UI;
using Beast.Claw.Editor.UI.UI;
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
        private CsButtonView _csButton;
        private bool _hasConfig;
        private ValidationConfig _config;
        private string _missingConfigMessage;
        
        protected override void Warmup()
        {
            _searchPatternLowerCase = new StringWithFormat(_searchPattern ?? string.Empty, EStringFormat.LowerCase);
            _csButton = new CsButtonView(_flow);
            _hasConfig = ConfigManager.FindConfig(out _config);
            _missingConfigMessage = ConfigManager.MissingConfigMessage<ValidationConfig>();
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
                    _flow.Begin().FieldWidth(UIStyle.SIZE_ASSET_FIELD).LabelWidth("Config").Draw(ui =>
                    {
                        ui.AssetField(Label.NoLabel, _config);
                    });
                    
                    const string TOTAL = "Total";
                    _flow.Begin().LabelWidth(TOTAL).FieldWidth(UIStyle.SIZE_TWO_CHAR).Draw(ui =>
                    {
                        ui.IntField(TOTAL, allMethods.Count);
                    });
                    
                    const string EXTRA_INFO = "Method Display";
                    _flow.Begin().FieldWidth(0).LabelWidth(0).Draw(ui =>
                    {
                        bool extraInfoEnabled = ValidationPreference.MethodDisplayEnabled;
                        ui.CheckBoxLeft(EXTRA_INFO, ref extraInfoEnabled);
                        ValidationPreference.MethodDisplayEnabled.Value = extraInfoEnabled;
                    });
                });

                if (!_hasConfig)
                {
                    ui.WarningBox(_missingConfigMessage);
                }
                
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