using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




namespace RDTS.Data
{
    /// <summary>
    /// 在Project中右键选择"Create/Parallel-RDTS/Create HelpWindowContentData"即可创建对应的asset文件
    /// </summary>
    [CreateAssetMenu(menuName = "Parallel-RDTS/Create HelpWindowContentData", fileName = "HelpWindowContentData")]
    public class HelpWindowContentData : ScriptableObject
    {
        public List<HelpData> helpDatas = new List<HelpData>() { new HelpData()};

    }


    [Serializable]
    public class HelpData
    {
        public string name;//数据名称
        public List<string> contents = new List<string>();//具体数据
    }



}