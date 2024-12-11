using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace RDTS.Data
{
    /// <summary>
    /// ��Project���Ҽ�ѡ��"Create/Parallel-RDTS/Create InterfaceLibraryData"���ɴ�����Ӧ��asset�ļ�
    /// </summary>
    [CreateAssetMenu(menuName = "Parallel-RDTS/Create InterfaceLibraryData", fileName = "InterfaceLibraryData")]
    public class InterfaceLibraryData : ScriptableObject
    {

        public List<InterfaceItem> interfaceItems = new List<InterfaceItem>();

    }



    [Serializable]
    public class InterfaceItem//һ��ӿ�ģ��
    {
        public string name;
        public GameObject interfaceObject;


    }


}
