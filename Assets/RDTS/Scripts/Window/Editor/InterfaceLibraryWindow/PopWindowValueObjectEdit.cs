using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using RDTS;
using RDTS.Utility;
using RDTS.Interface;
using RDTS.Data;

namespace RDTS.Window
{
#if UNITY_EDITOR


    /// <summary>
    /// ���������ڱ༭һ��ֵ����
    /// </summary>
    public class PopWindowValueObjectEdit : EditorWindow
    {
        private InterfaceLibraryWindow interfaceLibraryWindow;

        private string _objectType = "";//����
        public string objectType { get { return _objectType; } }//���ⲿ��ȡ�˶�������

        private string _objectName = "";//����
        public string objectName { get { return _objectName; } }//���ⲿ��ȡ�˶�������

        private string _objectAddress = "";//��ַ
        public string objectAddress { get { return _objectAddress; } }//���ⲿ��ȡ�˶����ַ

        string[] Menu_valueType;


        void OnGUI()
        {
            GUILayout.Space(5);
            GUILayout.Label("����������Ϣ����Ӷ���", labelStyle);
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            GUILayout.Label("��������", GUILayout.Width(position.width / 3), GUILayout.Height(25));
            GUILayout.FlexibleSpace();
            _objectType = DropDownMenu(_objectType, Menu_valueType);
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Height(25));
            GUILayout.Space(30);
            GUILayout.Label("��������", GUILayout.Width(position.width / 3), GUILayout.Height(25));
            GUILayout.FlexibleSpace();
            _objectName = GUILayout.TextField(_objectName, GUILayout.Width(position.width / 2), GUILayout.Height(22));
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Height(25));
            GUILayout.Space(30);
            GUILayout.Label("��ַ", GUILayout.Width(position.width / 3), GUILayout.Height(25));
            GUILayout.FlexibleSpace();
            _objectAddress = GUILayout.TextField(_objectAddress, GUILayout.Width(position.width / 2), GUILayout.Height(22));
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
            GUILayout.Space(15);
            EditorGUILayout.HelpBox("ע���ַӦ������ƥ��", MessageType.Info);

            GUILayout.FlexibleSpace();
            //��ť���༭���õĽӿڶ���
            if (GUILayout.Button("Edit", GUILayout.Width(60), GUILayout.Height(28), GUILayout.ExpandWidth(true)))
            {
                if (_objectName == "")
                {
                    EditorUtility.DisplayDialog("Error", "��������Ϊ�գ�", "OK");
                }
                else if (_objectAddress == "")
                {
                    EditorUtility.DisplayDialog("Error", "�����ַΪ�գ�", "OK");
                }
                else//����������Ϣ����£��ɱ༭����
                {

                    try
                    {
                        interfaceLibraryWindow.GetEditInfoByPopWindow(objectType, objectName, objectAddress);
                        this.Close();//�رմ˴���
                    }
                    catch
                    {

                    }
                }



            }

        }


        private GUISkin skin_interfaceLibrary;
        private GUIStyle labelStyle;//labelԪ�ص�GUI style
        private GUIStyle labelStyle_MiddleCenter;
        void OnEnable()
        {
            skin_interfaceLibrary = AssetDatabase.LoadAssetAtPath<GUISkin>("Assets/RDTS/Scripts/Window/Editor/InterfaceLibraryWindow/WindowInterfaceLibrary.guiskin");

            labelStyle = skin_interfaceLibrary.customStyles[6];
            labelStyle_MiddleCenter = skin_interfaceLibrary.customStyles[7];
        }


        /// <summary>
        /// ��InterfaceLibraryWindow��˴��ڹ���
        /// </summary>
        /// <param name="interfaceLibraryWindow"></param>
        public void LinkInterfaceLibraryWindow(InterfaceLibraryWindow interfaceLibraryWindow)
        {
            this.interfaceLibraryWindow = interfaceLibraryWindow;
            Menu_valueType = this.interfaceLibraryWindow.GetMenu_valueType;
            var editValueObj = this.interfaceLibraryWindow.GetEditedValueObject();
            Select_String = Menu_valueType.ToList().First(type => type == editValueObj.datatype) ?? Menu_valueType[0];
            _objectName = editValueObj.name;
            _objectAddress = editValueObj.address;
        }


        /*�����˵�����ģ�顾�л�ʱ������˸���󣬿��Ż���*/
        string Select_String = "";
        string DropDownMenu(string content, string[] itemList)
        {

            if (EditorGUILayout.DropdownButton(new GUIContent(content), FocusType.Keyboard, GUILayout.Width(position.width / 2), GUILayout.Height(25)))
            {
                GenericMenu menu = new GenericMenu();

                if (itemList.Length <= 0)
                    return null;

                foreach (string item in itemList)
                {
                    if (string.IsNullOrEmpty(item)) continue;
                    menu.AddItem(new GUIContent(item), content.Equals(item), OnValueSelected, item);
                }

                menu.ShowAsContext();
            }

            return Select_String;
        }

        void OnValueSelected(object value)
        {
            Select_String = value.ToString();

            // QM.Log(Select_String);
        }



    }






#endif
}
