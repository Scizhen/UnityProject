using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// ��Ҫʵ�ֶ�edge���϶������Ӳ�����
    ///     ������갴���¼���
    ///     ��������ƶ��¼���
    ///     �������̧���¼���
    ///     ��ͼ��ƽ�ƣ�
    ///     �˿ڵĸ����͹�Ӱ
    /// </summary>
    public class BaseEdgeDragHelper : EdgeDragHelper
    {
        ///Unity�ٷ�ʵ��
        // https://github.com/Unity-Technologies/UnityCsReference/blob/master/Modules/GraphViewEditor/Manipulators/EdgeDragHelper.cs#L21
        internal const int k_PanAreaWidth = 30;//ƽ��������
        internal const int k_PanSpeed = 4;//ƽ���ٶ�
        internal const int k_PanInterval = 10;//ƽ�Ƶļ��ʱ��
        internal const float k_MinSpeedFactor = 0.5f;
        internal const float k_MaxSpeedFactor = 7f;
        internal const float k_MaxPanSpeed = k_MaxSpeedFactor * k_PanSpeed;//���ƽ���ٶ�
        internal const float kPortDetectionWidth = 30;//�˿ڼ����

        protected Dictionary<BaseNodeView, List<PortView>> compatiblePorts = new Dictionary<BaseNodeView, List<PortView>>();
        private Edge ghostEdge;
        protected GraphView graphView;
        protected static NodeAdapter nodeAdapter = new NodeAdapter();

        /// <summary>�� EdgeConnector �ٿ���ʹ�������ʵ�ʵ�edge���������������˽ӿڣ��û����Բ�ͬ�ķ�ʽ���Ǻʹ���edge</summary>
        protected readonly IEdgeConnectorListener listener;

        /// <summary>��ʾʹ�� VisualElement �ļƻ��ӿڴ����ļƻ����� </summary>
        private IVisualElementScheduledItem panSchedule;
        private Vector3 panDiff = Vector3.zero;
        private bool wasPanned;//��ͼ(ͼ���Ƿ�ƽ��)

        public bool resetPositionOnPan { get; set; }

        public BaseEdgeDragHelper(IEdgeConnectorListener listener)
        {
            this.listener = listener;//ָ��edge���������Ľӿ�
            resetPositionOnPan = true;
            Reset();
        }

        /// <summary>[���Ǹ���]�����϶���edge</summary>
        public override Edge edgeCandidate { get; set; }
        /// <summary>[���Ǹ���]�����϶���Ե�Ķ˿�</summary>
        public override Port draggedPort { get; set; }

        /// <summary>
        /// ���ظ��ࣺ�����϶�helper��״̬
        /// </summary>
        /// <param name="didConnect"></param>
        public override void Reset(bool didConnect = false)
        {
            if (compatiblePorts != null && graphView != null)
            {
                // Reset the highlights.
                graphView.ports.ForEach((p) => {
                    p.OnStopEdgeDragging();//��edge�϶�����ʱ����
                });
                compatiblePorts.Clear();
            }

            // Clean up ghost edge. �����Ӱedge
            if ((ghostEdge != null) && (graphView != null))
            {
                var pv = ghostEdge.input as PortView;
                graphView.schedule.Execute(() => {
                    pv.portCapLit = false;//�˿ڸ��Ƿ����
                    // pv.UpdatePortView(pv.portData);
                }).ExecuteLater(10);//10ms��ִ��
                graphView.RemoveElement(ghostEdge);//ɾ����Ӱedge
            }

            if (wasPanned)//����ͼgraphView��ƽ����
            {
                ///Debug.Log("ƽ��");
                if (!resetPositionOnPan || didConnect)//���϶�edge����ͼ������ƽ�ƣ����������̧��ʱ�����ӵ��˿�
                {
                    Vector3 p = graphView.contentViewContainer.transform.position;
                    Vector3 s = graphView.contentViewContainer.transform.scale;
                    graphView.UpdateViewTransform(p, s);//�����ӿڱ任�� ��������λ�ã������ţ�

                }
            }

            if (panSchedule != null)
                panSchedule.Pause();

            if (ghostEdge != null)//����(����)��Ӱedge
            {
                ghostEdge.input = null;
                ghostEdge.output = null;
            }

            if (draggedPort != null && !didConnect)
            {
                draggedPort.portCapLit = false;
                draggedPort = null;
            }

            if (edgeCandidate != null)//���� �����϶���edge
            {
                edgeCandidate.SetEnabled(true);
            }

            ghostEdge = null;
            edgeCandidate = null;

            graphView = null;
        }

        /// <summary>
        /// ������갴���¼���
        ///     �������϶�edge�Ķ˿ڣ�������˶˿ںʹ˶˿ڼ��ݵĶ˿ڣ�
        ///     �����ڽڵ�Ķ˿ڽ������򣬱��ڰ�Χ�еļ��㣻
        ///     �ƻ���ͼƽ�ƵĲ���
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        public override bool HandleMouseDown(MouseDownEvent evt)
        {
            Vector2 mousePosition = evt.mousePosition;//���λ��

            if ((draggedPort == null) || (edgeCandidate == null))
            {
                return false;
            }

            graphView = draggedPort.GetFirstAncestorOfType<GraphView>();//�Ӵ�Ԫ�صĸ�����ʼ���Ϸ��ʲ㼶��ͼ�����ص�һ�� GraphView ���͵� VisualElement

            if (graphView == null)
            {
                return false;
            }

            if (edgeCandidate.parent == null)
            {
                graphView.AddElement(edgeCandidate);
            }

            bool startFromOutput = (draggedPort.direction == Direction.Output);//�˿ڷ���trueΪ�����falseΪ����

            edgeCandidate.candidatePosition = mousePosition;//������Եʱ�ı�Ե��λ�ã���Ϊ���λ��
            edgeCandidate.SetEnabled(false);// ���� VisualElement ����״̬�����õ� VisualElement �����մ�����¼�

            if (startFromOutput)//����˿�
            {
                edgeCandidate.output = draggedPort;
                edgeCandidate.input = null;
            }
            else//����˿�
            {
                edgeCandidate.output = null;
                edgeCandidate.input = draggedPort;
            }

            draggedPort.portCapLit = true;//�˿ڸǵ���

            compatiblePorts.Clear();

            foreach (PortView port in graphView.GetCompatiblePorts(draggedPort, nodeAdapter))//��ȡ������˿�draggedPort ���ݵ����ж˿�
            {
                compatiblePorts.TryGetValue(port.owner, out var portList);//���ҵ�����ֵ���������port.owner���������ֵ���ص�portList��
                if (portList == null)//Ϊ���򴴽�һ���µ��б�
                    portList = compatiblePorts[port.owner] = new List<PortView>();
                portList.Add(port);//��Ӽ��ݵĶ˿�
            }

            // Sort ports by position in the node ���ڵ��е�λ�öԶ˿ڽ�������
            foreach (var kp in compatiblePorts)
                kp.Value.Sort((e1, e2) => e1.worldBound.y.CompareTo(e2.worldBound.y));//����y���ϵ������С�������У�С���󣩣����˿ڴ�������

            // Only light compatible anchors when dragging an edge. �϶�edgeʱ��ֻ�������ݵĶ˿�
            graphView.ports.ForEach((p) => {
                p.OnStartEdgeDragging();//���϶���Եʱ����
            });

            foreach (var kp in compatiblePorts)
                foreach (var port in kp.Value)
                    port.highlight = true;//ͻ����ʾ�˿�

            edgeCandidate.UpdateEdgeControl();//����edge�� EdgeControl

            if (panSchedule == null)
            {
                //Execute:�ƻ��Ժ�ִ�д˲���  
                //Every��ָ��ʱ����ظ��˲���
                //StartingIn�����һ�ε��������ʱ
                //Pause���Ӵ���Ŀ�� VisualElement �ļƻ������н���ɾ��
                panSchedule = graphView.schedule.Execute(Pan).Every(k_PanInterval).StartingIn(k_PanInterval);
                panSchedule.Pause();
            }
            wasPanned = false;

            edgeCandidate.layer = Int32.MaxValue;//ͼ���е� GraphElement ͼ��

            return true;
        }

        /// <summary>
        /// �����Ч��ƽ���ٶ�
        /// </summary>
        /// <param name="mousePos"></param>
        /// <returns></returns>
        internal Vector2 GetEffectivePanSpeed(Vector2 mousePos)
        {
            Vector2 effectiveSpeed = Vector2.zero;

            if (mousePos.x <= k_PanAreaWidth)
                effectiveSpeed.x = -(((k_PanAreaWidth - mousePos.x) / k_PanAreaWidth) + 0.5f) * k_PanSpeed;
            else if (mousePos.x >= graphView.contentContainer.layout.width - k_PanAreaWidth)
                effectiveSpeed.x = (((mousePos.x - (graphView.contentContainer.layout.width - k_PanAreaWidth)) / k_PanAreaWidth) + 0.5f) * k_PanSpeed;

            if (mousePos.y <= k_PanAreaWidth)
                effectiveSpeed.y = -(((k_PanAreaWidth - mousePos.y) / k_PanAreaWidth) + 0.5f) * k_PanSpeed;
            else if (mousePos.y >= graphView.contentContainer.layout.height - k_PanAreaWidth)
                effectiveSpeed.y = (((mousePos.y - (graphView.contentContainer.layout.height - k_PanAreaWidth)) / k_PanAreaWidth) + 0.5f) * k_PanSpeed;

            effectiveSpeed = Vector2.ClampMagnitude(effectiveSpeed, k_MaxPanSpeed);

            return effectiveSpeed;
        }

        Vector2 lastMousePos;//��¼�ϴ����λ��
        /// <summary>
        /// ��������ƶ��¼���
        ///     ʹedge��һ�˸������λ���ƶ���
        ///     ��edge��һ�˵Ķ˿ڿ��ܴ���ʱ���ƹ�Ӱedge����ʾ��
        ///     �����ܵĶ˿ڲ����ڡ���Ӱedge����ʱ������Ӱedge
        /// </summary>
        /// <param name="evt"></param>
        public override void HandleMouseMove(MouseMoveEvent evt)
        {
            var ve = (VisualElement)evt.target;//�յ����¼���Ŀ���Ӿ�Ԫ�ء��� currentTarget ��ͬ�����¼��ش���·�����͸�����Ԫ��ʱ����Ŀ�겻����ġ�
            //contentContainer����Ԫ��ͨ����ӵ���Ԫ����
            Vector2 gvMousePos = ve.ChangeCoordinatesTo(graphView.contentContainer, evt.localMousePosition);//��[��ǰĿ������ϵ]�����λ��ת����contentContainer��
            panDiff = GetEffectivePanSpeed(gvMousePos);//�����Ч��ƽ���ٶ�

            if (panDiff != Vector3.zero)
                panSchedule.Resume();//�����δ����������� VisualElement �ļƻ�����ƻ�����Ŀ
            else
                panSchedule.Pause();//�Ӵ���Ŀ�� VisualElement �ļƻ������н���ɾ��

            Vector2 mousePosition = evt.mousePosition;//[��Ļ����ϵ]�е����λ��
            lastMousePos = evt.mousePosition;

            edgeCandidate.candidatePosition = mousePosition;//����edgeʱ��edge��λ��Ϊ����ƶ���λ��

            // Draw ghost edge if possible port exists.  ������ܵĶ˿ڴ��ڣ����ƹ�Ӱedge��������꽫edge�϶����������ӵĶ˿�ʱ������һ��Ԥ���edge��
            Port endPort = GetEndPort(mousePosition);

            ////* �Թ�Ӱedge�Ĵ��� *////

            if (endPort != null)///edgeĩ�˶˿ڴ���
            {
                if (ghostEdge == null)
                {
                    ghostEdge = CreateEdgeView();
                    ghostEdge.isGhostEdge = true;//�˱�ԵΪ��Ӱ��Ե��������Եʱ�Զ��뷽ʽ�����ڸ����˿ڵı�Ե��
                    ghostEdge.pickingMode = PickingMode.Ignore;//ȷ���� mouseEvents �� IPanel.Pick ��ѯ�ڼ��Ƿ����ѡȡ��Ԫ�ء�   Ignore������ʰȡ
                    graphView.AddElement(ghostEdge);//����ͼ����Ӵ�Ԫ��
                }

                if (edgeCandidate.output == null)//��ǰ������edge����˿�Ϊ��ʱ(����˿�һ�����ڣ����򲻻���edgeCandidate)��ȷ����Ӱedge������˿ڣ�������˿�Ϊ����endport��Ϊ������˿ڣ�������
                {
                    ghostEdge.input = edgeCandidate.input;//��ֵ����˿�
                    if (ghostEdge.output != null)
                        ghostEdge.output.portCapLit = false;
                    ghostEdge.output = endPort;
                    ghostEdge.output.portCapLit = true;
                }
                else //��ǰedge����˿ڲ�Ϊ�գ��򽫴�edge������/����˿ڸ�����Ӱedge
                {
                    if (ghostEdge.input != null)
                        ghostEdge.input.portCapLit = false;
                    ghostEdge.input = endPort;
                    ghostEdge.input.portCapLit = true;
                    ghostEdge.output = edgeCandidate.output;
                }
            }
            else if (ghostEdge != null)///��edge��ĩ�˶˿ڲ����ڣ��ҹ�Ӱedge���ڣ�����Ҫ�Ƴ���Ӱedge
            {
                if (edgeCandidate.input == null)//��ǰ������edge������˿�Ϊ�գ��������edge���ӵ�����˿ڣ�
                {
                    if (ghostEdge.input != null)//��Ӱedge����˿ڴ��ڣ��������˿�
                        ghostEdge.input.portCapLit = false;
                }
                else//��ǰ������edge������˿ڴ��ڣ��������edge���ӵ�����˿ڣ�
                {
                    if (ghostEdge.output != null)//��Ӱedge����˿ڴ��ڣ��������˿�
                        ghostEdge.output.portCapLit = false;
                }
                graphView.RemoveElement(ghostEdge);
                ghostEdge.input = null;
                ghostEdge.output = null;
                ghostEdge = null;
            }
        }

        protected virtual EdgeView CreateEdgeView()
        {
            return new EdgeView();
        }

        /// <summary>
        /// ����ͼ��ƽ�ƣ���edge���϶�������
        /// </summary>
        /// <param name="ts"></param>
        private void Pan(TimerState ts)//TimerState��:�����ƻ������¼��ļ�ʱ��Ϣ
        {
            //viewTransform��ͼ�����ͼ�任
            ///������϶�edge����ͼ��Եʱ���ƶ���ͼ
            graphView.viewTransform.position -= panDiff;//position���� VisualElement �ı任��λ��

            // Workaround to force edge to update when we pan the graph ������ƽ��ͼ��ʱǿ�Ʊ�Ե���µĽ������
            edgeCandidate.output = edgeCandidate.output;
            edgeCandidate.input = edgeCandidate.input;

            edgeCandidate.UpdateEdgeControl();
            wasPanned = true;
            ///Debug.Log("ƽ��");
        }

        /// <summary>
        /// ��������ɿ��¼���
        ///     �����Ӱ��
        ///     �����ڶ˿ھ����ӣ�
        ///     ���ڿհ�����ʹ�����������
        /// </summary>
        /// <param name="evt"></param>
        public override void HandleMouseUp(MouseUpEvent evt)
        {
            bool didConnect = false;

            Vector2 mousePosition = evt.mousePosition;

            // Reset the highlights. ���� ����
            graphView.ports.ForEach((p) => {
                p.OnStopEdgeDragging(); //�ڱ�Ե�϶�����ʱ����
            });

            // Clean up ghost edges. ����Ӱedge���ڣ��������
            if (ghostEdge != null)
            {
                if (ghostEdge.input != null)
                    ghostEdge.input.portCapLit = false;
                if (ghostEdge.output != null)
                    ghostEdge.output.portCapLit = false;

                graphView.RemoveElement(ghostEdge);
                ghostEdge.input = null;
                ghostEdge.output = null;
                ghostEdge = null;
            }

            Port endPort = GetEndPort(mousePosition);//��ȡ��괦�Ķ˿�Ϊedge��ĩ�˶˿�

            if (endPort == null && listener != null)
            {
                //�ڿհ׿ռ���ñ�Եʱ����
                //������edgeCandidate�����ڷ��õ�edge, mousePosition���ڿհ������з���edge��λ�ã�
                //��BaseEdgeConnectorListener�������˴˷��������ڴ�����������//
                listener.OnDropOutsidePort(edgeCandidate, mousePosition);
                Debug.Log("edgeĩ�˴�����������");
            }

            edgeCandidate.SetEnabled(true);

            if (edgeCandidate.input != null)
                edgeCandidate.input.portCapLit = false;

            if (edgeCandidate.output != null)
                edgeCandidate.output.portCapLit = false;

            // If it is an existing valid edge then delete and notify the model (using DeleteElements()).
            // ����������е���Ч�ߣ���ɾ����֪ͨģ�ͣ�ʹ�� DeleteElements()����[ɾ�����е�edge]
            if (edgeCandidate.input != null && edgeCandidate.output != null)
            {
                // Save the current input and output before deleting the edge as they will be reset
                Port oldInput = edgeCandidate.input;
                Port oldOutput = edgeCandidate.output;

                graphView.DeleteElements(new[] { edgeCandidate });

                // Restore the previous input and output
                edgeCandidate.input = oldInput;
                edgeCandidate.output = oldOutput;
            }
            // otherwise, if it is an temporary edge then just remove it as it is not already known my the model
            //�����������һ����ʱ��Ե����ôֻ�轫���Ƴ�����Ϊ�ҵ�ģ�ͻ���֪����
            else
            {
                graphView.RemoveElement(edgeCandidate);
            }

            //���ܵĶ˿ڴ��ڣ����õ�ǰ�϶���edgeȥ���������˿�
            if (endPort != null)
            {
                if (endPort.direction == Direction.Output)//���ܵĶ˿�Ϊ����˿�
                    edgeCandidate.output = endPort;
                else//���ܵĶ˿�Ϊ����˿�
                    edgeCandidate.input = endPort;

                //�ڶ˿ڷ����µı�Եʱ���ã���Ҫ���ӵ��µĶ˿���Ͽ��ɵ������µģ���û�����ӵ���Ӧ�Ķ˿�Ҳ�Ͽ�������ԭ�˿��򲻱�
                listener.OnDrop(graphView, edgeCandidate);
                didConnect = true;///��������
            }
            else
            {
                edgeCandidate.output = null;
                edgeCandidate.input = null;
            }

            edgeCandidate.ResetLayer();//��edge����Ϊ��ԭʼͼ��

            edgeCandidate = null;
            compatiblePorts.Clear();
            Reset(didConnect);
        }

        /// <summary>
        /// ��ȡ�˿ڵİ�Χ��
        /// </summary>
        /// <param name="nodeView"></param>
        /// <param name="index"></param>
        /// <param name="portList"></param>
        /// <returns></returns>
        Rect GetPortBounds(BaseNodeView nodeView, int index, List<PortView> portList)
        {
            var port = portList[index];//�˿��б��ж�Ӧ�����Ķ˿�
            var bounds = port.worldBound;//������任Ӧ���ھ��κ��AABB(�����İ�Χ�У���������벢����ȫ��Χ ĳ������ĺ���)

            ///�˿ڶ��� Ϊˮƽ����
            if (port.orientation == Orientation.Horizontal)
            {
                // Increase horizontal port bounds ����ˮƽ�˿ڰ�Χ��
                bounds.xMin = nodeView.worldBound.xMin;
                bounds.xMax = nodeView.worldBound.xMax;

                if (index == 0)//��һ��port
                    bounds.yMin = nodeView.worldBound.yMin;
                if (index == portList.Count - 1)//���һ��port
                    bounds.yMax = nodeView.worldBound.yMax;

                if (index > 0)
                {
                    Rect above = portList[index - 1].worldBound;//��ȡ��һ���˿ڵİ�Χ��
                    bounds.yMin = (above.yMax + bounds.yMin) / 2.0f;
                }
                if (index < portList.Count - 1)
                {
                    Rect below = portList[index + 1].worldBound;//��ȡ��һ���˿ڵİ�Χ��
                    bounds.yMax = (below.yMin + bounds.yMax) / 2.0f;
                }

                if (port.direction == Direction.Input)//�������Ͷ˿�
                    bounds.xMin -= kPortDetectionWidth;
                else//������Ͷ˿�
                    bounds.xMax += kPortDetectionWidth;
            }
            else///�˿ڶ��� Ϊ��ֱ����
            {
                // Increase vertical port bounds  ���Ӵ�ֱ�˿ڰ�Χ��
                if (port.direction == Direction.Input)//�������Ͷ˿�
                    bounds.yMin -= kPortDetectionWidth;
                else//������Ͷ˿�
                    bounds.yMax += kPortDetectionWidth;
            }

            return bounds;
        }

        /// <summary>
        /// �������λ�û�ȡedge��ĩ�˶˿�
        /// </summary>
        /// <param name="mousePosition"></param>
        /// <returns></returns>
        private Port GetEndPort(Vector2 mousePosition)
        {
            if (graphView == null)//��ͼΪ��ֱ�ӷ���
                return null;

            Port bestPort = null;
            float bestDistance = 1e20f;

            foreach (var kp in compatiblePorts)
            {
                var nodeView = kp.Key;//�ڵ�
                var portList = kp.Value;//�ڵ��ж�Ӧ�Ķ˿��б�

                // We know that the port in the list is top to bottom in term of layout 
                // �б��еĶ˿��ڲ������Ǵ��ϵ��µ�
                for (int i = 0; i < portList.Count; i++)//����һ���ڵ�����ж˿�
                {
                    var port = portList[i];
                    Rect bounds = GetPortBounds(nodeView, i, portList);

                    float distance = Vector2.Distance(port.worldBound.position, mousePosition);

                    // Check if mouse is over port. �������Ƿ��ڶ˿���
                    if (bounds.Contains(mousePosition) && distance < bestDistance)
                    {
                        bestPort = port;
                        bestDistance = distance;
                    }
                }
            }

            return bestPort;
        }
    }
}