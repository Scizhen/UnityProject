///********************************************************************************************
///this is part of Robot Digital Twin System.                                                 *
///@Copyright by Shaw.S                                                                       *
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

    public class ColliderLibraryWindow : EditorWindow
    {

        [MenuItem("Parallel-RDTS/Window/ColliderLibraryWindow", false, 200)]
        private static void ShowWindow()
        {
            ColliderLibraryWindow colliderWindow = (ColliderLibraryWindow)EditorWindow.GetWindow(typeof(ColliderLibraryWindow));
            colliderWindow.titleContent = ColliderLibraryWindow.thisTitleContent;//设置标题和图标
            colliderWindow.minSize = new Vector2(300, 200);
            colliderWindow.Show();
        }

        private void OnGUI()
        {

            if (HasOpenInstances<ColliderLibraryWindow>())//碰撞体库窗口是否打开
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
                    DrawColliderMenu(AreaMenu.x, AreaMenu.y, AreaMenu.width, AreaMenu.height);//绘制碰撞体类型菜单
                    DrawResizer1(AreaResizer1.x, AreaResizer1.y, AreaResizer1.width, AreaResizer1.height);//绘制调整条1
                    DrawColliderContent(AreaContent.x, AreaContent.y, AreaContent.width, AreaContent.height);//绘制碰撞体库主体内容
                    DrawResizer2(AreaResizer3.x, AreaResizer3.y, AreaResizer3.width, AreaResizer3.height);//绘制调整条1
                    DrawColliderControl(AreaControl.x, AreaControl.y, AreaControl.width, AreaControl.height);//绘制控制栏

                    GUILayout.EndHorizontal();//*

                    SetAreaRectParam();
                    ProcessEvents(Event.current);
                }

            }


        }


        #region Init

        string dataPath = "Assets/RDTS/Data/";
        string iconPath = "Assets/RDTS/Private/Resources/Icons/WindowIcon/";
        ColliderLibraryData colliderLibraryData;
        static GUIContent thisTitleContent;//窗口名称
        void OnEnable()
        {
            //数据资源读取
            colliderLibraryData = AssetDatabase.LoadAssetAtPath<ColliderLibraryData>(dataPath + "ColliderLibraryData.asset");
            colliderLibraries = ReadLibraryData(colliderLibraryData);
            //设置窗口标题及图标
            Texture titleIcon = AssetDatabase.LoadAssetAtPath<Texture>(iconPath + "Parallel.png");
            thisTitleContent = new GUIContent("工厂物流碰撞体库", titleIcon);
            //加载unity自带图标
            LoadTexture();
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
        private Texture2D icon_delete;
        private Texture2D icon_pick;

        void LoadTexture()//加载素材
        {
            skin_scriptLibrary = AssetDatabase.LoadAssetAtPath<GUISkin>("Assets/RDTS/Scripts/Window/Editor/ColliderWindow/WindowColliderLibrary.guiskin");
            labelStyle_MiddleCenter = skin_scriptLibrary.customStyles[3];
            labelStyle_MiddleLeft = skin_scriptLibrary.customStyles[4];

            image_Script = AssetDatabase.LoadAssetAtPath<Texture>(iconPath + "WindowColliderIcon.png");

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
            icon_delete = EditorGUIUtility.FindTexture("d_winbtn_win_close");
            icon_pick = EditorGUIUtility.FindTexture("d_scenepicking_pickable-mixed");
        }



        #endregion


        #region AreaParameter

        private AreaRectParam AreaMenu = new AreaRectParam();
        private AreaRectParam AreaContent = new AreaRectParam();
        private AreaRectParam AreaIntroduction = new AreaRectParam();
        private AreaRectParam AreaControl = new AreaRectParam();
        private AreaRectParam AreaResizer1 = new AreaRectParam();
        private AreaRectParam AreaResizer2 = new AreaRectParam();
        private AreaRectParam AreaResizer3 = new AreaRectParam();

        private float resizerWidth1 = 7f;//调整条1宽度（菜单栏和主体内容之间）
        private float resizerWidth2 = 7f;//调整条2宽度（主体内容和介绍内容之间）
        private float resizerWidth3 = 7f;//调整条2宽度（主体内容和介绍内容之间）

        private float menuWidthRatio = 0.10f;//菜单栏占窗口宽度比例
        private float areacontentWidthRatio = 0.7f;//菜单栏占窗口宽度比例

        /// <summary>
        /// 设置区域宽度(主要改变Rect起始点和宽度)
        /// </summary>
        void SetAreaRectParam()
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
                AreaContent.width = WindowWidth * areacontentWidthRatio;//*
                AreaContent.height = WindowHeight - ToolbarHeight;

                //调整条3的Rect参数
                AreaResizer3.x = AreaMenu.width + AreaResizer1.width + AreaContent.width;
                AreaResizer3.y = 0 + ToolbarHeight;
                AreaResizer3.width = resizerWidth1;
                AreaResizer3.height = WindowHeight - ToolbarHeight;


                AreaControl.x = AreaMenu.width + AreaResizer1.width + AreaContent.width + AreaResizer3.width;
                AreaControl.y = 0 + ToolbarHeight;
                AreaControl.width = WindowWidth *( 1-menuWidthRatio - areacontentWidthRatio)- AreaResizer1.width - AreaResizer3.width;
                AreaControl.height = WindowHeight - ToolbarHeight;

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
            GUILayout.Label("This is an collider library.", skin_scriptLibrary.label, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
            GUILayout.Label(image_Script, skin_scriptLibrary.label, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("@Copyright by Scizhen", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
            GUILayout.EndHorizontal();
        }


        string libraryViewName = "Welcome";
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
                libraryViewName = (viewIndex == 0) ? "Welcome" : "Collider";
            }
            //设置：是否实时刷新浏览窗口内容
            if (GUILayout.Button("重置", EditorStyles.toolbarButton, GUILayout.Width(50), GUILayout.Height(ToolbarHeight)))
            {
                menuWidthRatio = 0.15f;
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


            //帮助按钮—提示如何操作该库
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
        void DrawColliderMenu(float x, float y, float width, float height)
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
        protected int horizontalSize = 5;//每行绘制的设备数量
        /// <summary>
        /// 绘制碰撞体库主体内容
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        void DrawColliderContent(float x, float y, float width, float height)
        {
            Rect_content = new Rect(x, y, width, height);
            GUILayout.BeginArea(Rect_content);
            _scrollview2 = GUILayout.BeginScrollView(_scrollview2);//<! 滑动条

            if (viewIndex > 0)
            {
                ColliderLibraryContent(colliderLibraries, horizontalSize, viewIndex - 1);//碰撞体内容绘制
            }

            GUILayout.EndScrollView();//<! 滑动条
            GUILayout.EndArea();
        }

        private Rect Rect_control;
        Vector2 _scrollview4;
        /// <summary>
        /// 绘制碰撞体库主体内容
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        void DrawColliderControl(float x, float y, float width, float height)
        {
            Rect_control = new Rect(x, y, width, height);
            GUILayout.BeginArea(Rect_control);
            _scrollview4 = GUILayout.BeginScrollView(_scrollview4);//<! 滑动条

            ColliderControl();

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
        /// 宽度调整条3
        /// </summary>
        void DrawResizer2(float x, float y, float width, float height)
        {
            Rect_resizer2 = new Rect(x, y, width, height);
            GUILayout.BeginArea(new Rect(Rect_resizer2.position + (Vector2.right * resizerWidth3), new Vector2(2, height * 3)), resizerStyle);
            GUILayout.EndArea();

            EditorGUIUtility.AddCursorRect(Rect_resizer2, MouseCursor.ResizeHorizontal);//改变鼠标形状：显示鼠标指针位于调整大小位置
        }



        private float MaxColliderModuleWidth = 500f;
        private float MInColliderModuleWidth = 300f;
        private float colliderModuleWidth = 140f;//一个脚本内容总宽度
        private float colliderModuleHeight = 36;//一个脚本内容总宽度
        private float colliderIconWidth = 32f;//脚本图标宽度

        private List<string> RDTSNamespace = new List<string>() { "RDTS.", "RDTS.Utility." };

        void OneColliderContent(Colliders collider)
        {
            EditorGUILayout.BeginVertical("Box", GUILayout.Width(AreaContent.width / horizontalSize ), GUILayout.Height(colliderModuleHeight));
            EditorGUILayout.BeginHorizontal("Box");
            ///碰撞体名称
            EditorGUILayout.LabelField(collider.name, GUILayout.Width(80));
            ///获取碰撞体的贴图
            if(collider.icon == null)
                collider.icon = AssetDatabase.LoadAssetAtPath<Texture>(iconPath + "WindowColliderIcon.png");
            GUILayout.Label(collider.icon, labelStyle_MiddleCenter, GUILayout.Width(colliderIconWidth), GUILayout.Height(colliderIconWidth));

            EditorGUILayout.EndHorizontal();

            ///材质的描述信息
            GUILayout.TextArea(collider.notice);
            GUILayout.Space(10);
            ///为选中的对象添加材质，可为其子级对象添加
            if (GUILayout.Button("Click to use"))//点击添加此材质
            {
                AddColiiderToSelectedObject(collider);
                ///Debug.Log("添加:" + module.name);
            }

            EditorGUILayout.EndVertical();

        }

        void AddColiiderToSelectedObject(Colliders collider)
        {
            var colliderDetailsCount = collider.colliderDetails.Count;

            for (int i = 0; i < colliderDetailsCount; i++)
            {
                if (collider.colliderDetails[i].colliderSelect == ColliderSelect.BoxCollider)
                {
                    GameObject obj = Selection.activeGameObject;
                    if (obj == null)//未选中某个对象
                        return;
                    BoxCollider bx = obj.AddComponent<BoxCollider>();
                    bx.center = collider.colliderDetails[i].center;
                    bx.size = collider.colliderDetails[i].size;
                }
                if (collider.colliderDetails[i].colliderSelect == ColliderSelect.CapsuleCollider)
                {
                    GameObject obj = Selection.activeGameObject;
                    if (obj == null)//未选中某个对象
                        return;
                    CapsuleCollider cc = obj.AddComponent<CapsuleCollider>();
                    cc.center = collider.colliderDetails[i].center;
                    cc.radius = collider.colliderDetails[i].radius;
                    cc.height = collider.colliderDetails[i].height;
                    cc.direction = (int)collider.colliderDetails[i].direction;
                }
                if (collider.colliderDetails[i].colliderSelect == ColliderSelect.SphereCollider)
                {
                    GameObject obj = Selection.activeGameObject;
                    if (obj == null)//未选中某个对象
                        return;
                    SphereCollider sc = obj.AddComponent<SphereCollider>();
                    sc.center = collider.colliderDetails[i].center;
                    sc.radius = collider.colliderDetails[i].radius;
                }
                if (collider.colliderDetails[i].colliderSelect == ColliderSelect.TerrainCollider)
                {

                    GameObject obj = Selection.activeGameObject;
                    if (obj == null)//未选中某个对象
                        return;
                    TerrainCollider tc = obj.AddComponent<TerrainCollider>();
                }
                if (collider.colliderDetails[i].colliderSelect == ColliderSelect.WheelCollider)
                {

                    GameObject obj = Selection.activeGameObject;
                    if (obj == null)//未选中某个对象
                        return;
                    WheelCollider wc = obj.AddComponent<WheelCollider>();
                }


            }
        }

        void ColliderLibraryContent(List<ColliderLibrary> colliderLibraries, int horizontalSize = 5, int index = 0)
        {
            if (colliderLibraries == null || colliderLibraries.Count == 0)
                return;

            int rows = 0;//(完整)行数
            int columns = 0;//（不满足完整一行时，此行）列的个数
            rows = colliderLibraries[index].colliders.Count / horizontalSize;
            columns = colliderLibraries[index].colliders.Count % horizontalSize;

            if (rows != 0)
            {
                for (int i = 1; i <= rows; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                    for (int j = (i - 1) * horizontalSize; j < i * horizontalSize; j++)
                    {
                        OneColliderContent(colliderLibraries[index].colliders[j]);
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
                    OneColliderContent(colliderLibraries[index].colliders[z]);
                }

                EditorGUILayout.EndHorizontal();
            }

        }

        /// <summary>
        /// 碰撞体工具栏绘制
        /// </summary>
        float step = 0.1f;
        float Height_operation = 22f;
        float capsuleCollider_radius = 0.5f;
        float capsuleCollider_height = 0;
        int capsuleCollider_direction = 0;
        float sphereCollider_radius = 0.5f;

        void ColliderControl()
        {
            GameObject obj = Selection.activeGameObject;
            if (obj == null)//未选中某个对象
            {
                GUILayout.Button("请选择一个对象以展开工具栏");//名称
                return;
            }

            Collider[] colliders = obj.GetComponentsInChildren<Collider>();

            GUILayout.Box("All Collider Set", GUILayout.ExpandWidth(true));
            if (GUILayout.Button(new GUIContent("  Delete all Collider", icon_delete), GUILayout.Width(AreaControl.width), GUILayout.Height(Height_operation)))
            {
                if (EditorUtility.DisplayDialog("删除操作", "是否删除所有碰撞体（包含子对象）组件？", "yes", "no"))
                {
                    foreach (Collider signalCollider in colliders)
                    {
                        DestroyImmediate(signalCollider);
                    }
                }
            }
            if (GUILayout.Button(new GUIContent("  Set all Trigger", icon_pick), GUILayout.Width(AreaControl.width)))
            {
                foreach (Collider signalCollider in colliders)
                {
                    var nowistrigger = signalCollider.isTrigger;
                    signalCollider.isTrigger = !nowistrigger;
                }

            }
            GUILayout.Space(5);

            step = EditorGUILayout.FloatField("Step:", step);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Center.X up↑"), GUILayout.Width(AreaControl.width / 2)))
            {
                ControlEverySignalCollider(colliders,0,true,step,0);
            }
            if (GUILayout.Button(new GUIContent("Center.X down↓"), GUILayout.Width(AreaControl.width / 2)))
            {
                ControlEverySignalCollider(colliders, 0, false, step,0);
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Center.Y up↑"), GUILayout.Width(AreaControl.width / 2)))
            {
                ControlEverySignalCollider(colliders, 1, true, step,0);
            }
            if (GUILayout.Button(new GUIContent("Center.Y down↓"), GUILayout.Width(AreaControl.width / 2)))
            {
                ControlEverySignalCollider(colliders, 1, false, step,0);
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Center.Z up↑"), GUILayout.Width(AreaControl.width / 2)))
            {
                ControlEverySignalCollider(colliders, 2, true, step,0);
            }
            if (GUILayout.Button(new GUIContent("Center.Z down↓"), GUILayout.Width(AreaControl.width / 2)))
            {
                ControlEverySignalCollider(colliders, 2, false, step,0);
            }
            GUILayout.EndHorizontal();

            GUILayout.Box("BoxCollider Set", GUILayout.ExpandWidth(true));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Size.X up↑"), GUILayout.Width(AreaControl.width / 2)))
            {
                ControlEverySignalCollider(colliders, 0, true, step, 1);
            }
            if (GUILayout.Button(new GUIContent("Size.X down↓"), GUILayout.Width(AreaControl.width / 2)))
            {
                ControlEverySignalCollider(colliders, 0, false, step, 1);
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Size.Y up↑"), GUILayout.Width(AreaControl.width / 2)))
            {
                ControlEverySignalCollider(colliders, 1, true, step, 1);
            }
            if (GUILayout.Button(new GUIContent("Size.Y down↓"), GUILayout.Width(AreaControl.width / 2)))
            {
                ControlEverySignalCollider(colliders, 1, false, step, 1);
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Size.Z up↑"), GUILayout.Width(AreaControl.width / 2)))
            {
                ControlEverySignalCollider(colliders, 2, true, step, 1);
            }
            if (GUILayout.Button(new GUIContent("Size.Z down↓"), GUILayout.Width(AreaControl.width / 2)))
            {
                ControlEverySignalCollider(colliders, 2, false, step, 1);
            }
            GUILayout.EndHorizontal();
            GUILayout.Box("CapsuleCollider Set", GUILayout.ExpandWidth(true));
            capsuleCollider_radius = EditorGUILayout.FloatField("Radius:", capsuleCollider_radius);
            capsuleCollider_height = EditorGUILayout.FloatField("Height:", capsuleCollider_height);
            capsuleCollider_direction = EditorGUILayout.IntField("Direction:", capsuleCollider_direction);
            if (GUILayout.Button(new GUIContent("Change"),GUILayout.Width(AreaControl.width)))
            {
                ControlEverySignalCollider(colliders, 2, false, step, 2);
            }
            GUILayout.Box("SphereCollider Set", GUILayout.ExpandWidth(true));
            sphereCollider_radius = EditorGUILayout.FloatField("Radius:", sphereCollider_radius);
            if (GUILayout.Button(new GUIContent("Change"), GUILayout.Width(AreaControl.width)))
            {
                ControlEverySignalCollider(colliders, 2, false, step, 3);
            }

        }


        private Vector3 step2Vector3;
        /// <summary>
        /// 处理每一个碰撞体
        /// </summary>
        /// <param name="collider"></param>碰撞体大类
        /// <param name="vectorDirection"></param>方向变量，0：调整x轴，1：调整y轴，2：调整z轴
        /// <param name="addOrDelete"></param>加或减，true：加，false：减
        /// <param name="step"></param>步长
        /// <param name="mode"></param>模式，0：整体，1：boxcollider,2:CapsuleCollider
        void ControlEverySignalCollider(Collider[] collider,int vectorDirection,bool addOrDelete,float step,int mode) 
        {

            var boxColliders = collider[0].GetComponentsInChildren<BoxCollider>();
            var capsuleColliders = collider[0].GetComponentsInChildren<CapsuleCollider>();
            var sphereColliders = collider[0].GetComponentsInChildren<SphereCollider>();
            var terrainColliders = collider[0].GetComponentsInChildren<TerrainCollider>();
            var wheelColliders = collider[0].GetComponentsInChildren<WheelCollider>();

            if (vectorDirection == 0)
                step2Vector3 = new Vector3(step, 0, 0);
            if (vectorDirection == 1)
                step2Vector3 = new Vector3(0, step, 0);
            if (vectorDirection == 2)
                step2Vector3 = new Vector3(0, 0, step);

            if (mode == 0)
                AdjustColliderCenter(boxColliders, capsuleColliders, sphereColliders, wheelColliders, addOrDelete, vectorDirection, step);
            if (mode == 1)
            {
                foreach (BoxCollider bc in boxColliders)
                {
                    if (addOrDelete == true)
                    {
                        bc.size = bc.size + step2Vector3;
                    }
                    else
                    {
                        bc.size = bc.size - step2Vector3;
                    }

                }
            }
            if (mode == 2)
            {
                foreach (CapsuleCollider cc in capsuleColliders)
                {
                    cc.radius = capsuleCollider_radius;
                    cc.height = capsuleCollider_height;
                    cc.direction = capsuleCollider_direction;

                }
            }
            if (mode == 3)
            {
                foreach (SphereCollider sc in sphereColliders)
                {
                    sc.radius = sphereCollider_radius;

                }
            }


        }


        /// <summary>
        /// 调整所有碰撞体的中心
        /// </summary>
        /// <param name="boxColliders"></param>
        /// <param name="capsuleColliders"></param>
        /// <param name="sphereColliders"></param>
        /// <param name="wheelColliders"></param>
        /// <param name="addOrDelete"></param>
        /// <param name="vectorDirection"></param>
        /// <param name="step"></param>
        void AdjustColliderCenter(BoxCollider[] boxColliders, CapsuleCollider[] capsuleColliders, SphereCollider[] sphereColliders, WheelCollider[] wheelColliders,bool addOrDelete,int vectorDirection,float step)
        {


            foreach (BoxCollider bc in boxColliders)
            {
                if (addOrDelete == true)
                {
                    bc.center = bc.center + step2Vector3;
                }
                else
                {
                    bc.center = bc.center - step2Vector3;
                }

            }
            foreach (CapsuleCollider cc in capsuleColliders)
            {
                if (addOrDelete == true)
                {
                    cc.center = cc.center + step2Vector3;
                }
                else
                {
                    cc.center = cc.center - step2Vector3;
                }

            }
            foreach (SphereCollider sc in sphereColliders)
            {
                if (addOrDelete == true)
                {
                    sc.center = sc.center + step2Vector3;
                }
                else
                {
                    sc.center = sc.center - step2Vector3;
                }

            }
            foreach (WheelCollider wc in wheelColliders)
            {
                if (addOrDelete == true)
                {
                    wc.center = wc.center + step2Vector3;
                }
                else
                {
                    wc.center = wc.center - step2Vector3;
                }

            }
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

                //调整条3
                if (isResizingWidth2 && e.mousePosition.x > WindowWidth * 0.3 && e.mousePosition.x < WindowWidth * 0.9)
                {
                    areacontentWidthRatio = (e.mousePosition.x - AreaMenu.width) / WindowWidth;
                }

                //QM.Log("areacontentWidthRatio：" + areacontentWidthRatio + " " + "WindowWidth: " + WindowWidth + " " + "e.mousePosition.x" + e.mousePosition.x);

                Repaint();
            }
        }



        #endregion


        #region 读取库资源数据

        List<ColliderLibrary> colliderLibraries = new List<ColliderLibrary>();//脚本库数据
        List<string> menuItem = new List<string>();//记录菜单栏
        int number_ItemType = 0;//种类个数
        int number_Item = 0;//脚本数量

        List<ColliderLibrary> ReadLibraryData(ColliderLibraryData data)
        {
            List<ColliderLibrary> colliderLibraries = new List<ColliderLibrary>();
            colliderLibraries = data.ColliderLibraryDataList;//获取数据
            number_ItemType = colliderLibraries.Count;//获取碰撞体类型个数

            menuItem.Clear();
            colliderLibraries.ForEach(sl => {
                if (!menuItem.Contains(sl.type))
                    menuItem.Add(sl.type);//储存碰撞体类型

                number_Item += sl.colliders.Count;//一共多少脚本

            });


            return colliderLibraries;
        }


        /// <summary>
        /// 刷新当前窗口的内容
        /// </summary>
        void RedoWindowContent()
        {
            colliderLibraryData = AssetDatabase.LoadAssetAtPath<ColliderLibraryData>(dataPath + "ColliderLibraryData.asset");
            colliderLibraries = ReadLibraryData(colliderLibraryData);

            Repaint();
        }



        #endregion

    }

#endif
}
