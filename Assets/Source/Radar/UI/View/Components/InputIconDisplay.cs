using Beast.Radar.InputControl;
using TMPro;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class InputIconDisplay : UIView
    {
        [SerializeField] private EInputButton _gamePadButton;
        [SerializeField] private TextMeshProUGUI _text;

        public void SetKey(EInputButton key)
        {
            _gamePadButton = key;
        }

        public void Refresh(EInputScheme scheme)
        {
            switch (scheme)
            {
                case EInputScheme.None:
                case EInputScheme.Touch:
                default:
                    gameObject.SetActive(false);
                    break;
                case EInputScheme.Gamepad:
                    _setTag(_gamePadButton);
                    if (!gameObject.activeSelf)
                    {
                        gameObject.SetActive(true);
                    }
                    break;
                case EInputScheme.MouseAndKeyboard:
                    switch (_gamePadButton)
                    {
                        case EInputButton.None:
                        case EInputButton.LeftShoulder:
                        case EInputButton.East:
                        default:
                            gameObject.SetActive(false);
                            return;
                        case EInputButton.RightShoulder:
                            _setTag(EInputButton.Tab);
                            break;
                        case EInputButton.LeftTrigger:
                            _setTag(EInputButton.Q);
                            break;
                        case EInputButton.RightTrigger:
                            _setTag(EInputButton.E);
                            break;
                        case EInputButton.South:
                            _setTag(EInputButton.One);
                            break;
                        case EInputButton.West:
                            _setTag(EInputButton.Two);
                            break;
                        case EInputButton.North:
                            _setTag(EInputButton.Three);
                            break;
                    }
                    if (!gameObject.activeSelf)
                    {
                        gameObject.SetActive(true);
                    }
                    break;
            }
        }

        private void _setTag(EInputButton button)
        {
            _text.SetText($"<sprite name={button.ToString()}>");
        }
    }
}