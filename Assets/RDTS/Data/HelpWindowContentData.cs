using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




namespace RDTS.Data
{
    /// <summary>
    /// ��Project���Ҽ�ѡ��"Create/Parallel-RDTS/Create HelpWindowContentData"���ɴ�����Ӧ��asset�ļ�
    /// </summary>
    [CreateAssetMenu(menuName = "Parallel-RDTS/Create HelpWindowContentData", fileName = "HelpWindowContentData")]
    public class HelpWindowContentData : ScriptableObject
    {
        public List<HelpData> helpDatas = new List<HelpData>() { new HelpData()};

    }


    [Serializable]
    public class HelpData
    {
        public string name;//��������
        public List<string> contents = new List<string>();//��������
    }



}