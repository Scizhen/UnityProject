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
    /// ��ݲ�����壺��Hierarchy��Scene��ͼ������ؿ�ݲ���
    /// ע�⣺���Խ���ť���ֻ���ͼƬ����������
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

        //�۵���ͷ��ǩ���۵�״̬��true��չ��  false���۵���
        private bool isShow_control = true;//��ganmeobject�Ŀ���
        private bool isShow_valueobj = true;//��ֵ����Ĳ���
        private bool isShow_addcomponent = true;//��ӽű�


        [MenuItem("Parallel-RDTS/Utility/Operation Panel", false, 400)]
        static void ShowOperationPanel()
        {
            // Get existing open window or if none, make a new one:
            OperationPanel window = (OperationPanel)EditorWindow.GetWindow(typeof(OperationPanel));
            window.titleContent = OperationPanel.thisTitleContent;//���ñ����ͼ��
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
            //���ô��ڱ��⼰ͼ��
            Texture titleIcon = AssetDatabase.LoadAssetAtPath<Texture>(iconPath + "Parallel.png");
            thisTitleContent = new GUIContent("��ݲ������", titleIcon);
          
        }

        Vector2 _scrollview;
        void OnGUI()
        {
            if (HasOpenInstances<OperationPanel>())//�豸�ⴰ���Ƿ��
            {
                TitleMenu();//�˵���

                _scrollview = GUILayout.BeginScrollView(_scrollview);//<! ������
                PanelContent(index);//���ݻ���
                GUILayout.EndScrollView();//<! ������


            }

        }


        List<string> operationType = new List<string>() { "Hierarchy Operation", "Scene Operation" };//���������е��豸��������
        private int index = 0;
        /// <summary>
        ///�����������⣩
        /// </summary>
        void TitleMenu()
        {
            string[] moduleType = operationType.ToArray();
            index = GUILayout.Toolbar(index, moduleType);//��moduleType�����е�����ת���ɶ�Ӧ��������ť(����)
            //index = GUILayout.Toolbar(index, moduleType, panelGUISKin.textArea);//��moduleType�����е�����ת���ɶ�Ӧ��������ť(����)
        }


        void PanelContent(int index)//���ݻ���
        {
            switch(index)
            {
                case 0://�Բ㼶���Ĳ���
                    PanelContent_Hierarchy();
                    break;

                case 1://��Scene�ж���Ĳ���
                    PanelContent_Scene();
                    break;

            }
        }

        /// <summary>
        /// ������������
        /// </summary>
        void PanelContent_Hierarchy()
        {
            float width = position.width;//��ȡ���ڿ��


            //��10��λ
            GUILayout.BeginVertical();
            GUILayout.Width(10);
            GUILayout.EndVertical();   

            EditorGUILayout.Separator();

         //<! ��ֱ����
            GUILayout.BeginVertical();

            ///1
            EditorGUILayout.BeginHorizontal("Box");
            if (GUILayout.Button("Isolate", GUILayout.Width((width / 4)-10)))//ֻ��ʾѡ�еĶ���
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
            if (GUILayout.Button("All Visible", GUILayout.Width((width / 4) -10)))//ʹ���ж���ɼ�
            {
                VisibleAll(true, true);
            }
            EditorGUILayout.EndHorizontal();

            ///2
            EditorGUILayout.BeginHorizontal("Box");

            if (GUILayout.Button("Simplify Selected", GUILayout.Width((width / 3)-10)))//ѡ����󣬽��䲻����RDTSBehavior��ű����Ӷ�������
            {
                SupressChildren(true);
            }
            if (GUILayout.Button("Recover Selected", GUILayout.Width(width / 3)))//ѡ����󣬽����Ӷ�����Ϊ��Hierarchy����пɼ�
            {
                SupressChildren(false);
            }
            if (GUILayout.Button("Recover All", GUILayout.Width((width / 3) - 10)))//ʹ���ж�����Hierarchy����пɼ�
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
            if (GUILayout.Button("Select Visible", GUILayout.Width((width/2)-10)))//ѡ�����пɼ��Ķ���
            {
                SelectVisible(true);
            }
            if (GUILayout.Button("Select Invisible", GUILayout.Width((width/2)-10)))//ѡ�����в��ɼ��Ķ���
            {
                 SelectVisible(false);
            }
            EditorGUILayout.EndHorizontal();

            ///5
            EditorGUILayout.BeginHorizontal("Box");
            if (GUILayout.Button("Collapse same level", GUILayout.Width((width/2)-10)))//��ѡ�еĶ����۵�
            {
                CollapseSameLevel();
            }

            if (GUILayout.Button("Expand same level", GUILayout.Width((width/2) - 10)))//��ѡ�еĶ���չ��
            {
                ExpandSameLevel();
            }
            EditorGUILayout.EndHorizontal();

            ///6
            EditorGUILayout.BeginHorizontal("Box");
            if (GUILayout.Button("Remove missing scripts", GUILayout.Width(width/2)))//�Ƴ�ѡ�ж���ġ�missing scripts��
            {
                RemoveMissingScripts();
            }
            EditorGUILayout.EndHorizontal();

            ///7
            EditorGUILayout.BeginHorizontal("Box");
            if (GUILayout.Button("Copy to empty Parent", GUILayout.Width(width/2)))//��ѡ�еĶ����ƣ�����Ϊ������
            {
                CopyObjectsToNewEmptyParent();
            }
            EditorGUILayout.EndHorizontal();


            ///8
            EditorGUILayout.BeginHorizontal("Box");//Ϊѡ��������layer
            GUILayout.Label("Choose a layer��", GUILayout.Width((width/3) - 10));
            addLayer = EditorGUILayout.LayerField("", addLayer, GUILayout.Width(width/3));
            if (GUILayout.Button("Add Layer", GUILayout.Width((width/3) - 10)))
            {
                AddLayer(addLayer);
            }
            EditorGUILayout.EndHorizontal();

            ///9
            EditorGUILayout.BeginHorizontal("Box");//����layer���Ҷ���
            GUILayout.Label("Choose a layer��", GUILayout.Width((width/3) - 10)); 
            selectedLayer = EditorGUILayout.LayerField("", selectedLayer, GUILayout.Width(width/3));
            if (GUILayout.Button("Select Layer", GUILayout.Width((width/3)-10)))
            {
                SelectLayer(selectedLayer);
            }
            EditorGUILayout.EndHorizontal();

            ///10
            EditorGUILayout.BeginHorizontal("Box");//Ϊѡ��Ķ���������
            GUILayout.Label("Choose a material��", GUILayout.Width((width/3) - 10));
            selectedAddMaterial = (Material)EditorGUILayout.ObjectField(selectedAddMaterial, typeof(Material), false, GUILayout.Width(width / 3));
            if (GUILayout.Button("Add Material", GUILayout.Width((width/3) - 10)))
            {
                AssignMaterial(selectedAddMaterial);
            }
            EditorGUILayout.EndHorizontal();

            ///11
            EditorGUILayout.BeginHorizontal("Box");//����material���Ҷ���
            GUILayout.Label("Choose a material��", GUILayout.Width((width/3) - 10));
            selectedMaterial = (Material)EditorGUILayout.ObjectField(selectedMaterial, typeof(Material), false, GUILayout.Width(width / 3));
            if (GUILayout.Button("Select Material", GUILayout.Width((width/3) - 10)))
            {
                SelectMaterial(selectedMaterial);
            }
            EditorGUILayout.EndHorizontal();



            GUILayout.EndVertical();

        }


        float height = 20;
        int PositionFlag = 0;//λ�ñ�־λ��1���ֲ��������  2�������������
        int RotateFlag = 0;//��ת��־λ��1��x����ת  2��x�ᷴת  3��y������   4��y�ᷴ��   5��z������   6��y�ᷴ��  7:�ֲ�����  8��ȫ�ֹ���
        int EmptyFlag = 0;//�ն����־λ��1�������յ��Ӷ���  2�������յĸ�����
        int ValueObjFlag = 0;//�źű�־λ��1��ValueInputBool  2��ValueInputInt  3��ValueInputFloat  4��ValueOutputBool  5��ValueOutputInt  6��ValueOutputFloat
        int ComponentFlag = 0;//�����־λ��
        void PanelContent_Scene()
        {
            float width = position.width;//��ȡ���ڿ��


            //��10��λ
            GUILayout.BeginVertical();
            GUILayout.Width(10);
            GUILayout.EndVertical();

            EditorGUILayout.Separator();

            //<! ��ֱ����0
            GUILayout.BeginVertical();

            isShow_control = EditorGUILayout.BeginFoldoutHeaderGroup(isShow_control, "Control Gameobject");
            if(isShow_control)
            {
                //λ��
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

                //��ת
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

                //�ն���
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
                if (GUILayout.Button("Change Direction", GUILayout.Width(width / 3-10), GUILayout.Height(height)))//��ѡ�е�ֵ����任������뷽��
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

        //Scene��ͼ�Ļ���
        void OnSceneGUI(SceneView scene)
        {
            if (Selection.activeObject != null)
                if (Selection.activeObject.GetType() == typeof(GameObject))
                    sceneObj = (GameObject)Selection.activeObject;
                else
                    sceneObj = null;

            
            ScenePosition();//λ�ò���
            SceneRotation();//��ת����
            SceneEmpty();//�ն��󴴽�
            SceneAddValueObj();//ֵ�������
            SceneComponent();//������


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
                    ///AddComponent(typeof(Station));//֮�����¸Ľ��������
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

        //ʹ���ж���ɼ�/����
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

        //������ʾѡ��Ķ���
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

        //ʹ���б����򻯶����ء��Ķ���ɼ�
        private void UnhideAll()
        {
            List<GameObject> rootObjects = new List<GameObject>();
            Scene scene = SceneManager.GetActiveScene();//��ǰ����ĳ���
            scene.GetRootGameObjects(rootObjects);//���س����е����и���Ϸ���� rootObjects���ڽ��շ��صĶ���
            foreach (var obj in rootObjects)
            {
                if (obj.GetComponents<RDTSController>().Length == 0)
                {
                    var objs = Global.GatherObjects(obj);
                    Global.HideSubObjects(obj, false);
                }
            }
        }

        //�ɼ�/����ѡ��Ķ���
        private void Visible(bool visible)
        {
            var objs = GetAllSelectedObjects();
            foreach (var obj in objs)
            {
                Global.SetVisible((GameObject)obj, visible);
            }
        }

        //��ס/����ѡ��Ķ���
        private void Lock(bool lockelement)
        {
            var objs = GetAllSelectedObjectsIncludingSub();
            foreach (var obj in objs)
            {
                Global.SetLockObject(obj, lockelement);
            }
        }

        //ѡ������visible/invisible�Ķ���
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

        //�۵�ͬһ����������������
        private void CollapseSameLevel()
        {
            var objs = GetSameLevelObjects();
            foreach (var obj in objs)
            {
                Global.SetExpandedRecursive(obj, false);
            }
        }

        //չ��ͬһ����������������
        private void ExpandSameLevel()
        {
            var objs = GetSameLevelObjects();
            foreach (var obj in objs)
            {
                Global.SetExpandedRecursive(obj, true);
            }
        }

        //��ѡ��Ķ����Ƶ�һ���µĸ�������
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

        //����ָ���Ĳ㼶ѡ�����
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

        //Ϊ�������ò㼶
        private void AddLayer(LayerMask layer)
        {
            var objs = GetAllSelectedObjectsIncludingSub();
            foreach (var obj in objs)
            {
                obj.layer = addLayer;
            }
        }

        //����ָ���Ĳ���ѡ�����
        private void SelectMaterial(Material material)
        {
            if (material == null)
            {
                EditorUtility.DisplayDialog("δѡ�����", "Please select a material", "OK");
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

        //����/��� ѡ���Ĳ���
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

        //���Ӷ���
        private void SupressChildren(bool supress)
        {
            var objs = GetAllSelectedObjects();
            foreach (var obj in objs)
            {
                Global.HideSubObjects(obj, supress);
            }
        }

        //�Ƴ�missing scripts
        private void RemoveMissingScripts()
        {
            var objs = GetAllSelectedObjectsIncludingSub();
            foreach (var obj in objs)
            {
                Undo.RegisterCompleteObjectUndo(obj, "Remove missing scripts");
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
            }
        }


       
        //�ֲ��������
        static void LocalPosToZero()
        {
            var obj = Selection.activeGameObject;
            Undo.RecordObject(obj.transform, "Transform to local zero");
            obj.transform.localPosition = Vector3.zero;

        }

        //ȫ��λ�ù���
        static void GlobalPosToZero()
        {
            var obj = Selection.activeGameObject;
            Undo.RecordObject(obj.transform, "Transform to global zero");
            obj.transform.position = Vector3.zero;
        }

        //�ֲ���ת����
        static void LocalRotToZero()
        {
            var obj = Selection.activeGameObject;
            Undo.RecordObject(obj.transform, "Transform to local zero");
            obj.transform.localRotation = Quaternion.identity;
        }

        //ȫ����ת����
        static void GlobalRotToZero()
        {
            var obj = Selection.activeGameObject;
            Undo.RecordObject(obj.transform, "Transform to local zero");
            obj.transform.rotation = Quaternion.identity;
        }

        //��ת����
        static void Rotation(Vector3 rotation)
        {
            Undo.RecordObject(sceneObj.transform, "Rotation");
            sceneObj.transform.rotation = sceneObj.transform.rotation * Quaternion.Euler(rotation);
        }

        //����һ���յ��Ӷ���
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

        //����һ���յĸ�����
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

        //����һ���ź�ʵ��
        static void CreateValueObj(System.Type com)
        {
            var sel = Selection.activeGameObject;
            var go = new GameObject();
            Undo.RegisterCreatedObjectUndo(go, "Create Value Object");

            //��ѡ��������´����Ķ�����Ϊ���Ӽ�
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

        //���ָ�����͵����
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
