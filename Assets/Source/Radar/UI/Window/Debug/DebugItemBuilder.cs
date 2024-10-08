using UnityEngine;

namespace Beast.Radar.UI
{
    public class DebugItemBuilder
    {
        private readonly IUIFactory _factory;
        private readonly Transform _buttonsRoot;

        public DebugItemBuilder(IUIFactory factory, Transform buttonsRoot)
        {
            _factory = factory;
            _buttonsRoot = buttonsRoot;
        }

        public DebugItemButton[] BuildItemButtons(ItemData[] items)
        {
            DebugItemButton[] buttons = new DebugItemButton[items.Length];
            for (var index = 0; index < items.Length; index++)
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