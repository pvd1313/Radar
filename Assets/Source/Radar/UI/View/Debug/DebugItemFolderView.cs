using System;
using TMPro;
using UnityEngine;

namespace Beast.Radar.UI.Debug
{
    [Serializable]
    public class DebugItemFolderView
    {
        public GameObject body;
        public Transform itemsRoot;
        public UIButton nextItems;
        public UIButton previousItems;
        public UIButton[] itemGrantButtons;
        public DebugItemButton[] itemButtons;
        public TextMeshProUGUI itemName;
        public TextMeshProUGUI itemWeight;
    }
}