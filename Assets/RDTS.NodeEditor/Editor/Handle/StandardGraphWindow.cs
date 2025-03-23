using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RDTS.NodeEditor
{
    public class StandardGraphWindow : BaseGraphWindow
    {
        BaseGraph tmpGraph;
        StanardToolbarView toolbarView;


        [MenuItem("Parallel-RDTS/Window/StandardGraphWindow")]
        public static BaseGraphWindow OpenWithTmpGraph()
        {
            var graphWindow = CreateWindow<StandardGraphWindow>();

            // When the graph is opened from the window, we don't save the graph to disk
            graphWindow.tmpGraph = ScriptableObject.CreateInstance<BaseGraph>();
            graphWindow.tmpGraph.hideFlags = HideFlags.HideAndDontSave;
            graphWindow.InitializeGraph(graphWindow.tmpGraph);

            graphWindow.Show();

            return graphWindow;
        }

        protected override void OnDestroy()
        {
            graphView?.Dispose();
            DestroyImmediate(tmpGraph);
        }


        protected override void InitializeWindow(BaseGraph graph)
        {
            titleContent = new GUIContent("Graph Window");

            if (graphView == null)
            {
                graphView = new StandardGraphView(this);
                //工具栏
                toolbarView = new StanardToolbarView(graphView);
                graphView.Add(toolbarView);
                //小地图
                graphView.Add(new MiniMapView(graphView));
            }

            rootView.Add(graphView);
            Debug.Log("window 初始化");
        }

        protected override void InitializeGraphView(BaseGraphView view)
        {
            // graphView.OpenPinned< ExposedParameterView >();
            // toolbarView.UpdateButtonStatus();
        }

    }
}
