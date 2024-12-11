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
        public BaseNode removedNode;//�ڵ�
        public BaseNode addedNode;
        public BaseNode nodeChanged;
        public Group addedGroups;//����
        public Group removedGroups;
        public BaseStackNode addedStackNode;//��ջ
        public BaseStackNode removedStackNode;
        public StickyNote addedStickyNotes;//����ǩ��
        public StickyNote removedStickyNotes;
    }

    /// <summary>
    /// Compute order type used to determine the compute order integer on the nodes
    /// ����˳�����ͣ���� or ��ȣ�
    /// </summary>
    public enum ComputeOrderType
    {
        DepthFirst,//�������
        BreadthFirst,//�������
    }

    /// <summary>
    /// ����ͼ��graph�е�Ԫ����Ϣ���棬��Ԫ�ز���������ʵ�֣��Լ�������ع��ܷ���
    /// </summary>
	[System.Serializable]
    public class BaseGraph : ScriptableObject, ISerializationCallbackReceiver//���л��ӿ�
    {
        static readonly int maxComputeOrderDepth = 1000;

        /// <summary>Invalid compute order number of a node when it's inside a loop �ڵ���ѭ����ʱ����Ч����˳���</summary>
        public static readonly int loopComputeOrder = -2;
        /// <summary>Invalid compute order number of a node can't process ���ܴ���Ľڵ����Ч����˳���</summary>
        public static readonly int invalidComputeOrder = -1;

        /// <summary>
        /// Json list of serialized nodes only used for copy pasting in the editor. Note that this field isn't serialized
        /// �������ڱ༭���и���ճ�������л��ڵ�� Json �б� ��ע�⣬���ֶ�δ���л�
        /// </summary>
        /// <typeparam name="JsonElement"></typeparam>
        /// <returns></returns>
        [SerializeField, Obsolete("Use BaseGraph.nodes instead")]//Obsolete���ԣ���ǲ���ʹ�õĳ���Ԫ�ء� ��Ԫ�ر��Ϊ�ѹ�ʱ֪ͨ�û����ڲ�Ʒ��δ���汾�п��ܻ�ɾ����Ԫ�ء�
        public List<JsonElement> serializedNodes = new List<JsonElement>();//no use

        /// <summary>
        /// List of all the nodes in the graph.��ͼ��graph�д������нڵ���б�
        /// </summary>
        /// <typeparam name="BaseNode"></typeparam>
        /// <returns></returns>
        [SerializeReference]
        public List<BaseNode> nodes = new List<BaseNode>();

        /// <summary>
        /// Dictionary to access node per GUID, faster than a search in a list
        /// �ֵ䣨GUID, BaseNode),�����б��в��Ҹ���
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="BaseNode"></typeparam>
        /// <returns></returns>
        [System.NonSerialized]
        public Dictionary<string, BaseNode> nodesPerGUID = new Dictionary<string, BaseNode>();

        /// <summary>
        /// Json list of edges  (���л�)edge��Json�б�
        /// </summary>
        /// <typeparam name="SerializableEdge"></typeparam>
        /// <returns></returns>
        [SerializeField]
        public List<SerializableEdge> edges = new List<SerializableEdge>();
        /// <summary>
        /// Dictionary of edges per GUID, faster than a search in a list
        /// �ֵ�(GUID��SerializableEdge)
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="SerializableEdge"></typeparam>
        /// <returns></returns>
        [System.NonSerialized]
        public Dictionary<string, SerializableEdge> edgesPerGUID = new Dictionary<string, SerializableEdge>();

        /// <summary>
        /// All groups in the graph  ͼ��graph�д�������group���б�
        /// </summary>
        /// <typeparam name="Group"></typeparam>
        /// <returns></returns>
        [SerializeField, FormerlySerializedAs("commentBlocks")]
        public List<Group> groups = new List<Group>();

        /// <summary>
        /// All Stack Nodes in the graph  ͼ��graph�д�������BaseStackNode���б�
        /// </summary>
        /// <typeparam name="stackNodes"></typeparam>
        /// <returns></returns>
        [SerializeField, SerializeReference] // Polymorphic serialization
        public List<BaseStackNode> stackNodes = new List<BaseStackNode>();

        /// <summary>
        /// All pinned elements in the graph  ͼ��graph�д�������PinnedElement���б�
        /// </summary>
        /// <typeparam name="PinnedElement"></typeparam>
        /// <returns></returns>
        [SerializeField]
        public List<PinnedElement> pinnedElements = new List<PinnedElement>();

        /// <summary>
        /// All exposed parameters in the graph  ͼ��graph�д�������ExposedParameter���б�
        /// </summary>
        /// <typeparam name="ExposedParameter"></typeparam>
        /// <returns></returns>
        [SerializeField, SerializeReference]
        public List<ExposedParameter> exposedParameters = new List<ExposedParameter>();

        ///FormerlySerializedAs��ʹ�ô�����������һ���ֶΣ�ͬʱ����ʧ�����л���ֵ��  oldname��exposedParameters
		[SerializeField, FormerlySerializedAs("exposedParameters")] // We keep this for upgrade
        List<ExposedParameter> serializedParameterList = new List<ExposedParameter>();

        [SerializeField]
        public List<StickyNote> stickyNotes = new List<StickyNote>();

        //����˳����ֵ�
        [System.NonSerialized]
        Dictionary<BaseNode, int> computeOrderDictionary = new Dictionary<BaseNode, int>();

        [NonSerialized]
        Scene linkedScene;//ͼ��graph���ӵ��ĳ���Scene

        // Trick to keep the node inspector alive during the editor session �ڱ༭���Ự�ڼ䱣�ֽڵ���������ڻ״̬�ļ���
        [SerializeField]
        internal UnityEngine.Object nodeInspectorReference;

        //graph visual properties
        public Vector3 position = Vector3.zero;
        public Vector3 scale = Vector3.one;

        /// <summary>
        /// Triggered when something is changed in the list of exposed parameters
        /// �����������б��е�ĳЩ���ݷ�������ʱ����
        /// </summary>
        public event Action onExposedParameterListChanged;
        public event Action<ExposedParameter> onExposedParameterModified;
        public event Action<ExposedParameter> onExposedParameterValueChanged;

        /// <summary>
        /// Triggered when the graph is linked to an active scene.��ͼ�����ӵ������ʱ����
        /// </summary>
        public event Action<Scene> onSceneLinked;

        /// <summary>
        /// Triggered when the graph is enabled  ��ͼ������ʱ����
        /// </summary>
        public event Action onEnabled;

        /// <summary>
        /// Triggered when the graph is changed ��ͼ��ı�ʱ����
        /// </summary>
        public event Action<GraphChanges> onGraphChanges;

        [System.NonSerialized]
        bool _isEnabled = false;
        public bool isEnabled { get => _isEnabled; private set => _isEnabled = value; }

        /// <summary>
        /// ��������ͼ��graph�е�end nodes����edge���������ġ��ڵ������е����һ���ڵ㣩
        /// </summary>
        public HashSet<BaseNode> graphOutputs { get; private set; } = new HashSet<BaseNode>();


        /// <summary>
        /// ����NodeGraph�ʲ�ʱ���ã�ÿ�α�����ɺͽ���PlayModeʱҲ����ã���
        ///     ����Ҫ��Ǩ��ͼ����ʼ��Graph�еĽڵ�����ߣ����١��𻵡���edge��nodeԪ�أ�����ͼ�нڵ�ļ���˳��(�������(Ĭ��) or �������)��
        ///     ������ط���
        /// </summary>
        protected virtual void OnEnable()
        {
            if (isEnabled)
                OnDisable();

            MigrateGraphIfNeeded();//����Ҫ��Ǩ��ͼ��
            InitializeGraphElements();//*��ʼ��Graph�еĽڵ������
            DestroyBrokenGraphElements();//���١��𻵡���edge��nodeԪ��
            UpdateComputeOrder();//����ͼ�нڵ�ļ���˳��(�������(Ĭ��) or �������)
            isEnabled = true;
            onEnabled?.Invoke();
        }

        /// <summary>
        /// ��ʼ��Graph�еĽڵ������
        /// </summary>
		void InitializeGraphElements()
        {
            // Sanitize the element lists (it's possible that nodes are null if their full class name have changed)
            //����Ԫ���б�����ڵ�����������Ѹ��ģ���ڵ����Ϊ�գ�
            // If you rename / change the assembly of a node or parameter, please use the MovedFrom() attribute to avoid breaking the graph.
            //�����������/���Ľڵ������ĳ��򼯣���ʹ�� MovedFrom() �����Ա����ƻ�ͼ�Ρ�

            nodes.RemoveAll(n => n == null);//������пյ�node
            exposedParameters.RemoveAll(e => e == null);//������пյ�exposedParameters

            foreach (var node in nodes.ToList())
            {
                nodesPerGUID[node.GUID] = node;//����ֵ�
                node.Initialize(this);//*ע��Graph�����������ڵ���ͼ����ʼ��node����ports
            }

            foreach (var edge in edges.ToList())
            {
                edge.Deserialize();
                edgesPerGUID[edge.GUID] = edge;

                // Sanity check for the edge: ���ߵĽ�ȫ�Լ�飺
                if (edge.inputPort == null || edge.outputPort == null)
                {
                    Disconnect(edge.GUID);
                    continue;
                }

                // Add the edge to the non-serialized port data ��������ӵ������л��˿�����
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
        /// Adds a node to the graph  ��ͼ�������һ���ڵ�(��¼�ڵ���Ϣ)
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public BaseNode AddNode(BaseNode node)
        {
            nodesPerGUID[node.GUID] = node;

            nodes.Add(node);//����ڵ��б�
            node.Initialize(this);

            onGraphChanges?.Invoke(new GraphChanges { addedNode = node });

            return node;
        }

        /// <summary>
        /// Removes a node from the graph  ��ͼ�����Ƴ�һ���ڵ�
        /// </summary>
        /// <param name="node"></param>
        public void RemoveNode(BaseNode node)
        {
            node.DisableInternal();//������/����˿�������գ�����Disable()
            node.DestroyInternal();//����Destroy()

            //�Ƴ��б��ж�ӦԪ��
            nodesPerGUID.Remove(node.GUID);//�Ƴ��б��ж�ӦԪ��
            nodes.Remove(node);

            onGraphChanges?.Invoke(new GraphChanges { removedNode = node });
        }

        /// <summary>
        /// Connect two ports with an edge  
        /// �½�һ��(�������������Ϣ��)SerializableEdge�����������˿ڣ�������SerializableEdge��¼��graph�е�edges�б���
        /// </summary>
        /// <param name="inputPort">input port</param>
        /// <param name="outputPort">output port</param>
        /// <param name="DisconnectInputs">is the edge allowed to disconnect another edge</param>
        /// <returns>the connecting edge ���ӵ�edge</returns>
        public SerializableEdge Connect(NodePort inputPort, NodePort outputPort, bool autoDisconnectInputs = true)
        {
            var edge = SerializableEdge.CreateNewEdge(this, inputPort, outputPort);//����һ���µ����ߣ������������������Ϣ

            //If the input port does not support multi-connection, we remove them
            //�������˿ڲ�֧��edge�Ķ������ӣ����Ƴ�edge
            if (autoDisconnectInputs && !inputPort.portData.acceptMultipleEdges)
            {
                foreach (var e in inputPort.GetEdges().ToList())
                {
                    // TODO: do not disconnect them if the connected port is the same than the old connected
                    //������ӵĶ˿�������ӵĶ˿���ͬ���벻Ҫ�Ͽ�����
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

            edges.Add(edge);///*��edges�����Ԫ��

            // Add the edge to the list of connected edges in the nodes 
            // ����edge��ӵ��ڵ�������ӵ��б���
            inputPort.owner.OnEdgeConnected(edge);
            outputPort.owner.OnEdgeConnected(edge);

            onGraphChanges?.Invoke(new GraphChanges { addedEdge = edge });

            return edge;
        }

        /// <summary>
        /// Disconnect two ports  �Ͽ������˿�����
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
        /// Disconnect an edge  �Ͽ�һ������
        /// </summary>
        /// <param name="edge"></param>
        public void Disconnect(SerializableEdge edge) => Disconnect(edge.GUID);

        /// <summary>
        /// Disconnect an edge �Ͽ�һ������
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
            // �ӳٱ�Ե�Ͽ��¼��Ա���ݹ�
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
        /// ���� onGraphChanges �¼������ڵ����ݷ����仯ʱ��������Ϊ������ִ��graph
        /// </summary>
        /// <param name="node"></param>
        public void NotifyNodeChanged(BaseNode node) => onGraphChanges?.Invoke(new GraphChanges { nodeChanged = node });

        /// <summary>
        /// Open a pinned element of type viewType  ��ָ�����͵�PinnedElementViewԪ��
        /// </summary>
        /// <param name="viewType">type of the pinned element  PinnedElementView��������</param>
        /// <returns>the pinned element</returns>
        public PinnedElement OpenPinned(Type viewType)
        {
            var pinned = pinnedElements.Find(p => p.editorType.type == viewType);

            if (pinned == null)
            {
                pinned = new PinnedElement(viewType);
                pinnedElements.Add(pinned);//��ӵ�pinnedElements�б���
            }
            else
                pinned.opened = true;//��λopened

            return pinned;
        }

        /// <summary>
        /// Closes a pinned element of type viewType  �ر�ָ�����͵�PinnedElementViewԪ��
        /// </summary>
        /// <param name="viewType">type of the pinned element PinnedElementView��������</param>
        public void ClosePinned(Type viewType)
        {
            var pinned = pinnedElements.Find(p => p.editorType.type == viewType);

            pinned.opened = false;//����opened
        }

        public void OnBeforeSerialize()
        {
            // Cleanup broken elements  ������𻵡���Ԫ��
            stackNodes.RemoveAll(s => s == null);
            nodes.RemoveAll(n => n == null);
        }

        // We can deserialize data here because it's called in a unity context
        // so we can load objects references
        // ���ǿ��������ﷴ���л����ݣ���Ϊ������unity�������е��õģ�������ǿ��Լ��ض�������
        public void Deserialize()
        {
            // Disable nodes correctly before removing them: ���Ƴ��ڵ�֮ǰ��ȷ���ýڵ�
            if (nodes != null)
            {
                foreach (var node in nodes)
                    node.DisableInternal();
            }

            MigrateGraphIfNeeded();

            InitializeGraphElements();
        }

        /// <summary>
        /// �����Ҫ��Ǩ��ͼ��
        /// </summary>
		public void MigrateGraphIfNeeded()
        {
            ///�������ָ��pragma warning
#pragma warning disable CS0618
            // Migration step from JSON serialized nodes to [SerializeReference] 
            // �� JSON ���л��ڵ㵽 [SerializeReference] ��Ǩ�Ʋ���
            if (serializedNodes.Count > 0)
            {
                nodes.Clear();
                foreach (var serializedNode in serializedNodes.ToList())
                {
                    var node = JsonSerializer.DeserializeNode(serializedNode) as BaseNode;//��JsonElement�����л��ɽڵ�
                    if (node != null)
                        nodes.Add(node);//��serializedNodes�б��еĽڵ�Ǩ�Ƶ�nodes�б���
                }
                serializedNodes.Clear();

                // we also migrate parameters here: ���ǻ�������Ǩ�Ʋ���
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
                        exposedParameters.Add(newParam);//��serializedParameterList�б��еĲ���Ǩ�Ƶ�exposedParameters�б���
                }
            }
#pragma warning restore CS0618
        }

        public void OnAfterDeserialize() { }

        /// <summary>
        /// Update the compute order of the nodes in the graph
        /// ����ͼ�нڵ�ļ���˳��(�������(Ĭ��) or �������)
        /// </summary>
        /// <param name="type">Compute order type</param>
        public void UpdateComputeOrder(ComputeOrderType type = ComputeOrderType.DepthFirst)
        {
            if (nodes.Count == 0)
                return;

            // Find graph outputs (end nodes) and reset compute order
            // �ҵ�ͼ���и���end node�������������˳��
            graphOutputs.Clear();//�����
            foreach (var node in nodes)
            {
                if (node.GetOutputNodes().Count() == 0)//�����ӵ��ýڵ�����˿ڵ����нڵ������Ϊ0(��Ϊend node)�����뵽graphOutputs�б���
                    graphOutputs.Add(node);
                node.computeOrder = 0;//���ô˽ڵ����˳��Ϊ0
            }

            computeOrderDictionary.Clear();
            infiniteLoopTracker.Clear();

            switch (type)
            {
                default://Ĭ���������
                case ComputeOrderType.DepthFirst:
                    UpdateComputeOrderDepthFirst();//��������������¼���˳��
                    break;
                case ComputeOrderType.BreadthFirst:
                    foreach (var node in nodes)
                        UpdateComputeOrderBreadthFirst(0, node);
                    break;
            }
        }

        /// <summary>
        /// Add an exposed parameter  ���һ��ExposedParameter�����ƣ����ͣ�Ĭ��ֵ��
        /// </summary>
        /// <param name="name">parameter name</param>
        /// <param name="type">parameter type (must be a subclass of ExposedParameter)</param>
        /// <param name="value">default value Ĭ��ֵ</param>
        /// <returns>The unique id of the parameter ����Ԫ�ص�GUID</returns>
        public string AddExposedParameter(string name, Type type, object value = null)
        {

            if (!type.IsSubclassOf(typeof(ExposedParameter)))//������ExposedParameter������
            {
                Debug.LogError($"Can't add parameter of type {type}, the type doesn't inherit from ExposedParameter.");
            }

            var param = Activator.CreateInstance(type) as ExposedParameter;

            // patch value with correct type:
            ///IsValueType������ֵ������Ϊtrue������false��  ֵ���ͣ��������ӿ�
            if (param.GetValueType().IsValueType)
                value = Activator.CreateInstance(param.GetValueType());

            param.Initialize(name, value);
            exposedParameters.Add(param);//��ӵ�graph���б���

            onExposedParameterListChanged?.Invoke();

            return param.guid;
        }

        /// <summary>
        /// Add an already allocated / initialized parameter to the graph
        /// ��ͼ������ѷ���/��ʼ���Ĳ���
        /// </summary>
        /// <param name="parameter">The parameter to add</param>
        /// <returns>The unique id of the parameter</returns>
        public string AddExposedParameter(ExposedParameter parameter)
        {
            string guid = Guid.NewGuid().ToString(); // Generated once and unique per parameter ����һ����ÿ������Ψһ

            parameter.guid = guid;
            exposedParameters.Add(parameter);

            onExposedParameterListChanged?.Invoke();

            return guid;
        }

        /// <summary>
        /// Remove an exposed parameter �Ƴ�һ��ExposedParameter
        /// </summary>
        /// <param name="ep">the parameter to remove</param>
        public void RemoveExposedParameter(ExposedParameter ep)
        {
            exposedParameters.Remove(ep);

            onExposedParameterListChanged?.Invoke();
        }

        /// <summary>
        /// Remove an exposed parameter �Ƴ���ָ��GUID��ȵ�ExposedParameter
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
        /// Update an exposed parameter value  ����һ����ָ��GUID��ȵ�ExposedParameter��ֵ
        /// </summary>
        /// <param name="guid">GUID of the parameter</param>
        /// <param name="value">new value</param>
        public void UpdateExposedParameter(string guid, object value)
        {
            var param = exposedParameters.Find(e => e.guid == guid);//����GUID
            if (param == null)
                return;

            if (value != null && !param.GetValueType().IsAssignableFrom(value.GetType()))//���Ͳ�ƥ��
                throw new Exception("Type mismatch when updating parameter " + param.name + ": from " + param.GetValueType() + " to " + value.GetType().AssemblyQualifiedName);

            param.value = value;
            onExposedParameterModified?.Invoke(param);
        }

        /// <summary>
        /// Update the exposed parameter name  ����һ��ExposedParameter������
        /// </summary>
        /// <param name="parameter">The parameter</param>
        /// <param name="name">new name</param>
        public void UpdateExposedParameterName(ExposedParameter parameter, string name)
        {
            parameter.name = name;
            onExposedParameterModified?.Invoke(parameter);
        }

        /// <summary>
        /// Update parameter visibility ����һ��ExposedParameter�Ŀɼ���
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
        /// Get the exposed parameter from name  ����ָ�����ƻ�ȡһ��ExposedParameter��û���򷵻�null
        /// </summary>
        /// <param name="name">name</param>
        /// <returns>the parameter or null</returns>
        public ExposedParameter GetExposedParameter(string name)
        {
            return exposedParameters.FirstOrDefault(e => e.name == name);
        }

        /// <summary>
        /// Get exposed parameter from GUID  ����ָ����GUID��ȡһ��ExposedParameter
        /// </summary>
        /// <param name="guid">GUID of the parameter</param>
        /// <returns>The parameter</returns>
        public ExposedParameter GetExposedParameterFromGUID(string guid)
        {
            return exposedParameters.FirstOrDefault(e => e?.guid == guid);//�������������������ĵ�һ��Ԫ�أ����δ�ҵ�������Ԫ�أ��򷵻�Ĭ��ֵ��
        }

        /// <summary>
        /// Set parameter value from name. (Warning: the parameter name can be changed by the user)
        /// ����ָ�����Ʋ���һ��ExposedParameter����������ֵ�����棺���������Ա��û��޸ģ�
        /// </summary>
        /// <param name="name">name of the parameter</param>
        /// <param name="value">new value</param>
        /// <returns>true if the value have been assigned</returns>
        public bool SetParameterValue(string name, object value)
        {
            var e = exposedParameters.FirstOrDefault(p => p.name == name);//����������ͬ�ĵ�һ��ExposedParameter

            if (e == null)
                return false;

            e.value = value;

            return true;
        }

        /// <summary>
        /// Get the parameter value  ����ָ�����ƻ�ȡһ��ExposedParameter��ֵ
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
        /// ����ǰͼ��graph���ӵ������еĳ���������ͼ��ӳ�������ѡ�����л�����
        /// </summary>
        /// <param name="scene">Target scene to link</param>
        public void LinkToScene(Scene scene)
        {
            linkedScene = scene;
            onSceneLinked?.Invoke(scene);
        }

        /// <summary>
        /// Return true when the graph is linked to a scene, false otherwise.
        /// ��ͼ��graph���ӵ���Ч����ʱ���� true�����򷵻� false
        /// </summary>
        public bool IsLinkedToScene() => linkedScene.IsValid();//�Ƿ�Ϊ��Ч����

        /// <summary>
        /// Get the linked scene. If there is no linked scene, it returns an invalid scene
        /// ��ȡ���ӵĳ����� ���û�����ӳ������򷵻���Ч����
        /// </summary>
        public Scene GetLinkedScene() => linkedScene;

        HashSet<BaseNode> infiniteLoopTracker = new HashSet<BaseNode>();
        /// <summary>
        /// ���¼���˳�򣺹������
        /// </summary>
        /// <param name="depth"></param>
        /// <param name="node"></param>
        /// <returns></returns>
		int UpdateComputeOrderBreadthFirst(int depth, BaseNode node)
        {
            int computeOrder = 0;

            if (depth > maxComputeOrderDepth)//���ݹ�1000
            {
                Debug.LogError("Recursion error while updating compute order");
                return -1;
            }

            if (computeOrderDictionary.ContainsKey(node))//���ֵ��еļ������˽ڵ㣬�ͷ��ش˽ڵ�ļ���˳��
                return node.computeOrder;

            if (!infiniteLoopTracker.Add(node))//���������д˽ڵ㣬ֱ�ӷ���
                return -1;

            if (!node.canProcess)//���˽ڵ㲻�ܱ��������������˳��Ϊ-1��ֱ�ӷ���
            {
                node.computeOrder = -1;
                computeOrderDictionary[node] = -1;
                return -1;
            }

            foreach (var dep in node.GetInputNodes())//�������ӵ��˽ڵ�����˿ڵ����нڵ�
            {
                int c = UpdateComputeOrderBreadthFirst(depth + 1, dep);//�ݹ�

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

        /// <summary>���¼���˳���������</summary>
		void UpdateComputeOrderDepthFirst()
        {
            Stack<BaseNode> dfs = new Stack<BaseNode>();

            GraphUtils.FindCyclesInGraph(this, (n) => {
                PropagateComputeOrder(n, loopComputeOrder);//��ָ���ڵ�n����(����˿����ӵ�)�����ڵ�ļ���˳�������Ϊָ��ֵloopComputeOrder
            });

            int computeOrder = 0;
            foreach (var node in GraphUtils.DepthFirstSort(this))//�����б��е�˳���������ӽڵ�ļ���˳��
            {
                if (node.computeOrder == loopComputeOrder)//���ýڵ�Ϊ��Ч����˳��
                    continue;
                if (!node.canProcess)//���ýڵ㲻�ܱ�����
                    node.computeOrder = -1;
                else
                    node.computeOrder = computeOrder++;//����˳������
            }
        }

        /// <summary>
        /// ��������˳�򣺽�ָ�������ڵ㼰��(����˿����ӵ�)�����ڵ�ļ���˳�������Ϊָ���Ĳ���ֵ
        /// </summary>
        /// <param name="node"></param>
        /// <param name="computeOrder"></param>
		void PropagateComputeOrder(BaseNode node, int computeOrder)
        {
            Stack<BaseNode> deps = new Stack<BaseNode>();
            HashSet<BaseNode> loop = new HashSet<BaseNode>();

            deps.Push(node);//�Ƚ�ָ�������ڵ���ջ
            while (deps.Count > 0)
            {
                var n = deps.Pop();//�����ڵ��ջ������ȡ��
                n.computeOrder = computeOrder;//���ò����ڵ�ļ���˳��Ϊָ���Ĳ���ֵ

                if (!loop.Add(n))//������ӵ�����Ϊtrue����Ԫ���Ѵ���Ϊfalse
                    continue;

                foreach (var dep in n.GetOutputNodes())//�� ���ӵ��ýڵ�����˿ڵ����нڵ� ȫ����ջ
                    deps.Push(dep);
            }
        }

        //���١��𻵡���edge��nodeԪ��
        void DestroyBrokenGraphElements()
        {
            //�Ƴ���������edge
            edges.RemoveAll(e => e.inputNode == null
                || e.outputNode == null
                || string.IsNullOrEmpty(e.outputFieldName)
                || string.IsNullOrEmpty(e.inputFieldName)
            );
            //�ڽڵ��б����Ƴ�Ϊnull�Ľڵ�
            nodes.RemoveAll(n => n == null);
        }

        /// <summary>
        /// Tell if two types can be connected in the context of a graph 
        /// �ж����������Ƿ������ͼ��������������
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

            //Check if there is custom adapters for this assignation ���˷����Ƿ����Զ���������
            if (CustomPortIO.IsAssignable(t1, t2))
                return true;

            //Check for type assignability ������Ϳɷ�����
            if (t2.IsReallyAssignableFrom(t1))
                return true;

            // User defined type convertions �û����������ת��
            if (TypeAdapter.AreAssignable(t1, t2))
                return true;

            return false;
        }
    }
}