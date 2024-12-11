using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace RDTS
{
    /// <summary>
    /// �����ⲿ������qualitylevelֵ�����жϻ����������Quality
    /// </summary>
    public class QualityToggleChange : MonoBehaviour
    {
        public SettingsController settingscontroller;
        private Toggle toggle;
        public int qualitylevel;//Quality��ֵ

        void Awake()
        {
            toggle = GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(OnQualityToggleChanged);
        }

        //����qualityֵ�����õ�ǰToggle��״̬
        public void SetQualityStatus(int quality)
        {
            if (quality == qualitylevel)
                toggle.isOn = true;
        }
        //���Toggleֵ�ı䣬����active״̬��ison = true��������SettingsController�еķ��������á�Quality��
        public void OnQualityToggleChanged(bool ison)
        {
            if (ison)
                settingscontroller.OnQualityToggleChanged(qualitylevel);
        }


    }
}

