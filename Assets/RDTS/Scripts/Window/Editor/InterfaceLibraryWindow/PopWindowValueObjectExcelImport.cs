///********************************************************************************************
///this is part of Robot Digital Twin System.                                                 *
///@Copyright by Shaw.S , 2023                                                                *
///Do not distribute without authorization,thanks!                                            *
///********************************************************************************************
using System.Collections;
using System.Collections.Generic;
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
    ///  弹窗：用于导入Excel文件中一张表单的数据
    /// </summary>
    public class PopWindowValueObjectExcelImport : EditorWindow
    {
        private InterfaceLibraryWindow interfaceLibraryWindow;
        private string _tipPath = "no flie is selected...";//提示文件路径
        private string filePath;
        System.Data.DataSet fileResult;//文件结果
        System.Data.DataTableCollection fileTables;//文件表单

        Vector2 scroll;
        string sheetName;
        void OnGUI()
        {
            GUILayout.Space(5);
;           ///按钮：选择要导入的文件
            if (GUILayout.Button("Select File", GUILayout.Height(28), GUILayout.ExpandWidth(true)))
            {
                try
                {
                    //选择要读取的表单
                    var bp = new Extension.FileBrowser.BrowserProperties();
                    ///文件类型筛选
                    // bp.filter = @"Excel表格|*.xlsx|Excel|*.xls|Excel|*.csv";
                    bp.filter = "Excel files (*.xlsx, *.xls, *.csv) | *.xlsx; *.xls; *.csv" + "|" + "xlsx files|*.xlsx|xls files|*.xls|csv files|*.csv";
                    bp.filterIndex = 0;

                    ///窗口浏览选择本地的excel文件，记录文件路径
                    new Extension.FileBrowser.FileBrowser().OpenFileBrowser(bp, path =>
                    {
                        filePath = interfaceLibraryWindow.ExcelFilePath =  path;
                    });

                    ///QM.Log("Select Excel File");

                    fileResult = interfaceLibraryWindow.ExcelFileResult = interfaceLibraryWindow.ReadPortalExcelFile(filePath);//文件结果
                    fileTables = interfaceLibraryWindow.ExcelFileTables = interfaceLibraryWindow.ReadPortalExcelFileTables(fileResult);//文件表单
                }
                catch
                {
                    EditorUtility.DisplayDialog("文件类型错误", "选择的文件不是 *.xlsx 或 *.xls，请重新选择！", "OK");
                }
            }
            ///按钮：导入选择的表单数据
            if (GUILayout.Button("Import File", GUILayout.Height(28), GUILayout.ExpandWidth(true)))
            {
                try
                {
                    interfaceLibraryWindow.ReadPortalExcelFileRows(fileResult, sheetName);//读取数据
                    this.Close();//关闭此窗口
                }
                catch
                {
                    EditorUtility.DisplayDialog("表单选择错误", "请选择一份有效的表单！", "OK");
                }

            }
            GUILayout.Space(10);

            ///显示文件路径
            GUILayout.Label("当前文件路径：", labelStyle);
            GUILayout.TextField(filePath, (filePath == _tipPath) ? labelStyle_Italic: labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(10);

            ///表单列表
            GUILayout.Label("选择要读取的表单：", labelStyle);
            scroll = GUILayout.BeginScrollView(scroll, GUILayout.ExpandWidth(true));///滚动条
            if(fileTables != null)
            {
                for (int i = 0; i < fileTables.Count; i++)
                {
                    GUILayout.BeginHorizontal("Box");

                    if (GUILayout.Button(image_select, GUILayout.Width(30), GUILayout.Height(20)))//选择按钮
                    {
                        sheetName = fileTables[i].ToString();
                    }
                    GUILayout.Label(fileTables[i].ToString(), GUILayout.ExpandWidth(true));//表单名

                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndScrollView();///滚动条
            GUILayout.Space(10);

            ///显示选择的表单
            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("选中的表单：", labelStyle);
            GUILayout.Label(sheetName);
            GUILayout.EndHorizontal();

        }

        private GUISkin skin_interfaceLibrary;
        private Texture image_select;//选择图标
        private GUIStyle labelStyle;//label元素的GUI style
        private GUIStyle labelStyle_Italic;//自定义背景图片的标签样式
        void OnEnable()
        {
            skin_interfaceLibrary = AssetDatabase.LoadAssetAtPath<GUISkin>("Assets/RDTS/Scripts/Window/Editor/InterfaceLibraryWindow/WindowInterfaceLibrary.guiskin");
            image_select = AssetDatabase.LoadAssetAtPath<Texture>("Assets/RDTS/Private/Resources/Icons/WindowIcon/selection.png");
            labelStyle = skin_interfaceLibrary.customStyles[6];
            labelStyle_Italic = skin_interfaceLibrary.customStyles[9];
            filePath = _tipPath;
        }

        public void LinkInterfaceLibraryWindow(InterfaceLibraryWindow interfaceLibraryWindow)
        {
            this.interfaceLibraryWindow = interfaceLibraryWindow;
            //保留之前导入过的文件（若有的话）
            
            fileResult = interfaceLibraryWindow.ExcelFileResult;//文件结果
            fileTables = interfaceLibraryWindow.ExcelFileTables;//文件表单
            filePath = (fileTables != null)?interfaceLibraryWindow.ExcelFilePath: _tipPath;//文件路径
        }




    }



#endif
}
