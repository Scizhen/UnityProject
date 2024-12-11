using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System.Linq;
using System;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// Parameters������ͼ�ű�������graphView�����ñ��⣬���ί�кͻص������²����б���Ӱ�ť(���Ͻǵġ�+����ť)
    /// </summary>
	public class ExposedParameterView : PinnedElementView
    {
        protected BaseGraphView graphView;//������ͼ��

        new const string title = "Parameters";// ExposedParameter ���ı���

        readonly string exposedParameterViewStyle = "GraphProcessorStyles/ExposedParameterView";

        List<Rect> blackboardLayouts = new List<Rect>();//����������������Ԫ�صĲ��֣�λ�ã��ߴ磩���б�

        public ExposedParameterView()
        {
            var style = Resources.Load<StyleSheet>(exposedParameterViewStyle);
            if (style != null)
                styleSheets.Add(style);
        }

        protected virtual void OnAddClicked()
        {
            ///GenericMenu���������Զ��������Ĳ˵��������˵�
            var parameterType = new GenericMenu();

            foreach (var paramType in GetExposedParameterTypes())//����ExposedParameter���������ͣ���Ӳ˵���
                parameterType.AddItem(new GUIContent(GetNiceNameFromType(paramType)), false, () =>
                {
                    string uniqueName = "New " + GetNiceNameFromType(paramType);

                    uniqueName = GetUniqueExposedPropertyName(uniqueName);
                    graphView.graph.AddExposedParameter(uniqueName, paramType);
                });

            parameterType.ShowAsContext();///�Ҽ�����ʱ���������ʾ�˵�
        }

        protected string GetNiceNameFromType(Type type)
        {
            string name = type.Name;

            // Remove parameter in the name of the type if it exists
            name = name.Replace("Parameter", "");

            return ObjectNames.NicifyVariableName(name);
        }

        /// <summary>��ȡһ��Ψһ��ExposedParameter������</summary>
        protected string GetUniqueExposedPropertyName(string name)
        {
            // Generate unique name  ����Ψһ����
            string uniqueName = name;
            int i = 0;
            while (graphView.graph.exposedParameters.Any(e => e.name == name))//�������ظ��ģ��ͼӵ���������
                name = uniqueName + " " + i++;
            return name;
        }

        /// <summary>��һ��һ������ȡExposedParameter����������</summary>
        protected virtual IEnumerable<Type> GetExposedParameterTypes()
        {
            foreach (var type in TypeCache.GetTypesDerivedFrom<ExposedParameter>())//����ExposedParameter���������͵ļ���
            {
                ///IsGenericType����ȡһ��ֵ����ֵָʾ��ǰ�����Ƿ��Ƿ�������
                if (type.IsGenericType)
                    continue;

                yield return type;
            }
        }

        /// <summary>����ExposedParameter���(����)�еĲ����б������ĸ������Ƿ�չ�� </summary>
        protected virtual void UpdateParameterList()
        {
            content.Clear();//�����������

            foreach (var param in graphView.graph.exposedParameters)//����graph�е�exposedParameters
            {
                ///BlackboardRow��һ�����۵��� GraphElement�����ڱ�ʾ BlackboardSection �е�һ��
                ///           ����1������BlackboardRow�Ļ���
                ///           ����2������BlackboardRowչ��������ݵĻ���
                var row = new BlackboardRow(new ExposedParameterFieldView(graphView, param), new ExposedParameterPropertyView(graphView, param));
                row.expanded = param.settings.expanded;//�Ƿ���չ�� BlackboardRow
                row.RegisterCallback<GeometryChangedEvent>(e => {///ע��ص������в��ּ����Ԫ�ص�λ�û�ߴ緢���仯ʱ���͵��¼�
                    param.settings.expanded = row.expanded;//��¼�����Ƿ�չ����״̬
                });

                content.Add(row);//���һ������Ԫ��
            }
        }

        /// <summary>
        /// [����]����ExposedParameter���(����)�ĳ�ʼ��������graphView�����ñ��⣬���ί�кͻص������²����б���Ӱ�ť
        /// </summary>
        /// <param name="graphView"></param>
        protected override void Initialize(BaseGraphView graphView)
        {
            this.graphView = graphView;
            base.title = title;
            scrollable = true;

            graphView.onExposedParameterListChanged += UpdateParameterList;
            graphView.initialized += UpdateParameterList;
            Undo.undoRedoPerformed += UpdateParameterList;

            RegisterCallback<DragUpdatedEvent>(OnDragUpdatedEvent);//�϶���Ԫ�ؽ�����ܵķ���Ŀ��ʱ���͵��¼�����ExposedParameter���(����)�н���Ԫ�ص��϶����ƶ������б��е�λ��
            RegisterCallback<DragPerformEvent>(OnDragPerformEvent);//��һ��Ԫ�����Ϸ���һ��Ԫ��ʱ�����͵���Ԫ�ص��¼�������graph�е���ش�����Ϣ
            RegisterCallback<MouseDownEvent>(OnMouseDownEvent, TrickleDown.TrickleDown);//��갴�»ص�����������������Ԫ�صĲ��֣�λ�ã��ߴ磩�洢Ϊ�б�
            RegisterCallback<DetachFromPanelEvent>(OnViewClosed);//�Ӹ�������Ԫ��ǰ���͵��¼�

            UpdateParameterList();

            // Add exposed parameter button ���һ�� ExposedParameter�İ�ť�������Ͻǵġ�+����ť��
            header.Add(new Button(OnAddClicked)
            {
                text = "+"
            });
        }

        void OnViewClosed(DetachFromPanelEvent evt)
            => Undo.undoRedoPerformed -= UpdateParameterList;

        void OnMouseDownEvent(MouseDownEvent evt)
        {
            blackboardLayouts = content.Children().Select(c => c.layout).ToList();//��������������Ԫ�صĲ��֣�λ�ã��ߴ磩�洢Ϊ�б�
        }

        /// <summary>
        /// �����λ�û�ȡ��������
        /// </summary>
        /// <param name="pos">���λ��</param>
        /// <returns></returns>
        int GetInsertIndexFromMousePosition(Vector2 pos)
        {
            pos = content.WorldToLocal(pos);//ת��������������λ��
            // We only need to look for y axis; ֻ��Ҫ��y�����ֵ
            float mousePos = pos.y;

            if (mousePos < 0)
                return 0;

            int index = 0;
            foreach (var layout in blackboardLayouts)//����������������Ԫ�صĲ��֣�λ�ã��ߴ磩
            {
                if (mousePos > layout.yMin && mousePos < layout.yMax)//�ҳ���ǰ��������ĸ���Ԫ���ϣ����ҵ��ͷ��������б��е�����ֵ
                    return index + 1;
                index++;
            }

            return content.childCount;//��������Ԫ���ϣ��ͷ����б���������ֵ
        }

        void OnDragUpdatedEvent(DragUpdatedEvent evt)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Move;//������ָʾģʽ���ƶ��϶�����
            int newIndex = GetInsertIndexFromMousePosition(evt.mousePosition);//�����λ�û�ȡ��������
            var graphSelectionDragData = DragAndDrop.GetGenericData("DragSelection");//��ȡ��ǰ�϶��Ķ���

            if (graphSelectionDragData == null)//û���϶��Ķ����ֱ�ӷ���
                return;

            foreach (var obj in graphSelectionDragData as List<ISelectable>)//�����϶��Ķ���
            {
                if (obj is ExposedParameterFieldView view)//���϶�����ExposedParameterFieldView����
                {
                    var blackBoardRow = view.parent.parent.parent.parent.parent.parent;
                    int oldIndex = content.Children().ToList().FindIndex(c => c == blackBoardRow);
                    // Try to find the blackboard row
                    content.Remove(blackBoardRow);

                    if (newIndex > oldIndex)
                        newIndex--;

                    content.Insert(newIndex, blackBoardRow);
                }
            }
        }

        void OnDragPerformEvent(DragPerformEvent evt)
        {
            bool updateList = false;

            int newIndex = GetInsertIndexFromMousePosition(evt.mousePosition);
            foreach (var obj in DragAndDrop.GetGenericData("DragSelection") as List<ISelectable>)
            {
                if (obj is ExposedParameterFieldView view)
                {
                    if (!updateList)
                        graphView.RegisterCompleteObjectUndo("Moved parameters");

                    int oldIndex = graphView.graph.exposedParameters.FindIndex(e => e == view.parameter);
                    var parameter = graphView.graph.exposedParameters[oldIndex];
                    graphView.graph.exposedParameters.RemoveAt(oldIndex);

                    // Patch new index after the remove operation: ɾ���������޲�������
                    if (newIndex > oldIndex)
                        newIndex--;

                    graphView.graph.exposedParameters.Insert(newIndex, parameter);

                    updateList = true;
                }
            }

            if (updateList)
            {
                graphView.graph.NotifyExposedParameterListChanged();
                evt.StopImmediatePropagation();
                UpdateParameterList();
            }
        }
    }
}