// #define DEBUG_LAMBDA

using System.Linq;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Reflection;
using System.Linq.Expressions;
using System;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// Class that describe port attributes for it's creation
    /// �����䴴���Ķ˿����Ե��ࣨ�˿���Ϣ��
    /// </summary>
    public class PortData : IEquatable<PortData>
    {
        /// <summary>
        /// Unique identifier for the port �˿ڵ�Ψһ��ʶ��
        /// </summary>
        public string identifier;
        /// <summary>
        /// Display name on the node �ڽڵ�����ʾ����
        /// </summary>
        public string displayName;
        /// <summary>
        /// The type that will be used for coloring with the type stylesheet
        /// ������ʹ��������ʽ����ɫ������
        /// </summary>
        public Type displayType;
        /// <summary>
        /// If the port accept multiple connection �˶˿��Ƿ���ն������
        /// </summary>
        public bool acceptMultipleEdges;
        /// <summary>
        /// Port size, will also affect the size of the connected edge
        /// �˿ڳߴ磬Ҳ����Ӱ�������ߵĳߴ�
        /// </summary>
        public int sizeInPixel;
        /// <summary>
        /// Tooltip of the port �˿ڵĹ�����ʾ
        /// </summary> 
        public string tooltip;
        /// <summary>
        /// Is the port vertical �Ƿ�ֱ��ʾ�˿�
        /// </summary>
        public bool vertical;

        /// <summary>
        /// �Ƿ�����һ��PortData��ͬ�����򷵻�true
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(PortData other)
        {
            return identifier == other.identifier
                && displayName == other.displayName
                && displayType == other.displayType
                && acceptMultipleEdges == other.acceptMultipleEdges
                && sizeInPixel == other.sizeInPixel
                && tooltip == other.tooltip
                && vertical == other.vertical;
        }
        /// <summary>
        /// ������һ��PortData�ڴ�
        /// </summary>
        /// <param name="other"></param>
		public void CopyFrom(PortData other)
        {
            identifier = other.identifier;
            displayName = other.displayName;
            displayType = other.displayType;
            acceptMultipleEdges = other.acceptMultipleEdges;
            sizeInPixel = other.sizeInPixel;
            tooltip = other.tooltip;
            vertical = other.vertical;
        }
    }

    /// <summary>
    /// Runtime class that stores all info about one port that is needed for the processing
    /// �洢һ���˿ڴ���ʱ�����������Ϣ����
    /// </summary>
    public class NodePort
    {
        /// <summary>
        /// The actual name of the property behind the port (must be exact, it is used for Reflection)
        /// �˿ں�������Ե�ʵ�����ƣ�����׼ȷ�����ڷ��䣩
        /// </summary>
        public string fieldName;
        /// <summary>
        /// The node on which the port is �˿����ڵĽڵ�
        /// </summary>
        public BaseNode owner;
        /// <summary>
        /// The fieldInfo from the fieldName   fieldName�е�fieldInfo
        /// FieldInfo�ࣺ�����ֶε����Բ��ṩ���ֶ�Ԫ���ݵķ���Ȩ��
        /// </summary>
        public FieldInfo fieldInfo;//FieldInfo�������ֶε����Բ��ṩ���ֶ�Ԫ���ݵķ���Ȩ��

        /// <summary>
        /// Data of the port �˿�����
        /// </summary>
        public PortData portData;
        List<SerializableEdge> edges = new List<SerializableEdge>();//���ߵ��б�
        Dictionary<SerializableEdge, PushDataDelegate> pushDataDelegates = new Dictionary<SerializableEdge, PushDataDelegate>();
        List<SerializableEdge> edgeWithRemoteCustomIO = new List<SerializableEdge>();//��һ���Զ���IO�ı�

        /// <summary>
        /// Owner of the FieldInfo, to be used in case of Get/SetValue
        /// FieldInfo �������ߣ��� Get/SetValue �������ʹ��
        /// </summary>
        public object fieldOwner;//ӵ�и��ֶεĽڵ�

        CustomPortIODelegate customPortIOMethod;

        /// <summary>
        /// Delegate that is made to send the data from this port to another port connected through an edge
        /// �����ݴӸö˿ڷ��͵�ͨ�������ӵ���һ���˿ڵ�ί��
        /// This is an optimization compared to dynamically setting values using Reflection (which is really slow)
        /// ��ʹ�÷��䶯̬����ֵ��ȣ�����һ���Ż�������ĺ�����
        /// More info: https://codeblog.jonskeet.uk/2008/08/09/making-reflection-fly-and-exploring-delegates/
        /// </summary>
        public delegate void PushDataDelegate();

        /// <summary>
        /// Constructor ���캯��
        /// </summary>
        /// <param name="owner">owner node</param>
        /// <param name="fieldName">the C# property name</param>
        /// <param name="portData">Data of the port</param>
        public NodePort(BaseNode owner, string fieldName, PortData portData) : this(owner, owner, fieldName, portData)
        {
            Debug.Log($"fieldName: {this.fieldName} + fieldInfo: {this.fieldInfo.ToString()} + fieldInfoValue: {this.fieldInfo.GetValue(owner)?.ToString()}");


        }

        /// <summary>
        /// Constructor ���캯��
        /// </summary>
        /// <param name="owner">owner node</param>
        /// <param name="fieldOwner"></param>
        /// <param name="fieldName">the C# property name</param>
        /// <param name="portData">Data of the port</param>
        public NodePort(BaseNode owner, object fieldOwner, string fieldName, PortData portData)
        {
            this.fieldName = fieldName;
            this.owner = owner;
            this.portData = portData;
            this.fieldOwner = fieldOwner;

            fieldInfo = fieldOwner.GetType().GetField(//ʹ��ָ����Լ������ָ���ֶΣ���ʾ����ָ��Ҫ����ֶεĶ�������ҵ��Ļ���������Ϊ null
                fieldName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            customPortIOMethod = CustomPortIO.GetCustomPortMethod(owner.GetType(), fieldName);//����ָ���Ľڵ����ͺ�(�˿�)�ֶ�����ȡ��Ӧ�ľ��С��˿����/�������ԡ��ķ�����ί��
        }

        /// <summary>
        /// Connect an edge to this port  ���ߵ��˶˿�
        /// </summary>
        /// <param name="edge"></param>
        public void Add(SerializableEdge edge)
        {
            if (!edges.Contains(edge))//����������edge��������б�
                edges.Add(edge);

            if (edge.inputNode == owner)//����edge������ڵ��ǵ�ǰ�˿����ڽڵ�
            {
                if (edge.outputPort.customPortIOMethod != null)//����edge������˿ڵ��Զ���˿�IO��������
                    edgeWithRemoteCustomIO.Add(edge);
            }
            else//����edge������ڵ㲻�ǵ�ǰ�˿����ڽڵ㣨���Ӧ����ڵ㣩
            {
                if (edge.inputPort.customPortIOMethod != null)//����edge������˿ڵ��Զ���˿�IO��������
                    edgeWithRemoteCustomIO.Add(edge);
            }

            //if we have a custom io implementation, we don't need to genereate the defaut one
            //����������Զ���� io ʵ�֣����ǲ���Ҫ����Ĭ�ϵ�
            if (edge.inputPort.customPortIOMethod != null || edge.outputPort.customPortIOMethod != null)
                return;

            PushDataDelegate edgeDelegate = CreatePushDataDelegateForEdge(edge);

            if (edgeDelegate != null)
                pushDataDelegates[edge] = edgeDelegate;
        }

        /// <summary>
        /// �����ݴӸö˿ڷ��͵�ͨ�������ӵ���һ���˿ڵ�ί��
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
		PushDataDelegate CreatePushDataDelegateForEdge(SerializableEdge edge)
        {
            try
            {
                //Creation of the delegate to move the data from the input node to the output node:
                //����ί���Խ����ݴ�����ڵ��ƶ�������ڵ㣺
                FieldInfo inputField = edge.inputNode.GetType().GetField(edge.inputFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo outputField = edge.outputNode.GetType().GetField(edge.outputFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                Type inType, outType;

#if DEBUG_LAMBDA
				return new PushDataDelegate(() => {
					var outValue = outputField.GetValue(edge.outputNode);
					inType = edge.inputPort.portData.displayType ?? inputField.FieldType;
					outType = edge.outputPort.portData.displayType ?? outputField.FieldType;
					Debug.Log($"Push: {inType}({outValue}) -> {outType} | {owner.name}");

					object convertedValue = outValue;
					if (TypeAdapter.AreAssignable(outType, inType))
					{
						var convertionMethod = TypeAdapter.GetConvertionMethod(outType, inType);
						Debug.Log("Convertion method: " + convertionMethod.Name);
						convertedValue = convertionMethod.Invoke(null, new object[]{ outValue });
					}

					inputField.SetValue(edge.inputNode, convertedValue);
				});
#endif

                // We keep slow checks inside the editor
#if UNITY_EDITOR
                if (!BaseGraph.TypesAreConnectable(inputField.FieldType, outputField.FieldType))//���������Ƿ��ܽ�������
                {
                    Debug.LogError("Can't convert from " + inputField.FieldType + " to " + outputField.FieldType + ", you must specify a custom port function (i.e CustomPortInput or CustomPortOutput) for non-implicit convertions");
                    return null;
                }
#endif
                ///Expression���ṩһ�ֻ��࣬��ʾ���ʽ���ڵ���������Ըû���
				Expression inputParamField = Expression.Field(Expression.Constant(edge.inputNode), inputField);//��edge.inputNode���е�inputField�ֶΣ����е�ĳ��������
                Expression outputParamField = Expression.Field(Expression.Constant(edge.outputNode), outputField);

                inType = edge.inputPort.portData.displayType ?? inputField.FieldType;//����˿ڵ��ֶ�����
                outType = edge.outputPort.portData.displayType ?? outputField.FieldType;//����˿ڵ��ֶ�����

                // If there is a user defined convertion function, then we call it  ������Զ����ת����������ô������
                if (TypeAdapter.AreAssignable(outType, inType))//�����Ƿ�ɷ���
                {
                    // We add a cast in case there we're calling the conversion method with a base class parameter (like object)
                    // ���һ��ǿ��ת�����Է�ʹ�û������������󣩵���ת������
                    var convertedParam = Expression.Convert(outputParamField, outType);//��outputParamFieldת����outType����
                    ///Call����ʾ��ʹ��һ��������static�����ĵ���
                    outputParamField = Expression.Call(TypeAdapter.GetConvertionMethod(outType, inType), convertedParam);
                    // In case there is a custom port behavior in the output, then we need to re-cast to the base type because
                    // the convertion method return type is not always assignable directly:
                    // �����������Զ���˿���Ϊ����ô������Ҫ����ת��Ϊ�������ͣ���Ϊת�������������Ͳ������ǿ���ֱ�Ӹ�ֵ
                    outputParamField = Expression.Convert(outputParamField, inputField.FieldType);
                }
                else // otherwise we cast
                    outputParamField = Expression.Convert(outputParamField, inputField.FieldType);

                ///BinaryExpression����ʾ���ж�����������ı��ʽ
				BinaryExpression assign = Expression.Assign(inputParamField, outputParamField);//Assign��ֵ���㣺assignΪ��inputParamField = outputParamField��
                ///Compile����lambda���ʽ�����ί��
                return Expression.Lambda<PushDataDelegate>(assign).Compile();//���أ�inputParamField = outputParamField��ί��
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return null;
            }
        }

        /// <summary>
        /// Disconnect an Edge from this port  �Ӷ˿��Ƴ���edge
        /// </summary>
        /// <param name="edge"></param>
        public void Remove(SerializableEdge edge)
        {
            if (!edges.Contains(edge))
                return;

            pushDataDelegates.Remove(edge);
            edgeWithRemoteCustomIO.Remove(edge);
            edges.Remove(edge);
        }

        /// <summary>
        /// edge�ӿڣ� Get all the edges connected to this port ��ȡ�˶˿ڵ�����edge
        /// </summary>
        /// <returns></returns>
        public List<SerializableEdge> GetEdges() => edges;

        /// <summary>
        /// Push the value of the port through the edges ͨ����Ե���Ͷ˿ڵ�ֵ
        /// This method can only be called on output ports �˷���ֻ��������˿ڱ�����
        /// </summary>
        public void PushData()
        {
            /* �����Զ���IO�����ݴ��� */
            if (customPortIOMethod != null)
            {
                customPortIOMethod(owner, edges, this);
                return;
            }

            /* ���ڷ��Զ���IO�����ݴ��� */
            foreach (var pushDataDelegate in pushDataDelegates)//��������PushData��ί�з��������е���
                pushDataDelegate.Value();

            /* �����Զ���IO�����ݴ��� */
            if (edgeWithRemoteCustomIO.Count == 0)////����һ�˿ڲ�����������������ݴ���
                return;
            //if there are custom IO implementation on the other ports, they'll need our value in the passThrough buffer
            //��������˿������Զ��� IO ʵ�֣����ǽ���Ҫ������ passThrough �������е�ֵ
            object ourValue = fieldInfo.GetValue(fieldOwner);//GetValue����������������дʱ�����ظ�������(fieldOwner)֧�ֵ��ֶ�(fieldInfo)��ֵ
            foreach (var edge in edgeWithRemoteCustomIO)
                edge.passThroughBuffer = ourValue;//��ֵ���뻺����
        }

        /// <summary>
        /// Reset the value of the field to default if possible
        /// ������ܣ����ֶε�ֵ����ΪĬ��ֵ
        /// </summary>
        public void ResetToDefault()
        {
            // Clear lists, set classes to null and struct to default value.
            if (typeof(IList).IsAssignableFrom(fieldInfo.FieldType))//IsAssignableFrom��ȷ��ָ������(c)��ʵ���Ƿ��ܷ������ǰ���͵ı�����FieldType����ȡ���ֶζ��������
            {
                (fieldInfo.GetValue(fieldOwner) as IList)?.Clear();
                Debug.Log(fieldInfo.GetValue(fieldOwner).ToString());
            }

            else if (fieldInfo.FieldType.GetTypeInfo().IsClass)
                fieldInfo.SetValue(fieldOwner, null);
            else
            {
                try
                {
                    fieldInfo.SetValue(fieldOwner, Activator.CreateInstance(fieldInfo.FieldType));
                }
                catch { } // Catch types that don't have any constructors
            }
        }

        /// <summary>
        /// Pull values from the edge (in case of a custom convertion method) �ӱ�Ե��ȡֵ�����Զ���ת������������£�
        /// This method can only be called on input ports �˷���ֻ�ܱ�����˿ڵ���
        /// </summary>
        public void PullData()
        {
            /* �����Զ���IO�����ݴ��� */
            if (customPortIOMethod != null)
            {
                customPortIOMethod(owner, edges, this);
                return;
            }
            // check if this port have connection to ports that have custom output functions
            //���˶˿��Ƿ����ӵ������Զ���������ܵĶ˿�
            if (edgeWithRemoteCustomIO.Count == 0)//����һ�˿ڲ�����������������ݴ���
                return;

            /* �����Զ���IO�����ݴ��� */
            // Only one input connection is handled by this code, if you want to
            // take multiple inputs, you must create a custom input function see CustomPortsNode.cs
            //��δ���ֻ����һ���������ӣ����������ܶ�����룬����봴��һ���Զ������뺯������CustomPortsNode.cs
            if (edges.Count > 0)
            {
                var passThroughObject = edges.First().passThroughBuffer;//ֻ�����������ĵ�һ����

                // We do an extra convertion step in case the buffer output is not compatible with the input port
                // ������������������˿ڲ����ݣ����ǻ�ִ�ж����ת������
                if (passThroughObject != null)
                    if (TypeAdapter.AreAssignable(fieldInfo.FieldType, passThroughObject.GetType()))
                        passThroughObject = TypeAdapter.Convert(passThroughObject, fieldInfo.FieldType);

                fieldInfo.SetValue(fieldOwner, passThroughObject);//*���ø�������(fieldOwner)֧�ֵ��ֶ�(fieldInfo)��ֵ(passThroughObject)
            }
        }
    }

    /// <summary>
    /// Container of ports and the edges connected to these ports
    /// �˿����������ӵ���Щ�˿ڵı�
    /// </summary>
    public abstract class NodePortContainer : List<NodePort>
    {
        protected BaseNode node;

        public NodePortContainer(BaseNode node)//�в������캯��ȷ��ʹ�õĽڵ�
        {
            this.node = node;
        }

        /// <summary>
        /// Remove an edge that is connected to one of the node in the container
        /// ���������Ƴ����ӵ��ڵ��ĳ����
        /// </summary>
        /// <param name="edge"></param>
        public void Remove(SerializableEdge edge)
        {
            ForEach(p => p.Remove(edge));
        }

        /// <summary>
        /// Add an edge that is connected to one of the node in the container
        /// ��������������ӵ��ڵ��һ����
        /// </summary>
        /// <param name="edge"></param>
        public void Add(SerializableEdge edge)
        {
            string portFieldName = (edge.inputNode == node) ? edge.inputFieldName : edge.outputFieldName;//����/����ֶ�����
            string portIdentifier = (edge.inputNode == node) ? edge.inputPortIdentifier : edge.outputPortIdentifier;//����/����˿ڱ�ʶ��

            // Force empty string to null since portIdentifier is a serialized value 
            //ǿ�ƿ��ַ���Ϊ�գ���Ϊ portIdentifier ��һ�����л�ֵ
            if (String.IsNullOrEmpty(portIdentifier))//���Ϊ�ջ���ַ�("")���򷵻�true����ֵ���򷵻�false
                portIdentifier = null;

            var port = this.FirstOrDefault(p =>
            {
                return p.fieldName == portFieldName && p.portData.identifier == portIdentifier;
            });

            if (port == null)
            {
                Debug.LogError("The edge can't be properly connected because it's ports can't be found");
                return;
            }

            port.Add(edge);
        }
    }

    /// <inheritdoc/>
    public class NodeInputPortContainer : NodePortContainer
    {
        public NodeInputPortContainer(BaseNode node) : base(node) { }

        /// <summary>
        /// ����˿������еĶ˿���ȡ����
        /// </summary>
		public void PullDatas()
        {
            ForEach(p => p.PullData());
        }
    }

    /// <inheritdoc/>
    public class NodeOutputPortContainer : NodePortContainer
    {
        public NodeOutputPortContainer(BaseNode node) : base(node) { }

        /// <summary>
        /// ����˿������еĶ˿��������
        /// </summary>
		public void PushDatas()
        {
            ForEach(p => p.PushData());
        }
    }
}