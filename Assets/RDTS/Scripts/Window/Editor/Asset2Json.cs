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

        int number_ItemType = 0;//����
        int number_Item = 0;//����
        string js;
        List<string> menuItem = new List<string>();//���������е��豸��������
        string WriteJsonFile = "Assets/RDTS/Scripts/Window/Editor/DeviceWindow/jsonInfo/";//�ļ�д��·��

        public void ReadDeviceLibraryJsonData(DeviceLibraryData data)
        {

            List<ModuleLibrary> modulelibraries = new List<ModuleLibrary>();
            number_ItemType = data.DeviceLibraryDataList.Count;//��ȡһ���ж������͵��豸
            foreach (var deviceLibrary in data.DeviceLibraryDataList)//����ÿһ������
            {

                ModuleLibrary thisModulelibrary = new ModuleLibrary();
                thisModulelibrary.name = deviceLibrary.name;

                if (!menuItem.Contains(deviceLibrary.name))//�����豸�������Ƶ��б�
                {
                    menuItem.Add(deviceLibrary.name);
                }

                foreach (var device in deviceLibrary.devices)//����ÿһ���豸
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
                number_Item += deviceLibrary.devices.Count;//���һ���ж��ٸ��豸
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
                //��������
                sw.Write(js);
                //�ر��ĵ�
                sw.Close();
                sw.Dispose();
            }
        }
        public List<ModuleLibrary> ReadData(string fileUrl)
        {
            List<ModuleLibrary> modulelibraries = new List<ModuleLibrary>();
            //string���͵����ݳ���
            string readData;

            if (Directory.Exists(fileUrl))
            {
                DirectoryInfo direction = new DirectoryInfo(fileUrl);
                FileInfo[] files = direction.GetFiles("*");
                for (int i = 0; i < files.Length; i++)
                {
                    //���Թ����ļ�
                    if (files[i].Name.EndsWith(".meta"))
                    {
                        continue;
                    }
                    using (StreamReader sr = File.OpenText(files[i].FullName))
                    {
                        //���ݱ���
                        readData = sr.ReadToEnd();
                        sr.Close();
                    }
                    ModuleLibrary m_PersonData = JsonUtility.FromJson<ModuleLibrary>(readData);
                    modulelibraries.Add(m_PersonData);
                }
            }
            //��������
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
            public string name;//����
            public string description;//����
            public string textrue;//ͼ�� => [��������ԴԤ����ʽ������Ҫ���textrue]

            ///ģ��
            public string model;//ģ���ļ���һ��ΪԤ���壩
            public string modelPos;//ģ�ͱ���ӵ�λ��

            ///����
            public string material;//����
        }
    }
}

