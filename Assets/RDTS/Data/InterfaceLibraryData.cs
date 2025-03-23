using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace RDTS.Data
{
    /// <summary>
    /// 在Project中右键选择"Create/Parallel-RDTS/Create InterfaceLibraryData"即可创建对应的asset文件
    /// </summary>
    [CreateAssetMenu(menuName = "Parallel-RDTS/Create InterfaceLibraryData", fileName = "InterfaceLibraryData")]
    public class InterfaceLibraryData : ScriptableObject
    {

        public List<InterfaceItem> interfaceItems = new List<InterfaceItem>();

    }



    [Serializable]
    public class InterfaceItem//一类接口模块
    {
        public string name;
        public GameObject interfaceObject;


    }


}
