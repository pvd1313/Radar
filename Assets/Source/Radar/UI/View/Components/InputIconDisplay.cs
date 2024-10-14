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
                    SetTag(_gamePadButton);
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
                        case EInputButton.South:
                        default:
                            gameObject.SetActive(false);
                            return;
                        case EInputButton.RightShoulder:
                            SetTag(EInputButton.Tab);
                            break;
                        case EInputButton.LeftTrigger:
                            SetTag(EInputButton.Q);
                            break;
                        case EInputButton.RightTrigger:
                            SetTag(EInputButton.E);
                            break;
                        case EInputButton.West:
                            SetTag(EInputButton.One);
                            break;
                        case EInputButton.North:
                            SetTag(EInputButton.Two);
                            break;
                        case EInputButton.East:
                            SetTag(EInputButton.Three);
                            break;
                    }
                    if (!gameObject.activeSelf)
                    {
                        gameObject.SetActive(true);
                    }
                    break;
            }
        }

        private void SetTag(EInputButton button)
        {
            _text.SetText($"<sprite name={button.ToString()}>");
        }
    }
}