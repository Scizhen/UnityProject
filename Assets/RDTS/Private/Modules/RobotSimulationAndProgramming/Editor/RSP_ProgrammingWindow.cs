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
    /// 对于单个目标点的操作菜单
    /// </summary>
    public enum TargetOperationMenu
    {
        None,//无操作
        Select,//选中
        Tesch,//示教
        Edit,//编辑
        Copy,//复制（复制与新建目标点差异不大，所以先不实现，后续有需要可再实现）
        Delete//删除

    }

    /// <summary>
    /// 对于单个指令的操作菜单
    /// </summary>
    public enum InstructionOperationMenu
    {
        None,//无操作
        Select,//选中
        Process,//执行
        Edit,//编辑
        Delete//删除

    }


    /// <summary>
    /// 对于单个参数的操作菜单
    /// </summary>
    public enum ParameterOperationMenu
    {
        None,//无操作
        Select,//选中
        Edit,//编辑
        Delete//删除

    }

    /// <summary>
    /// 机器人编程窗口（应包含程序文件（asset文件）的打开窗口的内容）
    /// </summary>
    public class RSP_ProgrammingWindow : EditorWindow
    {
        public RSP_RobotProgram program;//关联的asset文件
        public RSP_RobotController robotController;//关联的机器人控制器脚本（组件）
        public List<TargetObject> targets = new List<TargetObject>();//目标点对象列表
        public TargetObject currentTarget;//当前选中的Target
        public List<BaseInstruction> instructions = new List<BaseInstruction>();//程序指令列表
        public BaseInstruction currentInstruction;//当前选中的指令
        public int currentInstructionIndex;//当前选中的指令的索引
        public List<IOParameter> ioParameters = new List<IOParameter>();//IO参数列表
        public List<IOParameter> inParameters = new List<IOParameter>();//input类型参数列表
        public List<IOParameter> outParameters = new List<IOParameter>();//output类型参数列表
        public IOParameter currentParameter;////当前选中的Parameter

        public event Action<RSP_RobotProgram> programLoaded;
        public event Action<RSP_RobotProgram> programUnloaded;



        private RSP_RobotController _lastRobotController;//记录上一个关联的控制器组件
        private bool isProgramInit = false;
        private bool isInstructionInit = false;


        [MenuItem("Parallel-RDTS/Window/RobotSimulationProgramming", false, 200)]
        public static void ShowWindow()
        {
            RSP_ProgrammingWindow window = (RSP_ProgrammingWindow)EditorWindow.GetWindow(typeof(RSP_ProgrammingWindow));
            window.titleContent = RSP_ProgrammingWindow.thisTitleContent;//设置标题和图标
            window.minSize = new Vector2(300, 200);
            window.Show();
        }


        public void InitializeProgram(RSP_RobotProgram program)
        {
            if (this.program != null && this.program != program)
            {
                // Save the graph to the disk 将图表保存到磁盘
                EditorUtility.SetDirty(this.program);
                AssetDatabase.SaveAssets();
                // Unload the graph 卸载此图表
                programUnloaded?.Invoke(this.program);
                //SetRobotController(null);
            }


            programLoaded?.Invoke(program);
            this.program = program;


            //重置窗口内容

            InitializeWindow(program);

        }


        protected virtual void InitializeWindow(RSP_RobotProgram program)
        {
            this.robotController = null;
            this.targets = program?.targets;
            this.ioParameters = program?.ioParameters;
            this.instructions = program?.instructions;
            //SeparateIOParameter();//分配input和output类型的参数列表
            isProgramInit = true;
            isInstructionInit = true;

            Repaint();
        }


        void OnDisable()
        {
            // Save the graph to the disk 将图表保存到磁盘
            EditorUtility.SetDirty(this.program);
            AssetDatabase.SaveAssets();
            // Unload the graph 卸载此图表
            programUnloaded?.Invoke(this.program);

        }




        void OnHierarchyChange()
        {
            //QM.Log("Hierarchy Change");
        }

        void OnInspectorUpdate()
        {
            //QM.Log("Inspector Update");
            /* 当未上锁（isLock未false），且在Hierarchy中选择的对象带有控制器组件时，会直接更新窗口内容 */
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

            // if (robotController != null) GetTargtsFromRobotController();//获取关联控制器组件中的目标点信息
            // else GetTargtsFromProgramAsset(program);//获取asset文件中的目标点信息

            //参数表参数值的更新
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


        float WindowWidth; //<! 窗口宽度
        float WindowHeight;//<! 窗口高度
        bool modechange = false;

        private void OnGUI()
        {
            if (HasOpenInstances<RSP_ProgrammingWindow>())//设备库窗口是否打开
            {
                WindowWidth = position.width;//获取窗口宽度
                WindowHeight = position.height;//获取窗口高度

                DrawToolBar();//绘制顶部菜单栏


                GUILayout.BeginHorizontal();//*


                DrawTargetPanel(TargetPanel.x, TargetPanel.y, TargetPanel.width, TargetPanel.height);//目标点面板绘制
                DrawResizer1(AreaResizer1.x, AreaResizer1.y, AreaResizer1.width, AreaResizer1.height);//绘制调整条1
                DrawProgramPanel(ProgramPanel.x, ProgramPanel.y, ProgramPanel.width, ProgramPanel.height);//
                DrawResizer2(AreaResizer2.x, AreaResizer2.y, AreaResizer2.width, AreaResizer2.height);//绘制调整条2
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



        #region 初始化

        string dataPath = "Assets/RDTS/Data/";
        string iconPath = "Assets/RDTS/Private/Resources/Icons/WindowIcon/";
        string backgroundPath = "Assets/RDTS/Private/Resources/Texture/";
        string skinPath = "Assets/RDTS/Scripts/Window/";
        static string targetPrefabPath = "Assets/RDTS/Private/Modules/RobotSimulationAndProgramming/Model/Target-SplinePoint.prefab";

        private GUISkin skin_rsp;
        private GUIStyle labelStyle_Italic;//自定义背景图片的标签样式
        private GUIStyle buttonStyle_target;//用于目标点的按钮样式
        private GUIStyle labelStyle_MiddleCenter;//中心标签UI样式
        private GUIStyle labelStyle_MiddleCenter_Red;//红色中心标签样式
        private GUIStyle labelStyle_MiddleCenter_Green;//绿色中心标签样式
        private GUIStyle rowStyle;//表一行的样式
        private Texture2D image_rowBgOdd;//奇数
        private Texture2D image_rowBgEven;//偶数
        private Texture2D image_rowSelected;//被选中
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
        private Texture2D icon_handleCircle;//手柄圆圈
        private Texture2D icon_execute;
        private Texture2D icon_executeLoop;
        private Texture2D icon_iParameter;
        private Texture2D icon_oParameter;

        static GUIContent thisTitleContent;//窗口标题
        void OnEnable()
        {
            //数据资源获取

            //设置窗口标题及图标
            Texture titleIcon = AssetDatabase.LoadAssetAtPath<Texture>(iconPath + "Parallel.png");
            thisTitleContent = new GUIContent("Parallel机器人仿真与编程", titleIcon);
            //GUI风格
            rowStyle = new GUIStyle();
            rowStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
            //GUI皮肤
            skin_rsp = Resources.Load<GUISkin>("GUISkinRSP");
            labelStyle_Italic = skin_rsp.customStyles[7];
            buttonStyle_target = skin_rsp.customStyles[8];
            labelStyle_MiddleCenter = skin_rsp.customStyles[9];
            labelStyle_MiddleCenter_Red = skin_rsp.customStyles[11];
            labelStyle_MiddleCenter_Green = skin_rsp.customStyles[12];

            //加载unity自带图标
            LoadTexture();
            //区域Rect初始设置
            //SetAreaRectParam();
            //初始化数值
            InitializeProgram(this.program);


        }



        private GUIStyle resizerStyle;
        void LoadTexture()//加载素材
        {

            //图标
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
            resizerStyle.normal.background = EditorGUIUtility.Load("icons/d_AvatarBlendBackground.png") as Texture2D;//默认的Unity编辑器纹理(所有图标)
            //奇偶行颜色差异的图片
            image_rowBgOdd = EditorGUIUtility.Load("builtin skins/darkskin/images/cn entrybackodd.png") as Texture2D;
            image_rowBgEven = EditorGUIUtility.Load("builtin skins/darkskin/images/cnentrybackeven.png") as Texture2D;
            image_rowSelected = EditorGUIUtility.Load("builtin skins/darkskin/images/menuitemhover.png") as Texture2D;


        }



        #endregion



        #region  区域绘制

        float ToolbarHeight = 20f;
        bool islock = false;
        bool isPlay = false;
        bool isPlayLoop = false;
        /// <summary>窗口菜单工具栏</summary>
        void DrawToolBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            //锁定
            islock = GUILayout.Toggle(islock, icon_lock, EditorStyles.toolbarButton, GUILayout.ExpandWidth(true), GUILayout.Height(ToolbarHeight));
            if (islock)
            {

            }
            //刷新按钮—当DataAsset中修改时，重新读取并重绘窗口
            if (GUILayout.Button("Associated: " + ((robotController != null) ? robotController.gameObject.name : " ...none "), EditorStyles.toolbarButton, GUILayout.ExpandWidth(true), GUILayout.Height(ToolbarHeight)))
            {
                QM.SelectGameObjectInScene(robotController?.gameObject);

            }
            //设置：是否实时刷新浏览窗口内容
            if (GUILayout.Button("设置", EditorStyles.toolbarButton, GUILayout.Width(50), GUILayout.Height(ToolbarHeight)))
            {
                QM.Log("设置窗口属性");
            }
            //保存按钮—保存当前窗口内容的修改，并更新至asset
            if (GUILayout.Button("保存", EditorStyles.toolbarButton, GUILayout.Width(50), GUILayout.Height(ToolbarHeight)))
            {
                QM.Log("保存");
            }
            //重置该窗口内容：取消关联的组件，清空内容
            if (GUILayout.Button(new GUIContent(" " + "Reset", icon_refresh), EditorStyles.toolbarButton, GUILayout.Width(80), GUILayout.Height(ToolbarHeight)))
            {
                SetRobotController(null);
                //SeparateIOParameter();
                //ClearTargetObjectsList();
                QM.SelectObjectIsNull();
                QM.Log("重置");
            }

            if (this.robotController != null)
            {
                icon_execute = (this.robotController.isExecute) ? icon_handleCircle : icon_play;
                icon_executeLoop = (this.robotController.isExecuteLoop) ? icon_handleCircle : icon_play;
            }

            //指令一次执行按钮—设置控制器中的执行变量，从而控制指令的执行
            if (GUILayout.Button(new GUIContent(" " + "Execute", icon_execute), EditorStyles.toolbarButton, GUILayout.Width(90), GUILayout.Height(ToolbarHeight)))
            {
                if (this.robotController != null)
                {
                    this.robotController.isExecute = !this.robotController.isExecute;
                }

                //QM.Log("执行程序指令");
            }
            //指令循环执行按钮—设置控制器中的执行变量，从而控制指令的执行
            if (GUILayout.Button(new GUIContent(" " + "Execute Loop", icon_executeLoop), EditorStyles.toolbarButton, GUILayout.Width(120), GUILayout.Height(ToolbarHeight)))
            {
                if (this.robotController != null)
                {
                    this.robotController.isExecuteLoop = !this.robotController.isExecuteLoop;
                }

                //QM.Log("执行程序指令");
            }


            GUILayout.FlexibleSpace();//灵活填充

            //帮助按钮—提示如何操作该设备库
            if (GUILayout.Button(icon_help, EditorStyles.toolbarButton, GUILayout.Width(24), GUILayout.Height(ToolbarHeight)))
            {
                ////打开弹窗
                //PopWindowHelp helpWindow = (PopWindowHelp)EditorWindow.GetWindow(
                //       typeof(PopWindowHelp),
                //       true,
                //       "Help");
                //var content = helpWindow.ReadHelpData(1);
                //helpWindow.SetShowContents(content);
                //helpWindow.ShowUtility();
                ///Debug.Log("帮助");
            }
            //关闭按钮—关闭当前窗口
            if (GUILayout.Button(icon_close, EditorStyles.toolbarButton, GUILayout.Width(24), GUILayout.Height(ToolbarHeight)))
            {
                if (EditorUtility.DisplayDialog("关闭窗口", "是否关闭该窗口？", "yes", "no"))
                {
                    this.Close();
                    ///Debug.Log("关闭");
                }

            }

            EditorGUILayout.EndHorizontal();


        }


        //四块区域
        private AreaRectParam TargetPanel = new AreaRectParam();
        private AreaRectParam ProgramPanel = new AreaRectParam();
        private AreaRectParam PropertyPanel = new AreaRectParam();
        //三条调整条
        private AreaRectParam AreaResizer1 = new AreaRectParam();//调整宽度
        private AreaRectParam AreaResizer2 = new AreaRectParam();//调整宽度

        //比例系数
        private float widthSizeRatio_TargetPanel = 0.3f;
        private float widthSizeRatio_ProgramPanel = 0.6f;
        private float resizerWidth = 10f;//调整区域的宽度
        float Area1Width;//<! 左侧区域1宽度
        float Area2Width;//<! 中间区域2宽度
        float Area3Width;//<! 右侧区域3宽度



        Rect Rect_targetPanel;
        Vector2 _scrollview1;
        /// <summary>机器人目标点面板</summary>
        void DrawTargetPanel(float x, float y, float width, float height)
        {
            Rect_targetPanel = new Rect(x, y, width, height);
            GUILayout.BeginArea(Rect_targetPanel);

            GUILayout.Space(2);

            TargetPanelContent();

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Number of targets：{targets.Count}", labelStyle_Italic);//目标点数量
            GUILayout.Space(10);
            GUILayout.Label($"CurrentTarget：{currentTarget?.name}", labelStyle_Italic);//当前选中的目标点
            GUILayout.EndHorizontal();
            GUILayout.Space(2);

            GUILayout.EndArea();

        }


        Rect Rect_programPanel;
        Vector2 _scrollview2;
        /// <summary>机器人程序面板</summary>
        void DrawProgramPanel(float x, float y, float width, float height)
        {
            Rect_programPanel = new Rect(x, y, width, height);
            GUILayout.BeginArea(Rect_programPanel);
            //_scrollview2 = GUILayout.BeginScrollView(_scrollview2);//<! 滑动条

            GUILayout.Space(2);

            ProgramPanelContent();

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Number of instructions：{instructions.Count}", labelStyle_Italic);//目标点数量
            GUILayout.Space(10);
            GUILayout.Label($"CurrentInstruction：{currentInstructionIndex + 1}", labelStyle_Italic);//当前选中的目标点
            GUILayout.EndHorizontal();
            GUILayout.Space(2);

            //GUILayout.EndScrollView();//<! 滑动条
            GUILayout.EndArea();

        }

        Rect Rect_propertyPanel;
        Vector2 _scrollview3;
        Vector2 _scrollview4;
        /// <summary>机器人属性面板</summary>
        void DrawPropertyPanel(float x, float y, float width, float height)
        {
            Rect_propertyPanel = new Rect(x, y, width, height);
            GUILayout.BeginArea(Rect_propertyPanel);
            //_scrollview3 = GUILayout.BeginScrollView(_scrollview3);//<! 滑动条

            GUILayout.Space(2);

            ParameterPanelContent();

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Number of parameters：{ioParameters.Count}", labelStyle_Italic);//参数数量
            GUILayout.Space(10);
            GUILayout.Label($"CurrentParameter：{currentParameter?.name}", labelStyle_Italic);//当前选中的参数
            GUILayout.EndHorizontal();
            GUILayout.Space(2);


            //GUILayout.EndScrollView();//<! 滑动条
            GUILayout.EndArea();

        }


        private Rect Rect_resizer1;
        /// <summary>
        /// 宽度调整条1
        /// </summary>
        void DrawResizer1(float x, float y, float width, float height)
        {
            Rect_resizer1 = new Rect(x, y, width, height);
            GUILayout.BeginArea(new Rect(Rect_resizer1.position + (Vector2.right * resizerWidth), new Vector2(4, height * 3)), resizerStyle);
            GUILayout.EndArea();

            EditorGUIUtility.AddCursorRect(Rect_resizer1, MouseCursor.ResizeHorizontal);//改变鼠标形状：显示鼠标指针位于调整大小位置
        }



        private Rect Rect_resizer2;
        /// <summary>
        /// 宽度调整条3
        /// </summary>
        void DrawResizer2(float x, float y, float width, float height)
        {
            Rect_resizer2 = new Rect(x, y, width, height);
            GUILayout.BeginArea(new Rect(Rect_resizer2.position + (Vector2.right * resizerWidth), new Vector2(4, height * 3)), resizerStyle);
            GUILayout.EndArea();

            EditorGUIUtility.AddCursorRect(Rect_resizer2, MouseCursor.ResizeHorizontal);//改变鼠标形状：显示鼠标指针位于调整大小位置
        }


        /* UI元素绘制 */

        #region 目标点面板绘制

        float Height_operation = 25f;
        string[] Operation_TargetPanel = new string[3] { "New target", "Delete all", "Refresh" };
        bool foldout_Target = true;//折叠箭头标签的折叠状态（true：展开  false：折叠）
        int countOfButton_row = 2;




        //private bool _isSelectNull = false;

        /// <summary>
        /// 目标点面板内容
        /// </summary>
        void TargetPanelContent()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            //“目标点”操作按钮
            if (GUILayout.Button(new GUIContent(" " + Operation_TargetPanel[0], icon_add), GUILayout.Width(Area1Width / countOfButton_row - 5), GUILayout.Height(Height_operation)))
            {
                //清空当前的选中对象
                //QM.SelectObjectIsNull();
                //打开"创建新目标点"弹窗
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
                if (EditorUtility.DisplayDialog("清空目标点", "是否删除所有的目标点？", "yes", "no"))
                {
                    //清空asset文件中的数据
                    this.program?.ClearAllTargets();
                    //清空RSP_RobotController脚本中的目标点对象
                    this.robotController?.ClearAllTargetVariables();
                }

            }

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            //_scrollview1 = GUILayout.BeginScrollView(_scrollview1);//<! 滑动条
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            DrawTargetListItems();
            GUILayout.EndHorizontal();
            //GUILayout.EndScrollView();//<! 滑动条



        }


        float Height_target = 30f;//UI元素所占高度（像素值）
        TargetOperationMenu _targetOperationMenu = TargetOperationMenu.None;//对于单个目标的操作菜单枚举
        ReorderableList reorderableList_target;//列表显示


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
                        currentTarget = element;//<! 设置当前选中的Target
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

                        if (e.button == 1)//<! 注意使用e.type而不是Event.current.type
                        {
                            //绘制按钮的下拉菜单
                            GenericMenu targetMenu = new GenericMenu();
                            targetMenu.AddItem(new GUIContent("选中目标点对象"), _targetOperationMenu.Equals(TargetOperationMenu.Select), OnValueSelectTargetCallback, TargetOperationMenu.Select);
                            targetMenu.AddItem(new GUIContent("示教目标点位置"), _targetOperationMenu.Equals(TargetOperationMenu.Tesch), OnValueTeachTargetCallback, TargetOperationMenu.Tesch);
                            targetMenu.AddItem(new GUIContent("编辑该目标点"), _targetOperationMenu.Equals(TargetOperationMenu.Edit), OnValueEditTargetCallback, TargetOperationMenu.Edit);
                            targetMenu.AddSeparator("");
                            targetMenu.AddItem(new GUIContent("删除该目标点"), _targetOperationMenu.Equals(TargetOperationMenu.Delete), OnValueDeleteTargetCallback, TargetOperationMenu.Delete);
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
                _scrollview1 = GUILayout.BeginScrollView(_scrollview1);//<! 滑动条
                reorderableList_target.DoLayoutList();
                EditorGUILayout.EndScrollView();
            }





        }


        #endregion


        #region  程序指令面板绘制

        float Height_operation2 = 25f;
        string[] Operation_ProgramPanel = new string[2] { "New instruction", "Delete all" };
        int countOfButton_row2 = 2;
        bool uiChange_instruction = false;//指令列表的UI发生改变
        void ProgramPanelContent()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            //“目标点”操作按钮
            if (GUILayout.Button(new GUIContent(" " + Operation_ProgramPanel[0], icon_add), GUILayout.Width(Area2Width / countOfButton_row2 - 5), GUILayout.Height(Height_operation2)))
            {
                //清空当前的选中对象
                //QM.SelectObjectIsNull();
                //打开"创建新目标点"弹窗
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
                if (EditorUtility.DisplayDialog("清空程序指令", "是否删除所有的指令？", "yes", "no"))
                {
                    //清空asset文件中的指令数据
                    this.program?.ClearAllInstructions();
                }

            }

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            //_scrollview1 = GUILayout.BeginScrollView(_scrollview1);//<! 滑动条
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);

            EditorGUI.BeginChangeCheck();
            DrawInstructionListItems();

            if (EditorGUI.EndChangeCheck())
            {
                //绘制轨迹
                robotController?.DrawLineBetweenTargets();

                QM.SelectObjectIsNull();

                ///QM.Log("UI change!");
                uiChange_instruction = true;
            }



            GUILayout.EndHorizontal();
            //GUILayout.EndScrollView();//<! 滑动条



        }


        float Height_instruction = 30f;//UI元素所占高度（像素值）
        InstructionOperationMenu _instructionOperationMenu = InstructionOperationMenu.None;
        ReorderableList reorderableList_instruction;//列表显示
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
                        currentInstruction = element;//<! 设置当前选中的指令
                        currentInstructionIndex = index;//<! 获取当前选中的指令的索引
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

                        if (e.button == 1)//<! 注意使用e.type而不是Event.current.type
                        {
                            //绘制按钮的下拉菜单
                            GenericMenu targetMenu = new GenericMenu();
                            targetMenu.AddItem(new GUIContent("执行该指令"), _instructionOperationMenu.Equals(InstructionOperationMenu.Process), OnValueProcessInstructionCallback, atrribute[0].instruction);
                            targetMenu.AddItem(new GUIContent("编辑该指令"), _instructionOperationMenu.Equals(InstructionOperationMenu.Edit), OnValueEditInstructionCallback, atrribute[0].instruction);
                            targetMenu.AddSeparator("");
                            targetMenu.AddItem(new GUIContent("删除该指令"), _instructionOperationMenu.Equals(InstructionOperationMenu.Delete), OnValueDeleteInstructionCallback, atrribute[0].instruction);
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
                _scrollview2 = GUILayout.BeginScrollView(_scrollview2);//<! 滑动条
                reorderableList_instruction.DoLayoutList();
                EditorGUILayout.EndScrollView();
            }





        }


        static bool IsInstruction(Type instructionType)
        {
            //此type是抽象的就返回false
            if (instructionType.IsAbstract)//如果Type是抽象的(不能实例化,但只能用作派生类的基类)，则为 true；否则为 false
                return false;
            //不是抽象的，就检索NodeMenuItemAttribute自定义特性的个数，若存在就返回true
            return instructionType.GetCustomAttributes<InstructionTypeAttribute>().Count() > 0;
        }


        #endregion



        #region 参数面板绘制


        float Height_operation3 = 25f;
        string[] Operation_ParameterPanel = new string[3] { "New Parameter", "Refresh", "Delete all" };
        string[] tableTitle_Parameter = new string[3] { "IO", "名称", "值" };
        bool foldout_Parameter = true;//折叠箭头标签的折叠状态（true：展开  false：折叠）
        int countOfButton_row3 = 3;
        float height_ParameterTitle = 20f;
        int selectedRow_Parameter = -1;//参数表中被选中那一行
        bool isShow_inputParameter = false;
        bool isShow_outputParameter = false;
        ParameterOperationMenu _parameterOperationMenu = ParameterOperationMenu.None;


        void ParameterPanelContent()
        {

            /* 参数表的操作按钮绘制 */
            GUILayout.BeginHorizontal();

            GUILayout.Space(8);

            //“目标点”操作按钮
            if (GUILayout.Button(new GUIContent(" " + Operation_ParameterPanel[0], icon_add), GUILayout.Width(Area3Width / countOfButton_row3 - 10), GUILayout.Height(Height_operation3)))
            {
                //清空当前的选中对象
                //QM.SelectObjectIsNull();
                //打开"创建新目标点"弹窗
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
                if (EditorUtility.DisplayDialog("清空参数", "是否删除所有的参数？", "yes", "no"))
                {
                    //清空asset文件中的参数数据
                    this.program?.ClearAllIOParameters();
                    //清空RSP_RobotController脚本中的参数变量
                    this.robotController?.ClearAllParameterVariables();
                }

            }

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            /* 参数表的标题栏绘制 */
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.Label(tableTitle_Parameter[0], EditorStyles.toolbarButton, GUILayout.Width(Area3Width * 1.5f / 10));
            GUILayout.Label(tableTitle_Parameter[1], EditorStyles.toolbarButton, GUILayout.Width(Area3Width * 4 / 10));
            GUILayout.Label(tableTitle_Parameter[2], EditorStyles.toolbarButton, GUILayout.Width(Area3Width * 4 / 10));
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
            GUILayout.Space(2);

            /* 绘制参数列表 */
            _scrollview3 = GUILayout.BeginScrollView(_scrollview3);//<! 滑动条
                                                                   ////List_interfaceObjs.ForEach(obj => OneInterfaceObjFormat(obj));//【*舍弃该语句】因为foeeach中不要使用remove或者add方法
            for (int i = 0; i < ioParameters.Count; i++)
                OneParameterFormat(ioParameters[i], i);


            GUILayout.EndScrollView();//<! 滑动条



        }


        void OneParameterFormat(IOParameter iOParameter, int index)
        {
            if (iOParameter == null)
                return;

            //<! 设置奇偶行背景颜色
            if (selectedRow_Parameter != index)
                rowStyle.normal.background = (index % 2 == 0) ? image_rowBgEven : image_rowBgOdd;
            else//<! 若此接口对象被选中，则修改背景颜色
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
            //<! 名称
            GUILayout.Space(5);
            if (GUILayout.Button(iOParameter.name, labelStyle_MiddleCenter, GUILayout.Width(Area3Width * 4 / 10 - 12), GUILayout.Height(20)))
            {
                ParameterButtonDeal(iOParameter, index);
            }
            //<! 值
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
                //绘制按钮的下拉菜单
                GenericMenu parameterMenu = new GenericMenu();
                parameterMenu.AddItem(new GUIContent("选中参数对象"), _parameterOperationMenu.Equals(ParameterOperationMenu.Select), OnValueSelectParameterCallback, ParameterOperationMenu.Select);
                parameterMenu.AddItem(new GUIContent("编辑该参数"), _parameterOperationMenu.Equals(ParameterOperationMenu.Edit), OnValueEditParameterCallback, ParameterOperationMenu.Edit);
                parameterMenu.AddSeparator("");
                parameterMenu.AddItem(new GUIContent("删除该参数"), _parameterOperationMenu.Equals(ParameterOperationMenu.Delete), OnValueDeleteParameterCallback, ParameterOperationMenu.Delete);
                parameterMenu.ShowAsContext();


            }


            Repaint();
        }


        #endregion



        /// <summary>
        /// 设置区域的Rect参数
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

        #region 区域调整的方法

        private bool isResizing;//是否在调整区域尺寸
        private bool isResizingWidth1;//是否在调整第一条调整条
        private bool isResizingHeight2;//是否在调整第二条调整条
        private bool isResizingWidth3;//是否在调整第三条调整条
        /// <summary>
        /// 鼠标按下和抬起事件
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

                        isResizingWidth1 = (Rect_resizer1.Contains(e.mousePosition)) ? true : false;//判断是否在调整宽度

                        isResizingHeight2 = (Rect_resizer2.Contains(e.mousePosition)) ? true : false;//判断是否在调整高度


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
        /// 调整区域尺寸大小
        /// </summary>
        /// <param name="e"></param>
        private void Resize(Event e)
        {
            if (isResizing)
            {

                //调整条1
                if (isResizingWidth1 && e.mousePosition.x > WindowWidth * 0.1 && e.mousePosition.x < WindowWidth * 0.4)
                {
                    widthSizeRatio_TargetPanel = e.mousePosition.x / WindowWidth;
                }
                //调整条2
                if (isResizingHeight2 && e.mousePosition.x > WindowWidth * 0.5 && e.mousePosition.x < WindowWidth * 0.8)
                {
                    widthSizeRatio_ProgramPanel = e.mousePosition.x / WindowWidth;
                }


                // QM.Log("widthSizeRatio：" + widthSizeRatio + " " + "heightSizeRatio: " + heightSizeRatio);

                Repaint();
            }
        }

        #endregion


        /// <summary>
        /// 交换列表中的两个元素
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
        /// 从关联的机器人控制器脚本中获取目标点信息
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
        // /// 清空目标点列表
        // /// </summary>
        // public void ClearTargetObjectsList()
        // {
        //     targetObjects?.Clear();
        // }


        /// <summary>
        /// 设置关联的机器人控制器
        /// </summary>
        /// <param name="controller"></param> <summary>
        public void SetRobotController(RSP_RobotController controller)
        {
            robotController = controller;
        }


        #region 目标点处理的方法

        /// <summary>
        /// 创建一个新的目标点
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="gameObject">是否提供了已有的目标点对象</param>
        /// <param name="robotController">关联的机器人控制器组件</param>
        /// <param name="transform">创建时的位姿</param>
        /// <param name="isSelected">是否创建后被选中</param>
        /// <returns></returns>
        public TargetVariable CreateNewTarget(string name, RSP_Target script, RSP_RobotController robotController, Transform transform, bool isSelected = false)
        {
            if (name == null || name == "")
                name = "new target";

            //避免名称重复
            var names = program.GetTargetNameList();
            while (names.Contains(name))
            {
                name += "(1)";
            }

            TargetVariable newTargetVariable = new TargetVariable();
            newTargetVariable.targetName = name;
            //若未给定关联的目标点gameobject
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
            /* 将目标点存储至asset、控制器组件相应列表中 */
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
        /// 编辑给定的目标点
        /// </summary>
        /// <param name="target">要被编辑的目标点</param>
        /// <param name="name">新的名称</param>
        /// <param name="newRobotController">新的关联的控制器组件</param>
        /// <param name="oldRobotController">旧的关联的控制器组件</param>
        /// <returns></returns>
        public TargetObject EditTarget(TargetObject target, string name, RSP_Target script)
        {
            if (target == null)
                return null;

            if (name == null || name == "")
                name = "new target";

            //若输入名称与原名称不同，则需要确保新的名称不重复；若输入名称未改变，则名称不需要修改
            if (target.name != name)
            {
                //避免名称重复
                var names = program.GetTargetNameList();
                while (names.Contains(name))
                {
                    name += "(1)";
                }
            }

            if (robotController != null)
            {
                //获取要编辑的目标点变量
                TargetVariable targetVariable = robotController.GetTargetVariableByName(target.name);
                //修改目标点名称
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
        /// 删除给定的目标点
        /// </summary>
        /// <param name="target"></param>
        public void DeleteTarget(TargetObject target)
        {
            if (target == null)
                return;

            if (currentTarget == target)
                currentTarget = null;


            robotController?.RemoveElementInTargetVariableListByName(target.name);//关联的控制器中移除
            program.RemoveTarget(target);//从asset文件中移除
            Repaint();

            // Undo.DestroyObjectImmediate(target.targetObj);//撤销销毁对象操作（*要放在销毁语句前）
            // DestroyImmediate(target.targetObj);


        }


        /* 目标点操作菜单所对应的回调方法 */

        void OnValueSelectTargetCallback(object value)
        {
            if (robotController == null)
                return;

            var script = robotController.GetTargetScriptByName(currentTarget.name);
            if (script != null)
            {
                GameObject selectObj = script.gameObject;
                QM.SelectGameObjectInScene(selectObj);

                QM.Log($"选中目标点：{currentTarget?.name}");
            }


        }

        void OnValueEditTargetCallback(object value)
        {
            //打开"编辑新目标点"弹窗
            RSP_TargetEditingWindow editWindow = (RSP_TargetEditingWindow)EditorWindow.GetWindowWithRect(
                    typeof(RSP_TargetEditingWindow),
                    new Rect(Screen.width / 2, Screen.height / 2, 400, 150),
                    false,
                    "Edit target");

            currentTarget.targetScript = robotController?.GetTargetScriptByName(currentTarget.name);
            editWindow.LinkProgrammingWindow(this, currentTarget);
            editWindow.ShowUtility();
            QM.Log($"编辑目标点：{currentTarget?.name}");
        }

        void OnValueTeachTargetCallback(object value)
        {
            if (robotController == null)
            {
                EditorUtility.DisplayDialog("示教警告", "未关联到有效的控制器组件！", "yes");
                return;
            }


            var script = robotController.GetTargetScriptByName(currentTarget.name);
            if (script != null && robotController.endPoint != null)
            {
                script.gameObject.transform.position = robotController.endPoint.transform.position;
                script.gameObject.transform.rotation = robotController.endPoint.transform.rotation;

                GameObject selectObj = script.gameObject;
                QM.SelectGameObjectInScene(selectObj);

                QM.Log($"示教目标点：{currentTarget?.name}");
            }

            if (robotController.endPoint == null)
            {
                EditorUtility.DisplayDialog("示教警告", "关联的控制器组件未赋予有效的EndPoint！", "yes");
            }

        }

        void OnValueCopyTargetCallback(object value)
        {
            QM.Log($"复制目标点：{currentTarget?.name}");
        }

        void OnValueDeleteTargetCallback(object value)
        {
            DeleteTarget(currentTarget);
            QM.Log($"删除目标点：{currentTarget?.name}");
        }

        #endregion


        #region 参数处理的方法



        /// <summary>
        /// 在参数列表中添加一个参数
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
        /// 在参数列表中移除一个参数
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
        /// 组合input和output类型的参数列表
        /// </summary>
        void AssembleIOParameter()
        {
            if (inParameters == null || outParameters == null || ioParameters == null)
                return;

            ioParameters = inParameters;
            ioParameters.AddRange(outParameters);
        }

        /// <summary>
        /// 分离input和output类型的参数列表
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

            //避免名称重复
            var names = program.GetIOParameterNameList();
            while (names.Contains(name))
            {
                name += "(1)";
            }

            IOParameterVariable newParameterVariable = new IOParameterVariable();
            newParameterVariable.variableName = name;

            //创建一个新的值对象
            GameObject newParameterObj = QM.CreateOneValueObj(null, name, null,
                VALUETYPE.INT,
                (parameterType == ParameterType.Input) ? VALUEDIRECTION.INPUT : VALUEDIRECTION.OUTPUT
            );
            newParameterVariable.variableObj = newParameterObj.GetComponent<Value>();
            newParameterObj.name = name;


            IOParameter newParameter = new IOParameter();
            newParameter.name = name;
            newParameter.type = parameterType;
            /* 将目标点存储至asset、控制器组件相应列表中 */
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
        /// 编辑给定的参数
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

            //若输入名称与原名称不同，则需要确保新的名称不重复；若输入名称未改变，则名称不需要修改
            if (parameter.name != name)
            {
                //避免名称重复
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
                    //获取要编辑的目标点变量
                    InParameterVariable inParameterVariable = robotController.GetInParameterVariableByName(parameter.name);
                    //修改参数名称
                    inParameterVariable.variableName = name;
                }
                else
                {
                    //获取要编辑的目标点变量
                    OutParameterVariable outParameterVariable = robotController.GetOutParameterVariableByName(parameter.name);
                    //修改参数名称
                    outParameterVariable.variableName = name;
                }

                parameter.name = name;

            }



            return parameter;

        }


        /// <summary>
        /// 删除给定的目标点
        /// </summary>
        /// <param name="target"></param>
        public void DeleteParameter(IOParameter parameter)
        {
            if (parameter == null)
                return;

            if (currentParameter == parameter)
                currentParameter = null;


            robotController?.RemoveElementInParameterVariableList(parameter);//关联的控制器中移除
            program.RemoveIOParameter(parameter);//从asset文件中移除

            Repaint();


        }


        /// <summary>
        /// 更新参数值
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


        /* 参数操作菜单所对应的回调方法 */

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
            //打开"编辑参数"弹窗
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


        #region 指令处理的方法


        // public void DeleteParameter(IOParameter parameter)
        // {
        //     if (parameter == null)
        //         return;

        //     if (currentParameter == parameter)
        //         currentParameter = null;


        //     robotController?.RemoveElementInParameterVariableList(parameter);//关联的控制器中移除
        //     program.RemoveIOParameter(parameter);//从asset文件中移除

        //     Repaint();


        // }




        /* 指令操作菜单所对应的回调方法 */

        void OnValueProcessInstructionCallback(object value)
        {
            if (robotController == null || value == null)
                return;

            bool isFinish = false;
            currentInstruction?.Process(this.robotController, out isFinish);
        }


        void OnValueEditInstructionCallback(object value)
        {
            //打开"编辑参数"弹窗
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