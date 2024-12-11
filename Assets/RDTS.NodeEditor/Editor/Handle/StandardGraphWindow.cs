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
                //������
                toolbarView = new StanardToolbarView(graphView);
                graphView.Add(toolbarView);
                //С��ͼ
                graphView.Add(new MiniMapView(graphView));
            }

            rootView.Add(graphView);
            Debug.Log("window ��ʼ��");
        }

        protected override void InitializeGraphView(BaseGraphView view)
        {
            // graphView.OpenPinned< ExposedParameterView >();
            // toolbarView.UpdateButtonStatus();
        }

    }
}
