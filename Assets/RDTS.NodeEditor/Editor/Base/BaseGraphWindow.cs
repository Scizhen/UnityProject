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
    /// GraphWindow的基类，可以继承此类来实现不同图表形式的窗口
    /// </summary>
	[System.Serializable]
    public abstract class BaseGraphWindow : EditorWindow
    {
        protected VisualElement rootView;//窗口层级视图的根视觉元素
        protected BaseGraphView graphView;

        [SerializeField]
        protected BaseGraph graph;

        readonly string graphWindowStyle = "GraphStyles/BaseGraphView";//uss样式表

        public bool isGraphLoaded
        {
            get { return graphView != null && graphView.graph != null; }
        }

        bool reloadWorkaround = false;//是否重新加载

        public event Action<BaseGraph> graphLoaded;
        public event Action<BaseGraph> graphUnloaded;

        /// <summary>
        /// Called by Unity when the window is enabled / opened
        /// 当窗口启用或打开时被unity调用
        /// </summary>
        protected virtual void OnEnable()
        {
            InitializeRootView();//初始化窗口层级视图的根视觉元素

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
            // 编辑器窗口的刷新选项的解决方法：点击Refresh时，在对编辑器窗口中的序列化数据进行反序列化之前调用了OnEnable，导致无法加载graph view
            if (reloadWorkaround && graph != null)
            {
                LoadGraph();
                reloadWorkaround = false;
            }
        }

        void LoadGraph()
        {
            // We wait for the graph to be initialized  等待图表初始化完成
            if (graph.isEnabled)
                InitializeGraph(graph);
            else
                graph.onEnabled += () => InitializeGraph(graph);
        }

        /// <summary>
        /// Called by Unity when the window is disabled (happens on domain reload)
        /// 当窗口被禁用时由 Unity 调用（在域重新加载时发生）
        /// </summary>
        protected virtual void OnDisable()
        {
            if (graph != null && graphView != null)
                graphView.SaveGraphToDisk();//存入磁盘
        }

        /// <summary>
        /// Called by Unity when the window is closed
        /// </summary>
        protected virtual void OnDestroy() { }

        /// <summary>
        /// 初始化窗口层级视图的根视觉元素
        /// </summary>
		void InitializeRootView()
        {
            rootView = base.rootVisualElement;//获取窗口层级视图的根视觉元素

            rootView.name = "graphRootView";

            rootView.styleSheets.Add(Resources.Load<StyleSheet>(graphWindowStyle));
        }

        /// <summary>
        /// 对图表的初始化
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="bindObjPerGuid"></param>
        public void InitializeGraph(BaseGraph graph, Dictionary<string, RDTSBehavior> bindObjPerGuid = null)
        {
            //将上一个graph保存
            if (this.graph != null && graph != this.graph)//此graph不为空 且 参数graph不为此graph
            {
                // Save the graph to the disk 将图表保存到磁盘
                EditorUtility.SetDirty(this.graph);
                AssetDatabase.SaveAssets();
                // Unload the graph 卸载此图表
                graphUnloaded?.Invoke(this.graph);
            }

            graphLoaded?.Invoke(graph); //加载参数graph
            this.graph = graph;//指定此graph为参数graph

            if (graphView != null)//需清除一下之前的graphView
                rootView.Remove(graphView);

            ///Initialize will provide the BaseGraphView 初始化将提供BaseGraphView
            InitializeWindow(graph);//需重载，以实现窗口的相关内容和功能

            graphView = rootView.Children().FirstOrDefault(e => e is BaseGraphView) as BaseGraphView;//获取第一个BaseGraphView

            if (graphView == null)//若获取的BaseGraphView为空，直接返回
            {
                Debug.LogError("GraphView has not been added to the BaseGraph root view !");
                return;
            }

            graphView.Initialize(graph, this, bindObjPerGuid);//初始化的同时绑定对象

            InitializeGraphView(graphView);//需重载

            // TOOD: onSceneLinked...

            if (graph.IsLinkedToScene())//当图表graph链接到有效场景时
                LinkGraphWindowToScene(graph.GetLinkedScene());
            else
                graph.onSceneLinked += LinkGraphWindowToScene;
        }

        void LinkGraphWindowToScene(Scene scene)
        {
            EditorSceneManager.sceneClosed += CloseWindowWhenSceneIsClosed;//在编辑器中关闭场景后，调用此事件（关闭此窗口）

            void CloseWindowWhenSceneIsClosed(Scene closedScene)
            {
                if (scene == closedScene)
                {
                    Close();//关闭自定义窗口
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


        //子类继承后重载，可实现不同图表功能
        protected abstract void InitializeWindow(BaseGraph graph);
        protected virtual void InitializeGraphView(BaseGraphView view) { }
    }
}