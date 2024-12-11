using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// ������edge�Ĳٿ���:
    ///      ָ��GraphView����ʼ��edge���ӣ��趨EdgeDragHelper����������������edge��
    ///      ע��/ȡ����Ŀ��Ԫ���ϵ��¼��ص���
    ///      ��갴���¼��������Ծ״̬����¼��갴�µ�λ�ã�����edge��صĶ˿ں��ߣ�
    ///      ����ƶ��¼�����edge���¿��ƣ�
    ///      ���̧���¼���ֻ����갴��ʱ��λ���뵱ǰ���λ�ü�ľ�������趨��ֵʱ���������̧���¼�
    ///      �����¼����ڲ���ʱ����Escape������ֹ��ǰ����
    /// </summary>
	public class BaseEdgeConnector : EdgeConnector
    {
        protected BaseEdgeDragHelper dragHelper;
        Edge edgeCandidate;
        protected bool active;
        Vector2 mouseDownPosition;
        protected BaseGraphView graphView;

        internal const float k_ConnectionDistanceTreshold = 10f;//���Ӿ������ֵ

        /// <summary>
        /// ���캯����ָ��GraphView����ʼ��edge����
        /// </summary>
        /// <param name="listener"></param>
		public BaseEdgeConnector(IEdgeConnectorListener listener) : base()
        {
            graphView = (listener as BaseEdgeConnectorListener)?.graphView;
            active = false;
            InitEdgeConnector(listener);
        }

        /// <summary>
        /// ��ʼ��edge���ӣ��趨EdgeDragHelper����������������edge
        /// </summary>
        /// <param name="listener"></param>
        protected virtual void InitEdgeConnector(IEdgeConnectorListener listener)
        {
            dragHelper = new BaseEdgeDragHelper(listener);
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });//* ����������edge *
            //activators��Activationfilters�б�
            //ManipulatorActivationFilter�����ٿ������ڸ�����Ҫ��ƥ���¼�
            //button�����ڼ�������İ�ť
        }

        public override EdgeDragHelper edgeDragHelper => dragHelper;//[����]����edge�϶� helper

        /// <summary>
        /// ���أ���������Ŀ��Ԫ����ע���¼��ص�
        /// </summary>
        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);//target����������VisualElement
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
            target.RegisterCallback<KeyDownEvent>(OnKeyDown);
            target.RegisterCallback<MouseCaptureOutEvent>(OnCaptureOut);
        }

        /// <summary>
        /// ���أ������Դ�Ŀ��Ԫ��ȡ��ע�ᵥ���¼��ص�
        /// </summary>
        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
            target.UnregisterCallback<KeyDownEvent>(OnKeyDown);
        }

        //��갴���¼��������Ծ״̬����¼��갴�µ�λ�ã�����edge��صĶ˿ں���
        protected virtual void OnMouseDown(MouseDownEvent e)
        {
            if (active)//�ڻ�Ծ״̬ʱ�����������¼�����ִ��
            {
                e.StopImmediatePropagation();//����ֹͣ�¼��Ĵ����� ���¼����ᷢ�͵�����·���ϵ�����Ԫ�ء� �˷����ɷ�ֹ�����¼���������ڵ�ǰĿ����ִ�С�
                return;
            }

            //CanStartManipulation:��� MouseEvent �Ƿ��������� ManipulatorActivationFilter Ҫ��
            if (!CanStartManipulation(e))//��������Ҫ��
            {
                return;
            }

            var graphElement = target as Port;//���������� VisualElement��ʾת����Port
            if (graphElement == null)//���޷�ת��Port����ֱ�ӷ���
            {
                return;
            }

            mouseDownPosition = e.localMousePosition;//��ǰĿ������ϵ�е����λ��

            edgeCandidate = graphView != null ? graphView.CreateEdgeView() : new EdgeView();
            edgeDragHelper.draggedPort = graphElement;//*�����϶���Ե�Ķ˿�*
            edgeDragHelper.edgeCandidate = edgeCandidate;//�����϶��ı�Ե

            if (edgeDragHelper.HandleMouseDown(e))//����BaseEdgeDragHelper�ű��б����ء�������갴���¼������ѿ�ʼ�϶���Ϊtrue����֮Ϊfalse
            {
                active = true;//��λ����Ծ״̬��
                target.CaptureMouse();//ָ��һ���¼������������������¼�

                e.StopPropagation();//ֹͣ�������¼����¼����������Ŵ���·�����͸�����Ԫ�ء��˷���������ֹ�����¼���������ڵ�ǰĿ����ִ��
            }
            else
            {
                edgeDragHelper.Reset();//�����϶� helper ��״̬
                edgeCandidate = null;
            }
        }

        //����״̬
        void OnCaptureOut(MouseCaptureOutEvent e)//MouseCaptureOutEvent���ڴ������ֹͣ�������ǰ���͵��¼�
        {
            active = false;
            if (edgeCandidate != null)
                Abort();
        }

        //����ƶ��¼�����edge���¿���
        protected virtual void OnMouseMove(MouseMoveEvent e)
        {
            if (!active) return;

            edgeDragHelper.HandleMouseMove(e);//����BaseEdgeDragHelper�ű��б����ء���������ƶ��¼�
            edgeCandidate.candidatePosition = e.mousePosition;//candidatePosition��������Եʱ�ı�Ե��λ��
            edgeCandidate.UpdateEdgeControl();//���±�Ե�� EdgeControl��������¿���ʧ�ܣ���Ϊ false������ɹ�����Ϊ true��
            e.StopPropagation();//ֹͣ�������¼����¼����������Ŵ���·�����͸�����Ԫ�ء��˷���������ֹ�����¼���������ڵ�ǰĿ����ִ��
        }

        //���̧���¼���ֻ����갴��ʱ��λ���뵱ǰ���λ�ü�ľ�������趨��ֵʱ���������̧���¼�
        protected virtual void OnMouseUp(MouseUpEvent e)
        {
            if (!active || !CanStopManipulation(e))//��� MouseEvent �Ƿ���� Manipulator ���
                return;

            if (CanPerformConnection(e.localMousePosition))//����갴��ʱ��λ���뵱ǰ���λ�ü�ľ��� > �趨����ֵ
                edgeDragHelper.HandleMouseUp(e);//����BaseEdgeDragHelper�ű��б����ء���������ɿ��¼�
            else
                Abort();

            active = false;
            edgeCandidate = null;
            target.ReleaseMouse();//ֹͣ�¼�������򲶻����
            e.StopPropagation();//ֹͣ�������¼����¼����������Ŵ���·�����͸�����Ԫ�ء��˷���������ֹ�����¼���������ڵ�ǰĿ����ִ��
        }

        //�����¼����ڲ���ʱ����Escape������ֹ��ǰ����
        private void OnKeyDown(KeyDownEvent e)
        {
            if (e.keyCode != KeyCode.Escape || !active)//�����µļ�����Escape �� ���ڻ�Ծ״̬
                return;

            Abort();//��ֹ

            active = false;
            target.ReleaseMouse();
            e.StopPropagation();
        }

        /// <summary>
        /// ��ֹ��ǰ���ߵĲ��������ߴ���ͼ��ɾ����������״̬
        /// </summary>
        void Abort()
        {
            var graphView = target?.GetFirstAncestorOfType<GraphView>();//�Ӵ�Ԫ�صĸ�����ʼ���Ϸ��ʲ㼶��ͼ�����ص�һ�� GraphView ���͵� VisualElement
            graphView?.RemoveElement(edgeCandidate);//��ͼ��ɾ��Ԫ��Edge

            edgeCandidate.input = null;
            edgeCandidate.output = null;
            edgeCandidate = null;

            edgeDragHelper.Reset();////�����϶� helper ��״̬
        }

        /// <summary>
        /// �ܷ��������Ӳ���
        /// </summary>
        /// <param name="mousePosition"></param>
        /// <returns></returns>
        bool CanPerformConnection(Vector2 mousePosition)
        {
            return Vector2.Distance(mouseDownPosition, mousePosition) > k_ConnectionDistanceTreshold;
        }
    }
}