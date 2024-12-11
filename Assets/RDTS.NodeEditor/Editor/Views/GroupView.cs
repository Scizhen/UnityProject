using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// GroupԪ����λ��ơ�����
    /// </summary>
    public class GroupView : UnityEditor.Experimental.GraphView.Group
    {
        public BaseGraphView owner;//������GrapgView
        public Group group;//��Ӧ��group

        Label titleLabel;//����
        ColorField colorField;//��ɫ�ֶ�

        readonly string groupStyle = "GraphStyles/GroupView";

        public GroupView()
        {
            styleSheets.Add(Resources.Load<StyleSheet>(groupStyle));//��ʽ
        }

        ///ContextualMenuPopulateEvent���������Ĳ˵���Ҫ�˵���ʱ���͵��¼�
        private static void BuildContextualMenu(ContextualMenuPopulateEvent evt) { }

        /// <summary>
        /// ��ʼ��GroupView������Group��BaseGraphView�����ñ��⡢λ�ã������ɫ�ֶβ�ע��ص�����ʼ�ڲ��ڵ�
        /// </summary>
        /// <param name="graphView"></param>
        /// <param name="block"></param>
        public void Initialize(BaseGraphView graphView, Group block)
        {
            group = block;
            owner = graphView;

            title = block.title;
            SetPosition(block.position);//����λ��

            //ContextualMenuManipulator���������û���������Ҽ����¼����ϵĲ˵���ʱ��ʾ�����Ĳ˵��Ĳٿ���
            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));

            headerContainer.Q<TextField>().RegisterCallback<ChangeEvent<string>>(TitleChangedCallback);
            titleLabel = headerContainer.Q<Label>();

            colorField = new ColorField { value = group.color, name = "headerColorPicker" };
            colorField.RegisterValueChangedCallback(e =>//ע��ص������ڸı�color�ֶ���ɫʱ����group����ɫ
            {
                UpdateGroupColor(e.newValue);
            });
            UpdateGroupColor(group.color);

            ///Scope����������ڽ��ڵ���鵽�������򣬻��Ϊ������ (Scope)����������Զ�����������д�С�����Ͷ�λ�����ɽڵ���ķ���
            ///headerContainer������������ı�ͷ������ʹ�ô˱�ͷ���������Զ���������ı�ͷ
            headerContainer.Add(colorField);//�ڱ�ͷ�������ɫ�ֶ�

            InitializeInnerNodes();//��ʼ���ڲ��ڵ�
        }

        /// <summary>
        /// ��ʼ��group�ڲ��ڵ㣺�Ƴ������ڵĽڵ㣬��Ӵ��ڵĽڵ�
        /// </summary>
        void InitializeInnerNodes()
        {
            foreach (var nodeGUID in group.innerNodeGUIDs.ToList())//������group�д���Ľڵ�
            {
                if (!owner.graph.nodesPerGUID.ContainsKey(nodeGUID))
                {
                    Debug.LogWarning("Node GUID not found: " + nodeGUID);
                    group.innerNodeGUIDs.Remove(nodeGUID);
                    continue;
                }
                var node = owner.graph.nodesPerGUID[nodeGUID];
                var nodeView = owner.nodeViewsPerNode[node];

                AddElement(nodeView);//��Ӵ˽ڵ㵽group��
            }
        }

        /// <summary>
        /// ��Ԫ����ӵ���groupʱ���ã���BaseNodeView���͵�Ԫ����ӵ�group��
        /// </summary>
        /// <param name="elements">��ӵ�Ԫ��</param>
        protected override void OnElementsAdded(IEnumerable<GraphElement> elements)
        {
            foreach (var element in elements)
            {
                var node = element as BaseNodeView;

                // Adding an element that is not a node currently supported ��Ӳ��ǵ�ǰ֧�ֵĽڵ��Ԫ�أ�group��û�е�BaseNodeViewԪ�أ�
                if (node == null)
                    continue;

                if (!group.innerNodeGUIDs.Contains(node.nodeTarget.GUID))
                    group.innerNodeGUIDs.Add(node.nodeTarget.GUID);
            }
            base.OnElementsAdded(elements);
        }

        /// <summary>
        /// ���Ӵ�����ɾ��Ԫ��ʱ���ã�ɾ��group�еĽڵ�
        /// </summary>
        /// <param name="elements">ɾ����Ԫ��</param>
        protected override void OnElementsRemoved(IEnumerable<GraphElement> elements)
        {
            // Only remove the nodes when the group exists in the hierarchy 
            // ����group�����ڲ�νṹ��ʱ��ɾ���ڵ�
            if (parent != null)
            {
                foreach (var elem in elements)
                {
                    if (elem is BaseNodeView nodeView)
                    {
                        group.innerNodeGUIDs.Remove(nodeView.nodeTarget.GUID);
                    }
                }
            }

            base.OnElementsRemoved(elements);
        }

        /// <summary>����group����ɫ</summary>
        public void UpdateGroupColor(Color newColor)
        {
            group.color = newColor;
            style.backgroundColor = newColor;
        }

        /// <summary>����group�ı���</summary>
        void TitleChangedCallback(ChangeEvent<string> e)
        {
            group.title = e.newValue;
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            group.position = newPos;
        }
    }
}