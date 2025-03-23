using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;
using System.Reflection;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// �̳�Port�࣬�Զ˿ڵ���ز���
    /// </summary>
	public class PortView : Port//Port��GraphView �˿���
    {
        public string fieldName => fieldInfo.Name;
        public Type fieldType => fieldInfo.FieldType;
        public new Type portType;
        public BaseNodeView owner { get; private set; }
        public PortData portData;

        public event Action<PortView, Edge> OnConnected;
        public event Action<PortView, Edge> OnDisconnected;

        protected FieldInfo fieldInfo;
        protected BaseEdgeConnectorListener listener;

        string userPortStyleFile = "PortViewTypes";

        List<EdgeView> edges = new List<EdgeView>();

        public int connectionCount => edges.Count;

        readonly string portStyle = "GraphStyles/PortView";

        /// <summary>
        /// ���캯������ֵ�˿���Ϣ����ʽ�����¶˿ڳߴ硣�Ƿ�ֱ��ʾ��
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="fieldInfo"></param>
        /// <param name="portData"></param>
        /// <param name="edgeConnectorListener"></param>
        protected PortView(Direction direction, FieldInfo fieldInfo, PortData portData, BaseEdgeConnectorListener edgeConnectorListener)
            : base(portData.vertical ? Orientation.Vertical : Orientation.Horizontal, direction, Capacity.Multi, portData.displayType ?? fieldInfo.FieldType)
        {
            this.fieldInfo = fieldInfo;
            this.listener = edgeConnectorListener;
            this.portType = portData.displayType ?? fieldInfo.FieldType;
            this.portData = portData;
            this.portName = fieldName;

            styleSheets.Add(Resources.Load<StyleSheet>(portStyle));

            UpdatePortSize();//���¶˿ڳߴ�

            var userPortStyle = Resources.Load<StyleSheet>(userPortStyleFile);
            if (userPortStyle != null)
                styleSheets.Add(userPortStyle);

            if (portData.vertical)
                AddToClassList("Vertical");

            this.tooltip = portData.tooltip;
        }

        /// <summary>
        /// �����˿���ͼ�����edge�Ĳٿ������Զ˿ڴ�ֱ��ʾ���в���
        /// </summary>
		public static PortView CreatePortView(Direction direction, FieldInfo fieldInfo, PortData portData, BaseEdgeConnectorListener edgeConnectorListener)
        {
            var pv = new PortView(direction, fieldInfo, portData, edgeConnectorListener);
            pv.m_EdgeConnector = new BaseEdgeConnector(edgeConnectorListener);
            pv.AddManipulator(pv.m_EdgeConnector);///�����VisualElement�����Ĳٿ��������ڴ�����edge�Ĳٿ���

            // Force picking in the port label to enlarge the edge creation zone
            //�ڶ˿ڱ�ǩ��ǿ��ѡȡ������edge��������
            ///Q����������������ƥ��ĵ�һ��Ԫ�ػ� null�����δ�ҵ���
            var portLabel = pv.Q("type");//��(VisualElement��)�㼶��ͼ�в�������Ϊ"type"��VisualElement
            if (portLabel != null)
            {
                portLabel.pickingMode = PickingMode.Position;//��ѡ��
                portLabel.style.flexGrow = 1;//flexGrow��ָ������Ŀ�����ͬһ�����е����������Ŀ���������١�   
            }

            // hide label when the port is vertical  ���˿ڴ�ֱ��ʾʱ������label
            if (portData.vertical && portLabel != null)
                portLabel.style.display = DisplayStyle.None;

            // Fixup picking mode for vertical top ports ��ֱ�����˿ڵĹ̶�ѡȡģʽ
            if (portData.vertical)
                pv.Q("connector").pickingMode = PickingMode.Position;

            return pv;
        }

        /// <summary>
        /// Update the size of the port view (using the portData.sizeInPixel property)
        /// ���¶˿���ͼ�Ĵ�С��ʹ�� portData.sizeInPixel ���ԣ�
        /// </summary>
        public void UpdatePortSize()
        {
            int size = portData.sizeInPixel == 0 ? 8 : portData.sizeInPixel;
            var connector = this.Q("connector");
            var cap = connector.Q("cap");
            connector.style.width = size;
            connector.style.height = size;
            cap.style.width = size - 4;
            cap.style.height = size - 4;

            // Update connected edge sizes: ����edge�ĳߴ�
            edges.ForEach(e => e.UpdateEdgeSize());
        }

        /// <summary>
        /// ��ʼ���������˿����ڵĽڵ㣬�����˿����ͣ��˿����ƣ���ʽ��أ��ı���ʾ
        /// </summary>
        /// <param name="nodeView"></param>
        /// <param name="name"></param>
		public virtual void Initialize(BaseNodeView nodeView, string name)
        {
            this.owner = nodeView;
            AddToClassList(fieldName);//�������б��Դ�uss������ʽ

            // Correct port type if port accept multiple values (and so is a container) ����˿ڽ��ܶ��ֵ��������Ķ˿����ͣ�����Ҳ����ˣ�
            if (direction == Direction.Input && portData.acceptMultipleEdges && portType == fieldType) // If the user haven't set a custom field type
            {
                if (fieldType.GetGenericArguments().Length > 0)//GetGenericArguments�����ر�ʾ���ʽ�������͵����Ͳ����������Ͷ�������Ͳ����� Type ���������
                    portType = fieldType.GetGenericArguments()[0];
            }

            if (name != null)
                portName = name;
            visualClass = "Port_" + portType.Name;//visualClass��������ʽ���˿ڵ� USS �������
            tooltip = portData.tooltip;//�û���Ԫ����ͣһС��ʱ�����ʾ����Ϣ���ڵ��ı�����ʾ��
        }

        /// <summary>
        /// ���ض˿����ӵĺ���
        /// </summary>
        /// <param name="edge"></param>
		public override void Connect(Edge edge)//�˿ڵ����Ӻͱ�Ե
        {
            OnConnected?.Invoke(this, edge);

            base.Connect(edge);///*����*

            var inputNode = (edge.input as PortView).owner;
            var outputNode = (edge.output as PortView).owner;

            edges.Add(edge as EdgeView);

            inputNode.OnPortConnected(edge.input as PortView);
            outputNode.OnPortConnected(edge.output as PortView);
        }

        /// <summary>
        /// ���ضϿ��˿����ߵĺ���
        /// </summary>
        /// <param name="edge"></param>
		public override void Disconnect(Edge edge)
        {
            OnDisconnected?.Invoke(this, edge);

            base.Disconnect(edge);//�Ͽ���Ե��˿ڵ�����

            if (!(edge as EdgeView).isConnected)
                return;

            var inputNode = (edge.input as PortView)?.owner;
            var outputNode = (edge.output as PortView)?.owner;

            inputNode?.OnPortDisconnected(edge.input as PortView);
            outputNode?.OnPortDisconnected(edge.output as PortView);

            edges.Remove(edge as EdgeView);
        }

        public void UpdatePortView(PortData data)
        {
            if (data.displayType != null)//��ʽ
            {
                base.portType = data.displayType;//����Ķ˿�����
                portType = data.displayType;
                visualClass = "Port_" + portType.Name;//������ʽ���˿ڵ�uss�������
            }
            if (!String.IsNullOrEmpty(data.displayName))//���ڵ���ʾ����Ϊ��
                base.portName = data.displayName;//�˿�����

            portData = data;

            // Update the edge in case the port color have changed ���±�Ե�Է��˿���ɫ�����仯
            //schedule�������� VisualElement �� IVisualElementScheduler
            schedule.Execute(() => {
                foreach (var edge in edges)
                {
                    edge.UpdateEdgeControl();//���±�Ե�� EdgeControl��
                    edge.MarkDirtyRepaint();//����һ֡���� VisualElement ���ػ�
                }
            }).ExecuteLater(50); // Hummm //50ms��ִ��

            UpdatePortSize();
        }

        public List<EdgeView> GetEdges()
        {
            return edges;
        }
    }
}