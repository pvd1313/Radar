using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class DebugItemTypeButton : UIView, IUIPoolEntity, IUIRadioButton<DebugItemTypeButton>
    {
        public EItemType ItemType { get; private set; }
        public UISelectable Selectable => _selectable;
        
        public event Action<DebugItemTypeButton> Clicked;
        
        private const float _defaultAlpha = 0.7f;
        private const float _selectedAlpha = 0.4f;

        [SerializeField] private UISelectable _selectable;
        [SerializeField] private TextMeshProUGUI _typeText;
        [SerializeField] private Button _button;
        [SerializeField] private Image _background;
        
        public void Init(params object[] parameters)
        {
            if (parameters[0] is EItemType itemType)
            {
                ItemType = itemType;
            }

            switch (ItemType)
            {
                case EItemType.None:
                default:
                    _background.color = Color.gray;
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
            _typeText.SetText(ItemType == EItemType.None ? "all" : ItemType.ToString().ToLower());
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
    }
}