using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System.Linq;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// ExposedParameterԪ�صĽڵ����ͼ��ʵ��ExposedParameter������graph�еĻ�����ͼ
    /// </summary>
    [NodeCustomEditor(typeof(ParameterNode))]
    public class ParameterNodeView : BaseNodeView
    {
        ParameterNode parameterNode;

        /// <summary>
        /// ������Ӧ��ParameterNode�����һ��ParameterAccessor��ö��Ԫ�أ����Ҳ�ɾ��չ��/�۵���ť������parameterNode�ı���
        /// </summary>
        /// <param name="fromInspector"></param>
        public override void Enable(bool fromInspector = false)
        {
            parameterNode = nodeTarget as ParameterNode;

            ///EnumField:����һ��������ö��ֵ֮���л��������б�
            EnumField accessorSelector = new EnumField(parameterNode.accessor);
            accessorSelector.SetValueWithoutNotify(parameterNode.accessor);
            //ע��ص�������ö��ֵ��ˢ�¶˿ڣ��ػ�Ԫ��
            accessorSelector.RegisterValueChangedCallback(evt =>
            {
                parameterNode.accessor = (ParameterAccessor)evt.newValue;
                UpdatePort();
                controlsContainer.MarkDirtyRepaint();
                ForceUpdatePorts();
            });

            UpdatePort();
            //���ö������Ԫ��
            controlsContainer.Add(accessorSelector);

            //    Find and remove expand/collapse button ���Ҳ�ɾ��չ��/�۵���ť
            titleContainer.Remove(titleContainer.Q("title-button-container"));
            //    Remove Port from the #content �Ƴ�������������
            // topContainer.parent.Remove(topContainer);
            //    Add Port to the #title  ���˿���ӵ�����������
            /// topContainer��������������������������������
            //titleContainer.Add(topContainer);

            parameterNode.onParameterChanged += UpdateView;
            UpdateView();
        }

        /// <summary>����parameterNode�ı��⣺exposed parameter��������Ϊ�ڵ����</summary>
        void UpdateView()
        {
            title = parameterNode.parameter?.name;
        }


        void UpdatePort()
        {
            if (parameterNode.accessor == ParameterAccessor.Set)
            {
                titleContainer.AddToClassList("input");
            }
            else
            {
                titleContainer.RemoveFromClassList("input");
            }
        }
    }

}