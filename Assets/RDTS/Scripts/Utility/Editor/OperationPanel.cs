///********************************************************************************************
///this is part of Robot Digital Twin System.                                                 *
///@Copyright by Shaw.S                                                                       *
///Thanks for the code reference game4automation provides.                                    *
///Do not distribute without authorization,thanks!                                            *
///********************************************************************************************
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
    /// 快捷操作面板：对Hierarchy、Scene视图进行相关快捷操作
    /// 注意：可以将按钮文字换成图片，进行美化
    /// </summary>
    public class OperationPanel : EditorWindow
    {
        ///Hierarchy
        private static List<Object> selectedobjs = new List<Object>();
        private static int selectedLayer = 0;
        private static LayerMask addLayer;
        private static Material selectedMaterial;
        private static Material selectedAddMaterial;

        ///Scene
        private bool sceneRepaint = false;
        private static GameObject sceneObj = null;

        //折叠箭头标签的折叠状态（true：展开  false：折叠）
        private bool isShow_control = true;//对ganmeobject的控制
        private bool isShow_valueobj = true;//对值对象的操作
        private bool isShow_addcomponent = true;//添加脚本


        [MenuItem("Parallel-RDTS/Utility/Operation Panel", false, 400)]
        static void ShowOperationPanel()
        {
            // Get existing open window or if none, make a new one:
            OperationPanel window = (OperationPanel)EditorWindow.GetWindow(typeof(OperationPanel));
            window.titleContent = OperationPanel.thisTitleContent;//设置标题和图标
            window.minSize = new Vector2(300, 200);
            window.Show();


        }

        OperationPanel()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }



        string iconPath = "Assets/RDTS/Private/Resources/Icons/WindowIcon/";
        static GUIContent thisTitleContent;
        void OnEnable()
        {
            //设置窗口标题及图标
            Texture titleIcon = AssetDatabase.LoadAssetAtPath<Texture>(iconPath + "Parallel.png");
            thisTitleContent = new GUIContent("快捷操作面板", titleIcon);
          
        }

        Vector2 _scrollview;
        void OnGUI()
        {
            if (HasOpenInstances<OperationPanel>())//设备库窗口是否打开
            {
                TitleMenu();//菜单栏

                _scrollview = GUILayout.BeginScrollView(_scrollview);//<! 滑动条
                PanelContent(index);//内容绘制
                GUILayout.EndScrollView();//<! 滑动条


            }

        }


        List<string> operationType = new List<string>() { "Hierarchy Operation", "Scene Operation" };//储存数据中的设备类型名称
        private int index = 0;
        /// <summary>
        ///工具栏（标题）
        /// </summary>
        void TitleMenu()
        {
            string[] moduleType = operationType.ToArray();
            index = GUILayout.Toolbar(index, moduleType);//将moduleType数组中的内容转换成对应工具栏按钮(名称)
            //index = GUILayout.Toolbar(index, moduleType, panelGUISKin.textArea);//将moduleType数组中的内容转换成对应工具栏按钮(名称)
        }


        void PanelContent(int index)//内容绘制
        {
            switch(index)
            {
                case 0://对层级面板的操作
                    PanelContent_Hierarchy();
                    break;

                case 1://对Scene中对象的操作
                    PanelContent_Scene();
                    break;

            }
        }

        /// <summary>
        /// 操作面板的内容
        /// </summary>
        void PanelContent_Hierarchy()
        {
            float width = position.width;//获取窗口宽度


            //空10单位
            GUILayout.BeginVertical();
            GUILayout.Width(10);
            GUILayout.EndVertical();   

            EditorGUILayout.Separator();

         //<! 垂直排列
            GUILayout.BeginVertical();

            ///1
            EditorGUILayout.BeginHorizontal("Box");
            if (GUILayout.Button("Isolate", GUILayout.Width((width / 4)-10)))//只显示选中的对象
            {
                Isolate();
            }
            if (GUILayout.Button("Invisible", GUILayout.Width(width / 4)))
            {
                Visible(false);
            }
            if (GUILayout.Button("Visible", GUILayout.Width(width / 4)))
            {
                Visible(true);
            }
            if (GUILayout.Button("All Visible", GUILayout.Width((width / 4) -10)))//使所有对象可见
            {
                VisibleAll(true, true);
            }
            EditorGUILayout.EndHorizontal();

            ///2
            EditorGUILayout.BeginHorizontal("Box");

            if (GUILayout.Button("Simplify Selected", GUILayout.Width((width / 3)-10)))//选择对象，将其不挂载RDTSBehavior类脚本的子对象隐藏
            {
                SupressChildren(true);
            }
            if (GUILayout.Button("Recover Selected", GUILayout.Width(width / 3)))//选择对象，将其子对象设为在Hierarchy面板中可见
            {
                SupressChildren(false);
            }
            if (GUILayout.Button("Recover All", GUILayout.Width((width / 3) - 10)))//使所有对象在Hierarchy面板中可见
            {
                UnhideAll();
            }
            EditorGUILayout.EndHorizontal();

            ///3
            EditorGUILayout.BeginHorizontal("Box");
            if (GUILayout.Button("Lock", GUILayout.Width((width / 2)-10)))
            {
                Lock(true);
            }

            if (GUILayout.Button("Unlock", GUILayout.Width((width / 2) - 10)))
            {
                Lock(false);
            }
            EditorGUILayout.EndHorizontal();

            ///4
            EditorGUILayout.BeginHorizontal("Box");
            if (GUILayout.Button("Select Visible", GUILayout.Width((width/2)-10)))//选择所有可见的对象
            {
                SelectVisible(true);
            }
            if (GUILayout.Button("Select Invisible", GUILayout.Width((width/2)-10)))//选择所有不可见的对象
            {
                 SelectVisible(false);
            }
            EditorGUILayout.EndHorizontal();

            ///5
            EditorGUILayout.BeginHorizontal("Box");
            if (GUILayout.Button("Collapse same level", GUILayout.Width((width/2)-10)))//将选中的对象折叠
            {
                CollapseSameLevel();
            }

            if (GUILayout.Button("Expand same level", GUILayout.Width((width/2) - 10)))//将选中的对象展开
            {
                ExpandSameLevel();
            }
            EditorGUILayout.EndHorizontal();

            ///6
            EditorGUILayout.BeginHorizontal("Box");
            if (GUILayout.Button("Remove missing scripts", GUILayout.Width(width/2)))//移除选中对象的“missing scripts”
            {
                RemoveMissingScripts();
            }
            EditorGUILayout.EndHorizontal();

            ///7
            EditorGUILayout.BeginHorizontal("Box");
            if (GUILayout.Button("Copy to empty Parent", GUILayout.Width(width/2)))//将选中的对象复制，并设为根对象
            {
                CopyObjectsToNewEmptyParent();
            }
            EditorGUILayout.EndHorizontal();


            ///8
            EditorGUILayout.BeginHorizontal("Box");//为选择对象添加layer
            GUILayout.Label("Choose a layer：", GUILayout.Width((width/3) - 10));
            addLayer = EditorGUILayout.LayerField("", addLayer, GUILayout.Width(width/3));
            if (GUILayout.Button("Add Layer", GUILayout.Width((width/3) - 10)))
            {
                AddLayer(addLayer);
            }
            EditorGUILayout.EndHorizontal();

            ///9
            EditorGUILayout.BeginHorizontal("Box");//按照layer查找对象
            GUILayout.Label("Choose a layer：", GUILayout.Width((width/3) - 10)); 
            selectedLayer = EditorGUILayout.LayerField("", selectedLayer, GUILayout.Width(width/3));
            if (GUILayout.Button("Select Layer", GUILayout.Width((width/3)-10)))
            {
                SelectLayer(selectedLayer);
            }
            EditorGUILayout.EndHorizontal();

            ///10
            EditorGUILayout.BeginHorizontal("Box");//为选择的对象分配材质
            GUILayout.Label("Choose a material：", GUILayout.Width((width/3) - 10));
            selectedAddMaterial = (Material)EditorGUILayout.ObjectField(selectedAddMaterial, typeof(Material), false, GUILayout.Width(width / 3));
            if (GUILayout.Button("Add Material", GUILayout.Width((width/3) - 10)))
            {
                AssignMaterial(selectedAddMaterial);
            }
            EditorGUILayout.EndHorizontal();

            ///11
            EditorGUILayout.BeginHorizontal("Box");//按照material查找对象
            GUILayout.Label("Choose a material：", GUILayout.Width((width/3) - 10));
            selectedMaterial = (Material)EditorGUILayout.ObjectField(selectedMaterial, typeof(Material), false, GUILayout.Width(width / 3));
            if (GUILayout.Button("Select Material", GUILayout.Width((width/3) - 10)))
            {
                SelectMaterial(selectedMaterial);
            }
            EditorGUILayout.EndHorizontal();



            GUILayout.EndVertical();

        }


        float height = 20;
        int PositionFlag = 0;//位置标志位。1：局部坐标归零  2：世界坐标归零
        int RotateFlag = 0;//旋转标志位。1：x轴正转  2：x轴反转  3：y轴正传   4：y轴反传   5：z轴正传   6：y轴反传  7:局部归零  8：全局归零
        int EmptyFlag = 0;//空对象标志位。1：创建空的子对象  2：创建空的父对象
        int ValueObjFlag = 0;//信号标志位。1：ValueInputBool  2：ValueInputInt  3：ValueInputFloat  4：ValueOutputBool  5：ValueOutputInt  6：ValueOutputFloat
        int ComponentFlag = 0;//组件标志位。
        void PanelContent_Scene()
        {
            float width = position.width;//获取窗口宽度


            //空10单位
            GUILayout.BeginVertical();
            GUILayout.Width(10);
            GUILayout.EndVertical();

            EditorGUILayout.Separator();

            //<! 垂直排列0
            GUILayout.BeginVertical();

            isShow_control = EditorGUILayout.BeginFoldoutHeaderGroup(isShow_control, "Control Gameobject");
            if(isShow_control)
            {
                //位置
                EditorGUILayout.BeginHorizontal("Box");
                if (GUILayout.Button("LocalPos 0", GUILayout.Width((width / 3) - 10), GUILayout.Height(height)))
                {
                    PositionFlag = 1;
                    sceneRepaint = true;
                }
                if (GUILayout.Button("GlobalPos 0", GUILayout.Width((width / 3)), GUILayout.Height(height)))
                {
                    PositionFlag = 2;
                    sceneRepaint = true;
                }
                EditorGUILayout.EndHorizontal();

                //旋转
                EditorGUILayout.BeginHorizontal("Box");
                if (GUILayout.Button("LocalRot 0", GUILayout.Width((width / 3) - 10), GUILayout.Height(height)))
                {
                    RotateFlag = 7;
                    sceneRepaint = true;
                }
                if (GUILayout.Button("GlobalRot 0", GUILayout.Width((width / 3)), GUILayout.Height(height)))
                {
                    RotateFlag = 8;
                    sceneRepaint = true;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal("Box");
                if (GUILayout.Button("Rot X +90", GUILayout.Width((width / 3)-10), GUILayout.Height(height)))
                {
                    RotateFlag = 1;
                    sceneRepaint = true;
                }
                if (GUILayout.Button("Rot Y +90", GUILayout.Width((width / 3)), GUILayout.Height(height)))
                {
                    RotateFlag = 3;
                    sceneRepaint = true;
                }
                if (GUILayout.Button("Rot Z +90", GUILayout.Width((width / 3)-10), GUILayout.Height(height)))
                {
                    RotateFlag = 5;
                    sceneRepaint = true;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal("Box");
                if (GUILayout.Button("Rot X -90", GUILayout.Width((width / 3) - 10), GUILayout.Height(height)))
                {
                    RotateFlag = 2;
                    sceneRepaint = true;
                }
                if (GUILayout.Button("Rot Y -90", GUILayout.Width((width / 3)), GUILayout.Height(height)))
                {
                    RotateFlag = 4;
                    sceneRepaint = true;
                }
                if (GUILayout.Button("Rot Z -90", GUILayout.Width((width / 3) - 10), GUILayout.Height(height)))
                {
                    RotateFlag = 6;
                    sceneRepaint = true;
                }
                EditorGUILayout.EndHorizontal();

                //空对象
                EditorGUILayout.BeginHorizontal("Box");
                if (GUILayout.Button("Empty Child", GUILayout.Width((width / 3) - 10), GUILayout.Height(height)))
                {
                    EmptyFlag = 1;
                    sceneRepaint = true;
                }
                if (GUILayout.Button("Empty Parent", GUILayout.Width((width / 3)), GUILayout.Height(height)))
                {
                    EmptyFlag = 2;
                    sceneRepaint = true;
                }
                EditorGUILayout.EndHorizontal();


            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            isShow_valueobj = EditorGUILayout.BeginFoldoutHeaderGroup(isShow_valueobj, "Value Object");
            if (isShow_valueobj)
            {
                EditorGUILayout.BeginHorizontal("Box");
                if (GUILayout.Button("Change Direction", GUILayout.Width(width / 3-10), GUILayout.Height(height)))//对选中的值对象变换输出输入方向
                {
                    RDTS.Utility.Editor.ValueHierarchyContextMenu.HierarchyChangeValueObjDirection();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal("Box");
                if (GUILayout.Button("ValueInputBool", GUILayout.Width(width / 3 - 10), GUILayout.Height(height)))
                {
                    ValueObjFlag = 1;
                    sceneRepaint = true;
                }
                if (GUILayout.Button("ValueInputInt", GUILayout.Width(width / 3), GUILayout.Height(height)))
                {
                    ValueObjFlag = 2;
                    sceneRepaint = true;
                }
                if (GUILayout.Button("ValueInputFloat", GUILayout.Width(width / 3 - 10), GUILayout.Height(height)))
                {
                    ValueObjFlag = 3;
                    sceneRepaint = true;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal("Box");
                if (GUILayout.Button("ValueOutputBool", GUILayout.Width(width / 3 - 10), GUILayout.Height(height)))
                {
                    ValueObjFlag = 4;
                    sceneRepaint = true;
                }
                if (GUILayout.Button("ValueOutputInt", GUILayout.Width(width / 3), GUILayout.Height(height)))
                {
                    ValueObjFlag = 5;
                    sceneRepaint = true;
                }
                if (GUILayout.Button("ValueOutputFloat", GUILayout.Width(width / 3 - 10), GUILayout.Height(height)))
                {
                    ValueObjFlag = 6;
                    sceneRepaint = true;
                }
                EditorGUILayout.EndHorizontal();


            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            isShow_addcomponent = EditorGUILayout.BeginFoldoutHeaderGroup(isShow_addcomponent, "Add Component");
            if(isShow_addcomponent)
            {
                EditorGUILayout.BeginHorizontal("Box");
                if (GUILayout.Button("MU", GUILayout.Width(width / 4 - 10), GUILayout.Height(height)))
                {
                    ComponentFlag = 1;
                    sceneRepaint = true;
                }
                if (GUILayout.Button("Source", GUILayout.Width(width / 4), GUILayout.Height(height)))
                {
                    ComponentFlag = 2;
                    sceneRepaint = true;
                }
                if (GUILayout.Button("Sink", GUILayout.Width(width / 4), GUILayout.Height(height)))
                {
                    ComponentFlag = 3;
                    sceneRepaint = true;
                }
                if (GUILayout.Button("Sensor", GUILayout.Width(width / 4 - 10), GUILayout.Height(height)))
                {
                    ComponentFlag = 4;
                    sceneRepaint = true;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal("Box");
                if (GUILayout.Button("Drive", GUILayout.Width(width / 4 - 10), GUILayout.Height(height)))
                {
                    ComponentFlag = 5;
                    sceneRepaint = true;
                }
                if (GUILayout.Button("Grip", GUILayout.Width(width / 4), GUILayout.Height(height)))
                {
                    ComponentFlag = 6;
                    sceneRepaint = true;
                }
                if (GUILayout.Button("RobotArm", GUILayout.Width(width / 4), GUILayout.Height(height)))
                {
                    ComponentFlag = 7;
                    sceneRepaint = true;
                }
                //if (GUILayout.Button("Station", GUILayout.Width(width / 4 - 10), GUILayout.Height(height)))
                //{
                //    ComponentFlag = 8;
                //    sceneRepaint = true;
                //}

                if (GUILayout.Button("TranspotrSurface", GUILayout.Width(width / 4 - 10), GUILayout.Height(height)))
                {
                    ComponentFlag = 9;
                    sceneRepaint = true;
                }
                //if (GUILayout.Button("null", GUILayout.Width(width / 4), GUILayout.Height(height)))
                //{
                //    ComponentFlag = 10;
                //    sceneRepaint = true;
                //}
                //if (GUILayout.Button("null", GUILayout.Width(width / 4), GUILayout.Height(height)))
                //{
                //    ComponentFlag = 11;
                //    sceneRepaint = true;
                //}
                //if (GUILayout.Button("null", GUILayout.Width(width / 4 - 10), GUILayout.Height(height)))
                //{
                //    ComponentFlag = 12;
                //    sceneRepaint = true;
                //}
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal("Box");
                if (GUILayout.Button("Drive_Cylinder", GUILayout.Width(width / 4 - 10), GUILayout.Height(height)))
                {
                    ComponentFlag = 13;
                    sceneRepaint = true;
                }
                if (GUILayout.Button("Drive_DestinationMotor", GUILayout.Width(width / 4), GUILayout.Height(height)))
                {
                    ComponentFlag = 14;
                    sceneRepaint = true;
                }
                if (GUILayout.Button("Drive_Simple", GUILayout.Width(width / 4), GUILayout.Height(height)))
                {
                    ComponentFlag = 15;
                    sceneRepaint = true;
                }
                if (GUILayout.Button("Drive_Gear", GUILayout.Width(width / 4 - 10), GUILayout.Height(height)))
                {
                    ComponentFlag = 16;
                    sceneRepaint = true;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal("Box");
                if (GUILayout.Button("Drive_ErraticPosition", GUILayout.Width(width / 4 - 10), GUILayout.Height(height)))
                {
                    ComponentFlag = 17;
                    sceneRepaint = true;
                }
                if (GUILayout.Button("Sensor_Standard", GUILayout.Width(width / 4), GUILayout.Height(height)))
                {
                    ComponentFlag = 18;
                    sceneRepaint = true;
                }
                //if (GUILayout.Button("null", GUILayout.Width(width / 4), GUILayout.Height(height)))
                //{
                //    ComponentFlag = 19;
                //    sceneRepaint = true;
                //}
                //if (GUILayout.Button("null", GUILayout.Width(width / 4 - 10), GUILayout.Height(height)))
                //{
                //    ComponentFlag = 20;
                //    sceneRepaint = true;
                //}
                EditorGUILayout.EndHorizontal();


            }
            EditorGUILayout.EndFoldoutHeaderGroup();





            //0
            GUILayout.EndVertical();
        }

        //Scene视图的绘制
        void OnSceneGUI(SceneView scene)
        {
            if (Selection.activeObject != null)
                if (Selection.activeObject.GetType() == typeof(GameObject))
                    sceneObj = (GameObject)Selection.activeObject;
                else
                    sceneObj = null;

            
            ScenePosition();//位置操作
            SceneRotation();//旋转操作
            SceneEmpty();//空对象创建
            SceneAddValueObj();//值对象添加
            SceneComponent();//组件添加


        }

        private void Update()
        {
            if (sceneRepaint)
            {
                SceneView.RepaintAll();
                //Debug.Log("repaint");
                sceneRepaint = false;

            }

        }


        void ScenePosition()
        {
            switch (PositionFlag)
            {
                case 1:
                    LocalPosToZero();
                    PositionFlag = 0;
                    break;
                case 2:
                    GlobalPosToZero();
                    PositionFlag = 0;
                    break;
            }
        }

        void SceneRotation()
        {
            switch (RotateFlag)
            {
                case 1:
                    Rotation(new Vector3(90, 0, 0));
                    RotateFlag = 0;
                    break;
                case 2:
                    Rotation(new Vector3(-90, 0, 0));
                    RotateFlag = 0;
                    break;
                case 3:
                    Rotation(new Vector3(0, 90, 0));
                    RotateFlag = 0;
                    break;
                case 4:
                    Rotation(new Vector3(0, -90, 0));
                    RotateFlag = 0;
                    break;
                case 5:
                    Rotation(new Vector3(0, 0, 90));
                    RotateFlag = 0;
                    break;
                case 6:
                    Rotation(new Vector3(0, 0, -90));
                    RotateFlag = 0;
                    break;
                case 7:
                    LocalRotToZero();
                    RotateFlag = 0;
                    break;
                case 8:
                    GlobalRotToZero();
                    RotateFlag = 0;
                    break;
            }
        }

        void SceneEmpty()
        {
            switch (EmptyFlag)
            {
                case 1:
                    EmptyChild();
                    EmptyFlag = 0;
                    break;
                case 2:
                    EmptyParent();
                    EmptyFlag = 0;
                    break;
            }

        }
        
        void SceneAddValueObj()
        {
            switch (ValueObjFlag)
            {
                case 1:
                    CreateValueObj(typeof(ValueInputBool));
                    ValueObjFlag = 0;
                    break;
                case 2:
                    CreateValueObj(typeof(ValueInputInt));
                    ValueObjFlag = 0;
                    break;
                case 3:
                    CreateValueObj(typeof(ValueInputFloat));
                    ValueObjFlag = 0;
                    break;
                case 4:
                    CreateValueObj(typeof(ValueOutputBool));
                    ValueObjFlag = 0;
                    break;
                case 5:
                    CreateValueObj(typeof(ValueOutputInt));
                    ValueObjFlag = 0;
                    break;
                case 6:
                    CreateValueObj(typeof(ValueOutputFloat));
                    ValueObjFlag = 0;
                    break;

            }

        }

        void SceneComponent()
        {
            switch (ComponentFlag)
            {
                case 1:
                    AddComponent(typeof(MU));
                    ComponentFlag = 0;
                    break;
                case 2:
                    AddComponent(typeof(Source));
                    ComponentFlag = 0;
                    break;
                case 3:
                    AddComponent(typeof(Sink));
                    ComponentFlag = 0;
                    break;
                case 4:
                    AddComponent(typeof(Sensor));
                    ComponentFlag = 0;
                    break;
                case 5:
                    AddComponent(typeof(Drive));
                    ComponentFlag = 0;
                    break;
                case 6:
                    AddComponent(typeof(Grip));
                    ComponentFlag = 0;
                    break;
                case 7:
                    AddComponent(typeof(RobotArm));
                    ComponentFlag = 0;
                    break;
                case 8:
                    ///AddComponent(typeof(Station));//之后重新改进操作面板
                    ComponentFlag = 0;
                    break;
                case 9:
                    AddComponent(typeof(TransportSurface));
                    ComponentFlag = 0;
                    break;
                case 10:
                    
                    ComponentFlag = 0;
                    break;
                case 11:
                   
                    ComponentFlag = 0;
                    break;
                case 12:
                   
                    ComponentFlag = 0;
                    break;
                case 13:
                    AddComponent(typeof(Drive_Cylinder)); 
                    ComponentFlag = 0;
                    break;
                case 14:
                    AddComponent(typeof(Drive_DestinationMotor));
                    ComponentFlag = 0;
                    break;
                case 15:
                    AddComponent(typeof(Drive_Simple));
                    ComponentFlag = 0;
                    break;
                case 16:
                    AddComponent(typeof(Drive_Gear));
                    ComponentFlag = 0;
                    break;
                case 17:
                    AddComponent(typeof(Drive_ErraticPosition));
                    ComponentFlag = 0;
                    break;
                case 18:
                    AddComponent(typeof(Sensor_Standard));
                    ComponentFlag = 0;
                    break;
            }

        }





        static void AddObjectsToSelection()
        {
            foreach (var myobj in Selection.objects)
            {
                if (myobj is GameObject && selectedobjs.IndexOf(myobj) == -1)
                {
                    selectedobjs.Add(myobj);
                }
            }

            ShowOperationPanel();
        }


        public GameObject[] GatherObjects(GameObject root)
        {
            List<GameObject> objects = new List<GameObject>();
            Stack<GameObject> recurseStack = new Stack<GameObject>(new GameObject[] { root });

            while (recurseStack.Count > 0)
            {
                GameObject obj = recurseStack.Pop();
                objects.Add(obj);

                foreach (Transform childT in obj.transform)
                    recurseStack.Push(childT.gameObject);
            }

            return objects.ToArray();
        }

        private void RecordUndo(ref List<GameObject> list)
        {
            foreach (var go in list)
            {
                Undo.RecordObject(go, "Operation Panel Changes");
            }
        }

        private List<GameObject> GetAllSelectedObjects()
        {
            List<GameObject> list = new List<GameObject>();
            AddObjectsToSelection();
            foreach (Object myobj in selectedobjs)
            {
                GameObject go = (GameObject)myobj;
                list.Add(go);
            }
            selectedobjs.Clear();
            RecordUndo(ref list);
            return list;
        }

        private List<GameObject> GetAllSelectedObjectsIncludingSub()
        {
            List<GameObject> list = new List<GameObject>();
            AddObjectsToSelection();
            foreach (Object myobj in selectedobjs)
            {
                var objs = GatherObjects((GameObject)myobj);
                foreach (var obj in objs)
                {
                    list.Add(obj);
                }
            }
            selectedobjs.Clear();
            RecordUndo(ref list);
            return list;
        }

        private List<GameObject> GetAllSelectedTopObjects()
        {
            List<GameObject> list = new List<GameObject>();
            AddObjectsToSelection();
            foreach (Object myobj in selectedobjs)
            {

                GameObject go = (GameObject)myobj;
                // is also parent in list then delete this from list
                var parent = go.transform.parent;
                var notadd = false;
                if (!ReferenceEquals(parent, null))
                    if (selectedobjs.Contains(parent))
                        notadd = true;

                if (!notadd) list.Add(go);
            }
            selectedobjs.Clear();
            RecordUndo(ref list);
            return list;
        }

        //使所有对象可见/隐藏
        private void VisibleAll(bool visible, bool considerhidden)
        {
            List<GameObject> rootObjects = new List<GameObject>();
            Scene scene = SceneManager.GetActiveScene();
            scene.GetRootGameObjects(rootObjects);
            foreach (var obj in rootObjects)
            {
                if (obj.GetComponents<RDTSController>().Length == 0)
                {
                    var objs = Global.GatherObjects(obj);
                    foreach (var obje in objs)
                    {
                        var ishidden = false;
                        var go = (GameObject)obje;

                        if (visible && ishidden == false)
                            go.SetActive(visible);
                        if (!visible)
                            go.SetActive(visible);
                    }
                }
            }
        }

        //单独显示选择的对象
        private void Isolate()
        {

            VisibleAll(false, false);
            var objs = GetAllSelectedObjects();
            foreach (var obj in objs)
            {
                var go = (GameObject)obj;
                Global.SetVisible(go, true);
                // set this object and everything above active
                do
                {
                    go.SetActive(true);
                    if (go.transform.parent != null)
                        go = go.transform.parent.gameObject;
                    else
                        go = null;
                } while (go != null);
            }
        }

        //使所有被“简化而隐藏”的对象可见
        private void UnhideAll()
        {
            List<GameObject> rootObjects = new List<GameObject>();
            Scene scene = SceneManager.GetActiveScene();//当前激活的场景
            scene.GetRootGameObjects(rootObjects);//返回场景中的所有根游戏对象。 rootObjects用于接收返回的对象
            foreach (var obj in rootObjects)
            {
                if (obj.GetComponents<RDTSController>().Length == 0)
                {
                    var objs = Global.GatherObjects(obj);
                    Global.HideSubObjects(obj, false);
                }
            }
        }

        //可见/隐藏选择的对象
        private void Visible(bool visible)
        {
            var objs = GetAllSelectedObjects();
            foreach (var obj in objs)
            {
                Global.SetVisible((GameObject)obj, visible);
            }
        }

        //锁住/解锁选择的对象
        private void Lock(bool lockelement)
        {
            var objs = GetAllSelectedObjectsIncludingSub();
            foreach (var obj in objs)
            {
                Global.SetLockObject(obj, lockelement);
            }
        }

        //选出所有visible/invisible的对象
        private void SelectVisible(bool visible)
        {
            List<GameObject> list = new List<GameObject>();
            var groupcomps = UnityEngine.Resources.FindObjectsOfTypeAll(typeof(GameObject));
            foreach (var comp in groupcomps)
            {
                var gr = (GameObject)comp;
                if (EditorUtility.IsPersistent(gr.transform.root.gameObject))
                    continue;
                if (gr.transform.IsChildOf(Global.rdtsController.transform))
                    continue;
                if (gr.activeSelf == visible)
                    list.Add(gr.gameObject);
            }
            Selection.objects = list.ToArray();
        }

        private List<GameObject> GetSameLevelObjects()
        {
            List<GameObject> list = new List<GameObject>();
            AddObjectsToSelection();
            foreach (Object myobj in selectedobjs)
            {
                var obj = (GameObject) myobj;
                var parent = obj.transform.parent;
                // add all children
                if (parent != null)
                {
                    foreach (Transform child in parent.transform)
                    {
                        list.Add(child.gameObject);
                    }
                   
                }
            }
            selectedobjs.Clear();
            return list;
        }

        //折叠同一级（不包括根对象）
        private void CollapseSameLevel()
        {
            var objs = GetSameLevelObjects();
            foreach (var obj in objs)
            {
                Global.SetExpandedRecursive(obj, false);
            }
        }

        //展开同一级（不包括根对象）
        private void ExpandSameLevel()
        {
            var objs = GetSameLevelObjects();
            foreach (var obj in objs)
            {
                Global.SetExpandedRecursive(obj, true);
            }
        }

        //将选择的对象复制到一个新的父对象下
        private void CopyObjectsToNewEmptyParent()
        {
            var objs = GetAllSelectedTopObjects();

            // Create new Object
            GameObject newobj = new GameObject();
            foreach (Object myobj in objs)
            {
                var copyobj = (GameObject)Object.Instantiate(myobj, newobj.transform);
                var origobj = (GameObject)myobj;
                copyobj.transform.position = origobj.transform.position;
                copyobj.transform.rotation = origobj.transform.rotation;
                copyobj.transform.localScale = origobj.transform.localScale;
            }
            Selection.activeObject = newobj;
            EditorGUIUtility.PingObject(Selection.activeObject);
        }

        //按照指定的层级选择对象
        private void SelectLayer(LayerMask layer)
        {
            List<GameObject> rootObjects = new List<GameObject>();
            List<Object> result = new List<Object>();
            Scene scene = SceneManager.GetActiveScene();
            scene.GetRootGameObjects(rootObjects);
            foreach (var obj in rootObjects)
            {
                if (obj.GetComponents<RDTSController>().Length == 0)
                {
                    var objs = Global.GatherObjects(obj);
                    foreach (var obje in objs)
                    {
                        var o = (GameObject)obje;
                        if (o.layer == layer)
                        {
                            result.Add(o);
                        }
                    }
                }
            }
            Selection.objects = result.ToArray();
        }

        //为对象设置层级
        private void AddLayer(LayerMask layer)
        {
            var objs = GetAllSelectedObjectsIncludingSub();
            foreach (var obj in objs)
            {
                obj.layer = addLayer;
            }
        }

        //按照指定的材质选择对象
        private void SelectMaterial(Material material)
        {
            if (material == null)
            {
                EditorUtility.DisplayDialog("未选择材质", "Please select a material", "OK");
                return;
            }
            List<GameObject> list = new List<GameObject>();
            var groupcomps = UnityEngine.Resources.FindObjectsOfTypeAll(typeof(MeshRenderer));

            foreach (var comp in groupcomps)
            {
                var gr = (MeshRenderer)comp;
                if (EditorUtility.IsPersistent(gr.transform.root.gameObject))
                    continue;
                if (gr.sharedMaterial.name.Contains(material.name))
                    list.Add(gr.gameObject);
            }

            Selection.objects = list.ToArray();
        }

        //分配/添加 选定的材质
        private void AssignMaterial(Material material)
        {
            if (material == null)
            {
                EditorUtility.DisplayDialog("No new material for asignment selected", "Please select a new material", "OK");
                return;
            }
            if (EditorUtility.DisplayCancelableProgressBar("Collecting objects", "Please wait",
                0))
            {
                EditorUtility.ClearProgressBar();
                return;
            }

            var a = 0;
            List<GameObject> list = GetAllSelectedObjectsIncludingSub();
            foreach (var myobj in list)
            {
                a++;
                float progress = (float)a / (float)list.Count;
                var renderer = myobj.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    if (EditorUtility.DisplayCancelableProgressBar("Progressing objects", $"Material update on object {a} of {list.Count}",
                        progress))
                    {
                        EditorUtility.ClearProgressBar();
                        return;
                    }
                    Material[] sharedMaterialsCopy = renderer.sharedMaterials;

                    for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                    {
                        sharedMaterialsCopy[i] = material;
                    }

                    renderer.sharedMaterials = sharedMaterialsCopy;
                }
            }
            EditorUtility.ClearProgressBar();
        }

        //简化子对象
        private void SupressChildren(bool supress)
        {
            var objs = GetAllSelectedObjects();
            foreach (var obj in objs)
            {
                Global.HideSubObjects(obj, supress);
            }
        }

        //移除missing scripts
        private void RemoveMissingScripts()
        {
            var objs = GetAllSelectedObjectsIncludingSub();
            foreach (var obj in objs)
            {
                Undo.RegisterCompleteObjectUndo(obj, "Remove missing scripts");
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
            }
        }


       
        //局部坐标归零
        static void LocalPosToZero()
        {
            var obj = Selection.activeGameObject;
            Undo.RecordObject(obj.transform, "Transform to local zero");
            obj.transform.localPosition = Vector3.zero;

        }

        //全局位置归零
        static void GlobalPosToZero()
        {
            var obj = Selection.activeGameObject;
            Undo.RecordObject(obj.transform, "Transform to global zero");
            obj.transform.position = Vector3.zero;
        }

        //局部旋转归零
        static void LocalRotToZero()
        {
            var obj = Selection.activeGameObject;
            Undo.RecordObject(obj.transform, "Transform to local zero");
            obj.transform.localRotation = Quaternion.identity;
        }

        //全局旋转归零
        static void GlobalRotToZero()
        {
            var obj = Selection.activeGameObject;
            Undo.RecordObject(obj.transform, "Transform to local zero");
            obj.transform.rotation = Quaternion.identity;
        }

        //旋转操作
        static void Rotation(Vector3 rotation)
        {
            Undo.RecordObject(sceneObj.transform, "Rotation");
            sceneObj.transform.rotation = sceneObj.transform.rotation * Quaternion.Euler(rotation);
        }

        //创建一个空的子对象
        static void EmptyChild()
        {
            var sel = Selection.activeGameObject;
            var go = new GameObject();
            Undo.RegisterCreatedObjectUndo(go, "Created GameObject");
            if (sel != null)
                go.transform.parent = sel.transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            Selection.activeGameObject = go;
        }

        //创建一个空的父对象
        static void EmptyParent()
        {

            var sel = Selection.activeGameObject;
            var go = new GameObject();

            go.transform.parent = sel.transform.parent;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            var sels = Selection.gameObjects;
            for (int i = 0; i < sels.Length; i++)
            {
                sels[i].transform.parent = go.transform;

            }
            Global.SetExpandedRecursive(go, true);
            Selection.activeGameObject = go;
            EditorApplication.DirtyHierarchyWindowSorting();
        }

        //创建一个信号实体
        static void CreateValueObj(System.Type com)
        {
            var sel = Selection.activeGameObject;
            var go = new GameObject();
            Undo.RegisterCreatedObjectUndo(go, "Create Value Object");

            //有选择对象则将新创建的对象作为其子级
            if(sel !=null)
            {
                go.transform.parent = sel.transform;
            }

            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.name = com.Name;
            go.AddComponent(com);
            //Selection.activeGameObject = go;
        }

        //添加指定类型的组件
        static void AddComponent(System.Type com)
        {
            var sel = Selection.gameObjects;
            for (int i = 0; i < sel.Length; i++)
            {
                Undo.AddComponent(sel[i], com);
            }
        }

    }

#endif
}
