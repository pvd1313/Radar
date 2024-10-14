using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class DebugItemButton : UIView, IUIPoolEntity, IUIRadioButton<DebugItemButton>
    {
        public ItemData Item { get; private set; }
        public UISelectable Selectable => _selectable;
        public event Action<DebugItemButton> Clicked;
        
        private const float _defaultAlpha = 0.7f;
        private const float _selectedAlpha = 0.4f;
        
        [SerializeField] private Button _button;
        [SerializeField] private Image _background;
        [SerializeField] private TextMeshProUGUI _textMeshPro;
        [SerializeField] private UISelectable _selectable;
        
        public void Init(params object[] parameters)
        {
            _button.onClick.AddListener(() => Clicked?.Invoke(this));
        }

        public void Enable()
        {
            gameObject.SetActive(true);
        }

        public void Disable()
        {
            gameObject.SetActive(false);
        }


        public void SetSelectedMode()
        {
            _button.interactable = false;
            Color currentColor = _background.color;
            currentColor.a = _selectedAlpha;
            _background.color = currentColor;
        }

        public void SetDefaultMode()
        {
            _button.interactable = true;
            Color currentColor = _background.color;
            currentColor.a = _defaultAlpha;
            _background.color = currentColor;
        }

        public void SetItemData(ItemData item)
        {
            Item = item;
            switch (item.type)
            {
                case EItemType.None:
                default:
                    _background.color = Color.black;
                    break;
                case EItemType.Eat:
                    _background.color = new Color(0.8f, 0.2f, 0.2f, _defaultAlpha);
                    break;
                case EItemType.Weapons:
                    _background.color = new Color(0.8f, 0.5f, 0.2f, _defaultAlpha);
                    break;
                case EItemType.Tools:
                    _background.color = new Color(0.3f, 0.5f, 0.2f, _defaultAlpha);
                    break;
                case EItemType.Equipment:
                    _background.color = new Color(0.3f, 0.5f, 0.7f, _defaultAlpha);
                    break;
            }
            _textMeshPro.SetText(item.name);
        }
    }
}