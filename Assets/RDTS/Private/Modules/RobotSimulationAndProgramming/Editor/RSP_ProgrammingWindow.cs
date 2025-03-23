using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using RDTS.Utility;
using RDTS.Window;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEditorInternal;
using System.Reflection;
using VisualSpline;


namespace RDTS.RobotSimulationProgramming
{

    /// <summary>
    /// ���ڵ���Ŀ���Ĳ����˵�
    /// </summary>
    public enum TargetOperationMenu
    {
        None,//�޲���
        Select,//ѡ��
        Tesch,//ʾ��
        Edit,//�༭
        Copy,//���ƣ��������½�Ŀ�����첻�������Ȳ�ʵ�֣���������Ҫ����ʵ�֣�
        Delete//ɾ��

    }

    /// <summary>
    /// ���ڵ���ָ��Ĳ����˵�
    /// </summary>
    public enum InstructionOperationMenu
    {
        None,//�޲���
        Select,//ѡ��
        Process,//ִ��
        Edit,//�༭
        Delete//ɾ��

    }


    /// <summary>
    /// ���ڵ��������Ĳ����˵�
    /// </summary>
    public enum ParameterOperationMenu
    {
        None,//�޲���
        Select,//ѡ��
        Edit,//�༭
        Delete//ɾ��

    }

    /// <summary>
    /// �����˱�̴��ڣ�Ӧ���������ļ���asset�ļ����Ĵ򿪴��ڵ����ݣ�
    /// </summary>
    public class RSP_ProgrammingWindow : EditorWindow
    {
        public RSP_RobotProgram program;//������asset�ļ�
        public RSP_RobotController robotController;//�����Ļ����˿������ű��������
        public List<TargetObject> targets = new List<TargetObject>();//Ŀ�������б�
        public TargetObject currentTarget;//��ǰѡ�е�Target
        public List<BaseInstruction> instructions = new List<BaseInstruction>();//����ָ���б�
        public BaseInstruction currentInstruction;//��ǰѡ�е�ָ��
        public int currentInstructionIndex;//��ǰѡ�е�ָ�������
        public List<IOParameter> ioParameters = new List<IOParameter>();//IO�����б�
        public List<IOParameter> inParameters = new List<IOParameter>();//input���Ͳ����б�
        public List<IOParameter> outParameters = new List<IOParameter>();//output���Ͳ����б�
        public IOParameter currentParameter;////��ǰѡ�е�Parameter

        public event Action<RSP_RobotProgram> programLoaded;
        public event Action<RSP_RobotProgram> programUnloaded;



        private RSP_RobotController _lastRobotController;//��¼��һ�������Ŀ��������
        private bool isProgramInit = false;
        private bool isInstructionInit = false;


        [MenuItem("Parallel-RDTS/Window/RobotSimulationProgramming", false, 200)]
        public static void ShowWindow()
        {
            RSP_ProgrammingWindow window = (RSP_ProgrammingWindow)EditorWindow.GetWindow(typeof(RSP_ProgrammingWindow));
            window.titleContent = RSP_ProgrammingWindow.thisTitleContent;//���ñ����ͼ��
            window.minSize = new Vector2(300, 200);
            window.Show();
        }


        public void InitializeProgram(RSP_RobotProgram program)
        {
            if (this.program != null && this.program != program)
            {
                // Save the graph to the disk ��ͼ���浽����
                EditorUtility.SetDirty(this.program);
                AssetDatabase.SaveAssets();
                // Unload the graph ж�ش�ͼ��
                programUnloaded?.Invoke(this.program);
                //SetRobotController(null);
            }


            programLoaded?.Invoke(program);
            this.program = program;


            //���ô�������

            InitializeWindow(program);

        }


        protected virtual void InitializeWindow(RSP_RobotProgram program)
        {
            this.robotController = null;
            this.targets = program?.targets;
            this.ioParameters = program?.ioParameters;
            this.instructions = program?.instructions;
            //SeparateIOParameter();//����input��output���͵Ĳ����б�
            isProgramInit = true;
            isInstructionInit = true;

            Repaint();
        }


        void OnDisable()
        {
            // Save the graph to the disk ��ͼ���浽����
            EditorUtility.SetDirty(this.program);
            AssetDatabase.SaveAssets();
            // Unload the graph ж�ش�ͼ��
            programUnloaded?.Invoke(this.program);

        }




        void OnHierarchyChange()
        {
            //QM.Log("Hierarchy Change");
        }

        void OnInspectorUpdate()
        {
            //QM.Log("Inspector Update");
            /* ��δ������isLockδfalse��������Hierarchy��ѡ��Ķ�����п��������ʱ����ֱ�Ӹ��´������� */
            var objs = Selection.gameObjects;
            if (objs != null && objs.Length > 0 && !islock)
            {
                if (objs[0] != null && objs[0].GetComponent<RSP_RobotController>() != null)
                {
                    robotController = objs[0].GetComponent<RSP_RobotController>();
                    if (_lastRobotController != robotController)
                    {
                        this.program = robotController.program;
                        isProgramInit = true;
                        isInstructionInit = true;
                    }

                    _lastRobotController = robotController;
                }



            }

            // if (robotController != null) GetTargtsFromRobotController();//��ȡ��������������е�Ŀ�����Ϣ
            // else GetTargtsFromProgramAsset(program);//��ȡasset�ļ��е�Ŀ�����Ϣ

            //���������ֵ�ĸ���
            UpdateValueOfParameter();
            //
            if (uiChange_instruction)
            {
                robotController?.DrawLineBetweenTargets();

                QM.SelectGameObjectInScene(robotController?.gameObject);
                // if (Selection.objects.Length == 0 || Selection.objects == null)
                // {
                //     QM.SelectGameObjectInScene(robotController?.gameObject);
                // }
                // else if (Selection.objects.Length != 0)
                // {
                //     var selectObj = (Selection.objects[0] != robotController?.gameObject)
                //         ? robotController?.endPoint.gameObject : robotController?.gameObject;
                //     QM.SelectGameObjectInScene(selectObj);
                // }

                uiChange_instruction = false;
            }


            Repaint();
        }


        float WindowWidth; //<! ���ڿ��
        float WindowHeight;//<! ���ڸ߶�
        bool modechange = false;

        private void OnGUI()
        {
            if (HasOpenInstances<RSP_ProgrammingWindow>())//�豸�ⴰ���Ƿ��
            {
                WindowWidth = position.width;//��ȡ���ڿ��
                WindowHeight = position.height;//��ȡ���ڸ߶�

                DrawToolBar();//���ƶ����˵���


                GUILayout.BeginHorizontal();//*


                DrawTargetPanel(TargetPanel.x, TargetPanel.y, TargetPanel.width, TargetPanel.height);//Ŀ���������
                DrawResizer1(AreaResizer1.x, AreaResizer1.y, AreaResizer1.width, AreaResizer1.height);//���Ƶ�����1
                DrawProgramPanel(ProgramPanel.x, ProgramPanel.y, ProgramPanel.width, ProgramPanel.height);//
                DrawResizer2(AreaResizer2.x, AreaResizer2.y, AreaResizer2.width, AreaResizer2.height);//���Ƶ�����2
                DrawPropertyPanel(PropertyPanel.x, PropertyPanel.y, PropertyPanel.width, PropertyPanel.height);//
                GUILayout.EndHorizontal();//*


                SetAreaRectParam();
                ProcessEvents(Event.current);


                if (Application.isPlaying)
                {
                    modechange = true;
                }
                else
                {
                    modechange = false;
                }

                if (!Application.isPlaying && modechange)
                {
                    modechange = false;
                    isPlay = isPlayLoop = false;
                }

            }
        }



        #region ��ʼ��

        string dataPath = "Assets/RDTS/Data/";
        string iconPath = "Assets/RDTS/Private/Resources/Icons/WindowIcon/";
        string backgroundPath = "Assets/RDTS/Private/Resources/Texture/";
        string skinPath = "Assets/RDTS/Scripts/Window/";
        static string targetPrefabPath = "Assets/RDTS/Private/Modules/RobotSimulationAndProgramming/Model/Target-SplinePoint.prefab";

        private GUISkin skin_rsp;
        private GUIStyle labelStyle_Italic;//�Զ��屳��ͼƬ�ı�ǩ��ʽ
        private GUIStyle buttonStyle_target;//����Ŀ���İ�ť��ʽ
        private GUIStyle labelStyle_MiddleCenter;//���ı�ǩUI��ʽ
        private GUIStyle labelStyle_MiddleCenter_Red;//��ɫ���ı�ǩ��ʽ
        private GUIStyle labelStyle_MiddleCenter_Green;//��ɫ���ı�ǩ��ʽ
        private GUIStyle rowStyle;//��һ�е���ʽ
        private Texture2D image_rowBgOdd;//����
        private Texture2D image_rowBgEven;//ż��
        private Texture2D image_rowSelected;//��ѡ��
        private Texture2D icon_play;
        private Texture2D icon_refresh;
        private Texture2D icon_close;
        private Texture2D icon_add;
        private Texture2D icon_delete;
        private Texture2D icon_help;
        private Texture2D icon_target;

        private Texture2D icon_arrow;
        private Texture2D icon_moveJ;
        private Texture2D icon_moveC;
        private Texture2D icon_moveS;
        private Texture2D icon_setSpeed;
        private Texture2D icon_parameter;
        private Texture2D icon_pause;

        private Texture2D icon_lock;
        private Texture2D icon_unlock;
        private Texture2D icon_handleCircle;//�ֱ�ԲȦ
        private Texture2D icon_execute;
        private Texture2D icon_executeLoop;
        private Texture2D icon_iParameter;
        private Texture2D icon_oParameter;

        static GUIContent thisTitleContent;//���ڱ���
        void OnEnable()
        {
            //������Դ��ȡ

            //���ô��ڱ��⼰ͼ��
            Texture titleIcon = AssetDatabase.LoadAssetAtPath<Texture>(iconPath + "Parallel.png");
            thisTitleContent = new GUIContent("Parallel�����˷�������", titleIcon);
            //GUI���
            rowStyle = new GUIStyle();
            rowStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
            //GUIƤ��
            skin_rsp = Resources.Load<GUISkin>("GUISkinRSP");
            labelStyle_Italic = skin_rsp.customStyles[7];
            buttonStyle_target = skin_rsp.customStyles[8];
            labelStyle_MiddleCenter = skin_rsp.customStyles[9];
            labelStyle_MiddleCenter_Red = skin_rsp.customStyles[11];
            labelStyle_MiddleCenter_Green = skin_rsp.customStyles[12];

            //����unity�Դ�ͼ��
            LoadTexture();
            //����Rect��ʼ����
            //SetAreaRectParam();
            //��ʼ����ֵ
            InitializeProgram(this.program);


        }



        private GUIStyle resizerStyle;
        void LoadTexture()//�����ز�
        {

            //ͼ��
            icon_play = EditorGUIUtility.FindTexture("d_PlayButton On");
            icon_refresh = EditorGUIUtility.FindTexture("Refresh");
            icon_add = EditorGUIUtility.FindTexture("d_CreateAddNew");
            icon_close = icon_delete = EditorGUIUtility.FindTexture("d_winbtn_win_close");
            icon_help = EditorGUIUtility.FindTexture("d__Help@2x");
            icon_target = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/RDTS/Private/Modules/RobotSimulationAndProgramming/Resources/Target.png");
            icon_lock = EditorGUIUtility.FindTexture("d_Assemblylock");
            icon_iParameter = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/RDTS/Private/Modules/RobotSimulationAndProgramming/Resources/IParameter.png");
            icon_oParameter = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/RDTS/Private/Modules/RobotSimulationAndProgramming/Resources/OParameter.png");
            icon_moveJ = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/RDTS/Private/Modules/RobotSimulationAndProgramming/Resources/JointMovement.png");
            icon_moveC = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/RDTS/Private/Modules/RobotSimulationAndProgramming/Resources/CurvedMovement.png");
            icon_moveS = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/RDTS/Private/Modules/RobotSimulationAndProgramming/Resources/LinearMovement.png");
            icon_setSpeed = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/RDTS/Private/Modules/RobotSimulationAndProgramming/Resources/Speed.png");
            icon_parameter = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/RDTS/Private/Modules/RobotSimulationAndProgramming/Resources/IOParameter.png");
            icon_pause = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/RDTS/Private/Modules/RobotSimulationAndProgramming/Resources/Pause.png");
            icon_arrow = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/RDTS/Private/Modules/RobotSimulationAndProgramming/Resources/DoubleRightArrow.png");
            icon_handleCircle = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/RDTS/Private/Modules/RobotSimulationAndProgramming/Resources/HandleCircle.png");
            icon_execute = icon_executeLoop = icon_play;

            //Style
            resizerStyle = new GUIStyle();
            resizerStyle.normal.background = EditorGUIUtility.Load("icons/d_AvatarBlendBackground.png") as Texture2D;//Ĭ�ϵ�Unity�༭������(����ͼ��)
            //��ż����ɫ�����ͼƬ
            image_rowBgOdd = EditorGUIUtility.Load("builtin skins/darkskin/images/cn entrybackodd.png") as Texture2D;
            image_rowBgEven = EditorGUIUtility.Load("builtin skins/darkskin/images/cnentrybackeven.png") as Texture2D;
            image_rowSelected = EditorGUIUtility.Load("builtin skins/darkskin/images/menuitemhover.png") as Texture2D;


        }



        #endregion



        #region  �������

        float ToolbarHeight = 20f;
        bool islock = false;
        bool isPlay = false;
        bool isPlayLoop = false;
        /// <summary>���ڲ˵�������</summary>
        void DrawToolBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            //����
            islock = GUILayout.Toggle(islock, icon_lock, EditorStyles.toolbarButton, GUILayout.ExpandWidth(true), GUILayout.Height(ToolbarHeight));
            if (islock)
            {

            }
            //ˢ�°�ť����DataAsset���޸�ʱ�����¶�ȡ���ػ洰��
            if (GUILayout.Button("Associated: " + ((robotController != null) ? robotController.gameObject.name : " ...none "), EditorStyles.toolbarButton, GUILayout.ExpandWidth(true), GUILayout.Height(ToolbarHeight)))
            {
                QM.SelectGameObjectInScene(robotController?.gameObject);

            }
            //���ã��Ƿ�ʵʱˢ�������������
            if (GUILayout.Button("����", EditorStyles.toolbarButton, GUILayout.Width(50), GUILayout.Height(ToolbarHeight)))
            {
                QM.Log("���ô�������");
            }
            //���水ť�����浱ǰ�������ݵ��޸ģ���������asset
            if (GUILayout.Button("����", EditorStyles.toolbarButton, GUILayout.Width(50), GUILayout.Height(ToolbarHeight)))
            {
                QM.Log("����");
            }
            //���øô������ݣ�ȡ��������������������
            if (GUILayout.Button(new GUIContent(" " + "Reset", icon_refresh), EditorStyles.toolbarButton, GUILayout.Width(80), GUILayout.Height(ToolbarHeight)))
            {
                SetRobotController(null);
                //SeparateIOParameter();
                //ClearTargetObjectsList();
                QM.SelectObjectIsNull();
                QM.Log("����");
            }

            if (this.robotController != null)
            {
                icon_execute = (this.robotController.isExecute) ? icon_handleCircle : icon_play;
                icon_executeLoop = (this.robotController.isExecuteLoop) ? icon_handleCircle : icon_play;
            }

            //ָ��һ��ִ�а�ť�����ÿ������е�ִ�б������Ӷ�����ָ���ִ��
            if (GUILayout.Button(new GUIContent(" " + "Execute", icon_execute), EditorStyles.toolbarButton, GUILayout.Width(90), GUILayout.Height(ToolbarHeight)))
            {
                if (this.robotController != null)
                {
                    this.robotController.isExecute = !this.robotController.isExecute;
                }

                //QM.Log("ִ�г���ָ��");
            }
            //ָ��ѭ��ִ�а�ť�����ÿ������е�ִ�б������Ӷ�����ָ���ִ��
            if (GUILayout.Button(new GUIContent(" " + "Execute Loop", icon_executeLoop), EditorStyles.toolbarButton, GUILayout.Width(120), GUILayout.Height(ToolbarHeight)))
            {
                if (this.robotController != null)
                {
                    this.robotController.isExecuteLoop = !this.robotController.isExecuteLoop;
                }

                //QM.Log("ִ�г���ָ��");
            }


            GUILayout.FlexibleSpace();//������

            //������ť����ʾ��β������豸��
            if (GUILayout.Button(icon_help, EditorStyles.toolbarButton, GUILayout.Width(24), GUILayout.Height(ToolbarHeight)))
            {
                ////�򿪵���
                //PopWindowHelp helpWindow = (PopWindowHelp)EditorWindow.GetWindow(
                //       typeof(PopWindowHelp),
                //       true,
                //       "Help");
                //var content = helpWindow.ReadHelpData(1);
                //helpWindow.SetShowContents(content);
                //helpWindow.ShowUtility();
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


        //�Ŀ�����
        private AreaRectParam TargetPanel = new AreaRectParam();
        private AreaRectParam ProgramPanel = new AreaRectParam();
        private AreaRectParam PropertyPanel = new AreaRectParam();
        //����������
        private AreaRectParam AreaResizer1 = new AreaRectParam();//�������
        private AreaRectParam AreaResizer2 = new AreaRectParam();//�������

        //����ϵ��
        private float widthSizeRatio_TargetPanel = 0.3f;
        private float widthSizeRatio_ProgramPanel = 0.6f;
        private float resizerWidth = 10f;//��������Ŀ��
        float Area1Width;//<! �������1���
        float Area2Width;//<! �м�����2���
        float Area3Width;//<! �Ҳ�����3���



        Rect Rect_targetPanel;
        Vector2 _scrollview1;
        /// <summary>������Ŀ������</summary>
        void DrawTargetPanel(float x, float y, float width, float height)
        {
            Rect_targetPanel = new Rect(x, y, width, height);
            GUILayout.BeginArea(Rect_targetPanel);

            GUILayout.Space(2);

            TargetPanelContent();

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Number of targets��{targets.Count}", labelStyle_Italic);//Ŀ�������
            GUILayout.Space(10);
            GUILayout.Label($"CurrentTarget��{currentTarget?.name}", labelStyle_Italic);//��ǰѡ�е�Ŀ���
            GUILayout.EndHorizontal();
            GUILayout.Space(2);

            GUILayout.EndArea();

        }


        Rect Rect_programPanel;
        Vector2 _scrollview2;
        /// <summary>�����˳������</summary>
        void DrawProgramPanel(float x, float y, float width, float height)
        {
            Rect_programPanel = new Rect(x, y, width, height);
            GUILayout.BeginArea(Rect_programPanel);
            //_scrollview2 = GUILayout.BeginScrollView(_scrollview2);//<! ������

            GUILayout.Space(2);

            ProgramPanelContent();

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Number of instructions��{instructions.Count}", labelStyle_Italic);//Ŀ�������
            GUILayout.Space(10);
            GUILayout.Label($"CurrentInstruction��{currentInstructionIndex + 1}", labelStyle_Italic);//��ǰѡ�е�Ŀ���
            GUILayout.EndHorizontal();
            GUILayout.Space(2);

            //GUILayout.EndScrollView();//<! ������
            GUILayout.EndArea();

        }

        Rect Rect_propertyPanel;
        Vector2 _scrollview3;
        Vector2 _scrollview4;
        /// <summary>�������������</summary>
        void DrawPropertyPanel(float x, float y, float width, float height)
        {
            Rect_propertyPanel = new Rect(x, y, width, height);
            GUILayout.BeginArea(Rect_propertyPanel);
            //_scrollview3 = GUILayout.BeginScrollView(_scrollview3);//<! ������

            GUILayout.Space(2);

            ParameterPanelContent();

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Number of parameters��{ioParameters.Count}", labelStyle_Italic);//��������
            GUILayout.Space(10);
            GUILayout.Label($"CurrentParameter��{currentParameter?.name}", labelStyle_Italic);//��ǰѡ�еĲ���
            GUILayout.EndHorizontal();
            GUILayout.Space(2);


            //GUILayout.EndScrollView();//<! ������
            GUILayout.EndArea();

        }


        private Rect Rect_resizer1;
        /// <summary>
        /// ��ȵ�����1
        /// </summary>
        void DrawResizer1(float x, float y, float width, float height)
        {
            Rect_resizer1 = new Rect(x, y, width, height);
            GUILayout.BeginArea(new Rect(Rect_resizer1.position + (Vector2.right * resizerWidth), new Vector2(4, height * 3)), resizerStyle);
            GUILayout.EndArea();

            EditorGUIUtility.AddCursorRect(Rect_resizer1, MouseCursor.ResizeHorizontal);//�ı������״����ʾ���ָ��λ�ڵ�����Сλ��
        }



        private Rect Rect_resizer2;
        /// <summary>
        /// ��ȵ�����3
        /// </summary>
        void DrawResizer2(float x, float y, float width, float height)
        {
            Rect_resizer2 = new Rect(x, y, width, height);
            GUILayout.BeginArea(new Rect(Rect_resizer2.position + (Vector2.right * resizerWidth), new Vector2(4, height * 3)), resizerStyle);
            GUILayout.EndArea();

            EditorGUIUtility.AddCursorRect(Rect_resizer2, MouseCursor.ResizeHorizontal);//�ı������״����ʾ���ָ��λ�ڵ�����Сλ��
        }


        /* UIԪ�ػ��� */

        #region Ŀ���������

        float Height_operation = 25f;
        string[] Operation_TargetPanel = new string[3] { "New target", "Delete all", "Refresh" };
        bool foldout_Target = true;//�۵���ͷ��ǩ���۵�״̬��true��չ��  false���۵���
        int countOfButton_row = 2;




        //private bool _isSelectNull = false;

        /// <summary>
        /// Ŀ����������
        /// </summary>
        void TargetPanelContent()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            //��Ŀ��㡱������ť
            if (GUILayout.Button(new GUIContent(" " + Operation_TargetPanel[0], icon_add), GUILayout.Width(Area1Width / countOfButton_row - 5), GUILayout.Height(Height_operation)))
            {
                //��յ�ǰ��ѡ�ж���
                //QM.SelectObjectIsNull();
                //��"������Ŀ���"����
                RSP_TargetCreationWindow addWindow = (RSP_TargetCreationWindow)EditorWindow.GetWindowWithRect(
                       typeof(RSP_TargetCreationWindow),
                       new Rect(Screen.width / 2, Screen.height / 2, 400, 200),
                       false,
                       "Add target");
                addWindow.LinkProgrammingWindow(this);
                addWindow.ShowUtility();
            }
            if (GUILayout.Button(new GUIContent(" " + Operation_TargetPanel[1], icon_delete), GUILayout.Width(Area1Width / countOfButton_row - 5), GUILayout.Height(Height_operation)))
            {
                if (EditorUtility.DisplayDialog("���Ŀ���", "�Ƿ�ɾ�����е�Ŀ��㣿", "yes", "no"))
                {
                    //���asset�ļ��е�����
                    this.program?.ClearAllTargets();
                    //���RSP_RobotController�ű��е�Ŀ������
                    this.robotController?.ClearAllTargetVariables();
                }

            }

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            //_scrollview1 = GUILayout.BeginScrollView(_scrollview1);//<! ������
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            DrawTargetListItems();
            GUILayout.EndHorizontal();
            //GUILayout.EndScrollView();//<! ������



        }


        float Height_target = 30f;//UIԪ����ռ�߶ȣ�����ֵ��
        TargetOperationMenu _targetOperationMenu = TargetOperationMenu.None;//���ڵ���Ŀ��Ĳ����˵�ö��
        ReorderableList reorderableList_target;//�б���ʾ


        void DrawTargetListItems()
        {
            if (program != null && isProgramInit)
            {
                isProgramInit = false;

                List<TargetObject> targets = this.targets;
                reorderableList_target = new ReorderableList(targets, typeof(TargetObject), true, true, false, false);
                reorderableList_target.elementHeight = Height_target;
                reorderableList_target.drawElementCallback =
                (rect, index, isActive, isFocused) =>
                {
                    var element = targets[index];
                    rect.x += 2;
                    rect.y += 2;
                    rect.width = TargetPanel.width - 20;
                    rect.height -= 3;
                    if (index == reorderableList_target.index)
                    {
                        currentTarget = element;//<! ���õ�ǰѡ�е�Target
                        GUI.backgroundColor = new Color(0.70f, 0.78f, 0.91f, 0.95f);
                    }
                    else
                    {
                        GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);
                    }
                    EditorGUI.HelpBox(rect, "", MessageType.None);
                    EditorGUI.LabelField(new Rect(rect.x + 8, rect.y, rect.width, rect.height), new GUIContent("  " + element.name, icon_target));

                    Event e = Event.current;
                    if (rect.Contains(e.mousePosition) && index == reorderableList_target.index)
                    {

                        if (e.button == 1)//<! ע��ʹ��e.type������Event.current.type
                        {
                            //���ư�ť�������˵�
                            GenericMenu targetMenu = new GenericMenu();
                            targetMenu.AddItem(new GUIContent("ѡ��Ŀ������"), _targetOperationMenu.Equals(TargetOperationMenu.Select), OnValueSelectTargetCallback, TargetOperationMenu.Select);
                            targetMenu.AddItem(new GUIContent("ʾ��Ŀ���λ��"), _targetOperationMenu.Equals(TargetOperationMenu.Tesch), OnValueTeachTargetCallback, TargetOperationMenu.Tesch);
                            targetMenu.AddItem(new GUIContent("�༭��Ŀ���"), _targetOperationMenu.Equals(TargetOperationMenu.Edit), OnValueEditTargetCallback, TargetOperationMenu.Edit);
                            targetMenu.AddSeparator("");
                            targetMenu.AddItem(new GUIContent("ɾ����Ŀ���"), _targetOperationMenu.Equals(TargetOperationMenu.Delete), OnValueDeleteTargetCallback, TargetOperationMenu.Delete);
                            targetMenu.ShowAsContext();

                        }



                    }



                    GUI.backgroundColor = Color.white;

                };

                var defaultColor = GUI.backgroundColor;
                reorderableList_target.drawElementBackgroundCallback = (rect, index, isActive, isFocused) =>
                {
                    GUI.backgroundColor = Color.white;
                };
                reorderableList_target.drawHeaderCallback = (rect) =>
                    EditorGUI.LabelField(rect, "Robot Targets");
            }


            if (reorderableList_target != null)
            {
                _scrollview1 = GUILayout.BeginScrollView(_scrollview1);//<! ������
                reorderableList_target.DoLayoutList();
                EditorGUILayout.EndScrollView();
            }





        }


        #endregion


        #region  ����ָ��������

        float Height_operation2 = 25f;
        string[] Operation_ProgramPanel = new string[2] { "New instruction", "Delete all" };
        int countOfButton_row2 = 2;
        bool uiChange_instruction = false;//ָ���б��UI�����ı�
        void ProgramPanelContent()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            //��Ŀ��㡱������ť
            if (GUILayout.Button(new GUIContent(" " + Operation_ProgramPanel[0], icon_add), GUILayout.Width(Area2Width / countOfButton_row2 - 5), GUILayout.Height(Height_operation2)))
            {
                //��յ�ǰ��ѡ�ж���
                //QM.SelectObjectIsNull();
                //��"������Ŀ���"����
                RSP_InstructionCreationWindow addWindow = (RSP_InstructionCreationWindow)EditorWindow.GetWindowWithRect(
                       typeof(RSP_InstructionCreationWindow),
                       new Rect(Screen.width / 2, Screen.height / 2, 400, 200),
                       false,
                       "Add instruction");
                addWindow.LinkProgrammingWindow(this);
                addWindow.ShowUtility();
            }
            if (GUILayout.Button(new GUIContent(" " + Operation_ProgramPanel[1], icon_delete), GUILayout.Width(Area2Width / countOfButton_row2 - 5), GUILayout.Height(Height_operation2)))
            {
                if (EditorUtility.DisplayDialog("��ճ���ָ��", "�Ƿ�ɾ�����е�ָ�", "yes", "no"))
                {
                    //���asset�ļ��е�ָ������
                    this.program?.ClearAllInstructions();
                }

            }

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            //_scrollview1 = GUILayout.BeginScrollView(_scrollview1);//<! ������
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);

            EditorGUI.BeginChangeCheck();
            DrawInstructionListItems();

            if (EditorGUI.EndChangeCheck())
            {
                //���ƹ켣
                robotController?.DrawLineBetweenTargets();

                QM.SelectObjectIsNull();

                ///QM.Log("UI change!");
                uiChange_instruction = true;
            }



            GUILayout.EndHorizontal();
            //GUILayout.EndScrollView();//<! ������



        }


        float Height_instruction = 30f;//UIԪ����ռ�߶ȣ�����ֵ��
        InstructionOperationMenu _instructionOperationMenu = InstructionOperationMenu.None;
        ReorderableList reorderableList_instruction;//�б���ʾ
        void DrawInstructionListItems()
        {
            if (program != null && isInstructionInit)
            {
                isInstructionInit = false;

                List<BaseInstruction> instructions = new List<BaseInstruction>();
                instructions = this.instructions;
                // instructions.Add(new BaseInstruction() { icon = icon_moveJ, name = "MoveJ" });
                // instructions.Add(new BaseInstruction() { icon = icon_moveC, name = "MoveC" });
                // instructions.Add(new BaseInstruction() { icon = icon_moveS, name = "MoveS" });
                // instructions.Add(new BaseInstruction() { icon = icon_parameter, name = "SetPara" });
                // instructions.Add(new BaseInstruction() { icon = icon_parameter, name = "WaitPara" });
                // instructions.Add(new BaseInstruction() { icon = icon_setSpeed, name = "SetSpeed" });
                // instructions.Add(new BaseInstruction() { icon = icon_pause, name = "Pause" });


                reorderableList_instruction = new ReorderableList(instructions, typeof(BaseInstruction), true, true, false, false);
                reorderableList_instruction.elementHeight = Height_instruction;
                reorderableList_instruction.drawElementCallback =
                (rect, index, isActive, isFocused) =>
                {
                    var element = instructions[index];
                    rect.x += 2;
                    rect.y += 2;
                    rect.width = ProgramPanel.width - 20;
                    rect.height -= 3;
                    if (index == reorderableList_instruction.index)
                    {
                        currentInstruction = element;//<! ���õ�ǰѡ�е�ָ��
                        currentInstructionIndex = index;//<! ��ȡ��ǰѡ�е�ָ�������
                        GUI.backgroundColor = new Color(0.70f, 0.78f, 0.91f, 0.95f);
                    }
                    else
                    {
                        GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);
                    }

                    //var type = element.GetType();
                    string showInfo = "  " + element.name + " : ";
                    var atrribute = element.GetType().GetCustomAttributes<InstructionTypeAttribute>().ToArray();
                    if (atrribute.Length > 0)
                    {
                        switch (atrribute[0].instruction)
                        {
                            case Instruction.MoveJoint:
                                showInfo += (element as RSP_MoveJoint).inTarget;
                                break;

                            case Instruction.MoveCurve:
                                showInfo += (element as RSP_MoveCurve).inTarget;
                                break;

                            case Instruction.MoveStraightLine:
                                showInfo += (element as RSP_MoveStraightLine).inTarget;
                                break;

                            case Instruction.SetParameter:
                                //element.icon = icon_parameter;
                                //var inputAtrribute = element.GetType().GetCustomAttributes<InstructionInputAttribute>().ToArray();
                                showInfo += (element as RSP_SetParameter).inParameter + " = "
                                            + (element as RSP_SetParameter).inParaValue.ToString();
                                break;

                            case Instruction.WaitParameter:
                                showInfo += (element as RSP_WaitParameter).inParameter + " = "
                                            + (element as RSP_WaitParameter).inParaValue.ToString();
                                break;

                            case Instruction.SetSpeed:
                                showInfo += (element as RSP_SetSpeed).inValue.ToString();
                                break;

                            case Instruction.PauseTime:
                                showInfo += (element as RSP_PauseTime).inValue.ToString();
                                break;
                        }
                    }

                    EditorGUI.HelpBox(rect, "", MessageType.None);
                    if (robotController != null)
                    {
                        if (index == robotController.whichInstruction - 1 && robotController.isExecute)
                            EditorGUI.LabelField(rect, new GUIContent(" ", icon_arrow));
                    }

                    //EditorGUI.LabelField(rect, new GUIContent(showInfo, element.icon));
                    EditorGUI.LabelField(new Rect(rect.x + 32, rect.y, rect.width, rect.height), new GUIContent(showInfo, element.icon));

                    Event e = Event.current;
                    if (rect.Contains(e.mousePosition) && index == reorderableList_instruction.index)
                    {

                        if (e.button == 1)//<! ע��ʹ��e.type������Event.current.type
                        {
                            //���ư�ť�������˵�
                            GenericMenu targetMenu = new GenericMenu();
                            targetMenu.AddItem(new GUIContent("ִ�и�ָ��"), _instructionOperationMenu.Equals(InstructionOperationMenu.Process), OnValueProcessInstructionCallback, atrribute[0].instruction);
                            targetMenu.AddItem(new GUIContent("�༭��ָ��"), _instructionOperationMenu.Equals(InstructionOperationMenu.Edit), OnValueEditInstructionCallback, atrribute[0].instruction);
                            targetMenu.AddSeparator("");
                            targetMenu.AddItem(new GUIContent("ɾ����ָ��"), _instructionOperationMenu.Equals(InstructionOperationMenu.Delete), OnValueDeleteInstructionCallback, atrribute[0].instruction);
                            targetMenu.ShowAsContext();

                        }



                    }



                    GUI.backgroundColor = Color.white;

                };

                var defaultColor = GUI.backgroundColor;
                reorderableList_instruction.drawElementBackgroundCallback = (rect, index, isActive, isFocused) =>
                {
                    GUI.backgroundColor = Color.white;
                };
                reorderableList_instruction.drawHeaderCallback = (rect) =>
                    EditorGUI.LabelField(rect, "Program Instructions");
            }


            if (reorderableList_instruction != null)
            {
                _scrollview2 = GUILayout.BeginScrollView(_scrollview2);//<! ������
                reorderableList_instruction.DoLayoutList();
                EditorGUILayout.EndScrollView();
            }





        }


        static bool IsInstruction(Type instructionType)
        {
            //��type�ǳ���ľͷ���false
            if (instructionType.IsAbstract)//���Type�ǳ����(����ʵ����,��ֻ������������Ļ���)����Ϊ true������Ϊ false
                return false;
            //���ǳ���ģ��ͼ���NodeMenuItemAttribute�Զ������Եĸ����������ھͷ���true
            return instructionType.GetCustomAttributes<InstructionTypeAttribute>().Count() > 0;
        }


        #endregion



        #region ����������


        float Height_operation3 = 25f;
        string[] Operation_ParameterPanel = new string[3] { "New Parameter", "Refresh", "Delete all" };
        string[] tableTitle_Parameter = new string[3] { "IO", "����", "ֵ" };
        bool foldout_Parameter = true;//�۵���ͷ��ǩ���۵�״̬��true��չ��  false���۵���
        int countOfButton_row3 = 3;
        float height_ParameterTitle = 20f;
        int selectedRow_Parameter = -1;//�������б�ѡ����һ��
        bool isShow_inputParameter = false;
        bool isShow_outputParameter = false;
        ParameterOperationMenu _parameterOperationMenu = ParameterOperationMenu.None;


        void ParameterPanelContent()
        {

            /* ������Ĳ�����ť���� */
            GUILayout.BeginHorizontal();

            GUILayout.Space(8);

            //��Ŀ��㡱������ť
            if (GUILayout.Button(new GUIContent(" " + Operation_ParameterPanel[0], icon_add), GUILayout.Width(Area3Width / countOfButton_row3 - 10), GUILayout.Height(Height_operation3)))
            {
                //��յ�ǰ��ѡ�ж���
                //QM.SelectObjectIsNull();
                //��"������Ŀ���"����
                RSP_ParameterCreationWindow addWindow = (RSP_ParameterCreationWindow)EditorWindow.GetWindowWithRect(
                       typeof(RSP_ParameterCreationWindow),
                       new Rect(Screen.width / 2, Screen.height / 2, 400, 200),
                       false,
                       "Add parameter");
                addWindow.LinkProgrammingWindow(this);
                addWindow.ShowUtility();
            }
            if (GUILayout.Button(new GUIContent(" " + Operation_ParameterPanel[1], icon_refresh), GUILayout.Width(Area3Width / countOfButton_row3 - 10), GUILayout.Height(Height_operation3)))
            {
                currentParameter = null;
                selectedRow_Parameter = -1;
            }
            if (GUILayout.Button(new GUIContent(" " + Operation_ParameterPanel[2], icon_delete), GUILayout.Width(Area3Width / countOfButton_row3 - 10), GUILayout.Height(Height_operation3)))
            {
                if (EditorUtility.DisplayDialog("��ղ���", "�Ƿ�ɾ�����еĲ�����", "yes", "no"))
                {
                    //���asset�ļ��еĲ�������
                    this.program?.ClearAllIOParameters();
                    //���RSP_RobotController�ű��еĲ�������
                    this.robotController?.ClearAllParameterVariables();
                }

            }

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            /* ������ı��������� */
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.Label(tableTitle_Parameter[0], EditorStyles.toolbarButton, GUILayout.Width(Area3Width * 1.5f / 10));
            GUILayout.Label(tableTitle_Parameter[1], EditorStyles.toolbarButton, GUILayout.Width(Area3Width * 4 / 10));
            GUILayout.Label(tableTitle_Parameter[2], EditorStyles.toolbarButton, GUILayout.Width(Area3Width * 4 / 10));
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
            GUILayout.Space(2);

            /* ���Ʋ����б� */
            _scrollview3 = GUILayout.BeginScrollView(_scrollview3);//<! ������
                                                                   ////List_interfaceObjs.ForEach(obj => OneInterfaceObjFormat(obj));//��*��������䡿��Ϊfoeeach�в�Ҫʹ��remove����add����
            for (int i = 0; i < ioParameters.Count; i++)
                OneParameterFormat(ioParameters[i], i);


            GUILayout.EndScrollView();//<! ������



        }


        void OneParameterFormat(IOParameter iOParameter, int index)
        {
            if (iOParameter == null)
                return;

            //<! ������ż�б�����ɫ
            if (selectedRow_Parameter != index)
                rowStyle.normal.background = (index % 2 == 0) ? image_rowBgEven : image_rowBgOdd;
            else//<! ���˽ӿڶ���ѡ�У����޸ı�����ɫ
                rowStyle.normal.background = image_rowSelected;

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginHorizontal(rowStyle);

            //<! io
            if (GUILayout.Button((iOParameter.type == ParameterType.Input) ? icon_iParameter : icon_oParameter,
                labelStyle_MiddleCenter,
                GUILayout.Width(Area3Width * 1.5f / 10 - 10),
                GUILayout.Height(20)))
            {
                ParameterButtonDeal(iOParameter, index);
            }
            //<! ����
            GUILayout.Space(5);
            if (GUILayout.Button(iOParameter.name, labelStyle_MiddleCenter, GUILayout.Width(Area3Width * 4 / 10 - 12), GUILayout.Height(20)))
            {
                ParameterButtonDeal(iOParameter, index);
            }
            //<! ֵ
            GUILayout.Space(9);
            if (GUILayout.Button(iOParameter.value.ToString(),
                (iOParameter.type == ParameterType.Input) ? labelStyle_MiddleCenter_Red : labelStyle_MiddleCenter_Green,
                GUILayout.Width(Area3Width * 4 / 10 - 12),
                GUILayout.Height(20)
            ))
            {
                ParameterButtonDeal(iOParameter, index);
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            GUILayout.EndHorizontal();
        }

        void ParameterButtonDeal(IOParameter iOParameter, int index)
        {
            currentParameter = iOParameter;
            selectedRow_Parameter = index;

            Event e = Event.current;
            if (e.button == 1)
            {
                //���ư�ť�������˵�
                GenericMenu parameterMenu = new GenericMenu();
                parameterMenu.AddItem(new GUIContent("ѡ�в�������"), _parameterOperationMenu.Equals(ParameterOperationMenu.Select), OnValueSelectParameterCallback, ParameterOperationMenu.Select);
                parameterMenu.AddItem(new GUIContent("�༭�ò���"), _parameterOperationMenu.Equals(ParameterOperationMenu.Edit), OnValueEditParameterCallback, ParameterOperationMenu.Edit);
                parameterMenu.AddSeparator("");
                parameterMenu.AddItem(new GUIContent("ɾ���ò���"), _parameterOperationMenu.Equals(ParameterOperationMenu.Delete), OnValueDeleteParameterCallback, ParameterOperationMenu.Delete);
                parameterMenu.ShowAsContext();


            }


            Repaint();
        }


        #endregion



        /// <summary>
        /// ���������Rect����
        /// </summary>
        void SetAreaRectParam()
        {
            //TargetPanel
            TargetPanel.x = 0;
            TargetPanel.y = 0 + ToolbarHeight;
            TargetPanel.width = WindowWidth * widthSizeRatio_TargetPanel;
            TargetPanel.height = WindowHeight - ToolbarHeight;

            //AreaResizer1
            AreaResizer1.x = TargetPanel.width;
            AreaResizer1.y = 0 + ToolbarHeight;
            AreaResizer1.width = resizerWidth;
            AreaResizer1.height = WindowHeight - ToolbarHeight;

            //ProgramPanel
            ProgramPanel.x = AreaResizer1.x + AreaResizer1.width;
            ProgramPanel.y = 0 + ToolbarHeight;
            ProgramPanel.width = WindowWidth * widthSizeRatio_ProgramPanel - TargetPanel.width - AreaResizer1.width;
            ProgramPanel.height = WindowHeight - ToolbarHeight;

            //AreaResizer2
            AreaResizer2.x = ProgramPanel.x + ProgramPanel.width;
            AreaResizer2.y = 0 + ToolbarHeight;
            AreaResizer2.width = resizerWidth;
            AreaResizer2.height = WindowHeight - ToolbarHeight;

            //PropertyPanel
            PropertyPanel.x = AreaResizer2.x + AreaResizer2.width;
            PropertyPanel.y = 0 + ToolbarHeight;
            PropertyPanel.width = WindowWidth - TargetPanel.width - AreaResizer1.width - ProgramPanel.width - AreaResizer2.width;
            PropertyPanel.height = WindowHeight - ToolbarHeight;

            Area1Width = TargetPanel.width;
            Area2Width = ProgramPanel.width;
            Area3Width = PropertyPanel.width;


        }



        #endregion



        #region Methods

        #region ��������ķ���

        private bool isResizing;//�Ƿ��ڵ�������ߴ�
        private bool isResizingWidth1;//�Ƿ��ڵ�����һ��������
        private bool isResizingHeight2;//�Ƿ��ڵ����ڶ���������
        private bool isResizingWidth3;//�Ƿ��ڵ���������������
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

                        isResizingWidth1 = (Rect_resizer1.Contains(e.mousePosition)) ? true : false;//�ж��Ƿ��ڵ������

                        isResizingHeight2 = (Rect_resizer2.Contains(e.mousePosition)) ? true : false;//�ж��Ƿ��ڵ����߶�


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
                if (isResizingWidth1 && e.mousePosition.x > WindowWidth * 0.1 && e.mousePosition.x < WindowWidth * 0.4)
                {
                    widthSizeRatio_TargetPanel = e.mousePosition.x / WindowWidth;
                }
                //������2
                if (isResizingHeight2 && e.mousePosition.x > WindowWidth * 0.5 && e.mousePosition.x < WindowWidth * 0.8)
                {
                    widthSizeRatio_ProgramPanel = e.mousePosition.x / WindowWidth;
                }


                // QM.Log("widthSizeRatio��" + widthSizeRatio + " " + "heightSizeRatio: " + heightSizeRatio);

                Repaint();
            }
        }

        #endregion


        /// <summary>
        /// �����б��е�����Ԫ��
        /// </summary>
        /// <param name="list"></param>
        /// <param name="index1"></param>
        /// <param name="index2"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static void Swap<T>(List<T> list, int index1, int index2)
        {
            if (list == null)
                return;

            if (list.Count < index1 + 1)
                return;

            if (list.Count < index2 + 1)
                return;

            if (index1 < 0 || index2 < 0)
                return;


            T cache = list[index1];
            list[index1] = list[index2];
            list[index2] = cache;

        }


        /// <summary>
        /// �ӹ����Ļ����˿������ű��л�ȡĿ�����Ϣ
        /// </summary>
        // public void GetTargtsFromRobotController()
        // {
        //     if (robotController == null)
        //         return;

        //     robotController.UpdateProgram();

        // }


        // public void GetTargtsFromProgramAsset(RSP_RobotProgram program)
        // {
        //     if (program == null)
        //         return;

        //     var list = program.GetTargetList();
        //     List<TargetObject> newTargetObjs = new List<TargetObject>();

        //     list.ForEach(tar =>
        //     {
        //         TargetObject newTargetObj = new TargetObject();
        //         newTargetObj.name = tar.name;
        //         // newTargetObj.Guid = tar.Guid;
        //         // newTargetObj.targetObj = null;
        //         // newTargetObj.targetScript = null;

        //         newTargetObjs.Add(newTargetObj);

        //     });


        //     targets = newTargetObjs;

        // }



        // /// <summary>
        // /// ���Ŀ����б�
        // /// </summary>
        // public void ClearTargetObjectsList()
        // {
        //     targetObjects?.Clear();
        // }


        /// <summary>
        /// ���ù����Ļ����˿�����
        /// </summary>
        /// <param name="controller"></param> <summary>
        public void SetRobotController(RSP_RobotController controller)
        {
            robotController = controller;
        }


        #region Ŀ��㴦��ķ���

        /// <summary>
        /// ����һ���µ�Ŀ���
        /// </summary>
        /// <param name="name">����</param>
        /// <param name="gameObject">�Ƿ��ṩ�����е�Ŀ������</param>
        /// <param name="robotController">�����Ļ����˿��������</param>
        /// <param name="transform">����ʱ��λ��</param>
        /// <param name="isSelected">�Ƿ񴴽���ѡ��</param>
        /// <returns></returns>
        public TargetVariable CreateNewTarget(string name, RSP_Target script, RSP_RobotController robotController, Transform transform, bool isSelected = false)
        {
            if (name == null || name == "")
                name = "new target";

            //���������ظ�
            var names = program.GetTargetNameList();
            while (names.Contains(name))
            {
                name += "(1)";
            }

            TargetVariable newTargetVariable = new TargetVariable();
            newTargetVariable.targetName = name;
            //��δ����������Ŀ���gameobject
            if (script == null)
            {
                GameObject newTargetObj = QM.AddComponentByPath(targetPrefabPath);
                newTargetObj.name = name;
                newTargetVariable.targetScript = newTargetObj.GetComponent<RSP_Target>();
            }
            else
            {
                newTargetVariable.targetScript = script;
            }

            TargetObject newTarget = new TargetObject();
            newTarget.name = name;
            /* ��Ŀ���洢��asset�������������Ӧ�б��� */
            program.AddTarget(newTarget);
            if (robotController != null)
            {
                robotController.AddElement2TargetVariableList(newTargetVariable);
            }



            if (transform != null)
            {
                newTargetVariable.targetScript.transform.position = transform.position;
                newTargetVariable.targetScript.transform.rotation = transform.rotation;
            }
            else
            {
                newTargetVariable.targetScript.transform.position = new Vector3(0, 0, 0);
                newTargetVariable.targetScript.transform.rotation = Quaternion.Euler(0, 0, 0);
            }


            if (isSelected)
                QM.SelectGameObjectInScene(newTargetVariable.targetScript.gameObject);


            return newTargetVariable;

        }

        /// <summary>
        /// �༭������Ŀ���
        /// </summary>
        /// <param name="target">Ҫ���༭��Ŀ���</param>
        /// <param name="name">�µ�����</param>
        /// <param name="newRobotController">�µĹ����Ŀ��������</param>
        /// <param name="oldRobotController">�ɵĹ����Ŀ��������</param>
        /// <returns></returns>
        public TargetObject EditTarget(TargetObject target, string name, RSP_Target script)
        {
            if (target == null)
                return null;

            if (name == null || name == "")
                name = "new target";

            //������������ԭ���Ʋ�ͬ������Ҫȷ���µ����Ʋ��ظ�������������δ�ı䣬�����Ʋ���Ҫ�޸�
            if (target.name != name)
            {
                //���������ظ�
                var names = program.GetTargetNameList();
                while (names.Contains(name))
                {
                    name += "(1)";
                }
            }

            if (robotController != null)
            {
                //��ȡҪ�༭��Ŀ������
                TargetVariable targetVariable = robotController.GetTargetVariableByName(target.name);
                //�޸�Ŀ�������
                target.name = name;
                targetVariable.targetName = name;

                if (script != null)
                {

                    robotController.SetTargetScriptByName(target.name, script, true);
                    //QM.SelectGameObjectInScene(target.targetScript.gameObject);
                }
            }

            target.targetScript = script;


            return target;

        }


        /// <summary>
        /// ɾ��������Ŀ���
        /// </summary>
        /// <param name="target"></param>
        public void DeleteTarget(TargetObject target)
        {
            if (target == null)
                return;

            if (currentTarget == target)
                currentTarget = null;


            robotController?.RemoveElementInTargetVariableListByName(target.name);//�����Ŀ��������Ƴ�
            program.RemoveTarget(target);//��asset�ļ����Ƴ�
            Repaint();

            // Undo.DestroyObjectImmediate(target.targetObj);//�������ٶ��������*Ҫ�����������ǰ��
            // DestroyImmediate(target.targetObj);


        }


        /* Ŀ�������˵�����Ӧ�Ļص����� */

        void OnValueSelectTargetCallback(object value)
        {
            if (robotController == null)
                return;

            var script = robotController.GetTargetScriptByName(currentTarget.name);
            if (script != null)
            {
                GameObject selectObj = script.gameObject;
                QM.SelectGameObjectInScene(selectObj);

                QM.Log($"ѡ��Ŀ��㣺{currentTarget?.name}");
            }


        }

        void OnValueEditTargetCallback(object value)
        {
            //��"�༭��Ŀ���"����
            RSP_TargetEditingWindow editWindow = (RSP_TargetEditingWindow)EditorWindow.GetWindowWithRect(
                    typeof(RSP_TargetEditingWindow),
                    new Rect(Screen.width / 2, Screen.height / 2, 400, 150),
                    false,
                    "Edit target");

            currentTarget.targetScript = robotController?.GetTargetScriptByName(currentTarget.name);
            editWindow.LinkProgrammingWindow(this, currentTarget);
            editWindow.ShowUtility();
            QM.Log($"�༭Ŀ��㣺{currentTarget?.name}");
        }

        void OnValueTeachTargetCallback(object value)
        {
            if (robotController == null)
            {
                EditorUtility.DisplayDialog("ʾ�̾���", "δ��������Ч�Ŀ����������", "yes");
                return;
            }


            var script = robotController.GetTargetScriptByName(currentTarget.name);
            if (script != null && robotController.endPoint != null)
            {
                script.gameObject.transform.position = robotController.endPoint.transform.position;
                script.gameObject.transform.rotation = robotController.endPoint.transform.rotation;

                GameObject selectObj = script.gameObject;
                QM.SelectGameObjectInScene(selectObj);

                QM.Log($"ʾ��Ŀ��㣺{currentTarget?.name}");
            }

            if (robotController.endPoint == null)
            {
                EditorUtility.DisplayDialog("ʾ�̾���", "�����Ŀ��������δ������Ч��EndPoint��", "yes");
            }

        }

        void OnValueCopyTargetCallback(object value)
        {
            QM.Log($"����Ŀ��㣺{currentTarget?.name}");
        }

        void OnValueDeleteTargetCallback(object value)
        {
            DeleteTarget(currentTarget);
            QM.Log($"ɾ��Ŀ��㣺{currentTarget?.name}");
        }

        #endregion


        #region ��������ķ���



        /// <summary>
        /// �ڲ����б������һ������
        /// </summary>
        /// <param name="iOParameter"></param>
        void AddOneElementInParameterList(IOParameter iOParameter)
        {
            if (iOParameter == null)
                return;

            SeparateIOParameter();

            if (iOParameter.type == ParameterType.Input && !inParameters.Contains(iOParameter))
                inParameters.Add(iOParameter);

            if (iOParameter.type == ParameterType.Output && !outParameters.Contains(iOParameter))
                outParameters.Add(iOParameter);

        }


        /// <summary>
        /// �ڲ����б����Ƴ�һ������
        /// </summary>
        /// <param name="iOParameter"></param>
        void RemoveOneElementInParameterList(IOParameter iOParameter)
        {
            if (iOParameter == null)
                return;

            SeparateIOParameter();

            if (iOParameter.type == ParameterType.Input && inParameters.Contains(iOParameter))
                inParameters.Remove(iOParameter);

            if (iOParameter.type == ParameterType.Output && outParameters.Contains(iOParameter))
                outParameters.Remove(iOParameter);

        }

        /// <summary>
        /// ���input��output���͵Ĳ����б�
        /// </summary>
        void AssembleIOParameter()
        {
            if (inParameters == null || outParameters == null || ioParameters == null)
                return;

            ioParameters = inParameters;
            ioParameters.AddRange(outParameters);
        }

        /// <summary>
        /// ����input��output���͵Ĳ����б�
        /// </summary>
        void SeparateIOParameter()
        {
            if (inParameters == null || outParameters == null || ioParameters == null)
                return;

            inParameters.Clear();
            outParameters.Clear();

            ioParameters.ForEach(ioPara =>
            {
                if (ioPara.type == ParameterType.Input)
                {
                    inParameters.Add(ioPara);
                }
                else
                {
                    outParameters.Add(ioPara);
                }
            });

            // ioParameters = inParameters;
            // ioParameters.AddRange(outParameters);
        }

        public IOParameterVariable CreateNewParameter(string name, RSP_RobotController robotController, ParameterType parameterType, bool isSelected = false)
        {
            if (name == null || name == "")
                name = "new parameter";

            //���������ظ�
            var names = program.GetIOParameterNameList();
            while (names.Contains(name))
            {
                name += "(1)";
            }

            IOParameterVariable newParameterVariable = new IOParameterVariable();
            newParameterVariable.variableName = name;

            //����һ���µ�ֵ����
            GameObject newParameterObj = QM.CreateOneValueObj(null, name, null,
                VALUETYPE.INT,
                (parameterType == ParameterType.Input) ? VALUEDIRECTION.INPUT : VALUEDIRECTION.OUTPUT
            );
            newParameterVariable.variableObj = newParameterObj.GetComponent<Value>();
            newParameterObj.name = name;


            IOParameter newParameter = new IOParameter();
            newParameter.name = name;
            newParameter.type = parameterType;
            /* ��Ŀ���洢��asset�������������Ӧ�б��� */
            program.AddIOParameter(newParameter);
            //AddOneElementInParameterList(newParameter, true);

            if (robotController != null)
            {
                if (parameterType == ParameterType.Input)
                {
                    InParameterVariable newInParaVariable = new InParameterVariable();
                    newInParaVariable.variableName = newParameterVariable.variableName;
                    newInParaVariable.variableObj = newParameterVariable.variableObj as ValueInputInt;
                    robotController.AddElement2InParameterVariableList(newInParaVariable);
                }
                else
                {
                    OutParameterVariable newOutParaVariable = new OutParameterVariable();
                    newOutParaVariable.variableName = newParameterVariable.variableName;
                    newOutParaVariable.variableObj = newParameterVariable.variableObj as ValueOutputInt;
                    robotController.AddElement2OutParameterVariableList(newOutParaVariable);
                }

            }


            newParameterObj.transform.position = new Vector3(0, 0, 0);
            newParameterObj.transform.rotation = Quaternion.Euler(0, 0, 0);


            if (isSelected)
                QM.SelectGameObjectInScene(newParameterObj);


            return newParameterVariable;

        }


        /// <summary>
        /// �༭�����Ĳ���
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public IOParameter EditParameter(IOParameter parameter, string name)
        {
            if (parameter == null)
                return null;

            if (name == null || name == "")
                name = "new target";

            //������������ԭ���Ʋ�ͬ������Ҫȷ���µ����Ʋ��ظ�������������δ�ı䣬�����Ʋ���Ҫ�޸�
            if (parameter.name != name)
            {
                //���������ظ�
                var names = program.GetIOParameterNameList();
                while (names.Contains(name))
                {
                    name += "(1)";
                }
            }

            if (robotController != null)
            {

                if (parameter.type == ParameterType.Input)
                {
                    //��ȡҪ�༭��Ŀ������
                    InParameterVariable inParameterVariable = robotController.GetInParameterVariableByName(parameter.name);
                    //�޸Ĳ�������
                    inParameterVariable.variableName = name;
                }
                else
                {
                    //��ȡҪ�༭��Ŀ������
                    OutParameterVariable outParameterVariable = robotController.GetOutParameterVariableByName(parameter.name);
                    //�޸Ĳ�������
                    outParameterVariable.variableName = name;
                }

                parameter.name = name;

            }



            return parameter;

        }


        /// <summary>
        /// ɾ��������Ŀ���
        /// </summary>
        /// <param name="target"></param>
        public void DeleteParameter(IOParameter parameter)
        {
            if (parameter == null)
                return;

            if (currentParameter == parameter)
                currentParameter = null;


            robotController?.RemoveElementInParameterVariableList(parameter);//�����Ŀ��������Ƴ�
            program.RemoveIOParameter(parameter);//��asset�ļ����Ƴ�

            Repaint();


        }


        /// <summary>
        /// ���²���ֵ
        /// </summary>
        public void UpdateValueOfParameter()
        {
            if (robotController == null)
                return;

            ioParameters.ForEach(ioPara =>
            {
                if (ioPara.type == ParameterType.Input)
                {
                    var valueObj = robotController.GetInParameterVariableByName(ioPara.name)?.variableObj;
                    if (valueObj != null) ioPara.value = valueObj.Value;
                }
                else
                {
                    var valueObj = robotController.GetOutParameterVariableByName(ioPara.name)?.variableObj;
                    if (valueObj != null) ioPara.value = valueObj.Value;
                }
            });
        }


        /* ���������˵�����Ӧ�Ļص����� */

        void OnValueSelectParameterCallback(object value)
        {
            if (robotController == null)
                return;

            var script = robotController.GetParameterScriptByName(currentParameter);
            if (script != null)
            {
                GameObject selectObj = script.gameObject;
                QM.SelectGameObjectInScene(selectObj);
                //QM.Log(selectObj.name);
            }


        }


        void OnValueEditParameterCallback(object value)
        {
            //��"�༭����"����
            RSP_ParameterEditingWindow editWindow = (RSP_ParameterEditingWindow)EditorWindow.GetWindowWithRect(
                    typeof(RSP_ParameterEditingWindow),
                    new Rect(Screen.width / 2, Screen.height / 2, 400, 150),
                    false,
                    "Edit target");

            currentTarget.targetScript = robotController?.GetTargetScriptByName(currentParameter.name);
            editWindow.LinkProgrammingWindow(this, currentParameter);
            editWindow.ShowUtility();


        }


        void OnValueDeleteParameterCallback(object value)
        {
            if (robotController == null)
                return;

            DeleteParameter(currentParameter);


        }


        #endregion


        #region ָ���ķ���


        // public void DeleteParameter(IOParameter parameter)
        // {
        //     if (parameter == null)
        //         return;

        //     if (currentParameter == parameter)
        //         currentParameter = null;


        //     robotController?.RemoveElementInParameterVariableList(parameter);//�����Ŀ��������Ƴ�
        //     program.RemoveIOParameter(parameter);//��asset�ļ����Ƴ�

        //     Repaint();


        // }




        /* ָ������˵�����Ӧ�Ļص����� */

        void OnValueProcessInstructionCallback(object value)
        {
            if (robotController == null || value == null)
                return;

            bool isFinish = false;
            currentInstruction?.Process(this.robotController, out isFinish);
        }


        void OnValueEditInstructionCallback(object value)
        {
            //��"�༭����"����
            RSP_InstructionEditingWindow editWindow = (RSP_InstructionEditingWindow)EditorWindow.GetWindowWithRect(
                    typeof(RSP_InstructionEditingWindow),
                    new Rect(Screen.width / 2, Screen.height / 2, 400, 150),
                    false,
                    "Edit instruction");

            editWindow.LinkProgrammingWindow(this, currentInstruction);
            editWindow.ShowUtility();
        }


        void OnValueDeleteInstructionCallback(object value)
        {
            program?.RemoveInstruction(currentInstruction);
            currentInstruction = null;

            Repaint();
        }

        #endregion










        #endregion

    }


}