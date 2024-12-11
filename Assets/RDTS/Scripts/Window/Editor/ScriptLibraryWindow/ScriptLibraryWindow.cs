///********************************************************************************************
///this is part of Robot Digital Twin System.                                                 *
///@Copyright by Shaw.S , 2023                                                                *
///Do not distribute without authorization,thanks!                                            *
///********************************************************************************************
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RDTS.Data;
using RDTS.Method;
using RDTS.Utility;
using System;

namespace RDTS.Window
{
#if UNITY_EDITOR

    public class ScriptLibraryWindow : EditorWindow
    {

        ///������ʾ/���ģʽ
        protected enum ShowMode
        {
            BriefMode,//��Ҫ��ʾģʽ
            DetailedMode//��ϸ��ʾģʽ
        }
        protected ShowMode showMode = ShowMode.BriefMode;








        [MenuItem("Parallel-RDTS/Window/ScriptLibraryWindow", false, 200)]
        private static void ShowWindow()
        {
            ScriptLibraryWindow scriptWindow = (ScriptLibraryWindow)EditorWindow.GetWindow(typeof(ScriptLibraryWindow));
            scriptWindow.titleContent = ScriptLibraryWindow.thisTitleContent;//���ñ����ͼ��
            scriptWindow.minSize = new Vector2(300, 200);
            scriptWindow.Show();
        }


        private void OnGUI()
        {

            if (HasOpenInstances<ScriptLibraryWindow>())//�ű��ⴰ���Ƿ��
            {
                WindowWidth = position.width;//��ȡ���ڿ��
                WindowHeight = position.height;//��ȡ���ڸ߶�

                DrawToolbar();//����������

                if (viewIndex == 0)//������Ϊ0������ƻ�ӭ����
                {
                    WelcomeView();
                }
                else
                {
                    GUILayout.BeginHorizontal();//*
                    DrawScriptMenu(AreaMenu.x, AreaMenu.y, AreaMenu.width, AreaMenu.height);//���ƽű����Ͳ˵�
                    DrawResizer1(AreaResizer1.x, AreaResizer1.y, AreaResizer1.width, AreaResizer1.height);//���Ƶ�����1
                    DrawScriptContent(AreaContent.x, AreaContent.y, AreaContent.width, AreaContent.height);//���ƽű�����������
                    if (showMode == ShowMode.DetailedMode)
                    {
                        DrawResizer2(AreaResizer2.x, AreaResizer2.y, AreaResizer2.width, AreaResizer2.height);//���Ƶ�����2
                        DrawScriptIntroduction(AreaIntroduction.x + 10, AreaIntroduction.y, AreaIntroduction.width, AreaIntroduction.height);//���ƽű���������
                    }
                    GUILayout.EndHorizontal();//*


                    SetAreaRectParam();
                    ModeChange();
                    ProcessEvents(Event.current);
                }

            }


        }





        #region Init

        string dataPath = "Assets/RDTS/Data/";
        string iconPath = "Assets/RDTS/Private/Resources/Icons/WindowIcon/";
        ScriptLibraryData scriptLibraryData;
        static GUIContent thisTitleContent;//��������
        void OnEnable()
        {
            //������Դ��ȡ
            scriptLibraryData = AssetDatabase.LoadAssetAtPath<ScriptLibraryData>(dataPath + "ScriptLibraryData.asset");
            scriptLibraries = ReadLibraryData(scriptLibraryData);
            //���ô��ڱ��⼰ͼ��
            Texture titleIcon = AssetDatabase.LoadAssetAtPath<Texture>(iconPath + "Parallel.png");
            thisTitleContent = new GUIContent("Parallel�ű���", titleIcon);
            //����unity�Դ�ͼ��
            LoadTexture();
            //��ʼ��
            showMode = ShowMode.BriefMode;
            isLockIntroduction = false;
        }


        private GUISkin skin_scriptLibrary;
        private GUIStyle labelStyle_MiddleCenter;//���ı�ǩUI��ʽ
        private GUIStyle labelStyle_MiddleLeft;//���ı�ǩUI��ʽ
        private GUIStyle resizerStyle;

        private Texture image_Script;

        private Texture2D icon_refresh;
        private Texture2D icon_lock;
        private Texture2D icon_help;
        private Texture2D icon_close;
        private Texture2D icon_eye;
        private Texture2D icon_add;

        void LoadTexture()//�����ز�
        {
            skin_scriptLibrary = AssetDatabase.LoadAssetAtPath<GUISkin>("Assets/RDTS/Scripts/Window/Editor/ScriptLibraryWindow/WindowScriptLibrary.guiskin");
            labelStyle_MiddleCenter = skin_scriptLibrary.customStyles[3];
            labelStyle_MiddleLeft = skin_scriptLibrary.customStyles[4];

            image_Script = AssetDatabase.LoadAssetAtPath<Texture>(iconPath + "WindowScriptIcon.png");

            //Style
            resizerStyle = new GUIStyle();
            resizerStyle.normal.background = EditorGUIUtility.Load("icons/d_AvatarBlendBackground.png") as Texture2D;//Ĭ�ϵ�Unity�༭������(����ͼ��)

            //ͼ��
            icon_refresh = EditorGUIUtility.FindTexture("Refresh");
            icon_help = EditorGUIUtility.FindTexture("d__Help@2x");
            icon_close = EditorGUIUtility.FindTexture("d_winbtn_win_close");
            icon_lock = EditorGUIUtility.FindTexture("d_AssemblyLock");
            icon_eye = EditorGUIUtility.FindTexture("d_scenevis_visible_hover@2x");
            icon_add = EditorGUIUtility.FindTexture("d_CreateAddNew");
        }



        #endregion


        #region AreaParameter

        private AreaRectParam AreaMenu = new AreaRectParam();
        private AreaRectParam AreaContent = new AreaRectParam();
        private AreaRectParam AreaIntroduction = new AreaRectParam();
        private AreaRectParam AreaResizer1 = new AreaRectParam();
        private AreaRectParam AreaResizer2 = new AreaRectParam();

        private float resizerWidth1 = 7f;//������1��ȣ��˵�������������֮�䣩
        private float resizerWidth2 = 7f;//������2��ȣ��������ݺͽ�������֮�䣩

        private float menuWidthRatio = 0.15f;//�˵���ռ���ڿ�ȱ���
        private float contentWidthRatio = 0.8f;//�������������ұ�Եռ���ڿ�ȱ��������˵���+������1+�������ݵĿ��ռ���ڿ�ȵı�����

        /// <summary>
        /// ����������(��Ҫ�ı�Rect��ʼ��Ϳ��)
        /// </summary>
        void SetAreaRectParam()
        {

            if (showMode == ShowMode.BriefMode)
            {
                //�˵������Rect����
                AreaMenu.x = 0;//*
                AreaMenu.y = 0 + ToolbarHeight;
                AreaMenu.width = WindowWidth * menuWidthRatio;//*
                AreaMenu.height = WindowHeight - ToolbarHeight;

                //������1��Rect����
                AreaResizer1.x = AreaMenu.width;//*
                AreaResizer1.y = 0 + ToolbarHeight;
                AreaResizer1.width = resizerWidth1;//*
                AreaResizer1.height = WindowHeight - ToolbarHeight;

                //�������������Rect����
                AreaContent.x = AreaMenu.width + AreaResizer1.width;//*
                AreaContent.y = 0 + ToolbarHeight;
                AreaContent.width = WindowWidth * (1 - menuWidthRatio) - AreaResizer1.width;//*
                AreaContent.height = WindowHeight - ToolbarHeight;

            }
            else
            {
                //�˵������Rect����
                AreaMenu.x = 0;//*
                AreaMenu.y = 0 + ToolbarHeight;
                AreaMenu.width = WindowWidth * menuWidthRatio;//*
                AreaMenu.height = WindowHeight - ToolbarHeight;

                //������1��Rect����
                AreaResizer1.x = AreaMenu.width;//*
                AreaResizer1.y = 0 + ToolbarHeight;
                AreaResizer1.width = resizerWidth1;//*
                AreaResizer1.height = WindowHeight - ToolbarHeight;

                //�������������Rect����
                AreaContent.x = AreaMenu.width + AreaResizer1.width;//*
                AreaContent.y = 0 + ToolbarHeight;
                AreaContent.width = WindowWidth * contentWidthRatio - AreaMenu.width - AreaResizer1.width;//*
                AreaContent.height = WindowHeight - ToolbarHeight;

                //������2��Rect����
                AreaResizer2.x = AreaMenu.width + AreaResizer1.width + AreaContent.width;//*
                AreaResizer2.y = 0 + ToolbarHeight;
                AreaResizer2.width = resizerWidth2;//*
                AreaResizer2.height = WindowHeight - ToolbarHeight;

                //�������������Rect����
                AreaIntroduction.x = AreaMenu.width + AreaResizer1.width + AreaContent.width + AreaResizer2.width;//*
                AreaIntroduction.y = 0 + ToolbarHeight;
                AreaIntroduction.width = WindowWidth * (1 - contentWidthRatio);//*
                AreaIntroduction.height = WindowHeight - ToolbarHeight;

            }


        }



        #endregion


        #region ViewContent

        int viewIndex = 0;//��ͼ������
        float WindowWidth; //<! ���ڿ��
        float WindowHeight;//<! ���ڸ߶�

        float ToolbarHeight = 20f;//�������߶�



        void WelcomeView()
        {
            GUILayout.Space(WindowHeight / 5);
            GUILayout.Label("This is an script library.", skin_scriptLibrary.label, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
            GUILayout.Label(image_Script, skin_scriptLibrary.label, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("@Copyright by Shaw", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
            GUILayout.EndHorizontal();
        }


        string libraryViewName = "Welcome";
        bool isLockIntroduction = false;
        /// <summary>
        /// ��������������
        /// </summary>
        void DrawToolbar()
        {

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            //���ô�������Ϊ��ʼ��Ĭ������
            if (GUILayout.Button(libraryViewName, EditorStyles.toolbarButton, GUILayout.Width(100), GUILayout.Height(ToolbarHeight)))
            {
                viewIndex = (viewIndex == 0) ? 1 : 0;
                libraryViewName = (viewIndex == 0) ? "Welcome" : "Script";
            }
            //���ã��Ƿ�ʵʱˢ�������������
            if (GUILayout.Button("����", EditorStyles.toolbarButton, GUILayout.Width(50), GUILayout.Height(ToolbarHeight)))
            {
                menuWidthRatio = 0.15f;
                contentWidthRatio = 0.8f;
                QM.Log("�����ô�������");
            }
            //���水ť�����浱ǰ�������ݵ��޸ģ���������asset
            if (GUILayout.Button("����", EditorStyles.toolbarButton, GUILayout.Width(50), GUILayout.Height(ToolbarHeight)))
            {
                Debug.Log("����");
            }
            //ˢ�°�ť����DataAsset���޸�ʱ�����¶�ȡ���ػ洰��
            if (GUILayout.Button(icon_refresh, EditorStyles.toolbarButton, GUILayout.Width(50), GUILayout.Height(ToolbarHeight)))
            {
                RedoWindowContent();
                ShowNotification(new GUIContent("����������ˢ��"));
                ///Debug.Log("ˢ��");
            }

            GUILayout.FlexibleSpace();//������

            //�Ƿ���סIntroduction����
            isLockIntroduction = GUILayout.Toggle(isLockIntroduction, new GUIContent("Introduction", icon_lock), EditorStyles.toolbarButton, GUILayout.Width(100), GUILayout.Height(ToolbarHeight));

            //������ť����ʾ��β������豸��
            if (GUILayout.Button(icon_help, EditorStyles.toolbarButton, GUILayout.Width(24), GUILayout.Height(ToolbarHeight)))
            {
                //�򿪵���
                PopWindowHelp helpWindow = (PopWindowHelp)EditorWindow.GetWindow(
                       typeof(PopWindowHelp),
                       true,
                       "Help");
                var content = helpWindow.ReadHelpData(2);
                helpWindow.SetShowContents(content);
                helpWindow.ShowUtility();
                ///Debug.Log("����");
            }
            //�رհ�ť���رյ�ǰ����
            if (GUILayout.Button(icon_close, EditorStyles.toolbarButton, GUILayout.Width(24), GUILayout.Height(ToolbarHeight)))
            {
                if (EditorUtility.DisplayDialog("�رմ���", "�Ƿ�رոô��ڣ�", "yes", "no"))
                {
                    this.Close();
                    ///Debug.Log("�ر�");
                }

            }

            EditorGUILayout.EndHorizontal();

        }




        private Rect Rect_menu;
        Vector2 _scrollview1;
        private float menuItemWidth => AreaMenu.width - 10;
        private float menuItemHeight = 50f;
        /// <summary>
        /// �����������Ͳ˵���
        /// </summary>
        void DrawScriptMenu(float x, float y, float width, float height)
        {
            Rect_menu = new Rect(x, y, width, height);
            GUILayout.BeginArea(Rect_menu);
            _scrollview1 = GUILayout.BeginScrollView(_scrollview1);//<! ������

            GUILayout.Space(7);
            for (int i = 0; i < menuItem.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(7);
                string typeName = menuItem[i];
                // menuItemHeight = AreaMenu.height / menuItem.Count - 10;
                if (GUILayout.Button(typeName, GUILayout.Width(menuItemWidth), GUILayout.Height(menuItemHeight)))
                {
                    viewIndex = i + 1;
                }
                GUILayout.EndHorizontal();

            }



            GUILayout.EndScrollView();//<! ������
            GUILayout.EndArea();
        }



        private Rect Rect_content;
        Vector2 _scrollview2;
        protected int horizontalSize = 2;//ÿ�л��Ƶ��豸����
        /// <summary>
        /// ���ƽű�����������
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        void DrawScriptContent(float x, float y, float width, float height)
        {
            Rect_content = new Rect(x, y, width, height);
            GUILayout.BeginArea(Rect_content);
            _scrollview2 = GUILayout.BeginScrollView(_scrollview2);//<! ������

            if (viewIndex > 0)
            {
                ScriptLibraryContent(scriptLibraries, horizontalSize, viewIndex - 1);//�ű����ݻ���
            }

            GUILayout.EndScrollView();//<! ������
            GUILayout.EndArea();
        }


        private Rect Rect_introduction;
        Vector2 _scrollview3;
        private Script _currentScript;//��ǰ���ܵĽű�
        /// <summary>
        /// �ű���������
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        void DrawScriptIntroduction(float x, float y, float width, float height)
        {
            Rect_introduction = new Rect(x, y, width, height);
            GUILayout.BeginArea(Rect_introduction);
            _scrollview3 = GUILayout.BeginScrollView(_scrollview3);//<! ������

            ScriptIntroduction(_currentScript);


            GUILayout.EndScrollView();//<! ������
            GUILayout.EndArea();
        }



        private Rect Rect_resizer1;
        /// <summary>
        /// ��ȵ�����1
        /// </summary>
        void DrawResizer1(float x, float y, float width, float height)
        {
            Rect_resizer1 = new Rect(x, y, width, height);
            GUILayout.BeginArea(new Rect(Rect_resizer1.position + (Vector2.right * resizerWidth1), new Vector2(4, height * 3)), resizerStyle);
            GUILayout.EndArea();

            EditorGUIUtility.AddCursorRect(Rect_resizer1, MouseCursor.ResizeHorizontal);//�ı������״����ʾ���ָ��λ�ڵ�����Сλ��
        }


        private Rect Rect_resizer2;
        /// <summary>
        /// ��ȵ�����2
        /// </summary>
        void DrawResizer2(float x, float y, float width, float height)
        {
            Rect_resizer2 = new Rect(x, y, width, height);
            GUILayout.BeginArea(new Rect(Rect_resizer2.position + (Vector2.right * resizerWidth2), new Vector2(2, height * 3)), resizerStyle);
            GUILayout.EndArea();

            EditorGUIUtility.AddCursorRect(Rect_resizer2, MouseCursor.ResizeHorizontal);//�ı������״����ʾ���ָ��λ�ڵ�����Сλ��
        }



        private float MaxScriptModuleWidth = 500f;
        private float MInScriptModuleWidth = 300f;
        private float scriptModuleWidth = 350f;//һ���ű������ܿ��
        private float scriptModuleHeight = 36;//һ���ű������ܿ��
        private float scriptIconWidth = 32f;//�ű�ͼ����

        private List<string> RDTSNamespace = new List<string>() { "RDTS.", "RDTS.Utility." };

        void OneScriptContent(Script script)
        {

            if (script == null)
                return;

            EditorGUILayout.BeginHorizontal("Box", GUILayout.Width(AreaContent.width / 2 - 10), GUILayout.Height(scriptModuleHeight));
            GUILayout.Space(5);

            //�ű�ͼ��
            GUILayout.Label(script.icon, labelStyle_MiddleCenter, GUILayout.Width(scriptIconWidth), GUILayout.Height(scriptIconWidth));
            GUILayout.Space(10);
            //�ű�����
            GUILayout.Label(script.name, labelStyle_MiddleLeft, GUILayout.ExpandHeight(true));
            GUILayout.FlexibleSpace();
            //��ť-����
            if (GUILayout.Button(icon_eye, GUILayout.Width(40), GUILayout.Height(scriptModuleHeight)))
            {
                _currentScript = script;
            }
            //��ť-���
            if (GUILayout.Button(icon_add, GUILayout.Width(40), GUILayout.Height(scriptModuleHeight)))
            {
                try
                {
                    Type type;
                    for (int i = 0; i < RDTSNamespace.Count; i++)
                    {
                        string typeString = RDTSNamespace[i] + script.name;//��Ҫ���������ռ�
                        type = Type.GetType(typeString);//����ָ�����Ƶ����ͣ�����ҵ��Ļ���,����Ϊ null
                        //QM.Log(typeString);
                        if (type != null)
                        {
                            QM.Log("Add " + typeString);
                            QM.AddScript(type);
                            break;
                        }

                    }

                }
                catch
                {

                }

            }

            GUILayout.Space(5);
            EditorGUILayout.EndHorizontal();
        }



        void ScriptLibraryContent(List<ScriptLibrary> scriptLibraries, int horizontalSize = 2, int index = 0)
        {
            if (scriptLibraries == null || scriptLibraries.Count == 0)
                return;

            int rows = 0;//(����)����
            int columns = 0;//������������һ��ʱ�����У��еĸ���
            rows = scriptLibraries[index].scripts.Count / horizontalSize;
            columns = scriptLibraries[index].scripts.Count % horizontalSize;

            if (rows != 0)
            {
                for (int i = 1; i <= rows; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                    for (int j = (i - 1) * horizontalSize; j < i * horizontalSize; j++)
                    {
                        OneScriptContent(scriptLibraries[index].scripts[j]);
                    }

                    EditorGUILayout.EndHorizontal();
                }

            }

            if (columns != 0)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                for (int z = rows * horizontalSize; z < rows * horizontalSize + columns; z++)
                {
                    OneScriptContent(scriptLibraries[index].scripts[z]);
                }

                EditorGUILayout.EndHorizontal();
            }




        }



        void ScriptIntroduction(Script script)
        {
            if (script == null)
                return;

            GUILayout.Space(7);

            GUILayout.Label("Name��");//����
            GUILayout.TextArea(script.name, skin_scriptLibrary.textArea, GUILayout.Width(AreaIntroduction.width - 30));
            GUILayout.Space(5);
            GUILayout.Label("Function��");//
            GUILayout.TextArea(script.introduction.function, skin_scriptLibrary.textArea, GUILayout.Width(AreaIntroduction.width - 30));
            GUILayout.Space(5);
            GUILayout.Label("Apply��");//
            GUILayout.TextArea(script.introduction.apply, skin_scriptLibrary.textArea, GUILayout.Width(AreaIntroduction.width - 30));
            GUILayout.Space(5);
            GUILayout.Label("Notice��");//ע��
            GUILayout.TextArea(script.introduction.notice, skin_scriptLibrary.textArea, GUILayout.Width(AreaIntroduction.width - 30));

        }









        #endregion



        #region Methods

        private bool isResizing;//�Ƿ��ڵ�������ߴ�
        private bool isResizingWidth1;//�Ƿ��ڵ�����һ��������ߴ�
        private bool isResizingWidth2;//�Ƿ��ڵ����ڶ���������ߴ�
        /// <summary>
        /// ��갴�º�̧���¼�
        /// </summary>
        /// <param name="e"></param>
        private void ProcessEvents(Event e)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0)
                    {
                        isResizing = true;

                        isResizingWidth1 = (Rect_resizer1.Contains(e.mousePosition)) ? true : false;//�ж��Ƿ��ڵ����߶�

                        isResizingWidth2 = (Rect_resizer2.Contains(e.mousePosition)) ? true : false;//�ж��Ƿ��ڵ������

                        ///QM.Log("isResizingHeight��" + isResizingHeight + " " + "isResizingWidth: " + isResizingWidth);
                        ///QM.Log(Rect_widthResizer);
                        ///QM.Log(e.mousePosition);
                    }
                    else
                    {
                        isResizing = false;
                    }

                    break;
                case EventType.MouseDrag:
                    Resize(e);
                    break;

                case EventType.MouseUp:
                    isResizing = false;
                    break;
            }


        }

        /// <summary>
        /// ��������ߴ��С
        /// </summary>
        /// <param name="e"></param>
        private void Resize(Event e)
        {
            if (isResizing)
            {

                //������1
                if (isResizingWidth1 && e.mousePosition.x > WindowWidth * 0.1 && e.mousePosition.x < WindowWidth * 0.3)
                {
                    menuWidthRatio = e.mousePosition.x / WindowWidth;
                }
                //������2
                if (isResizingWidth2 && e.mousePosition.x > WindowWidth * 0.4 && e.mousePosition.x < WindowWidth * 0.9)
                {
                    contentWidthRatio = e.mousePosition.x / WindowWidth;
                }

                // QM.Log("widthSizeRatio��" + widthSizeRatio + " " + "heightSizeRatio: " + heightSizeRatio);

                Repaint();
            }
        }


        /// <summary>
        /// �Ƿ���ʾIntroduction����
        /// </summary>
        void ModeChange()
        {
            if (!isLockIntroduction)
                showMode = ShowMode.BriefMode;
            else
                showMode = ShowMode.DetailedMode;

            Repaint();
        }




        #endregion


        #region ��ȡ����Դ����

        List<ScriptLibrary> scriptLibraries = new List<ScriptLibrary>();//�ű�������
        List<string> menuItem = new List<string>();//��¼�˵���
        int number_ItemType = 0;//�������
        int number_Item = 0;//�ű�����

        List<ScriptLibrary> ReadLibraryData(ScriptLibraryData data)
        {
            List<ScriptLibrary> scriptLibraries = new List<ScriptLibrary>();
            scriptLibraries = data.ScriptLibraryDataList;//��ȡ����
            number_ItemType = scriptLibraries.Count;//��ȡ�ű����͸���

            menuItem.Clear();
            scriptLibraries.ForEach(sl =>
            {
                if (!menuItem.Contains(sl.name))
                    menuItem.Add(sl.name);//����ű�����

                number_Item += sl.scripts.Count;//һ�����ٽű�

            });


            return scriptLibraries;
        }


        /// <summary>
        /// ˢ�µ�ǰ���ڵ�����
        /// </summary>
        void RedoWindowContent()
        {
            scriptLibraryData = AssetDatabase.LoadAssetAtPath<ScriptLibraryData>(dataPath + "ScriptLibraryData.asset");
            scriptLibraries = ReadLibraryData(scriptLibraryData);

            Repaint();
        }



        #endregion

    }

#endif

}
