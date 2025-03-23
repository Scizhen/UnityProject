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
    /// 描述其创建的端口属性的类（端口信息）
    /// </summary>
    public class PortData : IEquatable<PortData>
    {
        /// <summary>
        /// Unique identifier for the port 端口的唯一标识符
        /// </summary>
        public string identifier;
        /// <summary>
        /// Display name on the node 在节点上显示名称
        /// </summary>
        public string displayName;
        /// <summary>
        /// The type that will be used for coloring with the type stylesheet
        /// 将用于使用类型样式表着色的类型
        /// </summary>
        public Type displayType;
        /// <summary>
        /// If the port accept multiple connection 此端口是否接收多次连接
        /// </summary>
        public bool acceptMultipleEdges;
        /// <summary>
        /// Port size, will also affect the size of the connected edge
        /// 端口尺寸，也将会影响连接线的尺寸
        /// </summary>
        public int sizeInPixel;
        /// <summary>
        /// Tooltip of the port 端口的工具提示
        /// </summary> 
        public string tooltip;
        /// <summary>
        /// Is the port vertical 是否垂直显示端口
        /// </summary>
        public bool vertical;

        /// <summary>
        /// 是否与另一个PortData相同，是则返回true
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
        /// 复制另一个PortData于此
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
    /// 存储一个端口处理时所需的所有信息的类
    /// </summary>
    public class NodePort
    {
        /// <summary>
        /// The actual name of the property behind the port (must be exact, it is used for Reflection)
        /// 端口后面的属性的实际名称（必须准确，用于反射）
        /// </summary>
        public string fieldName;
        /// <summary>
        /// The node on which the port is 端口所在的节点
        /// </summary>
        public BaseNode owner;
        /// <summary>
        /// The fieldInfo from the fieldName   fieldName中的fieldInfo
        /// FieldInfo类：发现字段的属性并提供对字段元数据的访问权限
        /// </summary>
        public FieldInfo fieldInfo;//FieldInfo：发现字段的属性并提供对字段元数据的访问权限

        /// <summary>
        /// Data of the port 端口数据
        /// </summary>
        public PortData portData;
        List<SerializableEdge> edges = new List<SerializableEdge>();//连线的列表
        Dictionary<SerializableEdge, PushDataDelegate> pushDataDelegates = new Dictionary<SerializableEdge, PushDataDelegate>();
        List<SerializableEdge> edgeWithRemoteCustomIO = new List<SerializableEdge>();//另一个自定义IO的边

        /// <summary>
        /// Owner of the FieldInfo, to be used in case of Get/SetValue
        /// FieldInfo 的所有者，在 Get/SetValue 的情况下使用
        /// </summary>
        public object fieldOwner;//拥有该字段的节点

        CustomPortIODelegate customPortIOMethod;

        /// <summary>
        /// Delegate that is made to send the data from this port to another port connected through an edge
        /// 将数据从该端口发送到通过边连接的另一个端口的委托
        /// This is an optimization compared to dynamically setting values using Reflection (which is really slow)
        /// 与使用反射动态设置值相比，这是一种优化（这真的很慢）
        /// More info: https://codeblog.jonskeet.uk/2008/08/09/making-reflection-fly-and-exploring-delegates/
        /// </summary>
        public delegate void PushDataDelegate();

        /// <summary>
        /// Constructor 构造函数
        /// </summary>
        /// <param name="owner">owner node</param>
        /// <param name="fieldName">the C# property name</param>
        /// <param name="portData">Data of the port</param>
        public NodePort(BaseNode owner, string fieldName, PortData portData) : this(owner, owner, fieldName, portData)
        {
            Debug.Log($"fieldName: {this.fieldName} + fieldInfo: {this.fieldInfo.ToString()} + fieldInfoValue: {this.fieldInfo.GetValue(owner)?.ToString()}");


        }

        /// <summary>
        /// Constructor 构造函数
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

            fieldInfo = fieldOwner.GetType().GetField(//使用指定绑定约束搜索指定字段：表示符合指定要求的字段的对象（如果找到的话）；否则为 null
                fieldName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            customPortIOMethod = CustomPortIO.GetCustomPortMethod(owner.GetType(), fieldName);//根据指定的节点类型和(端口)字段名获取对应的具有“端口输出/输入特性”的方法的委托
        }

        /// <summary>
        /// Connect an edge to this port  连线到此端口
        /// </summary>
        /// <param name="edge"></param>
        public void Add(SerializableEdge edge)
        {
            if (!edges.Contains(edge))//若不包含此edge就添加至列表
                edges.Add(edge);

            if (edge.inputNode == owner)//若此edge的输入节点是当前端口所在节点
            {
                if (edge.outputPort.customPortIOMethod != null)//若此edge的输出端口的自定义端口IO方法存在
                    edgeWithRemoteCustomIO.Add(edge);
            }
            else//若此edge的输入节点不是当前端口所在节点（则对应输出节点）
            {
                if (edge.inputPort.customPortIOMethod != null)//若此edge的输入端口的自定义端口IO方法存在
                    edgeWithRemoteCustomIO.Add(edge);
            }

            //if we have a custom io implementation, we don't need to genereate the defaut one
            //如果我们有自定义的 io 实现，我们不需要生成默认的
            if (edge.inputPort.customPortIOMethod != null || edge.outputPort.customPortIOMethod != null)
                return;

            PushDataDelegate edgeDelegate = CreatePushDataDelegateForEdge(edge);

            if (edgeDelegate != null)
                pushDataDelegates[edge] = edgeDelegate;
        }

        /// <summary>
        /// 将数据从该端口发送到通过边连接的另一个端口的委托
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
		PushDataDelegate CreatePushDataDelegateForEdge(SerializableEdge edge)
        {
            try
            {
                //Creation of the delegate to move the data from the input node to the output node:
                //创建委托以将数据从输入节点移动到输出节点：
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
                if (!BaseGraph.TypesAreConnectable(inputField.FieldType, outputField.FieldType))//两种类型是否能进行连接
                {
                    Debug.LogError("Can't convert from " + inputField.FieldType + " to " + outputField.FieldType + ", you must specify a custom port function (i.e CustomPortInput or CustomPortOutput) for non-implicit convertions");
                    return null;
                }
#endif
                ///Expression：提供一种基类，表示表达式树节点的类派生自该基类
				Expression inputParamField = Expression.Field(Expression.Constant(edge.inputNode), inputField);//在edge.inputNode类中的inputField字段（类中的某个变量）
                Expression outputParamField = Expression.Field(Expression.Constant(edge.outputNode), outputField);

                inType = edge.inputPort.portData.displayType ?? inputField.FieldType;//输入端口的字段类型
                outType = edge.outputPort.portData.displayType ?? outputField.FieldType;//输出端口的字段类型

                // If there is a user defined convertion function, then we call it  如果有自定义的转换函数，那么调用它
                if (TypeAdapter.AreAssignable(outType, inType))//类型是否可分配
                {
                    // We add a cast in case there we're calling the conversion method with a base class parameter (like object)
                    // 添加一个强制转换，以防使用基类参数（如对象）调用转换方法
                    var convertedParam = Expression.Convert(outputParamField, outType);//将outputParamField转换成outType类型
                    ///Call：表示对使用一个参数的static方法的调用
                    outputParamField = Expression.Call(TypeAdapter.GetConvertionMethod(outType, inType), convertedParam);
                    // In case there is a custom port behavior in the output, then we need to re-cast to the base type because
                    // the convertion method return type is not always assignable directly:
                    // 如果输出中有自定义端口行为，那么我们需要重新转换为基本类型，因为转换方法返回类型并不总是可以直接赋值
                    outputParamField = Expression.Convert(outputParamField, inputField.FieldType);
                }
                else // otherwise we cast
                    outputParamField = Expression.Convert(outputParamField, inputField.FieldType);

                ///BinaryExpression：表示具有二进制运算符的表达式
				BinaryExpression assign = Expression.Assign(inputParamField, outputParamField);//Assign赋值运算：assign为“inputParamField = outputParamField”
                ///Compile：将lambda表达式编译成委托
                return Expression.Lambda<PushDataDelegate>(assign).Compile();//返回：inputParamField = outputParamField的委托
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return null;
            }
        }

        /// <summary>
        /// Disconnect an Edge from this port  从端口移除此edge
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
        /// edge接口： Get all the edges connected to this port 获取此端口的所有edge
        /// </summary>
        /// <returns></returns>
        public List<SerializableEdge> GetEdges() => edges;

        /// <summary>
        /// Push the value of the port through the edges 通过边缘推送端口的值
        /// This method can only be called on output ports 此方法只能在输出端口被调用
        /// </summary>
        public void PushData()
        {
            /* 对于自定义IO的数据传输 */
            if (customPortIOMethod != null)
            {
                customPortIOMethod(owner, edges, this);
                return;
            }

            /* 对于非自定义IO的数据传输 */
            foreach (var pushDataDelegate in pushDataDelegates)//遍历所有PushData的委托方法，进行调用
                pushDataDelegate.Value();

            /* 对于自定义IO的数据传输 */
            if (edgeWithRemoteCustomIO.Count == 0)////若另一端口不存在则无需进行数据传输
                return;
            //if there are custom IO implementation on the other ports, they'll need our value in the passThrough buffer
            //如果其他端口上有自定义 IO 实现，他们将需要我们在 passThrough 缓冲区中的值
            object ourValue = fieldInfo.GetValue(fieldOwner);//GetValue：当在派生类中重写时，返回给定对象(fieldOwner)支持的字段(fieldInfo)的值
            foreach (var edge in edgeWithRemoteCustomIO)
                edge.passThroughBuffer = ourValue;//将值放入缓存区
        }

        /// <summary>
        /// Reset the value of the field to default if possible
        /// 如果可能，将字段的值重置为默认值
        /// </summary>
        public void ResetToDefault()
        {
            // Clear lists, set classes to null and struct to default value.
            if (typeof(IList).IsAssignableFrom(fieldInfo.FieldType))//IsAssignableFrom：确定指定类型(c)的实例是否能分配给当前类型的变量；FieldType；获取此字段对象的类型
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
        /// Pull values from the edge (in case of a custom convertion method) 从边缘提取值（在自定义转换方法的情况下）
        /// This method can only be called on input ports 此方法只能被输入端口调用
        /// </summary>
        public void PullData()
        {
            /* 对于自定义IO的数据传输 */
            if (customPortIOMethod != null)
            {
                customPortIOMethod(owner, edges, this);
                return;
            }
            // check if this port have connection to ports that have custom output functions
            //检查此端口是否连接到具有自定义输出功能的端口
            if (edgeWithRemoteCustomIO.Count == 0)//若另一端口不存在则无需进行数据传输
                return;

            /* 对于自定义IO的数据传输 */
            // Only one input connection is handled by this code, if you want to
            // take multiple inputs, you must create a custom input function see CustomPortsNode.cs
            //这段代码只处理一个输入连接，如果你想接受多个输入，你必须创建一个自定义输入函数，见CustomPortsNode.cs
            if (edges.Count > 0)
            {
                var passThroughObject = edges.First().passThroughBuffer;//只处理搜索到的第一条线

                // We do an extra convertion step in case the buffer output is not compatible with the input port
                // 如果缓冲区输出与输入端口不兼容，我们会执行额外的转换步骤
                if (passThroughObject != null)
                    if (TypeAdapter.AreAssignable(fieldInfo.FieldType, passThroughObject.GetType()))
                        passThroughObject = TypeAdapter.Convert(passThroughObject, fieldInfo.FieldType);

                fieldInfo.SetValue(fieldOwner, passThroughObject);//*设置给定对象(fieldOwner)支持的字段(fieldInfo)的值(passThroughObject)
            }
        }
    }

    /// <summary>
    /// Container of ports and the edges connected to these ports
    /// 端口容器和连接到这些端口的边
    /// </summary>
    public abstract class NodePortContainer : List<NodePort>
    {
        protected BaseNode node;

        public NodePortContainer(BaseNode node)//有参数构造函数确定使用的节点
        {
            this.node = node;
        }

        /// <summary>
        /// Remove an edge that is connected to one of the node in the container
        /// 在容器中移除连接到节点的某条边
        /// </summary>
        /// <param name="edge"></param>
        public void Remove(SerializableEdge edge)
        {
            ForEach(p => p.Remove(edge));
        }

        /// <summary>
        /// Add an edge that is connected to one of the node in the container
        /// 在容器中添加连接到节点的一条边
        /// </summary>
        /// <param name="edge"></param>
        public void Add(SerializableEdge edge)
        {
            string portFieldName = (edge.inputNode == node) ? edge.inputFieldName : edge.outputFieldName;//输入/输出字段名称
            string portIdentifier = (edge.inputNode == node) ? edge.inputPortIdentifier : edge.outputPortIdentifier;//输入/输出端口标识符

            // Force empty string to null since portIdentifier is a serialized value 
            //强制空字符串为空，因为 portIdentifier 是一个序列化值
            if (String.IsNullOrEmpty(portIdentifier))//如果为空或空字符("")，则返回true；有值，则返回false
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
        /// 输入端口容器中的端口提取数据
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
        /// 输出端口容器中的端口输出数据
        /// </summary>
		public void PushDatas()
        {
            ForEach(p => p.PushData());
        }
    }
}