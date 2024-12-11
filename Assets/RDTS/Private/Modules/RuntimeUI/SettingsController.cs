using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RDTS
{
    /// <summary>
    /// ���Quality���ÿ���������Բ�ͬ��Level������
    /// </summary>
    public class SettingsController : MonoBehaviour
    {

        public GameObject window;

        //��ʼ�ȶ�ȡע����еġ�Quality����ֵ
        public void Start()
        {
            int quality = PlayerPrefs.GetInt("Quality", -1);
            if (quality != -1)
                QualitySettings.SetQualityLevel(quality, true);
            quality = QualitySettings.GetQualityLevel();//���ص�ǰ��ͼ��Ʒ�ʼ���
            var tog = GetComponentsInChildren<QualityToggleChange>();
            foreach (var to in tog)
            {
                to.SetQualityStatus(quality);
            }
        }

        //��QualityToggleChange�ű��б����ã���������Ʒ�ʼ��𣬲�������ע�����
        public void OnQualityToggleChanged(int qualitylevel)
        {
            QualitySettings.SetQualityLevel(qualitylevel, true); //�����µ�ͼ��Ʒ�ʼ���
            PlayerPrefs.SetInt("Quality", qualitylevel);
            PlayerPrefs.Save();
        }

        //Ӧ��Inspector��Button��OnClick���б�����
        public void CloseSettingsWindow()
        {
            window.SetActive(false);
        }

    }
}

