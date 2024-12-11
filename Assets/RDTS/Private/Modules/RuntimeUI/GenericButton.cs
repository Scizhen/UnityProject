using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;


namespace RDTS
{

    [System.Serializable]
    //! Unity event when clicking on GenericButton
    public class ButtonEventOnClick : UnityEngine.Events.UnityEvent<GenericButton> { }


    /// <summary>
    /// 通用按钮：点击切换图标和状态
    /// </summary>
    public class GenericButton : RDTSUI
    {

        [Header("Status")]
        public bool IsOn;
        public bool IsPressed;


        [Header("Display Settings")]
        [OnValueChanged("UpdateStatus")]
        public Sprite ImageOn;
        [OnValueChanged("UpdateStatus")]
        public Sprite ImageOff;
        public Image ActiveImageOnOn;


        [Header("Button Actions")]
        [ReorderableList] public List<GameObject> ActivateOnOn;
        [ReorderableList] public List<GameObject> ActivateOnOff;
        [ReorderableList] public List<GameObject> ActivateOnClick;
        public ButtonEventOnClick EventOnClick;
        private Button _button;

        private Image _image;
        // Use this for initialization

        public void SetColor(Color color)
        {
            var colors = GetComponent<Button>().colors;
            colors.normalColor = color;
            colors.selectedColor = color;
            GetComponent<Button>().colors = colors;

        }


        void Start()
        {

            _button = GetComponent<Button>();//获取此对象上的Button组件
            _image = GetComponent<Image>();//获取此对象上的Image组件

            if (_button != null)
            {
                _button.onClick.AddListener(delegate { ButtonClicked(_button); });
            }
            UpdateButton();
        }


        public void SetStatus(bool ison)
        {
            IsOn = ison;
            UpdateButton();
        }

        void UpdateButton()
        {
            _image = GetComponent<Image>();
            if (IsOn)
            {
                if (ImageOn != null)
                    _image.overrideSprite = ImageOn;
                if (ActiveImageOnOn != null)
                    ActiveImageOnOn.enabled = true;
                if (ActivateOnOn != null)
                {
                    foreach (var go in ActivateOnOn)
                    {
                        if (go != null)
                            go.SetActive(true);
                    }
                }
                if (ActivateOnOff != null)
                {
                    foreach (var go in ActivateOnOff)
                    {
                        if (go != null)
                            go.SetActive(false);
                    }
                }

            }
            else
            {
                if (ImageOff != null)
                    _image.overrideSprite = ImageOff;
                if (ActiveImageOnOn != null)
                    ActiveImageOnOn.enabled = true;
                if (ActivateOnOn != null)
                {
                    foreach (var go in ActivateOnOn)
                    {
                        if (go != null)
                            go.SetActive(false);
                    }
                }
                if (ActivateOnOff != null)
                {
                    foreach (var go in ActivateOnOff)
                    {
                        if (go != null)
                            go.SetActive(true);
                    }
                }

            }

        }
        //Output the new state of the Toggle into Text
        void ButtonClicked(Button button)
        {

            IsOn = !IsOn;
            UpdateButton();
            if (ActivateOnClick != null)
            {
                foreach (var go in ActivateOnClick)
                {
                    if (go != null)
                        go.SetActive(true);
                }
            }
            EventOnClick.Invoke(this);

        }


    }
}