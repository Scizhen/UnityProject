using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// 对应从黑板元素中拖动ExposedParameter元素至graph中产生新的节点
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
        // 我们序列化图中公开参数的 GUID，以便我们可以从图中检索真正的 ExposedParameter
        [SerializeField, HideInInspector]
        public string parameterGUID;//在BaseGraphView的DragPerformedCallback回调方法中被赋值

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
        /// 根据parameterGUID加载对应的Exposed Parameter
        /// </summary>
		void LoadExposedParameter()
        {
            parameter = graph.GetExposedParameterFromGUID(parameterGUID);//按照指定的GUID获取一个ExposedParameter

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

        //通过在BaseNode的InitializeInOutDatas()方法中检索CustomPortBehaviorAttribute特性来获取此方法，经过处理后，在选择不同的ParameterAccessor枚举值时，进行对应操作：
        //    ParameterAccessor.Get：显示输出端口，删除输入端口
        //    ParameterAccessor.Set：显示输入端口，删除输出端口
        [CustomPortBehavior(nameof(output))]
        IEnumerable<PortData> GetOutputPort(List<SerializableEdge> edges)
        {
            if (accessor == ParameterAccessor.Get)
            {
                /// Debug.Log("ParameterAccessor.Get");//在切换ParameterAccessor枚举值时被调用

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
                output = parameter.value;//设置要输出的值
            else
                graph.UpdateExposedParameter(parameter.guid, input);//更新输入进来的值
        }
    }

    public enum ParameterAccessor
    {
        Get,
        Set
    }
}
