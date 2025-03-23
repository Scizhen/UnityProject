using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace RDTS.Data
{
    /// <summary>
    /// 在Project中右键选择"Create/Parallel-RDTS/Create MaterialLibraryData"即可创建对应的asset文件
    /// </summary>
    [CreateAssetMenu(menuName = "Parallel-RDTS/Create MaterialLibraryData", fileName = "MaterialLibraryData")]
    public class MaterialLibraryData : ScriptableObject
    {
        public List<MaterialLibrary> MaterialLibraryDataList = new List<MaterialLibrary>() { new MaterialLibrary() };
    }


    [Serializable]
    public class MaterialLibrary//一类材质
    {
        public string name;
        public List<MaterialSingle> materials = new List<MaterialSingle>();

    }

    [Serializable]
    public class MaterialSingle//一个材质
    {
        ///基础信息
        public string name;
        public string description;
        public Texture textrue;//
        public Material material;

    }


}
