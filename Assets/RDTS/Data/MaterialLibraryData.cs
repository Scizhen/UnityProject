using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace RDTS.Data
{
    /// <summary>
    /// ��Project���Ҽ�ѡ��"Create/Parallel-RDTS/Create MaterialLibraryData"���ɴ�����Ӧ��asset�ļ�
    /// </summary>
    [CreateAssetMenu(menuName = "Parallel-RDTS/Create MaterialLibraryData", fileName = "MaterialLibraryData")]
    public class MaterialLibraryData : ScriptableObject
    {
        public List<MaterialLibrary> MaterialLibraryDataList = new List<MaterialLibrary>() { new MaterialLibrary() };
    }


    [Serializable]
    public class MaterialLibrary//һ�����
    {
        public string name;
        public List<MaterialSingle> materials = new List<MaterialSingle>();

    }

    [Serializable]
    public class MaterialSingle//һ������
    {
        ///������Ϣ
        public string name;
        public string description;
        public Texture textrue;//
        public Material material;

    }


}
