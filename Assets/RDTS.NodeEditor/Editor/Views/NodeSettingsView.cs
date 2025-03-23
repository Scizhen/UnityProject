using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine;

namespace RDTS.NodeEditor
{
    class NodeSettingsView : VisualElement
    {
        VisualElement m_ContentContainer;

        public NodeSettingsView()
        {
            pickingMode = PickingMode.Ignore;
            styleSheets.Add(Resources.Load<StyleSheet>("GraphStyles/NodeSettings"));
            var uxml = Resources.Load<VisualTreeAsset>("GraphUXML/NodeSettings");
            uxml.CloneTree(this);

            // Get the element we want to use as content container ��ȡ������Ҫ��������������Ԫ��
            m_ContentContainer = this.Q("contentContainer");
            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        void OnMouseUp(MouseUpEvent evt)
        {
            evt.StopPropagation();
        }

        void OnMouseDown(MouseDownEvent evt)
        {
            evt.StopPropagation();
        }

        public override VisualElement contentContainer
        {
            get { return m_ContentContainer; }
        }
    }
}