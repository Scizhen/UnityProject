using System.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using RDTS;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// GraphWindow�Ļ��࣬���Լ̳д�����ʵ�ֲ�ͬͼ����ʽ�Ĵ���
    /// </summary>
	[System.Serializable]
    public abstract class BaseGraphWindow : EditorWindow
    {
        protected VisualElement rootView;//���ڲ㼶��ͼ�ĸ��Ӿ�Ԫ��
        protected BaseGraphView graphView;

        [SerializeField]
        protected BaseGraph graph;

        readonly string graphWindowStyle = "GraphStyles/BaseGraphView";//uss��ʽ��

        public bool isGraphLoaded
        {
            get { return graphView != null && graphView.graph != null; }
        }

        bool reloadWorkaround = false;//�Ƿ����¼���

        public event Action<BaseGraph> graphLoaded;
        public event Action<BaseGraph> graphUnloaded;

        /// <summary>
        /// Called by Unity when the window is enabled / opened
        /// ���������û��ʱ��unity����
        /// </summary>
        protected virtual void OnEnable()
        {
            InitializeRootView();//��ʼ�����ڲ㼶��ͼ�ĸ��Ӿ�Ԫ��

            if (graph != null)
                LoadGraph();
            else
                reloadWorkaround = true;
        }

        protected virtual void Update()
        {
            // Workaround for the Refresh option of the editor window: 
            // When Refresh is clicked, OnEnable is called before the serialized data in the
            // editor window is deserialized, causing the graph view to not be loaded
            // �༭�����ڵ�ˢ��ѡ��Ľ�����������Refreshʱ���ڶԱ༭�������е����л����ݽ��з����л�֮ǰ������OnEnable�������޷�����graph view
            if (reloadWorkaround && graph != null)
            {
                LoadGraph();
                reloadWorkaround = false;
            }
        }

        void LoadGraph()
        {
            // We wait for the graph to be initialized  �ȴ�ͼ���ʼ�����
            if (graph.isEnabled)
                InitializeGraph(graph);
            else
                graph.onEnabled += () => InitializeGraph(graph);
        }

        /// <summary>
        /// Called by Unity when the window is disabled (happens on domain reload)
        /// �����ڱ�����ʱ�� Unity ���ã��������¼���ʱ������
        /// </summary>
        protected virtual void OnDisable()
        {
            if (graph != null && graphView != null)
                graphView.SaveGraphToDisk();//�������
        }

        /// <summary>
        /// Called by Unity when the window is closed
        /// </summary>
        protected virtual void OnDestroy() { }

        /// <summary>
        /// ��ʼ�����ڲ㼶��ͼ�ĸ��Ӿ�Ԫ��
        /// </summary>
		void InitializeRootView()
        {
            rootView = base.rootVisualElement;//��ȡ���ڲ㼶��ͼ�ĸ��Ӿ�Ԫ��

            rootView.name = "graphRootView";

            rootView.styleSheets.Add(Resources.Load<StyleSheet>(graphWindowStyle));
        }

        /// <summary>
        /// ��ͼ��ĳ�ʼ��
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="bindObjPerGuid"></param>
        public void InitializeGraph(BaseGraph graph, Dictionary<string, RDTSBehavior> bindObjPerGuid = null)
        {
            //����һ��graph����
            if (this.graph != null && graph != this.graph)//��graph��Ϊ�� �� ����graph��Ϊ��graph
            {
                // Save the graph to the disk ��ͼ���浽����
                EditorUtility.SetDirty(this.graph);
                AssetDatabase.SaveAssets();
                // Unload the graph ж�ش�ͼ��
                graphUnloaded?.Invoke(this.graph);
            }

            graphLoaded?.Invoke(graph); //���ز���graph
            this.graph = graph;//ָ����graphΪ����graph

            if (graphView != null)//�����һ��֮ǰ��graphView
                rootView.Remove(graphView);

            ///Initialize will provide the BaseGraphView ��ʼ�����ṩBaseGraphView
            InitializeWindow(graph);//�����أ���ʵ�ִ��ڵ�������ݺ͹���

            graphView = rootView.Children().FirstOrDefault(e => e is BaseGraphView) as BaseGraphView;//��ȡ��һ��BaseGraphView

            if (graphView == null)//����ȡ��BaseGraphViewΪ�գ�ֱ�ӷ���
            {
                Debug.LogError("GraphView has not been added to the BaseGraph root view !");
                return;
            }

            graphView.Initialize(graph, this, bindObjPerGuid);//��ʼ����ͬʱ�󶨶���

            InitializeGraphView(graphView);//������

            // TOOD: onSceneLinked...

            if (graph.IsLinkedToScene())//��ͼ��graph���ӵ���Ч����ʱ
                LinkGraphWindowToScene(graph.GetLinkedScene());
            else
                graph.onSceneLinked += LinkGraphWindowToScene;
        }

        void LinkGraphWindowToScene(Scene scene)
        {
            EditorSceneManager.sceneClosed += CloseWindowWhenSceneIsClosed;//�ڱ༭���йرճ����󣬵��ô��¼����رմ˴��ڣ�

            void CloseWindowWhenSceneIsClosed(Scene closedScene)
            {
                if (scene == closedScene)
                {
                    Close();//�ر��Զ��崰��
                    EditorSceneManager.sceneClosed -= CloseWindowWhenSceneIsClosed;
                }
            }
        }

        public virtual void OnGraphDeleted()
        {
            if (graph != null && graphView != null)
                rootView.Remove(graphView);

            graphView = null;
        }


        //����̳к����أ���ʵ�ֲ�ͬͼ����
        protected abstract void InitializeWindow(BaseGraph graph);
        protected virtual void InitializeGraphView(BaseGraphView view) { }
    }
}