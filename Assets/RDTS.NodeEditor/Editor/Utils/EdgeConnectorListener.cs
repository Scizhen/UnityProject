using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// Base class to write your own edge handling connection system edge处理连接系统的基类
    ///    连线到空白空间位置时，创建搜索窗口；
    ///    连线到新的端口时，若要连接到新的端口则断开，若没有连接到对应的端口也断开，若是原端口则不变
    /// </summary>
    public class BaseEdgeConnectorListener : IEdgeConnectorListener//供 EdgeConnector 操控器使用以完成实际的边缘创建工作。借助此接口，用户能以不同的方式覆盖和创建边缘
    {
        public readonly BaseGraphView graphView;

        //edge对应port
        Dictionary<Edge, PortView> edgeInputPorts = new Dictionary<Edge, PortView>();
        Dictionary<Edge, PortView> edgeOutputPorts = new Dictionary<Edge, PortView>();

        static CreateNodeMenuWindow edgeNodeCreateMenuWindow;

        public BaseEdgeConnectorListener(BaseGraphView graphView)
        {
            this.graphView = graphView;
        }

        /// <summary>
        /// 在空白空间放置边缘时调用,创建搜索窗口
        /// </summary>
        /// <param name="edge">正在放置的edge</param>
        /// <param name="position">在空白空间中放置边缘的位置</param>
        public virtual void OnDropOutsidePort(Edge edge, Vector2 position)
        {
            this.graphView.RegisterCompleteObjectUndo("Disconnect edge");

            //If the edge was already existing, remove it 如果edge已存在，则移除
            if (!edge.isGhostEdge)//此边缘是否为重影边缘（创建边缘时以对齐方式出现在附近端口的边缘）
                graphView.Disconnect(edge as EdgeView);

            // when on of the port is null, then the edge was created and dropped outside of a port
            if (edge.input == null || edge.output == null)
                ShowNodeCreationMenuFromEdge(edge as EdgeView, position);
        }

        /// <summary>
        /// 在端口放置新的边缘时调用，若要连接到新的端口则断开旧的连接新的，若没有连接到对应的端口也断开，若是原端口则不变
        /// </summary>
        /// <param name="graphView">引用 GraphView</param>
        /// <param name="edge">正在创建的edge</param>
        public virtual void OnDrop(GraphView graphView, Edge edge)
        {
            var edgeView = edge as EdgeView;
            bool wasOnTheSamePort = false;

            if (edgeView?.input == null || edgeView?.output == null)
                return;

            //If the edge was moved to another port  是否线被连接到另一个端口
            if (edgeView.isConnected)
            {
                if (edgeInputPorts.ContainsKey(edge) && edgeOutputPorts.ContainsKey(edge))
                    if (edgeInputPorts[edge] == edge.input && edgeOutputPorts[edge] == edge.output)
                        wasOnTheSamePort = true;

                if (!wasOnTheSamePort)
                    this.graphView.Disconnect(edgeView);
            }

            if (edgeView.input.node == null || edgeView.output.node == null)
                return;

            edgeInputPorts[edge] = edge.input as PortView;
            edgeOutputPorts[edge] = edge.output as PortView;
            try
            {
                this.graphView.RegisterCompleteObjectUndo("Connected " + edgeView.input.node.name + " and " + edgeView.output.node.name);
                if (!this.graphView.Connect(edge as EdgeView, autoDisconnectInputs: !wasOnTheSamePort))//指定参数autoDisconnectInputs为!wasOnTheSamePort
                    this.graphView.Disconnect(edge as EdgeView);
                // Debug.Log("端口放置新的边缘");
            }
            catch (System.Exception)
            {
                this.graphView.Disconnect(edge as EdgeView);
            }
        }

        /// <summary>
        /// 在连线的另一端在空白区域时，创建搜索窗口
        /// </summary>
        /// <param name="edgeView"></param>
        /// <param name="position"></param>
        void ShowNodeCreationMenuFromEdge(EdgeView edgeView, Vector2 position)
        {
            if (edgeNodeCreateMenuWindow == null)//没有搜索树窗口就创建
                edgeNodeCreateMenuWindow = ScriptableObject.CreateInstance<CreateNodeMenuWindow>();

            edgeNodeCreateMenuWindow.Initialize(graphView, EditorWindow.focusedWindow, edgeView);
            SearchWindow.Open(new SearchWindowContext(position + EditorWindow.focusedWindow.position.position), edgeNodeCreateMenuWindow);
            Debug.Log("搜索窗口");
            //SearchWindow：此类显示了可用图形节点的可搜索菜单
        }
    }
}