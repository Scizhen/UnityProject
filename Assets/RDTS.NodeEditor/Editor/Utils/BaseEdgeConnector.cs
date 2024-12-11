using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// 创建新edge的操控器:
    ///      指定GraphView，初始化edge连接，设定EdgeDragHelper，设置鼠标左键激活edge；
    ///      注册/取消在目标元素上的事件回调；
    ///      鼠标按下事件：进入活跃状态，记录鼠标按下的位置，设置edge相关的端口和线；
    ///      鼠标移动事件：对edge更新控制；
    ///      鼠标抬起事件：只当鼠标按下时的位置与当前鼠标位置间的距离大于设定阈值时，处理鼠标抬起事件
    ///      按键事件：在操作时按下Escape键，中止当前操作
    /// </summary>
	public class BaseEdgeConnector : EdgeConnector
    {
        protected BaseEdgeDragHelper dragHelper;
        Edge edgeCandidate;
        protected bool active;
        Vector2 mouseDownPosition;
        protected BaseGraphView graphView;

        internal const float k_ConnectionDistanceTreshold = 10f;//连接距离的阈值

        /// <summary>
        /// 构造函数：指定GraphView，初始化edge连接
        /// </summary>
        /// <param name="listener"></param>
		public BaseEdgeConnector(IEdgeConnectorListener listener) : base()
        {
            graphView = (listener as BaseEdgeConnectorListener)?.graphView;
            active = false;
            InitEdgeConnector(listener);
        }

        /// <summary>
        /// 初始化edge连接：设定EdgeDragHelper，设置鼠标左键激活edge
        /// </summary>
        /// <param name="listener"></param>
        protected virtual void InitEdgeConnector(IEdgeConnectorListener listener)
        {
            dragHelper = new BaseEdgeDragHelper(listener);
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });//* 鼠标左键激活edge *
            //activators：Activationfilters列表
            //ManipulatorActivationFilter：供操控器用于根据其要求匹配事件
            //button：用于激活操作的按钮
        }

        public override EdgeDragHelper edgeDragHelper => dragHelper;//[覆盖]引用edge拖动 helper

        /// <summary>
        /// 重载：调用来在目标元素上注册事件回调
        /// </summary>
        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);//target：被操作的VisualElement
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
            target.RegisterCallback<KeyDownEvent>(OnKeyDown);
            target.RegisterCallback<MouseCaptureOutEvent>(OnCaptureOut);
        }

        /// <summary>
        /// 重载：调用以从目标元素取消注册单击事件回调
        /// </summary>
        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
            target.UnregisterCallback<KeyDownEvent>(OnKeyDown);
        }

        //鼠标按下事件：进入活跃状态，记录鼠标按下的位置，设置edge相关的端口和线
        protected virtual void OnMouseDown(MouseDownEvent e)
        {
            if (active)//在活跃状态时，避免其他事件程序执行
            {
                e.StopImmediatePropagation();//立即停止事件的传播。 该事件不会发送到传播路径上的其他元素。 此方法可防止其他事件处理程序在当前目标上执行。
                return;
            }

            //CanStartManipulation:检查 MouseEvent 是否满足所有 ManipulatorActivationFilter 要求
            if (!CanStartManipulation(e))//若不满足要求
            {
                return;
            }

            var graphElement = target as Port;//将被操作的 VisualElement显示转换成Port
            if (graphElement == null)//若无法转换Port，则直接返回
            {
                return;
            }

            mouseDownPosition = e.localMousePosition;//当前目标坐标系中的鼠标位置

            edgeCandidate = graphView != null ? graphView.CreateEdgeView() : new EdgeView();
            edgeDragHelper.draggedPort = graphElement;//*从中拖动边缘的端口*
            edgeDragHelper.edgeCandidate = edgeCandidate;//正在拖动的边缘

            if (edgeDragHelper.HandleMouseDown(e))//【在BaseEdgeDragHelper脚本中被重载】处理鼠标按下事件：若已开始拖动则为true，反之为false
            {
                active = true;//置位“活跃状态”
                target.CaptureMouse();//指定一个事件处理程序来捕获鼠标事件

                e.StopPropagation();//停止传播此事件。事件将不会沿着传播路径发送给其他元素。此方法不会阻止其他事件处理程序在当前目标上执行
            }
            else
            {
                edgeDragHelper.Reset();//重置拖动 helper 的状态
                edgeCandidate = null;
            }
        }

        //重置状态
        void OnCaptureOut(MouseCaptureOutEvent e)//MouseCaptureOutEvent：在处理程序停止捕获鼠标前发送的事件
        {
            active = false;
            if (edgeCandidate != null)
                Abort();
        }

        //鼠标移动事件：对edge更新控制
        protected virtual void OnMouseMove(MouseMoveEvent e)
        {
            if (!active) return;

            edgeDragHelper.HandleMouseMove(e);//【在BaseEdgeDragHelper脚本中被重载】处理鼠标移动事件
            edgeCandidate.candidatePosition = e.mousePosition;//candidatePosition：创建边缘时的边缘端位置
            edgeCandidate.UpdateEdgeControl();//更新边缘的 EdgeControl。如果更新控制失败，则为 false。如果成功，则为 true。
            e.StopPropagation();//停止传播此事件。事件将不会沿着传播路径发送给其他元素。此方法不会阻止其他事件处理程序在当前目标上执行
        }

        //鼠标抬起事件：只当鼠标按下时的位置与当前鼠标位置间的距离大于设定阈值时，处理鼠标抬起事件
        protected virtual void OnMouseUp(MouseUpEvent e)
        {
            if (!active || !CanStopManipulation(e))//检查 MouseEvent 是否与此 Manipulator 相关
                return;

            if (CanPerformConnection(e.localMousePosition))//若鼠标按下时的位置与当前鼠标位置间的距离 > 设定的阈值
                edgeDragHelper.HandleMouseUp(e);//【在BaseEdgeDragHelper脚本中被重载】处理鼠标松开事件
            else
                Abort();

            active = false;
            edgeCandidate = null;
            target.ReleaseMouse();//停止事件处理程序捕获鼠标
            e.StopPropagation();//停止传播此事件。事件将不会沿着传播路径发送给其他元素。此方法不会阻止其他事件处理程序在当前目标上执行
        }

        //按键事件：在操作时按下Escape键，中止当前操作
        private void OnKeyDown(KeyDownEvent e)
        {
            if (e.keyCode != KeyCode.Escape || !active)//若按下的键不是Escape 或 不在活跃状态
                return;

            Abort();//中止

            active = false;
            target.ReleaseMouse();
            e.StopPropagation();
        }

        /// <summary>
        /// 中止当前连线的操作，将线从视图中删除，并重置状态
        /// </summary>
        void Abort()
        {
            var graphView = target?.GetFirstAncestorOfType<GraphView>();//从此元素的父级开始向上访问层级视图，返回第一个 GraphView 类型的 VisualElement
            graphView?.RemoveElement(edgeCandidate);//从图中删除元素Edge

            edgeCandidate.input = null;
            edgeCandidate.output = null;
            edgeCandidate = null;

            edgeDragHelper.Reset();////重置拖动 helper 的状态
        }

        /// <summary>
        /// 能否履行连接操作
        /// </summary>
        /// <param name="mousePosition"></param>
        /// <returns></returns>
        bool CanPerformConnection(Vector2 mousePosition)
        {
            return Vector2.Distance(mouseDownPosition, mousePosition) > k_ConnectionDistanceTreshold;
        }
    }
}