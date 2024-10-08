using System;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class DebugItemButton : UIView
    {
        public ItemData Item { get; private set; }
        public Selectable Selectable => _selectable;

        public event Action<DebugItemButton> Clicked;
        
        private const float _defaultAlpha = 0.7f;
        private const float _selectedAlpha = 0.4f;

        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _textMeshPro;
        [SerializeField] private Selectable _selectable;
        
        private void Start()
        {
            _button.onClick.AddListener(() => Clicked?.Invoke(this));
        }

        public void SetItemData(ItemData item)
        {
            Item = item;
            switch (item.type)
            {
                case EItemType.None:
                default:
                    _icon.color = Color.black;
                    break;
                case EItemType.Eat:
                    _icon.color = new Color(0.8f, 0.2f, 0.2f, _defaultAlpha);
                    break;
                case EItemType.Weapons:
                    _icon.color = new Color(0.8f, 0.5f, 0.2f, _defaultAlpha);
                    break;
                case EItemType.Tools:
                    _icon.color = new Color(0.3f, 0.5f, 0.2f, _defaultAlpha);
                    break;
                case EItemType.Equipment:
                    _icon.color = new Color(0.3f, 0.5f, 0.7f, _defaultAlpha);
                    break;
            }
            _textMeshPro.SetText(item.name);
        }

        public void SetSelectedMode()
        {
            Color currentColor = _icon.color;
            currentColor.a = _selectedAlpha;
            _icon.color = currentColor;
        }
        
        public void SetDefaultMode()
        {
            Color currentColor = _icon.color;
            currentColor.a = _defaultAlpha;
            _icon.color = currentColor;
        }
    }
}