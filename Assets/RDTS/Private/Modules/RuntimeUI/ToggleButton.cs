using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RDTS
{
    public enum ButtonColor
    {
        White,
        Yellow,
        Green,
        Red,
    }

    public class ToggleButton : RDTSUI
    {
        private Toggle _toggle;
        private Button _button;

        private Image _activebackground;
        // Use this for initialization

        void Start()
        {
            _toggle = GetComponent<Toggle>();
            _button = GetComponent<Button>();
            _activebackground = GetComponent<Image>();
            if (_toggle != null)
            {
                _toggle.onValueChanged.AddListener(delegate { ToggleValueChanged(_toggle); });//当 Toggle 的状态发生变化时添加监听器，以采取行动
                ToggleValueChanged(_toggle);
            }
            if (_button != null)
            {
                _button.onClick.AddListener(delegate { ButtonClicked(_button); });
            }
        }

        public void SetToggleOn(string buttonname)
        {
            if (gameObject.name != buttonname)
            {
                return;
            }
            _toggle.isOn = true;

        }

        public void SetToggleOff(string buttonname)
        {
            if (gameObject.name != buttonname)
            {
                return;
            }
            _toggle.isOn = false;

        }


        //Output the new state of the Toggle into Text
        void ButtonClicked(Button button)
        {
            ButtonHighLightOn();
            RDTSController.OnUIButtonPressed(gameObject);
            Invoke("ButtonHighLightOff", 0.5f * 1 / Time.timeScale);
        }

        void ButtonHighLightOff()
        {
            if (_activebackground != null)
            {
                _activebackground.enabled = false;
            }

        }

        void ButtonHighLightOn()
        {
            if (_activebackground != null)
            {
                _activebackground.enabled = true;
            }

        }


        void ToggleValueChanged(Toggle change)
        {
            if (_toggle.isOn)
            {
                _toggle.targetGraphic.enabled = false;
                ButtonHighLightOn();
            }
            else
            {
                _toggle.targetGraphic.enabled = true;
                ButtonHighLightOff();
            }
            RDTSController.OnUIButtonPressed(gameObject);
        }

    }
}