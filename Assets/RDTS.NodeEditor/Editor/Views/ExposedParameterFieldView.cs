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
    /// ExposedParameter�ֶε���ͼ����ʾ���۵�����һ��Ԫ��
    /// </summary>
	public class ExposedParameterFieldView : BlackboardField//BlackboardField��һ����ʾͼ���ֶε� GraphElement
    {
        protected BaseGraphView graphView;

        public ExposedParameter parameter { get; private set; }

        public ExposedParameterFieldView(BaseGraphView graphView, ExposedParameter param) : base(null, param.name, param.shortType)
        {
            this.graphView = graphView;//���� graph
            parameter = param;//����ExposedParameter�����ں���ɾ��
            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));//��Ӳ˵�
            this.Q("icon").AddToClassList("parameter-" + param.shortType);
            this.Q("icon").visible = true;//��ʾ�ֶε�ͼ�꣨СԲ�㣩

            (this.Q("textField") as TextField).RegisterValueChangedCallback((e) => {
                param.name = e.newValue;
                text = e.newValue;
                graphView.graph.UpdateExposedParameterName(param, e.newValue);//��������
            });
        }

        /// <summary>�ֶβ˵�</summary>
		void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            ///OpenTextEditor���� TextField �Ա༭ BlackboardField �е��ı�
            ///AlwaysEnabled��ʼ�շ��� Status.Enabled ��״̬�ص�
            //�ֶ�������
            evt.menu.AppendAction("Rename", (a) => OpenTextEditor(), DropdownMenuAction.AlwaysEnabled);
            //�Ƴ�һ��ExposedParameter
            evt.menu.AppendAction("Delete", (a) => graphView.graph.RemoveExposedParameter(parameter), DropdownMenuAction.AlwaysEnabled);

            evt.StopPropagation();
        }
    }
}