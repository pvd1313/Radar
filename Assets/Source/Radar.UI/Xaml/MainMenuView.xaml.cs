using System.Collections.Generic;
using Beast.Claw.UI;
using Noesis;
using UnityEngine;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class MainMenuView : BeastControl
    {
        public StackPanel scroll_items;


        private Style _versionStyle;
        private Style _descriptionStyle;
        public override void BindElements()
        {
            _versionStyle = (Style)Resources["Style.Text.Active.Header"];
            _descriptionStyle = (Style)Resources["Style.Text.Light.Normal"];
            
            _find("stack_updates", out  scroll_items);
            
            var versionChanges = new List<VersionChange>
            {
                new VersionChange { Version = "v1.0", Description = "Initial release with basic features." },
                new VersionChange { Version = "v1.1", Description = "Bug fixes and performance improvements." },
                new VersionChange { Version = "v2.0", Description = "Major update with new UI redesign." }
            };
            
            AddVersionDescription("-v.1","\tdsajdjasl");
            AddVersionDescription("-v.2","\tdafasdfdsfs\n\tfdfds");
            AddVersionDescription("-v.3","\tdafasdfdsfs \n\tfdfds");
            AddVersionDescription("-v.4","\tdafasdfdsfs \n\tfdfds");
            AddVersionDescription("-v.5","\tdafasdfdsfs \n\tfdfds");
            AddVersionDescription("-v.5","\tdafasdfdsfs \n\tfdfds");
            AddVersionDescription("-v.5","\tdafasdfdsfs \n\tfdfds");
            AddVersionDescription("-v.5","\tdafasdfdsfs \n\tfdfds");
            AddVersionDescription("-v.5","\tdafasdfdsfs \n\tfdfds");
            AddVersionDescription("-v.5","\tdafasdfdsfs \n\tfdfds");
            AddVersionDescription("-v.5","\tdafasdfdsfs \n\tfdfds");
            AddVersionDescription("-v.5","\tdafasdfdsfs \n\tfdfds");
            AddVersionDescription("-v.5","\tdafasdfdsfs \n\tfdfds");
            AddVersionDescription("-v.5","\tdafasdfdsfs \n\tfdfds");
            AddVersionDescription("-v.5","\tdafasdfdsfs \n\tfdfds");
            AddVersionDescription("-v.5","\tdafasdfdsfs \n\tfdfds");
            AddVersionDescription("-v.5","\tdafasdfdsfs \n\tfdfds");
            AddVersionDescription("-v.5","\tdafasdfdsfs \n\tfdfds");
            AddVersionDescription("-v.5","\tdafasdfdsfs \n\tfdfds");         AddVersionDescription("-v.5","\tdafasdfdsfs \n\tfdfds");
            AddVersionDescription("-v.5","\tdafasdfdsfs \n\tfdfds");
            AddVersionDescription("-v.5","\tdafasdfdsfs \n\tfdfds");
            AddVersionDescription("-v.5","\tdafasdfdsfs \n\tfdfds");
            AddVersionDescription("-v.5","\tdafasdfdsfs \n\tfdfds");
            AddVersionDescription("-v.5","\tdafasdfdsfs \n\tfdfds");
            AddVersionDescription("-v.5","\tdafasdfdsfs \n\tfdfds");
            AddVersionDescription("-v.5","\tdafasdfdsfs \n\tfdfds");
            AddVersionDescription("-v.5","\tdafasdfdsfs \n\tfdfds");
            AddVersionDescription("-v.5","\tdafasdfdsfs \n\tfdfds");
            AddVersionDescription("-v.5","\tdafasdfdsfs \n\tfdfds");
            AddVersionDescription("-v.5","\tdafasdfdsfs \n\tfdfds");
            AddVersionDescription("-v.5","\tdafasdfdsfs \n\tfdfds");
            AddVersionDescription("-v.5","\tdafasdfdsfs \n\tfdfds");
            AddVersionDescription("-v.5","\tdafasdfdsfs \n\tfdfds");

            Debug.Log($"{scroll_items.Children.Count}");
        }

        public void AddVersionDescription(string version, string description)
        {
            TextBlock versionText = new TextBlock()
            {
                Text = version,
               Style = _versionStyle
            };
            
            scroll_items.Children.Add(versionText);
            Panel.SetZIndex(versionText,2);
            
            scroll_items.Children.Add(new TextBlock()
            {
                Text = description,
                Style = _descriptionStyle
            });
        }
    }
    
    public class VersionChange
    {
        public string Version { get; set; }
        public string Description { get; set; }
    }
}
