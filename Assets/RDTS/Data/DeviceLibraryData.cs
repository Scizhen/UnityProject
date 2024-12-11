using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



namespace RDTS.Data
{
    /// <summary>
    /// ��Project���Ҽ�ѡ��"Create/Parallel-RDTS/Create DeviceLibraryData"���ɴ�����Ӧ��asset�ļ�
    /// </summary>
    [CreateAssetMenu(menuName = "Parallel-RDTS/Create DeviceLibraryData", fileName = "DeviceLibraryData")]
    public class DeviceLibraryData : ScriptableObject//�����豸
    {
        public List<DeviceLibrary> DeviceLibraryDataList = new List<DeviceLibrary>() { new DeviceLibrary() };


    }


    [Serializable]
    public class DeviceLibrary//һ���豸
    {
        public string name;
        public List<Device> devices = new List<Device>();

    }

    [Serializable]
    public class Device//һ���豸
    {
        ///������Ϣ
        public string name;
        public string description;
        public Texture textrue;//
        public GameObject model;
        ///��ϸ��Ϣ

    }
}
