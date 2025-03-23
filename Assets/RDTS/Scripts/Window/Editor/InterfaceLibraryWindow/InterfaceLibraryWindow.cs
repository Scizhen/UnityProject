///********************************************************************************************
///this is part of Robot Digital Twin System.                                                 *
///@Copyright by Shaw.S , 2023                                                                *
///Do not distribute without authorization,thanks!                                            *
///********************************************************************************************
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using RDTS.Data;
using RDTS.Extension.EditorCoroutines;
using RDTS.Utility;


namespace RDTS.Window
{
#if UNITY_EDITOR

    /// <summary>
    /// ������Ʋ���
    /// </summary>
    public class AreaRectParam
    {
        public float x;
        public float y;
        public float width;
        public float height;
    }

    /// <summary>
    /// �ӿ�ģ���ࣺһ���м���ӿ����
    /// </summary>
    public class InterfaceModule
    {
        public string type;//�ӿ����ͣ���PLC��Modbus��
        public GameObject interfaceObject;//��asset�ļ��ж�Ӧ��Ԥ����gameobject
        public Interface.BaseInterface script;//��Ӧ�ӿ�����Ľű����ƣ�������
    }

    /// <summary>
    /// �ӿڿ������ʽ
    /// </summary>
    public enum InterfaceSortWay
    {
        Order,//������Hierarchy������Ⱥ��������
        Type//���սӿ���������
    }


    /// <summary>
    /// �ӿڶ����ࣺһ��������Ӧһ���ӿڶ���
    /// </summary>
    public class InterfaceObject
    {
        public int number;//���
        public string name;//��������
        public string type;//�ӿ�����
        public GameObject interfaceObject;//��Scene�ж�Ӧ�Ľӿ����gameobject
    }


    /// <summary>
    /// ֵ�����ࣺһ��������Ӧһ��ֵ����
    /// </summary>
    public class ValueObject
    {
        public int number;//���
        public string name;//��������
        public string datatype;//�������ͣ���InputBool��
        public string address;//��ַ
        public string value;//ֵ
        public GameObject valueObject;//��Scene�ж�Ӧ��ֵ����gameobject

        public InterfaceObject parent;//������
        public VALUETYPE type;//ֵ����
        public VALUEDIRECTION direction;//�����������
    }



    /// <summary>
    /// �Ľ��Ľӿڿ⣺��Ϊ�������򣨽ӿ���壬�ӿڶ����ֵ�������ʵ������ӡ������һ�������ơ��������С�ѡ�С�ѡ�����С�ɾ����ɾ�����С�ˢ�¡����Browse����������ʽ��
    /// �ӿڶ�����ֵ�����Ӽ���ϵ���༭�޸ġ�Excel����ȹ��ܡ�
    /// </summary>
    public class InterfaceLibraryWindow : EditorWindow
    {
        Scene currentScene;//��ǰ����
        Scene lastScene;//��һ������

        //�ӿڶ���
        List<InterfaceModule> List_InterfaceModules = new List<InterfaceModule>();//���湲�м���ӿ����
        List<InterfaceObject> List_interfaceObjs = new List<InterfaceObject>();//<! �洢һ���ж��ٽӿڶ���
        InterfaceObject currentInterfaceObject;//<! ��ǰҪ��ӵĽӿڶ���
        InterfaceObject lastInterfaceObject;//<! ��һ����ӵĽӿڶ���
        InterfaceObject currentSelectedInterfaceObj;//<! ��ǰѡ�еĽӿڶ��󣬼�ֵ����ĸ�����
        //�ӿڶ�������
        InterfaceSortWay SortWay_Interface = InterfaceSortWay.Order;//����ʽ��Ĭ�ϰ����Ⱥ�˳��

        string[] Menu_interfaceType = new string[6] { "PLC", "Modbus", "OPC UA", "RoboDK", "Redis", "MySQL" };//�ӿ����͵�����
        public string[] GetMenu_interfaceType { get { return Menu_interfaceType; } }//���ⲿ��ȡ�˱���

        //ֵ����
        List<ValueObject> List_valueObjects = new List<ValueObject>();//<! �洢һ���ж���ֵ����
        ValueObject currentValueObject;//<! ��ǰҪ��ӵ�ֵ����
        ValueObject lastValueObject;//<! ��һ����ӵ�ֵ����
        ValueObject editedValueObject;//<! Ҫ���༭��ֵ����
        ValueObject EditInformation;//<! ����ӵ�����ȡ�ı༭��Ϣ

        HashSet<ValueObject> HashSet_errorValueObjects = new HashSet<ValueObject>();//��¼�и�ʽ�����ֵ����ļ�
        bool isCheckError = false;//�Ƿ����д���
        //ֵ��������
        InterfaceSortWay SortWay_Value = InterfaceSortWay.Order;//����ʽ��Ĭ�ϰ����Ⱥ�˳��
        //ֵ�������������
        string[] Menu_valueType = new string[9] { 
            "InputBool", "InputFloat", "InputInt", 
            "OutputBool", "OutputFloat", "OutputInt", 
            "MiddleBool", "MiddleFloat", "MiddleInt" };
        public string[] GetMenu_valueType { get { return Menu_valueType; } }//���ⲿ��ȡ�˱���

        //����Excel
        string _ExcelFilePath = "";//Ҫ�����Excel�ļ�·��
        public string ExcelFilePath { 
            get { return _ExcelFilePath;  } 
            set { _ExcelFilePath = value; }
        }///���ⲿ��ȡ�ļ�·���Ľӿ�
        System.Data.DataSet _ExcelFileResult;//��ȡ����Excel�ļ��ṹ
        public System.Data.DataSet ExcelFileResult { 
            get { return _ExcelFileResult;  } 
            set { _ExcelFileResult = value; }
        }///���ⲿ��ȡ��ȡ�ļ��Ľӿ�
        System.Data.DataTableCollection _ExcelFileTables;//��ȡ����Excel�ı�
        public System.Data.DataTableCollection ExcelFileTables { 
            get { return _ExcelFileTables;  } 
            set { _ExcelFileTables = value; }
        }///���ⲿ��ȡ���Ľӿ�

        //��־λ
        bool isAdding_Interface = false;//<! �Ƿ�ǰ��Ҫ��ӽӿڶ���
        bool isRemoveAllObjs_Interface = false;//<! �Ƿ�ǰ��Ҫɾ�����нӿڶ���
        bool isCopyAllObjs_Interface = false;//<! �Ƿ�ǰ��Ҫ�������нӿڶ���

        bool isAdding_Value = false;//<! �Ƿ�ǰ��Ҫ���ֵ����
        bool isRemoveAllObjs_Value = false;//<! �Ƿ�ǰ��Ҫɾ������ֵ����
        bool isCopyAllObjs_Value = false;//<! �Ƿ�ǰ��Ҫ��������ֵ����
        bool isEditing_Value = false;//<! �Ƿ�ǰ�ڱ༭ֵ����


        [MenuItem("Parallel-RDTS/Window/InterfaceLibraryWindow", false, 200)]
        private static void ShowWindow()
        {
            InterfaceLibraryWindow window = (InterfaceLibraryWindow)EditorWindow.GetWindow(typeof(InterfaceLibraryWindow));
            window.titleContent = InterfaceLibraryWindow.thisTitleContent;//���ñ����ͼ��
            window.minSize = new Vector2(300, 200);
            window.Show();
        }


        float WindowWidth; //<! ���ڿ��
        float WindowHeight;//<! ���ڸ߶�
        float LeftAreaWidth;//<1 ���������
        float RightAreaWidth;//<! �Ҳ�������
        float LeftAreaHeight;//<1 �������߶�
        float RightAreaHeight;//<! �Ҳ�����߶�
        private void OnGUI()
        {
            if (HasOpenInstances<InterfaceLibraryWindow>())//�豸�ⴰ���Ƿ��
            {
                WindowWidth = position.width;//��ȡ���ڿ��
                WindowHeight = position.height;//��ȡ���ڸ߶�
                ////��������ĸ����
                //LeftAreaWidth = WindowWidth * widthSizeRatio;
                //RightAreaWidth = WindowWidth - LeftAreaWidth;
                //LeftAreaHeight = RightAreaHeight = WindowHeight;

                //WindowOperation();//<! ע��Ҫ��ǰ�棬��Ȼ�ᱨlayout/repaint�Ĵ�������layout/repaint�ĳ�ͻ��
               
                MenuList();//�����˵���

                //GUILayout.BeginHorizontal();//*
                /////�������
                //GUILayout.BeginArea(new Rect(0, 0, LeftAreaWidth, LeftAreaHeight));//*
                //DrawInterfacePanel(InterfacePanel.x, InterfacePanel.y, InterfacePanel.width, InterfacePanel.height);//�ӿ�������
                //DrawInterfaceObjectTable(InterfaceObjectTable.x, InterfaceObjectTable.y, InterfaceObjectTable.width, InterfaceObjectTable.height);//�ӿڶ����б����
                //DrawHeightResizer(HeightResizer.x, HeightResizer.y, HeightResizer.width, HeightResizer.height);//�߶ȷ���ĵ�����
                //GUILayout.EndArea();

                /////�ұ�����
                //GUILayout.BeginArea(new Rect(WindowWidth * widthSizeRatio, 0, RightAreaWidth, RightAreaHeight));//*
                //DrawValueObjectTable(ValueObjectTable.x, ValueObjectTable.y, ValueObjectTable.width, ValueObjectTable.height);//ֵ�����б����
                //DrawWidthResizer(WidthResizer.x, WidthResizer.y, WidthResizer.width, WidthResizer.height);//��ȷ���ĵ�����
                //GUILayout.EndArea();
                //GUILayout.EndHorizontal();//*

               
                //ProcessEvents(Event.current);//���������Ӧ�¼�
                //SetAreaRectParam();//���������͸�
                
                switch(viewIndex)
                {
                    case 0:
                        WelcomeView();//��ӭ����
                        break;
                    case 1:
                        InterfaceView();//�ӿڿ����ݽ���
                        break;
                }



                if (GUI.changed) Repaint();
               
            }


        }


        void OnFocus()
        {
            ///QM.Log("InterfaceLibraryWindow �� OnFocus");
            //if(!isCoroutineStart)
            //    StartEditorCoroutine("BrowseObjectsInCurrentScene");//����Э��
        }

        void OnLostFocus()
        {
            ///QM.Log("InterfaceLibraryWindow �� OnLostFocus");
            //if (isCoroutineStart)
            //    StopEditorCoroutine("BrowseObjectsInCurrentScene");//�ر�Э��

        }

        private void OnHierarchyChange()
        {
          
        }


        private void Update()
        {

        }


        /// <summary>
        /// ���ڴ˴��ڵĲ���
        /// </summary>
        void WindowOperation()
        {
            //���һ��ֵ����
            if(isAdding_Value && currentValueObject != null)
            {
                AddCurrentValueObject();
                SetCurrentValueObject(null);
                isAdding_Value = false;
            }
            //�������е�ǰ�б��е�ֵ����*ֱ��ͨ������gameobject�ķ�ʽ��������ͨ������currentInterfaceObject��
            if (isCopyAllObjs_Value)
            {
                if (CopyAllObjectsInValueObjectList(List_valueObjects))//��ȫ���������
                    isCopyAllObjs_Value = false;
            }
            //ɾ������ֵ����
            if (isRemoveAllObjs_Value)
            {
                RemoveAllObjectsInValueObjectList(List_valueObjects);
                if (List_valueObjects.Count == 0)//<! *��������жϣ���Ϊ����gameobject��Ҫһ��ʱ�䣬����Ҫ���б�Ԫ��ȫ�����������λ
                    isRemoveAllObjs_Value = false;
            }
            //�༭ֵ����
            if(isEditing_Value && editedValueObject != null && EditInformation != null)
            {
                if(EditValueObject(editedValueObject, EditInformation))
                {
                    SetEditedValueObject(null);
                    SetEditInformation(null);
                    isEditing_Value = false;
                }
               
            }

            //���һ���ӿڶ���
            if (isAdding_Interface && currentInterfaceObject!=null)
            {
                AddCurrentInterfaceObject();
                SetCurrentInterfaceObject(null);
                isAdding_Interface = false;///ֻ�ڴ˴���Ϊfalse
            }
            //�������е�ǰ�б��еĽӿڶ���*ֱ��ͨ������gameobject�ķ�ʽ��������ͨ������currentInterfaceObject��
            if(isCopyAllObjs_Interface)
            {
                if (CopyAllObjectsInInterfaceObjectList(List_interfaceObjs))//��ȫ���������
                    isCopyAllObjs_Interface = false;
            }
            //ɾ�����нӿڶ���
            if (isRemoveAllObjs_Interface)
            {
                RemoveAllObjectsInInterfaceObjectList(List_interfaceObjs);
                if (List_interfaceObjs.Count == 0)//<! *��������жϣ���Ϊ����gameobject��Ҫһ��ʱ�䣬����Ҫ���б�Ԫ��ȫ�����������λ
                    isRemoveAllObjs_Interface = false;
            }

            //��ע�⣺Э�̲���ԭ�򣬵�����û�ر�ʱһֱ��������״̬�����ļ�����Դ������
            BrowseInterfaceGameObjectsInCurrentScene();//<! ʵʱˢ�����Scene�еĽӿ������ע�⣺���Ǹ������Ҵ���δ����״̬�µ�gameobject���ᱻ�ҵ��������ɴ˱����ĳһ�ӿڶ�����в�����
            BrowseValueGameObjectsInCurrentScene();//<! ʵʱˢ�����Scene�е�ֵ����ע�⣺���Ǹ������Ҵ���δ����״̬�µ�gameobject���ᱻ�ҵ��������ɴ˱����ĳһֵ������в�����
            ValueObjectPropertyCheck(List_valueObjects);//<! ֵ�����ĸ�ʽ��� ע�⣺��ַ��⡢��ַ�����δ���룬��Ϊ��ͬ�ӿ�Ҫ��ĵ�ַ��ʽ��ͬ����������Ҫ���Լ��룬�й�������PLC��ַ��ʽ�ɲο�InterfaceLibraryWindow.SignalPropertyCheck()����
        }




        #region Init

        Coroutine c;//Э��
        bool isCoroutineStart = false;//Э���Ƿ���

        string dataPath = "Assets/RDTS/Data/";
        string iconPath = "Assets/RDTS/Private/Resources/Icons/WindowIcon/";
        string backgroundPath = "Assets/RDTS/Private/Resources/Texture/";
        string skinPath = "Assets/RDTS/Scripts/Window/";

        InterfaceLibraryData interfaceLibraryData;//�ӿڿ�����

        static GUIContent thisTitleContent;//���ڱ���
        void OnEnable()
        {
            //������Դ��ȡ
            interfaceLibraryData = AssetDatabase.LoadAssetAtPath<InterfaceLibraryData>(RDTSPath.path_Data + "InterfaceLibraryData.asset");
            List_InterfaceModules = ReadLibraryData(interfaceLibraryData);
            Menu_interfaceType = GetMenuOfInterfaceType(List_InterfaceModules);
            //���ô��ڱ��⼰ͼ��
            Texture titleIcon = AssetDatabase.LoadAssetAtPath<Texture>(iconPath + "Parallel.png");
            thisTitleContent = new GUIContent("Parallel�ӿڿ�", titleIcon);
            //GUI���
            rowStyle = new GUIStyle();
            rowStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
            //����unity�Դ�ͼ��
            LoadTexture();
            //����Rect��ʼ����
            SetAreaRectParam();
            //��ʼ����ֵ
            number_selectedRow_interfaceObj = 0;
            isAdding_Interface = false;
            isAdding_Value = false;
            isRemoveAllObjs_Interface = false;
            isRemoveAllObjs_Value = false;
            isCopyAllObjs_Interface = false;
            isCopyAllObjs_Value = false;
            isEditing_Value = false;
            isCheckError = false;
            //Э�̡������á�
            //StartEditorCoroutine("BrowseObjectsInCurrentScene");//����Э��
        }

        void OnDisable()
        {
            //if(isCoroutineStart)
            //    StopEditorCoroutine("BrowseObjectsInCurrentScene");//�ر�Э��

        }


        private GUISkin skin_interfaceLibrary;
        private GUIStyle resizerStyle;
        private GUIStyle labelStyle_MiddleCenter;//���ı�ǩUI��ʽ
        private GUIStyle labelStyle_MiddleCenter_Color;//��ɫ���ı�ǩ��ʽ
        private GUIStyle labelStyle_MiddleCenter_Red;//��ɫ���ı�ǩ��ʽ
        private GUIStyle labelStyle_MiddleCenter_Green;//��ɫ���ı�ǩ��ʽ
        private GUIStyle labelStyle_MiddleCenter_Orange;//��ɫ���ı�ǩ��ʽ
        private GUIStyle labelStyle_Italic;//�Զ��屳��ͼƬ�ı�ǩ��ʽ
        private GUIStyle rowStyle;//�б�һ�е���ʽ

        private Texture2D icon_refresh;
        private Texture2D icon_center;
        private Texture2D icon_close;
        private Texture2D icon_add;
        private Texture2D icon_delete;
        private Texture2D icon_help;
        private Texture2D icon_sort;
        private Texture2D icon_eye;
        private Texture2D icon_pick;
        private Texture2D icon_copy;
        private Texture2D icon_edit;
        private Texture2D icon_excel;

        private Texture2D image_rowBgOdd;//����
        private Texture2D image_rowBgEven;//ż��
        private Texture2D image_rowSelected;//��ѡ��
        private Texture2D image_error;//������

        private Texture image_Dropdown;
        private Texture image_Interface;

        void LoadTexture()//�����ز�
        {
            //GUIƤ��
            skin_interfaceLibrary = AssetDatabase.LoadAssetAtPath<GUISkin>("Assets/RDTS/Scripts/Window/Editor/InterfaceLibraryWindow/WindowInterfaceLibrary.guiskin");
            labelStyle_MiddleCenter = skin_interfaceLibrary.customStyles[7];
            labelStyle_MiddleCenter_Color = skin_interfaceLibrary.customStyles[8];
            labelStyle_Italic = skin_interfaceLibrary.customStyles[9];
            labelStyle_MiddleCenter_Red = skin_interfaceLibrary.customStyles[10];
            labelStyle_MiddleCenter_Green = skin_interfaceLibrary.customStyles[11];
            labelStyle_MiddleCenter_Orange = skin_interfaceLibrary.customStyles[12];

            //Style
            resizerStyle = new GUIStyle();
            resizerStyle.normal.background = EditorGUIUtility.Load("icons/d_AvatarBlendBackground.png") as Texture2D;//Ĭ�ϵ�Unity�༭������(����ͼ��)

            //ͼ��
            icon_refresh = EditorGUIUtility.FindTexture("Refresh");
            icon_center = EditorGUIUtility.FindTexture("d_scenevis_visible_hover@2x");
            icon_add = EditorGUIUtility.FindTexture("d_CreateAddNew");
            icon_close = icon_delete = EditorGUIUtility.FindTexture("d_winbtn_win_close");
            icon_help = EditorGUIUtility.FindTexture("d__Help@2x");
            icon_sort = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath + "sort.png");
            icon_eye = EditorGUIUtility.FindTexture("d_scenevis_visible_hover@2x");
            icon_pick = EditorGUIUtility.FindTexture("d_scenepicking_pickable-mixed");
            icon_copy = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath + "copy.png");
            icon_excel = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath + "excel.png");
            icon_edit = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath + "edit.png");


            //����ͼ��
            image_Dropdown = AssetDatabase.LoadAssetAtPath<Texture>(iconPath + "dropdown.png");
            image_Interface = AssetDatabase.LoadAssetAtPath<Texture>(iconPath + "WindowInterfaceIcon.png");

            //��ż����ɫ�����ͼƬ
            image_rowBgOdd = EditorGUIUtility.Load("builtin skins/darkskin/images/cn entrybackodd.png") as Texture2D;
            image_rowBgEven = EditorGUIUtility.Load("builtin skins/darkskin/images/cnentrybackeven.png") as Texture2D;
            image_rowSelected = EditorGUIUtility.Load("builtin skins/darkskin/images/menuitemhover.png") as Texture2D;
            image_error = AssetDatabase.LoadAssetAtPath<Texture2D>(backgroundPath + "LightRedBackground.png");
        }


        #endregion




        #region Toolbar

        float menuHeight = 20f;
        string libraryViewName = "Welcome";

        /// <summary>���ڲ˵�������</summary>
        void MenuList()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            //���ô�������Ϊ��ʼ��Ĭ������
            if (GUILayout.Button(libraryViewName, EditorStyles.toolbarButton, GUILayout.Width(100), GUILayout.Height(menuHeight)))
            {
                viewIndex = (viewIndex == 0) ? 1 : 0;
                libraryViewName = (viewIndex == 0) ? "Welcome" : "Interface";
            }
            //���ã��Ƿ�ʵʱˢ�������������
            if (GUILayout.Button("����", EditorStyles.toolbarButton, GUILayout.Width(50), GUILayout.Height(menuHeight)))
            {
                QM.Log("���ô�������");
            }
            //���水ť�����浱ǰ�������ݵ��޸ģ���������asset
            if (GUILayout.Button("����", EditorStyles.toolbarButton, GUILayout.Width(50), GUILayout.Height(menuHeight)))
            {
                Debug.Log("����");
            }
            //ˢ�°�ť����DataAsset���޸�ʱ�����¶�ȡ���ػ洰��
            if (GUILayout.Button(icon_refresh, EditorStyles.toolbarButton, GUILayout.Width(50), GUILayout.Height(menuHeight)))
            {
                RefreshWindowContent();///1.ˢ�µ�ǰ����  2.���ô�����ز���
                ShowNotification(new GUIContent("����������ˢ��"));
                ///Debug.Log("ˢ��");
            }

            GUILayout.FlexibleSpace();//������

            //������ť����ʾ��β������豸��
            if (GUILayout.Button(icon_help, EditorStyles.toolbarButton, GUILayout.Width(24), GUILayout.Height(menuHeight)))
            {
                //�򿪵���
                PopWindowHelp helpWindow = (PopWindowHelp)EditorWindow.GetWindow(
                       typeof(PopWindowHelp),
                       true,
                       "Help");
                var content = helpWindow.ReadHelpData(1);
                helpWindow.SetShowContents(content);
                helpWindow.ShowUtility();
                ///Debug.Log("����");
            }
            //�رհ�ť���رյ�ǰ����
            if (GUILayout.Button(icon_close, EditorStyles.toolbarButton, GUILayout.Width(24), GUILayout.Height(menuHeight)))
            {
                if (EditorUtility.DisplayDialog("�رմ���", "�Ƿ�رոô��ڣ�", "yes", "no"))
                {
                    this.Close();
                    ///Debug.Log("�ر�");
                }

            }

            EditorGUILayout.EndHorizontal();


        }

        #endregion




        #region ViewContent

        int viewIndex = 0;//��������������

        void WelcomeView()
        {
            GUILayout.Space(WindowHeight/5);
            GUILayout.Label("This is an interface library.", skin_interfaceLibrary.label, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
            GUILayout.Label(image_Interface, skin_interfaceLibrary.label, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("@Copyright by Shaw", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
            GUILayout.EndHorizontal();
        }


        void InterfaceView()
        {
            //����
            currentScene = SceneManager.GetActiveScene();
            if (currentScene != lastScene)
                RefreshWindowContent();
            lastScene = currentScene;

            //��������ĸ����
            LeftAreaWidth = WindowWidth * widthSizeRatio;
            RightAreaWidth = WindowWidth - LeftAreaWidth;
            LeftAreaHeight = RightAreaHeight = WindowHeight;

            WindowOperation();//<! ע��Ҫ��ǰ�棬��Ȼ�ᱨlayout/repaint�Ĵ�������layout/repaint�ĳ�ͻ��

            GUILayout.BeginHorizontal();//*
            ///�������
            GUILayout.BeginArea(new Rect(0, 0, LeftAreaWidth, LeftAreaHeight));//*
            DrawInterfacePanel(InterfacePanel.x, InterfacePanel.y, InterfacePanel.width, InterfacePanel.height);//�ӿ�������
            DrawInterfaceObjectTable(InterfaceObjectTable.x, InterfaceObjectTable.y, InterfaceObjectTable.width, InterfaceObjectTable.height);//�ӿڶ����б����
            DrawHeightResizer(HeightResizer.x, HeightResizer.y, HeightResizer.width, HeightResizer.height);//�߶ȷ���ĵ�����
            GUILayout.EndArea();

            ///�ұ�����
            GUILayout.BeginArea(new Rect(WindowWidth * widthSizeRatio, 0, RightAreaWidth, RightAreaHeight));//*
            DrawValueObjectTable(ValueObjectTable.x, ValueObjectTable.y, ValueObjectTable.width, ValueObjectTable.height);//ֵ�����б����
            DrawWidthResizer(WidthResizer.x, WidthResizer.y, WidthResizer.width, WidthResizer.height);//��ȷ���ĵ�����
            GUILayout.EndArea();
            GUILayout.EndHorizontal();//*


            ProcessEvents(Event.current);//���������Ӧ�¼�
            SetAreaRectParam();//���������͸�
        }


        

        //��������
        private AreaRectParam InterfacePanel = new AreaRectParam();
        private AreaRectParam InterfaceObjectTable = new AreaRectParam();
        private AreaRectParam ValueObjectTable = new AreaRectParam();
        //����������
        private AreaRectParam HeightResizer = new AreaRectParam();
        private AreaRectParam WidthResizer = new AreaRectParam();

        private float heightSizeRatio = 0.35f;//�߶ȳߴ�ȱ���
        private float widthSizeRatio = 0.35f;//��ȳߴ�ȱ���
        private float resizerHeight = 10f;//��������ĸ߶�
        private float resizerWidth = 10f;//��������Ŀ��
        private bool isResizing;//�Ƿ��ڵ�������ߴ�
        private bool isResizingHeight;//�Ƿ��ڵ����߶ȷ��������ߴ�
        private bool isResizingWidth;//�Ƿ��ڵ�����ȷ��������ߴ�

        private bool isSelected_row;//�Ƿ�ĳһ�б�ѡ��
        private int number_selectedRow_interfaceObj;//�ӿڶ����б�ѡ����һ�е����
        private int number_selectedRow_valueObj;//ֵ�����б�ѡ����һ�е����

        private Rect Rect_interfacePanel;
        Vector2 _scrollview1;
        /// <summary>
        ///�ӿ������ͼ����
        /// </summary>
        void DrawInterfacePanel(float x, float y, float width, float height)
        {
            Rect_interfacePanel = new Rect(x, y, width, height);
            GUILayout.BeginArea(Rect_interfacePanel);
            _scrollview1 = GUILayout.BeginScrollView(_scrollview1);//<! ������

            GUILayout.Space(5);
            //��ʾ��ǰ������Ϣ
            GUILayout.Box($"Scene��{currentScene.name}", GUILayout.ExpandWidth(true));
            //�ӿ���壺ѡ��ӿ�����
            InterfacePanelContent();

            GUILayout.EndScrollView();//<! ������
            GUILayout.EndArea();

        }


        private Rect Rect_interfaceObjectTable;
        Vector2 _scrollview2;
        /// <summary>
        /// �ӿڶ����б�Ļ���
        /// </summary>
        void DrawInterfaceObjectTable(float x, float y, float width, float height)
        {
            Rect_interfaceObjectTable = new Rect(x, y, width, height);
            GUILayout.BeginArea(Rect_interfaceObjectTable);
            ////_scrollview2 = GUILayout.BeginScrollView(_scrollview2);//<! ������

            GUILayout.Space(2);
            //�ӿڶ����б������Ĺ������������
            InterfaceObjectTableContent();

            ////GUILayout.EndScrollView();//<! ������
            GUILayout.Label($"Number of interface objects��{List_interfaceObjs.Count}", labelStyle_Italic, GUILayout.ExpandWidth(true));//�ӿڶ�������
            GUILayout.Space(2);
            GUILayout.EndArea();
        }


        private Rect Rect_valueObjectTable;
        Vector2 _scrollview3;
        /// <summary>
        /// ֵ�����б�Ļ��ƣ��źš���ֵ�ȣ�
        /// </summary>
        void DrawValueObjectTable(float x, float y, float width, float height)
        {
            Rect_valueObjectTable = new Rect(x, y, width, height);
            GUILayout.BeginArea(Rect_valueObjectTable);
            //// _scrollview3 = GUILayout.BeginScrollView(_scrollview3);//<! ������

            //GUILayout.Label("ValueObjectTable");
            GUILayout.Space(5);
            ValueObjectTableContent();

            //// GUILayout.EndScrollView();//<! ������
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Number of Value Objects��{List_valueObjects.Count}", labelStyle_Italic, GUILayout.ExpandWidth(true));//ֵ��������
            GUILayout.EndHorizontal();
            GUILayout.Space(2);

            GUILayout.EndArea();
        }


        private Rect Rect_heightResizer;
        /// <summary>
        /// �߶ȷ���ĳߴ������
        /// </summary>
        void DrawHeightResizer(float x, float y, float width, float height)
        {
            Rect_heightResizer = new Rect(x, y, width, height);
            GUILayout.BeginArea(new Rect(Rect_heightResizer.position + (Vector2.up * resizerHeight), new Vector2(width * 2, 4)), resizerStyle);
            GUILayout.EndArea();

            EditorGUIUtility.AddCursorRect(Rect_heightResizer, MouseCursor.ResizeVertical);//�ı������״����ʾ���ָ��λ�ڵ�����Сλ��
        }


        private Rect Rect_widthResizer;
        /// <summary>
        /// �߶ȷ���ĳߴ������
        /// </summary>
        void DrawWidthResizer(float x, float y, float width, float height)
        {
            Rect_widthResizer = new Rect(x, y, width, height);
            GUILayout.BeginArea(new Rect(Rect_widthResizer.position + (Vector2.right * resizerWidth), new Vector2(4, height * 3)), resizerStyle);//�߶�*3����Ϊ������ʾʱ���ֵ�����������ɫ
            GUILayout.EndArea();

            EditorGUIUtility.AddCursorRect(Rect_widthResizer, MouseCursor.ResizeHorizontal);//�ı������״����ʾ���ָ��λ�ڵ�����Сλ��
        }





        /* UIԪ�ػ������� */


        float Height_operation = 22f;
        /// <summary>
        /// �ӿ��������
        /// </summary>
        void InterfacePanelContent()
        {
            GUILayout.Label("Interface Operation");
            
            //���һ���ӿڶ��󣬵�����ʽ
            if (GUILayout.Button(new GUIContent("  Add", icon_add), GUILayout.Height(Height_operation)))
            {
                isAdding_Interface = true;//��λ��־λ��������Ӷ���
                //�򿪵���
                PopWindowInterfaceObjectAdd addWindow = (PopWindowInterfaceObjectAdd)EditorWindow.GetWindowWithRect(
                       typeof(PopWindowInterfaceObjectAdd),
                       new Rect(Screen.width / 2, Screen.height / 2, 400, 150),
                       false,
                       "Add interface object");
                addWindow.LinkInterfaceLibraryWindow(this);
                addWindow.ShowUtility();
                
            }
            //�������һ����ͬ�Ľӿڶ��󣬼򻯲���
            if (GUILayout.Button(new GUIContent("  Add previous", icon_add), GUILayout.Height(Height_operation)))
            {
                if (lastInterfaceObject != null)
                {
                    SetCurrentInterfaceObject(lastInterfaceObject);
                    isAdding_Interface = true;
                }
                  
            }
            //ɾ����ǰ���������еĽӿ����������
            if (GUILayout.Button(new GUIContent("  Delete all", icon_delete), GUILayout.Height(Height_operation)))
            {
                if (EditorUtility.DisplayDialog("ɾ������", "�Ƿ�ɾ�����нӿڶ���", "yes", "no"))
                {
                    currentSelectedInterfaceObj = null;
                    isRemoveAllObjs_Interface = true;
                }
            }

            GUILayout.BeginHorizontal();
            GUILayout.EndHorizontal();
        }



        string[] TableOperation_interfaceObject = new string[4] { "Refresh", "Select all", "Copy all", "Sort" };
        string[] TableTitle_interfaceObject = new string[4] { "Number", "Name", "Type", "Operate" };//"���", "����", "����", "����"
        /// <summary>
        /// �ӿڶ����б�����
        /// </summary>
        void InterfaceObjectTableContent()
        {
            
            /* 1.���ƹ����� */
            GUILayout.BeginHorizontal();
            int count_operation = TableOperation_interfaceObject.Length;
            //ˢ���������
            if (GUILayout.Button(new GUIContent("  " + TableOperation_interfaceObject[0], icon_refresh), GUILayout.Width(LeftAreaWidth / count_operation), GUILayout.Height(Height_operation)))
            {
                SortWay_Interface = InterfaceSortWay.Order;//��������ʽ
                number_selectedRow_interfaceObj = 0;//����ѡ���еı�����ʽ
                currentSelectedInterfaceObj = null;//������ǰҪ�鿴�Ľӿڶ���
                SelectObjectIsNull();
            }
            //��Scene��ѡ�У��������б������еĽӿڶ���
            if (GUILayout.Button(new GUIContent("  " + TableOperation_interfaceObject[1], icon_pick), GUILayout.Width(LeftAreaWidth / count_operation), GUILayout.Height(Height_operation)))
            {
                SelectInterfaceObjectInScene(List_interfaceObjs);
            }
            //���Ƶ�ǰ�������еĽӿڶ���
            if (GUILayout.Button(new GUIContent("  " + TableOperation_interfaceObject[2], icon_copy), GUILayout.Width(LeftAreaWidth / count_operation), GUILayout.Height(Height_operation)))
            {
                if (EditorUtility.DisplayDialog("��������", "�Ƿ������нӿڶ���", "yes", "no"))
                {
                    isCopyAllObjs_Interface = true;
                }

            }
            GUILayout.FlexibleSpace();
            //ѡ����нӿڶ�������з�ʽ
            if (GUILayout.Button(icon_sort, GUILayout.Width(Height_operation * 1.5f), GUILayout.Height(Height_operation)))
            {
                //���ư�ť�������˵�
                GenericMenu sortwayMenu = new GenericMenu();
                sortwayMenu.AddItem(new GUIContent("�Ⱥ�˳��"), SortWay_Interface.Equals(InterfaceSortWay.Order), OnValueSelectedOfSortwayCallback, InterfaceSortWay.Order);
                sortwayMenu.AddItem(new GUIContent("�ӿ�����"), SortWay_Interface.Equals(InterfaceSortWay.Type), OnValueSelectedOfSortwayCallback, InterfaceSortWay.Type);

                sortwayMenu.ShowAsContext();
            }
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            /* 2.���Ʊ��� */
            GUILayout.BeginHorizontal();
            GUILayout.Label(TableTitle_interfaceObject[0], EditorStyles.toolbarButton, GUILayout.Width(LeftAreaWidth / 7));
            GUILayout.Label(TableTitle_interfaceObject[1], EditorStyles.toolbarButton, GUILayout.Width(LeftAreaWidth * 2 / 7));
            GUILayout.Label(TableTitle_interfaceObject[2], EditorStyles.toolbarButton, GUILayout.Width(LeftAreaWidth * 2 / 7));
            GUILayout.Label(TableTitle_interfaceObject[3], EditorStyles.toolbarButton, GUILayout.Width(LeftAreaWidth * 2 / 7 - 5));
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            _scrollview2 = GUILayout.BeginScrollView(_scrollview2);//<! ������
            /* 3.���ƽӿڶ����б� */
            ////List_interfaceObjs.ForEach(obj => OneInterfaceObjFormat(obj));//��*��������䡿��Ϊfoeeach�в�Ҫʹ��remove����add����
            for (int i = 0; i < List_interfaceObjs.Count; i++)
                OneInterfaceObjFormat(List_interfaceObjs[i]);
            GUILayout.EndScrollView();//<! ������
        }


        /// <summary>
        /// һ�����У��ӿڶ���ĸ�ʽ
        /// </summary>
        /// <param name="interfaceObj"></param>
        void OneInterfaceObjFormat(InterfaceObject interfaceObj)
        {
            if (interfaceObj == null)
                return;

            int rowNumber = interfaceObj.number;//��ȡ�˽ӿڶ�������
            //<! ������ż�б�����ɫ
            if (number_selectedRow_interfaceObj != rowNumber)
                rowStyle.normal.background = (rowNumber % 2 == 0) ? image_rowBgEven : image_rowBgOdd;
            else//<! ���˽ӿڶ���ѡ�У����޸ı�����ɫ
                rowStyle.normal.background = image_rowSelected;

            GUILayout.Space(5);
            GUILayout.BeginHorizontal(rowStyle);

            GUIStyle style_button = new GUIStyle(EditorStyles.miniButtonMid);//��ť��ʽ
            //<! ���
            GUILayout.Label(rowNumber.ToString(), labelStyle_MiddleCenter, GUILayout.Width(LeftAreaWidth / 7 - 5));
            //<! ����
            GUILayout.Label(interfaceObj.name, labelStyle_MiddleCenter_Color, GUILayout.Width(LeftAreaWidth * 2 / 7 - 5));
            //<! ����
            GUILayout.Label(interfaceObj.type, labelStyle_MiddleCenter, GUILayout.Width(LeftAreaWidth * 2 / 7 - 5));
            //<! ����
            float width = LeftAreaWidth * 2 / 7;
            //�鿴�˽ӿڶ����°����ġ�ֵ����
            if (GUILayout.Button(icon_eye, style_button, GUILayout.Width(width / 4 - 2)))
            {
                number_selectedRow_interfaceObj = rowNumber;
                currentSelectedInterfaceObj = interfaceObj;//��ǰҪ�鿴�Ľӿڶ���
            }
            //Pingһ����Hierarchy����е�λ�ã���Scene��ѡ�д˽ӿڶ���
            if (GUILayout.Button(icon_pick, style_button, GUILayout.Width(width / 4 - 2)))
            {
                SelectInterfaceObjectInScene(interfaceObj);
                number_selectedRow_interfaceObj = rowNumber;//���ô��б�ѡ��
            }
            //���ƴ˽ӿڶ���
            if (GUILayout.Button("+", style_button, GUILayout.Width(width / 4 - 2)))
            {
                CopyOneInterfaceObject(interfaceObj);
            }
            //ɾ���˽ӿڶ���
            if (GUILayout.Button("-", style_button, GUILayout.Width(width / 4 - 2)))
            {
                //if (currentSelectedInterfaceObj == interfaceObj)
                //    currentSelectedInterfaceObj = null;
                currentSelectedInterfaceObj = null;
                ///ɾ���б�Ԫ�ء���Ӧ��gameobject
                RemoveOneObjectInInterfaceObjectList(interfaceObj, List_interfaceObjs);
               
            }

            GUILayout.EndHorizontal();


        }




        string[] TableOperation_valueObject = new string[8] { "Refresh", "Add", "Add previous", "Import Excel", "Select all", "Copy all", "Delete all",  "Sort" };
        string[] TableTitle_valueObject = new string[6] { "Number", "Name", "DataType", "Address", "Value", "Operate" };//"���", "����", "��������", "��ַ", "ֵ", "����"
        /// <summary>
        /// ֵ�����б�����
        /// </summary>
        void ValueObjectTableContent()
        {
            /* 1.���ƹ����� */
            GUILayout.BeginHorizontal();
            int count_operation1 = 4 + 1;
            //ˢ���������
            if (GUILayout.Button(new GUIContent("  " + TableOperation_valueObject[0], icon_refresh), GUILayout.Width(RightAreaWidth / count_operation1), GUILayout.Height(Height_operation)))
            {
                SortWay_Value = InterfaceSortWay.Order;//��������ʽ
                number_selectedRow_valueObj = 0;//����ѡ���еı�����ʽ
                SelectObjectIsNull();
            }
            //���һ���ӿڶ��󣬵�����ʽ
            if (GUILayout.Button(new GUIContent("  " + TableOperation_valueObject[1], icon_add), GUILayout.Width(RightAreaWidth / count_operation1), GUILayout.Height(Height_operation)))
            {
                isAdding_Value = true;
                //�򿪵���
                PopWindowValueObjectAdd addWindow = (PopWindowValueObjectAdd)EditorWindow.GetWindowWithRect(
                       typeof(PopWindowValueObjectAdd),
                       new Rect(Screen.width / 2, Screen.height / 2, 400, 200),
                       false,
                       "Add value object");
                addWindow.LinkInterfaceLibraryWindow(this);
                addWindow.Show();
            }
            //�������һ����ͬ��ֵ���󣬼򻯲���
            if (GUILayout.Button(new GUIContent("  " + TableOperation_valueObject[2], icon_add), GUILayout.Width(RightAreaWidth / count_operation1), GUILayout.Height(Height_operation)))
            {
                if (lastValueObject != null)
                {
                    SetCurrentValueObject(lastValueObject);
                    isAdding_Value = true;
                }
                
            }
            //��Excel���е���ֵ����
            if (GUILayout.Button(new GUIContent("  " + TableOperation_valueObject[3], icon_excel), GUILayout.Width(RightAreaWidth / count_operation1), GUILayout.Height(Height_operation)))
            {
                ///�������ڣ�ѡ��Ҫ��ȡ�ı�
                PopWindowValueObjectExcelImport importWindow = (PopWindowValueObjectExcelImport)EditorWindow.GetWindowWithRect(
                    typeof(PopWindowValueObjectExcelImport),
                    new Rect(Screen.width / 2, Screen.height / 2, 400, 350),
                    false,
                    "Import Excel File");
                importWindow.LinkInterfaceLibraryWindow(this);
                importWindow.Show();

                ///QM.Log("Read Excel File");
            }
            GUILayout.Space(5);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            int count_operation2 = 4 + 1;
            //��Scene��ѡ�У��������б������е�ֵ����
            if (GUILayout.Button(new GUIContent("  " + TableOperation_valueObject[4], icon_pick), GUILayout.Width(RightAreaWidth / count_operation2), GUILayout.Height(Height_operation)))
            {
                SelectValueObjectInScene(List_valueObjects);
            }
            //���Ƶ�ǰ�������е�ֵ����
            if (GUILayout.Button(new GUIContent("  " + TableOperation_valueObject[5], icon_copy), GUILayout.Width(RightAreaWidth / count_operation2), GUILayout.Height(Height_operation)))
            {
                if (EditorUtility.DisplayDialog("��������", "�Ƿ�������ֵ����", "yes", "no"))
                {
                    isCopyAllObjs_Value = true;
                }
            }
            //ɾ����ǰ�������е�ֵ����
            if (GUILayout.Button(new GUIContent("  " + TableOperation_valueObject[6], icon_delete), GUILayout.Width(RightAreaWidth / count_operation2), GUILayout.Height(Height_operation)))
            {
                if (EditorUtility.DisplayDialog("ɾ������", "�Ƿ�ɾ������ֵ����", "yes", "no"))
                {
                    isRemoveAllObjs_Value = true;
                }
            }
            GUILayout.FlexibleSpace();
            ////ѡ�����ֵ��������з�ʽ
            if (GUILayout.Button(new GUIContent(icon_sort), GUILayout.Width(Height_operation * 1.5f), GUILayout.Height(Height_operation)))
            {
                //���ư�ť�������˵�
                GenericMenu sortwayMenu = new GenericMenu();
                sortwayMenu.AddItem(new GUIContent("�Ⱥ�˳��"), SortWay_Value.Equals(InterfaceSortWay.Order), OnValueSelectedOfValueSortwayCallback, InterfaceSortWay.Order);
                sortwayMenu.AddItem(new GUIContent("�ӿ�����"), SortWay_Value.Equals(InterfaceSortWay.Type), OnValueSelectedOfValueSortwayCallback, InterfaceSortWay.Type);

                sortwayMenu.ShowAsContext();
            }
            GUILayout.Space(15);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            /* 2.���Ʊ��� */
            GUILayout.BeginHorizontal();
            GUILayout.Space(2);
            GUILayout.Label(TableTitle_valueObject[0], EditorStyles.toolbarButton, GUILayout.Width(RightAreaWidth / 9));
            GUILayout.Label(TableTitle_valueObject[1], EditorStyles.toolbarButton, GUILayout.Width(RightAreaWidth * 2 / 9));
            GUILayout.Label(TableTitle_valueObject[2], EditorStyles.toolbarButton, GUILayout.Width(RightAreaWidth * 2 / 9));
            GUILayout.Label(TableTitle_valueObject[3], EditorStyles.toolbarButton, GUILayout.Width(RightAreaWidth * 1 / 9));
            GUILayout.Label(TableTitle_valueObject[4], EditorStyles.toolbarButton, GUILayout.Width(RightAreaWidth * 1 / 9));
            GUILayout.Label(TableTitle_valueObject[5], EditorStyles.toolbarButton, GUILayout.Width(RightAreaWidth * 2 / 9 - 22));
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            _scrollview3 = GUILayout.BeginScrollView(_scrollview3);//<! ������
            /* 3.����ֵ�����б� */
            ////List_interfaceObjs.ForEach(obj => OneInterfaceObjFormat(obj));//��*��������䡿��Ϊfoeeach�в�Ҫʹ��remove����add����
            for (int i = 0; i < List_valueObjects.Count; i++)
                OneValueObjFormat(List_valueObjects[i]);

            GUILayout.EndScrollView();//<! ������
        }


        /// <summary>
        /// һ�����У�ֵ����ĸ�ʽ
        /// </summary>
        /// <param name="valueObj"></param>
        void OneValueObjFormat(ValueObject valueObj)
        {
            if (valueObj == null)
                return;

            int rowNumber = valueObj.number;//��ȡ��ֵ��������
            //<! ������ż�б�����ɫ
            if (number_selectedRow_valueObj != rowNumber)
                rowStyle.normal.background = (rowNumber % 2 == 0) ? image_rowBgEven : image_rowBgOdd;
            else//<! ���˽ӿڶ���ѡ�У����޸ı�����ɫ
                rowStyle.normal.background = image_rowSelected;

            if(HashSet_errorValueObjects.Contains(valueObj))
                rowStyle.normal.background = image_error;

            GUILayout.Space(5);
            GUILayout.BeginHorizontal(rowStyle);
            GUILayout.Space(3);
            GUIStyle style_button = new GUIStyle(EditorStyles.miniButtonMid);//��ť��ʽ
            //<! ���
            GUILayout.Label(rowNumber.ToString(), labelStyle_MiddleCenter, GUILayout.Width(RightAreaWidth / 9 - 5));
            //<! ����
            GUILayout.Label(valueObj.name, labelStyle_MiddleCenter_Color, GUILayout.Width(RightAreaWidth * 2 / 9 - 5));
            //<! ��������
            GUILayout.Label(valueObj.datatype, labelStyle_MiddleCenter, GUILayout.Width(RightAreaWidth * 2 / 9 - 5));
            //<! ��ַ
            GUILayout.Label(valueObj.address, labelStyle_MiddleCenter, GUILayout.Width(RightAreaWidth * 1 / 9 - 5));
            //<! ֵ
            GUILayout.Label(valueObj.value, SetValueLabelGUIStyleByValueType(valueObj), GUILayout.Width(RightAreaWidth * 1 / 9 - 2));
            //<! ����
            float width = RightAreaWidth * 2 / 9 - 22;
            //�ı��ֵ�������Ϣ��������
            if (GUILayout.Button(icon_edit, style_button, GUILayout.Width(width / 4 - 1)))
            {
                isEditing_Value = true;
                SetEditedValueObject(valueObj);
                SelectValueObjectInScene(valueObj);
                number_selectedRow_valueObj = rowNumber;
                //�򿪵���
                PopWindowValueObjectEdit editWindow = (PopWindowValueObjectEdit)EditorWindow.GetWindowWithRect(
                       typeof(PopWindowValueObjectEdit),
                       new Rect(Screen.width / 2, Screen.height / 2, 400, 200),
                       false,
                       "Edit value object");
                editWindow.LinkInterfaceLibraryWindow(this);
                editWindow.Show();
            }
            //Pingһ����Hierarchy����е�λ�ã���Scene��ѡ�д�ֵ����
            if (GUILayout.Button(icon_pick, style_button, GUILayout.Width(width / 4 - 1)))
            {
                SelectValueObjectInScene(valueObj);
                number_selectedRow_valueObj = rowNumber;
            }
            //���ƴ�ֵ����
            if (GUILayout.Button("+", style_button, GUILayout.Width(width / 4 - 1)))
            {
                CopyOneValueObject(valueObj);
            }
            //ɾ����ֵ����
            if (GUILayout.Button("-", style_button, GUILayout.Width(width / 4 - 1)))
            {
                ///ɾ���б�Ԫ�ء���Ӧ��gameobject
                RemoveOneObjectInValueObjectList(valueObj, List_valueObjects);
            }

            GUILayout.EndHorizontal();
        }



        #endregion




        #region Methods


        /// <summary>
        /// ˢ�´˴��ڵ������Ϣ�����ȡasset���ݵ�
        /// </summary>
        void RefreshWindowContent()
        {
            //���»�ȡ��ǰ����
            currentScene = SceneManager.GetActiveScene();
            //���¶�ȡ����
            interfaceLibraryData = AssetDatabase.LoadAssetAtPath<InterfaceLibraryData>(RDTSPath.path_Data + "InterfaceLibraryData.asset");
            List_InterfaceModules = ReadLibraryData(interfaceLibraryData);
            Menu_interfaceType = GetMenuOfInterfaceType(List_InterfaceModules);
            //���ò���
            number_selectedRow_interfaceObj = 0;//����ѡ���еı�����ʽ
            number_selectedRow_valueObj = 0;
            currentSelectedInterfaceObj = null;//������ǰҪ�鿴�Ľӿڶ���
            SelectObjectIsNull();
            Repaint();
        }




        /*�����˵�����ģ�顾�л�ʱ������˸���󣬿��Ż���*/
        string Select_String = "";//���б������洢�����˵���ѡ����
        string DropDownMenu(string content, string[] itemList)
        {

            if (EditorGUILayout.DropdownButton(new GUIContent(content), FocusType.Keyboard))
            {
                GenericMenu menu = new GenericMenu();

                if (itemList.Length <= 0)
                    return null;

                //��Ӳ˵���ص������������
                foreach (string item in itemList)
                {
                    if (string.IsNullOrEmpty(item)) continue;
                    menu.AddItem(new GUIContent(item), content.Equals(item), OnValueSelectedCallback, item);
                }

                menu.ShowAsContext();
            }

            return Select_String;
        }

        //�ص�����
        void OnValueSelectedCallback(object value)
        {
            Select_String = value.ToString();

            // QM.Log(Select_String);
        }

        /// <summary>
        /// ����ʽ�Ļص�����
        /// </summary>
        /// <param name="value"></param>
        void OnValueSelectedOfSortwayCallback(object value)
        {
            SortWay_Interface = (InterfaceSortWay)value;
        }

        void OnValueSelectedOfValueSortwayCallback(object value)
        {
            SortWay_Value = (InterfaceSortWay)value;
        }


        /// <summary>
        /// ��ȡ�ӿڿ��asset������Դ
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        List<InterfaceModule> ReadLibraryData(InterfaceLibraryData data)
        {
            List<InterfaceModule> interfaceModules = new List<InterfaceModule>();

            if (data == null)
                return null;

            data.interfaceItems.ForEach(item =>
            {
                InterfaceModule newModule = new InterfaceModule();
                newModule.type = item.name;
                newModule.interfaceObject = item.interfaceObject;
                newModule.script = item.interfaceObject.GetComponent<Interface.BaseInterface>();
                interfaceModules.Add(newModule);
                QM.Log(newModule.type + ", " + newModule.interfaceObject.name + ", " + newModule.script);
            });

            return interfaceModules;
        }


        /// <summary>
        /// ���ݶ�ȡ����asset��������ȡ���ӿ����͵������˵��
        /// </summary>
        /// <param name="modules"></param>
        /// <returns></returns>
        string[] GetMenuOfInterfaceType(List<InterfaceModule> modules)
        {
            if (modules == null)
                return null;

            string[] menu_interfaceType = new string[0];
            List<string> menu = new List<string>();

            modules.ForEach(module =>
            {
                string type = module.type;
                menu.Add(type);
            });

            menu_interfaceType = menu.ToArray();

            return menu_interfaceType;
        }


        /// <summary>
        /// ���������Rect����
        /// </summary>
        void SetAreaRectParam()
        {
            //InterfacePanel��Rect����
            InterfacePanel.x = 0;///��������ڵ�Area��һ����Ԫ�أ�
            InterfacePanel.y = 0 + menuHeight;
            InterfacePanel.width = WindowWidth * widthSizeRatio;
            InterfacePanel.height = (WindowHeight - menuHeight) * heightSizeRatio;

            //InterfaceObjectTable��Rect����
            InterfaceObjectTable.x = 0;
            InterfaceObjectTable.y = 0 + menuHeight + InterfacePanel.height + resizerHeight;
            InterfaceObjectTable.width = WindowWidth * widthSizeRatio;
            InterfaceObjectTable.height = (WindowHeight - menuHeight) * (1 - heightSizeRatio) - resizerHeight;

            //ValueObjectTable��Rect����
            ValueObjectTable.x = resizerWidth;///��������ڵ�Area��һ����Ԫ�أ�
            ValueObjectTable.y = 0 + menuHeight;
            ValueObjectTable.width = WindowWidth * (1 - widthSizeRatio) - resizerWidth;
            ValueObjectTable.height = WindowHeight - menuHeight;

            //HeightResizer��Rect����
            HeightResizer.x = 0;
            HeightResizer.y = 0 + menuHeight + InterfacePanel.height - 15f;//-15fΪ�����϶�ʱ��ʾ�����ı仯
            HeightResizer.width = WindowWidth * widthSizeRatio;
            HeightResizer.height = resizerHeight +10f;//ֵ���resizerHeight��һЩ���ñ�ʶ��ȥ�϶�

            //WidthResizer��Rect����
            WidthResizer.x = 0 - 8f;//-8fΪ�˽������������ڽ��洦����Щ
            WidthResizer.y = 0 + menuHeight;
            WidthResizer.width = resizerWidth + 10f;//ֵ���resizerWidth��һЩ���ñ�ʶ��ȥ�϶�
            WidthResizer.height = WindowHeight - menuHeight;

        }


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

                        isResizingHeight = (Rect_heightResizer.Contains(e.mousePosition)) ? true : false;//�ж��Ƿ��ڵ����߶�

                        //���ڿ�ȵ�������Rect��������Ҳ�����ģ�������Ҫ����Rect���б任�����������ж��������λ��e.mousePosition�Ƿ��ڵ�������ΧRect��
                        Rect _RectWidthResizer = new Rect(
                            Rect_widthResizer.x + WindowWidth * widthSizeRatio,
                            Rect_widthResizer.y,
                            Rect_widthResizer.width,
                            Rect_widthResizer.height);
                        isResizingWidth = (_RectWidthResizer.Contains(e.mousePosition)) ? true : false;//�ж��Ƿ��ڵ������
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
                //�������¸߶�
                if (isResizingHeight && e.mousePosition.y > (menuHeight + 10) && e.mousePosition.y < (WindowHeight - 50))
                {
                    heightSizeRatio = e.mousePosition.y / WindowHeight;
                }
                //�������ҿ��
                if (isResizingWidth && e.mousePosition.x > 50 && e.mousePosition.x < (WindowWidth - 50))
                {
                    widthSizeRatio = e.mousePosition.x / WindowWidth;
                }

                // QM.Log("widthSizeRatio��" + widthSizeRatio + " " + "heightSizeRatio: " + heightSizeRatio);

                Repaint();
            }
        }


        /// <summary>
        /// ����ֵ�������������������������ֵ�ı�ǩGUI��ʽ
        /// </summary>
        /// <param name="valueObject"></param>
        /// <returns></returns>
        GUIStyle SetValueLabelGUIStyleByValueType(ValueObject valueObject)
        {
            GUIStyle labelGuiStyle = labelStyle_MiddleCenter;

            switch(valueObject.direction)
            {
                case VALUEDIRECTION.INPUT: labelGuiStyle = labelStyle_MiddleCenter_Red; break;
                case VALUEDIRECTION.OUTPUT: labelGuiStyle = labelStyle_MiddleCenter_Green; break;
                case VALUEDIRECTION.INPUTOUTPUT: labelGuiStyle = labelStyle_MiddleCenter_Orange; break;
            }

            return labelGuiStyle;
        }




        /* �Դ洢�����б�Ĳ��� */

        /// <summary>
        /// ���һ��InterfaceObject���͵��б�
        /// </summary>
        /// <param name="list">��յ�Ŀ���б�</param>
        void ClearInterfaceObjectList(List<InterfaceObject> list)
        {
            list.Clear();
        }


        /// <summary>
        /// ���һ��ValueObject���͵��б�
        /// </summary>
        /// <param name="list"></param>
        void ClearValueObjectList(List<ValueObject> list)
        {
            list.Clear();
        }


        /// <summary>
        /// ���õ�ǰҪ��ӵĽӿڶ���
        /// </summary>
        /// <param name="interfaceObj"></param>
        public InterfaceObject SetCurrentInterfaceObject(InterfaceObject interfaceObj)
        { 
            return currentInterfaceObject = interfaceObj;
        }


        /// <summary>
        /// ���õ�ǰҪ��ӵ�ֵ����
        /// </summary>
        /// <param name="valueObj"></param>
        /// <returns></returns>
        public ValueObject SetCurrentValueObject(ValueObject valueObj)
        {
            return currentValueObject = valueObj;
        }


        /// <summary>
        /// ���õ�ǰҪ�༭��ֵ����
        /// </summary>
        /// <param name="valueObj"></param>
        /// <returns></returns>
        ValueObject SetEditedValueObject(ValueObject valueObj)
        {
            return editedValueObject = valueObj;
        }


        /// <summary>
        /// ���ⲿ��ȡҪ�༭��ֵ����
        /// </summary>
        /// <returns></returns>
        public ValueObject GetEditedValueObject()
        {
            return editedValueObject;
        }


        /// <summary>
        /// ���õ�ǰ�ı༭��Ϣ
        /// </summary>
        /// <param name="valueObj"></param>
        /// <returns></returns>
        ValueObject SetEditInformation(ValueObject valueObj)
        {
            return EditInformation = valueObj;
        }


        /// <summary>
        /// ����isAdding_Interface��־λ
        /// </summary>
        /// <param name="value"></param>
        public void SetFlagIsAdding(bool value = true)
        {
            isAdding_Interface = value ? true : false;
        }


        /// <summary>
        /// ͨ�������������ӿڶ���ѡ��ӿ����͡�����ӿ����ƣ�������ӿ����
        /// </summary>
        /// <param name="type">�ӿ�����</param>
        /// <param name="name">�ӿ�����</param>
        public InterfaceObject GetOneInterfaceObjectByPopWindow(string type, string name)
        {
            InterfaceObject io = new InterfaceObject();
            int length = List_interfaceObjs.Count;

            io.number = length + 1;
            io.type = type;
            io.name = name;
            List_InterfaceModules.ForEach(module => {
                if (module.type == type)
                    io.interfaceObject = module.interfaceObject;
            });

            return io;
        }


        /// <summary>
        /// ͨ������������ֵ����ѡ���������͡������������ַ
        /// </summary>
        /// <param name="datatype"></param>
        /// <param name="name"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        public ValueObject GetOneValueObjectByPopWindow(string datatype, string name, string address)
        {
            ValueObject vo = new ValueObject();
            int length = List_valueObjects.Count;

            vo.number = length + 1;
            vo.datatype = datatype;
            vo.name = name;
            vo.address = address;
            vo.valueObject = null;
            ///QM.Log($"add valueObj: { vo.number} { vo.datatype} { vo.number} { vo.name} { vo.address}");
            SetTypeAndDirectionByDataTypeOfValueObj(vo);

            return vo;
        }


        /// <summary>
        /// ͨ���������༭ֵ���󣺻�ȡ�������͡������������ַ����Ϣ����Ϣ���ɸ��浯�����ݶ�����
        /// </summary>
        /// <param name="datatype"></param>
        /// <param name="name"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        public void GetEditInfoByPopWindow(string datatype, string name, string address)
        {
            ValueObject vo = new ValueObject();

            vo.datatype = datatype;
            vo.name = name;
            vo.address = address;

            SetEditInformation(vo);
        }




        /// <summary>
        /// �ӿڶ�����ӷ�������Ҫ������ط������λ��䣨���������ܣ�
        /// </summary>
        void AddCurrentInterfaceObject()
        {
            if (currentInterfaceObject == null || currentInterfaceObject.interfaceObject== null)
                return;

            InterfaceObject newIobj = new InterfaceObject();

            ///QM.Log(currentInterfaceObject.interfaceObject.ToString());

            //<! ���ݸ������ͺ�������ӽӿ���������ӿ��������Ϊnull����ᴴ����asset�ļ���Ԥ����ͬ���Ķ��������Ҫ�������ô˶����name
            GameObject newInterfaceObj = QM.AddComponent(currentInterfaceObject.interfaceObject, currentInterfaceObject.name);
           // Undo.RecordObject(go, "Add Interface Object");//��������
            currentInterfaceObject.name = newInterfaceObj.name;

            newIobj.name = currentInterfaceObject.name;
            newIobj.type = currentInterfaceObject.type;
            newIobj.interfaceObject = newInterfaceObj;

            List_interfaceObjs.Add(newIobj);//�����б�
            lastInterfaceObject = newIobj;//��¼Ϊ��һ����ӵĶ���
            SetCurrentInterfaceObject(null);//����ǰҪ��ӵĶ�����Ϊnull

        }


        /// <summary>
        /// ����ֵ�����datatype������type��direction
        /// </summary>
        /// <param name="valueObject"></param>
        ValueObject SetTypeAndDirectionByDataTypeOfValueObj(ValueObject valueObject)
        {
            VALUETYPE type = VALUETYPE.BOOL;
            VALUEDIRECTION direction = VALUEDIRECTION.INPUT;

            switch (valueObject.datatype)
            {
                //"InputBool", "InputFloat", "InputInt", "OutputBool", "OutputFloat", "OutputInt", "MiddleBool", "MiddleFloat", "MiddleInt"
                case "InputBool":
                    type = VALUETYPE.BOOL;
                    direction = VALUEDIRECTION.INPUT;
                    break;
                case "InputFloat":
                    type = VALUETYPE.REAL;
                    direction = VALUEDIRECTION.INPUT;
                    break;
                case "InputInt":
                    type = VALUETYPE.INT;
                    direction = VALUEDIRECTION.INPUT;
                    break;
                case "OutputBool":
                    type = VALUETYPE.BOOL;
                    direction = VALUEDIRECTION.OUTPUT;
                    break;
                case "OutputFloat":
                    type = VALUETYPE.REAL;
                    direction = VALUEDIRECTION.OUTPUT;
                    break;
                case "OutputInt":
                    type = VALUETYPE.INT;
                    direction = VALUEDIRECTION.OUTPUT;
                    break;
                case "MiddleBool":
                    type = VALUETYPE.BOOL;
                    direction = VALUEDIRECTION.INPUTOUTPUT;
                    break;
                case "MiddleFloat":
                    type = VALUETYPE.REAL;
                    direction = VALUEDIRECTION.INPUTOUTPUT;
                    break;
                case "MiddleInt":
                    type = VALUETYPE.INT;
                    direction = VALUEDIRECTION.INPUTOUTPUT;
                    break;
            }

            valueObject.type = type;
            valueObject.direction = direction;

            return valueObject;
        }


        /// <summary>
        /// ����ֵ�����type��direction������datatype
        /// </summary>
        /// <param name="valueObject"></param>
        /// <returns></returns>
        ValueObject SetDataTypeByTypeAndDirectionOfValueObj(ValueObject valueObject)
        {
            VALUETYPE type = valueObject.type;
            VALUEDIRECTION direction = valueObject.direction;

            //"InputBool", "InputFloat", "InputInt", "OutputBool", "OutputFloat", "OutputInt", "MiddleBool", "MiddleFloat", "MiddleInt"
            if (direction == VALUEDIRECTION.INPUT && type == VALUETYPE.BOOL)
                valueObject.datatype = "InputBool";
            if (direction == VALUEDIRECTION.INPUT && type == VALUETYPE.INT)
                valueObject.datatype = "InputInt";
            if (direction == VALUEDIRECTION.INPUT && type == VALUETYPE.REAL)
                valueObject.datatype = "InputFloat";

            if (direction == VALUEDIRECTION.OUTPUT && type == VALUETYPE.BOOL)
                valueObject.datatype = "OutputBool";
            if (direction == VALUEDIRECTION.OUTPUT && type == VALUETYPE.INT)
                valueObject.datatype = "OutputInt";
            if (direction == VALUEDIRECTION.OUTPUT && type == VALUETYPE.REAL)
                valueObject.datatype = "OutputFloat";

            if (direction == VALUEDIRECTION.INPUTOUTPUT && type == VALUETYPE.BOOL)
                valueObject.datatype = "MiddleBool";
            if (direction == VALUEDIRECTION.INPUTOUTPUT && type == VALUETYPE.INT)
                valueObject.datatype = "MiddleInt";
            if (direction == VALUEDIRECTION.INPUTOUTPUT && type == VALUETYPE.REAL)
                valueObject.datatype = "MiddleFloat";


            return valueObject;
        }


        /// <summary>
        /// ֵ������ӷ�������Ҫ������ط������λ��䣨���������ܣ�
        /// </summary>
        bool AddCurrentValueObject()
        {
            if (currentValueObject == null)
                return false;

            currentValueObject.parent  = currentSelectedInterfaceObj ?? null;

            GameObject parent = currentValueObject.parent?.interfaceObject ?? null;
            string name = currentValueObject.name;
            string address = currentValueObject.address;
            VALUETYPE type = currentValueObject.type;
            VALUEDIRECTION direction = currentValueObject.direction;
            //VALUETYPE type = VALUETYPE.BOOL;
            //VALUEDIRECTION direction = VALUEDIRECTION.INPUT;
            //switch (currentValueObject.datatype)
            //{
            //    //"InputBool", "InputFloat", "InputInt", "OutputBool", "OutputFloat", "OutputInt", "MiddleBool", "MiddleFloat", "MiddleInt"
            //    case "InputBool":
            //        type = VALUETYPE.BOOL;
            //        direction = VALUEDIRECTION.INPUT;
            //        break;
            //    case "InputFloat":
            //        type = VALUETYPE.REAL;
            //        direction = VALUEDIRECTION.INPUT;
            //        break;
            //    case "InputInt":
            //        type = VALUETYPE.INT;
            //        direction = VALUEDIRECTION.INPUT;
            //        break;
            //    case "OutputBool":
            //        type = VALUETYPE.BOOL;
            //        direction = VALUEDIRECTION.OUTPUT;
            //        break;
            //    case "OutputFloat":
            //        type = VALUETYPE.REAL;
            //        direction = VALUEDIRECTION.OUTPUT;
            //        break;
            //    case "OutputInt":
            //        type = VALUETYPE.INT;
            //        direction = VALUEDIRECTION.OUTPUT;
            //        break;
            //    case "MiddleBool":
            //        type = VALUETYPE.BOOL;
            //        direction = VALUEDIRECTION.INPUTOUTPUT;
            //        break;
            //    case "MiddleFloat":
            //        type = VALUETYPE.REAL;
            //        direction = VALUEDIRECTION.INPUTOUTPUT;
            //        break;
            //    case "MiddleInt":
            //        type = VALUETYPE.INT;
            //        direction = VALUEDIRECTION.INPUTOUTPUT;
            //        break;
            //}

            //��ָ���Ķ����´���һ��"ֵ����",��δѡ�и������򴴽�Ϊ������
            GameObject newValueObj =  QM.CreateOneValueObj(parent, name, address, type, direction);
            QM.Log($"����ֵ���� {newValueObj.name}");

            currentValueObject.name = newValueObj.name;
            currentValueObject.valueObject = newValueObj;
            List_valueObjects.Add(currentValueObject);

            lastValueObject = currentValueObject;//��¼Ϊ��һ����ӵĶ���
            SetCurrentValueObject(null); //����ǰҪ��ӵĶ�����Ϊnull

            return true;
        }


        /// <summary>
        /// ����targetObj����Ϣ�༭�޸�editedObj����Ҫ�����ơ��������͡���ַ��
        /// </summary>
        /// <param name="editedObj"></param>
        /// <param name="targetObj"></param>
        /// <returns></returns>
        bool EditValueObject(ValueObject editedObj, ValueObject targetObj)
        {
            editedObj.name = targetObj.name;
            editedObj.datatype = targetObj.datatype;
            editedObj.address = targetObj.address;

            ///���Ķ�Ӧ��gameobject�ϵ�Value�ű�������Ϣ
            SetTypeAndDirectionByDataTypeOfValueObj(editedObj);
            QM.EditOneValueObj(editedObj.valueObject, editedObj.name, editedObj.address, editedObj.type, editedObj.direction);

            return true;
        }




        /// <summary>
        /// �Ƴ�һ���ӿڶ��󣨴��������ܣ�
        /// </summary>
        /// <param name="interfaceObj"></param>
        /// <param name="list"></param>
        void RemoveOneObjectInInterfaceObjectList(InterfaceObject interfaceObj, List<InterfaceObject> list)
        {
            if (list == null || interfaceObj == null || interfaceObj.interfaceObject == null)
                return;

            if (!list.Contains(interfaceObj))//����������Ԫ�ؾ�ֱ�ӷ���
                return;

            int numberRemove = interfaceObj.number;//Ҫ�Ƴ��Ľӿڶ������
            list.Remove(interfaceObj);//���б��Ƴ�

            Undo.DestroyObjectImmediate(interfaceObj.interfaceObject);//�������ٶ��������*Ҫ�����������ǰ��
            DestroyImmediate(interfaceObj.interfaceObject);//���ٶ�Ӧ��gameobject 

            //�����б���Ԫ�ص����
            list.ForEach(obj => {
                obj.number = (obj.number > numberRemove) ? (obj.number - 1) : obj.number;//�������Ҫ�Ƴ��Ľӿڶ������֮��ģ���һ
            });

        }


        /// <summary>
        /// ɾ��ָ���ӿڶ����б������еĽӿڶ���
        /// </summary>
        /// <param name="list"></param>
        bool RemoveAllObjectsInInterfaceObjectList(List<InterfaceObject> list)
        {
            if (list == null)
                return false;

            for (int i=0; i<list.Count; i++)
            {
                InterfaceObject obj = list[i];
                list.Remove(obj);//���б��Ƴ�
                if (obj != null && obj.interfaceObject != null)
                {
                    Undo.DestroyObjectImmediate(obj.interfaceObject);//�������ٶ��������*Ҫ�����������ǰ��
                    DestroyImmediate(obj.interfaceObject);//���ٶ�Ӧ��gameobject 
                }
            }

            return true;
        }


        /// <summary>
        /// �Ƴ�һ��ֵ���󣨴��������ܣ�
        /// </summary>
        /// <param name="valueObj"></param>
        /// <param name="list"></param>
        void RemoveOneObjectInValueObjectList(ValueObject valueObj, List<ValueObject> list)
        {
            if (list == null || valueObj == null || valueObj.valueObject == null)
                return;

            if (!list.Contains(valueObj))//����������Ԫ�ؾ�ֱ�ӷ���
                return;

            int numberRemove = valueObj.number;//Ҫ�Ƴ��Ľӿڶ������
            list.Remove(valueObj);//���б��Ƴ�

            Undo.DestroyObjectImmediate(valueObj.valueObject);//�������ٶ��������*Ҫ�����������ǰ��
            DestroyImmediate(valueObj.valueObject);//���ٶ�Ӧ��gameobject 

            //�����б���Ԫ�ص����
            list.ForEach(obj => {
                obj.number = (obj.number > numberRemove) ? (obj.number - 1) : obj.number;//�������Ҫ�Ƴ��Ľӿڶ������֮��ģ���һ
            });

        }


        /// <summary>
        /// ɾ��ָ��ֵ�����б������е�ֵ����
        /// </summary>
        /// <param name="list"></param>
        bool RemoveAllObjectsInValueObjectList(List<ValueObject> list)
        {
            if (list == null)
                return false;

            for (int i = 0; i < list.Count; i++)
            {
                ValueObject obj = list[i];
                list.Remove(obj);//���б��Ƴ�
                if (obj != null && obj.valueObject != null)
                {
                    Undo.DestroyObjectImmediate(obj.valueObject);//�������ٶ��������*Ҫ�����������ǰ��
                    DestroyImmediate(obj.valueObject);//���ٶ�Ӧ��gameobject 
                }
            }

            return true;
        }




        /// <summary>
        /// ����ָ���Ľӿڶ���
        /// </summary>
        /// <param name="interfaceObj"></param>
        void CopyOneInterfaceObject(InterfaceObject interfaceObj)
        {
            currentInterfaceObject = interfaceObj;
            isAdding_Interface = true;
        }


        /// <summary>
        /// �������е�ǰ�б��еĽӿڶ���*ֱ��ͨ������gameobject�ķ�ʽ��������ͨ������currentInterfaceObject��
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        bool CopyAllObjectsInInterfaceObjectList(List<InterfaceObject> list)
        {
            if (list == null)
                return false;

            for(int i=0; i<list.Count; i++)
            {
                InterfaceObject iobj = list[i];
                GameObject go = QM.AddComponent(iobj.interfaceObject, iobj.name);
                Undo.RecordObject(go, "Add Interface Object");//��������
            }

            return true;//ȫ��������ɺ󷵻�true
        }


        /// <summary>
        /// ����ָ����ֵ����
        /// </summary>
        /// <param name="valueObj"></param>
        void CopyOneValueObject(ValueObject valueObj)
        {
            currentValueObject = valueObj;
            isAdding_Value = true;
        }


        /// <summary>
        /// �������е�ǰ�б��е�ֵ����*ֱ��ͨ������gameobject�ķ�ʽ��������ͨ������currentValueObject��
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        bool CopyAllObjectsInValueObjectList(List<ValueObject> list)
        {
            if (list == null)
                return false;

            for (int i = 0; i < list.Count; i++)
            {
                ValueObject vobj = list[i];
                GameObject go = QM.CreateOneValueObj(vobj.parent?.interfaceObject, vobj.name, vobj.address, vobj.type, vobj.direction);
                ////Undo.RecordObject(go, "Add Value Object");//��������
            }

            return true;//ȫ��������ɺ󷵻�true
        }




        /// <summary>
        /// �ڳ�����ѡ��һ��ָ���ӿڶ���
        /// </summary>
        /// <param name="interfaceObj"></param>
        void SelectInterfaceObjectInScene(InterfaceObject interfaceObj)
        {
            if (interfaceObj == null)
                return;

            List<UnityEngine.Object> select = new List<UnityEngine.Object>();
            select.Add(interfaceObj.interfaceObject);
            Selection.objects = select.ToArray();
        }


        /// <summary>
        /// �ڳ�����ѡ��ָ���ӿڶ����б��е����ж���
        /// </summary>
        /// <param name="interfaceObjs"></param>
        void SelectInterfaceObjectInScene(List<InterfaceObject> interfaceObjs)
        {
            if (interfaceObjs == null)
                return;

            List<UnityEngine.Object> select = new List<UnityEngine.Object>();
            interfaceObjs.ForEach(obj => {
                if (obj == null) return;
                select.Add(obj.interfaceObject);
            });
            
            Selection.objects = select.ToArray();
        }


        /// <summary>
        /// �ڳ�����ѡ��һ��ָ��ֵ����
        /// </summary>
        /// <param name="valueObj"></param>
        void SelectValueObjectInScene(ValueObject valueObj)
        {
            if (valueObj == null)
                return;

            List<UnityEngine.Object> select = new List<UnityEngine.Object>();
            select.Add(valueObj.valueObject);
            Selection.objects = select.ToArray();
        }


        /// <summary>
        /// �ڳ�����ѡ��ָ��ֵ�����б��е����ж���
        /// </summary>
        /// <param name="valueObjs"></param>
        void SelectValueObjectInScene(List<ValueObject> valueObjs)
        {
            if (valueObjs == null)
                return;

            List<UnityEngine.Object> select = new List<UnityEngine.Object>();
            valueObjs.ForEach(obj => {
                if (obj == null) return;
                select.Add(obj.valueObject);
            });

            Selection.objects = select.ToArray();
        }


        /// <summary>
        /// ����ѡ�����Ϊ��
        /// </summary>
        void SelectObjectIsNull()
        {
            Selection.objects = null;
        }




        /// <summary>
        /// ��ȡָ�������Ͻӿ���������ͣ�������
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        string GetInterfaceTypeOfGameobject(GameObject obj)
        {
            string type = null;
            ///QM.Log(obj.GetComponent<Interface.BaseInterface>().GetType().ToString());

            List_InterfaceModules.ForEach(module => {
                ///QM.Log(module.script.ToString());
                if (module.script.GetType().ToString() == obj?.GetComponent<Interface.BaseInterface>().GetType().ToString())
                    type = module.type;
            });

            return type;
        }


        /// <summary>
        /// ��ȡָ��������ֵ��������ͣ�������
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        string GetValueTypeOfGameobject(GameObject obj)
        {
            string datatype = null;
            ///QM.Log(obj.GetComponent<Interface.BaseInterface>().GetType().ToString());

            List<string> List_ValueType = Menu_valueType.ToList();

            List_ValueType.ForEach(type => {
                ///QM.Log(obj?.GetComponent<Value>().GetType().ToString());
                if (("RDTS.Value"+type).Equals(obj?.GetComponent<Value>().GetType().ToString()))
                    datatype = type;
            });
           /// QM.Log(datatype);
            return datatype;
        }


        /// <summary>
        /// ��ȡָ�����͵Ľӿڶ���
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        GameObject GetInterfaceObjectOfGameobject(GameObject obj)
        { 
            GameObject go = null;

            List_InterfaceModules.ForEach(module => {
                if (module.script.GetType().ToString().Equals(obj?.GetComponent<Interface.BaseInterface>().GetType().ToString()))//�жϵ�ǰ�ӿ������Ƿ������asset�ļ���
                    go = obj;
            });
            ///QM.Log(go.name);
            return go;
        }


        /// <summary>
        /// ��ȡָ�����͵�ֵ����
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        GameObject GetValueObjectOfGameobject(GameObject obj)
        {
            GameObject go = null;

            List<string> List_ValueType = Menu_valueType.ToList();

            List_ValueType.ForEach(type => {
                ///QM.Log(module.script.ToString());
                if (("RDTS.Value" + type).Equals(obj?.GetComponent<Value>().GetType().ToString()))//�жϵ�ǰֵ�����Ƿ�������趨��menu��
                    go = obj;
            });

            return go;
        }


        /// <summary>
        /// ��ȡֵ�����Name����
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        string GetValueNameOfGameobject(GameObject obj)
        {
            return obj?.GetComponent<Value>().Name;
        }

        /// <summary>
        /// ����ֵ�����Name����
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        void SetValueNameOfGameobject(GameObject obj, string name)
        {
            if (obj == null)
                return;

            obj.GetComponent<Value>().Name = name;
        }

        /// <summary>
        /// ��ȡֵ����ĵ�ַ��Ϣ
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        string GetValueAddressOfGameobject(GameObject obj)
        {
            return obj?.GetComponent<Value>().Address;
        }


        /// <summary>
        /// ����ָ����gameobject����ֵ�������ر���
        /// </summary>
        /// <param name="valueObject"></param>
        /// <param name="obj"></param>
        void SetValueObjectVariablesByGameObject(ValueObject valueObject, GameObject obj)
        {
            if (valueObject == null || obj == null)
                return;


            Value valueScript = obj.GetComponent<Value>();//��ȡValue�ű�
            List<string> List_ValueType = Menu_valueType.ToList();

            List_ValueType.ForEach(type => {
                ///QM.Log(obj?.GetComponent<Value>().GetType().ToString());
                
                if (("RDTS.Value" + type).Equals(valueScript.GetType().ToString()))//�жϵ�ǰֵ�����Ƿ�������趨��menu��
                {
                    valueObject.name = valueScript.Name = obj.name;//��ֵ���� ��Value�е�Name��������gameobject����ͬ��
                    valueObject.datatype = type;//����ֵ�������������
                    valueObject.address = valueScript.Address;//����ֵ�����ַ
                    valueObject.valueObject = obj;//����ֵ�����gameobject
                    valueObject.value = valueScript.GetVisuText();//����ֵ�����ֵ
                }
            });
           
        }




        /// <summary>
        /// �����ǰ���������еĽӿڶ��󣬲���ӵ��б���������ʾ��*ע�⣺���Ǹ������Ҵ���δ����״̬�µ�gameobject���ᱻ�ҵ���
        /// </summary>
        void BrowseInterfaceGameObjectsInCurrentScene()
        {
            //��ȡ��ǰ���������еĽӿڶ���
            List<GameObject> interfaceObjsInScene = new List<GameObject>();
            interfaceObjsInScene = QM.GetGameObjectsInCurrentScene(typeof(Interface.BaseInterface));
            ///interfaceObjsInScene.ForEach(obj => QM.Log(obj.name));
            //���½ӿڶ����б�
            List<InterfaceObject> newList_interfaceObjs = new List<InterfaceObject>();
            interfaceObjsInScene.ForEach(obj => {
                InterfaceObject io = new InterfaceObject();
                int length = newList_interfaceObjs.Count;

                io.number = length + 1;
                io.type = GetInterfaceTypeOfGameobject(obj);
                io.name = obj.name;
                io.interfaceObject = GetInterfaceObjectOfGameobject(obj);

                newList_interfaceObjs.Add(io);
            });
            //<! ��������ʽ�Ա��ж����������
            switch (SortWay_Interface)
            {
                case InterfaceSortWay.Order://���Ⱥ�˳������
                    //do nothing...
                    break;
                case InterfaceSortWay.Type://���ӿ���������
                    List<InterfaceObject> cacheList_interfaceObjs = new List<InterfaceObject>();///�����б����ڰ��սӿ����͵�˳��洢�ӿڶ���
                    List_InterfaceModules.ForEach(module => {///����ÿһ��������Դ�еĽӿ�����
                        newList_interfaceObjs.ForEach(obj => { ///�����ӳ����л�ȡ�������нӿڶ���
                            if(module.type.Equals(obj.type))///������ƥ��
                            {
                                InterfaceObject newObj = obj;
                                newObj.number = cacheList_interfaceObjs.Count + 1;///�������
                                cacheList_interfaceObjs.Add(newObj);///����������б�
                            }

                        });
                    });
                    newList_interfaceObjs = cacheList_interfaceObjs;

                    break;
            }

            ClearInterfaceObjectList(List_interfaceObjs);//�����һ���б�
            List_interfaceObjs = newList_interfaceObjs;
            Repaint();//<! ����ʵʱˢ�´��ڸı�����
            
        }


        /// <summary>
        /// �����ǰ���������е�ֵ���󣬲���ӵ��б���������ʾ��*ע�⣺���Ǹ������Ҵ���δ����״̬�µ�gameobject���ᱻ�ҵ���
        /// </summary>
        void BrowseValueGameObjectsInCurrentScene()
        {
            List<string> List_ValueType = Menu_valueType.ToList();//ֵ����
            //��ȡ��ǰ���������е�ֵ����
            List<GameObject> valueObjsInScene = new List<GameObject>();
            valueObjsInScene = (currentSelectedInterfaceObj != null)? 
                    QM.GetGameObjectsUnderGivenGameobject(currentSelectedInterfaceObj.interfaceObject, typeof(Value)) : 
                    QM.GetGameObjectsInCurrentScene(typeof(Value));
            //����ֵ�����б�
            List<ValueObject> newList_valueObjs = new List<ValueObject>();
            valueObjsInScene.ForEach(obj => {
                ValueObject vo = new ValueObject();
                int length = newList_valueObjs.Count;

                vo.number = length + 1;
                ////vo.datatype = GetValueTypeOfGameobject(obj);
                ////vo.name = obj.name; SetValueNameOfGameobject(obj, obj.name);
                ////vo.address = GetValueAddressOfGameobject(obj);
                ////vo.valueObject = GetValueObjectOfGameobject(obj);
                SetValueObjectVariablesByGameObject(vo, obj);///������������д���Ч�ʸ���
                SetTypeAndDirectionByDataTypeOfValueObj(vo);
                vo.parent = currentSelectedInterfaceObj ?? null;

                newList_valueObjs.Add(vo);
            });
            //<! ��������ʽ�Ա��ж����������
            switch (SortWay_Value)
            {
                case InterfaceSortWay.Order://���Ⱥ�˳������
                    //do nothing...
                    break;
                case InterfaceSortWay.Type://���ӿ���������
                    List<ValueObject> cacheList_valueObjs = new List<ValueObject>();///�����б����ڰ���ֵ���͵�˳��洢ֵ����
                    List_ValueType.ForEach(type => {///����ÿһ��ֵ����menu�е�����
                        newList_valueObjs.ForEach(obj => { ///�����ӳ����л�ȡ��������ֵ����
                            if (type.Equals(obj.datatype))///������ƥ��
                            {
                                ValueObject newObj = obj;
                                newObj.number = cacheList_valueObjs.Count + 1;///�������
                                cacheList_valueObjs.Add(newObj);///����������б�
                            }

                        });
                    });
                    newList_valueObjs = cacheList_valueObjs;

                    break;
            }

            ClearValueObjectList(List_valueObjects);//�����һ���б�
            List_valueObjects = newList_valueObjs;
            Repaint();//<! ����ʵʱˢ�´��ڸı�����
            ///QM.Log("BrowseObjectsInCurrentScene...");
        }




        /* ֵ�����ʽ�����ط��� *//////////////
        /// <summary>
        /// ��ֵ������еĶ�����и�������(��ʽ)�ļ�⡾ע�⣺��δ����Ե�ַ�ļ�⣬��Ϊ��ͬ�ӿڶ��ڵ�ַ�ĸ�ʽ��ͬ���������Լ����Ż���
        /// </summary>
        /// <param name="valueObjList">������б�</param>
        void ValueObjectPropertyCheck(List<ValueObject> valueObjList)
        {
            if (valueObjList == null)
                return;

            HashSet<ValueObject> NullName = new HashSet<ValueObject>();//��¼�����Ķ���
            HashSet<ValueObject> SameName = new HashSet<ValueObject>();//��¼ͬ���Ķ���
            HashSet<ValueObject> SameAddress = new HashSet<ValueObject>();//��¼ͬ��ַ�Ķ���
            HashSet<ValueObject> ErrorAddress = new HashSet<ValueObject>();//��¼��ַ��ʽ����Ķ���
            HashSet<ValueObject> ExceededAddress = new HashSet<ValueObject>();//��¼������ַ�Ķ���

            int length = valueObjList.Count;//��ȡ�б���   

            for (int i=0; i< length; i++)
            {
                ValueObject currentObj = valueObjList[i];
                //<! �������
                if (currentObj.name == null || currentObj.name == "")
                    NullName.Add(currentObj);

                
                for(int j=i+1; j<length; j++)
                {
                    ValueObject nextObj = valueObjList[j];
                    //<! ͬ�����
                    if (currentObj.name == nextObj.name)
                    {
                        SameName.Add(currentObj);
                        SameName.Add(nextObj);
                    }
                    //<! ַͬ���
                    if (currentObj.direction == nextObj.direction && currentObj.address == nextObj.address)
                    {
                        SameAddress.Add(currentObj);
                        SameAddress.Add(nextObj);
                    }

                }

            }


            HashSet_errorValueObjects.Clear();//�����
            //���������е�Ԫ�غϲ�
            if (NullName.Count > 0)
                HashSet_errorValueObjects.UnionWith(NullName);
            if (SameName.Count > 0)
                HashSet_errorValueObjects.UnionWith(SameName);
            if (SameAddress.Count > 0)
                HashSet_errorValueObjects.UnionWith(SameAddress);
            if (ErrorAddress.Count > 0)
                HashSet_errorValueObjects.UnionWith(ErrorAddress);
            if (ExceededAddress.Count > 0)
                HashSet_errorValueObjects.UnionWith(ExceededAddress);
            //���ϲ���ļ���Ԫ�أ�˵���и�ʽ����Ķ���
            if (HashSet_errorValueObjects.Count > 0)
                isCheckError = true;
            else
                isCheckError = false;

        }



        #endregion




        #region Import Excel

        /// <summary>
        /// ��ȡ�Ӳ�;potal�е�����excel�������
        /// </summary>
        /// <param name="filePath">excel�ļ�·��</param>
        /// <param name="sheetName">Ҫ��ȡ�ı�</param>
        public  System.Data.DataSet ReadPortalExcelFile(string filePath)
        {
            return ExcelReader.ReadExcel(filePath);
        }


        /// <summary>
        /// ��ȡ��;excel���еı�����
        /// </summary>
        /// <param name="excelResult"></param>
        /// <returns></returns>
        public  System.Data.DataTableCollection ReadPortalExcelFileTables(System.Data.DataSet excelResult)
        {
            return excelResult.Tables;
        }


        /// <summary>
        /// ���ж�ȡ��;excel���б�������
        /// </summary>
        /// <param name="excelResult"></param>
        /// <param name="sheetName"></param>
        public void ReadPortalExcelFileRows(System.Data.DataSet excelResult, string sheetName)
        {
            var excelRowData = excelResult.Tables[sheetName].Rows;


            for (int i = 1; i < excelRowData.Count; i++)
            {
                ValueObject readObj = new ValueObject();
                readObj.number = List_valueObjects.Count + 1;//�������
                readObj.name = excelRowData[i][0].ToString();//��������

                string datatType = excelRowData[i][2].ToString();///��Bool Int Real��
                string logicalAddress = excelRowData[i][3].ToString();///�硰%I5.0��
                string addressType = logicalAddress[1].ToString();///��I Q M��
                string addressValue = logicalAddress.Remove(0, 2);///�硰5.0��

                readObj.address = addressType + addressValue;//���õ�ַ
                //����type
                switch (datatType)
                {
                    case "Bool": readObj.type = VALUETYPE.BOOL; break;
                    case "Int": readObj.type = VALUETYPE.INT; break;
                    case "Real": readObj.type = VALUETYPE.REAL; break;
                }
                //����direction
                switch (addressType)
                {
                    case "I": readObj.direction = VALUEDIRECTION.INPUT; break;
                    case "Q": readObj.direction = VALUEDIRECTION.OUTPUT; break;
                    case "M": readObj.direction = VALUEDIRECTION.INPUTOUTPUT; break;
                }

                SetDataTypeByTypeAndDirectionOfValueObj(readObj);//����datatype
                readObj.parent = currentSelectedInterfaceObj ?? null;//����parent

                GameObject go = QM.CreateOneValueObj(readObj.parent?.interfaceObject, readObj.name, readObj.address, readObj.type, readObj.direction);
                readObj.valueObject = go;//����valueobject

            }
        }



        #endregion


        #region  EditorCoroutine


        void StartEditorCoroutine(string methodName)
        {
            this.StartCoroutine(methodName);
            QM.Log("InterfaceLibraryWindow �� ����Э��");
            isCoroutineStart = true;
        }


        void StopEditorCoroutine(string methodName)
        {
            this.StopCoroutine(methodName);//�ر�Э��
            QM.Log("InterfaceLibraryWindow �� �ر�Э��");
            isCoroutineStart = false;
        }



        /// <summary>
        /// Э�̷��������ڶ�ʱ�����������������OnGUI�ж�ε����������������Layout/Repaint��������
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        IEnumerator BrowseObjectsInCurrentScene()
        {
            for (; ; )
            {
                BrowseInterfaceGameObjectsInCurrentScene();
                BrowseValueGameObjectsInCurrentScene();
                yield return new WaitForSeconds(.1f);//�ӳ�ʱ��
                //QM.Log("BrowseObjectsInCurrentScene...");
            }
        }




        #endregion


    }

#endif
}