using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace RDTS
{
    /// <summary>
    /// 根据外部给定的qualitylevel值，来判断或设置软件的Quality
    /// </summary>
    public class QualityToggleChange : MonoBehaviour
    {
        public SettingsController settingscontroller;
        private Toggle toggle;
        public int qualitylevel;//Quality的值

        void Awake()
        {
            toggle = GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(OnQualityToggleChanged);
        }

        //根据quality值来设置当前Toggle的状态
        public void SetQualityStatus(int quality)
        {
            if (quality == qualitylevel)
                toggle.isOn = true;
        }
        //如果Toggle值改变，且是active状态（ison = true），调用SettingsController中的方法来设置“Quality”
        public void OnQualityToggleChanged(bool ison)
        {
            if (ison)
                settingscontroller.OnQualityToggleChanged(qualitylevel);
        }


    }
}

