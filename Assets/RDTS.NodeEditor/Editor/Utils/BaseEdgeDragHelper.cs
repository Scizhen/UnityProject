using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// 主要实现对edge的拖动和连接操作：
    ///     重载鼠标按下事件；
    ///     重载鼠标移动事件；
    ///     重载鼠标抬起事件；
    ///     视图的平移；
    ///     端口的高亮和鬼影
    /// </summary>
    public class BaseEdgeDragHelper : EdgeDragHelper
    {
        ///Unity官方实例
        // https://github.com/Unity-Technologies/UnityCsReference/blob/master/Modules/GraphViewEditor/Manipulators/EdgeDragHelper.cs#L21
        internal const int k_PanAreaWidth = 30;//平移区域宽度
        internal const int k_PanSpeed = 4;//平移速度
        internal const int k_PanInterval = 10;//平移的间隔时间
        internal const float k_MinSpeedFactor = 0.5f;
        internal const float k_MaxSpeedFactor = 7f;
        internal const float k_MaxPanSpeed = k_MaxSpeedFactor * k_PanSpeed;//最大平移速度
        internal const float kPortDetectionWidth = 30;//端口检测宽度

        protected Dictionary<BaseNodeView, List<PortView>> compatiblePorts = new Dictionary<BaseNodeView, List<PortView>>();
        private Edge ghostEdge;
        protected GraphView graphView;
        protected static NodeAdapter nodeAdapter = new NodeAdapter();

        /// <summary>供 EdgeConnector 操控器使用以完成实际的edge创建工作。借助此接口，用户能以不同的方式覆盖和创建edge</summary>
        protected readonly IEdgeConnectorListener listener;

        /// <summary>表示使用 VisualElement 的计划接口创建的计划任务 </summary>
        private IVisualElementScheduledItem panSchedule;
        private Vector3 panDiff = Vector3.zero;
        private bool wasPanned;//视图(图表是否平移)

        public bool resetPositionOnPan { get; set; }

        public BaseEdgeDragHelper(IEdgeConnectorListener listener)
        {
            this.listener = listener;//指定edge创建工作的接口
            resetPositionOnPan = true;
            Reset();
        }

        /// <summary>[覆盖父类]正在拖动的edge</summary>
        public override Edge edgeCandidate { get; set; }
        /// <summary>[覆盖父类]从中拖动边缘的端口</summary>
        public override Port draggedPort { get; set; }

        /// <summary>
        /// 重载父类：重置拖动helper的状态
        /// </summary>
        /// <param name="didConnect"></param>
        public override void Reset(bool didConnect = false)
        {
            if (compatiblePorts != null && graphView != null)
            {
                // Reset the highlights.
                graphView.ports.ForEach((p) => {
                    p.OnStopEdgeDragging();//在edge拖动结束时调用
                });
                compatiblePorts.Clear();
            }

            // Clean up ghost edge. 清理鬼影edge
            if ((ghostEdge != null) && (graphView != null))
            {
                var pv = ghostEdge.input as PortView;
                graphView.schedule.Execute(() => {
                    pv.portCapLit = false;//端口盖是否点亮
                    // pv.UpdatePortView(pv.portData);
                }).ExecuteLater(10);//10ms后执行
                graphView.RemoveElement(ghostEdge);//删除鬼影edge
            }

            if (wasPanned)//若视图graphView被平移了
            {
                ///Debug.Log("平移");
                if (!resetPositionOnPan || didConnect)//在拖动edge对视图进行了平移，并且在鼠标抬起时有连接到端口
                {
                    Vector3 p = graphView.contentViewContainer.transform.position;
                    Vector3 s = graphView.contentViewContainer.transform.scale;
                    graphView.UpdateViewTransform(p, s);//更新视口变换。 参数（新位置，新缩放）

                }
            }

            if (panSchedule != null)
                panSchedule.Pause();

            if (ghostEdge != null)//重置(消除)鬼影edge
            {
                ghostEdge.input = null;
                ghostEdge.output = null;
            }

            if (draggedPort != null && !didConnect)
            {
                draggedPort.portCapLit = false;
                draggedPort = null;
            }

            if (edgeCandidate != null)//若有 正在拖动的edge
            {
                edgeCandidate.SetEnabled(true);
            }

            ghostEdge = null;
            edgeCandidate = null;

            graphView = null;
        }

        /// <summary>
        /// 处理鼠标按下事件：
        ///     若点中拖动edge的端口，则高亮此端口和此端口兼容的端口；
        ///     对所在节点的端口进行排序，便于包围盒的计算；
        ///     计划视图平移的操作
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        public override bool HandleMouseDown(MouseDownEvent evt)
        {
            Vector2 mousePosition = evt.mousePosition;//鼠标位置

            if ((draggedPort == null) || (edgeCandidate == null))
            {
                return false;
            }

            graphView = draggedPort.GetFirstAncestorOfType<GraphView>();//从此元素的父级开始向上访问层级视图，返回第一个 GraphView 类型的 VisualElement

            if (graphView == null)
            {
                return false;
            }

            if (edgeCandidate.parent == null)
            {
                graphView.AddElement(edgeCandidate);
            }

            bool startFromOutput = (draggedPort.direction == Direction.Output);//端口方向：true为输出，false为输入

            edgeCandidate.candidatePosition = mousePosition;//创建边缘时的边缘端位置，即为鼠标位置
            edgeCandidate.SetEnabled(false);// 更改 VisualElement 启用状态。禁用的 VisualElement 不接收大多数事件

            if (startFromOutput)//输出端口
            {
                edgeCandidate.output = draggedPort;
                edgeCandidate.input = null;
            }
            else//输入端口
            {
                edgeCandidate.output = null;
                edgeCandidate.input = draggedPort;
            }

            draggedPort.portCapLit = true;//端口盖点亮

            compatiblePorts.Clear();

            foreach (PortView port in graphView.GetCompatiblePorts(draggedPort, nodeAdapter))//获取与给定端口draggedPort 兼容的所有端口
            {
                compatiblePorts.TryGetValue(port.owner, out var portList);//若找到返回值，将与键“port.owner”相关联的值返回到portList中
                if (portList == null)//为空则创建一个新的列表
                    portList = compatiblePorts[port.owner] = new List<PortView>();
                portList.Add(port);//添加兼容的端口
            }

            // Sort ports by position in the node 按节点中的位置对端口进行排序
            foreach (var kp in compatiblePorts)
                kp.Value.Sort((e1, e2) => e1.worldBound.y.CompareTo(e2.worldBound.y));//按照y轴上的坐标大小进行排列（小到大），即端口从上往下

            // Only light compatible anchors when dragging an edge. 拖动edge时，只点亮兼容的端口
            graphView.ports.ForEach((p) => {
                p.OnStartEdgeDragging();//在拖动边缘时调用
            });

            foreach (var kp in compatiblePorts)
                foreach (var port in kp.Value)
                    port.highlight = true;//突出显示端口

            edgeCandidate.UpdateEdgeControl();//更新edge的 EdgeControl

            if (panSchedule == null)
            {
                //Execute:计划稍后执行此操作  
                //Every：指定时间后重复此操作
                //StartingIn：向第一次调用添加延时
                //Pause：从此项目的 VisualElement 的计划程序中将其删除
                panSchedule = graphView.schedule.Execute(Pan).Every(k_PanInterval).StartingIn(k_PanInterval);
                panSchedule.Pause();
            }
            wasPanned = false;

            edgeCandidate.layer = Int32.MaxValue;//图形中的 GraphElement 图层

            return true;
        }

        /// <summary>
        /// 获得有效的平移速度
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

        Vector2 lastMousePos;//记录上次鼠标位置
        /// <summary>
        /// 处理鼠标移动事件：
        ///     使edge另一端跟随鼠标位置移动；
        ///     当edge另一端的端口可能存在时绘制鬼影edge以提示；
        ///     当可能的端口不存在、鬼影edge存在时消除鬼影edge
        /// </summary>
        /// <param name="evt"></param>
        public override void HandleMouseMove(MouseMoveEvent evt)
        {
            var ve = (VisualElement)evt.target;//收到此事件的目标视觉元素。与 currentTarget 不同，当事件沿传播路径发送给其他元素时，此目标不会更改。
            //contentContainer：子元素通常添加到此元素中
            Vector2 gvMousePos = ve.ChangeCoordinatesTo(graphView.contentContainer, evt.localMousePosition);//将[当前目标坐标系]的鼠标位置转换到contentContainer下
            panDiff = GetEffectivePanSpeed(gvMousePos);//获得有效的平移速度

            if (panDiff != Vector3.zero)
                panSchedule.Resume();//如果尚未激活，将根据其 VisualElement 的计划程序计划此项目
            else
                panSchedule.Pause();//从此项目的 VisualElement 的计划程序中将其删除

            Vector2 mousePosition = evt.mousePosition;//[屏幕坐标系]中的鼠标位置
            lastMousePos = evt.mousePosition;

            edgeCandidate.candidatePosition = mousePosition;//创建edge时的edge端位置为鼠标移动的位置

            // Draw ghost edge if possible port exists.  如果可能的端口存在，绘制鬼影edge（即当鼠标将edge拖动到可以连接的端口时，产生一条预想的edge）
            Port endPort = GetEndPort(mousePosition);

            ////* 对鬼影edge的处理 *////

            if (endPort != null)///edge末端端口存在
            {
                if (ghostEdge == null)
                {
                    ghostEdge = CreateEdgeView();
                    ghostEdge.isGhostEdge = true;//此边缘为重影边缘（创建边缘时以对齐方式出现在附近端口的边缘）
                    ghostEdge.pickingMode = PickingMode.Ignore;//确定在 mouseEvents 或 IPanel.Pick 查询期间是否可以选取此元素。   Ignore：禁用拾取
                    graphView.AddElement(ghostEdge);//在视图中添加此元素
                }

                if (edgeCandidate.output == null)//当前操作的edge输出端口为空时(输入端口一定存在，否则不会有edgeCandidate)，确定鬼影edge的输入端口，若输出端口为空则将endport作为其输出端口，并高亮
                {
                    ghostEdge.input = edgeCandidate.input;//赋值输入端口
                    if (ghostEdge.output != null)
                        ghostEdge.output.portCapLit = false;
                    ghostEdge.output = endPort;
                    ghostEdge.output.portCapLit = true;
                }
                else //当前edge输出端口不为空，则将此edge的输入/输出端口赋给鬼影edge
                {
                    if (ghostEdge.input != null)
                        ghostEdge.input.portCapLit = false;
                    ghostEdge.input = endPort;
                    ghostEdge.input.portCapLit = true;
                    ghostEdge.output = edgeCandidate.output;
                }
            }
            else if (ghostEdge != null)///若edge的末端端口不存在，且鬼影edge存在，则需要移除鬼影edge
            {
                if (edgeCandidate.input == null)//当前操作的edge的输入端口为空（则需操作edge连接到输入端口）
                {
                    if (ghostEdge.input != null)//鬼影edge输入端口存在，不高亮端口
                        ghostEdge.input.portCapLit = false;
                }
                else//当前操作的edge的输入端口存在（则需操作edge连接到输出端口）
                {
                    if (ghostEdge.output != null)//鬼影edge输出端口存在，不高亮端口
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
        /// 对视图的平移，对edge的拖动和连接
        /// </summary>
        /// <param name="ts"></param>
        private void Pan(TimerState ts)//TimerState类:包含计划程序事件的计时信息
        {
            //viewTransform：图表的视图变换
            ///当鼠标拖动edge到视图边缘时，移动视图
            graphView.viewTransform.position -= panDiff;//position：此 VisualElement 的变换的位置

            // Workaround to force edge to update when we pan the graph 当我们平移图形时强制边缘更新的解决方法
            edgeCandidate.output = edgeCandidate.output;
            edgeCandidate.input = edgeCandidate.input;

            edgeCandidate.UpdateEdgeControl();
            wasPanned = true;
            ///Debug.Log("平移");
        }

        /// <summary>
        /// 处理鼠标松开事件：
        ///     清除鬼影；
        ///     若存在端口就连接；
        ///     若在空白区域就创建搜索窗口
        /// </summary>
        /// <param name="evt"></param>
        public override void HandleMouseUp(MouseUpEvent evt)
        {
            bool didConnect = false;

            Vector2 mousePosition = evt.mousePosition;

            // Reset the highlights. 重置 高亮
            graphView.ports.ForEach((p) => {
                p.OnStopEdgeDragging(); //在边缘拖动结束时调用
            });

            // Clean up ghost edges. 若鬼影edge存在，就清除它
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

            Port endPort = GetEndPort(mousePosition);//获取鼠标处的端口为edge的末端端口

            if (endPort == null && listener != null)
            {
                //在空白空间放置边缘时调用
                //参数（edgeCandidate：正在放置的edge, mousePosition：在空白区域中放置edge的位置）
                //在BaseEdgeConnectorListener中重载了此方法，用于创建搜索窗口//
                listener.OnDropOutsidePort(edgeCandidate, mousePosition);
                Debug.Log("edge末端创建搜索窗口");
            }

            edgeCandidate.SetEnabled(true);

            if (edgeCandidate.input != null)
                edgeCandidate.input.portCapLit = false;

            if (edgeCandidate.output != null)
                edgeCandidate.output.portCapLit = false;

            // If it is an existing valid edge then delete and notify the model (using DeleteElements()).
            // 如果它是现有的有效边，则删除并通知模型（使用 DeleteElements()）。[删除已有的edge]
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
            //否则，如果它是一个临时边缘，那么只需将其移除，因为我的模型还不知道它
            else
            {
                graphView.RemoveElement(edgeCandidate);
            }

            //可能的端口存在，则用当前拖动的edge去连接两个端口
            if (endPort != null)
            {
                if (endPort.direction == Direction.Output)//可能的端口为输出端口
                    edgeCandidate.output = endPort;
                else//可能的端口为输入端口
                    edgeCandidate.input = endPort;

                //在端口放置新的边缘时调用，若要连接到新的端口则断开旧的连接新的，若没有连接到对应的端口也断开，若是原端口则不变
                listener.OnDrop(graphView, edgeCandidate);
                didConnect = true;///进行连接
            }
            else
            {
                edgeCandidate.output = null;
                edgeCandidate.input = null;
            }

            edgeCandidate.ResetLayer();//将edge重置为其原始图层

            edgeCandidate = null;
            compatiblePorts.Clear();
            Reset(didConnect);
        }

        /// <summary>
        /// 获取端口的包围盒
        /// </summary>
        /// <param name="nodeView"></param>
        /// <param name="index"></param>
        /// <param name="portList"></param>
        /// <returns></returns>
        Rect GetPortBounds(BaseNodeView nodeView, int index, List<PortView> portList)
        {
            var port = portList[index];//端口列表中对应索引的端口
            var bounds = port.worldBound;//将世界变换应用于矩形后的AABB(轴对齐的包围盒：坐标轴对齐并且完全包围 某个对象的盒体)

            ///端口定向 为水平方向
            if (port.orientation == Orientation.Horizontal)
            {
                // Increase horizontal port bounds 增加水平端口包围盒
                bounds.xMin = nodeView.worldBound.xMin;
                bounds.xMax = nodeView.worldBound.xMax;

                if (index == 0)//第一个port
                    bounds.yMin = nodeView.worldBound.yMin;
                if (index == portList.Count - 1)//最后一个port
                    bounds.yMax = nodeView.worldBound.yMax;

                if (index > 0)
                {
                    Rect above = portList[index - 1].worldBound;//获取上一个端口的包围盒
                    bounds.yMin = (above.yMax + bounds.yMin) / 2.0f;
                }
                if (index < portList.Count - 1)
                {
                    Rect below = portList[index + 1].worldBound;//获取下一个端口的包围盒
                    bounds.yMax = (below.yMin + bounds.yMax) / 2.0f;
                }

                if (port.direction == Direction.Input)//输入类型端口
                    bounds.xMin -= kPortDetectionWidth;
                else//输出类型端口
                    bounds.xMax += kPortDetectionWidth;
            }
            else///端口定向 为垂直方向
            {
                // Increase vertical port bounds  增加垂直端口包围盒
                if (port.direction == Direction.Input)//输入类型端口
                    bounds.yMin -= kPortDetectionWidth;
                else//输出类型端口
                    bounds.yMax += kPortDetectionWidth;
            }

            return bounds;
        }

        /// <summary>
        /// 根据鼠标位置获取edge的末端端口
        /// </summary>
        /// <param name="mousePosition"></param>
        /// <returns></returns>
        private Port GetEndPort(Vector2 mousePosition)
        {
            if (graphView == null)//视图为空直接返回
                return null;

            Port bestPort = null;
            float bestDistance = 1e20f;

            foreach (var kp in compatiblePorts)
            {
                var nodeView = kp.Key;//节点
                var portList = kp.Value;//节点中对应的端口列表

                // We know that the port in the list is top to bottom in term of layout 
                // 列表中的端口在布局上是从上到下的
                for (int i = 0; i < portList.Count; i++)//遍历一个节点的所有端口
                {
                    var port = portList[i];
                    Rect bounds = GetPortBounds(nodeView, i, portList);

                    float distance = Vector2.Distance(port.worldBound.position, mousePosition);

                    // Check if mouse is over port. 检查鼠标是否在端口上
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