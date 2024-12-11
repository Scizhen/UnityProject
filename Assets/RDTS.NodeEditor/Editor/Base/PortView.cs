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
    /// 继承Port类，对端口的相关操作
    /// </summary>
	public class PortView : Port//Port：GraphView 端口类
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
        /// 构造函数：赋值端口信息，样式，更新端口尺寸。是否垂直显示等
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

            UpdatePortSize();//更新端口尺寸

            var userPortStyle = Resources.Load<StyleSheet>(userPortStyleFile);
            if (userPortStyle != null)
                styleSheets.Add(userPortStyle);

            if (portData.vertical)
                AddToClassList("Vertical");

            this.tooltip = portData.tooltip;
        }

        /// <summary>
        /// 创建端口视图，添加edge的操控器，对端口垂直显示进行操作
        /// </summary>
		public static PortView CreatePortView(Direction direction, FieldInfo fieldInfo, PortData portData, BaseEdgeConnectorListener edgeConnectorListener)
        {
            var pv = new PortView(direction, fieldInfo, portData, edgeConnectorListener);
            pv.m_EdgeConnector = new BaseEdgeConnector(edgeConnectorListener);
            pv.AddManipulator(pv.m_EdgeConnector);///添加与VisualElement关联的操控器：用于创建新edge的操控器

            // Force picking in the port label to enlarge the edge creation zone
            //在端口标签中强制选取以扩大edge创建区域
            ///Q：返回与所有条件匹配的第一个元素或 null（如果未找到）
            var portLabel = pv.Q("type");//在(VisualElement的)层级视图中查找名称为"type"的VisualElement
            if (portLabel != null)
            {
                portLabel.pickingMode = PickingMode.Position;//可选中
                portLabel.style.flexGrow = 1;//flexGrow：指定此项目相对于同一容器中的其余灵活项目将增长多少。   
            }

            // hide label when the port is vertical  当端口垂直显示时，隐藏label
            if (portData.vertical && portLabel != null)
                portLabel.style.display = DisplayStyle.None;

            // Fixup picking mode for vertical top ports 垂直顶部端口的固定选取模式
            if (portData.vertical)
                pv.Q("connector").pickingMode = PickingMode.Position;

            return pv;
        }

        /// <summary>
        /// Update the size of the port view (using the portData.sizeInPixel property)
        /// 更新端口视图的大小（使用 portData.sizeInPixel 属性）
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

            // Update connected edge sizes: 更新edge的尺寸
            edges.ForEach(e => e.UpdateEdgeSize());
        }

        /// <summary>
        /// 初始化：关联端口所在的节点，纠正端口类型，端口名称，样式相关，文本提示
        /// </summary>
        /// <param name="nodeView"></param>
        /// <param name="name"></param>
		public virtual void Initialize(BaseNodeView nodeView, string name)
        {
            this.owner = nodeView;
            AddToClassList(fieldName);//加入类列表，以从uss分配样式

            // Correct port type if port accept multiple values (and so is a container) 如果端口接受多个值，则纠正的端口类型（容器也是如此）
            if (direction == Direction.Input && portData.acceptMultipleEdges && portType == fieldType) // If the user haven't set a custom field type
            {
                if (fieldType.GetGenericArguments().Length > 0)//GetGenericArguments：返回表示封闭式泛型类型的类型参数或泛型类型定义的类型参数的 Type 对象的数组
                    portType = fieldType.GetGenericArguments()[0];
            }

            if (name != null)
                portName = name;
            visualClass = "Port_" + portType.Name;//visualClass：用于样式化端口的 USS 类的名称
            tooltip = portData.tooltip;//用户将元素悬停一小段时间后显示在信息框内的文本（提示）
        }

        /// <summary>
        /// 重载端口连接的函数
        /// </summary>
        /// <param name="edge"></param>
		public override void Connect(Edge edge)//端口的连接和边缘
        {
            OnConnected?.Invoke(this, edge);

            base.Connect(edge);///*连线*

            var inputNode = (edge.input as PortView).owner;
            var outputNode = (edge.output as PortView).owner;

            edges.Add(edge as EdgeView);

            inputNode.OnPortConnected(edge.input as PortView);
            outputNode.OnPortConnected(edge.output as PortView);
        }

        /// <summary>
        /// 重载断开端口连线的函数
        /// </summary>
        /// <param name="edge"></param>
		public override void Disconnect(Edge edge)
        {
            OnDisconnected?.Invoke(this, edge);

            base.Disconnect(edge);//断开边缘与端口的连接

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
            if (data.displayType != null)//样式
            {
                base.portType = data.displayType;//基类的端口类型
                portType = data.displayType;
                visualClass = "Port_" + portType.Name;//用于样式化端口的uss类的名称
            }
            if (!String.IsNullOrEmpty(data.displayName))//若节点显示名称为空
                base.portName = data.displayName;//端口名称

            portData = data;

            // Update the edge in case the port color have changed 更新边缘以防端口颜色发生变化
            //schedule：检索此 VisualElement 的 IVisualElementScheduler
            schedule.Execute(() => {
                foreach (var edge in edges)
                {
                    edge.UpdateEdgeControl();//更新边缘的 EdgeControl。
                    edge.MarkDirtyRepaint();//在下一帧触发 VisualElement 的重绘
                }
            }).ExecuteLater(50); // Hummm //50ms后执行

            UpdatePortSize();
        }

        public List<EdgeView> GetEdges()
        {
            return edges;
        }
    }
}