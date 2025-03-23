using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// Custom editor of the node inspector, you can inherit from this class to customize your node inspector.
    /// �ڵ���������Զ���༭���������ԴӸ���̳����Զ������Ľڵ������塣
    /// </summary>
    [CustomEditor(typeof(NodeInspectorObject))]
    public class NodeInspectorObjectEditor : Editor
    {
        NodeInspectorObject inspector;
        protected VisualElement root;
        protected VisualElement selectedNodeList;
        protected VisualElement placeholder;//��ǩԪ��

        protected virtual void OnEnable()
        {
            inspector = target as NodeInspectorObject;
            inspector.nodeSelectionUpdated += UpdateNodeInspectorList;//��ѡ�����ʱ����
            root = new VisualElement();
            selectedNodeList = new VisualElement();
            selectedNodeList.styleSheets.Add(Resources.Load<StyleSheet>("GraphStyles/InspectorView"));
            root.Add(selectedNodeList);
            placeholder = new Label("Select a node to show it's settings in the inspector");
            placeholder.AddToClassList("PlaceHolder");
            UpdateNodeInspectorList();
        }

        protected virtual void OnDisable()
        {
            inspector.nodeSelectionUpdated -= UpdateNodeInspectorList;
        }

        /// <summary>��ʾ�ڵ���Զ���Inspector</summary>
        public override VisualElement CreateInspectorGUI() => root;

        protected virtual void UpdateNodeInspectorList()
        {
            selectedNodeList.Clear();

            if (inspector.selectedNodes.Count == 0)
                selectedNodeList.Add(placeholder);

            foreach (var nodeView in inspector.selectedNodes)
                selectedNodeList.Add(CreateNodeBlock(nodeView));
        }

        protected VisualElement CreateNodeBlock(BaseNodeView nodeView)
        {
            var view = new VisualElement();

            view.Add(new Label(nodeView.nodeTarget.name));//��ӽڵ����Ƶı�ǩԪ�أ���Inspector��

            var tmp = nodeView.controlsContainer;
            nodeView.controlsContainer = view;
            nodeView.Enable(true);
            nodeView.controlsContainer.AddToClassList("NodeControls");
            var block = nodeView.controlsContainer;//��ӽڵ��controlsContainer�е�Ԫ�أ���Inspector��
            nodeView.controlsContainer = tmp;

            return block;
        }
    }

    /// <summary>
    /// Node inspector object, you can inherit from this class to customize your node inspector.
    /// �ڵ�������������Դ������̳����Զ�����Ľڵ�������
    /// </summary>
    public class NodeInspectorObject : ScriptableObject
    {
        /// <summary>Previously selected object by the inspector �������ǰѡ��Ķ���</summary>
        public Object previouslySelectedObject;
        /// <summary>List of currently selected nodes ��ǰ��ѡ�е�Ҫ��ʾ�ڼ������Ľڵ�</summary>
        public HashSet<BaseNodeView> selectedNodes { get; private set; } = new HashSet<BaseNodeView>();

        /// <summary>Triggered when the selection is updated ��ѡ�����ʱ����</summary>
        public event Action nodeSelectionUpdated;

        /// <summary>Updates the selection from the graph ����ͼ���е�ѡ��ڵ���б�</summary>
        public virtual void UpdateSelectedNodes(HashSet<BaseNodeView> views)
        {
            selectedNodes = views;
            nodeSelectionUpdated?.Invoke();
        }

        public virtual void RefreshNodes() => nodeSelectionUpdated?.Invoke();

        /// <summary>�Ƴ�һ���ڵ���ͼ</summary>
        public virtual void NodeViewRemoved(BaseNodeView view)
        {
            selectedNodes.Remove(view);
            nodeSelectionUpdated?.Invoke();
        }
    }
}