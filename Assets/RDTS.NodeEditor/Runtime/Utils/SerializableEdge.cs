using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// 连线的信息
    /// </summary>
	[System.Serializable]
    public class SerializableEdge : ISerializationCallbackReceiver
    {
        public string GUID;

        [SerializeField]
        BaseGraph owner;

        [SerializeField]
        string inputNodeGUID;
        [SerializeField]
        string outputNodeGUID;

        [System.NonSerialized]
        public BaseNode inputNode;

        [System.NonSerialized]
        public NodePort inputPort;
        [System.NonSerialized]
        public NodePort outputPort;

        //temporary object used to send port to port data when a custom input/output function is used.
        //使用自定义输入/输出功能时，用于将端口发送到端口数据的临时对象
        [System.NonSerialized]
        public object passThroughBuffer;

        [System.NonSerialized]
        public BaseNode outputNode;

        public string inputFieldName;
        public string outputFieldName;

        // Use to store the id of the field that generate multiple ports 用于存储生成多个端口的字段的id
        public string inputPortIdentifier;
        public string outputPortIdentifier;

        public SerializableEdge() { }

        /// <summary>
        /// 创建一条新的连线（赋予其连线的相关信息）
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="inputPort"></param>
        /// <param name="outputPort"></param>
        /// <returns></returns>
		public static SerializableEdge CreateNewEdge(BaseGraph graph, NodePort inputPort, NodePort outputPort)
        {
            SerializableEdge edge = new SerializableEdge();

            edge.owner = graph;
            edge.GUID = System.Guid.NewGuid().ToString();
            edge.inputNode = inputPort.owner;
            edge.inputFieldName = inputPort.fieldName;
            edge.outputNode = outputPort.owner;
            edge.outputFieldName = outputPort.fieldName;
            edge.inputPort = inputPort;
            edge.outputPort = outputPort;
            edge.inputPortIdentifier = inputPort.portData.identifier;
            edge.outputPortIdentifier = outputPort.portData.identifier;

            return edge;
        }

        /// <summary>
        /// 实现继承的接口类的函数
        /// 实现该方法，以便在 Unity 序列化您的对象前接收回调
        /// 
        /// </summary>
		public void OnBeforeSerialize()
        {
            if (outputNode == null || inputNode == null)
                return;

            outputNodeGUID = outputNode.GUID;
            inputNodeGUID = inputNode.GUID;
        }

        /// <summary>
        /// 实现继承的接口类的函数
        /// 实现该方法，以便在 Unity 反序列化您的对象后接收回调
        /// </summary>
        public void OnAfterDeserialize() { }

        //here our owner have been deserialized 这里我们的BaseGraph已经被反序列化了
        public void Deserialize()
        {
            if (!owner.nodesPerGUID.ContainsKey(outputNodeGUID) || !owner.nodesPerGUID.ContainsKey(inputNodeGUID))
                return;

            outputNode = owner.nodesPerGUID[outputNodeGUID];
            inputNode = owner.nodesPerGUID[inputNodeGUID];
            inputPort = inputNode.GetPort(inputFieldName, inputPortIdentifier);//根据inputFieldName和inputPortIdentifier来获取相应的端口
            outputPort = outputNode.GetPort(outputFieldName, outputPortIdentifier);//根据outputFieldName和outputPortIdentifier来获取相应的端口
        }

        public override string ToString() => $"{outputNode.name}:{outputPort.fieldName} -> {inputNode.name}:{inputPort.fieldName}";
    }
}
