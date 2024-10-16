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

        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _typeText;
        [SerializeField] private UISelectable _selectable;
        [SerializeField] private SelectableColor _visualEffect;
        
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
                    _visualEffect.SetDefaultColor(new Color(0.5f, 0.5f, 0.5f));
                    break;
                case EItemType.Eat:
                    _visualEffect.SetDefaultColor(new Color(0.8f, 0.2f, 0.2f));
                    break;
                case EItemType.Weapons:
                    _visualEffect.SetDefaultColor(new Color(0.8f, 0.5f, 0.2f));
                    break;
                case EItemType.Tools:
                    _visualEffect.SetDefaultColor(new Color(0.3f, 0.5f, 0.2f));
                    break;
                case EItemType.Equipment:
                    _visualEffect.SetDefaultColor(new Color(0.3f, 0.5f, 0.7f));
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
            IsSelected = true;
            _selectable.SetInteractable(false);
            _visualEffect.SetSelectedMode();
        }

        public void Deselect()
        {
            IsSelected = false;
            _selectable.SetInteractable(true);
            _visualEffect.SetDefaultMode();
        }
    }
}