using System.Collections.Generic;
using Beast.Claw.UI;
using Noesis;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class MainMenuView : BeastControl
    {
        public override void BindElements()
        {
            /*_find("scroll_items", out ItemsControl scroll_items);
            
            var versionChanges = new List<VersionChange>
            {
                new VersionChange { Version = "v1.0", Description = "Initial release with basic features." },
                new VersionChange { Version = "v1.1", Description = "Bug fixes and performance improvements." },
                new VersionChange { Version = "v2.0", Description = "Major update with new UI redesign." }
            };
            
           // scroll_items.ItemsSource = versionChanges;

            scroll_items.Items.Add(versionChanges[0]);
            Debug.Log($"{scroll_items.Items.Count} {scroll_items.ItemsSource}");*/
        }
    }
    
    public class VersionChange
    {
        public string Version { get; set; }
        public string Description { get; set; }
    }
}
