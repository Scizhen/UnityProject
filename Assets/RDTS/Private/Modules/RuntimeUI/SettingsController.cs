using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RDTS
{
    /// <summary>
    /// 软件Quality设置控制器，针对不同的Level来设置
    /// </summary>
    public class SettingsController : MonoBehaviour
    {

        public GameObject window;

        //开始先读取注册表中的“Quality”数值
        public void Start()
        {
            int quality = PlayerPrefs.GetInt("Quality", -1);
            if (quality != -1)
                QualitySettings.SetQualityLevel(quality, true);
            quality = QualitySettings.GetQualityLevel();//返回当前的图形品质级别
            var tog = GetComponentsInChildren<QualityToggleChange>();
            foreach (var to in tog)
            {
                to.SetQualityStatus(quality);
            }
        }

        //在QualityToggleChange脚本中被调用，用来设置品质级别，并保存在注册表中
        public void OnQualityToggleChanged(int qualitylevel)
        {
            QualitySettings.SetQualityLevel(qualitylevel, true); //设置新的图形品质级别
            PlayerPrefs.SetInt("Quality", qualitylevel);
            PlayerPrefs.Save();
        }

        //应在Inspector中Button的OnClick的中北调用
        public void CloseSettingsWindow()
        {
            window.SetActive(false);
        }

    }
}

