using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using Unity.Jobs;
using System.Linq;

namespace RDTS.NodeEditor
{
    public delegate IEnumerable<PortData> CustomPortBehaviorDelegate(List<SerializableEdge> edges);
    public delegate IEnumerable<PortData> CustomPortTypeBehaviorDelegate(string fieldName, string displayName, object value);

    [Serializable]
    public abstract class BaseNode
    {
        [SerializeField]
        internal string nodeCustomName = null; // The name of the node in case it was renamed by a user 节点的名称，以防它被用户重命名

        /// <summary>
        /// Name of the node, it will be displayed in the title section  
        /// 节点名称，将显示在标题部分
        /// </summary>
        /// <returns></returns>
        public virtual string name => GetType().Name;

        /// <summary>
        /// The accent color of the node
        /// 节点的强调色
        /// </summary>
        public virtual Color color => Color.clear;

        /// <summary>
        /// Set a custom uss file for the node. We use a Resources.Load to get the stylesheet so be sure to put the correct resources path
        /// 为节点设置自定义 uss 文件。 我们使用 Resources.Load 来获取样式表，因此请确保放置正确的资源路径
        /// https://docs.unity3d.com/ScriptReference/Resources.Load.html
        /// </summary>
        public virtual string layoutStyle => string.Empty;

        /// <summary>
        /// If the node can be locked or not
        /// 节点是否能被锁住
        /// </summary>
        public virtual bool unlockable => true;

        /// <summary>
        /// Is the node is locked (if locked it can't be moved)
        /// </summary>
        public virtual bool isLocked => nodeLock;

        //id
        public string GUID;
        /// <summary>
        /// 图表处理时的计算顺序
        /// </summary>
		public int computeOrder = -1;

        /// <summary>
        /// Tell wether or not the node can be processed. Do not check anything from inputs because this step happens before inputs are sent to the node
        /// 判断是否可以处理节点。 不要检查输入中的任何内容，因为此步骤发生在输入发送到节点之前
        /// </summary>
        public virtual bool canProcess => true;

        /// <summary>
        /// Show the node controlContainer only when the mouse is over the node
        /// 仅当鼠标悬停在节点上时才显示节点的controlContainer
        /// </summary>
        public virtual bool showControlsOnHover => false;

        /// <summary>
        /// True if the node can be deleted, false otherwise
        /// 如果可以删除节点，则为 true，否则为 false
        /// </summary>
        public virtual bool deletable => true;

        /// <summary>
        /// Container of input ports 输入端口的容器
        /// </summary>
        [NonSerialized]
        public readonly NodeInputPortContainer inputPorts;
        /// <summary>
        /// Container of output ports  输出端口的容器
        /// </summary>
        [NonSerialized]
        public readonly NodeOutputPortContainer outputPorts;

        //Node view datas
        public Rect position;
        /// <summary>
        /// Is the node expanded 节点是否展开
        /// </summary>
        public bool expanded;
        /// <summary>
        /// Is debug visible 调试是否可见
        /// </summary>
        public bool debug;
        /// <summary>
        /// Node locked state  节点锁定状态
        /// </summary>
        public bool nodeLock;
        /// <summary>
        /// 节点要绑定的对象的guid
        /// </summary>
        public string bindGuid;

        public delegate void ProcessDelegate();

        /// <summary>
        /// Triggered when the node is processes 节点为进程时触发
        /// </summary>
        public event ProcessDelegate onProcessed;
        public event Action<string, NodeMessageType> onMessageAdded;
        public event Action<string> onMessageRemoved;
        /// <summary>
        /// Triggered after an edge was connected on the node  当连线到节点后被触发
        /// </summary>
        public event Action<SerializableEdge> onAfterEdgeConnected;
        /// <summary>
        /// Triggered after an edge was disconnected on the node  当节点上的连线被断开后触发
        /// </summary>
        public event Action<SerializableEdge> onAfterEdgeDisconnected;

        /// <summary>
        /// Triggered after a single/list of port(s) is updated, the parameter is the field name
        /// 更新单个/端口列表后触发，参数为字段名称
        /// </summary>
        public event Action<string> onPortsUpdated;

        [NonSerialized]
        bool _needsInspector = false;

        /// <summary>
        /// Does the node needs to be visible in the inspector (when selected).  
        /// （当被选中时）节点是否需要在检查器中可见
        /// </summary>
        public virtual bool needsInspector => _needsInspector;

        /// <summary>
        /// Can the node be renamed in the UI. By default a node can be renamed by double clicking it's name.
        /// 是否可以在 UI 中重命名节点。 默认情况下，可以通过双击节点名称来重命名节点。
        /// </summary>
        public virtual bool isRenamable => false;

        /// <summary>
        /// Is the node created from a duplicate operation (either ctrl-D or copy/paste).
        /// 是从重复操作（ctrl-D 或复制/粘贴）创建的节点。
        /// </summary>
        public bool createdFromDuplication { get; internal set; } = false;

        /// <summary>
        /// True only when the node was created from a duplicate operation and is inside a group that was also duplicated at the same time. 
        /// 仅当节点是从复制操作创建的并且位于同时复制的group内时才为真。
        /// </summary>
        public bool createdWithinGroup { get; internal set; } = false;//加internal：只有在一个程序集里才能访问到此变量

        /// <summary>字典：节点字段信息（要具备输入/输出特性） </summary>
		[NonSerialized]
        internal Dictionary<string, NodeFieldInformation> nodeFields = new Dictionary<string, NodeFieldInformation>();

        [NonSerialized]
        internal Dictionary<Type, CustomPortTypeBehaviorDelegate> customPortTypeBehaviorMap = new Dictionary<Type, CustomPortTypeBehaviorDelegate>();

        [NonSerialized]
        List<string> messages = new List<string>();

        [NonSerialized]
        protected BaseGraph graph;

        /// <summary>
        /// 节点字段信息（8个信息）
        /// </summary>
		internal class NodeFieldInformation
        {
            public string name;
            public string fieldName;
            public FieldInfo info;
            public bool input;
            public bool isMultiple;
            public string tooltip;
            public CustomPortBehaviorDelegate behavior;
            public bool vertical;

            /// <summary>
            /// 构造函数：节点字段信息（7个参数，8个信息）
            /// </summary>
            /// <param name="info"></param>
            /// <param name="name"></param>
            /// <param name="input"></param>
            /// <param name="isMultiple"></param>
            /// <param name="tooltip"></param>
            /// <param name="vertical"></param>
            /// <param name="behavior"></param>
			public NodeFieldInformation(FieldInfo info, string name, bool input, bool isMultiple, string tooltip, bool vertical, CustomPortBehaviorDelegate behavior)
            {
                this.input = input;
                this.isMultiple = isMultiple;
                this.info = info;
                this.name = name;
                this.fieldName = info.Name;
                this.behavior = behavior;
                this.tooltip = tooltip;
                this.vertical = vertical;
            }
        }

        /// <summary>
        /// 存储一个节点和其包含的字段名称(列表)
        /// </summary>
		struct PortUpdate
        {
            public List<string> fieldNames;//一个节点可以有多个字段（如多个port）
            public BaseNode node;

            public void Deconstruct(out List<string> fieldNames, out BaseNode node)
            {
                fieldNames = this.fieldNames;
                node = this.node;
            }
        }

        // Used in port update algorithm 用于端口更新算法
        Stack<PortUpdate> fieldsToUpdate = new Stack<PortUpdate>();//堆栈：其中元素类型为PortUpdate Push/Pop/Peek
        HashSet<PortUpdate> updatedFields = new HashSet<PortUpdate>();//表示值的集：HashSet<T>类提供高性能集操作。(集是不包含重复元素的集合，其元素没有特定顺序)

        /// <summary>
        /// Creates a node of type T at a certain position 在某个位置创建一个类型为 T 的节点
        /// </summary>
        /// <param name="position">position in the graph in pixels</param>
        /// <typeparam name="T">type of the node</typeparam>
        /// <returns>the node instance</returns>
        public static T CreateFromType<T>(Vector2 position) where T : BaseNode
        {
            return CreateFromType(typeof(T), position) as T;
        }

        /// <summary>
        /// Creates a node of type nodeType at a certain position 在一个给定位置创建一个nodetype的节点
        /// </summary>
        /// <param name="position">position in the graph in pixels</param>
        /// <typeparam name="nodeType">type of the node</typeparam>
        /// <returns>the node instance</returns>
        public static BaseNode CreateFromType(Type nodeType, Vector2 position)
        {
            if (!nodeType.IsSubclassOf(typeof(BaseNode)))//此节点类型不是BaseNode的子类
                return null;

            //Activator：包含特定的方法，用以在本地或从远程创建对象类型，或获取对现有远程对象的引用。 此类不能被继承
            //CreateInstance：使用类型的无参数构造函数创建指定类型的实例
            var node = Activator.CreateInstance(nodeType) as BaseNode;//创建nodetype的对象，并返回对其的引用

            node.position = new Rect(position, new Vector2(100, 100));

            ExceptionToLog.Call(() => node.OnNodeCreated());//分配一个Guid

            return node;
        }

        #region Initialization

        // called by the BaseGraph when the node is added to the graph 当节点被添加至图表时被调用
        public void Initialize(BaseGraph graph)
        {
            this.graph = graph;

            ExceptionToLog.Call(() => Enable());//如果是ParameterNode就会加载其对应的ExposedParameter; 是BaseNode的话Enable()为空，在其子类中可以重载

            InitializePorts();//（通过nodeFields反射信息）初始化port，拉起port打开的CreationMenu匹配正确节点类型也是依赖这里


        }

        /// <summary>
        /// 初始化自定义端口类型方法
        /// </summary>
		void InitializeCustomPortTypeMethods()
        {
            MethodInfo[] methods = new MethodInfo[0];//数组
            Type baseType = GetType();//获取当前的type
            while (true)
            {
                methods = baseType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);//GetMethods()当在派生类中重写时，使用指定绑定约束，搜索为当前 Type 定义的成员。【包含非公共方法和实例方法】
                foreach (var method in methods)
                {
                    var typeBehaviors = method.GetCustomAttributes<CustomPortTypeBehavior>().ToArray();//GetCustomAttributes:检索应用于指定成员的指定类型的自定义特性

                    if (typeBehaviors.Length == 0)
                        continue;

                    CustomPortTypeBehaviorDelegate deleg = null;
                    try
                    {
                        //                               参数：创建委托的类型， 委托要绑定的对象， 表示的静态或实例方法      
                        deleg = Delegate.CreateDelegate(typeof(CustomPortTypeBehaviorDelegate), this, method) as CustomPortTypeBehaviorDelegate;//指定类型的委托，表示指定的静态或实例方法
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                        Debug.LogError($"Cannot convert method {method} to a delegate of type {typeof(CustomPortTypeBehaviorDelegate)}");
                    }

                    foreach (var typeBehavior in typeBehaviors)
                        customPortTypeBehaviorMap[typeBehavior.type] = deleg;
                }

                // Try to also find private methods in the base class
                baseType = baseType.BaseType;
                if (baseType == null)//直至找到最后的基类后(返回null)，退出循环
                    break;
            }
        }

        /// <summary>
        /// Use this function to initialize anything related to ports generation in your node
        /// This will allow the node creation menu to correctly recognize ports that can be connected between nodes
        /// 使用此函数初始化节点中与端口生成相关的任何内容
        /// 这将允许节点创建菜单正确识别可以在节点之间连接的端口
        /// </summary>
        public virtual void InitializePorts()
        {
            InitializeCustomPortTypeMethods();

            foreach (var key in OverrideFieldOrder(nodeFields.Values.Select(k => k.info)))
            {
                var nodeField = nodeFields[key.Name];

                if (HasCustomBehavior(nodeField))//有自定义行为
                {
                    UpdatePortsForField(nodeField.fieldName, sendPortUpdatedEvent: false);
                }
                else
                {
                    // If we don't have a custom behavior on the node, we just have to create a simple port
                    //如果没有在节点上有自定义行为，只需要创建一个简单的端口
                    AddPort(nodeField.input, nodeField.fieldName, new PortData { acceptMultipleEdges = nodeField.isMultiple, displayName = nodeField.name, tooltip = nodeField.tooltip, vertical = nodeField.vertical });
                }
            }
        }

        /// <summary>
        /// Override the field order inside the node. It allows to re-order all the ports and field in the UI.
        /// 覆盖节点内的字段顺序。 它允许重新排序 UI 中的所有端口和字段
        /// </summary>
        /// <param name="fields">List of fields to sort</param>
        /// <returns>Sorted list of fields 排序的字段列表 </returns>
        public virtual IEnumerable<FieldInfo> OverrideFieldOrder(IEnumerable<FieldInfo> fields)
        {
            long GetFieldInheritanceLevel(FieldInfo f)//获取字段继承级别：即从最初的基类继承开始是第几级类
            {
                int level = 0;
                var t = f.DeclaringType;//DeclaringType：获取声明该成员的类
                while (t != null)
                {
                    t = t.BaseType;//BaseType：获取当前 Type 直接从中继承的类型（即此类的上一级的父类）
                    level++;
                }

                return level;//第几级
            }

            // Order by MetadataToken and inheritance level to sync the order with the port order (make sure FieldDrawers are next to the correct port)
            //按 MetadataToken 和继承级别排序以将顺序与端口顺序同步（确保 FieldDrawers 位于正确的端口旁边）
            //OrderByDescending：根据键按降序对序列的元素进行排序
            //按照先继承级别，再Metadata的方式降序排列字段 ——> 继承级别越大（越是子类），顺序越前 
            return fields.OrderByDescending(f => (long)(((GetFieldInheritanceLevel(f) << 32)) | (long)f.MetadataToken));//MetadataToken：获取一个值，该值标识元数据元素
        }

        /// <summary>
        /// 构造函数，初始化inputPorts/outputPorts
        /// </summary>
		protected BaseNode()
        {
            inputPorts = new NodeInputPortContainer(this);
            outputPorts = new NodeOutputPortContainer(this);

            InitializeInOutDatas();//*反射，根据Attribu构造In/Out数据信息，并缓存到nodeFields中

            if (bindGuid == null)
            {
                bindGuid = System.Guid.NewGuid().ToString();
            }
        }

        /// <summary>
        /// Update all ports of the node 更新节点上所有的端口
        /// </summary>
        public bool UpdateAllPorts()
        {
            bool changed = false;

            //Select：返回一个 IEnumerable<T>，其元素是对 source(即nodeFields.Values) 的每个元素调用转换函数(k => k.info)得到的结果
            foreach (var key in OverrideFieldOrder(nodeFields.Values.Select(k => k.info)))//返回FieldInfo类型的(结果)表单
            {
                var field = nodeFields[key.Name];
                changed |= UpdatePortsForField(field.fieldName);//更新与一个 C# 属性字段相关的端口和图中所有连接的节点
            }

            return changed;//是否全部更新完毕
        }

        /// <summary>
        /// Update all ports of the node without updating the connected ports. Only use this method when you need to update all the nodes ports in your graph.
        /// 更新节点的所有端口，而不更新连接的端口。 仅当需要更新图中的所有节点端口时才使用此方法。
        /// </summary>
        public bool UpdateAllPortsLocal()
        {
            bool changed = false;

            foreach (var key in OverrideFieldOrder(nodeFields.Values.Select(k => k.info)))
            {
                var field = nodeFields[key.Name];
                changed |= UpdatePortsForFieldLocal(field.fieldName);
            }

            return changed;
        }


        /// <summary>
        /// Update the ports related to one C# property field (only for this node)
        /// 更新与一个 C# 属性字段相关的端口（仅适用于该节点）
        /// 所做工作：
        ///     1.判断是输入/输出端口（容器），收集此端口的所有字段和edge信息
        ///     2.添加端口数据，确定最终的端口列表（用于进一步处理）
        ///     3.删除不存在的端口
        ///     4.确保端口顺序正确
        ///     5.更新端口列表
        /// </summary>
        /// <param name="fieldName"></param>
        public bool UpdatePortsForFieldLocal(string fieldName, bool sendPortUpdatedEvent = true)
        {
            bool changed = false;

            if (!nodeFields.ContainsKey(fieldName))
                return false;

            var fieldInfo = nodeFields[fieldName];

            if (!HasCustomBehavior(fieldInfo))
                return false;

            List<string> finalPorts = new List<string>();

            var portCollection = fieldInfo.input ? (NodePortContainer)inputPorts : outputPorts;//判断是输出/输入端口容器

            // Gather all fields for this port (before to modify them) 收集此端口的所有字段（在修改它们之前）
            var nodePorts = portCollection.Where(p => p.fieldName == fieldName);
            // Gather all edges connected to these fields: 收集连接到这些字段的所有边：
            var edges = nodePorts.SelectMany(n => n.GetEdges()).ToList();

            //添加端口数据，确定最终的端口列表（用于进一步处理）
            if (fieldInfo.behavior != null)
            {
                foreach (var portData in fieldInfo.behavior(edges))
                    AddPortData(portData);
            }
            else
            {
                var customPortTypeBehavior = customPortTypeBehaviorMap[fieldInfo.info.FieldType];

                foreach (var portData in customPortTypeBehavior(fieldName, fieldInfo.name, fieldInfo.info.GetValue(this)))
                    AddPortData(portData);
            }

            ///若端口不存在则增加一个；
            ///若端口存在：端口不兼容则断开连线，修补端口数据（信息）
            ///在最终的端口列表中添加此端口的标识符
			void AddPortData(PortData portData)
            {
                var port = nodePorts.FirstOrDefault(n => n.portData.identifier == portData.identifier);
                // Guard using the port identifier so we don't duplicate identifiers
                if (port == null)//为空则增加一个port
                {
                    AddPort(fieldInfo.input, fieldName, portData);
                    changed = true;
                }
                else
                {
                    // in case the port type have changed for an incompatible type, we disconnect all the edges attached to this port
                    //如果端口类型已更改为不兼容的类型，我们将断开连接到该端口的所有边缘
                    if (!BaseGraph.TypesAreConnectable(port.portData.displayType, portData.displayType))
                    {
                        foreach (var edge in port.GetEdges().ToList())
                            graph.Disconnect(edge.GUID);
                    }

                    // patch the port data 修补端口数据
                    if (port.portData != portData)
                    {
                        port.portData.CopyFrom(portData);
                        changed = true;
                    }
                }

                finalPorts.Add(portData.identifier);//在最终的端口列表中添加标识符
            }

            // TODO
            // Remove only the ports that are no more in the list 仅删除列表中不再存在的端口
            if (nodePorts != null)
            {
                var currentPortsCopy = nodePorts.ToList();
                foreach (var currentPort in currentPortsCopy)
                {
                    // If the current port does not appear in the list of final ports, we remove it
                    // 如果当前端口没有出现在最终端口列表中，我们将其删除
                    if (!finalPorts.Any(id => id == currentPort.portData.identifier))
                    {
                        RemovePort(fieldInfo.input, currentPort);
                        changed = true;
                    }
                }
            }

            // Make sure the port order is correct: 确保端口顺序正确：
            portCollection.Sort((p1, p2) => {//Sort:使用指定的比较器对【整个 List<T> 】中的元素进行排序（从小到大）
                int p1Index = finalPorts.FindIndex(id => p1.portData.identifier == id);//FindIndex:搜索与指定谓词所定义的条件相匹配的元素，并返回整个 List<T> 中第一个匹配元素的从零开始的索引
                int p2Index = finalPorts.FindIndex(id => p2.portData.identifier == id);

                if (p1Index == -1 || p2Index == -1)//FindIndex没找到则返回-1
                    return 0;

                return p1Index.CompareTo(p2Index);//将此实例(p1Index)与指定的 32 位有符号整数(p2Index)进行比较并返回对其相对值的指示
            });

            if (sendPortUpdatedEvent)
                onPortsUpdated?.Invoke(fieldName);//更新单个/端口列表后触发，参数为字段名称

            return changed;
        }

        /// <summary>
        /// 判断是否有自定义的行为。 
        /// 有为true，无为false
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
		bool HasCustomBehavior(NodeFieldInformation info)
        {
            if (info.behavior != null)
                return true;

            if (customPortTypeBehaviorMap.ContainsKey(info.info.FieldType))
                return true;

            return false;
        }

        /// <summary>
        /// Update the ports related to one C# property field and all connected nodes in the graph
        /// 更新与一个 C# 属性字段相关的端口和图中所有连接的节点
        ///   工作流程：将此节点的堆栈和集清空，将以参数字段和此节点为内容的元素入栈(此时堆栈中元素仅一个)，
        ///        在第1次while循环中将此元素弹出，通过此参数字段对节点的端口进行更新，通过对端口和edge的两次遍
        ///        历获取更新后的信息存入堆栈中。此时由于堆栈元素个数不为0进入第2次循环，将元素弹出，因为此
        ///        元素已更新所以与集中的元素不相等，故再进行一次更新。等到第3次进入循环时，由于堆栈与集中
        ///        的元素相等便会跳过更新的程序，所以堆栈中元素个数为0，跳出循环，完成此方法的更新功能。
        /// </summary>
        /// <param name="fieldName"></param>
        public bool UpdatePortsForField(string fieldName, bool sendPortUpdatedEvent = true)
        {
            bool changed = false;

            ///先将此节点的堆栈和集 清空
			fieldsToUpdate.Clear();//堆栈情况
            updatedFields.Clear();//存值的集情况

            fieldsToUpdate.Push(new PortUpdate { fieldNames = new List<string>() { fieldName }, node = this });//将 (参数字段, 此节点) 入栈。 栈中仅一个元素

            // Iterate through all the ports that needs to be updated, following graph connection when the 
            // port is updated. This is required ton have type propagation multiple nodes that changes port types
            // are connected to each other (i.e. the relay node)
            //迭代所有需要更新的端口，更新端口时遵循图形连接。 
            //这是必需的，具有类型传播多个更改端口类型的节点相互连接（即中继节点）
            while (fieldsToUpdate.Count != 0)
            {
                //将上面刚入栈的元素弹出
                var (fields, node) = fieldsToUpdate.Pop();//*因为PortUpdate类中包含List<string>和BaseNode两种类型

                // Avoid updating twice a port  避免对一个端口更新两次
                if (updatedFields.Any((t) => t.node == node && fields.SequenceEqual(t.fieldNames)))//将从堆栈弹出的这个元素与“集”中的元素进行比较，若有任意一个元素相等，则跳过后续程序进行下一次循环
                    continue;
                updatedFields.Add(new PortUpdate { fieldNames = fields, node = node });//若有不相等的元素（新的元素），就加入到“集”中
                                                                                       ///【集中的元素都是不相等的】

                foreach (var field in fields)//遍历字段列表（第一次 只有一个参数变量的字段）
                {
                    if (node.UpdatePortsForFieldLocal(field, sendPortUpdatedEvent))//【更新】该节点上此字段相关的端口
                    {
                        foreach (var port in node.IsFieldInput(field) ? (NodePortContainer)node.inputPorts : node.outputPorts)//遍历更新好的端口容器
                        {
                            //找到与给定字段对应的端口
                            //不是：就直接返回
                            if (port.fieldName != field)
                                continue;
                            //是：
                            foreach (var edge in port.GetEdges())//遍历此端口中所有的edge
                            {
                                //注意：edge的输入端口所在节点是edge的输出节点
                                var edgeNode = (node.IsFieldInput(field)) ? edge.outputNode : edge.inputNode;//找到此edge在给定字段下对应的节点
                                var fieldsWithBehavior = edgeNode.nodeFields.Values.Where(f => HasCustomBehavior(f)).Select(f => f.fieldName).ToList();//获取此节点上具有自定义行为的字段
                                fieldsToUpdate.Push(new PortUpdate { fieldNames = fieldsWithBehavior, node = edgeNode });//将重新检索后的元素放入堆栈
                            }
                        }
                        changed = true;
                    }
                }
            }

            return changed;
        }

        HashSet<BaseNode> portUpdateHashSet = new HashSet<BaseNode>();

        /// <summary>将输入/输出端口容器清空，调用Disable()</summary>
        internal void DisableInternal()
        {
            // port containers are initialized in the OnEnable 端口容器在
            inputPorts.Clear();
            outputPorts.Clear();

            ExceptionToLog.Call(() => Disable());
        }
        /// <summary>调用Destroy()</summary>
		internal void DestroyInternal() => ExceptionToLog.Call(() => Destroy());

        /// <summary>
        /// Called only when the node is created, not when instantiated 仅在创建节点时调用，而不是在实例化时调用
        /// 在节点创建时分配一个GUID
        /// </summary>
        public virtual void OnNodeCreated() => GUID = Guid.NewGuid().ToString();

        /// <summary>获取节点字段</summary>
		public virtual FieldInfo[] GetNodeFields()
            => GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);


        /// <summary>
        /// 初始化输入/输出特性的节点字段信息（对字典 nodeFields 进行填充）
        /// </summary>
		void InitializeInOutDatas()
        {
            var fields = GetNodeFields();//获取节点字段
            var methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);//获取方法

            /* 遍历字段：储存具有输入/输出端口特性的字段信息，但自定义行为为null */
            foreach (var field in fields)
            {
                //获取字段的“特性”信息
                var inputAttribute = field.GetCustomAttribute<InputAttribute>();
                var outputAttribute = field.GetCustomAttribute<OutputAttribute>();
                var tooltipAttribute = field.GetCustomAttribute<TooltipAttribute>();
                var showInInspector = field.GetCustomAttribute<ShowInInspector>();//是否在节点视图中显示该字段
                var vertical = field.GetCustomAttribute<VerticalAttribute>();
                bool isMultiple = false;
                bool input = false;
                string name = field.Name;
                string tooltip = null;

                if (showInInspector != null)
                    _needsInspector = true;

                /* 不是输入/输出端口特性就跳过 */
                if (inputAttribute == null && outputAttribute == null)
                    continue;

                //check if field is a collection type 检查字段是否为集合类型
                isMultiple = (inputAttribute != null) ? inputAttribute.allowMultiple : (outputAttribute.allowMultiple);//是否允许一个端口多个edge
                input = inputAttribute != null;//true为输入端口，false为输出端口
                tooltip = tooltipAttribute?.tooltip;//获取tooltip信息

                //端口有显示名称就获取
                if (!String.IsNullOrEmpty(inputAttribute?.name))
                    name = inputAttribute.name;
                if (!String.IsNullOrEmpty(outputAttribute?.name))
                    name = outputAttribute.name;

                // By default we set the behavior to null, if the field have a custom behavior, it will be set in the loop just below
                // 默认情况下我们将行为设置为null，如果该字段有自定义行为，它将在下面的循环中设置
                nodeFields[field.Name] = new NodeFieldInformation(field, name, input, isMultiple, tooltip, vertical != null, null);
            }

            /* 遍历方法：设置字段信息中的自定义行为 */
            foreach (var method in methods)
            {
                var customPortBehaviorAttribute = method.GetCustomAttribute<CustomPortBehaviorAttribute>();//检索应用于指定成员的指定类型CustomPortBehaviorAttribute的自定义特性
                CustomPortBehaviorDelegate behavior = null;

                if (customPortBehaviorAttribute == null)
                    continue;

                // Check if custom port behavior function is valid 
                //检查自定义端口行为函数是否有效
                try
                {
                    var referenceType = typeof(CustomPortBehaviorDelegate);
                    behavior = (CustomPortBehaviorDelegate)Delegate.CreateDelegate(referenceType, this, method, true);
                    //CreateDelegate：创建表示指定的静态方法或实例方法的指定类型的委托
                    //    参数：（要创建的委托类型， 委托表示的方法的第一个自变量， 该委托要表示的静态或实例方法的MethodInfo， 如果无法绑定 method 时引发异常，则为 true；否则为 false）
                    //    返回：表示指定静态方法或实例方法的指定类型的委托；如果 第四个参数 为 false，并且委托无法绑定到 method，则为 null
                }
                catch
                {
                    Debug.LogError("The function " + method + " cannot be converted to the required delegate format: " + typeof(CustomPortBehaviorDelegate));
                }

                //若已包含该自定义特性的字段名，就设置其行为委托
                if (nodeFields.ContainsKey(customPortBehaviorAttribute.fieldName))
                    nodeFields[customPortBehaviorAttribute.fieldName].behavior = behavior;
                else//否则 自定义端口行为的字段名称无效
                    Debug.LogError("Invalid field name for custom port behavior: " + method + ", " + customPortBehaviorAttribute.fieldName);
            }
        }

        #endregion

        #region Events and Processing

        public void OnEdgeConnected(SerializableEdge edge)
        {
            bool input = edge.inputNode == this;//是否是输入节点
            NodePortContainer portCollection = (input) ? (NodePortContainer)inputPorts : outputPorts;//是输出还是输入端口的容器

            portCollection.Add(edge);

            UpdateAllPorts();//更新此节点上的所有端口

            onAfterEdgeConnected?.Invoke(edge);
        }

        protected virtual bool CanResetPort(NodePort port) => true;

        public void OnEdgeDisconnected(SerializableEdge edge)
        {
            if (edge == null)
                return;

            bool input = edge.inputNode == this;
            NodePortContainer portCollection = (input) ? (NodePortContainer)inputPorts : outputPorts;

            portCollection.Remove(edge);//移除该edge

            // Reset default values of input port: 重置输入端口为默认值
            bool haveConnectedEdges = edge.inputNode.inputPorts.Where(p => p.fieldName == edge.inputFieldName).Any(p => p.GetEdges().Count != 0);//该节点是否有连接的edge
            if (edge.inputNode == this && !haveConnectedEdges && CanResetPort(edge.inputPort))
                edge.inputPort?.ResetToDefault();

            UpdateAllPorts();

            onAfterEdgeDisconnected?.Invoke(edge);
        }

        /// <summary>
        /// *处理：通过edge流通数据，调用相关的处理方法
        /// </summary>
		public void OnProcess()
        {
            inputPorts.PullDatas();//从edge缓存区提取数据到输入端口

            ExceptionToLog.Call(() => Process());//调用自定义处理方法

            InvokeOnProcessed();//调用进程委托

            outputPorts.PushDatas();//输出数据到edge的缓存区
        }

        public void InvokeOnProcessed() => onProcessed?.Invoke();

        /// <summary>
        /// Called when the node is enabled  当启用节点时调用
        /// </summary>
        protected virtual void Enable() { }
        /// <summary>
        /// Called when the node is disabled 当禁用节点时调用
        /// </summary>
        protected virtual void Disable() { }
        /// <summary>
        /// Called when the node is removed 当移除节点时调用
        /// </summary>
        protected virtual void Destroy() { }

        /// <summary>
        /// Override this method to implement custom processing 重写该方法实现自定义处理
        /// </summary>
        protected virtual void Process() { }

        #endregion

        #region API and utils

        /// <summary>
        /// Add a port  添加一个端口
        /// </summary>
        /// <param name="input">is input port</param>
        /// <param name="fieldName">C# field name</param>
        /// <param name="portData">Data of the port</param>
        public void AddPort(bool input, string fieldName, PortData portData)
        {
            // Fixup port data info if needed: 如果需要，修复端口数据信息：
            if (portData.displayType == null)
                portData.displayType = nodeFields[fieldName].info.FieldType;

            if (input)
                inputPorts.Add(new NodePort(this, fieldName, portData));
            else
                outputPorts.Add(new NodePort(this, fieldName, portData));
        }

        /// <summary>
        /// Remove a port  移除一个端口
        /// </summary>
        /// <param name="input">is input port</param>
        /// <param name="port">the port to delete</param>
        public void RemovePort(bool input, NodePort port)
        {
            ///Debug.Log("Test: Remove port  "+ port.fieldName);

            if (input)
                inputPorts.Remove(port);
            else
                outputPorts.Remove(port);
        }

        /// <summary>
        /// Remove port(s) from field name 从字段名称中删除端口
        /// </summary>
        /// <param name="input">is input</param>
        /// <param name="fieldName">C# field name</param>
        public void RemovePort(bool input, string fieldName)
        {
            if (input)
                inputPorts.RemoveAll(p => p.fieldName == fieldName);
            else
                outputPorts.RemoveAll(p => p.fieldName == fieldName);
        }

        /// <summary>
        /// Get all the nodes connected to the input ports of this node
        /// 获取连接到该节点输入端口的所有节点
        /// </summary>
        /// <returns>an enumerable of node</returns>
        public IEnumerable<BaseNode> GetInputNodes()
        {
            foreach (var port in inputPorts)
                foreach (var edge in port.GetEdges())
                    yield return edge.outputNode;
        }

        /// <summary>
        /// Get all the nodes connected to the output ports of this node
        /// 获取连接到该节点输出端口的所有节点
        /// </summary>
        /// <returns>an enumerable of node</returns>
        public IEnumerable<BaseNode> GetOutputNodes()
        {
            foreach (var port in outputPorts)
                foreach (var edge in port.GetEdges())
                    yield return edge.inputNode;
        }

        /// <summary>
        /// Return a node matching the condition in the dependencies of the node
        /// 返回与节点的依赖项中的条件匹配的节点
        /// </summary>
        /// <param name="condition">Condition to choose the node</param>
        /// <returns>Matched node or null</returns>
        public BaseNode FindInDependencies(Func<BaseNode, bool> condition)
        {
            Stack<BaseNode> dependencies = new Stack<BaseNode>();

            dependencies.Push(this);

            int depth = 0;
            while (dependencies.Count > 0)
            {
                var node = dependencies.Pop();

                // Guard for infinite loop (faster than a HashSet based solution)
                depth++;
                if (depth > 2000)//堆栈中个数大于2000就跳出
                    break;

                if (condition(node))//按给定的条件方法去查找
                    return node;

                foreach (var dep in node.GetInputNodes())//未找到就将此节点的输入端的所有节点放入堆栈中进行查找
                    dependencies.Push(dep);
            }
            return null;
        }

        /// <summary>
        /// Get the port from field name and identifier  根据字段名称和标识符来获取端口
        /// </summary>
        /// <param name="fieldName">C# field name</param>
        /// <param name="identifier">Unique port identifier</param>
        /// <returns></returns>
        public NodePort GetPort(string fieldName, string identifier)
        {
            //Concat：连接两个序列。 即将inputPorts和outputPorts两个序列连接起来
            return inputPorts.Concat(outputPorts).FirstOrDefault(p => {
                var bothNull = String.IsNullOrEmpty(identifier) && String.IsNullOrEmpty(p.portData.identifier);
                return p.fieldName == fieldName && (bothNull || identifier == p.portData.identifier);
            });
        }

        /// <summary>
        /// Return all the ports of the node  返回此节点的所有端口（包括输入输出）
        /// </summary>
        /// <returns></returns>
        public IEnumerable<NodePort> GetAllPorts()
        {
            foreach (var port in inputPorts)
                yield return port;
            foreach (var port in outputPorts)
                yield return port;
        }

        /// <summary>
        /// Return all the connected edges of the node  返回此节点的所有连线
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SerializableEdge> GetAllEdges()
        {
            foreach (var port in GetAllPorts())
                foreach (var edge in port.GetEdges())
                    yield return edge;
        }

        /// <summary>
        /// Is the port an input    判断是否是输入端口
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public bool IsFieldInput(string fieldName) => nodeFields[fieldName].input;

        /// <summary>
        /// Add a message on the node   在节点上添加消息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageType"></param>
        public void AddMessage(string message, NodeMessageType messageType)
        {
            if (messages.Contains(message))
                return;

            onMessageAdded?.Invoke(message, messageType);
            messages.Add(message);
        }

        /// <summary>
        /// Remove a message on the node  从节点上移除消息
        /// </summary>
        /// <param name="message"></param>
        public void RemoveMessage(string message)
        {
            onMessageRemoved?.Invoke(message);
            messages.Remove(message);
        }

        /// <summary>
        /// Remove a message that contains
        /// </summary>
        /// <param name="subMessage"></param>
        public void RemoveMessageContains(string subMessage)
        {
            string toRemove = messages.Find(m => m.Contains(subMessage));
            messages.Remove(toRemove);
            onMessageRemoved?.Invoke(toRemove);
        }

        /// <summary>
        /// Remove all messages on the node  清除节点上的所有消息
        /// </summary>
        public void ClearMessages()
        {
            foreach (var message in messages)
                onMessageRemoved?.Invoke(message);
            messages.Clear();
        }

        /// <summary>
        /// Set the custom name of the node. This is intended to be used by renamable nodes.
        /// 设置节点的 nodeCustomName，这是供可重命名节点使用
        /// This custom name will be serialized inside the node.
        /// 此自定义名称将在节点内序列化
        /// </summary>
        /// <param name="customNodeName">New name of the node.</param>
        public void SetCustomName(string customName) => nodeCustomName = customName;

        /// <summary>
        /// Get the name of the node. If the node have a custom name (set using the UI by double clicking on the node title) then it will return this name first, otherwise it returns the value of the name field.
        /// 获取节点的名称。 如果节点有自定义名称（通过双击节点标题使用 UI 设置），那么它将首先返回此名称，否则返回名称字段的值。
        /// </summary>
        /// <returns>The name of the node as written in the title</returns>
        public string GetCustomName() => String.IsNullOrEmpty(nodeCustomName) ? name : nodeCustomName;

        #endregion
    }
}
