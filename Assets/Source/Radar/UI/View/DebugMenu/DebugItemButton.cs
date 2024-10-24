using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class DebugItemButton : UIView, IUIPoolable, IUIRadioButton<DebugItemButton>
    {
        public bool IsSelected { get; private set; }
        public ItemData Item { get; private set; }
        public UISelectable Selectable => _selectable;
        public event Action<DebugItemButton> Clicked;
        
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _textMeshPro;
        [SerializeField] private UISelectable _selectable;
        [SerializeField] private SelectableColor _selectableColor;
        
        public void Enable()
        {
            _button.onClick.AddListener(Click);
            gameObject.SetActive(true);
        }

        public void Disable()
        {
            gameObject.SetActive(false);
            _button.onClick.RemoveAllListeners();
        }

        public void Click()
        {
            Clicked?.Invoke(this);
        }
        
        public void Select()
        {
            IsSelected = true;
            _selectableColor.SetSelectedMode();
        }

        public void Deselect()
        {
            IsSelected = false;
            _selectableColor.SetDefaultMode();
        }
        
        public void SetItemData(ItemData item)
        {
            Item = item;
            switch (item.type)
            {
                case EItemType.None:
                default:
                    _selectableColor.SetDefaultColor(Color.black);
                    break;
                case EItemType.Eat:
                    _selectableColor.SetDefaultColor(new Color(0.8f, 0.2f, 0.2f));
                    break;
                case EItemType.Weapons:
                    _selectableColor.SetDefaultColor(new Color(0.8f, 0.5f, 0.2f));
                    break;
                case EItemType.Tools:
                    _selectableColor.SetDefaultColor(new Color(0.3f, 0.5f, 0.2f));
                    break;
                case EItemType.Equipment:
                    _selectableColor.SetDefaultColor(new Color(0.3f, 0.5f, 0.7f));
                    break;
            }
            _textMeshPro.SetText(item.name);
        }

        public override string ToString()
        {
            return $"{Item.type} {Item.name}";
        }
    }
}