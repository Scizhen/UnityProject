using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RDTS.Data;
using System;
using System.IO;

namespace RDTS.Window1
{
    public class Asset2Json : MonoBehaviour
    {
        public DeviceLibraryData devicelibraryData;
        // Start is called before the first frame update
        void Start()
        {
            ReadDeviceLibraryJsonData(devicelibraryData);
            List<ModuleLibrary> jsonData = ReadData(WriteJsonFile);

        }

        int number_ItemType = 0;//种类
        int number_Item = 0;//数量
        string js;
        List<string> menuItem = new List<string>();//储存数据中的设备类型名称
        string WriteJsonFile = "Assets/RDTS/Scripts/Window/Editor/DeviceWindow/jsonInfo/";//文件写入路径

        public void ReadDeviceLibraryJsonData(DeviceLibraryData data)
        {

            List<ModuleLibrary> modulelibraries = new List<ModuleLibrary>();
            number_ItemType = data.DeviceLibraryDataList.Count;//获取一共有多少类型的设备
            foreach (var deviceLibrary in data.DeviceLibraryDataList)//遍历每一种类型
            {

                ModuleLibrary thisModulelibrary = new ModuleLibrary();
                thisModulelibrary.name = deviceLibrary.name;

                if (!menuItem.Contains(deviceLibrary.name))//储存设备类型名称的列表
                {
                    menuItem.Add(deviceLibrary.name);
                }

                foreach (var device in deviceLibrary.devices)//遍历每一个设备
                {
                    Module thisModule = new Module();
                    thisModule.name = device.name;
                    thisModule.description = device.description;
                    thisModule.textrue = AssetDatabase.GetAssetPath(device.textrue);
                    thisModule.model = AssetDatabase.GetAssetPath(device.model);
                    thisModulelibrary.modules.Add(thisModule);


                }
                js = JsonUtility.ToJson(thisModulelibrary);
                WriteData(WriteJsonFile + thisModulelibrary.name + ".json");
                modulelibraries.Add(thisModulelibrary);
                number_Item += deviceLibrary.devices.Count;//获得一共有多少个设备
            }






        }
        public void WriteData(string fileUrl)
        {
            string dir_name = Path.GetDirectoryName(fileUrl);
            if (!Directory.Exists(dir_name))
            {
                Directory.CreateDirectory(dir_name);
            }
            using (StreamWriter sw = new StreamWriter(fileUrl))
            {
                //保存数据
                sw.Write(js);
                //关闭文档
                sw.Close();
                sw.Dispose();
            }
        }
        public List<ModuleLibrary> ReadData(string fileUrl)
        {
            List<ModuleLibrary> modulelibraries = new List<ModuleLibrary>();
            //string类型的数据常量
            string readData;

            if (Directory.Exists(fileUrl))
            {
                DirectoryInfo direction = new DirectoryInfo(fileUrl);
                FileInfo[] files = direction.GetFiles("*");
                for (int i = 0; i < files.Length; i++)
                {
                    //忽略关联文件
                    if (files[i].Name.EndsWith(".meta"))
                    {
                        continue;
                    }
                    using (StreamReader sr = File.OpenText(files[i].FullName))
                    {
                        //数据保存
                        readData = sr.ReadToEnd();
                        sr.Close();
                    }
                    ModuleLibrary m_PersonData = JsonUtility.FromJson<ModuleLibrary>(readData);
                    modulelibraries.Add(m_PersonData);
                }
            }
            //返回数据
            return modulelibraries;
        }



        [Serializable]
        public class ModuleLibraryAll
        {
            public List<ModuleLibrary> moduleLibrary = new List<ModuleLibrary>();
        }

        [Serializable]
        public class ModuleLibrary
        {
            public string name;
            public List<Module> modules = new List<Module>();
        }
        [Serializable]
        public class Module
        {
            public string name;//名称
            public string description;//描述
            public string textrue;//图标 => [若采用资源预览方式，则不需要添加textrue]

            ///模型
            public string model;//模型文件（一般为预制体）
            public string modelPos;//模型被添加的位置

            ///材质
            public string material;//材质
        }
    }
}

