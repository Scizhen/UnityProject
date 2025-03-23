using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// ��Ӧ�Ӻڰ�Ԫ�����϶�ExposedParameterԪ����graph�в����µĽڵ�
    /// </summary>
	[System.Serializable]
    public class ParameterNode : BaseNode
    {
        [Input]
        public object input;

        [Output]
        public object output;

        public override string name => "Parameter";

        // We serialize the GUID of the exposed parameter in the graph so we can retrieve the true ExposedParameter from the graph
        // �������л�ͼ�й��������� GUID���Ա����ǿ��Դ�ͼ�м��������� ExposedParameter
        [SerializeField, HideInInspector]
        public string parameterGUID;//��BaseGraphView��DragPerformedCallback�ص������б���ֵ

        public ExposedParameter parameter { get; private set; }

        public event Action onParameterChanged;

        public ParameterAccessor accessor;

        protected override void Enable()
        {
            // load the parameter
            LoadExposedParameter();

            graph.onExposedParameterModified += OnParamChanged;
            if (onParameterChanged != null)
                onParameterChanged?.Invoke();
        }

        /// <summary>
        /// ����parameterGUID���ض�Ӧ��Exposed Parameter
        /// </summary>
		void LoadExposedParameter()
        {
            parameter = graph.GetExposedParameterFromGUID(parameterGUID);//����ָ����GUID��ȡһ��ExposedParameter

            if (parameter == null)
            {
                Debug.Log("Property \"" + parameterGUID + "\" Can't be found !");

                // Delete this node as the property can't be found
                graph.RemoveNode(this);
                return;
            }

            output = parameter.value;
        }

        void OnParamChanged(ExposedParameter modifiedParam)
        {
            if (parameter == modifiedParam)
            {
                onParameterChanged?.Invoke();
            }
        }

        //ͨ����BaseNode��InitializeInOutDatas()�����м���CustomPortBehaviorAttribute��������ȡ�˷����������������ѡ��ͬ��ParameterAccessorö��ֵʱ�����ж�Ӧ������
        //    ParameterAccessor.Get����ʾ����˿ڣ�ɾ������˿�
        //    ParameterAccessor.Set����ʾ����˿ڣ�ɾ������˿�
        [CustomPortBehavior(nameof(output))]
        IEnumerable<PortData> GetOutputPort(List<SerializableEdge> edges)
        {
            if (accessor == ParameterAccessor.Get)
            {
                /// Debug.Log("ParameterAccessor.Get");//���л�ParameterAccessorö��ֵʱ������

                yield return new PortData
                {
                    identifier = "output",
                    displayName = "Value",
                    displayType = (parameter == null) ? typeof(object) : parameter.GetValueType(),
                    acceptMultipleEdges = true
                };
            }
        }

        [CustomPortBehavior(nameof(input))]
        IEnumerable<PortData> GetInputPort(List<SerializableEdge> edges)
        {
            if (accessor == ParameterAccessor.Set)
            {
                yield return new PortData
                {
                    identifier = "input",
                    displayName = "Value",
                    displayType = (parameter == null) ? typeof(object) : parameter.GetValueType(),
                };
            }
        }

        protected override void Process()
        {
#if UNITY_EDITOR // In the editor, an undo/redo can change the parameter instance in the graph, in this case the field in this class will point to the wrong parameter
            parameter = graph.GetExposedParameterFromGUID(parameterGUID);
#endif

            ClearMessages();
            if (parameter == null)
            {
                AddMessage($"Parameter not found: {parameterGUID}", NodeMessageType.Error);
                return;
            }

            if (accessor == ParameterAccessor.Get)
                output = parameter.value;//����Ҫ�����ֵ
            else
                graph.UpdateExposedParameter(parameter.guid, input);//�������������ֵ
        }
    }

    public enum ParameterAccessor
    {
        Get,
        Set
    }
}
