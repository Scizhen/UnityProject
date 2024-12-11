using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using RDTS.Method;


namespace RDTS.Utility.Editor
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


    public class RobotOperationPanel : EditorWindow
    {
        public class RobotItem
        {
            public int ID;//标识符
            public string name;//名称
            public GameObject gameobject;//对应的对象
            public List<Drive> axisDrives = new List<Drive>();//对应的Drive脚本
            public List<float> axisValues = new List<float>();//对应的关节轴数值
            public Dictionary<int, float> axisValues_Record = new Dictionary<int, float>();//记录的关节轴角度

        }

        protected int RobotItemCount = 0;//机器人对象个数
        protected Dictionary<int, RobotItem> RobotItemPerID = new Dictionary<int, RobotItem>();//存储机器人对象信息
        protected List<string> RobotNames = new List<string>();//记录机器人名称
        protected int CurrentRobotID = 0;//当前操作的机器人ID
        protected List<float> CurrentAxis = new List<float>();//当前操作的关节轴

        float WindowWidth; //<! 窗口宽度
        float WindowHeight;//<! 窗口高度

        [MenuItem("Parallel-RDTS/Utility/Robot Operation Panel", false, 400)]
        static void ShowOperationPanel()
        {
            // Get existing open window or if none, make a new one:
            RobotOperationPanel window = (RobotOperationPanel)EditorWindow.GetWindow(typeof(RobotOperationPanel));
            window.titleContent = RobotOperationPanel.thisTitleContent;//设置标题和图标
            window.minSize = new Vector2(200, 350);
            window.Show();


        }

        string iconPath = "Assets/RDTS/Private/Resources/Icons/WindowIcon/";
        static GUIContent thisTitleContent;

        private GUIStyle resizerStyle;

        private Texture2D icon_refresh;
        private Texture2D icon_help;
        private Texture2D icon_close;
        private Texture2D icon_delete;

        void OnEnable()
        {
            //初始化工作
            Init();

            //设置窗口标题及图标
            Texture titleIcon = AssetDatabase.LoadAssetAtPath<Texture>(iconPath + "Parallel.png");
            thisTitleContent = new GUIContent("机器人操作面板", titleIcon);
           
            //加载素材
            LoadTexture();

          
        }


        void Init()
        {
            SearchRobotItem();
          
            if (RobotItemPerID.Count != 0)
            {
                CurrentRobotID = 1;
                CurrentAxis = RobotItemPerID[0].axisValues;//设置初始对象的关节轴角度值

            }
            else
            {
                CurrentRobotID = 0;
                CurrentAxis.Clear();
            }

            QM.Log($"CurrentRobotID {CurrentRobotID}; CurrentAxis.Count {CurrentAxis.Count}; RobotItemPerID.Count {RobotItemPerID.Count};");
            QM.Log($"robot name: {RobotItemPerID[0].name}");
                
        }


        void LoadTexture()
        {
            //Style
            resizerStyle = new GUIStyle();
            resizerStyle.normal.background = EditorGUIUtility.Load("icons/d_AvatarBlendBackground.png") as Texture2D;//默认的Unity编辑器纹理(所有图标)


            icon_refresh = EditorGUIUtility.FindTexture("Refresh");
            icon_help = EditorGUIUtility.FindTexture("d__Help@2x");
            icon_close = icon_delete = EditorGUIUtility.FindTexture("d_winbtn_win_close");
        }



        void OnGUI()
        {
            if (HasOpenInstances<RobotOperationPanel>())//窗口是否打开
            {
                WindowWidth = position.width;//获取窗口宽度
                WindowHeight = position.height;//获取窗口高度

                DrawToolbar();//顶部工具栏

                GUILayout.BeginHorizontal();//*
                DrawItemMenu(AreaItemMenu.x, AreaItemMenu.y, AreaItemMenu.width, AreaItemMenu.height);//绘制机器人项目菜单
                DrawResizer1(AreaResizer1.x, AreaResizer1.y, AreaResizer1.width, AreaResizer1.height);//绘制调整条1
                DrawItemContent(AreaItemContent.x, AreaItemContent.y, AreaItemContent.width, AreaItemContent.height);//绘制机器人项目主体内容
                GUILayout.EndHorizontal();//*

                SetAreaRectParam();//区域参数设置
                ProcessEvents(Event.current);
                if (CurrentRobotID != 0 && CurrentAxis.Count != 0 && RobotItemPerID.Count != 0)
                {

                    for (int i = 0; i < CurrentAxis.Count; i++)
                    {
                        RobotItemPerID[CurrentRobotID - 1].axisDrives[i].CurrentPosition =
                            RobotItemPerID[CurrentRobotID - 1].axisValues[i] = CurrentAxis[i];

                    }

                }
            }

        }


        void OnFocus()
        {
         



        }







        #region Toolbar

        float ToolbarHeight = 20f;
        bool isSync = false;

        /// <summary>窗口菜单工具栏</summary>
        void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            //刷新：重新获取场景中的机器人对象
            if (GUILayout.Button(icon_refresh, EditorStyles.toolbarButton, GUILayout.Width(50), GUILayout.Height(ToolbarHeight)))
            {
                Init();
            }
            //是否锁住Introduction界面
            isSync = GUILayout.Toggle(isSync, new GUIContent("Synchronize"), EditorStyles.toolbarButton, GUILayout.Width(100), GUILayout.Height(ToolbarHeight));


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

        #endregion


        #region ViewContent



        void OneRobotItemContent(string robotName)
        {
            EditorGUILayout.BeginVertical("Box");

          






            EditorGUILayout.EndVertical();
        }





        private Rect Rect_menu;
        Vector2 _scrollview1;
        private float menuItemWidth => AreaItemMenu.width - 10;
        private float menuItemHeight = 50f;
        /// <summary>
        /// 绘制左侧的类型菜单栏
        /// </summary>
        void DrawItemMenu(float x, float y, float width, float height)
        {
            Rect_menu = new Rect(x, y, width, height);
            GUILayout.BeginArea(Rect_menu);
            _scrollview1 = GUILayout.BeginScrollView(_scrollview1);//<! 滑动条

            GUILayout.Space(7);
            for (int i = 0; i < RobotNames.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(7);
                string typeName = RobotNames[i];
                if (GUILayout.Button(typeName, GUILayout.Width(menuItemWidth), GUILayout.Height(menuItemHeight)))
                {
                    SelectGameObjectInScene(RobotItemPerID[i]?.gameobject);//选中此对象
                    CurrentRobotID = RobotItemPerID[i].ID;//选中对象的ID
                    CurrentAxis = RobotItemPerID[i].axisValues;//选中对象的关节轴角度值
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
        /// 绘制机器人项目内容
        /// </summary>
        void DrawItemContent(float x, float y, float width, float height)
        {
            Rect_content = new Rect(x, y, width, height);
            GUILayout.BeginArea(Rect_content);
            _scrollview2 = GUILayout.BeginScrollView(_scrollview2);//<! 滑动条

            //显示当前机器人名称
            GUILayout.BeginHorizontal();
            GUILayout.Space(7);
            string name = (CurrentRobotID != 0) ? RobotItemPerID[CurrentRobotID - 1].name : "none";
            GUILayout.Box($"Name：{name}", GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            for(int i=0; i< CurrentAxis.Count; i++)
            {
                CurrentAxis[i] = DrawOneAxis(i, CurrentAxis[i]);
                GUILayout.Space(10);
            }
            


            GUILayout.EndScrollView();//<! 滑动条
            GUILayout.EndArea();
        }


        float DrawOneAxis(int index,float value)
        {
            float axisValue = value;
            //机器人关节轴操作
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(7);
            GUILayout.Label($"Axis{index+1}：");//缩放大小
            axisValue = EditorGUILayout.Slider(axisValue, -180f, 180f);//<! 拖动条
            EditorGUILayout.EndHorizontal();


            return axisValue;
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







        #endregion


        #region AreaParameter

        private AreaRectParam AreaItemMenu = new AreaRectParam();
        private AreaRectParam AreaItemContent = new AreaRectParam();
        private AreaRectParam AreaResizer1 = new AreaRectParam();

        private float resizerWidth1 = 7f;//调整条1宽度（菜单栏和主体内容之间）
        private float menuWidthRatio = 0.15f;//菜单栏占窗口宽度比例


        void SetAreaRectParam()
        {
            //菜单区域的Rect参数
            AreaItemMenu.x = 0;//*
            AreaItemMenu.y = 0 + ToolbarHeight;
            AreaItemMenu.width = WindowWidth * menuWidthRatio;//*
            AreaItemMenu.height = WindowHeight - ToolbarHeight;

            //调整条1的Rect参数
            AreaResizer1.x = AreaItemMenu.width;//*
            AreaResizer1.y = 0 + ToolbarHeight;
            AreaResizer1.width = resizerWidth1;//*
            AreaResizer1.height = WindowHeight - ToolbarHeight;

            //主体内容区域的Rect参数
            AreaItemContent.x = AreaItemMenu.width + AreaResizer1.width;//*
            AreaItemContent.y = 0 + ToolbarHeight;
            AreaItemContent.width = WindowWidth * (1 - menuWidthRatio) - AreaResizer1.width;//*
            AreaItemContent.height = WindowHeight - ToolbarHeight;


        }


        private bool isResizing;//是否在调整区域尺寸
        private bool isResizingWidth1;//是否在调整第一条的区域尺寸
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
               

                // QM.Log("widthSizeRatio：" + widthSizeRatio + " " + "heightSizeRatio: " + heightSizeRatio);

                Repaint();
            }
        }


        #endregion



        #region Method


        void SearchRobotItem()
        {
           
            var modelList =  QM.GetGameObjectsInCurrentScene(typeof(TwinModel));
            if(modelList.Count != 0)
            {
                SelectObjectIsNull();
                RobotItemPerID.Clear();
                RobotNames.Clear();
                RobotItemCount = 0;

                modelList.ForEach(m => {
                    var script_TwinModel = m.GetComponent<TwinModel>();
                    if (script_TwinModel.Type == TwinModelType.RobotArm)
                    {
                        RobotItemCount++;
                        RobotItem newItem = new RobotItem();
                        newItem.ID = RobotItemCount;
                        newItem.name = m.name;
                        newItem.gameobject = m;
                        newItem.axisDrives = script_TwinModel.RobotAxis;
                        script_TwinModel.RobotAxis.ForEach(axis => {
                            newItem.axisValues.Add(axis.CurrentPosition);
                        });

                        RobotItemPerID[RobotItemCount - 1] = newItem;
                        RobotNames.Add(newItem.name);

                        QM.Log(newItem.name);
                    }

                });
            }
           

            QM.Log("搜索到的机器人数量：" + RobotItemCount.ToString());
        }


        /// <summary>
        /// 在场景中选择一个指定对象
        /// </summary>
        /// <param name="valueObj"></param>
        void SelectGameObjectInScene(GameObject go)
        {
            if (go == null)
                return;

            List<UnityEngine.Object> select = new List<UnityEngine.Object>();
            select.Add(go);
            Selection.objects = select.ToArray();
        }

        /// <summary>
        /// 重置选择对象为空
        /// </summary>
        void SelectObjectIsNull()
        {
            Selection.objects = null;
        }






        #endregion



















    }

#endif
}
