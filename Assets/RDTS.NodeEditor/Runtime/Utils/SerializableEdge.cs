using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// ���ߵ���Ϣ
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
        //ʹ���Զ�������/�������ʱ�����ڽ��˿ڷ��͵��˿����ݵ���ʱ����
        [System.NonSerialized]
        public object passThroughBuffer;

        [System.NonSerialized]
        public BaseNode outputNode;

        public string inputFieldName;
        public string outputFieldName;

        // Use to store the id of the field that generate multiple ports ���ڴ洢���ɶ���˿ڵ��ֶε�id
        public string inputPortIdentifier;
        public string outputPortIdentifier;

        public SerializableEdge() { }

        /// <summary>
        /// ����һ���µ����ߣ����������ߵ������Ϣ��
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
        /// ʵ�ּ̳еĽӿ���ĺ���
        /// ʵ�ָ÷������Ա��� Unity ���л����Ķ���ǰ���ջص�
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
        /// ʵ�ּ̳еĽӿ���ĺ���
        /// ʵ�ָ÷������Ա��� Unity �����л����Ķ������ջص�
        /// </summary>
        public void OnAfterDeserialize() { }

        //here our owner have been deserialized �������ǵ�BaseGraph�Ѿ��������л���
        public void Deserialize()
        {
            if (!owner.nodesPerGUID.ContainsKey(outputNodeGUID) || !owner.nodesPerGUID.ContainsKey(inputNodeGUID))
                return;

            outputNode = owner.nodesPerGUID[outputNodeGUID];
            inputNode = owner.nodesPerGUID[inputNodeGUID];
            inputPort = inputNode.GetPort(inputFieldName, inputPortIdentifier);//����inputFieldName��inputPortIdentifier����ȡ��Ӧ�Ķ˿�
            outputPort = outputNode.GetPort(outputFieldName, outputPortIdentifier);//����outputFieldName��outputPortIdentifier����ȡ��Ӧ�Ķ˿�
        }

        public override string ToString() => $"{outputNode.name}:{outputPort.fieldName} -> {inputNode.name}:{inputPort.fieldName}";
    }
}
