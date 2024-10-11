using UnityEngine;

namespace Beast.Radar.UI
{
    public class DebugItemPanelBuilder
    {
        private readonly IUIFactory _factory;
        private readonly Transform _buttonsRoot;

        public DebugItemPanelBuilder(IUIFactory factory, Transform buttonsRoot)
        {
            _factory = factory;
            _buttonsRoot = buttonsRoot;
        }

        public DebugItemButton[] BuildItemButtons(ItemData[] items)
        {
            DebugItemButton[] buttons = new DebugItemButton[items.Length];
            for (int index = 0; index < items.Length; index++)
            {
                _factory.Create(this, PrefabID.DebugItemButton, out DebugItemButton button);
                button.transform.SetParent(_buttonsRoot, false);
                button.SetItemData(items[index]);
                buttons[index] = button;
            }
            return buttons;
        }
    }
}