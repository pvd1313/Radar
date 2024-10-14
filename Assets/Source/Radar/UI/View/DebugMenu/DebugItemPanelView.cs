using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Beast.Radar.UI
{
    [Serializable]
    public class DebugItemPanelView
    {
        public GameObject body;
        public Transform itemsRoot;
        public Transform itemTypeRoot;
        public UIButton nextItems;
        public UIButton previousItems;
        public UIButton[] itemGrantButtons;
        public TextMeshProUGUI itemName;
        public TextMeshProUGUI itemWeight;
        public List<InputIconDisplay> inputIcons;
    }
}