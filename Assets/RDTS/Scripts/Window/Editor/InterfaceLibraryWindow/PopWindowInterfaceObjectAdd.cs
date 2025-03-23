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
    /// �������������һ���ӿ����
    /// </summary>
    public class PopWindowInterfaceObjectAdd : EditorWindow
    {
        private InterfaceLibraryWindow interfaceLibraryWindow;

        private string _objectType = "";//����
        public string objectType { get { return _objectType; } }//���ⲿ��ȡ�˶�������
        private string _objectName = "";//����
        public string objectName { get { return _objectName; } }//���ⲿ��ȡ�˶�������

        string[] Menu_interfaceType;

        void OnGUI()
        {
            GUILayout.Space(5);
            GUILayout.Label("����������Ϣ����Ӷ���", labelStyle);
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            GUILayout.Label("�ӿ�����", GUILayout.Width(position.width / 3), GUILayout.Height(25));
            GUILayout.FlexibleSpace();
            _objectType  = DropDownMenu(_objectType, Menu_interfaceType);
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(GUILayout.Height(25));
            GUILayout.Space(30);
            GUILayout.Label("�ӿ�����", GUILayout.Width(position.width / 3), GUILayout.Height(25));
            GUILayout.FlexibleSpace();
            _objectName = GUILayout.TextField(_objectName, GUILayout.Width(position.width/2), GUILayout.Height(22));
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            GUILayout.Space(35);
            //��ť��������õĽӿڶ���
            if (GUILayout.Button("Add", GUILayout.Width(60), GUILayout.Height(28), GUILayout.ExpandWidth(true)))
            {
                bool isNameNull = false;
                if (_objectName == "")
                    isNameNull = true;
                    
                if(isNameNull)
                {
                    //���ڡ�����������£�ѡ���ˡ�yes�������Ĭ��Ԥ����������������������ѡ��no����ȡ���˴β���
                    if(EditorUtility.DisplayDialog("Error", "��������Ϊ�գ�", "yes", "no"))
                    {
                        ///����LinkInterfaceLibraryWindow()���������������ര�ڣ��ʲ���ʹ��GetWindow()����
                        ///InterfaceLibraryWindow interfaceLibraryWindow = (InterfaceLibraryWindow)EditorWindow.GetWindow(typeof(InterfaceLibraryWindow));
                        try
                        {
                            //interfaceLibraryWindow.GetOneInterfaceObjectByPopWindow(objectType, objectName);
                            interfaceLibraryWindow.SetCurrentInterfaceObject(interfaceLibraryWindow.GetOneInterfaceObjectByPopWindow(objectType, objectName));//���õ�ǰҪ��ӵĽӿڶ�����Ϣ��
                            this.Close();//�رմ˴���
                        }
                        catch
                        {

                        }
                    }
                    
                }
                else//����������������£�ֱ�Ӵ�������
                {
                    ///����LinkInterfaceLibraryWindow()���������������ര�ڣ��ʲ���ʹ��GetWindow()����
                    ///InterfaceLibraryWindow interfaceLibraryWindow = (InterfaceLibraryWindow)EditorWindow.GetWindow(typeof(InterfaceLibraryWindow));
                    try
                    {
                        interfaceLibraryWindow.SetCurrentInterfaceObject(interfaceLibraryWindow.GetOneInterfaceObjectByPopWindow(objectType, objectName));//���õ�ǰҪ��ӵĽӿڶ�����Ϣ��
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

            //InterfaceLibraryWindow window = (InterfaceLibraryWindow)EditorWindow.GetWindow(typeof(InterfaceLibraryWindow));
            //Menu_interfaceType = window.GetMenu_interfaceType;

            //Select_String = Menu_interfaceType[0];

            //_objectName = "new interface object";
        }

        /// <summary>
        /// ��InterfaceLibraryWindow��˴��ڹ���
        /// </summary>
        /// <param name="interfaceLibraryWindow"></param>
        public void LinkInterfaceLibraryWindow(InterfaceLibraryWindow interfaceLibraryWindow)
        {
            this.interfaceLibraryWindow = interfaceLibraryWindow;
            Menu_interfaceType = this.interfaceLibraryWindow.GetMenu_interfaceType;
            Select_String = Menu_interfaceType[0];
        }


        /*�����˵�����ģ�顾�л�ʱ������˸���󣬿��Ż���*/
        string Select_String = "";
        string DropDownMenu(string content, string[] itemList)
        {

            if (EditorGUILayout.DropdownButton(new GUIContent(content), FocusType.Keyboard ,GUILayout.Width(position.width / 2), GUILayout.Height(25)))
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
