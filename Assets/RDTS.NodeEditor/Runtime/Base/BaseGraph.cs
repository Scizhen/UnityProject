using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace RDTS.NodeEditor
{
    public class GraphChanges
    {
        public SerializableEdge removedEdge;//edge
        public SerializableEdge addedEdge;
        public BaseNode removedNode;//节点
        public BaseNode addedNode;
        public BaseNode nodeChanged;
        public Group addedGroups;//分组
        public Group removedGroups;
        public BaseStackNode addedStackNode;//堆栈
        public BaseStackNode removedStackNode;
        public StickyNote addedStickyNotes;//“便签”
        public StickyNote removedStickyNotes;
    }

    /// <summary>
    /// Compute order type used to determine the compute order integer on the nodes
    /// 计算顺序类型（深度 or 广度）
    /// </summary>
    public enum ComputeOrderType
    {
        DepthFirst,//深度优先
        BreadthFirst,//广度优先
    }

    /// <summary>
    /// 对于图表graph中的元素信息储存，对元素操作方法的实现，以及其他相关功能方法
    /// </summary>
	[System.Serializable]
    public class BaseGraph : ScriptableObject, ISerializationCallbackReceiver//序列化接口
    {
        static readonly int maxComputeOrderDepth = 1000;

        /// <summary>Invalid compute order number of a node when it's inside a loop 节点在循环内时的无效计算顺序号</summary>
        public static readonly int loopComputeOrder = -2;
        /// <summary>Invalid compute order number of a node can't process 不能处理的节点的无效计算顺序号</summary>
        public static readonly int invalidComputeOrder = -1;

        /// <summary>
        /// Json list of serialized nodes only used for copy pasting in the editor. Note that this field isn't serialized
        /// 仅用于在编辑器中复制粘贴的序列化节点的 Json 列表。 请注意，此字段未序列化
        /// </summary>
        /// <typeparam name="JsonElement"></typeparam>
        /// <returns></returns>
        [SerializeField, Obsolete("Use BaseGraph.nodes instead")]//Obsolete特性：标记不再使用的程序元素。 将元素标记为已过时通知用户，在产品的未来版本中可能会删除该元素。
        public List<JsonElement> serializedNodes = new List<JsonElement>();//no use

        /// <summary>
        /// List of all the nodes in the graph.在图表graph中储存所有节点的列表
        /// </summary>
        /// <typeparam name="BaseNode"></typeparam>
        /// <returns></returns>
        [SerializeReference]
        public List<BaseNode> nodes = new List<BaseNode>();

        /// <summary>
        /// Dictionary to access node per GUID, faster than a search in a list
        /// 字典（GUID, BaseNode),比在列表中查找更快
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="BaseNode"></typeparam>
        /// <returns></returns>
        [System.NonSerialized]
        public Dictionary<string, BaseNode> nodesPerGUID = new Dictionary<string, BaseNode>();

        /// <summary>
        /// Json list of edges  (序列化)edge的Json列表
        /// </summary>
        /// <typeparam name="SerializableEdge"></typeparam>
        /// <returns></returns>
        [SerializeField]
        public List<SerializableEdge> edges = new List<SerializableEdge>();
        /// <summary>
        /// Dictionary of edges per GUID, faster than a search in a list
        /// 字典(GUID，SerializableEdge)
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="SerializableEdge"></typeparam>
        /// <returns></returns>
        [System.NonSerialized]
        public Dictionary<string, SerializableEdge> edgesPerGUID = new Dictionary<string, SerializableEdge>();

        /// <summary>
        /// All groups in the graph  图表graph中储存所有group的列表
        /// </summary>
        /// <typeparam name="Group"></typeparam>
        /// <returns></returns>
        [SerializeField, FormerlySerializedAs("commentBlocks")]
        public List<Group> groups = new List<Group>();

        /// <summary>
        /// All Stack Nodes in the graph  图表graph中储存所有BaseStackNode的列表
        /// </summary>
        /// <typeparam name="stackNodes"></typeparam>
        /// <returns></returns>
        [SerializeField, SerializeReference] // Polymorphic serialization
        public List<BaseStackNode> stackNodes = new List<BaseStackNode>();

        /// <summary>
        /// All pinned elements in the graph  图表graph中储存所有PinnedElement的列表
        /// </summary>
        /// <typeparam name="PinnedElement"></typeparam>
        /// <returns></returns>
        [SerializeField]
        public List<PinnedElement> pinnedElements = new List<PinnedElement>();

        /// <summary>
        /// All exposed parameters in the graph  图表graph中储存所有ExposedParameter的列表
        /// </summary>
        /// <typeparam name="ExposedParameter"></typeparam>
        /// <returns></returns>
        [SerializeField, SerializeReference]
        public List<ExposedParameter> exposedParameters = new List<ExposedParameter>();

        ///FormerlySerializedAs：使用此属性重命名一个字段，同时不丢失其序列化的值。  oldname：exposedParameters
		[SerializeField, FormerlySerializedAs("exposedParameters")] // We keep this for upgrade
        List<ExposedParameter> serializedParameterList = new List<ExposedParameter>();

        [SerializeField]
        public List<StickyNote> stickyNotes = new List<StickyNote>();

        //计算顺序的字典
        [System.NonSerialized]
        Dictionary<BaseNode, int> computeOrderDictionary = new Dictionary<BaseNode, int>();

        [NonSerialized]
        Scene linkedScene;//图表graph链接到的场景Scene

        // Trick to keep the node inspector alive during the editor session 在编辑器会话期间保持节点检视器处于活动状态的技巧
        [SerializeField]
        internal UnityEngine.Object nodeInspectorReference;

        //graph visual properties
        public Vector3 position = Vector3.zero;
        public Vector3 scale = Vector3.one;

        /// <summary>
        /// Triggered when something is changed in the list of exposed parameters
        /// 当公开参数列表中的某些内容发生更改时触发
        /// </summary>
        public event Action onExposedParameterListChanged;
        public event Action<ExposedParameter> onExposedParameterModified;
        public event Action<ExposedParameter> onExposedParameterValueChanged;

        /// <summary>
        /// Triggered when the graph is linked to an active scene.当图表链接到活动场景时触发
        /// </summary>
        public event Action<Scene> onSceneLinked;

        /// <summary>
        /// Triggered when the graph is enabled  当图表启用时触发
        /// </summary>
        public event Action onEnabled;

        /// <summary>
        /// Triggered when the graph is changed 当图表改变时触发
        /// </summary>
        public event Action<GraphChanges> onGraphChanges;

        [System.NonSerialized]
        bool _isEnabled = false;
        public bool isEnabled { get => _isEnabled; private set => _isEnabled = value; }

        /// <summary>
        /// 集：储存图表graph中的end nodes（用edge连接起来的“节点链”中的最后一个节点）
        /// </summary>
        public HashSet<BaseNode> graphOutputs { get; private set; } = new HashSet<BaseNode>();


        /// <summary>
        /// 创建NodeGraph资产时调用（每次编译完成和进出PlayMode时也会调用）：
        ///     若需要则迁移图表；初始化Graph中的节点和连线；销毁“损坏”的edge和node元素；更新图中节点的计算顺序(深度优先(默认) or 广度优先)；
        ///     调用相关方法
        /// </summary>
        protected virtual void OnEnable()
        {
            if (isEnabled)
                OnDisable();

            MigrateGraphIfNeeded();//若需要则迁移图表
            InitializeGraphElements();//*初始化Graph中的节点和连线
            DestroyBrokenGraphElements();//销毁“损坏”的edge和node元素
            UpdateComputeOrder();//更新图中节点的计算顺序(深度优先(默认) or 广度优先)
            isEnabled = true;
            onEnabled?.Invoke();
        }

        /// <summary>
        /// 初始化Graph中的节点和连线
        /// </summary>
		void InitializeGraphElements()
        {
            // Sanitize the element lists (it's possible that nodes are null if their full class name have changed)
            //清理元素列表（如果节点的完整类名已更改，则节点可能为空）
            // If you rename / change the assembly of a node or parameter, please use the MovedFrom() attribute to avoid breaking the graph.
            //如果您重命名/更改节点或参数的程序集，请使用 MovedFrom() 属性以避免破坏图形。

            nodes.RemoveAll(n => n == null);//清除所有空的node
            exposedParameters.RemoveAll(e => e == null);//清除所有空的exposedParameters

            foreach (var node in nodes.ToList())
            {
                nodesPerGUID[node.GUID] = node;//填充字典
                node.Initialize(this);//*注入Graph依赖，关联节点与图表，初始化node及其ports
            }

            foreach (var edge in edges.ToList())
            {
                edge.Deserialize();
                edgesPerGUID[edge.GUID] = edge;

                // Sanity check for the edge: 连线的健全性检查：
                if (edge.inputPort == null || edge.outputPort == null)
                {
                    Disconnect(edge.GUID);
                    continue;
                }

                // Add the edge to the non-serialized port data 将连线添加到非序列化端口数据
                edge.inputPort.owner.OnEdgeConnected(edge);
                edge.outputPort.owner.OnEdgeConnected(edge);
            }
        }

        protected virtual void OnDisable()
        {
            isEnabled = false;
            foreach (var node in nodes)
                node.DisableInternal();
        }

        public virtual void OnAssetDeleted() { }

        /// <summary>
        /// Adds a node to the graph  在图表中添加一个节点(记录节点信息)
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public BaseNode AddNode(BaseNode node)
        {
            nodesPerGUID[node.GUID] = node;

            nodes.Add(node);//加入节点列表
            node.Initialize(this);

            onGraphChanges?.Invoke(new GraphChanges { addedNode = node });

            return node;
        }

        /// <summary>
        /// Removes a node from the graph  从图表中移除一个节点
        /// </summary>
        /// <param name="node"></param>
        public void RemoveNode(BaseNode node)
        {
            node.DisableInternal();//将输入/输出端口容器清空，调用Disable()
            node.DestroyInternal();//调用Destroy()

            //移除列表中对应元素
            nodesPerGUID.Remove(node.GUID);//移除列表中对应元素
            nodes.Remove(node);

            onGraphChanges?.Invoke(new GraphChanges { removedNode = node });
        }

        /// <summary>
        /// Connect two ports with an edge  
        /// 新建一条(包含相关连线信息的)SerializableEdge来连接两个端口，并将此SerializableEdge记录到graph中的edges列表中
        /// </summary>
        /// <param name="inputPort">input port</param>
        /// <param name="outputPort">output port</param>
        /// <param name="DisconnectInputs">is the edge allowed to disconnect another edge</param>
        /// <returns>the connecting edge 连接的edge</returns>
        public SerializableEdge Connect(NodePort inputPort, NodePort outputPort, bool autoDisconnectInputs = true)
        {
            var edge = SerializableEdge.CreateNewEdge(this, inputPort, outputPort);//创建一条新的连线，并赋予其相关连线信息

            //If the input port does not support multi-connection, we remove them
            //如果输入端口不支持edge的多重连接，就移除edge
            if (autoDisconnectInputs && !inputPort.portData.acceptMultipleEdges)
            {
                foreach (var e in inputPort.GetEdges().ToList())
                {
                    // TODO: do not disconnect them if the connected port is the same than the old connected
                    //如果连接的端口与旧连接的端口相同，请不要断开它们
                    Disconnect(e);
                }
            }
            // same for the output port:
            if (autoDisconnectInputs && !outputPort.portData.acceptMultipleEdges)
            {
                foreach (var e in outputPort.GetEdges().ToList())
                {
                    // TODO: do not disconnect them if the connected port is the same than the old connected
                    Disconnect(e);
                }
            }

            edges.Add(edge);///*向edges中添加元素

            // Add the edge to the list of connected edges in the nodes 
            // 将此edge添加到节点的已连接的列表中
            inputPort.owner.OnEdgeConnected(edge);
            outputPort.owner.OnEdgeConnected(edge);

            onGraphChanges?.Invoke(new GraphChanges { addedEdge = edge });

            return edge;
        }

        /// <summary>
        /// Disconnect two ports  断开两个端口链接
        /// </summary>
        /// <param name="inputNode">input node</param>
        /// <param name="inputFieldName">input field name</param>
        /// <param name="outputNode">output node</param>
        /// <param name="outputFieldName">output field name</param>
        public void Disconnect(BaseNode inputNode, string inputFieldName, BaseNode outputNode, string outputFieldName)
        {
            edges.RemoveAll(r => {
                bool remove = r.inputNode == inputNode
                && r.outputNode == outputNode
                && r.outputFieldName == outputFieldName
                && r.inputFieldName == inputFieldName;

                if (remove)
                {
                    r.inputNode?.OnEdgeDisconnected(r);
                    r.outputNode?.OnEdgeDisconnected(r);
                    onGraphChanges?.Invoke(new GraphChanges { removedEdge = r });
                }

                return remove;
            });
        }

        /// <summary>
        /// Disconnect an edge  断开一条连线
        /// </summary>
        /// <param name="edge"></param>
        public void Disconnect(SerializableEdge edge) => Disconnect(edge.GUID);

        /// <summary>
        /// Disconnect an edge 断开一条连线
        /// </summary>
        /// <param name="edgeGUID"></param>
        public void Disconnect(string edgeGUID)
        {
            List<(BaseNode, SerializableEdge)> disconnectEvents = new List<(BaseNode, SerializableEdge)>();

            edges.RemoveAll(r => {
                if (r.GUID == edgeGUID)
                {
                    disconnectEvents.Add((r.inputNode, r));
                    disconnectEvents.Add((r.outputNode, r));
                    onGraphChanges?.Invoke(new GraphChanges { removedEdge = r });
                }
                return r.GUID == edgeGUID;
            });

            // Delay the edge disconnect event to avoid recursion
            // 延迟边缘断开事件以避免递归
            foreach (var (node, edge) in disconnectEvents)
                node?.OnEdgeDisconnected(edge);
        }

        /// <summary>
        /// Add a group  
        /// </summary>
        /// <param name="block"></param>
        public void AddGroup(Group block)
        {
            groups.Add(block);
            onGraphChanges?.Invoke(new GraphChanges { addedGroups = block });
        }

        /// <summary>
        /// Removes a group
        /// </summary>
        /// <param name="block"></param>
        public void RemoveGroup(Group block)
        {
            groups.Remove(block);
            onGraphChanges?.Invoke(new GraphChanges { removedGroups = block });
        }

        /// <summary>
        /// Add a StackNode
        /// </summary>
        /// <param name="stackNode"></param>
        public void AddStackNode(BaseStackNode stackNode)
        {
            stackNodes.Add(stackNode);
            onGraphChanges?.Invoke(new GraphChanges { addedStackNode = stackNode });
        }

        /// <summary>
        /// Remove a StackNode
        /// </summary>
        /// <param name="stackNode"></param>
        public void RemoveStackNode(BaseStackNode stackNode)
        {
            stackNodes.Remove(stackNode);
            onGraphChanges?.Invoke(new GraphChanges { removedStackNode = stackNode });
        }

        /// <summary>
        /// Add a sticky note 
        /// </summary>
        /// <param name="note"></param>
        public void AddStickyNote(StickyNote note)
        {
            stickyNotes.Add(note);
            onGraphChanges?.Invoke(new GraphChanges { addedStickyNotes = note });
        }

        /// <summary>
        /// Removes a sticky note 
        /// </summary>
        /// <param name="note"></param>
        public void RemoveStickyNote(StickyNote note)
        {
            stickyNotes.Remove(note);
            onGraphChanges?.Invoke(new GraphChanges { removedStickyNotes = note });
        }

        /// <summary>
        /// Invoke the onGraphChanges event, can be used as trigger to execute the graph when the content of a node is changed 
        /// 调用 onGraphChanges 事件，当节点内容发生变化时，可以作为触发器执行graph
        /// </summary>
        /// <param name="node"></param>
        public void NotifyNodeChanged(BaseNode node) => onGraphChanges?.Invoke(new GraphChanges { nodeChanged = node });

        /// <summary>
        /// Open a pinned element of type viewType  打开指定类型的PinnedElementView元素
        /// </summary>
        /// <param name="viewType">type of the pinned element  PinnedElementView或其子类</param>
        /// <returns>the pinned element</returns>
        public PinnedElement OpenPinned(Type viewType)
        {
            var pinned = pinnedElements.Find(p => p.editorType.type == viewType);

            if (pinned == null)
            {
                pinned = new PinnedElement(viewType);
                pinnedElements.Add(pinned);//添加到pinnedElements列表中
            }
            else
                pinned.opened = true;//置位opened

            return pinned;
        }

        /// <summary>
        /// Closes a pinned element of type viewType  关闭指定类型的PinnedElementView元素
        /// </summary>
        /// <param name="viewType">type of the pinned element PinnedElementView或其子类</param>
        public void ClosePinned(Type viewType)
        {
            var pinned = pinnedElements.Find(p => p.editorType.type == viewType);

            pinned.opened = false;//重置opened
        }

        public void OnBeforeSerialize()
        {
            // Cleanup broken elements  清除“损坏”的元素
            stackNodes.RemoveAll(s => s == null);
            nodes.RemoveAll(n => n == null);
        }

        // We can deserialize data here because it's called in a unity context
        // so we can load objects references
        // 我们可以在这里反序列化数据，因为它是在unity上下文中调用的，因此我们可以加载对象引用
        public void Deserialize()
        {
            // Disable nodes correctly before removing them: 在移除节点之前正确禁用节点
            if (nodes != null)
            {
                foreach (var node in nodes)
                    node.DisableInternal();
            }

            MigrateGraphIfNeeded();

            InitializeGraphElements();
        }

        /// <summary>
        /// 如果需要，迁移图表
        /// </summary>
		public void MigrateGraphIfNeeded()
        {
            ///警告禁用指令pragma warning
#pragma warning disable CS0618
            // Migration step from JSON serialized nodes to [SerializeReference] 
            // 从 JSON 序列化节点到 [SerializeReference] 的迁移步骤
            if (serializedNodes.Count > 0)
            {
                nodes.Clear();
                foreach (var serializedNode in serializedNodes.ToList())
                {
                    var node = JsonSerializer.DeserializeNode(serializedNode) as BaseNode;//将JsonElement反序列化成节点
                    if (node != null)
                        nodes.Add(node);//将serializedNodes列表中的节点迁移到nodes列表中
                }
                serializedNodes.Clear();

                // we also migrate parameters here: 我们还在这里迁移参数
                var paramsToMigrate = serializedParameterList.ToList();
                exposedParameters.Clear();
                foreach (var param in paramsToMigrate)
                {
                    if (param == null)
                        continue;

                    var newParam = param.Migrate();

                    if (newParam == null)
                    {
                        Debug.LogError($"Can't migrate parameter of type {param.type}, please create an Exposed Parameter class that implements this type.");
                        continue;
                    }
                    else
                        exposedParameters.Add(newParam);//将serializedParameterList列表中的参数迁移到exposedParameters列表中
                }
            }
#pragma warning restore CS0618
        }

        public void OnAfterDeserialize() { }

        /// <summary>
        /// Update the compute order of the nodes in the graph
        /// 更新图中节点的计算顺序(深度优先(默认) or 广度优先)
        /// </summary>
        /// <param name="type">Compute order type</param>
        public void UpdateComputeOrder(ComputeOrderType type = ComputeOrderType.DepthFirst)
        {
            if (nodes.Count == 0)
                return;

            // Find graph outputs (end nodes) and reset compute order
            // 找到图表中各个end node，并重置其计算顺序
            graphOutputs.Clear();//先清空
            foreach (var node in nodes)
            {
                if (node.GetOutputNodes().Count() == 0)//若连接到该节点输出端口的所有节点的数量为0(即为end node)，加入到graphOutputs列表中
                    graphOutputs.Add(node);
                node.computeOrder = 0;//设置此节点计算顺序为0
            }

            computeOrderDictionary.Clear();
            infiniteLoopTracker.Clear();

            switch (type)
            {
                default://默认深度优先
                case ComputeOrderType.DepthFirst:
                    UpdateComputeOrderDepthFirst();//按深度有限来更新计算顺序
                    break;
                case ComputeOrderType.BreadthFirst:
                    foreach (var node in nodes)
                        UpdateComputeOrderBreadthFirst(0, node);
                    break;
            }
        }

        /// <summary>
        /// Add an exposed parameter  添加一个ExposedParameter（名称，类型，默认值）
        /// </summary>
        /// <param name="name">parameter name</param>
        /// <param name="type">parameter type (must be a subclass of ExposedParameter)</param>
        /// <param name="value">default value 默认值</param>
        /// <returns>The unique id of the parameter 参数元素的GUID</returns>
        public string AddExposedParameter(string name, Type type, object value = null)
        {

            if (!type.IsSubclassOf(typeof(ExposedParameter)))//若不是ExposedParameter的子类
            {
                Debug.LogError($"Can't add parameter of type {type}, the type doesn't inherit from ExposedParameter.");
            }

            var param = Activator.CreateInstance(type) as ExposedParameter;

            // patch value with correct type:
            ///IsValueType：若是值类型则为true，否则false。  值类型：不是类或接口
            if (param.GetValueType().IsValueType)
                value = Activator.CreateInstance(param.GetValueType());

            param.Initialize(name, value);
            exposedParameters.Add(param);//添加到graph的列表中

            onExposedParameterListChanged?.Invoke();

            return param.guid;
        }

        /// <summary>
        /// Add an already allocated / initialized parameter to the graph
        /// 向图中添加已分配/初始化的参数
        /// </summary>
        /// <param name="parameter">The parameter to add</param>
        /// <returns>The unique id of the parameter</returns>
        public string AddExposedParameter(ExposedParameter parameter)
        {
            string guid = Guid.NewGuid().ToString(); // Generated once and unique per parameter 生成一次且每个参数唯一

            parameter.guid = guid;
            exposedParameters.Add(parameter);

            onExposedParameterListChanged?.Invoke();

            return guid;
        }

        /// <summary>
        /// Remove an exposed parameter 移除一个ExposedParameter
        /// </summary>
        /// <param name="ep">the parameter to remove</param>
        public void RemoveExposedParameter(ExposedParameter ep)
        {
            exposedParameters.Remove(ep);

            onExposedParameterListChanged?.Invoke();
        }

        /// <summary>
        /// Remove an exposed parameter 移除与指定GUID相等的ExposedParameter
        /// </summary>
        /// <param name="guid">GUID of the parameter</param>
        public void RemoveExposedParameter(string guid)
        {
            if (exposedParameters.RemoveAll(e => e.guid == guid) != 0)
                onExposedParameterListChanged?.Invoke();
        }

        internal void NotifyExposedParameterListChanged()
            => onExposedParameterListChanged?.Invoke();

        /// <summary>
        /// Update an exposed parameter value  更新一个与指定GUID相等的ExposedParameter的值
        /// </summary>
        /// <param name="guid">GUID of the parameter</param>
        /// <param name="value">new value</param>
        public void UpdateExposedParameter(string guid, object value)
        {
            var param = exposedParameters.Find(e => e.guid == guid);//查找GUID
            if (param == null)
                return;

            if (value != null && !param.GetValueType().IsAssignableFrom(value.GetType()))//类型不匹配
                throw new Exception("Type mismatch when updating parameter " + param.name + ": from " + param.GetValueType() + " to " + value.GetType().AssemblyQualifiedName);

            param.value = value;
            onExposedParameterModified?.Invoke(param);
        }

        /// <summary>
        /// Update the exposed parameter name  更新一个ExposedParameter的名称
        /// </summary>
        /// <param name="parameter">The parameter</param>
        /// <param name="name">new name</param>
        public void UpdateExposedParameterName(ExposedParameter parameter, string name)
        {
            parameter.name = name;
            onExposedParameterModified?.Invoke(parameter);
        }

        /// <summary>
        /// Update parameter visibility 更新一个ExposedParameter的可见性
        /// </summary>
        /// <param name="parameter">The parameter</param>
        /// <param name="isHidden">is Hidden</param>
        public void NotifyExposedParameterChanged(ExposedParameter parameter)
        {
            onExposedParameterModified?.Invoke(parameter);
        }

        public void NotifyExposedParameterValueChanged(ExposedParameter parameter)
        {
            onExposedParameterValueChanged?.Invoke(parameter);
        }

        /// <summary>
        /// Get the exposed parameter from name  按照指定名称获取一个ExposedParameter，没有则返回null
        /// </summary>
        /// <param name="name">name</param>
        /// <returns>the parameter or null</returns>
        public ExposedParameter GetExposedParameter(string name)
        {
            return exposedParameters.FirstOrDefault(e => e.name == name);
        }

        /// <summary>
        /// Get exposed parameter from GUID  按照指定的GUID获取一个ExposedParameter
        /// </summary>
        /// <param name="guid">GUID of the parameter</param>
        /// <returns>The parameter</returns>
        public ExposedParameter GetExposedParameterFromGUID(string guid)
        {
            return exposedParameters.FirstOrDefault(e => e?.guid == guid);//返回序列中满足条件的第一个元素；如果未找到这样的元素，则返回默认值。
        }

        /// <summary>
        /// Set parameter value from name. (Warning: the parameter name can be changed by the user)
        /// 按照指定名称查找一个ExposedParameter，并设置其值（警告：参数名可以被用户修改）
        /// </summary>
        /// <param name="name">name of the parameter</param>
        /// <param name="value">new value</param>
        /// <returns>true if the value have been assigned</returns>
        public bool SetParameterValue(string name, object value)
        {
            var e = exposedParameters.FirstOrDefault(p => p.name == name);//查找名称相同的第一个ExposedParameter

            if (e == null)
                return false;

            e.value = value;

            return true;
        }

        /// <summary>
        /// Get the parameter value  按照指定名称获取一个ExposedParameter的值
        /// </summary>
        /// <param name="name">parameter name</param>
        /// <returns>value</returns>
        public object GetParameterValue(string name) => exposedParameters.FirstOrDefault(p => p.name == name)?.value;

        /// <summary>
        /// Get the parameter value template  
        /// </summary>
        /// <param name="name">parameter name</param>
        /// <typeparam name="T">type of the parameter</typeparam>
        /// <returns>value</returns>
        public T GetParameterValue<T>(string name) => (T)GetParameterValue(name);

        /// <summary>
        /// Link the current graph to the scene in parameter, allowing the graph to pick and serialize objects from the scene.
        /// 将当前图表graph链接到参数中的场景，允许图表从场景中挑选和序列化对象
        /// </summary>
        /// <param name="scene">Target scene to link</param>
        public void LinkToScene(Scene scene)
        {
            linkedScene = scene;
            onSceneLinked?.Invoke(scene);
        }

        /// <summary>
        /// Return true when the graph is linked to a scene, false otherwise.
        /// 当图表graph链接到有效场景时返回 true，否则返回 false
        /// </summary>
        public bool IsLinkedToScene() => linkedScene.IsValid();//是否为有效场景

        /// <summary>
        /// Get the linked scene. If there is no linked scene, it returns an invalid scene
        /// 获取链接的场景。 如果没有链接场景，则返回无效场景
        /// </summary>
        public Scene GetLinkedScene() => linkedScene;

        HashSet<BaseNode> infiniteLoopTracker = new HashSet<BaseNode>();
        /// <summary>
        /// 更新计算顺序：广度优先
        /// </summary>
        /// <param name="depth"></param>
        /// <param name="node"></param>
        /// <returns></returns>
		int UpdateComputeOrderBreadthFirst(int depth, BaseNode node)
        {
            int computeOrder = 0;

            if (depth > maxComputeOrderDepth)//最大递归1000
            {
                Debug.LogError("Recursion error while updating compute order");
                return -1;
            }

            if (computeOrderDictionary.ContainsKey(node))//若字典中的键包含此节点，就返回此节点的计算顺序
                return node.computeOrder;

            if (!infiniteLoopTracker.Add(node))//若集中已有此节点，直接返回
                return -1;

            if (!node.canProcess)//若此节点不能被处理，设置其计算顺序为-1，直接返回
            {
                node.computeOrder = -1;
                computeOrderDictionary[node] = -1;
                return -1;
            }

            foreach (var dep in node.GetInputNodes())//遍历连接到此节点输入端口的所有节点
            {
                int c = UpdateComputeOrderBreadthFirst(depth + 1, dep);//递归

                if (c == -1)
                {
                    computeOrder = -1;
                    break;
                }

                computeOrder += c;
            }

            if (computeOrder != -1)
                computeOrder++;

            node.computeOrder = computeOrder;
            computeOrderDictionary[node] = computeOrder;

            return computeOrder;
        }

        /// <summary>更新计算顺序：深度优先</summary>
		void UpdateComputeOrderDepthFirst()
        {
            Stack<BaseNode> dfs = new Stack<BaseNode>();

            GraphUtils.FindCyclesInGraph(this, (n) => {
                PropagateComputeOrder(n, loopComputeOrder);//将指定节点n及其(输出端口连接的)后续节点的计算顺序均设置为指定值loopComputeOrder
            });

            int computeOrder = 0;
            foreach (var node in GraphUtils.DepthFirstSort(this))//按照列表中的顺序依次增加节点的计算顺序
            {
                if (node.computeOrder == loopComputeOrder)//若该节点为无效计算顺序
                    continue;
                if (!node.canProcess)//若该节点不能被处理
                    node.computeOrder = -1;
                else
                    node.computeOrder = computeOrder++;//计算顺序增加
            }
        }

        /// <summary>
        /// 传播计算顺序：将指定参数节点及其(输出端口连接的)后续节点的计算顺序均设置为指定的参数值
        /// </summary>
        /// <param name="node"></param>
        /// <param name="computeOrder"></param>
		void PropagateComputeOrder(BaseNode node, int computeOrder)
        {
            Stack<BaseNode> deps = new Stack<BaseNode>();
            HashSet<BaseNode> loop = new HashSet<BaseNode>();

            deps.Push(node);//先将指定参数节点入栈
            while (deps.Count > 0)
            {
                var n = deps.Pop();//参数节点出栈，并获取它
                n.computeOrder = computeOrder;//设置参数节点的计算顺序为指定的参数值

                if (!loop.Add(n))//若能添加到集中为true，若元素已存在为false
                    continue;

                foreach (var dep in n.GetOutputNodes())//将 连接到该节点输出端口的所有节点 全部入栈
                    deps.Push(dep);
            }
        }

        //销毁“损坏”的edge和node元素
        void DestroyBrokenGraphElements()
        {
            //移除不完整的edge
            edges.RemoveAll(e => e.inputNode == null
                || e.outputNode == null
                || string.IsNullOrEmpty(e.outputFieldName)
                || string.IsNullOrEmpty(e.inputFieldName)
            );
            //在节点列表中移除为null的节点
            nodes.RemoveAll(n => n == null);
        }

        /// <summary>
        /// Tell if two types can be connected in the context of a graph 
        /// 判断两种类型是否可以在图的上下文中连接
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public static bool TypesAreConnectable(Type t1, Type t2)
        {
            if (t1 == null || t2 == null)
                return false;

            if (TypeAdapter.AreIncompatible(t1, t2))
                return false;

            //Check if there is custom adapters for this assignation 检查此分配是否有自定义适配器
            if (CustomPortIO.IsAssignable(t1, t2))
                return true;

            //Check for type assignability 检查类型可分配性
            if (t2.IsReallyAssignableFrom(t1))
                return true;

            // User defined type convertions 用户定义的类型转换
            if (TypeAdapter.AreAssignable(t1, t2))
                return true;

            return false;
        }
    }
}