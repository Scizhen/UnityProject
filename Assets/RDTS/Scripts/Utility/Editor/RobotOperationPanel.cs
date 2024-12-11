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
    /// ������Ʋ���
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
            public int ID;//��ʶ��
            public string name;//����
            public GameObject gameobject;//��Ӧ�Ķ���
            public List<Drive> axisDrives = new List<Drive>();//��Ӧ��Drive�ű�
            public List<float> axisValues = new List<float>();//��Ӧ�Ĺؽ�����ֵ
            public Dictionary<int, float> axisValues_Record = new Dictionary<int, float>();//��¼�Ĺؽ���Ƕ�

        }

        protected int RobotItemCount = 0;//�����˶������
        protected Dictionary<int, RobotItem> RobotItemPerID = new Dictionary<int, RobotItem>();//�洢�����˶�����Ϣ
        protected List<string> RobotNames = new List<string>();//��¼����������
        protected int CurrentRobotID = 0;//��ǰ�����Ļ�����ID
        protected List<float> CurrentAxis = new List<float>();//��ǰ�����Ĺؽ���

        float WindowWidth; //<! ���ڿ��
        float WindowHeight;//<! ���ڸ߶�

        [MenuItem("Parallel-RDTS/Utility/Robot Operation Panel", false, 400)]
        static void ShowOperationPanel()
        {
            // Get existing open window or if none, make a new one:
            RobotOperationPanel window = (RobotOperationPanel)EditorWindow.GetWindow(typeof(RobotOperationPanel));
            window.titleContent = RobotOperationPanel.thisTitleContent;//���ñ����ͼ��
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
            //��ʼ������
            Init();

            //���ô��ڱ��⼰ͼ��
            Texture titleIcon = AssetDatabase.LoadAssetAtPath<Texture>(iconPath + "Parallel.png");
            thisTitleContent = new GUIContent("�����˲������", titleIcon);
           
            //�����ز�
            LoadTexture();

          
        }


        void Init()
        {
            SearchRobotItem();
          
            if (RobotItemPerID.Count != 0)
            {
                CurrentRobotID = 1;
                CurrentAxis = RobotItemPerID[0].axisValues;//���ó�ʼ����Ĺؽ���Ƕ�ֵ

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
            resizerStyle.normal.background = EditorGUIUtility.Load("icons/d_AvatarBlendBackground.png") as Texture2D;//Ĭ�ϵ�Unity�༭������(����ͼ��)


            icon_refresh = EditorGUIUtility.FindTexture("Refresh");
            icon_help = EditorGUIUtility.FindTexture("d__Help@2x");
            icon_close = icon_delete = EditorGUIUtility.FindTexture("d_winbtn_win_close");
        }



        void OnGUI()
        {
            if (HasOpenInstances<RobotOperationPanel>())//�����Ƿ��
            {
                WindowWidth = position.width;//��ȡ���ڿ��
                WindowHeight = position.height;//��ȡ���ڸ߶�

                DrawToolbar();//����������

                GUILayout.BeginHorizontal();//*
                DrawItemMenu(AreaItemMenu.x, AreaItemMenu.y, AreaItemMenu.width, AreaItemMenu.height);//���ƻ�������Ŀ�˵�
                DrawResizer1(AreaResizer1.x, AreaResizer1.y, AreaResizer1.width, AreaResizer1.height);//���Ƶ�����1
                DrawItemContent(AreaItemContent.x, AreaItemContent.y, AreaItemContent.width, AreaItemContent.height);//���ƻ�������Ŀ��������
                GUILayout.EndHorizontal();//*

                SetAreaRectParam();//�����������
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

        /// <summary>���ڲ˵�������</summary>
        void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            //ˢ�£����»�ȡ�����еĻ����˶���
            if (GUILayout.Button(icon_refresh, EditorStyles.toolbarButton, GUILayout.Width(50), GUILayout.Height(ToolbarHeight)))
            {
                Init();
            }
            //�Ƿ���סIntroduction����
            isSync = GUILayout.Toggle(isSync, new GUIContent("Synchronize"), EditorStyles.toolbarButton, GUILayout.Width(100), GUILayout.Height(ToolbarHeight));


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
        /// �����������Ͳ˵���
        /// </summary>
        void DrawItemMenu(float x, float y, float width, float height)
        {
            Rect_menu = new Rect(x, y, width, height);
            GUILayout.BeginArea(Rect_menu);
            _scrollview1 = GUILayout.BeginScrollView(_scrollview1);//<! ������

            GUILayout.Space(7);
            for (int i = 0; i < RobotNames.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(7);
                string typeName = RobotNames[i];
                if (GUILayout.Button(typeName, GUILayout.Width(menuItemWidth), GUILayout.Height(menuItemHeight)))
                {
                    SelectGameObjectInScene(RobotItemPerID[i]?.gameobject);//ѡ�д˶���
                    CurrentRobotID = RobotItemPerID[i].ID;//ѡ�ж����ID
                    CurrentAxis = RobotItemPerID[i].axisValues;//ѡ�ж���Ĺؽ���Ƕ�ֵ
                }
                GUILayout.EndHorizontal();

            }



            GUILayout.EndScrollView();//<! ������
            GUILayout.EndArea();
        }


        private Rect Rect_content;
        Vector2 _scrollview2;
        protected int horizontalSize = 2;//ÿ�л��Ƶ��豸����
        /// <summary>
        /// ���ƻ�������Ŀ����
        /// </summary>
        void DrawItemContent(float x, float y, float width, float height)
        {
            Rect_content = new Rect(x, y, width, height);
            GUILayout.BeginArea(Rect_content);
            _scrollview2 = GUILayout.BeginScrollView(_scrollview2);//<! ������

            //��ʾ��ǰ����������
            GUILayout.BeginHorizontal();
            GUILayout.Space(7);
            string name = (CurrentRobotID != 0) ? RobotItemPerID[CurrentRobotID - 1].name : "none";
            GUILayout.Box($"Name��{name}", GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            for(int i=0; i< CurrentAxis.Count; i++)
            {
                CurrentAxis[i] = DrawOneAxis(i, CurrentAxis[i]);
                GUILayout.Space(10);
            }
            


            GUILayout.EndScrollView();//<! ������
            GUILayout.EndArea();
        }


        float DrawOneAxis(int index,float value)
        {
            float axisValue = value;
            //�����˹ؽ������
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(7);
            GUILayout.Label($"Axis{index+1}��");//���Ŵ�С
            axisValue = EditorGUILayout.Slider(axisValue, -180f, 180f);//<! �϶���
            EditorGUILayout.EndHorizontal();


            return axisValue;
        }


        private Rect Rect_resizer1;
        /// <summary>
        /// ��ȵ�����1
        /// </summary>
        void DrawResizer1(float x, float y, float width, float height)
        {
            Rect_resizer1 = new Rect(x, y, width, height);
            GUILayout.BeginArea(new Rect(Rect_resizer1.position + (Vector2.right * resizerWidth1), new Vector2(4, height * 3)), resizerStyle);
            GUILayout.EndArea();

            EditorGUIUtility.AddCursorRect(Rect_resizer1, MouseCursor.ResizeHorizontal);//�ı������״����ʾ���ָ��λ�ڵ�����Сλ��
        }







        #endregion


        #region AreaParameter

        private AreaRectParam AreaItemMenu = new AreaRectParam();
        private AreaRectParam AreaItemContent = new AreaRectParam();
        private AreaRectParam AreaResizer1 = new AreaRectParam();

        private float resizerWidth1 = 7f;//������1��ȣ��˵�������������֮�䣩
        private float menuWidthRatio = 0.15f;//�˵���ռ���ڿ�ȱ���


        void SetAreaRectParam()
        {
            //�˵������Rect����
            AreaItemMenu.x = 0;//*
            AreaItemMenu.y = 0 + ToolbarHeight;
            AreaItemMenu.width = WindowWidth * menuWidthRatio;//*
            AreaItemMenu.height = WindowHeight - ToolbarHeight;

            //������1��Rect����
            AreaResizer1.x = AreaItemMenu.width;//*
            AreaResizer1.y = 0 + ToolbarHeight;
            AreaResizer1.width = resizerWidth1;//*
            AreaResizer1.height = WindowHeight - ToolbarHeight;

            //�������������Rect����
            AreaItemContent.x = AreaItemMenu.width + AreaResizer1.width;//*
            AreaItemContent.y = 0 + ToolbarHeight;
            AreaItemContent.width = WindowWidth * (1 - menuWidthRatio) - AreaResizer1.width;//*
            AreaItemContent.height = WindowHeight - ToolbarHeight;


        }


        private bool isResizing;//�Ƿ��ڵ�������ߴ�
        private bool isResizingWidth1;//�Ƿ��ڵ�����һ��������ߴ�
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

                        isResizingWidth1 = (Rect_resizer1.Contains(e.mousePosition)) ? true : false;//�ж��Ƿ��ڵ����߶�
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
                if (isResizingWidth1 && e.mousePosition.x > WindowWidth * 0.1 && e.mousePosition.x < WindowWidth * 0.3)
                {
                    menuWidthRatio = e.mousePosition.x / WindowWidth;
                }
               

                // QM.Log("widthSizeRatio��" + widthSizeRatio + " " + "heightSizeRatio: " + heightSizeRatio);

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
           

            QM.Log("�������Ļ�����������" + RobotItemCount.ToString());
        }


        /// <summary>
        /// �ڳ�����ѡ��һ��ָ������
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
        /// ����ѡ�����Ϊ��
        /// </summary>
        void SelectObjectIsNull()
        {
            Selection.objects = null;
        }






        #endregion



















    }

#endif
}
