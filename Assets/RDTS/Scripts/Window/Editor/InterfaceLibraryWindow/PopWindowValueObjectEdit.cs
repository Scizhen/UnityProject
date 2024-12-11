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
    /// 弹窗：用于编辑一个值对象
    /// </summary>
    public class PopWindowValueObjectEdit : EditorWindow
    {
        private InterfaceLibraryWindow interfaceLibraryWindow;

        private string _objectType = "";//类型
        public string objectType { get { return _objectType; } }//供外部读取此对象类型

        private string _objectName = "";//名称
        public string objectName { get { return _objectName; } }//供外部读取此对象名称

        private string _objectAddress = "";//地址
        public string objectAddress { get { return _objectAddress; } }//供外部读取此对象地址

        string[] Menu_valueType;


        void OnGUI()
        {
            GUILayout.Space(5);
            GUILayout.Label("填入以下信息以添加对象：", labelStyle);
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            GUILayout.Label("数据类型", GUILayout.Width(position.width / 3), GUILayout.Height(25));
            GUILayout.FlexibleSpace();
            _objectType = DropDownMenu(_objectType, Menu_valueType);
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Height(25));
            GUILayout.Space(30);
            GUILayout.Label("对象名称", GUILayout.Width(position.width / 3), GUILayout.Height(25));
            GUILayout.FlexibleSpace();
            _objectName = GUILayout.TextField(_objectName, GUILayout.Width(position.width / 2), GUILayout.Height(22));
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Height(25));
            GUILayout.Space(30);
            GUILayout.Label("地址", GUILayout.Width(position.width / 3), GUILayout.Height(25));
            GUILayout.FlexibleSpace();
            _objectAddress = GUILayout.TextField(_objectAddress, GUILayout.Width(position.width / 2), GUILayout.Height(22));
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
            GUILayout.Space(15);
            EditorGUILayout.HelpBox("注意地址应与类型匹配", MessageType.Info);

            GUILayout.FlexibleSpace();
            //按钮：编辑配置的接口对象
            if (GUILayout.Button("Edit", GUILayout.Width(60), GUILayout.Height(28), GUILayout.ExpandWidth(true)))
            {
                if (_objectName == "")
                {
                    EditorUtility.DisplayDialog("Error", "输入名称为空！", "OK");
                }
                else if (_objectAddress == "")
                {
                    EditorUtility.DisplayDialog("Error", "输入地址为空！", "OK");
                }
                else//正常输入信息情况下，可编辑对象
                {

                    try
                    {
                        interfaceLibraryWindow.GetEditInfoByPopWindow(objectType, objectName, objectAddress);
                        this.Close();//关闭此窗口
                    }
                    catch
                    {

                    }
                }



            }

        }


        private GUISkin skin_interfaceLibrary;
        private GUIStyle labelStyle;//label元素的GUI style
        private GUIStyle labelStyle_MiddleCenter;
        void OnEnable()
        {
            skin_interfaceLibrary = AssetDatabase.LoadAssetAtPath<GUISkin>("Assets/RDTS/Scripts/Window/Editor/InterfaceLibraryWindow/WindowInterfaceLibrary.guiskin");

            labelStyle = skin_interfaceLibrary.customStyles[6];
            labelStyle_MiddleCenter = skin_interfaceLibrary.customStyles[7];
        }


        /// <summary>
        /// 将InterfaceLibraryWindow与此窗口关联
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


        /*下拉菜单功能模块【切换时存在闪烁现象，可优化】*/
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
