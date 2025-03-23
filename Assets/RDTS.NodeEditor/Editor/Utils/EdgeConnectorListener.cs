using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// Base class to write your own edge handling connection system edge��������ϵͳ�Ļ���
    ///    ���ߵ��հ׿ռ�λ��ʱ�������������ڣ�
    ///    ���ߵ��µĶ˿�ʱ����Ҫ���ӵ��µĶ˿���Ͽ�����û�����ӵ���Ӧ�Ķ˿�Ҳ�Ͽ�������ԭ�˿��򲻱�
    /// </summary>
    public class BaseEdgeConnectorListener : IEdgeConnectorListener//�� EdgeConnector �ٿ���ʹ�������ʵ�ʵı�Ե���������������˽ӿڣ��û����Բ�ͬ�ķ�ʽ���Ǻʹ�����Ե
    {
        public readonly BaseGraphView graphView;

        //edge��Ӧport
        Dictionary<Edge, PortView> edgeInputPorts = new Dictionary<Edge, PortView>();
        Dictionary<Edge, PortView> edgeOutputPorts = new Dictionary<Edge, PortView>();

        static CreateNodeMenuWindow edgeNodeCreateMenuWindow;

        public BaseEdgeConnectorListener(BaseGraphView graphView)
        {
            this.graphView = graphView;
        }

        /// <summary>
        /// �ڿհ׿ռ���ñ�Եʱ����,������������
        /// </summary>
        /// <param name="edge">���ڷ��õ�edge</param>
        /// <param name="position">�ڿհ׿ռ��з��ñ�Ե��λ��</param>
        public virtual void OnDropOutsidePort(Edge edge, Vector2 position)
        {
            this.graphView.RegisterCompleteObjectUndo("Disconnect edge");

            //If the edge was already existing, remove it ���edge�Ѵ��ڣ����Ƴ�
            if (!edge.isGhostEdge)//�˱�Ե�Ƿ�Ϊ��Ӱ��Ե��������Եʱ�Զ��뷽ʽ�����ڸ����˿ڵı�Ե��
                graphView.Disconnect(edge as EdgeView);

            // when on of the port is null, then the edge was created and dropped outside of a port
            if (edge.input == null || edge.output == null)
                ShowNodeCreationMenuFromEdge(edge as EdgeView, position);
        }

        /// <summary>
        /// �ڶ˿ڷ����µı�Եʱ���ã���Ҫ���ӵ��µĶ˿���Ͽ��ɵ������µģ���û�����ӵ���Ӧ�Ķ˿�Ҳ�Ͽ�������ԭ�˿��򲻱�
        /// </summary>
        /// <param name="graphView">���� GraphView</param>
        /// <param name="edge">���ڴ�����edge</param>
        public virtual void OnDrop(GraphView graphView, Edge edge)
        {
            var edgeView = edge as EdgeView;
            bool wasOnTheSamePort = false;

            if (edgeView?.input == null || edgeView?.output == null)
                return;

            //If the edge was moved to another port  �Ƿ��߱����ӵ���һ���˿�
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
                if (!this.graphView.Connect(edge as EdgeView, autoDisconnectInputs: !wasOnTheSamePort))//ָ������autoDisconnectInputsΪ!wasOnTheSamePort
                    this.graphView.Disconnect(edge as EdgeView);
                // Debug.Log("�˿ڷ����µı�Ե");
            }
            catch (System.Exception)
            {
                this.graphView.Disconnect(edge as EdgeView);
            }
        }

        /// <summary>
        /// �����ߵ���һ���ڿհ�����ʱ��������������
        /// </summary>
        /// <param name="edgeView"></param>
        /// <param name="position"></param>
        void ShowNodeCreationMenuFromEdge(EdgeView edgeView, Vector2 position)
        {
            if (edgeNodeCreateMenuWindow == null)//û�����������ھʹ���
                edgeNodeCreateMenuWindow = ScriptableObject.CreateInstance<CreateNodeMenuWindow>();

            edgeNodeCreateMenuWindow.Initialize(graphView, EditorWindow.focusedWindow, edgeView);
            SearchWindow.Open(new SearchWindowContext(position + EditorWindow.focusedWindow.position.position), edgeNodeCreateMenuWindow);
            Debug.Log("��������");
            //SearchWindow��������ʾ�˿���ͼ�νڵ�Ŀ������˵�
        }
    }
}