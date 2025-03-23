using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace RDTS.Data
{

    [CreateAssetMenu(menuName = "Parallel-RDTS/Create ScriptLibraryData", fileName = "ScriptLibraryData")]
    public class ScriptLibraryData : ScriptableObject//���нű�
    {

        public List<ScriptLibrary> ScriptLibraryDataList = new List<ScriptLibrary>() { new ScriptLibrary()};

    }

    [Serializable]
    public class ScriptLibrary//һ��ű�
    {
        public string name;//�ű���������
        public List<Script> scripts;

    }

    [Serializable]
    public class Script//һ���ű�
    {
        public string name;//�ű�����
        public Texture icon;//ͼ��
        public ScriptIntroduction introduction = new ScriptIntroduction();//�ű���������

    }

    [Serializable]
    public class ScriptIntroduction//һ����������
    {
        public string function;//����
        public string apply;//ʹ��
        public string notice;//ע��
    }




}
