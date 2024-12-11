using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace RDTS.Data
{

    [CreateAssetMenu(menuName = "Parallel-RDTS/Create ScriptLibraryData", fileName = "ScriptLibraryData")]
    public class ScriptLibraryData : ScriptableObject//所有脚本
    {

        public List<ScriptLibrary> ScriptLibraryDataList = new List<ScriptLibrary>() { new ScriptLibrary()};

    }

    [Serializable]
    public class ScriptLibrary//一类脚本
    {
        public string name;//脚本类型名称
        public List<Script> scripts;

    }

    [Serializable]
    public class Script//一个脚本
    {
        public string name;//脚本类名
        public Texture icon;//图标
        public ScriptIntroduction introduction = new ScriptIntroduction();//脚本介绍内容

    }

    [Serializable]
    public class ScriptIntroduction//一个介绍内容
    {
        public string function;//功能
        public string apply;//使用
        public string notice;//注意
    }




}
