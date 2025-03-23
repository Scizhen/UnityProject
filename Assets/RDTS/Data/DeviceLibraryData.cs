using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



namespace RDTS.Data
{
    /// <summary>
    /// 在Project中右键选择"Create/Parallel-RDTS/Create DeviceLibraryData"即可创建对应的asset文件
    /// </summary>
    [CreateAssetMenu(menuName = "Parallel-RDTS/Create DeviceLibraryData", fileName = "DeviceLibraryData")]
    public class DeviceLibraryData : ScriptableObject//所有设备
    {
        public List<DeviceLibrary> DeviceLibraryDataList = new List<DeviceLibrary>() { new DeviceLibrary() };


    }


    [Serializable]
    public class DeviceLibrary//一类设备
    {
        public string name;
        public List<Device> devices = new List<Device>();

    }

    [Serializable]
    public class Device//一个设备
    {
        ///基础信息
        public string name;
        public string description;
        public Texture textrue;//
        public GameObject model;
        ///详细信息

    }
}
