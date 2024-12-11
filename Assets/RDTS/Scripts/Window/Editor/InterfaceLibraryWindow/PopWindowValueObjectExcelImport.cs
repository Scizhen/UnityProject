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
    ///  ���������ڵ���Excel�ļ���һ�ű�������
    /// </summary>
    public class PopWindowValueObjectExcelImport : EditorWindow
    {
        private InterfaceLibraryWindow interfaceLibraryWindow;
        private string _tipPath = "no flie is selected...";//��ʾ�ļ�·��
        private string filePath;
        System.Data.DataSet fileResult;//�ļ����
        System.Data.DataTableCollection fileTables;//�ļ���

        Vector2 scroll;
        string sheetName;
        void OnGUI()
        {
            GUILayout.Space(5);
;           ///��ť��ѡ��Ҫ������ļ�
            if (GUILayout.Button("Select File", GUILayout.Height(28), GUILayout.ExpandWidth(true)))
            {
                try
                {
                    //ѡ��Ҫ��ȡ�ı�
                    var bp = new Extension.FileBrowser.BrowserProperties();
                    ///�ļ�����ɸѡ
                    // bp.filter = @"Excel���|*.xlsx|Excel|*.xls|Excel|*.csv";
                    bp.filter = "Excel files (*.xlsx, *.xls, *.csv) | *.xlsx; *.xls; *.csv" + "|" + "xlsx files|*.xlsx|xls files|*.xls|csv files|*.csv";
                    bp.filterIndex = 0;

                    ///�������ѡ�񱾵ص�excel�ļ�����¼�ļ�·��
                    new Extension.FileBrowser.FileBrowser().OpenFileBrowser(bp, path =>
                    {
                        filePath = interfaceLibraryWindow.ExcelFilePath =  path;
                    });

                    ///QM.Log("Select Excel File");

                    fileResult = interfaceLibraryWindow.ExcelFileResult = interfaceLibraryWindow.ReadPortalExcelFile(filePath);//�ļ����
                    fileTables = interfaceLibraryWindow.ExcelFileTables = interfaceLibraryWindow.ReadPortalExcelFileTables(fileResult);//�ļ���
                }
                catch
                {
                    EditorUtility.DisplayDialog("�ļ����ʹ���", "ѡ����ļ����� *.xlsx �� *.xls��������ѡ��", "OK");
                }
            }
            ///��ť������ѡ��ı�����
            if (GUILayout.Button("Import File", GUILayout.Height(28), GUILayout.ExpandWidth(true)))
            {
                try
                {
                    interfaceLibraryWindow.ReadPortalExcelFileRows(fileResult, sheetName);//��ȡ����
                    this.Close();//�رմ˴���
                }
                catch
                {
                    EditorUtility.DisplayDialog("��ѡ�����", "��ѡ��һ����Ч�ı���", "OK");
                }

            }
            GUILayout.Space(10);

            ///��ʾ�ļ�·��
            GUILayout.Label("��ǰ�ļ�·����", labelStyle);
            GUILayout.TextField(filePath, (filePath == _tipPath) ? labelStyle_Italic: labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(10);

            ///���б�
            GUILayout.Label("ѡ��Ҫ��ȡ�ı���", labelStyle);
            scroll = GUILayout.BeginScrollView(scroll, GUILayout.ExpandWidth(true));///������
            if(fileTables != null)
            {
                for (int i = 0; i < fileTables.Count; i++)
                {
                    GUILayout.BeginHorizontal("Box");

                    if (GUILayout.Button(image_select, GUILayout.Width(30), GUILayout.Height(20)))//ѡ��ť
                    {
                        sheetName = fileTables[i].ToString();
                    }
                    GUILayout.Label(fileTables[i].ToString(), GUILayout.ExpandWidth(true));//����

                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndScrollView();///������
            GUILayout.Space(10);

            ///��ʾѡ��ı�
            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("ѡ�еı���", labelStyle);
            GUILayout.Label(sheetName);
            GUILayout.EndHorizontal();

        }

        private GUISkin skin_interfaceLibrary;
        private Texture image_select;//ѡ��ͼ��
        private GUIStyle labelStyle;//labelԪ�ص�GUI style
        private GUIStyle labelStyle_Italic;//�Զ��屳��ͼƬ�ı�ǩ��ʽ
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
            //����֮ǰ��������ļ������еĻ���
            
            fileResult = interfaceLibraryWindow.ExcelFileResult;//�ļ����
            fileTables = interfaceLibraryWindow.ExcelFileTables;//�ļ���
            filePath = (fileTables != null)?interfaceLibraryWindow.ExcelFilePath: _tipPath;//�ļ�·��
        }




    }



#endif
}
