///********************************************************************************************
///this is part of Robot Digital Twin System.                                                 *
///@Copyright by Shaw.S , 2022                                                                *
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
using System.IO;




namespace RDTS.Window
{
#if UNITY_EDITOR
    /// <summary>
    /// 模型/材质库：将模型与材质集合成一个库，通过asset文件存储数据。
    /// </summary>
    public class DeviceWindow : EditorWindow
    {
        ///库的显示模式  
        protected enum LibraryMode
        {
            ModelLibrary,//模型库
            MaterialLibrary//材质库
        }
        protected LibraryMode libraryMode = LibraryMode.ModelLibrary;

        ///窗口显示/浏览模式
        protected enum ShowMode
        {
            LibraryMode,//“库浏览”模式
            ModuleMode//“设备浏览”模式
        }
        protected ShowMode showMode = ShowMode.LibraryMode;


        float ScreenWidth; //<! 窗口宽度
        float ScreenHeight;//<! 窗口高度
        protected int horizontalSize = 5;//每行绘制的设备数量

        ///分隔条
        Rect splitterRect;
        float splitterPos;//分隔条位置
        float splitterWidth = 10;//分隔条宽度
        bool dragging;//拖动与否


        [MenuItem("Parallel-RDTS/Window/DeviceWindow", false, 200)]
        private static void ShowWindow()
        {
            DeviceWindow deviceWindow = (DeviceWindow)EditorWindow.GetWindow(typeof(DeviceWindow));
            deviceWindow.titleContent = DeviceWindow.thisTitleContent;//设置标题和图标
            deviceWindow.minSize = new Vector2(300, 200);
            deviceWindow.splitterPos = 75;
            deviceWindow.Show(); 
        }

 
        /// <summary>设定库中每行绘制的模块数量</summary>
        public  void SetHorizontalSize(int size = 5)
        {
            horizontalSize = size;
        }

        //窗口界面内容绘制
        void OnGUI()
        {

            if (HasOpenInstances<DeviceWindow>())//设备库窗口是否打开
            {
                ScreenWidth = position.width;//获取窗口宽度
                ScreenHeight = position.height;//获取窗口高度

                DeviceWindowOperate();
                ///TitleMenu();//旧的菜单项（已弃用）

                if (viewIndex == 0)//索引号为0，则绘制欢迎界面
                {
                    WelcomeView();
                }
                else//绘制模型/材质库界面
                {
                    GUILayout.BeginHorizontal();//*水平绘制
                    LeftView();//<! 左侧的菜单栏
                    DrawVerticalSplitter();//Splitter 分隔条
                    RightView();//<! 右侧对应菜单项的窗口内容
                    GUILayout.EndHorizontal();//*水平绘制/
                }

             
                ///分隔条的拖动处理
                SplitterControl(60, 180);
                ///拖拽添加所选中的设备
                if (_dragCreate)
                    UserInterfaceEdit.StartDrag(_dragObjName, _dragCreate);

            }


        }



        /// <summary>
        /// 绘制分隔条
        /// </summary>
        void DrawVerticalSplitter()
        {
            GUILayout.Box("",
                   GUILayout.Width(splitterWidth),
                   GUILayout.MaxWidth(splitterWidth),
                   GUILayout.MinWidth(splitterWidth),
                   GUILayout.ExpandHeight(true));
            splitterRect = GUILayoutUtility.GetLastRect();

            EditorGUIUtility.AddCursorRect(splitterRect, MouseCursor.ResizeHorizontal);//改变鼠标形状：显示鼠标指针位于调整大小位置

        }


        /// <summary>
        /// 分隔条的拖动控制
        /// </summary>
        /// <param name="mixValue">能拖动的最小距离</param>
        /// <param name="maxValue">能拖动的最大距离</param>
        void SplitterControl(float mixValue = 55, float maxValue = 255)
        {
            // Splitter events 分隔条事件
            if (Event.current != null)
            {
                switch (Event.current.rawType)
                {
                    case EventType.MouseDown:
                        if (splitterRect.Contains(Event.current.mousePosition))//是否当前鼠标位置在分隔条范围内
                        {
                            ///Debug.Log("Start dragging");
                            dragging = true;
                        }
                        break;
                    case EventType.MouseDrag:
                        if (dragging)
                        {
                            ///Debug.Log("moving splitter");
                            splitterPos += Event.current.delta.x;
                            Repaint();
                        }
                        break;
                    case EventType.MouseUp:
                        if (dragging)
                        {
                            ///Debug.Log("Done dragging");
                            dragging = false;
                        }
                        break;
                }

                ///限幅
                if (splitterPos > maxValue)//最大值
                    splitterPos = maxValue;
                if (splitterPos < mixValue)//最小值
                    splitterPos = mixValue;
            }
        }


        /// <summary>
        /// 鼠标从设备库窗口拖拽至Scene窗口中创建设备的事件
        /// </summary>
        /// <param name="sceneView"></param>
        void DragEvent(SceneView sceneView)
        {
            Event e = Event.current;
            DragAndDrop.visualMode = DragAndDropVisualMode.Generic;

            Vector3 mousePos = UserInterfaceEdit.GetMousePosToScene();
            mousePos = new Vector3(mousePos.x/5 -2 , mousePos.y/50+0.4f, mousePos.z/5 -2);///调整创建位置

            if (e.button == 0 && e.type == EventType.DragExited && _dragCreate)//<! 注意使用e.type而不是Event.current.type
            {
                Instantiate(_dragObj, mousePos, Quaternion.identity);

                SceneView.duringSceneGui -= DragEvent;//删除事件
                _dragCreate = false;
                /// ShowNotification(new GUIContent($"已添加{_dragObjName}"));
            }

        }


        void OnInspectorUpdate()
        {
            ///none...

        }


        int viewIndex = 0;//右侧视图索引号
        Vector2 posLeft;
        float leftPos => splitterPos - 5;
        float leftViewHeight = 30f;
        /// <summary>
        /// 左侧菜单项绘制
        /// </summary>
        void LeftView()
        {
            int count = menuItem.Count;//类型个数，决定菜单项个数

            // Left view
            posLeft = GUILayout.BeginScrollView(posLeft,
                GUILayout.Width(splitterPos),
                GUILayout.MaxWidth(splitterPos),
                GUILayout.MinWidth(splitterPos));//<! 滑动条

            leftViewHeight = (ScreenHeight - menuHeight - 40) / count;//每个菜单项高度

            ///根据读取到的menuItem列表来绘制左侧的菜单项
            for (int i=0; i< count; i++)
            {
                string typeName = menuItem[i];
                if (GUILayout.Button(typeName, GUILayout.Width(leftPos), GUILayout.Height(leftViewHeight)))
                {
                    if (showMode == ShowMode.LibraryMode)//<! 只在“库浏览”模式下会生效
                    {
                        viewIndex = i + 1;//对应右侧视图的索引号
                        ///QM.Log(typeName);
                    }
                   
                }
            }

            GUILayout.EndScrollView();//<! 滑动条
        }


        Vector2 posRight;
        /// <summary>
        /// 右侧内容绘制
        /// </summary>
        void RightView()
        {
            // Right view
            posRight = GUILayout.BeginScrollView(posRight, GUILayout.ExpandWidth(true));//<! 滑动条

            if(viewIndex > 0)//索引号大于0
            {
                AdjustHorizontalSize();//动态调整每行绘制数量

                if (showMode == ShowMode.LibraryMode)///一类设备模块的库
                    ModuleLibraryContent(modulelibraries, horizontalSize, viewIndex - 1);//模块内容绘制
                else///单个设备模块
                    OneModuleContent_InDetail(showModule);
            }

            GUILayout.EndScrollView();//<! 滑动条
        }


        /// <summary>
        /// 根据窗口宽度和模块(模型/材质)宽度实现动态调整库中每行绘制的个数
        /// </summary>
        void AdjustHorizontalSize()
        {
            float width = ScreenWidth - splitterPos - splitterWidth - 50;//右侧绘制区域的大致宽度
            int horizontalCount_model = Mathf.FloorToInt(width / modelModuleWidth);//每行绘制模型的个数
            int horizontalCount_material = Mathf.FloorToInt(width / materialModuleWidth);//每行绘制材质的个数

            //限幅：最少每行绘制一个
            if (horizontalCount_model < 1)
                horizontalCount_model = 1;
            if (horizontalCount_material < 1)
                horizontalCount_material = 1;

            horizontalSize = (libraryMode == LibraryMode.ModelLibrary) ? horizontalCount_model : horizontalCount_material;
        }


        #region WelcomeView
        /// <summary>
        /// 欢迎界面，说明此窗口的内容
        /// </summary>
        void WelcomeView()
        {
            GUILayout.Space(ScreenHeight / 6);
            GUILayout.Label("This is a model/material library.", skin_modelLibrary.label, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
            GUILayout.Label(image_modellibrary, skin_modelLibrary.label, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("@Copyright by Shaw", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
            GUILayout.EndHorizontal();

        }

        #endregion


        #region 设备库的总体操作

        float menuHeight = 20f;
        string libraryName = "模型库";
        bool libraryNameFlag = true;//为true显示模型库，为false显示材质库
        /// <summary>
        /// 设备库窗口的相关操作
        /// </summary>
        void DeviceWindowOperate()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            //切换按钮—切换模型库/材质库
            if (GUILayout.Button(libraryName, EditorStyles.toolbarButton, GUILayout.Width(100), GUILayout.Height(menuHeight)))
            {
                libraryNameFlag = !libraryNameFlag;//取反
                libraryName = (libraryNameFlag) ? "模型库" : "材质库";
                libraryMode = (libraryNameFlag) ? LibraryMode.ModelLibrary : LibraryMode.MaterialLibrary;
                RedoWindowContent();//重绘窗口内容
                showMode = ShowMode.LibraryMode;//重置显示模式
                viewIndex = 1;
                ShowNotification(new GUIContent($"已切换为{libraryName}"));
            }
            //重置窗口内容为起始的默认内容
            if (GUILayout.Button("重置", EditorStyles.toolbarButton, GUILayout.Width(50), GUILayout.Height(menuHeight)))
            {
                splitterPos = 75;
                viewIndex = 0;
                showMode = ShowMode.LibraryMode;
                Repaint();
            }
            //保存按钮—保存对窗口中所做的修改
            if (GUILayout.Button("保存", EditorStyles.toolbarButton, GUILayout.Width(50), GUILayout.Height(menuHeight)))
            {
                SaveWindowContent();
                ShowNotification(new GUIContent("设备库已保存"));
                Debug.Log("保存");
            }
            //刷新按钮—当DataAsset中修改时，重新读取并重绘窗口
            if (GUILayout.Button(image_Refresh, EditorStyles.toolbarButton, GUILayout.Width(50), GUILayout.Height(menuHeight)))
            {
                showMode = ShowMode.LibraryMode;//重置显示模式
                _editor?.SetZoomRateValue(1);//重置预览窗口缩放大小
                RedoWindowContent();//重绘窗口内容
                ShowNotification(new GUIContent("库已刷新"));
                Debug.Log("刷新");
            }

            GUILayout.FlexibleSpace();//灵活填充
            
            //帮助按钮—提示如何操作该设备库
            if (GUILayout.Button(image_Help, EditorStyles.toolbarButton, GUILayout.Width(24), GUILayout.Height(menuHeight)))
            {
                ////PopupWindow.Show(new Rect(this.position.width / 2, 20, 0, 0), new DeviceTipsWindow());//PopupWindow的大小固定，只能设置位置
                //打开弹窗
                PopWindowHelp helpWindow = (PopWindowHelp)EditorWindow.GetWindow(
                       typeof(PopWindowHelp),
                       true,
                       "Help");
                var content = helpWindow.ReadHelpData(0);
                helpWindow.SetShowContents(content);
                helpWindow.ShowUtility();
                ///Debug.Log("帮助");
            }
            //关闭按钮—关闭当前窗口
            if (GUILayout.Button(image_close, EditorStyles.toolbarButton, GUILayout.Width(24), GUILayout.Height(menuHeight)))
            {
                if (EditorUtility.DisplayDialog("关闭窗口", "是否确定关闭设备库窗口？", "yes", "no"))
                {
                    this.Close();
                    ///Debug.Log("关闭");
                }

            }


            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);

        }


        /// <summary>
        /// 刷新当前窗口的内容
        /// </summary>
        void RedoWindowContent()
        {
            devicelibraryData = AssetDatabase.LoadAssetAtPath<DeviceLibraryData>(dataPath + "DeviceLibraryData.asset");//重新读取一下asset文件
            modulelibraries.Clear();
            modulelibraries = (libraryMode == LibraryMode.ModelLibrary) ? ReadLibraryData(devicelibraryData) : ReadLibraryData(materialLibraryData);//重新读取数据资源
            Repaint();
        }


        void SaveWindowContent()
        {
            //nothing...

        }


        #endregion



        #region 设备库标题和内容

        List<string> menuItem = new List<string>();//储存数据中的设备类型名称

        # region 这个是旧的菜单栏绘制，已弃用！
        private int index = 0;
        /// <summary>设备库工具栏（标题）</summary>
        void TitleMenu()
        {
            string[] moduleType = menuItem.ToArray();
            index = GUILayout.Toolbar(index, moduleType);//将moduleType数组中的内容转换成对应工具栏按钮(名称)
        }
        #endregion

        private float modelModuleWidth = 200;
        private float materialModuleWidth = 140;

        private bool _dragCreate = false;//是否进入拖拽
        private GameObject _dragObj;//拖拽的对象
        private string _dragObjName;//拖拽对象名称
        /// <summary>
        /// 一个模块的内容
        /// </summary>
        /// <param name="module">要绘制的模块</param>
        /// <param name="isModel">是否绘制模块为模型，默认为模型而非材质</param>
        void OneModuleContent(Module module, bool isModel = true)
        {
            if (module == null)
                return;

            if(isModel)/*一个模型模块绘制*/
            {
                EditorGUILayout.BeginVertical("Box", GUILayout.Width(modelModuleWidth));

                EditorGUILayout.BeginHorizontal("Box");
                ///模型名称
                EditorGUILayout.LabelField(module.name, GUILayout.Width(140));
                ///点击开启一次拖拽创建模型的操作，再次点击可取消该次拖拽事件
                /////<! 若module.textrue为空，则将显示image_model，作为按钮的图标
                var image_model = AssetPreview.GetAssetPreview(module.model) as Texture;//获取材质的资源预览纹理
                Texture image = module.textrue;
                if (image == null)
                    image = image_model;
                if (GUILayout.Button( image, skin_modelLibrary.button, GUILayout.Width(60), GUILayout.Height(60)))
                {
                    _dragCreate = !_dragCreate;
                    if (_dragCreate)
                    {
                        _dragObj = module.model;
                        _dragObjName = module.name;
                        SceneView.duringSceneGui += DragEvent;//添加事件：此事件以在Scene视图每次调用OnGUI方法时接收回调
                    }
                    else
                        SceneView.duringSceneGui -= DragEvent;

                }
                EditorGUILayout.EndHorizontal();
                ///模型的描述信息
                GUILayout.TextArea(module.description);
                GUILayout.Space(10);
                ///坐标输入方式向Scene中添加模型
                module.modelPos = EditorGUILayout.Vector3Field("Add location", module.modelPos);//添加位置
                if (GUILayout.Button("Click to add"))// 点击添加此设备
                {
                    GameObject obj = Instantiate(module.model, module.modelPos, module.model.transform.rotation);
                    QM.Log("添加:" + module.name);
                }
                ///查看此模型的详细信息：包含三维视图预览，详细信息等
                if (GUILayout.Button("Click to view"))//点击查看此设备
                {
                    showMode = ShowMode.ModuleMode;
                    showModule = module;
                    QM.Log("查看:" + module.name);
                }

                EditorGUILayout.EndVertical();
            }
            else/*一个材质模块的绘制*/
            {
                EditorGUILayout.BeginVertical("Box", GUILayout.Width(materialModuleWidth));

                EditorGUILayout.BeginHorizontal("Box");
                ///材质名称
                EditorGUILayout.LabelField(module.name, GUILayout.Width(80));
                ///获取材质的资源预览纹理
                var image_material = AssetPreview.GetAssetPreview(module.material);
                //图标按钮只添加当前选中且具有meshRender的对象的材质
                if (GUILayout.Button(image_material, skin_modelLibrary.button, GUILayout.Width(60), GUILayout.Height(60)))
                {
                    ///为选中的对象添加材质（点击图标可添加材质）
                    bool addMaterial = UserInterfaceEdit.AddMaterialToGameobject(module.material);
                    if (!addMaterial)
                    {
                        EditorUtility.DisplayDialog("Error", "当前选中的对象为空或不具备添加材质的组件！请选择合适的对象！", "OK");
                    }
                    ///Debug.Log("添加:" + module.name);
                }

                EditorGUILayout.EndHorizontal();

                ///材质的描述信息
                GUILayout.TextArea(module.description);
                GUILayout.Space(10);
                ///为选中的对象添加材质，可为其子级对象添加
                if (GUILayout.Button("Click to use"))//点击添加此材质
                {
                    AddMaterialToSelectedObject(module.material);
                    ///Debug.Log("添加:" + module.name);
                }

                EditorGUILayout.EndVertical();
            }
           


        }



        /// <summary>
        /// 向选中对象添加材质
        /// </summary>
        void AddMaterialToSelectedObject(Material material)
        {
            if (EditorUtility.DisplayDialog("Notice", "是否将当前选中对象及其子对象均添加材质！", "yes", "no"))
            {
                UserInterfaceEdit.AddAddMaterialToGameobjects(material);
            }
            else
            {
                bool addMaterial = UserInterfaceEdit.AddMaterialToGameobject(material);
                if (!addMaterial)
                {
                    EditorUtility.DisplayDialog("Error", "当前选中的对象为空或不具备添加材质的组件！请选择合适的对象！", "OK");
                }
            }
        }






        int number_ModuleType = 0;
        /// <summary>
        /// 绘制一个类型的设备库
        /// </summary>
        /// <param name="modulelibrariies">所要绘制的设备库</param>
        /// <param name="horizontalSize">一行所要绘制的设备数量</param>
        void ModuleLibraryContent(List<ModuleLibrary> modulelibraries, int horizontalSize = 5, int index = 0 )
        {
            number_ModuleType = modulelibraries.Count;//获取当前的模块库中的类型数量

            if (number_ModuleType == number_ItemType)//先判断读取到的资源数据是否完整
            {
                int rows = 0;//(完整)行数
                int columns = 0;//（不满足完整一行时，此行）列的个数
                rows = modulelibraries[index].modules.Count / horizontalSize;
                columns = modulelibraries[index].modules.Count % horizontalSize;

                if (rows != 0)
                {

                    for (int i = 1; i <= rows; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        for (int j = (i - 1) * horizontalSize; j < i * horizontalSize; j++)
                        {
                            OneModuleContent(modulelibraries[index].modules[j], (libraryMode == LibraryMode.ModelLibrary)? true : false);
                        }

                        EditorGUILayout.EndHorizontal();
                    }
                }

                if (columns != 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    for (int z = rows * horizontalSize; z < rows * horizontalSize + columns; z++)
                    {
                        OneModuleContent(modulelibraries[index].modules[z], (libraryMode == LibraryMode.ModelLibrary) ? true : false);
                    }

                    EditorGUILayout.EndHorizontal();
                }

            }
            else
            {
                modulelibraries = (libraryMode == LibraryMode.ModelLibrary) ? ReadLibraryData(devicelibraryData) : ReadLibraryData(materialLibraryData);//不完整则再次进行读取
            }

        }




        #endregion

        #region 预览界面

        Module showModule;//要预览的设备模块
        GameObject _lastGameObject;
        PreviewRenderEditor _editor = null;
        bool _load = true;
        Vector2 _lightRot = new Vector2(75, 20);
        Vector2 _lastLightRot;
        float _scale = 1.25f;

        
        /// <summary>
        /// 模型的三维视图预览
        /// </summary>
        /// <param name="showModule">要显示的设备模块</param>
        /// <param name="scale">缩放值</param>
        void ModulePreview(Module showModule, float scale = 1f)
        {
            GameObject showObject = showModule?.model ?? null;//要预览的对象  
            float previewWidth = (ScreenWidth - splitterPos - splitterWidth) / 3;//右侧视图的1/3宽度

            EditorGUILayout.BeginHorizontal("Box");

            if (_editor == null)
            {
                ///Editor：从此基类派生，以便为自定义对象创建自定义检视面板和编辑器
                _editor = Editor.CreateEditor(this, typeof(PreviewRenderEditor)) as PreviewRenderEditor;
            }

            if (_editor)
            {
                if (_lastLightRot != _lightRot)//光线
                {
                    _lastLightRot = _lightRot;
                    _editor.RefreshLightRot(_lightRot);
                }

                _editor.SetZoomRateValue(1 / scale);//缩放【已改成用鼠标滚轮代替手动拖动滑动条】
                _editor.DrawPreview(GUILayoutUtility.GetRect(200, previewWidth));//<! 绘制的预览框大小，水平方向会填充满窗口，故调节高度数值即可
            }

            if (showObject && _load)
            {
                _editor.RefreshPreviewInstance(showObject);
                _load = false;
                _lastGameObject = showObject;
            }

            if (_lastGameObject != showObject)
            {
                _load = true;
            }

            EditorGUILayout.EndHorizontal();
        }


        /// <summary>
        /// 绘制一个模型详细的界面：三维预览，更详细的信息等
        /// </summary>
        void OneModuleContent_InDetail(Module showModule)
        {
            float halfWidth = (ScreenWidth - splitterPos - splitterWidth) / 2;//右侧视图的一半宽度
            float spaceWidth = (ScreenWidth - splitterPos - splitterWidth ) / 3 - 80;//空白间隔

            Module module = new Module();
            if (showModule != null) module = showModule;

            //showObject = (GameObject)EditorGUILayout.ObjectField("预览预制体", showObject, typeof(GameObject), true);
            EditorGUILayout.BeginHorizontal("Box");

            if (GUILayout.Button("Back", EditorStyles.toolbarButton, GUILayout.Width(80), GUILayout.Height(menuHeight)))//返回
            {
                showMode = ShowMode.LibraryMode;
                showModule = null;
            }
            GUILayout.Space(spaceWidth);
            ///调整模型缩放大小
            GUILayout.Label("Zoom size：");//缩放大小
            if(_editor != null) _scale = _editor.GetZoomRateValue();
            _scale = EditorGUILayout.Slider( _scale, 0.1f, 5);//<! 滑动条：相机与对象距离，可视为缩放大小
            /// _lightRot = EditorGUILayout.Vector2Field("光源方向", _lightRot);    
            EditorGUILayout.EndHorizontal();

            ModulePreview(showModule, _scale);//三维视图预览（注意：这里要使用showModule，以避免模型切换时快速更新三维视图对象）

            EditorGUILayout.BeginHorizontal("Box");

            ///点击开启一次拖拽创建模型的操作，再次点击可取消该次拖拽事件
            if (GUILayout.Button(image_mouseleft, GUILayout.Width(halfWidth / 4), GUILayout.Height(40)))
            {
                _dragCreate = !_dragCreate;
                if (_dragCreate)
                {
                    _dragObj = module?.model;
                    _dragObjName = module?.name;
                    SceneView.duringSceneGui += DragEvent;//添加事件：此事件以在Scene视图每次调用OnGUI方法时接收回调
                }
                else
                    SceneView.duringSceneGui -= DragEvent;

            }

            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginVertical(GUILayout.Width(halfWidth));
            module.modelPos = EditorGUILayout.Vector3Field("", module.modelPos);
            if (GUILayout.Button("Click to add this model"))//添加此设备
            {
                GameObject obj = Instantiate(module?.model, module.modelPos, Quaternion.identity);
                Debug.Log("添加:" + module.name);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);

            ModelDataItem("Name", new string[1] { module?.name });//名称
            ModelDataItem("Description", new string[1] { module?.description });//描述

            /* 另外的详细信息（读取挂载的“TwinModel”脚本） */
            if (module.model == null)
            {
                EditorGUILayout.Space(20);
                RedLabel("注意：此模型为空！");
                return;
            }
            TwinModel script_tm = module?.model?.GetComponent<TwinModel>();
            ShowTwinModelStaticData(script_tm);


        }

        /// <summary>
        /// 针对于模型上挂载的“TwinModel”脚本，读取并显示其中的数据信息
        /// </summary>
        /// <param name="script_tm"></param>
        void ShowTwinModelStaticData(TwinModel script_tm)
        {
            if (script_tm == null)
            {
                EditorGUILayout.Space(20);
                RedLabel("注意：此模型未挂载“TwinModel”脚本！");
                return;
            }
            ///共有的数据信息
            ModelDataItem("Model hierarchy", new string[1] { script_tm?.Level.ToString() });//模型层次
            ModelDataItem("Model type", new string[1] { script_tm?.Type.ToString() });//模型类型
            ModelDataItem("Function and Use", new string[1] { script_tm?.FunctionAndUse.ToString() });//功能与用途
            ModelDataItem("Device model", new string[1] { script_tm?.DeviceBrand.ToString() });//设备型号
            ModelDataItem("Size", //尺寸
                new string[3]
                {
                    "length","width","height"//"长","宽","高"
                },
                new string[3] {
                    script_tm?.modelSize.Length.ToString(),
                    script_tm?.modelSize.Width.ToString(),
                    script_tm?.modelSize.Height.ToString()
                });

            ///针对于“机械臂类模型”绘制特定内容
            if(script_tm?.Type == TwinModelType.RobotArm)
            {
                EditorGUILayout.BeginHorizontal("Box");
                EditorGUILayout.LabelField("D-H Param", skin_modelLibrary.customStyles[4], GUILayout.Width((ScreenWidth - splitterPos - splitterWidth) / 5), GUILayout.Height(25));
                EditorGUILayout.LabelField("", GUILayout.Width(4), GUILayout.Height(25));//间隔

                string[] tableItem = new string[5] { "link", "a_i", "d_i", "θ", "Joint range" };//关节范围
                
                var DHPara = script_tm.RobotDHParameter;
                int count = DHPara.Count;//列表长度
                List<string[]> tableData = new List<string[]>();
                for(int i=0; i<count; i++)
                {
                    //每一个列表元素包含的数据（数组形式）
                    string[] newData = new string[6] {
                        DHPara[i].link,
                        DHPara[i].alpha_i.ToString(),
                        DHPara[i].d.ToString(),
                        DHPara[i].theta.ToString(),
                        DHPara[i].jointRange.MinValue.ToString(),
                        DHPara[i].jointRange.MaxValue.ToString()
                    };
                    //存入列表
                    tableData.Add(newData);
                }
                RobotDHParameterTable(tableItem, tableData);


                EditorGUILayout.EndHorizontal();
            }
               
        }


        /// <summary>
        /// 一项模型数据绘制：一个数据名称对应一行数据内容
        /// </summary>
        /// <param name="itemName">数据名称</param>
        /// <param name="itemContenr">数据内容</param>
        void ModelDataItem(string itemName, string[] itemContenr)
        {
            GUIStyle dataStyle = skin_modelLibrary.customStyles[4];
            float nameWidth = (ScreenWidth - splitterPos - splitterWidth) / 5;//数据名称长度
            EditorGUILayout.BeginHorizontal("Box");
            //数据名称
            EditorGUILayout.LabelField(itemName, dataStyle, GUILayout.Width(nameWidth), GUILayout.Height(25));
            EditorGUILayout.LabelField("", GUILayout.Width(4), GUILayout.Height(25));//间隔
            //数据内容
            EditorGUILayout.BeginVertical();
            for (int i = 0; i < itemContenr.Length; i++)
            {
                EditorGUILayout.LabelField(itemContenr[i], GUILayout.Height(25));
            }
            EditorGUILayout.EndVertical();


            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 一项模型数据绘制：一个数据名称可包含多个子名称，一个子名称对应一行数据内容
        /// </summary>
        /// <param name="itemName">数据名称</param>
        /// <param name="secondName">子名称</param>
        /// <param name="itemContenr">数据内容</param>
        void ModelDataItem(string itemName, string[] secondName, string[] itemContenr)
        {
            GUIStyle dataStyle = skin_modelLibrary.customStyles[4];
            GUIStyle dataStyle2 = skin_modelLibrary.customStyles[5];
            float nameWidth = (ScreenWidth - splitterPos - splitterWidth) / 5;//数据名称长度
            float secondNameWidth = (ScreenWidth - splitterPos - splitterWidth)*2 / 15;//第二数据名称长度
            EditorGUILayout.BeginHorizontal("Box");
            //数据名称
            EditorGUILayout.LabelField(itemName, dataStyle, GUILayout.Width(nameWidth), GUILayout.Height(25));
            EditorGUILayout.LabelField("", GUILayout.Width(4), GUILayout.Height(25));//间隔
            //数据内容
            EditorGUILayout.BeginVertical();
            for (int i = 0; i < itemContenr.Length; i++)
            {
                EditorGUILayout.BeginHorizontal("Box");
                EditorGUILayout.LabelField(secondName[i], dataStyle2, GUILayout.Width(secondNameWidth), GUILayout.Height(20));
                EditorGUILayout.LabelField("", GUILayout.Width(4), GUILayout.Height(20));//间隔
                EditorGUILayout.LabelField(itemContenr[i], GUILayout.Height(20));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();


            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 红色的标签控件
        /// </summary>
        /// <param name="content"></param>
        void RedLabel(string content)
        {
            GUIStyle redStyle = skin_modelLibrary.customStyles[3];
            EditorGUILayout.LabelField(content, redStyle);
        }

        /// <summary>
        /// 机器人DH参数表绘制
        /// </summary>
        /// <param name="tableItem">表的菜单项名称</param>
        /// <param name="tableData">（每一行）表数据</param>
        void RobotDHParameterTable(string[] tableItem, List<string[]> tableData = null)
        {
            GUIStyle dataStyle3 = skin_modelLibrary.customStyles[5];
            GUIStyle dataStyle4 = skin_modelLibrary.customStyles[6];
            float itemWidth = (ScreenWidth - splitterPos - splitterWidth)*4 / 28;//名称长度
            EditorGUILayout.BeginVertical();
            //表项名称绘制
            EditorGUILayout.BeginHorizontal("Box");
            for (int i = 0; i < tableItem.Length; i++)
            {
                EditorGUILayout.LabelField(tableItem[i], dataStyle3, GUILayout.Width(itemWidth), GUILayout.Height(20));
            }
            EditorGUILayout.EndHorizontal();

            //表内容绘制
            if (tableData == null)
                return;

            int count = tableData.Count;//列表长度，即共需绘制多少行数据
            
            foreach(var data in tableData)
            {
                EditorGUILayout.BeginHorizontal();
                for (int i = 0; i < data.Length-2; i++)
                {
                    EditorGUILayout.LabelField(data[i].ToString(), dataStyle4, GUILayout.Width(itemWidth), GUILayout.Height(20));
                }
                EditorGUILayout.LabelField(data[data.Length-2].ToString()+"/"+ data[data.Length-1].ToString(), dataStyle4, GUILayout.Width(itemWidth), GUILayout.Height(20));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical(); 

        }

        #endregion


        #region 读取库资源数据
        string WriteDeviceJsonFile = "Assets/RDTS/Scripts/Window/Editor/DeviceWindow/jsonInfo/";//文件写入路径
        string dataPath = "Assets/RDTS/Data/";
        string iconPath = "Assets/RDTS/Private/Resources/Icons/WindowIcon/";
        string skinPath = "Assets/RDTS/Scripts/Window/";
        DeviceLibraryData devicelibraryData;//模型库的数据
        MaterialLibraryData materialLibraryData;//材质库的数据
        static GUIContent thisTitleContent;//窗口名称
        void OnEnable()
        {
            //数据资源读取
            devicelibraryData =  AssetDatabase.LoadAssetAtPath<DeviceLibraryData>(dataPath + "DeviceLibraryData.asset");
            materialLibraryData =  AssetDatabase.LoadAssetAtPath<MaterialLibraryData>(dataPath + "MaterialLibraryData.asset");
            Debug.Log(devicelibraryData);
            modulelibraries = (libraryMode == LibraryMode.ModelLibrary)? ReadLibraryData(devicelibraryData): ReadLibraryData(materialLibraryData);
            //设置窗口标题及图标
            Texture titleIcon = AssetDatabase.LoadAssetAtPath<Texture>(iconPath+ "Parallel.png");
            thisTitleContent = new GUIContent("Parallel设备模型库", titleIcon);
            //加载unity自带图标
            LoadTexture();
           
        }

        private GUISkin skin_modelLibrary;

        private Texture2D image_Refresh;
        private Texture2D image_Help;
        private Texture2D image_close;
        private Texture2D image_modellibrary;
        private Texture2D image_robot;
        private Texture2D image_mouseleft;
        void LoadTexture()//加载素材
        {
            skin_modelLibrary = AssetDatabase.LoadAssetAtPath<GUISkin>("Assets/RDTS/Scripts/Window/Editor/DeviceWindow/WindowModelLibrary.guiskin");

            image_Refresh = EditorGUIUtility.FindTexture("Refresh");
            image_Help = EditorGUIUtility.FindTexture("d__Help@2x");
            image_close = EditorGUIUtility.FindTexture("d_winbtn_win_close");

            image_modellibrary = AssetDatabase.LoadAssetAtPath<Texture>(iconPath + "WindowModelIcon.png") as Texture2D;
            image_robot = AssetDatabase.LoadAssetAtPath<Texture>(iconPath + "Devicerobot.png") as Texture2D;
            image_mouseleft = AssetDatabase.LoadAssetAtPath<Texture>(iconPath + "mouseleft.png") as Texture2D;
        }


        int number_ItemType = 0;//种类
        int number_Item = 0;//数量
        List<ModuleLibrary> modulelibraries = new List<ModuleLibrary>();//储存数据
        List<Module> modules;

        /// <summary>
        /// 读取设备模型库资源数据
        /// </summary>
        /// <param name="data">对应要读取的库的数据</param>
        List<ModuleLibrary> ReadLibraryData (DeviceLibraryData data)
        {
            menuItem.Clear();//先清空，不然“刷新”时可能会将 已删除的元素 继续保留，导致出错
            List<ModuleLibrary> modulelibraries = new List<ModuleLibrary>();

            if (data is null)
            {
                modulelibraries = ReadJsonData(WriteDeviceJsonFile);
                return modulelibraries;
            }
            number_ItemType = data.DeviceLibraryDataList.Count;//获取一共有多少类型的设备
            var number_ItemId = 0;
            foreach (var deviceLibrary in data.DeviceLibraryDataList)//遍历每一种类型
            {

                ModuleLibrary thisModulelibrary = new ModuleLibrary();
                Json_ModuleLibrary Json_thisModulelibrary = new Json_ModuleLibrary();
                thisModulelibrary.name = deviceLibrary.name;
                Json_thisModulelibrary.type = deviceLibrary.name;
                Json_thisModulelibrary.id = number_ItemId.ToString();
                number_ItemId += 1;
                if (!menuItem.Contains(deviceLibrary.name))//储存设备类型名称的列表
                {
                    menuItem.Add(deviceLibrary.name);
                }

                foreach (var device in deviceLibrary.devices)//遍历每一个设备
                {
                    Module thisModule = new Module();
                    thisModule.name = device.name;
                    thisModule.description = device.description;
                    thisModule.textrue = device.textrue;
                    thisModule.model = device.model;
                    thisModulelibrary.modules.Add(thisModule);

                    Json_Module Json_thisModule = new Json_Module();
                    Json_thisModule.name = device.name;
                    Json_thisModule.description = device.description;
                    Json_thisModule.textrue = AssetDatabase.GetAssetPath(device.textrue);
                    Json_thisModule.model = AssetDatabase.GetAssetPath(device.model);
                    Json_thisModulelibrary.modules.Add(Json_thisModule);

                }
                //将设备库信息存储在WriteJsonFile变量的文件夹下
                string jsondata = JsonUtility.ToJson(Json_thisModulelibrary);
                WriteDataToJson(WriteDeviceJsonFile + thisModulelibrary.name + ".json", jsondata);

                modulelibraries.Add(thisModulelibrary);

                number_Item += deviceLibrary.devices.Count;//获得一共有多少个设备
            }


            return modulelibraries;
        }


        public void WriteDataToJson(string fileUrl, string jsondata)
        {
            string dir_name = Path.GetDirectoryName(fileUrl);
            if (!Directory.Exists(dir_name))
            {
                Directory.CreateDirectory(dir_name);
            }
            using (StreamWriter sw = new StreamWriter(fileUrl))
            {
                //保存数据
                sw.Write(jsondata);
                //关闭文档
                sw.Close();
                sw.Dispose();
            }
        }
        public List<ModuleLibrary> ReadJsonData(string fileUrl)
        {
            List<ModuleLibrary> modulelibraries = new List<ModuleLibrary>();
            //string类型的数据常量
            string readData;

            if (Directory.Exists(fileUrl))
            {
                DirectoryInfo direction = new DirectoryInfo(fileUrl);
                FileInfo[] files = direction.GetFiles("*");
                number_ItemType = direction.GetFiles("*.json").Length;
                Debug.Log("number_ItemType: "+ number_ItemType);

                //先填充空的列表，为后续添加做准备，实现顺序保留
                for (int i = 0; i < number_ItemType; i++)
                {
                    menuItem.Add("");
                    modulelibraries.Add(new ModuleLibrary());
                }

                for (int i = 0; i < files.Length; i++)
                {
                    //忽略关联文件
                    if (files[i].Name.EndsWith(".json"))
                    {

                        using (StreamReader sr = File.OpenText(files[i].FullName))
                        {
                            //数据保存
                            readData = sr.ReadToEnd();
                            sr.Close();
                        }
                        Json_ModuleLibrary m_PersonData = JsonUtility.FromJson<Json_ModuleLibrary>(readData);

                        ModuleLibrary thisModulelibrary = new ModuleLibrary();
                        thisModulelibrary.name = m_PersonData.type;
                        var ModulelivraryId = Convert.ToInt32(m_PersonData.id);

                        if (!menuItem.Contains(thisModulelibrary.name))//储存设备类型名称的列表
                        {
                            menuItem[ModulelivraryId] = thisModulelibrary.name;
                        }

                        foreach (Json_Module module in m_PersonData.modules)
                        {
                            Module thisModule = new Module();
                            thisModule.name = module.name;
                            thisModule.description = module.description;
                            thisModule.textrue = AssetDatabase.LoadAssetAtPath<Texture>(module.textrue);  //module.textrue;
                            thisModule.model = AssetDatabase.LoadAssetAtPath<GameObject>(module.model); 
                            thisModulelibrary.modules.Add(thisModule);
                        }
                        modulelibraries[ModulelivraryId] = thisModulelibrary;
                    }

                }
            }
            //返回数据
            return modulelibraries;
        }


        /// <summary>
        /// 读取材质库资源数据
        /// </summary>
        /// <param name="data">对应要读取的库的数据</param>
        List<ModuleLibrary> ReadLibraryData(MaterialLibraryData data)   
        {
            menuItem.Clear();//先清空，不然“刷新”时可能会将 已删除的元素 继续保留，导致出错
            List<ModuleLibrary> modulelibraries = new List<ModuleLibrary>();
            number_ItemType = data.MaterialLibraryDataList.Count;//获取一共有多少类型的材质

            foreach (var materialLibrary in data.MaterialLibraryDataList)//遍历每一种类型
            {

                ModuleLibrary thisModulelibrary = new ModuleLibrary();
                thisModulelibrary.name = materialLibrary.name;

                if (!menuItem.Contains(materialLibrary.name))//储存设备类型名称的列表
                {
                    menuItem.Add(materialLibrary.name);
                }

                foreach (var device in materialLibrary.materials)//遍历每一个设备
                {
                    Module thisModule = new Module();
                    thisModule.name = device.name;
                    thisModule.description = device.description;
                    thisModule.textrue = device.textrue;
                    thisModule.material = device.material;

                    thisModulelibrary.modules.Add(thisModule);
                }

                modulelibraries.Add(thisModulelibrary);

                number_Item += materialLibrary.materials.Count;//获得一共有多少个材质
            }

            return modulelibraries;
        }



        #endregion

    }




    /// <summary>
    ///一个设备模块类：包含一个设备及其相关操作
    ///[对应一个Device类或MaterialSingle类]
    /// </summary>
    public class Module
    {
        public string name;//名称
        public string description;//描述
        public Texture textrue;//图标 => [若采用资源预览方式，则不需要添加textrue]

        ///模型
        public GameObject model;//模型文件（一般为预制体）
        public Vector3 modelPos;//模型被添加的位置

        ///材质
        public Material material;//材质
    }

    /// <summary>
    /// 一种设备模块类：包括一种类型的设备
    /// [对应一个DeviceLibrary类]
    /// </summary>
    /// 
    public class ModuleLibrary
    {
        public string name;
        public List<Module> modules = new List<Module>();
    }


    [Serializable]
    public class Json_ModuleLibrary
    {
        public string type;
        public string id;
        public List<Json_Module> modules = new List<Json_Module>();
    }

    [Serializable]
    public class Json_Module
    {
        public string name;//名称
        public string description;//描述
        public string textrue;//图标 => [若采用资源预览方式，则不需要添加textrue]

        ///模型
        public string model;//模型文件（一般为预制体）
        public string modelPos;//模型被添加的位置

        ///材质
        public string material;//材质
    }


#endif

}
