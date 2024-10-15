using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class DebugItemTypeButton : UIView, IUIPoolEntity, IUIRadioButton<DebugItemTypeButton>
    {
        public bool IsSelected { get; private set; }
        public EItemType ItemType { get; private set; }
        public UISelectable Selectable => _selectable;
        public event Action<DebugItemTypeButton> Clicked;
        
        private const float DefaultAlpha = 1f;
        private const float SelectedAlpha = 0.7f;

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
                    _background.color = new Color(0.5f, 0.5f, 0.5f, DefaultAlpha);
                    break;
                case EItemType.Eat:
                    _background.color = new Color(0.8f, 0.2f, 0.2f, DefaultAlpha);
                    break;
                case EItemType.Weapons:
                    _background.color = new Color(0.8f, 0.5f, 0.2f, DefaultAlpha);
                    break;
                case EItemType.Tools:
                    _background.color = new Color(0.3f, 0.5f, 0.2f, DefaultAlpha);
                    break;
                case EItemType.Equipment:
                    _background.color = new Color(0.3f, 0.5f, 0.7f, DefaultAlpha);
                    break;
            }
            _typeText.SetText(ItemType == EItemType.None ? "all" : ItemType.ToString().ToLower());
            _button.onClick.AddListener(Select);
        }

        public void Enable()
        {
            gameObject.SetActive(true);
        }

        public void Disable()
        {
            gameObject.SetActive(false);
        }

        public void Select()
        {
            Clicked?.Invoke(this);
        }

        public void SetSelectedMode()
        {
            IsSelected = true;
            _selectable.SetInteractable(false);
            Color currentColor = _background.color;
            currentColor.a = SelectedAlpha;
            _background.color = currentColor;
        }

        public void SetDefaultMode()
        {
            IsSelected = false;
            _selectable.SetInteractable(true);
            Color currentColor = _background.color;
            currentColor.a = DefaultAlpha;
            _background.color = currentColor;
        }
    }
}