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
    /// 区域绘制参数
    /// </summary>
    public class AreaRectParam
    {
        public float x;
        public float y;
        public float width;
        public float height;
    }

    /// <summary>
    /// 接口模块类：一共有几类接口组件
    /// </summary>
    public class InterfaceModule
    {
        public string type;//接口类型，如PLC、Modbus等
        public GameObject interfaceObject;//在asset文件中对应的预制体gameobject
        public Interface.BaseInterface script;//对应接口组件的脚本名称（类名）
    }

    /// <summary>
    /// 接口库的排序方式
    /// </summary>
    public enum InterfaceSortWay
    {
        Order,//按照在Hierarchy面板中先后次序排序
        Type//按照接口类型排序
    }


    /// <summary>
    /// 接口对象类：一个类对象对应一个接口对象
    /// </summary>
    public class InterfaceObject
    {
        public int number;//序号
        public string name;//对象名称
        public string type;//接口类型
        public GameObject interfaceObject;//在Scene中对应的接口组件gameobject
    }


    /// <summary>
    /// 值对象类：一个类对象对应一个值对象
    /// </summary>
    public class ValueObject
    {
        public int number;//序号
        public string name;//对象名称
        public string datatype;//数据类型，如InputBool等
        public string address;//地址
        public string value;//值
        public GameObject valueObject;//在Scene中对应的值对象gameobject

        public InterfaceObject parent;//父对象
        public VALUETYPE type;//值类型
        public VALUEDIRECTION direction;//输入输出方向
    }



    /// <summary>
    /// 改进的接口库：分为三块区域（接口面板，接口对象表，值对象表），实现了添加、添加上一个、复制、复制所有、选中、选中所有、删除、删除所有、刷新、浏览Browse、两种排序方式、
    /// 接口对象与值对象父子级关系、编辑修改、Excel导入等功能。
    /// </summary>
    public class InterfaceLibraryWindow : EditorWindow
    {
        Scene currentScene;//当前场景
        Scene lastScene;//上一个场景

        //接口对象
        List<InterfaceModule> List_InterfaceModules = new List<InterfaceModule>();//储存共有几类接口组件
        List<InterfaceObject> List_interfaceObjs = new List<InterfaceObject>();//<! 存储一共有多少接口对象
        InterfaceObject currentInterfaceObject;//<! 当前要添加的接口对象
        InterfaceObject lastInterfaceObject;//<! 上一个添加的接口对象
        InterfaceObject currentSelectedInterfaceObj;//<! 当前选中的接口对象，即值对象的父对象
        //接口对象排序
        InterfaceSortWay SortWay_Interface = InterfaceSortWay.Order;//排序方式，默认按照先后顺序

        string[] Menu_interfaceType = new string[6] { "PLC", "Modbus", "OPC UA", "RoboDK", "Redis", "MySQL" };//接口类型的数组
        public string[] GetMenu_interfaceType { get { return Menu_interfaceType; } }//供外部读取此变量

        //值对象
        List<ValueObject> List_valueObjects = new List<ValueObject>();//<! 存储一共有多少值对象
        ValueObject currentValueObject;//<! 当前要添加的值对象
        ValueObject lastValueObject;//<! 上一个添加的值对象
        ValueObject editedValueObject;//<! 要被编辑的值对象
        ValueObject EditInformation;//<! 缓存从弹窗获取的编辑信息

        HashSet<ValueObject> HashSet_errorValueObjects = new HashSet<ValueObject>();//记录有格式错误的值对象的集
        bool isCheckError = false;//是否检测有错误
        //值对象排序
        InterfaceSortWay SortWay_Value = InterfaceSortWay.Order;//排序方式，默认按照先后顺序
        //值对象的数据类型
        string[] Menu_valueType = new string[9] { 
            "InputBool", "InputFloat", "InputInt", 
            "OutputBool", "OutputFloat", "OutputInt", 
            "MiddleBool", "MiddleFloat", "MiddleInt" };
        public string[] GetMenu_valueType { get { return Menu_valueType; } }//供外部读取此变量

        //导入Excel
        string _ExcelFilePath = "";//要导入的Excel文件路径
        public string ExcelFilePath { 
            get { return _ExcelFilePath;  } 
            set { _ExcelFilePath = value; }
        }///供外部获取文件路径的接口
        System.Data.DataSet _ExcelFileResult;//读取到的Excel文件结构
        public System.Data.DataSet ExcelFileResult { 
            get { return _ExcelFileResult;  } 
            set { _ExcelFileResult = value; }
        }///供外部获取读取文件的接口
        System.Data.DataTableCollection _ExcelFileTables;//读取到的Excel的表单
        public System.Data.DataTableCollection ExcelFileTables { 
            get { return _ExcelFileTables;  } 
            set { _ExcelFileTables = value; }
        }///供外部获取表单的接口

        //标志位
        bool isAdding_Interface = false;//<! 是否当前需要添加接口对象
        bool isRemoveAllObjs_Interface = false;//<! 是否当前需要删除所有接口对象
        bool isCopyAllObjs_Interface = false;//<! 是否当前需要复制所有接口对象

        bool isAdding_Value = false;//<! 是否当前需要添加值对象
        bool isRemoveAllObjs_Value = false;//<! 是否当前需要删除所有值对象
        bool isCopyAllObjs_Value = false;//<! 是否当前需要复制所有值对象
        bool isEditing_Value = false;//<! 是否当前在编辑值对象


        [MenuItem("Parallel-RDTS/Window/InterfaceLibraryWindow", false, 200)]
        private static void ShowWindow()
        {
            InterfaceLibraryWindow window = (InterfaceLibraryWindow)EditorWindow.GetWindow(typeof(InterfaceLibraryWindow));
            window.titleContent = InterfaceLibraryWindow.thisTitleContent;//设置标题和图标
            window.minSize = new Vector2(300, 200);
            window.Show();
        }


        float WindowWidth; //<! 窗口宽度
        float WindowHeight;//<! 窗口高度
        float LeftAreaWidth;//<1 左侧区域宽度
        float RightAreaWidth;//<! 右侧区域宽度
        float LeftAreaHeight;//<1 左侧区域高度
        float RightAreaHeight;//<! 右侧区域高度
        private void OnGUI()
        {
            if (HasOpenInstances<InterfaceLibraryWindow>())//设备库窗口是否打开
            {
                WindowWidth = position.width;//获取窗口宽度
                WindowHeight = position.height;//获取窗口高度
                ////左右区域的高与宽
                //LeftAreaWidth = WindowWidth * widthSizeRatio;
                //RightAreaWidth = WindowWidth - LeftAreaWidth;
                //LeftAreaHeight = RightAreaHeight = WindowHeight;

                //WindowOperation();//<! 注意要放前面，不然会报layout/repaint的错（好像是layout/repaint的冲突）
               
                MenuList();//顶部菜单栏

                //GUILayout.BeginHorizontal();//*
                /////左边区域
                //GUILayout.BeginArea(new Rect(0, 0, LeftAreaWidth, LeftAreaHeight));//*
                //DrawInterfacePanel(InterfacePanel.x, InterfacePanel.y, InterfacePanel.width, InterfacePanel.height);//接口面板绘制
                //DrawInterfaceObjectTable(InterfaceObjectTable.x, InterfaceObjectTable.y, InterfaceObjectTable.width, InterfaceObjectTable.height);//接口对象列表绘制
                //DrawHeightResizer(HeightResizer.x, HeightResizer.y, HeightResizer.width, HeightResizer.height);//高度方向的调整条
                //GUILayout.EndArea();

                /////右边区域
                //GUILayout.BeginArea(new Rect(WindowWidth * widthSizeRatio, 0, RightAreaWidth, RightAreaHeight));//*
                //DrawValueObjectTable(ValueObjectTable.x, ValueObjectTable.y, ValueObjectTable.width, ValueObjectTable.height);//值对象列表绘制
                //DrawWidthResizer(WidthResizer.x, WidthResizer.y, WidthResizer.width, WidthResizer.height);//宽度方向的调整条
                //GUILayout.EndArea();
                //GUILayout.EndHorizontal();//*

               
                //ProcessEvents(Event.current);//鼠标或键盘响应事件
                //SetAreaRectParam();//设置区域宽和高
                
                switch(viewIndex)
                {
                    case 0:
                        WelcomeView();//欢迎界面
                        break;
                    case 1:
                        InterfaceView();//接口库内容界面
                        break;
                }



                if (GUI.changed) Repaint();
               
            }


        }


        void OnFocus()
        {
            ///QM.Log("InterfaceLibraryWindow — OnFocus");
            //if(!isCoroutineStart)
            //    StartEditorCoroutine("BrowseObjectsInCurrentScene");//开启协程
        }

        void OnLostFocus()
        {
            ///QM.Log("InterfaceLibraryWindow — OnLostFocus");
            //if (isCoroutineStart)
            //    StopEditorCoroutine("BrowseObjectsInCurrentScene");//关闭协程

        }

        private void OnHierarchyChange()
        {
          
        }


        private void Update()
        {

        }


        /// <summary>
        /// 对于此窗口的操作
        /// </summary>
        void WindowOperation()
        {
            //添加一个值对象
            if(isAdding_Value && currentValueObject != null)
            {
                AddCurrentValueObject();
                SetCurrentValueObject(null);
                isAdding_Value = false;
            }
            //拷贝所有当前列表中的值对象（*直接通过创建gameobject的方式，而不是通过设置currentInterfaceObject）
            if (isCopyAllObjs_Value)
            {
                if (CopyAllObjectsInValueObjectList(List_valueObjects))//若全部复制完成
                    isCopyAllObjs_Value = false;
            }
            //删除所有值对象
            if (isRemoveAllObjs_Value)
            {
                RemoveAllObjectsInValueObjectList(List_valueObjects);
                if (List_valueObjects.Count == 0)//<! *增加这句判断：因为销毁gameobject需要一定时间，所以要等列表元素全部情况才能置位
                    isRemoveAllObjs_Value = false;
            }
            //编辑值对象
            if(isEditing_Value && editedValueObject != null && EditInformation != null)
            {
                if(EditValueObject(editedValueObject, EditInformation))
                {
                    SetEditedValueObject(null);
                    SetEditInformation(null);
                    isEditing_Value = false;
                }
               
            }

            //添加一个接口对象
            if (isAdding_Interface && currentInterfaceObject!=null)
            {
                AddCurrentInterfaceObject();
                SetCurrentInterfaceObject(null);
                isAdding_Interface = false;///只在此处置为false
            }
            //拷贝所有当前列表中的接口对象（*直接通过创建gameobject的方式，而不是通过设置currentInterfaceObject）
            if(isCopyAllObjs_Interface)
            {
                if (CopyAllObjectsInInterfaceObjectList(List_interfaceObjs))//若全部复制完成
                    isCopyAllObjs_Interface = false;
            }
            //删除所有接口对象
            if (isRemoveAllObjs_Interface)
            {
                RemoveAllObjectsInInterfaceObjectList(List_interfaceObjs);
                if (List_interfaceObjs.Count == 0)//<! *增加这句判断：因为销毁gameobject需要一定时间，所以要等列表元素全部情况才能置位
                    isRemoveAllObjs_Interface = false;
            }

            //【注意：协程不用原因，当窗口没关闭时一直处于运行状态，消耗计算资源反而大】
            BrowseInterfaceGameObjectsInCurrentScene();//<! 实时刷新浏览Scene中的接口组件。注意：不是根对象，且处于未激活状态下的gameobject不会被找到（可以由此避免对某一接口对象进行操作）
            BrowseValueGameObjectsInCurrentScene();//<! 实时刷新浏览Scene中的值对象。注意：不是根对象，且处于未激活状态下的gameobject不会被找到（可以由此避免对某一值对象进行操作）
            ValueObjectPropertyCheck(List_valueObjects);//<! 值对象表的格式检测 注意：错址检测、超址检测暂未加入，因为不同接口要求的地址格式不同。后续有需要可以加入，有关西门子PLC地址格式可参考InterfaceLibraryWindow.SignalPropertyCheck()方法
        }




        #region Init

        Coroutine c;//协程
        bool isCoroutineStart = false;//协程是否开启

        string dataPath = "Assets/RDTS/Data/";
        string iconPath = "Assets/RDTS/Private/Resources/Icons/WindowIcon/";
        string backgroundPath = "Assets/RDTS/Private/Resources/Texture/";
        string skinPath = "Assets/RDTS/Scripts/Window/";

        InterfaceLibraryData interfaceLibraryData;//接口库数据

        static GUIContent thisTitleContent;//窗口标题
        void OnEnable()
        {
            //数据资源获取
            interfaceLibraryData = AssetDatabase.LoadAssetAtPath<InterfaceLibraryData>(RDTSPath.path_Data + "InterfaceLibraryData.asset");
            List_InterfaceModules = ReadLibraryData(interfaceLibraryData);
            Menu_interfaceType = GetMenuOfInterfaceType(List_InterfaceModules);
            //设置窗口标题及图标
            Texture titleIcon = AssetDatabase.LoadAssetAtPath<Texture>(iconPath + "Parallel.png");
            thisTitleContent = new GUIContent("Parallel接口库", titleIcon);
            //GUI风格
            rowStyle = new GUIStyle();
            rowStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
            //加载unity自带图标
            LoadTexture();
            //区域Rect初始设置
            SetAreaRectParam();
            //初始化数值
            number_selectedRow_interfaceObj = 0;
            isAdding_Interface = false;
            isAdding_Value = false;
            isRemoveAllObjs_Interface = false;
            isRemoveAllObjs_Value = false;
            isCopyAllObjs_Interface = false;
            isCopyAllObjs_Value = false;
            isEditing_Value = false;
            isCheckError = false;
            //协程【不采用】
            //StartEditorCoroutine("BrowseObjectsInCurrentScene");//开启协程
        }

        void OnDisable()
        {
            //if(isCoroutineStart)
            //    StopEditorCoroutine("BrowseObjectsInCurrentScene");//关闭协程

        }


        private GUISkin skin_interfaceLibrary;
        private GUIStyle resizerStyle;
        private GUIStyle labelStyle_MiddleCenter;//中心标签UI样式
        private GUIStyle labelStyle_MiddleCenter_Color;//颜色中心标签样式
        private GUIStyle labelStyle_MiddleCenter_Red;//红色中心标签样式
        private GUIStyle labelStyle_MiddleCenter_Green;//绿色中心标签样式
        private GUIStyle labelStyle_MiddleCenter_Orange;//橙色中心标签样式
        private GUIStyle labelStyle_Italic;//自定义背景图片的标签样式
        private GUIStyle rowStyle;//列表一行的样式

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

        private Texture2D image_rowBgOdd;//奇数
        private Texture2D image_rowBgEven;//偶数
        private Texture2D image_rowSelected;//被选中
        private Texture2D image_error;//检测错误

        private Texture image_Dropdown;
        private Texture image_Interface;

        void LoadTexture()//加载素材
        {
            //GUI皮肤
            skin_interfaceLibrary = AssetDatabase.LoadAssetAtPath<GUISkin>("Assets/RDTS/Scripts/Window/Editor/InterfaceLibraryWindow/WindowInterfaceLibrary.guiskin");
            labelStyle_MiddleCenter = skin_interfaceLibrary.customStyles[7];
            labelStyle_MiddleCenter_Color = skin_interfaceLibrary.customStyles[8];
            labelStyle_Italic = skin_interfaceLibrary.customStyles[9];
            labelStyle_MiddleCenter_Red = skin_interfaceLibrary.customStyles[10];
            labelStyle_MiddleCenter_Green = skin_interfaceLibrary.customStyles[11];
            labelStyle_MiddleCenter_Orange = skin_interfaceLibrary.customStyles[12];

            //Style
            resizerStyle = new GUIStyle();
            resizerStyle.normal.background = EditorGUIUtility.Load("icons/d_AvatarBlendBackground.png") as Texture2D;//默认的Unity编辑器纹理(所有图标)

            //图标
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


            //下拉图标
            image_Dropdown = AssetDatabase.LoadAssetAtPath<Texture>(iconPath + "dropdown.png");
            image_Interface = AssetDatabase.LoadAssetAtPath<Texture>(iconPath + "WindowInterfaceIcon.png");

            //奇偶行颜色差异的图片
            image_rowBgOdd = EditorGUIUtility.Load("builtin skins/darkskin/images/cn entrybackodd.png") as Texture2D;
            image_rowBgEven = EditorGUIUtility.Load("builtin skins/darkskin/images/cnentrybackeven.png") as Texture2D;
            image_rowSelected = EditorGUIUtility.Load("builtin skins/darkskin/images/menuitemhover.png") as Texture2D;
            image_error = AssetDatabase.LoadAssetAtPath<Texture2D>(backgroundPath + "LightRedBackground.png");
        }


        #endregion




        #region Toolbar

        float menuHeight = 20f;
        string libraryViewName = "Welcome";

        /// <summary>窗口菜单工具栏</summary>
        void MenuList()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            //重置窗口内容为起始的默认内容
            if (GUILayout.Button(libraryViewName, EditorStyles.toolbarButton, GUILayout.Width(100), GUILayout.Height(menuHeight)))
            {
                viewIndex = (viewIndex == 0) ? 1 : 0;
                libraryViewName = (viewIndex == 0) ? "Welcome" : "Interface";
            }
            //设置：是否实时刷新浏览窗口内容
            if (GUILayout.Button("设置", EditorStyles.toolbarButton, GUILayout.Width(50), GUILayout.Height(menuHeight)))
            {
                QM.Log("设置窗口属性");
            }
            //保存按钮—保存当前窗口内容的修改，并更新至asset
            if (GUILayout.Button("保存", EditorStyles.toolbarButton, GUILayout.Width(50), GUILayout.Height(menuHeight)))
            {
                Debug.Log("保存");
            }
            //刷新按钮—当DataAsset中修改时，重新读取并重绘窗口
            if (GUILayout.Button(icon_refresh, EditorStyles.toolbarButton, GUILayout.Width(50), GUILayout.Height(menuHeight)))
            {
                RefreshWindowContent();///1.刷新当前场景  2.重置窗口相关参数
                ShowNotification(new GUIContent("窗口内容已刷新"));
                ///Debug.Log("刷新");
            }

            GUILayout.FlexibleSpace();//灵活填充

            //帮助按钮—提示如何操作该设备库
            if (GUILayout.Button(icon_help, EditorStyles.toolbarButton, GUILayout.Width(24), GUILayout.Height(menuHeight)))
            {
                //打开弹窗
                PopWindowHelp helpWindow = (PopWindowHelp)EditorWindow.GetWindow(
                       typeof(PopWindowHelp),
                       true,
                       "Help");
                var content = helpWindow.ReadHelpData(1);
                helpWindow.SetShowContents(content);
                helpWindow.ShowUtility();
                ///Debug.Log("帮助");
            }
            //关闭按钮—关闭当前窗口
            if (GUILayout.Button(icon_close, EditorStyles.toolbarButton, GUILayout.Width(24), GUILayout.Height(menuHeight)))
            {
                if (EditorUtility.DisplayDialog("关闭窗口", "是否关闭该窗口？", "yes", "no"))
                {
                    this.Close();
                    ///Debug.Log("关闭");
                }

            }

            EditorGUILayout.EndHorizontal();


        }

        #endregion




        #region ViewContent

        int viewIndex = 0;//界面内容索引号

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
            //场景
            currentScene = SceneManager.GetActiveScene();
            if (currentScene != lastScene)
                RefreshWindowContent();
            lastScene = currentScene;

            //左右区域的高与宽
            LeftAreaWidth = WindowWidth * widthSizeRatio;
            RightAreaWidth = WindowWidth - LeftAreaWidth;
            LeftAreaHeight = RightAreaHeight = WindowHeight;

            WindowOperation();//<! 注意要放前面，不然会报layout/repaint的错（好像是layout/repaint的冲突）

            GUILayout.BeginHorizontal();//*
            ///左边区域
            GUILayout.BeginArea(new Rect(0, 0, LeftAreaWidth, LeftAreaHeight));//*
            DrawInterfacePanel(InterfacePanel.x, InterfacePanel.y, InterfacePanel.width, InterfacePanel.height);//接口面板绘制
            DrawInterfaceObjectTable(InterfaceObjectTable.x, InterfaceObjectTable.y, InterfaceObjectTable.width, InterfaceObjectTable.height);//接口对象列表绘制
            DrawHeightResizer(HeightResizer.x, HeightResizer.y, HeightResizer.width, HeightResizer.height);//高度方向的调整条
            GUILayout.EndArea();

            ///右边区域
            GUILayout.BeginArea(new Rect(WindowWidth * widthSizeRatio, 0, RightAreaWidth, RightAreaHeight));//*
            DrawValueObjectTable(ValueObjectTable.x, ValueObjectTable.y, ValueObjectTable.width, ValueObjectTable.height);//值对象列表绘制
            DrawWidthResizer(WidthResizer.x, WidthResizer.y, WidthResizer.width, WidthResizer.height);//宽度方向的调整条
            GUILayout.EndArea();
            GUILayout.EndHorizontal();//*


            ProcessEvents(Event.current);//鼠标或键盘响应事件
            SetAreaRectParam();//设置区域宽和高
        }


        

        //三块区域
        private AreaRectParam InterfacePanel = new AreaRectParam();
        private AreaRectParam InterfaceObjectTable = new AreaRectParam();
        private AreaRectParam ValueObjectTable = new AreaRectParam();
        //两条调整条
        private AreaRectParam HeightResizer = new AreaRectParam();
        private AreaRectParam WidthResizer = new AreaRectParam();

        private float heightSizeRatio = 0.35f;//高度尺寸比变量
        private float widthSizeRatio = 0.35f;//宽度尺寸比变量
        private float resizerHeight = 10f;//调整区域的高度
        private float resizerWidth = 10f;//调整区域的宽度
        private bool isResizing;//是否在调整区域尺寸
        private bool isResizingHeight;//是否在调整高度方向的区域尺寸
        private bool isResizingWidth;//是否在调整宽度方向的区域尺寸

        private bool isSelected_row;//是否某一行被选中
        private int number_selectedRow_interfaceObj;//接口对象中被选中那一行的序号
        private int number_selectedRow_valueObj;//值对象中被选中那一行的序号

        private Rect Rect_interfacePanel;
        Vector2 _scrollview1;
        /// <summary>
        ///接口面板视图绘制
        /// </summary>
        void DrawInterfacePanel(float x, float y, float width, float height)
        {
            Rect_interfacePanel = new Rect(x, y, width, height);
            GUILayout.BeginArea(Rect_interfacePanel);
            _scrollview1 = GUILayout.BeginScrollView(_scrollview1);//<! 滑动条

            GUILayout.Space(5);
            //显示当前场景信息
            GUILayout.Box($"Scene：{currentScene.name}", GUILayout.ExpandWidth(true));
            //接口面板：选择接口类型
            InterfacePanelContent();

            GUILayout.EndScrollView();//<! 滑动条
            GUILayout.EndArea();

        }


        private Rect Rect_interfaceObjectTable;
        Vector2 _scrollview2;
        /// <summary>
        /// 接口对象列表的绘制
        /// </summary>
        void DrawInterfaceObjectTable(float x, float y, float width, float height)
        {
            Rect_interfaceObjectTable = new Rect(x, y, width, height);
            GUILayout.BeginArea(Rect_interfaceObjectTable);
            ////_scrollview2 = GUILayout.BeginScrollView(_scrollview2);//<! 滑动条

            GUILayout.Space(2);
            //接口对象列表：顶部的工具栏，对象表
            InterfaceObjectTableContent();

            ////GUILayout.EndScrollView();//<! 滑动条
            GUILayout.Label($"Number of interface objects：{List_interfaceObjs.Count}", labelStyle_Italic, GUILayout.ExpandWidth(true));//接口对象数量
            GUILayout.Space(2);
            GUILayout.EndArea();
        }


        private Rect Rect_valueObjectTable;
        Vector2 _scrollview3;
        /// <summary>
        /// 值对象列表的绘制（信号、数值等）
        /// </summary>
        void DrawValueObjectTable(float x, float y, float width, float height)
        {
            Rect_valueObjectTable = new Rect(x, y, width, height);
            GUILayout.BeginArea(Rect_valueObjectTable);
            //// _scrollview3 = GUILayout.BeginScrollView(_scrollview3);//<! 滑动条

            //GUILayout.Label("ValueObjectTable");
            GUILayout.Space(5);
            ValueObjectTableContent();

            //// GUILayout.EndScrollView();//<! 滑动条
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Number of Value Objects：{List_valueObjects.Count}", labelStyle_Italic, GUILayout.ExpandWidth(true));//值对象数量
            GUILayout.EndHorizontal();
            GUILayout.Space(2);

            GUILayout.EndArea();
        }


        private Rect Rect_heightResizer;
        /// <summary>
        /// 高度方向的尺寸调整条
        /// </summary>
        void DrawHeightResizer(float x, float y, float width, float height)
        {
            Rect_heightResizer = new Rect(x, y, width, height);
            GUILayout.BeginArea(new Rect(Rect_heightResizer.position + (Vector2.up * resizerHeight), new Vector2(width * 2, 4)), resizerStyle);
            GUILayout.EndArea();

            EditorGUIUtility.AddCursorRect(Rect_heightResizer, MouseCursor.ResizeVertical);//改变鼠标形状：显示鼠标指针位于调整大小位置
        }


        private Rect Rect_widthResizer;
        /// <summary>
        /// 高度方向的尺寸调整条
        /// </summary>
        void DrawWidthResizer(float x, float y, float width, float height)
        {
            Rect_widthResizer = new Rect(x, y, width, height);
            GUILayout.BeginArea(new Rect(Rect_widthResizer.position + (Vector2.right * resizerWidth), new Vector2(4, height * 3)), resizerStyle);//高度*3是因为避免显示时出现调整条渐变褪色
            GUILayout.EndArea();

            EditorGUIUtility.AddCursorRect(Rect_widthResizer, MouseCursor.ResizeHorizontal);//改变鼠标形状：显示鼠标指针位于调整大小位置
        }





        /* UI元素绘制内容 */


        float Height_operation = 22f;
        /// <summary>
        /// 接口面板内容
        /// </summary>
        void InterfacePanelContent()
        {
            GUILayout.Label("Interface Operation");
            
            //添加一个接口对象，弹窗形式
            if (GUILayout.Button(new GUIContent("  Add", icon_add), GUILayout.Height(Height_operation)))
            {
                isAdding_Interface = true;//置位标志位，用于添加对象
                //打开弹窗
                PopWindowInterfaceObjectAdd addWindow = (PopWindowInterfaceObjectAdd)EditorWindow.GetWindowWithRect(
                       typeof(PopWindowInterfaceObjectAdd),
                       new Rect(Screen.width / 2, Screen.height / 2, 400, 150),
                       false,
                       "Add interface object");
                addWindow.LinkInterfaceLibraryWindow(this);
                addWindow.ShowUtility();
                
            }
            //添加与上一个相同的接口对象，简化操作
            if (GUILayout.Button(new GUIContent("  Add previous", icon_add), GUILayout.Height(Height_operation)))
            {
                if (lastInterfaceObject != null)
                {
                    SetCurrentInterfaceObject(lastInterfaceObject);
                    isAdding_Interface = true;
                }
                  
            }
            //删除当前场景下所有的接口组件（对象）
            if (GUILayout.Button(new GUIContent("  Delete all", icon_delete), GUILayout.Height(Height_operation)))
            {
                if (EditorUtility.DisplayDialog("删除操作", "是否删除所有接口对象？", "yes", "no"))
                {
                    currentSelectedInterfaceObj = null;
                    isRemoveAllObjs_Interface = true;
                }
            }

            GUILayout.BeginHorizontal();
            GUILayout.EndHorizontal();
        }



        string[] TableOperation_interfaceObject = new string[4] { "Refresh", "Select all", "Copy all", "Sort" };
        string[] TableTitle_interfaceObject = new string[4] { "Number", "Name", "Type", "Operate" };//"序号", "名称", "类型", "操作"
        /// <summary>
        /// 接口对象列表内容
        /// </summary>
        void InterfaceObjectTableContent()
        {
            
            /* 1.绘制工具栏 */
            GUILayout.BeginHorizontal();
            int count_operation = TableOperation_interfaceObject.Length;
            //刷新相关设置
            if (GUILayout.Button(new GUIContent("  " + TableOperation_interfaceObject[0], icon_refresh), GUILayout.Width(LeftAreaWidth / count_operation), GUILayout.Height(Height_operation)))
            {
                SortWay_Interface = InterfaceSortWay.Order;//重置排序方式
                number_selectedRow_interfaceObj = 0;//消除选中行的背景样式
                currentSelectedInterfaceObj = null;//消除当前要查看的接口对象
                SelectObjectIsNull();
            }
            //在Scene中选中（高亮）列表中所有的接口对象
            if (GUILayout.Button(new GUIContent("  " + TableOperation_interfaceObject[1], icon_pick), GUILayout.Width(LeftAreaWidth / count_operation), GUILayout.Height(Height_operation)))
            {
                SelectInterfaceObjectInScene(List_interfaceObjs);
            }
            //复制当前表中所有的接口对象
            if (GUILayout.Button(new GUIContent("  " + TableOperation_interfaceObject[2], icon_copy), GUILayout.Width(LeftAreaWidth / count_operation), GUILayout.Height(Height_operation)))
            {
                if (EditorUtility.DisplayDialog("拷贝操作", "是否复制所有接口对象？", "yes", "no"))
                {
                    isCopyAllObjs_Interface = true;
                }

            }
            GUILayout.FlexibleSpace();
            //选择表中接口对象的排列方式
            if (GUILayout.Button(icon_sort, GUILayout.Width(Height_operation * 1.5f), GUILayout.Height(Height_operation)))
            {
                //绘制按钮的下拉菜单
                GenericMenu sortwayMenu = new GenericMenu();
                sortwayMenu.AddItem(new GUIContent("先后顺序"), SortWay_Interface.Equals(InterfaceSortWay.Order), OnValueSelectedOfSortwayCallback, InterfaceSortWay.Order);
                sortwayMenu.AddItem(new GUIContent("接口类型"), SortWay_Interface.Equals(InterfaceSortWay.Type), OnValueSelectedOfSortwayCallback, InterfaceSortWay.Type);

                sortwayMenu.ShowAsContext();
            }
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            /* 2.绘制标题 */
            GUILayout.BeginHorizontal();
            GUILayout.Label(TableTitle_interfaceObject[0], EditorStyles.toolbarButton, GUILayout.Width(LeftAreaWidth / 7));
            GUILayout.Label(TableTitle_interfaceObject[1], EditorStyles.toolbarButton, GUILayout.Width(LeftAreaWidth * 2 / 7));
            GUILayout.Label(TableTitle_interfaceObject[2], EditorStyles.toolbarButton, GUILayout.Width(LeftAreaWidth * 2 / 7));
            GUILayout.Label(TableTitle_interfaceObject[3], EditorStyles.toolbarButton, GUILayout.Width(LeftAreaWidth * 2 / 7 - 5));
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            _scrollview2 = GUILayout.BeginScrollView(_scrollview2);//<! 滑动条
            /* 3.绘制接口对象列表 */
            ////List_interfaceObjs.ForEach(obj => OneInterfaceObjFormat(obj));//【*舍弃该语句】因为foeeach中不要使用remove或者add方法
            for (int i = 0; i < List_interfaceObjs.Count; i++)
                OneInterfaceObjFormat(List_interfaceObjs[i]);
            GUILayout.EndScrollView();//<! 滑动条
        }


        /// <summary>
        /// 一个（行）接口对象的格式
        /// </summary>
        /// <param name="interfaceObj"></param>
        void OneInterfaceObjFormat(InterfaceObject interfaceObj)
        {
            if (interfaceObj == null)
                return;

            int rowNumber = interfaceObj.number;//获取此接口对象的序号
            //<! 设置奇偶行背景颜色
            if (number_selectedRow_interfaceObj != rowNumber)
                rowStyle.normal.background = (rowNumber % 2 == 0) ? image_rowBgEven : image_rowBgOdd;
            else//<! 若此接口对象被选中，则修改背景颜色
                rowStyle.normal.background = image_rowSelected;

            GUILayout.Space(5);
            GUILayout.BeginHorizontal(rowStyle);

            GUIStyle style_button = new GUIStyle(EditorStyles.miniButtonMid);//按钮样式
            //<! 序号
            GUILayout.Label(rowNumber.ToString(), labelStyle_MiddleCenter, GUILayout.Width(LeftAreaWidth / 7 - 5));
            //<! 名称
            GUILayout.Label(interfaceObj.name, labelStyle_MiddleCenter_Color, GUILayout.Width(LeftAreaWidth * 2 / 7 - 5));
            //<! 类型
            GUILayout.Label(interfaceObj.type, labelStyle_MiddleCenter, GUILayout.Width(LeftAreaWidth * 2 / 7 - 5));
            //<! 操作
            float width = LeftAreaWidth * 2 / 7;
            //查看此接口对象下包含的“值对象”
            if (GUILayout.Button(icon_eye, style_button, GUILayout.Width(width / 4 - 2)))
            {
                number_selectedRow_interfaceObj = rowNumber;
                currentSelectedInterfaceObj = interfaceObj;//当前要查看的接口对象
            }
            //Ping一下在Hierarchy面板中的位置（在Scene中选中此接口对象）
            if (GUILayout.Button(icon_pick, style_button, GUILayout.Width(width / 4 - 2)))
            {
                SelectInterfaceObjectInScene(interfaceObj);
                number_selectedRow_interfaceObj = rowNumber;//设置此行被选中
            }
            //复制此接口对象
            if (GUILayout.Button("+", style_button, GUILayout.Width(width / 4 - 2)))
            {
                CopyOneInterfaceObject(interfaceObj);
            }
            //删除此接口对象
            if (GUILayout.Button("-", style_button, GUILayout.Width(width / 4 - 2)))
            {
                //if (currentSelectedInterfaceObj == interfaceObj)
                //    currentSelectedInterfaceObj = null;
                currentSelectedInterfaceObj = null;
                ///删除列表元素、对应的gameobject
                RemoveOneObjectInInterfaceObjectList(interfaceObj, List_interfaceObjs);
               
            }

            GUILayout.EndHorizontal();


        }




        string[] TableOperation_valueObject = new string[8] { "Refresh", "Add", "Add previous", "Import Excel", "Select all", "Copy all", "Delete all",  "Sort" };
        string[] TableTitle_valueObject = new string[6] { "Number", "Name", "DataType", "Address", "Value", "Operate" };//"序号", "名称", "数据类型", "地址", "值", "操作"
        /// <summary>
        /// 值对象列表内容
        /// </summary>
        void ValueObjectTableContent()
        {
            /* 1.绘制工具栏 */
            GUILayout.BeginHorizontal();
            int count_operation1 = 4 + 1;
            //刷新相关设置
            if (GUILayout.Button(new GUIContent("  " + TableOperation_valueObject[0], icon_refresh), GUILayout.Width(RightAreaWidth / count_operation1), GUILayout.Height(Height_operation)))
            {
                SortWay_Value = InterfaceSortWay.Order;//重置排序方式
                number_selectedRow_valueObj = 0;//消除选中行的背景样式
                SelectObjectIsNull();
            }
            //添加一个接口对象，弹窗形式
            if (GUILayout.Button(new GUIContent("  " + TableOperation_valueObject[1], icon_add), GUILayout.Width(RightAreaWidth / count_operation1), GUILayout.Height(Height_operation)))
            {
                isAdding_Value = true;
                //打开弹窗
                PopWindowValueObjectAdd addWindow = (PopWindowValueObjectAdd)EditorWindow.GetWindowWithRect(
                       typeof(PopWindowValueObjectAdd),
                       new Rect(Screen.width / 2, Screen.height / 2, 400, 200),
                       false,
                       "Add value object");
                addWindow.LinkInterfaceLibraryWindow(this);
                addWindow.Show();
            }
            //添加与上一个相同的值对象，简化操作
            if (GUILayout.Button(new GUIContent("  " + TableOperation_valueObject[2], icon_add), GUILayout.Width(RightAreaWidth / count_operation1), GUILayout.Height(Height_operation)))
            {
                if (lastValueObject != null)
                {
                    SetCurrentValueObject(lastValueObject);
                    isAdding_Value = true;
                }
                
            }
            //从Excel表中导入值对象
            if (GUILayout.Button(new GUIContent("  " + TableOperation_valueObject[3], icon_excel), GUILayout.Width(RightAreaWidth / count_operation1), GUILayout.Height(Height_operation)))
            {
                ///弹出窗口：选择要读取的表单
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
            //在Scene中选中（高亮）列表中所有的值对象
            if (GUILayout.Button(new GUIContent("  " + TableOperation_valueObject[4], icon_pick), GUILayout.Width(RightAreaWidth / count_operation2), GUILayout.Height(Height_operation)))
            {
                SelectValueObjectInScene(List_valueObjects);
            }
            //复制当前表中所有的值对象
            if (GUILayout.Button(new GUIContent("  " + TableOperation_valueObject[5], icon_copy), GUILayout.Width(RightAreaWidth / count_operation2), GUILayout.Height(Height_operation)))
            {
                if (EditorUtility.DisplayDialog("拷贝操作", "是否复制所有值对象？", "yes", "no"))
                {
                    isCopyAllObjs_Value = true;
                }
            }
            //删除当前表中所有的值对象
            if (GUILayout.Button(new GUIContent("  " + TableOperation_valueObject[6], icon_delete), GUILayout.Width(RightAreaWidth / count_operation2), GUILayout.Height(Height_operation)))
            {
                if (EditorUtility.DisplayDialog("删除操作", "是否删除所有值对象？", "yes", "no"))
                {
                    isRemoveAllObjs_Value = true;
                }
            }
            GUILayout.FlexibleSpace();
            ////选择表中值对象的排列方式
            if (GUILayout.Button(new GUIContent(icon_sort), GUILayout.Width(Height_operation * 1.5f), GUILayout.Height(Height_operation)))
            {
                //绘制按钮的下拉菜单
                GenericMenu sortwayMenu = new GenericMenu();
                sortwayMenu.AddItem(new GUIContent("先后顺序"), SortWay_Value.Equals(InterfaceSortWay.Order), OnValueSelectedOfValueSortwayCallback, InterfaceSortWay.Order);
                sortwayMenu.AddItem(new GUIContent("接口类型"), SortWay_Value.Equals(InterfaceSortWay.Type), OnValueSelectedOfValueSortwayCallback, InterfaceSortWay.Type);

                sortwayMenu.ShowAsContext();
            }
            GUILayout.Space(15);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            /* 2.绘制标题 */
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

            _scrollview3 = GUILayout.BeginScrollView(_scrollview3);//<! 滑动条
            /* 3.绘制值对象列表 */
            ////List_interfaceObjs.ForEach(obj => OneInterfaceObjFormat(obj));//【*舍弃该语句】因为foeeach中不要使用remove或者add方法
            for (int i = 0; i < List_valueObjects.Count; i++)
                OneValueObjFormat(List_valueObjects[i]);

            GUILayout.EndScrollView();//<! 滑动条
        }


        /// <summary>
        /// 一个（行）值对象的格式
        /// </summary>
        /// <param name="valueObj"></param>
        void OneValueObjFormat(ValueObject valueObj)
        {
            if (valueObj == null)
                return;

            int rowNumber = valueObj.number;//获取此值对象的序号
            //<! 设置奇偶行背景颜色
            if (number_selectedRow_valueObj != rowNumber)
                rowStyle.normal.background = (rowNumber % 2 == 0) ? image_rowBgEven : image_rowBgOdd;
            else//<! 若此接口对象被选中，则修改背景颜色
                rowStyle.normal.background = image_rowSelected;

            if(HashSet_errorValueObjects.Contains(valueObj))
                rowStyle.normal.background = image_error;

            GUILayout.Space(5);
            GUILayout.BeginHorizontal(rowStyle);
            GUILayout.Space(3);
            GUIStyle style_button = new GUIStyle(EditorStyles.miniButtonMid);//按钮样式
            //<! 序号
            GUILayout.Label(rowNumber.ToString(), labelStyle_MiddleCenter, GUILayout.Width(RightAreaWidth / 9 - 5));
            //<! 名称
            GUILayout.Label(valueObj.name, labelStyle_MiddleCenter_Color, GUILayout.Width(RightAreaWidth * 2 / 9 - 5));
            //<! 数据类型
            GUILayout.Label(valueObj.datatype, labelStyle_MiddleCenter, GUILayout.Width(RightAreaWidth * 2 / 9 - 5));
            //<! 地址
            GUILayout.Label(valueObj.address, labelStyle_MiddleCenter, GUILayout.Width(RightAreaWidth * 1 / 9 - 5));
            //<! 值
            GUILayout.Label(valueObj.value, SetValueLabelGUIStyleByValueType(valueObj), GUILayout.Width(RightAreaWidth * 1 / 9 - 2));
            //<! 操作
            float width = RightAreaWidth * 2 / 9 - 22;
            //改变此值对象的信息（弹窗）
            if (GUILayout.Button(icon_edit, style_button, GUILayout.Width(width / 4 - 1)))
            {
                isEditing_Value = true;
                SetEditedValueObject(valueObj);
                SelectValueObjectInScene(valueObj);
                number_selectedRow_valueObj = rowNumber;
                //打开弹窗
                PopWindowValueObjectEdit editWindow = (PopWindowValueObjectEdit)EditorWindow.GetWindowWithRect(
                       typeof(PopWindowValueObjectEdit),
                       new Rect(Screen.width / 2, Screen.height / 2, 400, 200),
                       false,
                       "Edit value object");
                editWindow.LinkInterfaceLibraryWindow(this);
                editWindow.Show();
            }
            //Ping一下在Hierarchy面板中的位置（在Scene中选中此值对象）
            if (GUILayout.Button(icon_pick, style_button, GUILayout.Width(width / 4 - 1)))
            {
                SelectValueObjectInScene(valueObj);
                number_selectedRow_valueObj = rowNumber;
            }
            //复制此值对象
            if (GUILayout.Button("+", style_button, GUILayout.Width(width / 4 - 1)))
            {
                CopyOneValueObject(valueObj);
            }
            //删除此值对象
            if (GUILayout.Button("-", style_button, GUILayout.Width(width / 4 - 1)))
            {
                ///删除列表元素、对应的gameobject
                RemoveOneObjectInValueObjectList(valueObj, List_valueObjects);
            }

            GUILayout.EndHorizontal();
        }



        #endregion




        #region Methods


        /// <summary>
        /// 刷新此窗口的相关信息：如读取asset数据等
        /// </summary>
        void RefreshWindowContent()
        {
            //重新获取当前场景
            currentScene = SceneManager.GetActiveScene();
            //重新读取数据
            interfaceLibraryData = AssetDatabase.LoadAssetAtPath<InterfaceLibraryData>(RDTSPath.path_Data + "InterfaceLibraryData.asset");
            List_InterfaceModules = ReadLibraryData(interfaceLibraryData);
            Menu_interfaceType = GetMenuOfInterfaceType(List_InterfaceModules);
            //重置参数
            number_selectedRow_interfaceObj = 0;//消除选中行的背景样式
            number_selectedRow_valueObj = 0;
            currentSelectedInterfaceObj = null;//消除当前要查看的接口对象
            SelectObjectIsNull();
            Repaint();
        }




        /*下拉菜单功能模块【切换时存在闪烁现象，可优化】*/
        string Select_String = "";//共有变量，存储下拉菜单中选中项
        string DropDownMenu(string content, string[] itemList)
        {

            if (EditorGUILayout.DropdownButton(new GUIContent(content), FocusType.Keyboard))
            {
                GenericMenu menu = new GenericMenu();

                if (itemList.Length <= 0)
                    return null;

                //添加菜单项、回调方法及其参数
                foreach (string item in itemList)
                {
                    if (string.IsNullOrEmpty(item)) continue;
                    menu.AddItem(new GUIContent(item), content.Equals(item), OnValueSelectedCallback, item);
                }

                menu.ShowAsContext();
            }

            return Select_String;
        }

        //回调方法
        void OnValueSelectedCallback(object value)
        {
            Select_String = value.ToString();

            // QM.Log(Select_String);
        }

        /// <summary>
        /// 排序方式的回调方法
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
        /// 读取接口库的asset数据资源
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
        /// 根据读取到的asset数据来获取“接口类型的下拉菜单项”
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
        /// 设置区域的Rect参数
        /// </summary>
        void SetAreaRectParam()
        {
            //InterfacePanel的Rect参数
            InterfacePanel.x = 0;///相对于所在的Area（一级父元素）
            InterfacePanel.y = 0 + menuHeight;
            InterfacePanel.width = WindowWidth * widthSizeRatio;
            InterfacePanel.height = (WindowHeight - menuHeight) * heightSizeRatio;

            //InterfaceObjectTable的Rect参数
            InterfaceObjectTable.x = 0;
            InterfaceObjectTable.y = 0 + menuHeight + InterfacePanel.height + resizerHeight;
            InterfaceObjectTable.width = WindowWidth * widthSizeRatio;
            InterfaceObjectTable.height = (WindowHeight - menuHeight) * (1 - heightSizeRatio) - resizerHeight;

            //ValueObjectTable的Rect参数
            ValueObjectTable.x = resizerWidth;///相对于所在的Area（一级父元素）
            ValueObjectTable.y = 0 + menuHeight;
            ValueObjectTable.width = WindowWidth * (1 - widthSizeRatio) - resizerWidth;
            ValueObjectTable.height = WindowHeight - menuHeight;

            //HeightResizer的Rect参数
            HeightResizer.x = 0;
            HeightResizer.y = 0 + menuHeight + InterfacePanel.height - 15f;//-15f为了在拖动时显示鼠标光标的变化
            HeightResizer.width = WindowWidth * widthSizeRatio;
            HeightResizer.height = resizerHeight +10f;//值相比resizerHeight大一些，好被识别去拖动

            //WidthResizer的Rect参数
            WidthResizer.x = 0 - 8f;//-8f为了将两个调整条在交叉处贴近些
            WidthResizer.y = 0 + menuHeight;
            WidthResizer.width = resizerWidth + 10f;//值相比resizerWidth大一些，好被识别去拖动
            WidthResizer.height = WindowHeight - menuHeight;

        }


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

                        isResizingHeight = (Rect_heightResizer.Contains(e.mousePosition)) ? true : false;//判断是否在调整高度

                        //由于宽度调整条的Rect是相对于右侧区域的，所以需要对其Rect进行变换，才能用于判断鼠标坐标位置e.mousePosition是否在调整条范围Rect内
                        Rect _RectWidthResizer = new Rect(
                            Rect_widthResizer.x + WindowWidth * widthSizeRatio,
                            Rect_widthResizer.y,
                            Rect_widthResizer.width,
                            Rect_widthResizer.height);
                        isResizingWidth = (_RectWidthResizer.Contains(e.mousePosition)) ? true : false;//判断是否在调整宽度
                        ///QM.Log("isResizingHeight：" + isResizingHeight + " " + "isResizingWidth: " + isResizingWidth);
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
        /// 调整区域尺寸大小
        /// </summary>
        /// <param name="e"></param>
        private void Resize(Event e)
        {
            if (isResizing)
            {
                //调整上下高度
                if (isResizingHeight && e.mousePosition.y > (menuHeight + 10) && e.mousePosition.y < (WindowHeight - 50))
                {
                    heightSizeRatio = e.mousePosition.y / WindowHeight;
                }
                //调整左右宽度
                if (isResizingWidth && e.mousePosition.x > 50 && e.mousePosition.x < (WindowWidth - 50))
                {
                    widthSizeRatio = e.mousePosition.x / WindowWidth;
                }

                // QM.Log("widthSizeRatio：" + widthSizeRatio + " " + "heightSizeRatio: " + heightSizeRatio);

                Repaint();
            }
        }


        /// <summary>
        /// 根据值对象输入输出方向类型设置其值的标签GUI样式
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




        /* 对存储对象列表的操作 */

        /// <summary>
        /// 清空一个InterfaceObject类型的列表
        /// </summary>
        /// <param name="list">清空的目标列表</param>
        void ClearInterfaceObjectList(List<InterfaceObject> list)
        {
            list.Clear();
        }


        /// <summary>
        /// 清空一个ValueObject类型的列表
        /// </summary>
        /// <param name="list"></param>
        void ClearValueObjectList(List<ValueObject> list)
        {
            list.Clear();
        }


        /// <summary>
        /// 设置当前要添加的接口对象
        /// </summary>
        /// <param name="interfaceObj"></param>
        public InterfaceObject SetCurrentInterfaceObject(InterfaceObject interfaceObj)
        { 
            return currentInterfaceObject = interfaceObj;
        }


        /// <summary>
        /// 设置当前要添加的值对象
        /// </summary>
        /// <param name="valueObj"></param>
        /// <returns></returns>
        public ValueObject SetCurrentValueObject(ValueObject valueObj)
        {
            return currentValueObject = valueObj;
        }


        /// <summary>
        /// 设置当前要编辑的值对象
        /// </summary>
        /// <param name="valueObj"></param>
        /// <returns></returns>
        ValueObject SetEditedValueObject(ValueObject valueObj)
        {
            return editedValueObject = valueObj;
        }


        /// <summary>
        /// 供外部获取要编辑的值对象
        /// </summary>
        /// <returns></returns>
        public ValueObject GetEditedValueObject()
        {
            return editedValueObject;
        }


        /// <summary>
        /// 设置当前的编辑信息
        /// </summary>
        /// <param name="valueObj"></param>
        /// <returns></returns>
        ValueObject SetEditInformation(ValueObject valueObj)
        {
            return EditInformation = valueObj;
        }


        /// <summary>
        /// 设置isAdding_Interface标志位
        /// </summary>
        /// <param name="value"></param>
        public void SetFlagIsAdding(bool value = true)
        {
            isAdding_Interface = value ? true : false;
        }


        /// <summary>
        /// 通过弹窗来创建接口对象：选择接口类型、输入接口名称，并赋予接口组件
        /// </summary>
        /// <param name="type">接口类型</param>
        /// <param name="name">接口名称</param>
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
        /// 通过弹窗来创建值对象：选择数据类型、输入名称与地址
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
        /// 通过弹窗来编辑值对象：获取数据类型、输入名称与地址等信息。信息量可跟随弹窗内容而增减
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
        /// 接口对象添加方法，需要在所需地方添加置位语句（带撤销功能）
        /// </summary>
        void AddCurrentInterfaceObject()
        {
            if (currentInterfaceObject == null || currentInterfaceObject.interfaceObject== null)
                return;

            InterfaceObject newIobj = new InterfaceObject();

            ///QM.Log(currentInterfaceObject.interfaceObject.ToString());

            //<! 根据给定类型和名称添加接口组件，若接口组件名称为null，则会创建与asset文件中预制体同名的对象，因此需要重新设置此对象的name
            GameObject newInterfaceObj = QM.AddComponent(currentInterfaceObject.interfaceObject, currentInterfaceObject.name);
           // Undo.RecordObject(go, "Add Interface Object");//撤销操作
            currentInterfaceObject.name = newInterfaceObj.name;

            newIobj.name = currentInterfaceObject.name;
            newIobj.type = currentInterfaceObject.type;
            newIobj.interfaceObject = newInterfaceObj;

            List_interfaceObjs.Add(newIobj);//加入列表
            lastInterfaceObject = newIobj;//记录为上一次添加的对象
            SetCurrentInterfaceObject(null);//将当前要添加的对象设为null

        }


        /// <summary>
        /// 根据值对象的datatype来设置type和direction
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
        /// 根据值对象的type和direction来设置datatype
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
        /// 值对象添加方法，需要在所需地方添加置位语句（带撤销功能）
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

            //在指定的对象下创建一个"值对象",若未选中父对象，则创建为根对象
            GameObject newValueObj =  QM.CreateOneValueObj(parent, name, address, type, direction);
            QM.Log($"创建值对象： {newValueObj.name}");

            currentValueObject.name = newValueObj.name;
            currentValueObject.valueObject = newValueObj;
            List_valueObjects.Add(currentValueObject);

            lastValueObject = currentValueObject;//记录为上一次添加的对象
            SetCurrentValueObject(null); //将当前要添加的对象设为null

            return true;
        }


        /// <summary>
        /// 根据targetObj的信息编辑修改editedObj（主要是名称、数据类型、地址）
        /// </summary>
        /// <param name="editedObj"></param>
        /// <param name="targetObj"></param>
        /// <returns></returns>
        bool EditValueObject(ValueObject editedObj, ValueObject targetObj)
        {
            editedObj.name = targetObj.name;
            editedObj.datatype = targetObj.datatype;
            editedObj.address = targetObj.address;

            ///更改对应的gameobject上的Value脚本及其信息
            SetTypeAndDirectionByDataTypeOfValueObj(editedObj);
            QM.EditOneValueObj(editedObj.valueObject, editedObj.name, editedObj.address, editedObj.type, editedObj.direction);

            return true;
        }




        /// <summary>
        /// 移除一个接口对象（带撤销功能）
        /// </summary>
        /// <param name="interfaceObj"></param>
        /// <param name="list"></param>
        void RemoveOneObjectInInterfaceObjectList(InterfaceObject interfaceObj, List<InterfaceObject> list)
        {
            if (list == null || interfaceObj == null || interfaceObj.interfaceObject == null)
                return;

            if (!list.Contains(interfaceObj))//若不包含此元素就直接返回
                return;

            int numberRemove = interfaceObj.number;//要移除的接口对象序号
            list.Remove(interfaceObj);//从列表移除

            Undo.DestroyObjectImmediate(interfaceObj.interfaceObject);//撤销销毁对象操作（*要放在销毁语句前）
            DestroyImmediate(interfaceObj.interfaceObject);//销毁对应的gameobject 

            //更改列表中元素的序号
            list.ForEach(obj => {
                obj.number = (obj.number > numberRemove) ? (obj.number - 1) : obj.number;//若序号在要移除的接口对象序号之后的，减一
            });

        }


        /// <summary>
        /// 删除指定接口对象列表中所有的接口对象
        /// </summary>
        /// <param name="list"></param>
        bool RemoveAllObjectsInInterfaceObjectList(List<InterfaceObject> list)
        {
            if (list == null)
                return false;

            for (int i=0; i<list.Count; i++)
            {
                InterfaceObject obj = list[i];
                list.Remove(obj);//从列表移除
                if (obj != null && obj.interfaceObject != null)
                {
                    Undo.DestroyObjectImmediate(obj.interfaceObject);//撤销销毁对象操作（*要放在销毁语句前）
                    DestroyImmediate(obj.interfaceObject);//销毁对应的gameobject 
                }
            }

            return true;
        }


        /// <summary>
        /// 移除一个值对象（带撤销功能）
        /// </summary>
        /// <param name="valueObj"></param>
        /// <param name="list"></param>
        void RemoveOneObjectInValueObjectList(ValueObject valueObj, List<ValueObject> list)
        {
            if (list == null || valueObj == null || valueObj.valueObject == null)
                return;

            if (!list.Contains(valueObj))//若不包含此元素就直接返回
                return;

            int numberRemove = valueObj.number;//要移除的接口对象序号
            list.Remove(valueObj);//从列表移除

            Undo.DestroyObjectImmediate(valueObj.valueObject);//撤销销毁对象操作（*要放在销毁语句前）
            DestroyImmediate(valueObj.valueObject);//销毁对应的gameobject 

            //更改列表中元素的序号
            list.ForEach(obj => {
                obj.number = (obj.number > numberRemove) ? (obj.number - 1) : obj.number;//若序号在要移除的接口对象序号之后的，减一
            });

        }


        /// <summary>
        /// 删除指定值对象列表中所有的值对象
        /// </summary>
        /// <param name="list"></param>
        bool RemoveAllObjectsInValueObjectList(List<ValueObject> list)
        {
            if (list == null)
                return false;

            for (int i = 0; i < list.Count; i++)
            {
                ValueObject obj = list[i];
                list.Remove(obj);//从列表移除
                if (obj != null && obj.valueObject != null)
                {
                    Undo.DestroyObjectImmediate(obj.valueObject);//撤销销毁对象操作（*要放在销毁语句前）
                    DestroyImmediate(obj.valueObject);//销毁对应的gameobject 
                }
            }

            return true;
        }




        /// <summary>
        /// 复制指定的接口对象
        /// </summary>
        /// <param name="interfaceObj"></param>
        void CopyOneInterfaceObject(InterfaceObject interfaceObj)
        {
            currentInterfaceObject = interfaceObj;
            isAdding_Interface = true;
        }


        /// <summary>
        /// 拷贝所有当前列表中的接口对象（*直接通过创建gameobject的方式，而不是通过设置currentInterfaceObject）
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
                Undo.RecordObject(go, "Add Interface Object");//撤销操作
            }

            return true;//全部创建完成后返回true
        }


        /// <summary>
        /// 复制指定的值对象
        /// </summary>
        /// <param name="valueObj"></param>
        void CopyOneValueObject(ValueObject valueObj)
        {
            currentValueObject = valueObj;
            isAdding_Value = true;
        }


        /// <summary>
        /// 拷贝所有当前列表中的值对象（*直接通过创建gameobject的方式，而不是通过设置currentValueObject）
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
                ////Undo.RecordObject(go, "Add Value Object");//撤销操作
            }

            return true;//全部创建完成后返回true
        }




        /// <summary>
        /// 在场景中选择一个指定接口对象
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
        /// 在场景中选择指定接口对象列表中的所有对象
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
        /// 在场景中选择一个指定值对象
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
        /// 在场景中选择指定值对象列表中的所有对象
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
        /// 重置选择对象为空
        /// </summary>
        void SelectObjectIsNull()
        {
            Selection.objects = null;
        }




        /// <summary>
        /// 获取指定对象上接口组件的类型（类名）
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
        /// 获取指定对象上值对象的类型（类名）
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
        /// 获取指定类型的接口对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        GameObject GetInterfaceObjectOfGameobject(GameObject obj)
        { 
            GameObject go = null;

            List_InterfaceModules.ForEach(module => {
                if (module.script.GetType().ToString().Equals(obj?.GetComponent<Interface.BaseInterface>().GetType().ToString()))//判断当前接口类型是否包含在asset文件中
                    go = obj;
            });
            ///QM.Log(go.name);
            return go;
        }


        /// <summary>
        /// 获取指定类型的值对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        GameObject GetValueObjectOfGameobject(GameObject obj)
        {
            GameObject go = null;

            List<string> List_ValueType = Menu_valueType.ToList();

            List_ValueType.ForEach(type => {
                ///QM.Log(module.script.ToString());
                if (("RDTS.Value" + type).Equals(obj?.GetComponent<Value>().GetType().ToString()))//判断当前值类型是否包含在设定的menu中
                    go = obj;
            });

            return go;
        }


        /// <summary>
        /// 获取值对象的Name变量
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        string GetValueNameOfGameobject(GameObject obj)
        {
            return obj?.GetComponent<Value>().Name;
        }

        /// <summary>
        /// 设置值对象的Name变量
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
        /// 获取值对象的地址信息
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        string GetValueAddressOfGameobject(GameObject obj)
        {
            return obj?.GetComponent<Value>().Address;
        }


        /// <summary>
        /// 根据指定的gameobject设置值对象的相关变量
        /// </summary>
        /// <param name="valueObject"></param>
        /// <param name="obj"></param>
        void SetValueObjectVariablesByGameObject(ValueObject valueObject, GameObject obj)
        {
            if (valueObject == null || obj == null)
                return;


            Value valueScript = obj.GetComponent<Value>();//获取Value脚本
            List<string> List_ValueType = Menu_valueType.ToList();

            List_ValueType.ForEach(type => {
                ///QM.Log(obj?.GetComponent<Value>().GetType().ToString());
                
                if (("RDTS.Value" + type).Equals(valueScript.GetType().ToString()))//判断当前值类型是否包含在设定的menu中
                {
                    valueObject.name = valueScript.Name = obj.name;//将值对象 、Value中的Name变量和其gameobject名称同步
                    valueObject.datatype = type;//设置值对象的数据类型
                    valueObject.address = valueScript.Address;//设置值对象地址
                    valueObject.valueObject = obj;//设置值对象的gameobject
                    valueObject.value = valueScript.GetVisuText();//设置值对象的值
                }
            });
           
        }




        /// <summary>
        /// 浏览当前场景下所有的接口对象，并添加到列表中用于显示【*注意：不是根对象，且处于未激活状态下的gameobject不会被找到】
        /// </summary>
        void BrowseInterfaceGameObjectsInCurrentScene()
        {
            //获取当前场景下所有的接口对象
            List<GameObject> interfaceObjsInScene = new List<GameObject>();
            interfaceObjsInScene = QM.GetGameObjectsInCurrentScene(typeof(Interface.BaseInterface));
            ///interfaceObjsInScene.ForEach(obj => QM.Log(obj.name));
            //更新接口对象列表
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
            //<! 根据排序方式对表中对象进行排列
            switch (SortWay_Interface)
            {
                case InterfaceSortWay.Order://按先后顺序排序
                    //do nothing...
                    break;
                case InterfaceSortWay.Type://按接口类型排序
                    List<InterfaceObject> cacheList_interfaceObjs = new List<InterfaceObject>();///缓存列表，用于按照接口类型的顺序存储接口对象
                    List_InterfaceModules.ForEach(module => {///遍历每一种数据资源中的接口类型
                        newList_interfaceObjs.ForEach(obj => { ///遍历从场景中获取到的所有接口对象
                            if(module.type.Equals(obj.type))///若类型匹配
                            {
                                InterfaceObject newObj = obj;
                                newObj.number = cacheList_interfaceObjs.Count + 1;///重置序号
                                cacheList_interfaceObjs.Add(newObj);///添加至缓存列表
                            }

                        });
                    });
                    newList_interfaceObjs = cacheList_interfaceObjs;

                    break;
            }

            ClearInterfaceObjectList(List_interfaceObjs);//先清空一下列表
            List_interfaceObjs = newList_interfaceObjs;
            Repaint();//<! 用于实时刷新窗口改变内容
            
        }


        /// <summary>
        /// 浏览当前场景下所有的值对象，并添加到列表中用于显示【*注意：不是根对象，且处于未激活状态下的gameobject不会被找到】
        /// </summary>
        void BrowseValueGameObjectsInCurrentScene()
        {
            List<string> List_ValueType = Menu_valueType.ToList();//值类型
            //获取当前场景下所有的值对象
            List<GameObject> valueObjsInScene = new List<GameObject>();
            valueObjsInScene = (currentSelectedInterfaceObj != null)? 
                    QM.GetGameObjectsUnderGivenGameobject(currentSelectedInterfaceObj.interfaceObject, typeof(Value)) : 
                    QM.GetGameObjectsInCurrentScene(typeof(Value));
            //更新值对象列表
            List<ValueObject> newList_valueObjs = new List<ValueObject>();
            valueObjsInScene.ForEach(obj => {
                ValueObject vo = new ValueObject();
                int length = newList_valueObjs.Count;

                vo.number = length + 1;
                ////vo.datatype = GetValueTypeOfGameobject(obj);
                ////vo.name = obj.name; SetValueNameOfGameobject(obj, obj.name);
                ////vo.address = GetValueAddressOfGameobject(obj);
                ////vo.valueObject = GetValueObjectOfGameobject(obj);
                SetValueObjectVariablesByGameObject(vo, obj);///相比于上述四行代码效率更高
                SetTypeAndDirectionByDataTypeOfValueObj(vo);
                vo.parent = currentSelectedInterfaceObj ?? null;

                newList_valueObjs.Add(vo);
            });
            //<! 根据排序方式对表中对象进行排列
            switch (SortWay_Value)
            {
                case InterfaceSortWay.Order://按先后顺序排序
                    //do nothing...
                    break;
                case InterfaceSortWay.Type://按接口类型排序
                    List<ValueObject> cacheList_valueObjs = new List<ValueObject>();///缓存列表，用于按照值类型的顺序存储值对象
                    List_ValueType.ForEach(type => {///遍历每一种值类型menu中的类型
                        newList_valueObjs.ForEach(obj => { ///遍历从场景中获取到的所有值对象
                            if (type.Equals(obj.datatype))///若类型匹配
                            {
                                ValueObject newObj = obj;
                                newObj.number = cacheList_valueObjs.Count + 1;///重置序号
                                cacheList_valueObjs.Add(newObj);///添加至缓存列表
                            }

                        });
                    });
                    newList_valueObjs = cacheList_valueObjs;

                    break;
            }

            ClearValueObjectList(List_valueObjects);//先清空一下列表
            List_valueObjects = newList_valueObjs;
            Repaint();//<! 用于实时刷新窗口改变内容
            ///QM.Log("BrowseObjectsInCurrentScene...");
        }




        /* 值对象格式检测相关方法 *//////////////
        /// <summary>
        /// 对值对象表中的对象进行各项属性(格式)的检测【注意：暂未加入对地址的检测，因为不同接口对于地址的格式不同。后续可以继续优化】
        /// </summary>
        /// <param name="valueObjList">需检测的列表</param>
        void ValueObjectPropertyCheck(List<ValueObject> valueObjList)
        {
            if (valueObjList == null)
                return;

            HashSet<ValueObject> NullName = new HashSet<ValueObject>();//记录空名的对象
            HashSet<ValueObject> SameName = new HashSet<ValueObject>();//记录同名的对象
            HashSet<ValueObject> SameAddress = new HashSet<ValueObject>();//记录同地址的对象
            HashSet<ValueObject> ErrorAddress = new HashSet<ValueObject>();//记录地址格式错误的对象
            HashSet<ValueObject> ExceededAddress = new HashSet<ValueObject>();//记录超出地址的对象

            int length = valueObjList.Count;//获取列表长度   

            for (int i=0; i< length; i++)
            {
                ValueObject currentObj = valueObjList[i];
                //<! 空名检测
                if (currentObj.name == null || currentObj.name == "")
                    NullName.Add(currentObj);

                
                for(int j=i+1; j<length; j++)
                {
                    ValueObject nextObj = valueObjList[j];
                    //<! 同名检测
                    if (currentObj.name == nextObj.name)
                    {
                        SameName.Add(currentObj);
                        SameName.Add(nextObj);
                    }
                    //<! 同址检测
                    if (currentObj.direction == nextObj.direction && currentObj.address == nextObj.address)
                    {
                        SameAddress.Add(currentObj);
                        SameAddress.Add(nextObj);
                    }

                }

            }


            HashSet_errorValueObjects.Clear();//先清空
            //将各个集中的元素合并
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
            //若合并后的集有元素，说明有格式错误的对象
            if (HashSet_errorValueObjects.Count > 0)
                isCheckError = true;
            else
                isCheckError = false;

        }



        #endregion




        #region Import Excel

        /// <summary>
        /// 读取从博途potal中导出的excel表格数据
        /// </summary>
        /// <param name="filePath">excel文件路径</param>
        /// <param name="sheetName">要读取的表单</param>
        public  System.Data.DataSet ReadPortalExcelFile(string filePath)
        {
            return ExcelReader.ReadExcel(filePath);
        }


        /// <summary>
        /// 获取博途excel表中的表单数据
        /// </summary>
        /// <param name="excelResult"></param>
        /// <returns></returns>
        public  System.Data.DataTableCollection ReadPortalExcelFileTables(System.Data.DataSet excelResult)
        {
            return excelResult.Tables;
        }


        /// <summary>
        /// 按行读取博途excel表中表单的数据
        /// </summary>
        /// <param name="excelResult"></param>
        /// <param name="sheetName"></param>
        public void ReadPortalExcelFileRows(System.Data.DataSet excelResult, string sheetName)
        {
            var excelRowData = excelResult.Tables[sheetName].Rows;


            for (int i = 1; i < excelRowData.Count; i++)
            {
                ValueObject readObj = new ValueObject();
                readObj.number = List_valueObjects.Count + 1;//设置序号
                readObj.name = excelRowData[i][0].ToString();//设置名称

                string datatType = excelRowData[i][2].ToString();///“Bool Int Real”
                string logicalAddress = excelRowData[i][3].ToString();///如“%I5.0”
                string addressType = logicalAddress[1].ToString();///“I Q M”
                string addressValue = logicalAddress.Remove(0, 2);///如“5.0”

                readObj.address = addressType + addressValue;//设置地址
                //设置type
                switch (datatType)
                {
                    case "Bool": readObj.type = VALUETYPE.BOOL; break;
                    case "Int": readObj.type = VALUETYPE.INT; break;
                    case "Real": readObj.type = VALUETYPE.REAL; break;
                }
                //设置direction
                switch (addressType)
                {
                    case "I": readObj.direction = VALUEDIRECTION.INPUT; break;
                    case "Q": readObj.direction = VALUEDIRECTION.OUTPUT; break;
                    case "M": readObj.direction = VALUEDIRECTION.INPUTOUTPUT; break;
                }

                SetDataTypeByTypeAndDirectionOfValueObj(readObj);//设置datatype
                readObj.parent = currentSelectedInterfaceObj ?? null;//设置parent

                GameObject go = QM.CreateOneValueObj(readObj.parent?.interfaceObject, readObj.name, readObj.address, readObj.type, readObj.direction);
                readObj.valueObject = go;//设置valueobject

            }
        }



        #endregion


        #region  EditorCoroutine


        void StartEditorCoroutine(string methodName)
        {
            this.StartCoroutine(methodName);
            QM.Log("InterfaceLibraryWindow — 开启协程");
            isCoroutineStart = true;
        }


        void StopEditorCoroutine(string methodName)
        {
            this.StopCoroutine(methodName);//关闭协程
            QM.Log("InterfaceLibraryWindow — 关闭协程");
            isCoroutineStart = false;
        }



        /// <summary>
        /// 协程方法：用于定时调用浏览方法，避免OnGUI中多次调用浏览方法而导致Layout/Repaint报错等情况
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        IEnumerator BrowseObjectsInCurrentScene()
        {
            for (; ; )
            {
                BrowseInterfaceGameObjectsInCurrentScene();
                BrowseValueGameObjectsInCurrentScene();
                yield return new WaitForSeconds(.1f);//延迟时间
                //QM.Log("BrowseObjectsInCurrentScene...");
            }
        }




        #endregion


    }

#endif
}