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

        ///窗口显示/浏览模式
        protected enum ShowMode
        {
            BriefMode,//简要显示模式
            DetailedMode//详细显示模式
        }
        protected ShowMode showMode = ShowMode.BriefMode;








        [MenuItem("Parallel-RDTS/Window/ScriptLibraryWindow", false, 200)]
        private static void ShowWindow()
        {
            ScriptLibraryWindow scriptWindow = (ScriptLibraryWindow)EditorWindow.GetWindow(typeof(ScriptLibraryWindow));
            scriptWindow.titleContent = ScriptLibraryWindow.thisTitleContent;//设置标题和图标
            scriptWindow.minSize = new Vector2(300, 200);
            scriptWindow.Show();
        }


        private void OnGUI()
        {

            if (HasOpenInstances<ScriptLibraryWindow>())//脚本库窗口是否打开
            {
                WindowWidth = position.width;//获取窗口宽度
                WindowHeight = position.height;//获取窗口高度

                DrawToolbar();//顶部工具栏

                if (viewIndex == 0)//索引号为0，则绘制欢迎界面
                {
                    WelcomeView();
                }
                else
                {
                    GUILayout.BeginHorizontal();//*
                    DrawScriptMenu(AreaMenu.x, AreaMenu.y, AreaMenu.width, AreaMenu.height);//绘制脚本类型菜单
                    DrawResizer1(AreaResizer1.x, AreaResizer1.y, AreaResizer1.width, AreaResizer1.height);//绘制调整条1
                    DrawScriptContent(AreaContent.x, AreaContent.y, AreaContent.width, AreaContent.height);//绘制脚本库主体内容
                    if (showMode == ShowMode.DetailedMode)
                    {
                        DrawResizer2(AreaResizer2.x, AreaResizer2.y, AreaResizer2.width, AreaResizer2.height);//绘制调整条2
                        DrawScriptIntroduction(AreaIntroduction.x + 10, AreaIntroduction.y, AreaIntroduction.width, AreaIntroduction.height);//绘制脚本介绍内容
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
        static GUIContent thisTitleContent;//窗口名称
        void OnEnable()
        {
            //数据资源读取
            scriptLibraryData = AssetDatabase.LoadAssetAtPath<ScriptLibraryData>(dataPath + "ScriptLibraryData.asset");
            scriptLibraries = ReadLibraryData(scriptLibraryData);
            //设置窗口标题及图标
            Texture titleIcon = AssetDatabase.LoadAssetAtPath<Texture>(iconPath + "Parallel.png");
            thisTitleContent = new GUIContent("Parallel脚本库", titleIcon);
            //加载unity自带图标
            LoadTexture();
            //初始化
            showMode = ShowMode.BriefMode;
            isLockIntroduction = false;
        }


        private GUISkin skin_scriptLibrary;
        private GUIStyle labelStyle_MiddleCenter;//中心标签UI样式
        private GUIStyle labelStyle_MiddleLeft;//中心标签UI样式
        private GUIStyle resizerStyle;

        private Texture image_Script;

        private Texture2D icon_refresh;
        private Texture2D icon_lock;
        private Texture2D icon_help;
        private Texture2D icon_close;
        private Texture2D icon_eye;
        private Texture2D icon_add;

        void LoadTexture()//加载素材
        {
            skin_scriptLibrary = AssetDatabase.LoadAssetAtPath<GUISkin>("Assets/RDTS/Scripts/Window/Editor/ScriptLibraryWindow/WindowScriptLibrary.guiskin");
            labelStyle_MiddleCenter = skin_scriptLibrary.customStyles[3];
            labelStyle_MiddleLeft = skin_scriptLibrary.customStyles[4];

            image_Script = AssetDatabase.LoadAssetAtPath<Texture>(iconPath + "WindowScriptIcon.png");

            //Style
            resizerStyle = new GUIStyle();
            resizerStyle.normal.background = EditorGUIUtility.Load("icons/d_AvatarBlendBackground.png") as Texture2D;//默认的Unity编辑器纹理(所有图标)

            //图标
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

        private float resizerWidth1 = 7f;//调整条1宽度（菜单栏和主体内容之间）
        private float resizerWidth2 = 7f;//调整条2宽度（主体内容和介绍内容之间）

        private float menuWidthRatio = 0.15f;//菜单栏占窗口宽度比例
        private float contentWidthRatio = 0.8f;//库中主体内容右边缘占窗口宽度比例（即菜单栏+调整条1+主体内容的宽度占窗口宽度的比例）

        /// <summary>
        /// 设置区域宽度(主要改变Rect起始点和宽度)
        /// </summary>
        void SetAreaRectParam()
        {

            if (showMode == ShowMode.BriefMode)
            {
                //菜单区域的Rect参数
                AreaMenu.x = 0;//*
                AreaMenu.y = 0 + ToolbarHeight;
                AreaMenu.width = WindowWidth * menuWidthRatio;//*
                AreaMenu.height = WindowHeight - ToolbarHeight;

                //调整条1的Rect参数
                AreaResizer1.x = AreaMenu.width;//*
                AreaResizer1.y = 0 + ToolbarHeight;
                AreaResizer1.width = resizerWidth1;//*
                AreaResizer1.height = WindowHeight - ToolbarHeight;

                //主体内容区域的Rect参数
                AreaContent.x = AreaMenu.width + AreaResizer1.width;//*
                AreaContent.y = 0 + ToolbarHeight;
                AreaContent.width = WindowWidth * (1 - menuWidthRatio) - AreaResizer1.width;//*
                AreaContent.height = WindowHeight - ToolbarHeight;

            }
            else
            {
                //菜单区域的Rect参数
                AreaMenu.x = 0;//*
                AreaMenu.y = 0 + ToolbarHeight;
                AreaMenu.width = WindowWidth * menuWidthRatio;//*
                AreaMenu.height = WindowHeight - ToolbarHeight;

                //调整条1的Rect参数
                AreaResizer1.x = AreaMenu.width;//*
                AreaResizer1.y = 0 + ToolbarHeight;
                AreaResizer1.width = resizerWidth1;//*
                AreaResizer1.height = WindowHeight - ToolbarHeight;

                //主体内容区域的Rect参数
                AreaContent.x = AreaMenu.width + AreaResizer1.width;//*
                AreaContent.y = 0 + ToolbarHeight;
                AreaContent.width = WindowWidth * contentWidthRatio - AreaMenu.width - AreaResizer1.width;//*
                AreaContent.height = WindowHeight - ToolbarHeight;

                //调整条2的Rect参数
                AreaResizer2.x = AreaMenu.width + AreaResizer1.width + AreaContent.width;//*
                AreaResizer2.y = 0 + ToolbarHeight;
                AreaResizer2.width = resizerWidth2;//*
                AreaResizer2.height = WindowHeight - ToolbarHeight;

                //介绍内容区域的Rect参数
                AreaIntroduction.x = AreaMenu.width + AreaResizer1.width + AreaContent.width + AreaResizer2.width;//*
                AreaIntroduction.y = 0 + ToolbarHeight;
                AreaIntroduction.width = WindowWidth * (1 - contentWidthRatio);//*
                AreaIntroduction.height = WindowHeight - ToolbarHeight;

            }


        }



        #endregion


        #region ViewContent

        int viewIndex = 0;//视图索引号
        float WindowWidth; //<! 窗口宽度
        float WindowHeight;//<! 窗口高度

        float ToolbarHeight = 20f;//工具栏高度



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
        /// 顶部工具栏绘制
        /// </summary>
        void DrawToolbar()
        {

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            //重置窗口内容为起始的默认内容
            if (GUILayout.Button(libraryViewName, EditorStyles.toolbarButton, GUILayout.Width(100), GUILayout.Height(ToolbarHeight)))
            {
                viewIndex = (viewIndex == 0) ? 1 : 0;
                libraryViewName = (viewIndex == 0) ? "Welcome" : "Script";
            }
            //设置：是否实时刷新浏览窗口内容
            if (GUILayout.Button("重置", EditorStyles.toolbarButton, GUILayout.Width(50), GUILayout.Height(ToolbarHeight)))
            {
                menuWidthRatio = 0.15f;
                contentWidthRatio = 0.8f;
                QM.Log("重置置窗口属性");
            }
            //保存按钮—保存当前窗口内容的修改，并更新至asset
            if (GUILayout.Button("保存", EditorStyles.toolbarButton, GUILayout.Width(50), GUILayout.Height(ToolbarHeight)))
            {
                Debug.Log("保存");
            }
            //刷新按钮—当DataAsset中修改时，重新读取并重绘窗口
            if (GUILayout.Button(icon_refresh, EditorStyles.toolbarButton, GUILayout.Width(50), GUILayout.Height(ToolbarHeight)))
            {
                RedoWindowContent();
                ShowNotification(new GUIContent("窗口内容已刷新"));
                ///Debug.Log("刷新");
            }

            GUILayout.FlexibleSpace();//灵活填充

            //是否锁住Introduction界面
            isLockIntroduction = GUILayout.Toggle(isLockIntroduction, new GUIContent("Introduction", icon_lock), EditorStyles.toolbarButton, GUILayout.Width(100), GUILayout.Height(ToolbarHeight));

            //帮助按钮—提示如何操作该设备库
            if (GUILayout.Button(icon_help, EditorStyles.toolbarButton, GUILayout.Width(24), GUILayout.Height(ToolbarHeight)))
            {
                //打开弹窗
                PopWindowHelp helpWindow = (PopWindowHelp)EditorWindow.GetWindow(
                       typeof(PopWindowHelp),
                       true,
                       "Help");
                var content = helpWindow.ReadHelpData(2);
                helpWindow.SetShowContents(content);
                helpWindow.ShowUtility();
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




        private Rect Rect_menu;
        Vector2 _scrollview1;
        private float menuItemWidth => AreaMenu.width - 10;
        private float menuItemHeight = 50f;
        /// <summary>
        /// 绘制左侧的类型菜单栏
        /// </summary>
        void DrawScriptMenu(float x, float y, float width, float height)
        {
            Rect_menu = new Rect(x, y, width, height);
            GUILayout.BeginArea(Rect_menu);
            _scrollview1 = GUILayout.BeginScrollView(_scrollview1);//<! 滑动条

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



            GUILayout.EndScrollView();//<! 滑动条
            GUILayout.EndArea();
        }



        private Rect Rect_content;
        Vector2 _scrollview2;
        protected int horizontalSize = 2;//每行绘制的设备数量
        /// <summary>
        /// 绘制脚本库主体内容
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        void DrawScriptContent(float x, float y, float width, float height)
        {
            Rect_content = new Rect(x, y, width, height);
            GUILayout.BeginArea(Rect_content);
            _scrollview2 = GUILayout.BeginScrollView(_scrollview2);//<! 滑动条

            if (viewIndex > 0)
            {
                ScriptLibraryContent(scriptLibraries, horizontalSize, viewIndex - 1);//脚本内容绘制
            }

            GUILayout.EndScrollView();//<! 滑动条
            GUILayout.EndArea();
        }


        private Rect Rect_introduction;
        Vector2 _scrollview3;
        private Script _currentScript;//当前介绍的脚本
        /// <summary>
        /// 脚本介绍内容
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        void DrawScriptIntroduction(float x, float y, float width, float height)
        {
            Rect_introduction = new Rect(x, y, width, height);
            GUILayout.BeginArea(Rect_introduction);
            _scrollview3 = GUILayout.BeginScrollView(_scrollview3);//<! 滑动条

            ScriptIntroduction(_currentScript);


            GUILayout.EndScrollView();//<! 滑动条
            GUILayout.EndArea();
        }



        private Rect Rect_resizer1;
        /// <summary>
        /// 宽度调整条1
        /// </summary>
        void DrawResizer1(float x, float y, float width, float height)
        {
            Rect_resizer1 = new Rect(x, y, width, height);
            GUILayout.BeginArea(new Rect(Rect_resizer1.position + (Vector2.right * resizerWidth1), new Vector2(4, height * 3)), resizerStyle);
            GUILayout.EndArea();

            EditorGUIUtility.AddCursorRect(Rect_resizer1, MouseCursor.ResizeHorizontal);//改变鼠标形状：显示鼠标指针位于调整大小位置
        }


        private Rect Rect_resizer2;
        /// <summary>
        /// 宽度调整条2
        /// </summary>
        void DrawResizer2(float x, float y, float width, float height)
        {
            Rect_resizer2 = new Rect(x, y, width, height);
            GUILayout.BeginArea(new Rect(Rect_resizer2.position + (Vector2.right * resizerWidth2), new Vector2(2, height * 3)), resizerStyle);
            GUILayout.EndArea();

            EditorGUIUtility.AddCursorRect(Rect_resizer2, MouseCursor.ResizeHorizontal);//改变鼠标形状：显示鼠标指针位于调整大小位置
        }



        private float MaxScriptModuleWidth = 500f;
        private float MInScriptModuleWidth = 300f;
        private float scriptModuleWidth = 350f;//一个脚本内容总宽度
        private float scriptModuleHeight = 36;//一个脚本内容总宽度
        private float scriptIconWidth = 32f;//脚本图标宽度

        private List<string> RDTSNamespace = new List<string>() { "RDTS.", "RDTS.Utility." };

        void OneScriptContent(Script script)
        {

            if (script == null)
                return;

            EditorGUILayout.BeginHorizontal("Box", GUILayout.Width(AreaContent.width / 2 - 10), GUILayout.Height(scriptModuleHeight));
            GUILayout.Space(5);

            //脚本图标
            GUILayout.Label(script.icon, labelStyle_MiddleCenter, GUILayout.Width(scriptIconWidth), GUILayout.Height(scriptIconWidth));
            GUILayout.Space(10);
            //脚本名称
            GUILayout.Label(script.name, labelStyle_MiddleLeft, GUILayout.ExpandHeight(true));
            GUILayout.FlexibleSpace();
            //按钮-介绍
            if (GUILayout.Button(icon_eye, GUILayout.Width(40), GUILayout.Height(scriptModuleHeight)))
            {
                _currentScript = script;
            }
            //按钮-添加
            if (GUILayout.Button(icon_add, GUILayout.Width(40), GUILayout.Height(scriptModuleHeight)))
            {
                try
                {
                    Type type;
                    for (int i = 0; i < RDTSNamespace.Count; i++)
                    {
                        string typeString = RDTSNamespace[i] + script.name;//需要加上命名空间
                        type = Type.GetType(typeString);//具有指定名称的类型（如果找到的话）,否则为 null
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

            int rows = 0;//(完整)行数
            int columns = 0;//（不满足完整一行时，此行）列的个数
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

            GUILayout.Label("Name：");//名称
            GUILayout.TextArea(script.name, skin_scriptLibrary.textArea, GUILayout.Width(AreaIntroduction.width - 30));
            GUILayout.Space(5);
            GUILayout.Label("Function：");//
            GUILayout.TextArea(script.introduction.function, skin_scriptLibrary.textArea, GUILayout.Width(AreaIntroduction.width - 30));
            GUILayout.Space(5);
            GUILayout.Label("Apply：");//
            GUILayout.TextArea(script.introduction.apply, skin_scriptLibrary.textArea, GUILayout.Width(AreaIntroduction.width - 30));
            GUILayout.Space(5);
            GUILayout.Label("Notice：");//注意
            GUILayout.TextArea(script.introduction.notice, skin_scriptLibrary.textArea, GUILayout.Width(AreaIntroduction.width - 30));

        }









        #endregion



        #region Methods

        private bool isResizing;//是否在调整区域尺寸
        private bool isResizingWidth1;//是否在调整第一条的区域尺寸
        private bool isResizingWidth2;//是否在调整第二条的区域尺寸
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

                        isResizingWidth1 = (Rect_resizer1.Contains(e.mousePosition)) ? true : false;//判断是否在调整高度

                        isResizingWidth2 = (Rect_resizer2.Contains(e.mousePosition)) ? true : false;//判断是否在调整宽度

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

                //调整条1
                if (isResizingWidth1 && e.mousePosition.x > WindowWidth * 0.1 && e.mousePosition.x < WindowWidth * 0.3)
                {
                    menuWidthRatio = e.mousePosition.x / WindowWidth;
                }
                //调整条2
                if (isResizingWidth2 && e.mousePosition.x > WindowWidth * 0.4 && e.mousePosition.x < WindowWidth * 0.9)
                {
                    contentWidthRatio = e.mousePosition.x / WindowWidth;
                }

                // QM.Log("widthSizeRatio：" + widthSizeRatio + " " + "heightSizeRatio: " + heightSizeRatio);

                Repaint();
            }
        }


        /// <summary>
        /// 是否显示Introduction界面
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


        #region 读取库资源数据

        List<ScriptLibrary> scriptLibraries = new List<ScriptLibrary>();//脚本库数据
        List<string> menuItem = new List<string>();//记录菜单栏
        int number_ItemType = 0;//种类个数
        int number_Item = 0;//脚本数量

        List<ScriptLibrary> ReadLibraryData(ScriptLibraryData data)
        {
            List<ScriptLibrary> scriptLibraries = new List<ScriptLibrary>();
            scriptLibraries = data.ScriptLibraryDataList;//获取数据
            number_ItemType = scriptLibraries.Count;//获取脚本类型个数

            menuItem.Clear();
            scriptLibraries.ForEach(sl =>
            {
                if (!menuItem.Contains(sl.name))
                    menuItem.Add(sl.name);//储存脚本类型

                number_Item += sl.scripts.Count;//一共多少脚本

            });


            return scriptLibraries;
        }


        /// <summary>
        /// 刷新当前窗口的内容
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
