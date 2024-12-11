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
    /// ģ��/���ʿ⣺��ģ������ʼ��ϳ�һ���⣬ͨ��asset�ļ��洢���ݡ�
    /// </summary>
    public class DeviceWindow : EditorWindow
    {
        ///�����ʾģʽ  
        protected enum LibraryMode
        {
            ModelLibrary,//ģ�Ϳ�
            MaterialLibrary//���ʿ�
        }
        protected LibraryMode libraryMode = LibraryMode.ModelLibrary;

        ///������ʾ/���ģʽ
        protected enum ShowMode
        {
            LibraryMode,//���������ģʽ
            ModuleMode//���豸�����ģʽ
        }
        protected ShowMode showMode = ShowMode.LibraryMode;


        float ScreenWidth; //<! ���ڿ��
        float ScreenHeight;//<! ���ڸ߶�
        protected int horizontalSize = 5;//ÿ�л��Ƶ��豸����

        ///�ָ���
        Rect splitterRect;
        float splitterPos;//�ָ���λ��
        float splitterWidth = 10;//�ָ������
        bool dragging;//�϶����


        [MenuItem("Parallel-RDTS/Window/DeviceWindow", false, 200)]
        private static void ShowWindow()
        {
            DeviceWindow deviceWindow = (DeviceWindow)EditorWindow.GetWindow(typeof(DeviceWindow));
            deviceWindow.titleContent = DeviceWindow.thisTitleContent;//���ñ����ͼ��
            deviceWindow.minSize = new Vector2(300, 200);
            deviceWindow.splitterPos = 75;
            deviceWindow.Show(); 
        }

 
        /// <summary>�趨����ÿ�л��Ƶ�ģ������</summary>
        public  void SetHorizontalSize(int size = 5)
        {
            horizontalSize = size;
        }

        //���ڽ������ݻ���
        void OnGUI()
        {

            if (HasOpenInstances<DeviceWindow>())//�豸�ⴰ���Ƿ��
            {
                ScreenWidth = position.width;//��ȡ���ڿ��
                ScreenHeight = position.height;//��ȡ���ڸ߶�

                DeviceWindowOperate();
                ///TitleMenu();//�ɵĲ˵�������ã�

                if (viewIndex == 0)//������Ϊ0������ƻ�ӭ����
                {
                    WelcomeView();
                }
                else//����ģ��/���ʿ����
                {
                    GUILayout.BeginHorizontal();//*ˮƽ����
                    LeftView();//<! ���Ĳ˵���
                    DrawVerticalSplitter();//Splitter �ָ���
                    RightView();//<! �Ҳ��Ӧ�˵���Ĵ�������
                    GUILayout.EndHorizontal();//*ˮƽ����/
                }

             
                ///�ָ������϶�����
                SplitterControl(60, 180);
                ///��ק�����ѡ�е��豸
                if (_dragCreate)
                    UserInterfaceEdit.StartDrag(_dragObjName, _dragCreate);

            }


        }



        /// <summary>
        /// ���Ʒָ���
        /// </summary>
        void DrawVerticalSplitter()
        {
            GUILayout.Box("",
                   GUILayout.Width(splitterWidth),
                   GUILayout.MaxWidth(splitterWidth),
                   GUILayout.MinWidth(splitterWidth),
                   GUILayout.ExpandHeight(true));
            splitterRect = GUILayoutUtility.GetLastRect();

            EditorGUIUtility.AddCursorRect(splitterRect, MouseCursor.ResizeHorizontal);//�ı������״����ʾ���ָ��λ�ڵ�����Сλ��

        }


        /// <summary>
        /// �ָ������϶�����
        /// </summary>
        /// <param name="mixValue">���϶�����С����</param>
        /// <param name="maxValue">���϶���������</param>
        void SplitterControl(float mixValue = 55, float maxValue = 255)
        {
            // Splitter events �ָ����¼�
            if (Event.current != null)
            {
                switch (Event.current.rawType)
                {
                    case EventType.MouseDown:
                        if (splitterRect.Contains(Event.current.mousePosition))//�Ƿ�ǰ���λ���ڷָ�����Χ��
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

                ///�޷�
                if (splitterPos > maxValue)//���ֵ
                    splitterPos = maxValue;
                if (splitterPos < mixValue)//��Сֵ
                    splitterPos = mixValue;
            }
        }


        /// <summary>
        /// �����豸�ⴰ����ק��Scene�����д����豸���¼�
        /// </summary>
        /// <param name="sceneView"></param>
        void DragEvent(SceneView sceneView)
        {
            Event e = Event.current;
            DragAndDrop.visualMode = DragAndDropVisualMode.Generic;

            Vector3 mousePos = UserInterfaceEdit.GetMousePosToScene();
            mousePos = new Vector3(mousePos.x/5 -2 , mousePos.y/50+0.4f, mousePos.z/5 -2);///��������λ��

            if (e.button == 0 && e.type == EventType.DragExited && _dragCreate)//<! ע��ʹ��e.type������Event.current.type
            {
                Instantiate(_dragObj, mousePos, Quaternion.identity);

                SceneView.duringSceneGui -= DragEvent;//ɾ���¼�
                _dragCreate = false;
                /// ShowNotification(new GUIContent($"�����{_dragObjName}"));
            }

        }


        void OnInspectorUpdate()
        {
            ///none...

        }


        int viewIndex = 0;//�Ҳ���ͼ������
        Vector2 posLeft;
        float leftPos => splitterPos - 5;
        float leftViewHeight = 30f;
        /// <summary>
        /// ���˵������
        /// </summary>
        void LeftView()
        {
            int count = menuItem.Count;//���͸����������˵������

            // Left view
            posLeft = GUILayout.BeginScrollView(posLeft,
                GUILayout.Width(splitterPos),
                GUILayout.MaxWidth(splitterPos),
                GUILayout.MinWidth(splitterPos));//<! ������

            leftViewHeight = (ScreenHeight - menuHeight - 40) / count;//ÿ���˵���߶�

            ///���ݶ�ȡ����menuItem�б����������Ĳ˵���
            for (int i=0; i< count; i++)
            {
                string typeName = menuItem[i];
                if (GUILayout.Button(typeName, GUILayout.Width(leftPos), GUILayout.Height(leftViewHeight)))
                {
                    if (showMode == ShowMode.LibraryMode)//<! ֻ�ڡ��������ģʽ�»���Ч
                    {
                        viewIndex = i + 1;//��Ӧ�Ҳ���ͼ��������
                        ///QM.Log(typeName);
                    }
                   
                }
            }

            GUILayout.EndScrollView();//<! ������
        }


        Vector2 posRight;
        /// <summary>
        /// �Ҳ����ݻ���
        /// </summary>
        void RightView()
        {
            // Right view
            posRight = GUILayout.BeginScrollView(posRight, GUILayout.ExpandWidth(true));//<! ������

            if(viewIndex > 0)//�����Ŵ���0
            {
                AdjustHorizontalSize();//��̬����ÿ�л�������

                if (showMode == ShowMode.LibraryMode)///һ���豸ģ��Ŀ�
                    ModuleLibraryContent(modulelibraries, horizontalSize, viewIndex - 1);//ģ�����ݻ���
                else///�����豸ģ��
                    OneModuleContent_InDetail(showModule);
            }

            GUILayout.EndScrollView();//<! ������
        }


        /// <summary>
        /// ���ݴ��ڿ�Ⱥ�ģ��(ģ��/����)���ʵ�ֶ�̬��������ÿ�л��Ƶĸ���
        /// </summary>
        void AdjustHorizontalSize()
        {
            float width = ScreenWidth - splitterPos - splitterWidth - 50;//�Ҳ��������Ĵ��¿��
            int horizontalCount_model = Mathf.FloorToInt(width / modelModuleWidth);//ÿ�л���ģ�͵ĸ���
            int horizontalCount_material = Mathf.FloorToInt(width / materialModuleWidth);//ÿ�л��Ʋ��ʵĸ���

            //�޷�������ÿ�л���һ��
            if (horizontalCount_model < 1)
                horizontalCount_model = 1;
            if (horizontalCount_material < 1)
                horizontalCount_material = 1;

            horizontalSize = (libraryMode == LibraryMode.ModelLibrary) ? horizontalCount_model : horizontalCount_material;
        }


        #region WelcomeView
        /// <summary>
        /// ��ӭ���棬˵���˴��ڵ�����
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


        #region �豸����������

        float menuHeight = 20f;
        string libraryName = "ģ�Ϳ�";
        bool libraryNameFlag = true;//Ϊtrue��ʾģ�Ϳ⣬Ϊfalse��ʾ���ʿ�
        /// <summary>
        /// �豸�ⴰ�ڵ���ز���
        /// </summary>
        void DeviceWindowOperate()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            //�л���ť���л�ģ�Ϳ�/���ʿ�
            if (GUILayout.Button(libraryName, EditorStyles.toolbarButton, GUILayout.Width(100), GUILayout.Height(menuHeight)))
            {
                libraryNameFlag = !libraryNameFlag;//ȡ��
                libraryName = (libraryNameFlag) ? "ģ�Ϳ�" : "���ʿ�";
                libraryMode = (libraryNameFlag) ? LibraryMode.ModelLibrary : LibraryMode.MaterialLibrary;
                RedoWindowContent();//�ػ洰������
                showMode = ShowMode.LibraryMode;//������ʾģʽ
                viewIndex = 1;
                ShowNotification(new GUIContent($"���л�Ϊ{libraryName}"));
            }
            //���ô�������Ϊ��ʼ��Ĭ������
            if (GUILayout.Button("����", EditorStyles.toolbarButton, GUILayout.Width(50), GUILayout.Height(menuHeight)))
            {
                splitterPos = 75;
                viewIndex = 0;
                showMode = ShowMode.LibraryMode;
                Repaint();
            }
            //���水ť������Դ������������޸�
            if (GUILayout.Button("����", EditorStyles.toolbarButton, GUILayout.Width(50), GUILayout.Height(menuHeight)))
            {
                SaveWindowContent();
                ShowNotification(new GUIContent("�豸���ѱ���"));
                Debug.Log("����");
            }
            //ˢ�°�ť����DataAsset���޸�ʱ�����¶�ȡ���ػ洰��
            if (GUILayout.Button(image_Refresh, EditorStyles.toolbarButton, GUILayout.Width(50), GUILayout.Height(menuHeight)))
            {
                showMode = ShowMode.LibraryMode;//������ʾģʽ
                _editor?.SetZoomRateValue(1);//����Ԥ���������Ŵ�С
                RedoWindowContent();//�ػ洰������
                ShowNotification(new GUIContent("����ˢ��"));
                Debug.Log("ˢ��");
            }

            GUILayout.FlexibleSpace();//������
            
            //������ť����ʾ��β������豸��
            if (GUILayout.Button(image_Help, EditorStyles.toolbarButton, GUILayout.Width(24), GUILayout.Height(menuHeight)))
            {
                ////PopupWindow.Show(new Rect(this.position.width / 2, 20, 0, 0), new DeviceTipsWindow());//PopupWindow�Ĵ�С�̶���ֻ������λ��
                //�򿪵���
                PopWindowHelp helpWindow = (PopWindowHelp)EditorWindow.GetWindow(
                       typeof(PopWindowHelp),
                       true,
                       "Help");
                var content = helpWindow.ReadHelpData(0);
                helpWindow.SetShowContents(content);
                helpWindow.ShowUtility();
                ///Debug.Log("����");
            }
            //�رհ�ť���رյ�ǰ����
            if (GUILayout.Button(image_close, EditorStyles.toolbarButton, GUILayout.Width(24), GUILayout.Height(menuHeight)))
            {
                if (EditorUtility.DisplayDialog("�رմ���", "�Ƿ�ȷ���ر��豸�ⴰ�ڣ�", "yes", "no"))
                {
                    this.Close();
                    ///Debug.Log("�ر�");
                }

            }


            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);

        }


        /// <summary>
        /// ˢ�µ�ǰ���ڵ�����
        /// </summary>
        void RedoWindowContent()
        {
            devicelibraryData = AssetDatabase.LoadAssetAtPath<DeviceLibraryData>(dataPath + "DeviceLibraryData.asset");//���¶�ȡһ��asset�ļ�
            modulelibraries.Clear();
            modulelibraries = (libraryMode == LibraryMode.ModelLibrary) ? ReadLibraryData(devicelibraryData) : ReadLibraryData(materialLibraryData);//���¶�ȡ������Դ
            Repaint();
        }


        void SaveWindowContent()
        {
            //nothing...

        }


        #endregion



        #region �豸����������

        List<string> menuItem = new List<string>();//���������е��豸��������

        # region ����ǾɵĲ˵������ƣ������ã�
        private int index = 0;
        /// <summary>�豸�⹤���������⣩</summary>
        void TitleMenu()
        {
            string[] moduleType = menuItem.ToArray();
            index = GUILayout.Toolbar(index, moduleType);//��moduleType�����е�����ת���ɶ�Ӧ��������ť(����)
        }
        #endregion

        private float modelModuleWidth = 200;
        private float materialModuleWidth = 140;

        private bool _dragCreate = false;//�Ƿ������ק
        private GameObject _dragObj;//��ק�Ķ���
        private string _dragObjName;//��ק��������
        /// <summary>
        /// һ��ģ�������
        /// </summary>
        /// <param name="module">Ҫ���Ƶ�ģ��</param>
        /// <param name="isModel">�Ƿ����ģ��Ϊģ�ͣ�Ĭ��Ϊģ�Ͷ��ǲ���</param>
        void OneModuleContent(Module module, bool isModel = true)
        {
            if (module == null)
                return;

            if(isModel)/*һ��ģ��ģ�����*/
            {
                EditorGUILayout.BeginVertical("Box", GUILayout.Width(modelModuleWidth));

                EditorGUILayout.BeginHorizontal("Box");
                ///ģ������
                EditorGUILayout.LabelField(module.name, GUILayout.Width(140));
                ///�������һ����ק����ģ�͵Ĳ������ٴε����ȡ���ô���ק�¼�
                /////<! ��module.textrueΪ�գ�����ʾimage_model����Ϊ��ť��ͼ��
                var image_model = AssetPreview.GetAssetPreview(module.model) as Texture;//��ȡ���ʵ���ԴԤ������
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
                        SceneView.duringSceneGui += DragEvent;//����¼������¼�����Scene��ͼÿ�ε���OnGUI����ʱ���ջص�
                    }
                    else
                        SceneView.duringSceneGui -= DragEvent;

                }
                EditorGUILayout.EndHorizontal();
                ///ģ�͵�������Ϣ
                GUILayout.TextArea(module.description);
                GUILayout.Space(10);
                ///�������뷽ʽ��Scene�����ģ��
                module.modelPos = EditorGUILayout.Vector3Field("Add location", module.modelPos);//���λ��
                if (GUILayout.Button("Click to add"))// �����Ӵ��豸
                {
                    GameObject obj = Instantiate(module.model, module.modelPos, module.model.transform.rotation);
                    QM.Log("���:" + module.name);
                }
                ///�鿴��ģ�͵���ϸ��Ϣ��������ά��ͼԤ������ϸ��Ϣ��
                if (GUILayout.Button("Click to view"))//����鿴���豸
                {
                    showMode = ShowMode.ModuleMode;
                    showModule = module;
                    QM.Log("�鿴:" + module.name);
                }

                EditorGUILayout.EndVertical();
            }
            else/*һ������ģ��Ļ���*/
            {
                EditorGUILayout.BeginVertical("Box", GUILayout.Width(materialModuleWidth));

                EditorGUILayout.BeginHorizontal("Box");
                ///��������
                EditorGUILayout.LabelField(module.name, GUILayout.Width(80));
                ///��ȡ���ʵ���ԴԤ������
                var image_material = AssetPreview.GetAssetPreview(module.material);
                //ͼ�갴ťֻ��ӵ�ǰѡ���Ҿ���meshRender�Ķ���Ĳ���
                if (GUILayout.Button(image_material, skin_modelLibrary.button, GUILayout.Width(60), GUILayout.Height(60)))
                {
                    ///Ϊѡ�еĶ�����Ӳ��ʣ����ͼ�����Ӳ��ʣ�
                    bool addMaterial = UserInterfaceEdit.AddMaterialToGameobject(module.material);
                    if (!addMaterial)
                    {
                        EditorUtility.DisplayDialog("Error", "��ǰѡ�еĶ���Ϊ�ջ򲻾߱���Ӳ��ʵ��������ѡ����ʵĶ���", "OK");
                    }
                    ///Debug.Log("���:" + module.name);
                }

                EditorGUILayout.EndHorizontal();

                ///���ʵ�������Ϣ
                GUILayout.TextArea(module.description);
                GUILayout.Space(10);
                ///Ϊѡ�еĶ�����Ӳ��ʣ���Ϊ���Ӽ��������
                if (GUILayout.Button("Click to use"))//�����Ӵ˲���
                {
                    AddMaterialToSelectedObject(module.material);
                    ///Debug.Log("���:" + module.name);
                }

                EditorGUILayout.EndVertical();
            }
           


        }



        /// <summary>
        /// ��ѡ�ж�����Ӳ���
        /// </summary>
        void AddMaterialToSelectedObject(Material material)
        {
            if (EditorUtility.DisplayDialog("Notice", "�Ƿ񽫵�ǰѡ�ж������Ӷ������Ӳ��ʣ�", "yes", "no"))
            {
                UserInterfaceEdit.AddAddMaterialToGameobjects(material);
            }
            else
            {
                bool addMaterial = UserInterfaceEdit.AddMaterialToGameobject(material);
                if (!addMaterial)
                {
                    EditorUtility.DisplayDialog("Error", "��ǰѡ�еĶ���Ϊ�ջ򲻾߱���Ӳ��ʵ��������ѡ����ʵĶ���", "OK");
                }
            }
        }






        int number_ModuleType = 0;
        /// <summary>
        /// ����һ�����͵��豸��
        /// </summary>
        /// <param name="modulelibrariies">��Ҫ���Ƶ��豸��</param>
        /// <param name="horizontalSize">һ����Ҫ���Ƶ��豸����</param>
        void ModuleLibraryContent(List<ModuleLibrary> modulelibraries, int horizontalSize = 5, int index = 0 )
        {
            number_ModuleType = modulelibraries.Count;//��ȡ��ǰ��ģ����е���������

            if (number_ModuleType == number_ItemType)//���ж϶�ȡ������Դ�����Ƿ�����
            {
                int rows = 0;//(����)����
                int columns = 0;//������������һ��ʱ�����У��еĸ���
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
                modulelibraries = (libraryMode == LibraryMode.ModelLibrary) ? ReadLibraryData(devicelibraryData) : ReadLibraryData(materialLibraryData);//���������ٴν��ж�ȡ
            }

        }




        #endregion

        #region Ԥ������

        Module showModule;//ҪԤ�����豸ģ��
        GameObject _lastGameObject;
        PreviewRenderEditor _editor = null;
        bool _load = true;
        Vector2 _lightRot = new Vector2(75, 20);
        Vector2 _lastLightRot;
        float _scale = 1.25f;

        
        /// <summary>
        /// ģ�͵���ά��ͼԤ��
        /// </summary>
        /// <param name="showModule">Ҫ��ʾ���豸ģ��</param>
        /// <param name="scale">����ֵ</param>
        void ModulePreview(Module showModule, float scale = 1f)
        {
            GameObject showObject = showModule?.model ?? null;//ҪԤ���Ķ���  
            float previewWidth = (ScreenWidth - splitterPos - splitterWidth) / 3;//�Ҳ���ͼ��1/3���

            EditorGUILayout.BeginHorizontal("Box");

            if (_editor == null)
            {
                ///Editor���Ӵ˻����������Ա�Ϊ�Զ�����󴴽��Զ���������ͱ༭��
                _editor = Editor.CreateEditor(this, typeof(PreviewRenderEditor)) as PreviewRenderEditor;
            }

            if (_editor)
            {
                if (_lastLightRot != _lightRot)//����
                {
                    _lastLightRot = _lightRot;
                    _editor.RefreshLightRot(_lightRot);
                }

                _editor.SetZoomRateValue(1 / scale);//���š��Ѹĳ��������ִ����ֶ��϶���������
                _editor.DrawPreview(GUILayoutUtility.GetRect(200, previewWidth));//<! ���Ƶ�Ԥ�����С��ˮƽ�������������ڣ��ʵ��ڸ߶���ֵ����
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
        /// ����һ��ģ����ϸ�Ľ��棺��άԤ��������ϸ����Ϣ��
        /// </summary>
        void OneModuleContent_InDetail(Module showModule)
        {
            float halfWidth = (ScreenWidth - splitterPos - splitterWidth) / 2;//�Ҳ���ͼ��һ����
            float spaceWidth = (ScreenWidth - splitterPos - splitterWidth ) / 3 - 80;//�հ׼��

            Module module = new Module();
            if (showModule != null) module = showModule;

            //showObject = (GameObject)EditorGUILayout.ObjectField("Ԥ��Ԥ����", showObject, typeof(GameObject), true);
            EditorGUILayout.BeginHorizontal("Box");

            if (GUILayout.Button("Back", EditorStyles.toolbarButton, GUILayout.Width(80), GUILayout.Height(menuHeight)))//����
            {
                showMode = ShowMode.LibraryMode;
                showModule = null;
            }
            GUILayout.Space(spaceWidth);
            ///����ģ�����Ŵ�С
            GUILayout.Label("Zoom size��");//���Ŵ�С
            if(_editor != null) _scale = _editor.GetZoomRateValue();
            _scale = EditorGUILayout.Slider( _scale, 0.1f, 5);//<! ������������������룬����Ϊ���Ŵ�С
            /// _lightRot = EditorGUILayout.Vector2Field("��Դ����", _lightRot);    
            EditorGUILayout.EndHorizontal();

            ModulePreview(showModule, _scale);//��ά��ͼԤ����ע�⣺����Ҫʹ��showModule���Ա���ģ���л�ʱ���ٸ�����ά��ͼ����

            EditorGUILayout.BeginHorizontal("Box");

            ///�������һ����ק����ģ�͵Ĳ������ٴε����ȡ���ô���ק�¼�
            if (GUILayout.Button(image_mouseleft, GUILayout.Width(halfWidth / 4), GUILayout.Height(40)))
            {
                _dragCreate = !_dragCreate;
                if (_dragCreate)
                {
                    _dragObj = module?.model;
                    _dragObjName = module?.name;
                    SceneView.duringSceneGui += DragEvent;//����¼������¼�����Scene��ͼÿ�ε���OnGUI����ʱ���ջص�
                }
                else
                    SceneView.duringSceneGui -= DragEvent;

            }

            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginVertical(GUILayout.Width(halfWidth));
            module.modelPos = EditorGUILayout.Vector3Field("", module.modelPos);
            if (GUILayout.Button("Click to add this model"))//��Ӵ��豸
            {
                GameObject obj = Instantiate(module?.model, module.modelPos, Quaternion.identity);
                Debug.Log("���:" + module.name);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);

            ModelDataItem("Name", new string[1] { module?.name });//����
            ModelDataItem("Description", new string[1] { module?.description });//����

            /* �������ϸ��Ϣ����ȡ���صġ�TwinModel���ű��� */
            if (module.model == null)
            {
                EditorGUILayout.Space(20);
                RedLabel("ע�⣺��ģ��Ϊ�գ�");
                return;
            }
            TwinModel script_tm = module?.model?.GetComponent<TwinModel>();
            ShowTwinModelStaticData(script_tm);


        }

        /// <summary>
        /// �����ģ���Ϲ��صġ�TwinModel���ű�����ȡ����ʾ���е�������Ϣ
        /// </summary>
        /// <param name="script_tm"></param>
        void ShowTwinModelStaticData(TwinModel script_tm)
        {
            if (script_tm == null)
            {
                EditorGUILayout.Space(20);
                RedLabel("ע�⣺��ģ��δ���ء�TwinModel���ű���");
                return;
            }
            ///���е�������Ϣ
            ModelDataItem("Model hierarchy", new string[1] { script_tm?.Level.ToString() });//ģ�Ͳ��
            ModelDataItem("Model type", new string[1] { script_tm?.Type.ToString() });//ģ������
            ModelDataItem("Function and Use", new string[1] { script_tm?.FunctionAndUse.ToString() });//��������;
            ModelDataItem("Device model", new string[1] { script_tm?.DeviceBrand.ToString() });//�豸�ͺ�
            ModelDataItem("Size", //�ߴ�
                new string[3]
                {
                    "length","width","height"//"��","��","��"
                },
                new string[3] {
                    script_tm?.modelSize.Length.ToString(),
                    script_tm?.modelSize.Width.ToString(),
                    script_tm?.modelSize.Height.ToString()
                });

            ///����ڡ���е����ģ�͡������ض�����
            if(script_tm?.Type == TwinModelType.RobotArm)
            {
                EditorGUILayout.BeginHorizontal("Box");
                EditorGUILayout.LabelField("D-H Param", skin_modelLibrary.customStyles[4], GUILayout.Width((ScreenWidth - splitterPos - splitterWidth) / 5), GUILayout.Height(25));
                EditorGUILayout.LabelField("", GUILayout.Width(4), GUILayout.Height(25));//���

                string[] tableItem = new string[5] { "link", "a_i", "d_i", "��", "Joint range" };//�ؽڷ�Χ
                
                var DHPara = script_tm.RobotDHParameter;
                int count = DHPara.Count;//�б���
                List<string[]> tableData = new List<string[]>();
                for(int i=0; i<count; i++)
                {
                    //ÿһ���б�Ԫ�ذ��������ݣ�������ʽ��
                    string[] newData = new string[6] {
                        DHPara[i].link,
                        DHPara[i].alpha_i.ToString(),
                        DHPara[i].d.ToString(),
                        DHPara[i].theta.ToString(),
                        DHPara[i].jointRange.MinValue.ToString(),
                        DHPara[i].jointRange.MaxValue.ToString()
                    };
                    //�����б�
                    tableData.Add(newData);
                }
                RobotDHParameterTable(tableItem, tableData);


                EditorGUILayout.EndHorizontal();
            }
               
        }


        /// <summary>
        /// һ��ģ�����ݻ��ƣ�һ���������ƶ�Ӧһ����������
        /// </summary>
        /// <param name="itemName">��������</param>
        /// <param name="itemContenr">��������</param>
        void ModelDataItem(string itemName, string[] itemContenr)
        {
            GUIStyle dataStyle = skin_modelLibrary.customStyles[4];
            float nameWidth = (ScreenWidth - splitterPos - splitterWidth) / 5;//�������Ƴ���
            EditorGUILayout.BeginHorizontal("Box");
            //��������
            EditorGUILayout.LabelField(itemName, dataStyle, GUILayout.Width(nameWidth), GUILayout.Height(25));
            EditorGUILayout.LabelField("", GUILayout.Width(4), GUILayout.Height(25));//���
            //��������
            EditorGUILayout.BeginVertical();
            for (int i = 0; i < itemContenr.Length; i++)
            {
                EditorGUILayout.LabelField(itemContenr[i], GUILayout.Height(25));
            }
            EditorGUILayout.EndVertical();


            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// һ��ģ�����ݻ��ƣ�һ���������ƿɰ�����������ƣ�һ�������ƶ�Ӧһ����������
        /// </summary>
        /// <param name="itemName">��������</param>
        /// <param name="secondName">������</param>
        /// <param name="itemContenr">��������</param>
        void ModelDataItem(string itemName, string[] secondName, string[] itemContenr)
        {
            GUIStyle dataStyle = skin_modelLibrary.customStyles[4];
            GUIStyle dataStyle2 = skin_modelLibrary.customStyles[5];
            float nameWidth = (ScreenWidth - splitterPos - splitterWidth) / 5;//�������Ƴ���
            float secondNameWidth = (ScreenWidth - splitterPos - splitterWidth)*2 / 15;//�ڶ��������Ƴ���
            EditorGUILayout.BeginHorizontal("Box");
            //��������
            EditorGUILayout.LabelField(itemName, dataStyle, GUILayout.Width(nameWidth), GUILayout.Height(25));
            EditorGUILayout.LabelField("", GUILayout.Width(4), GUILayout.Height(25));//���
            //��������
            EditorGUILayout.BeginVertical();
            for (int i = 0; i < itemContenr.Length; i++)
            {
                EditorGUILayout.BeginHorizontal("Box");
                EditorGUILayout.LabelField(secondName[i], dataStyle2, GUILayout.Width(secondNameWidth), GUILayout.Height(20));
                EditorGUILayout.LabelField("", GUILayout.Width(4), GUILayout.Height(20));//���
                EditorGUILayout.LabelField(itemContenr[i], GUILayout.Height(20));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();


            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// ��ɫ�ı�ǩ�ؼ�
        /// </summary>
        /// <param name="content"></param>
        void RedLabel(string content)
        {
            GUIStyle redStyle = skin_modelLibrary.customStyles[3];
            EditorGUILayout.LabelField(content, redStyle);
        }

        /// <summary>
        /// ������DH���������
        /// </summary>
        /// <param name="tableItem">��Ĳ˵�������</param>
        /// <param name="tableData">��ÿһ�У�������</param>
        void RobotDHParameterTable(string[] tableItem, List<string[]> tableData = null)
        {
            GUIStyle dataStyle3 = skin_modelLibrary.customStyles[5];
            GUIStyle dataStyle4 = skin_modelLibrary.customStyles[6];
            float itemWidth = (ScreenWidth - splitterPos - splitterWidth)*4 / 28;//���Ƴ���
            EditorGUILayout.BeginVertical();
            //�������ƻ���
            EditorGUILayout.BeginHorizontal("Box");
            for (int i = 0; i < tableItem.Length; i++)
            {
                EditorGUILayout.LabelField(tableItem[i], dataStyle3, GUILayout.Width(itemWidth), GUILayout.Height(20));
            }
            EditorGUILayout.EndHorizontal();

            //�����ݻ���
            if (tableData == null)
                return;

            int count = tableData.Count;//�б��ȣ���������ƶ���������
            
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


        #region ��ȡ����Դ����
        string WriteDeviceJsonFile = "Assets/RDTS/Scripts/Window/Editor/DeviceWindow/jsonInfo/";//�ļ�д��·��
        string dataPath = "Assets/RDTS/Data/";
        string iconPath = "Assets/RDTS/Private/Resources/Icons/WindowIcon/";
        string skinPath = "Assets/RDTS/Scripts/Window/";
        DeviceLibraryData devicelibraryData;//ģ�Ϳ������
        MaterialLibraryData materialLibraryData;//���ʿ������
        static GUIContent thisTitleContent;//��������
        void OnEnable()
        {
            //������Դ��ȡ
            devicelibraryData =  AssetDatabase.LoadAssetAtPath<DeviceLibraryData>(dataPath + "DeviceLibraryData.asset");
            materialLibraryData =  AssetDatabase.LoadAssetAtPath<MaterialLibraryData>(dataPath + "MaterialLibraryData.asset");
            Debug.Log(devicelibraryData);
            modulelibraries = (libraryMode == LibraryMode.ModelLibrary)? ReadLibraryData(devicelibraryData): ReadLibraryData(materialLibraryData);
            //���ô��ڱ��⼰ͼ��
            Texture titleIcon = AssetDatabase.LoadAssetAtPath<Texture>(iconPath+ "Parallel.png");
            thisTitleContent = new GUIContent("Parallel�豸ģ�Ϳ�", titleIcon);
            //����unity�Դ�ͼ��
            LoadTexture();
           
        }

        private GUISkin skin_modelLibrary;

        private Texture2D image_Refresh;
        private Texture2D image_Help;
        private Texture2D image_close;
        private Texture2D image_modellibrary;
        private Texture2D image_robot;
        private Texture2D image_mouseleft;
        void LoadTexture()//�����ز�
        {
            skin_modelLibrary = AssetDatabase.LoadAssetAtPath<GUISkin>("Assets/RDTS/Scripts/Window/Editor/DeviceWindow/WindowModelLibrary.guiskin");

            image_Refresh = EditorGUIUtility.FindTexture("Refresh");
            image_Help = EditorGUIUtility.FindTexture("d__Help@2x");
            image_close = EditorGUIUtility.FindTexture("d_winbtn_win_close");

            image_modellibrary = AssetDatabase.LoadAssetAtPath<Texture>(iconPath + "WindowModelIcon.png") as Texture2D;
            image_robot = AssetDatabase.LoadAssetAtPath<Texture>(iconPath + "Devicerobot.png") as Texture2D;
            image_mouseleft = AssetDatabase.LoadAssetAtPath<Texture>(iconPath + "mouseleft.png") as Texture2D;
        }


        int number_ItemType = 0;//����
        int number_Item = 0;//����
        List<ModuleLibrary> modulelibraries = new List<ModuleLibrary>();//��������
        List<Module> modules;

        /// <summary>
        /// ��ȡ�豸ģ�Ϳ���Դ����
        /// </summary>
        /// <param name="data">��ӦҪ��ȡ�Ŀ������</param>
        List<ModuleLibrary> ReadLibraryData (DeviceLibraryData data)
        {
            menuItem.Clear();//����գ���Ȼ��ˢ�¡�ʱ���ܻὫ ��ɾ����Ԫ�� �������������³���
            List<ModuleLibrary> modulelibraries = new List<ModuleLibrary>();

            if (data is null)
            {
                modulelibraries = ReadJsonData(WriteDeviceJsonFile);
                return modulelibraries;
            }
            number_ItemType = data.DeviceLibraryDataList.Count;//��ȡһ���ж������͵��豸
            var number_ItemId = 0;
            foreach (var deviceLibrary in data.DeviceLibraryDataList)//����ÿһ������
            {

                ModuleLibrary thisModulelibrary = new ModuleLibrary();
                Json_ModuleLibrary Json_thisModulelibrary = new Json_ModuleLibrary();
                thisModulelibrary.name = deviceLibrary.name;
                Json_thisModulelibrary.type = deviceLibrary.name;
                Json_thisModulelibrary.id = number_ItemId.ToString();
                number_ItemId += 1;
                if (!menuItem.Contains(deviceLibrary.name))//�����豸�������Ƶ��б�
                {
                    menuItem.Add(deviceLibrary.name);
                }

                foreach (var device in deviceLibrary.devices)//����ÿһ���豸
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
                //���豸����Ϣ�洢��WriteJsonFile�������ļ�����
                string jsondata = JsonUtility.ToJson(Json_thisModulelibrary);
                WriteDataToJson(WriteDeviceJsonFile + thisModulelibrary.name + ".json", jsondata);

                modulelibraries.Add(thisModulelibrary);

                number_Item += deviceLibrary.devices.Count;//���һ���ж��ٸ��豸
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
                //��������
                sw.Write(jsondata);
                //�ر��ĵ�
                sw.Close();
                sw.Dispose();
            }
        }
        public List<ModuleLibrary> ReadJsonData(string fileUrl)
        {
            List<ModuleLibrary> modulelibraries = new List<ModuleLibrary>();
            //string���͵����ݳ���
            string readData;

            if (Directory.Exists(fileUrl))
            {
                DirectoryInfo direction = new DirectoryInfo(fileUrl);
                FileInfo[] files = direction.GetFiles("*");
                number_ItemType = direction.GetFiles("*.json").Length;
                Debug.Log("number_ItemType: "+ number_ItemType);

                //�����յ��б�Ϊ���������׼����ʵ��˳����
                for (int i = 0; i < number_ItemType; i++)
                {
                    menuItem.Add("");
                    modulelibraries.Add(new ModuleLibrary());
                }

                for (int i = 0; i < files.Length; i++)
                {
                    //���Թ����ļ�
                    if (files[i].Name.EndsWith(".json"))
                    {

                        using (StreamReader sr = File.OpenText(files[i].FullName))
                        {
                            //���ݱ���
                            readData = sr.ReadToEnd();
                            sr.Close();
                        }
                        Json_ModuleLibrary m_PersonData = JsonUtility.FromJson<Json_ModuleLibrary>(readData);

                        ModuleLibrary thisModulelibrary = new ModuleLibrary();
                        thisModulelibrary.name = m_PersonData.type;
                        var ModulelivraryId = Convert.ToInt32(m_PersonData.id);

                        if (!menuItem.Contains(thisModulelibrary.name))//�����豸�������Ƶ��б�
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
            //��������
            return modulelibraries;
        }


        /// <summary>
        /// ��ȡ���ʿ���Դ����
        /// </summary>
        /// <param name="data">��ӦҪ��ȡ�Ŀ������</param>
        List<ModuleLibrary> ReadLibraryData(MaterialLibraryData data)   
        {
            menuItem.Clear();//����գ���Ȼ��ˢ�¡�ʱ���ܻὫ ��ɾ����Ԫ�� �������������³���
            List<ModuleLibrary> modulelibraries = new List<ModuleLibrary>();
            number_ItemType = data.MaterialLibraryDataList.Count;//��ȡһ���ж������͵Ĳ���

            foreach (var materialLibrary in data.MaterialLibraryDataList)//����ÿһ������
            {

                ModuleLibrary thisModulelibrary = new ModuleLibrary();
                thisModulelibrary.name = materialLibrary.name;

                if (!menuItem.Contains(materialLibrary.name))//�����豸�������Ƶ��б�
                {
                    menuItem.Add(materialLibrary.name);
                }

                foreach (var device in materialLibrary.materials)//����ÿһ���豸
                {
                    Module thisModule = new Module();
                    thisModule.name = device.name;
                    thisModule.description = device.description;
                    thisModule.textrue = device.textrue;
                    thisModule.material = device.material;

                    thisModulelibrary.modules.Add(thisModule);
                }

                modulelibraries.Add(thisModulelibrary);

                number_Item += materialLibrary.materials.Count;//���һ���ж��ٸ�����
            }

            return modulelibraries;
        }



        #endregion

    }




    /// <summary>
    ///һ���豸ģ���ࣺ����һ���豸������ز���
    ///[��Ӧһ��Device���MaterialSingle��]
    /// </summary>
    public class Module
    {
        public string name;//����
        public string description;//����
        public Texture textrue;//ͼ�� => [��������ԴԤ����ʽ������Ҫ���textrue]

        ///ģ��
        public GameObject model;//ģ���ļ���һ��ΪԤ���壩
        public Vector3 modelPos;//ģ�ͱ���ӵ�λ��

        ///����
        public Material material;//����
    }

    /// <summary>
    /// һ���豸ģ���ࣺ����һ�����͵��豸
    /// [��Ӧһ��DeviceLibrary��]
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
        public string name;//����
        public string description;//����
        public string textrue;//ͼ�� => [��������ԴԤ����ʽ������Ҫ���textrue]

        ///ģ��
        public string model;//ģ���ļ���һ��ΪԤ���壩
        public string modelPos;//ģ�ͱ���ӵ�λ��

        ///����
        public string material;//����
    }


#endif

}
