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
        internal string nodeCustomName = null; // The name of the node in case it was renamed by a user �ڵ�����ƣ��Է������û�������

        /// <summary>
        /// Name of the node, it will be displayed in the title section  
        /// �ڵ����ƣ�����ʾ�ڱ��ⲿ��
        /// </summary>
        /// <returns></returns>
        public virtual string name => GetType().Name;

        /// <summary>
        /// The accent color of the node
        /// �ڵ��ǿ��ɫ
        /// </summary>
        public virtual Color color => Color.clear;

        /// <summary>
        /// Set a custom uss file for the node. We use a Resources.Load to get the stylesheet so be sure to put the correct resources path
        /// Ϊ�ڵ������Զ��� uss �ļ��� ����ʹ�� Resources.Load ����ȡ��ʽ�������ȷ��������ȷ����Դ·��
        /// https://docs.unity3d.com/ScriptReference/Resources.Load.html
        /// </summary>
        public virtual string layoutStyle => string.Empty;

        /// <summary>
        /// If the node can be locked or not
        /// �ڵ��Ƿ��ܱ���ס
        /// </summary>
        public virtual bool unlockable => true;

        /// <summary>
        /// Is the node is locked (if locked it can't be moved)
        /// </summary>
        public virtual bool isLocked => nodeLock;

        //id
        public string GUID;
        /// <summary>
        /// ͼ����ʱ�ļ���˳��
        /// </summary>
		public int computeOrder = -1;

        /// <summary>
        /// Tell wether or not the node can be processed. Do not check anything from inputs because this step happens before inputs are sent to the node
        /// �ж��Ƿ���Դ���ڵ㡣 ��Ҫ��������е��κ����ݣ���Ϊ�˲��跢�������뷢�͵��ڵ�֮ǰ
        /// </summary>
        public virtual bool canProcess => true;

        /// <summary>
        /// Show the node controlContainer only when the mouse is over the node
        /// ���������ͣ�ڽڵ���ʱ����ʾ�ڵ��controlContainer
        /// </summary>
        public virtual bool showControlsOnHover => false;

        /// <summary>
        /// True if the node can be deleted, false otherwise
        /// �������ɾ���ڵ㣬��Ϊ true������Ϊ false
        /// </summary>
        public virtual bool deletable => true;

        /// <summary>
        /// Container of input ports ����˿ڵ�����
        /// </summary>
        [NonSerialized]
        public readonly NodeInputPortContainer inputPorts;
        /// <summary>
        /// Container of output ports  ����˿ڵ�����
        /// </summary>
        [NonSerialized]
        public readonly NodeOutputPortContainer outputPorts;

        //Node view datas
        public Rect position;
        /// <summary>
        /// Is the node expanded �ڵ��Ƿ�չ��
        /// </summary>
        public bool expanded;
        /// <summary>
        /// Is debug visible �����Ƿ�ɼ�
        /// </summary>
        public bool debug;
        /// <summary>
        /// Node locked state  �ڵ�����״̬
        /// </summary>
        public bool nodeLock;
        /// <summary>
        /// �ڵ�Ҫ�󶨵Ķ����guid
        /// </summary>
        public string bindGuid;

        public delegate void ProcessDelegate();

        /// <summary>
        /// Triggered when the node is processes �ڵ�Ϊ����ʱ����
        /// </summary>
        public event ProcessDelegate onProcessed;
        public event Action<string, NodeMessageType> onMessageAdded;
        public event Action<string> onMessageRemoved;
        /// <summary>
        /// Triggered after an edge was connected on the node  �����ߵ��ڵ�󱻴���
        /// </summary>
        public event Action<SerializableEdge> onAfterEdgeConnected;
        /// <summary>
        /// Triggered after an edge was disconnected on the node  ���ڵ��ϵ����߱��Ͽ��󴥷�
        /// </summary>
        public event Action<SerializableEdge> onAfterEdgeDisconnected;

        /// <summary>
        /// Triggered after a single/list of port(s) is updated, the parameter is the field name
        /// ���µ���/�˿��б�󴥷�������Ϊ�ֶ�����
        /// </summary>
        public event Action<string> onPortsUpdated;

        [NonSerialized]
        bool _needsInspector = false;

        /// <summary>
        /// Does the node needs to be visible in the inspector (when selected).  
        /// ������ѡ��ʱ���ڵ��Ƿ���Ҫ�ڼ�����пɼ�
        /// </summary>
        public virtual bool needsInspector => _needsInspector;

        /// <summary>
        /// Can the node be renamed in the UI. By default a node can be renamed by double clicking it's name.
        /// �Ƿ������ UI ���������ڵ㡣 Ĭ������£�����ͨ��˫���ڵ��������������ڵ㡣
        /// </summary>
        public virtual bool isRenamable => false;

        /// <summary>
        /// Is the node created from a duplicate operation (either ctrl-D or copy/paste).
        /// �Ǵ��ظ�������ctrl-D ����/ճ���������Ľڵ㡣
        /// </summary>
        public bool createdFromDuplication { get; internal set; } = false;

        /// <summary>
        /// True only when the node was created from a duplicate operation and is inside a group that was also duplicated at the same time. 
        /// �����ڵ��ǴӸ��Ʋ��������Ĳ���λ��ͬʱ���Ƶ�group��ʱ��Ϊ�档
        /// </summary>
        public bool createdWithinGroup { get; internal set; } = false;//��internal��ֻ����һ����������ܷ��ʵ��˱���

        /// <summary>�ֵ䣺�ڵ��ֶ���Ϣ��Ҫ�߱�����/������ԣ� </summary>
		[NonSerialized]
        internal Dictionary<string, NodeFieldInformation> nodeFields = new Dictionary<string, NodeFieldInformation>();

        [NonSerialized]
        internal Dictionary<Type, CustomPortTypeBehaviorDelegate> customPortTypeBehaviorMap = new Dictionary<Type, CustomPortTypeBehaviorDelegate>();

        [NonSerialized]
        List<string> messages = new List<string>();

        [NonSerialized]
        protected BaseGraph graph;

        /// <summary>
        /// �ڵ��ֶ���Ϣ��8����Ϣ��
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
            /// ���캯�����ڵ��ֶ���Ϣ��7��������8����Ϣ��
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
        /// �洢һ���ڵ����������ֶ�����(�б�)
        /// </summary>
		struct PortUpdate
        {
            public List<string> fieldNames;//һ���ڵ�����ж���ֶΣ�����port��
            public BaseNode node;

            public void Deconstruct(out List<string> fieldNames, out BaseNode node)
            {
                fieldNames = this.fieldNames;
                node = this.node;
            }
        }

        // Used in port update algorithm ���ڶ˿ڸ����㷨
        Stack<PortUpdate> fieldsToUpdate = new Stack<PortUpdate>();//��ջ������Ԫ������ΪPortUpdate Push/Pop/Peek
        HashSet<PortUpdate> updatedFields = new HashSet<PortUpdate>();//��ʾֵ�ļ���HashSet<T>���ṩ�����ܼ�������(���ǲ������ظ�Ԫ�صļ��ϣ���Ԫ��û���ض�˳��)

        /// <summary>
        /// Creates a node of type T at a certain position ��ĳ��λ�ô���һ������Ϊ T �Ľڵ�
        /// </summary>
        /// <param name="position">position in the graph in pixels</param>
        /// <typeparam name="T">type of the node</typeparam>
        /// <returns>the node instance</returns>
        public static T CreateFromType<T>(Vector2 position) where T : BaseNode
        {
            return CreateFromType(typeof(T), position) as T;
        }

        /// <summary>
        /// Creates a node of type nodeType at a certain position ��һ������λ�ô���һ��nodetype�Ľڵ�
        /// </summary>
        /// <param name="position">position in the graph in pixels</param>
        /// <typeparam name="nodeType">type of the node</typeparam>
        /// <returns>the node instance</returns>
        public static BaseNode CreateFromType(Type nodeType, Vector2 position)
        {
            if (!nodeType.IsSubclassOf(typeof(BaseNode)))//�˽ڵ����Ͳ���BaseNode������
                return null;

            //Activator�������ض��ķ����������ڱ��ػ��Զ�̴����������ͣ����ȡ������Զ�̶�������á� ���಻�ܱ��̳�
            //CreateInstance��ʹ�����͵��޲������캯������ָ�����͵�ʵ��
            var node = Activator.CreateInstance(nodeType) as BaseNode;//����nodetype�Ķ��󣬲����ض��������

            node.position = new Rect(position, new Vector2(100, 100));

            ExceptionToLog.Call(() => node.OnNodeCreated());//����һ��Guid

            return node;
        }

        #region Initialization

        // called by the BaseGraph when the node is added to the graph ���ڵ㱻�����ͼ��ʱ������
        public void Initialize(BaseGraph graph)
        {
            this.graph = graph;

            ExceptionToLog.Call(() => Enable());//�����ParameterNode�ͻ�������Ӧ��ExposedParameter; ��BaseNode�Ļ�Enable()Ϊ�գ����������п�������

            InitializePorts();//��ͨ��nodeFields������Ϣ����ʼ��port������port�򿪵�CreationMenuƥ����ȷ�ڵ�����Ҳ����������


        }

        /// <summary>
        /// ��ʼ���Զ���˿����ͷ���
        /// </summary>
		void InitializeCustomPortTypeMethods()
        {
            MethodInfo[] methods = new MethodInfo[0];//����
            Type baseType = GetType();//��ȡ��ǰ��type
            while (true)
            {
                methods = baseType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);//GetMethods()��������������дʱ��ʹ��ָ����Լ��������Ϊ��ǰ Type ����ĳ�Ա���������ǹ���������ʵ��������
                foreach (var method in methods)
                {
                    var typeBehaviors = method.GetCustomAttributes<CustomPortTypeBehavior>().ToArray();//GetCustomAttributes:����Ӧ����ָ����Ա��ָ�����͵��Զ�������

                    if (typeBehaviors.Length == 0)
                        continue;

                    CustomPortTypeBehaviorDelegate deleg = null;
                    try
                    {
                        //                               ����������ί�е����ͣ� ί��Ҫ�󶨵Ķ��� ��ʾ�ľ�̬��ʵ������      
                        deleg = Delegate.CreateDelegate(typeof(CustomPortTypeBehaviorDelegate), this, method) as CustomPortTypeBehaviorDelegate;//ָ�����͵�ί�У���ʾָ���ľ�̬��ʵ������
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
                if (baseType == null)//ֱ���ҵ����Ļ����(����null)���˳�ѭ��
                    break;
            }
        }

        /// <summary>
        /// Use this function to initialize anything related to ports generation in your node
        /// This will allow the node creation menu to correctly recognize ports that can be connected between nodes
        /// ʹ�ô˺�����ʼ���ڵ�����˿�������ص��κ�����
        /// �⽫����ڵ㴴���˵���ȷʶ������ڽڵ�֮�����ӵĶ˿�
        /// </summary>
        public virtual void InitializePorts()
        {
            InitializeCustomPortTypeMethods();

            foreach (var key in OverrideFieldOrder(nodeFields.Values.Select(k => k.info)))
            {
                var nodeField = nodeFields[key.Name];

                if (HasCustomBehavior(nodeField))//���Զ�����Ϊ
                {
                    UpdatePortsForField(nodeField.fieldName, sendPortUpdatedEvent: false);
                }
                else
                {
                    // If we don't have a custom behavior on the node, we just have to create a simple port
                    //���û���ڽڵ������Զ�����Ϊ��ֻ��Ҫ����һ���򵥵Ķ˿�
                    AddPort(nodeField.input, nodeField.fieldName, new PortData { acceptMultipleEdges = nodeField.isMultiple, displayName = nodeField.name, tooltip = nodeField.tooltip, vertical = nodeField.vertical });
                }
            }
        }

        /// <summary>
        /// Override the field order inside the node. It allows to re-order all the ports and field in the UI.
        /// ���ǽڵ��ڵ��ֶ�˳�� �������������� UI �е����ж˿ں��ֶ�
        /// </summary>
        /// <param name="fields">List of fields to sort</param>
        /// <returns>Sorted list of fields ������ֶ��б� </returns>
        public virtual IEnumerable<FieldInfo> OverrideFieldOrder(IEnumerable<FieldInfo> fields)
        {
            long GetFieldInheritanceLevel(FieldInfo f)//��ȡ�ֶμ̳м��𣺼�������Ļ���̳п�ʼ�ǵڼ�����
            {
                int level = 0;
                var t = f.DeclaringType;//DeclaringType����ȡ�����ó�Ա����
                while (t != null)
                {
                    t = t.BaseType;//BaseType����ȡ��ǰ Type ֱ�Ӵ��м̳е����ͣ����������һ���ĸ��ࣩ
                    level++;
                }

                return level;//�ڼ���
            }

            // Order by MetadataToken and inheritance level to sync the order with the port order (make sure FieldDrawers are next to the correct port)
            //�� MetadataToken �ͼ̳м��������Խ�˳����˿�˳��ͬ����ȷ�� FieldDrawers λ����ȷ�Ķ˿��Աߣ�
            //OrderByDescending�����ݼ�����������е�Ԫ�ؽ�������
            //�����ȼ̳м�����Metadata�ķ�ʽ���������ֶ� ����> �̳м���Խ��Խ�����ࣩ��˳��Խǰ 
            return fields.OrderByDescending(f => (long)(((GetFieldInheritanceLevel(f) << 32)) | (long)f.MetadataToken));//MetadataToken����ȡһ��ֵ����ֵ��ʶԪ����Ԫ��
        }

        /// <summary>
        /// ���캯������ʼ��inputPorts/outputPorts
        /// </summary>
		protected BaseNode()
        {
            inputPorts = new NodeInputPortContainer(this);
            outputPorts = new NodeOutputPortContainer(this);

            InitializeInOutDatas();//*���䣬����Attribu����In/Out������Ϣ�������浽nodeFields��

            if (bindGuid == null)
            {
                bindGuid = System.Guid.NewGuid().ToString();
            }
        }

        /// <summary>
        /// Update all ports of the node ���½ڵ������еĶ˿�
        /// </summary>
        public bool UpdateAllPorts()
        {
            bool changed = false;

            //Select������һ�� IEnumerable<T>����Ԫ���Ƕ� source(��nodeFields.Values) ��ÿ��Ԫ�ص���ת������(k => k.info)�õ��Ľ��
            foreach (var key in OverrideFieldOrder(nodeFields.Values.Select(k => k.info)))//����FieldInfo���͵�(���)��
            {
                var field = nodeFields[key.Name];
                changed |= UpdatePortsForField(field.fieldName);//������һ�� C# �����ֶ���صĶ˿ں�ͼ���������ӵĽڵ�
            }

            return changed;//�Ƿ�ȫ���������
        }

        /// <summary>
        /// Update all ports of the node without updating the connected ports. Only use this method when you need to update all the nodes ports in your graph.
        /// ���½ڵ�����ж˿ڣ������������ӵĶ˿ڡ� ������Ҫ����ͼ�е����нڵ�˿�ʱ��ʹ�ô˷�����
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
        /// ������һ�� C# �����ֶ���صĶ˿ڣ��������ڸýڵ㣩
        /// ����������
        ///     1.�ж�������/����˿ڣ����������ռ��˶˿ڵ������ֶκ�edge��Ϣ
        ///     2.��Ӷ˿����ݣ�ȷ�����յĶ˿��б����ڽ�һ������
        ///     3.ɾ�������ڵĶ˿�
        ///     4.ȷ���˿�˳����ȷ
        ///     5.���¶˿��б�
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

            var portCollection = fieldInfo.input ? (NodePortContainer)inputPorts : outputPorts;//�ж������/����˿�����

            // Gather all fields for this port (before to modify them) �ռ��˶˿ڵ������ֶΣ����޸�����֮ǰ��
            var nodePorts = portCollection.Where(p => p.fieldName == fieldName);
            // Gather all edges connected to these fields: �ռ����ӵ���Щ�ֶε����бߣ�
            var edges = nodePorts.SelectMany(n => n.GetEdges()).ToList();

            //��Ӷ˿����ݣ�ȷ�����յĶ˿��б����ڽ�һ������
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

            ///���˿ڲ�����������һ����
            ///���˿ڴ��ڣ��˿ڲ�������Ͽ����ߣ��޲��˿����ݣ���Ϣ��
            ///�����յĶ˿��б�����Ӵ˶˿ڵı�ʶ��
			void AddPortData(PortData portData)
            {
                var port = nodePorts.FirstOrDefault(n => n.portData.identifier == portData.identifier);
                // Guard using the port identifier so we don't duplicate identifiers
                if (port == null)//Ϊ��������һ��port
                {
                    AddPort(fieldInfo.input, fieldName, portData);
                    changed = true;
                }
                else
                {
                    // in case the port type have changed for an incompatible type, we disconnect all the edges attached to this port
                    //����˿������Ѹ���Ϊ�����ݵ����ͣ����ǽ��Ͽ����ӵ��ö˿ڵ����б�Ե
                    if (!BaseGraph.TypesAreConnectable(port.portData.displayType, portData.displayType))
                    {
                        foreach (var edge in port.GetEdges().ToList())
                            graph.Disconnect(edge.GUID);
                    }

                    // patch the port data �޲��˿�����
                    if (port.portData != portData)
                    {
                        port.portData.CopyFrom(portData);
                        changed = true;
                    }
                }

                finalPorts.Add(portData.identifier);//�����յĶ˿��б�����ӱ�ʶ��
            }

            // TODO
            // Remove only the ports that are no more in the list ��ɾ���б��в��ٴ��ڵĶ˿�
            if (nodePorts != null)
            {
                var currentPortsCopy = nodePorts.ToList();
                foreach (var currentPort in currentPortsCopy)
                {
                    // If the current port does not appear in the list of final ports, we remove it
                    // �����ǰ�˿�û�г��������ն˿��б��У����ǽ���ɾ��
                    if (!finalPorts.Any(id => id == currentPort.portData.identifier))
                    {
                        RemovePort(fieldInfo.input, currentPort);
                        changed = true;
                    }
                }
            }

            // Make sure the port order is correct: ȷ���˿�˳����ȷ��
            portCollection.Sort((p1, p2) => {//Sort:ʹ��ָ���ıȽ����ԡ����� List<T> ���е�Ԫ�ؽ������򣨴�С����
                int p1Index = finalPorts.FindIndex(id => p1.portData.identifier == id);//FindIndex:������ָ��ν���������������ƥ���Ԫ�أ����������� List<T> �е�һ��ƥ��Ԫ�صĴ��㿪ʼ������
                int p2Index = finalPorts.FindIndex(id => p2.portData.identifier == id);

                if (p1Index == -1 || p2Index == -1)//FindIndexû�ҵ��򷵻�-1
                    return 0;

                return p1Index.CompareTo(p2Index);//����ʵ��(p1Index)��ָ���� 32 λ�з�������(p2Index)���бȽϲ����ض������ֵ��ָʾ
            });

            if (sendPortUpdatedEvent)
                onPortsUpdated?.Invoke(fieldName);//���µ���/�˿��б�󴥷�������Ϊ�ֶ�����

            return changed;
        }

        /// <summary>
        /// �ж��Ƿ����Զ������Ϊ�� 
        /// ��Ϊtrue����Ϊfalse
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
        /// ������һ�� C# �����ֶ���صĶ˿ں�ͼ���������ӵĽڵ�
        ///   �������̣����˽ڵ�Ķ�ջ�ͼ���գ����Բ����ֶκʹ˽ڵ�Ϊ���ݵ�Ԫ����ջ(��ʱ��ջ��Ԫ�ؽ�һ��)��
        ///        �ڵ�1��whileѭ���н���Ԫ�ص�����ͨ���˲����ֶζԽڵ�Ķ˿ڽ��и��£�ͨ���Զ˿ں�edge�����α�
        ///        ����ȡ���º����Ϣ�����ջ�С���ʱ���ڶ�ջԪ�ظ�����Ϊ0�����2��ѭ������Ԫ�ص�������Ϊ��
        ///        Ԫ���Ѹ��������뼯�е�Ԫ�ز���ȣ����ٽ���һ�θ��¡��ȵ���3�ν���ѭ��ʱ�����ڶ�ջ�뼯��
        ///        ��Ԫ����ȱ���������µĳ������Զ�ջ��Ԫ�ظ���Ϊ0������ѭ������ɴ˷����ĸ��¹��ܡ�
        /// </summary>
        /// <param name="fieldName"></param>
        public bool UpdatePortsForField(string fieldName, bool sendPortUpdatedEvent = true)
        {
            bool changed = false;

            ///�Ƚ��˽ڵ�Ķ�ջ�ͼ� ���
			fieldsToUpdate.Clear();//��ջ���
            updatedFields.Clear();//��ֵ�ļ����

            fieldsToUpdate.Push(new PortUpdate { fieldNames = new List<string>() { fieldName }, node = this });//�� (�����ֶ�, �˽ڵ�) ��ջ�� ջ�н�һ��Ԫ��

            // Iterate through all the ports that needs to be updated, following graph connection when the 
            // port is updated. This is required ton have type propagation multiple nodes that changes port types
            // are connected to each other (i.e. the relay node)
            //����������Ҫ���µĶ˿ڣ����¶˿�ʱ��ѭͼ�����ӡ� 
            //���Ǳ���ģ��������ʹ���������Ķ˿����͵Ľڵ��໥���ӣ����м̽ڵ㣩
            while (fieldsToUpdate.Count != 0)
            {
                //���������ջ��Ԫ�ص���
                var (fields, node) = fieldsToUpdate.Pop();//*��ΪPortUpdate���а���List<string>��BaseNode��������

                // Avoid updating twice a port  �����һ���˿ڸ�������
                if (updatedFields.Any((t) => t.node == node && fields.SequenceEqual(t.fieldNames)))//���Ӷ�ջ���������Ԫ���롰�����е�Ԫ�ؽ��бȽϣ���������һ��Ԫ����ȣ��������������������һ��ѭ��
                    continue;
                updatedFields.Add(new PortUpdate { fieldNames = fields, node = node });//���в���ȵ�Ԫ�أ��µ�Ԫ�أ����ͼ��뵽��������
                                                                                       ///�����е�Ԫ�ض��ǲ���ȵġ�

                foreach (var field in fields)//�����ֶ��б���һ�� ֻ��һ�������������ֶΣ�
                {
                    if (node.UpdatePortsForFieldLocal(field, sendPortUpdatedEvent))//�����¡��ýڵ��ϴ��ֶ���صĶ˿�
                    {
                        foreach (var port in node.IsFieldInput(field) ? (NodePortContainer)node.inputPorts : node.outputPorts)//�������ºõĶ˿�����
                        {
                            //�ҵ�������ֶζ�Ӧ�Ķ˿�
                            //���ǣ���ֱ�ӷ���
                            if (port.fieldName != field)
                                continue;
                            //�ǣ�
                            foreach (var edge in port.GetEdges())//�����˶˿������е�edge
                            {
                                //ע�⣺edge������˿����ڽڵ���edge������ڵ�
                                var edgeNode = (node.IsFieldInput(field)) ? edge.outputNode : edge.inputNode;//�ҵ���edge�ڸ����ֶ��¶�Ӧ�Ľڵ�
                                var fieldsWithBehavior = edgeNode.nodeFields.Values.Where(f => HasCustomBehavior(f)).Select(f => f.fieldName).ToList();//��ȡ�˽ڵ��Ͼ����Զ�����Ϊ���ֶ�
                                fieldsToUpdate.Push(new PortUpdate { fieldNames = fieldsWithBehavior, node = edgeNode });//�����¼������Ԫ�ط����ջ
                            }
                        }
                        changed = true;
                    }
                }
            }

            return changed;
        }

        HashSet<BaseNode> portUpdateHashSet = new HashSet<BaseNode>();

        /// <summary>������/����˿�������գ�����Disable()</summary>
        internal void DisableInternal()
        {
            // port containers are initialized in the OnEnable �˿�������
            inputPorts.Clear();
            outputPorts.Clear();

            ExceptionToLog.Call(() => Disable());
        }
        /// <summary>����Destroy()</summary>
		internal void DestroyInternal() => ExceptionToLog.Call(() => Destroy());

        /// <summary>
        /// Called only when the node is created, not when instantiated ���ڴ����ڵ�ʱ���ã���������ʵ����ʱ����
        /// �ڽڵ㴴��ʱ����һ��GUID
        /// </summary>
        public virtual void OnNodeCreated() => GUID = Guid.NewGuid().ToString();

        /// <summary>��ȡ�ڵ��ֶ�</summary>
		public virtual FieldInfo[] GetNodeFields()
            => GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);


        /// <summary>
        /// ��ʼ������/������ԵĽڵ��ֶ���Ϣ�����ֵ� nodeFields ������䣩
        /// </summary>
		void InitializeInOutDatas()
        {
            var fields = GetNodeFields();//��ȡ�ڵ��ֶ�
            var methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);//��ȡ����

            /* �����ֶΣ������������/����˿����Ե��ֶ���Ϣ�����Զ�����ΪΪnull */
            foreach (var field in fields)
            {
                //��ȡ�ֶεġ����ԡ���Ϣ
                var inputAttribute = field.GetCustomAttribute<InputAttribute>();
                var outputAttribute = field.GetCustomAttribute<OutputAttribute>();
                var tooltipAttribute = field.GetCustomAttribute<TooltipAttribute>();
                var showInInspector = field.GetCustomAttribute<ShowInInspector>();//�Ƿ��ڽڵ���ͼ����ʾ���ֶ�
                var vertical = field.GetCustomAttribute<VerticalAttribute>();
                bool isMultiple = false;
                bool input = false;
                string name = field.Name;
                string tooltip = null;

                if (showInInspector != null)
                    _needsInspector = true;

                /* ��������/����˿����Ծ����� */
                if (inputAttribute == null && outputAttribute == null)
                    continue;

                //check if field is a collection type ����ֶ��Ƿ�Ϊ��������
                isMultiple = (inputAttribute != null) ? inputAttribute.allowMultiple : (outputAttribute.allowMultiple);//�Ƿ�����һ���˿ڶ��edge
                input = inputAttribute != null;//trueΪ����˿ڣ�falseΪ����˿�
                tooltip = tooltipAttribute?.tooltip;//��ȡtooltip��Ϣ

                //�˿�����ʾ���ƾͻ�ȡ
                if (!String.IsNullOrEmpty(inputAttribute?.name))
                    name = inputAttribute.name;
                if (!String.IsNullOrEmpty(outputAttribute?.name))
                    name = outputAttribute.name;

                // By default we set the behavior to null, if the field have a custom behavior, it will be set in the loop just below
                // Ĭ����������ǽ���Ϊ����Ϊnull��������ֶ����Զ�����Ϊ�������������ѭ��������
                nodeFields[field.Name] = new NodeFieldInformation(field, name, input, isMultiple, tooltip, vertical != null, null);
            }

            /* ���������������ֶ���Ϣ�е��Զ�����Ϊ */
            foreach (var method in methods)
            {
                var customPortBehaviorAttribute = method.GetCustomAttribute<CustomPortBehaviorAttribute>();//����Ӧ����ָ����Ա��ָ������CustomPortBehaviorAttribute���Զ�������
                CustomPortBehaviorDelegate behavior = null;

                if (customPortBehaviorAttribute == null)
                    continue;

                // Check if custom port behavior function is valid 
                //����Զ���˿���Ϊ�����Ƿ���Ч
                try
                {
                    var referenceType = typeof(CustomPortBehaviorDelegate);
                    behavior = (CustomPortBehaviorDelegate)Delegate.CreateDelegate(referenceType, this, method, true);
                    //CreateDelegate��������ʾָ���ľ�̬������ʵ��������ָ�����͵�ί��
                    //    ��������Ҫ������ί�����ͣ� ί�б�ʾ�ķ����ĵ�һ���Ա����� ��ί��Ҫ��ʾ�ľ�̬��ʵ��������MethodInfo�� ����޷��� method ʱ�����쳣����Ϊ true������Ϊ false��
                    //    ���أ���ʾָ����̬������ʵ��������ָ�����͵�ί�У���� ���ĸ����� Ϊ false������ί���޷��󶨵� method����Ϊ null
                }
                catch
                {
                    Debug.LogError("The function " + method + " cannot be converted to the required delegate format: " + typeof(CustomPortBehaviorDelegate));
                }

                //���Ѱ������Զ������Ե��ֶ���������������Ϊί��
                if (nodeFields.ContainsKey(customPortBehaviorAttribute.fieldName))
                    nodeFields[customPortBehaviorAttribute.fieldName].behavior = behavior;
                else//���� �Զ���˿���Ϊ���ֶ�������Ч
                    Debug.LogError("Invalid field name for custom port behavior: " + method + ", " + customPortBehaviorAttribute.fieldName);
            }
        }

        #endregion

        #region Events and Processing

        public void OnEdgeConnected(SerializableEdge edge)
        {
            bool input = edge.inputNode == this;//�Ƿ�������ڵ�
            NodePortContainer portCollection = (input) ? (NodePortContainer)inputPorts : outputPorts;//�������������˿ڵ�����

            portCollection.Add(edge);

            UpdateAllPorts();//���´˽ڵ��ϵ����ж˿�

            onAfterEdgeConnected?.Invoke(edge);
        }

        protected virtual bool CanResetPort(NodePort port) => true;

        public void OnEdgeDisconnected(SerializableEdge edge)
        {
            if (edge == null)
                return;

            bool input = edge.inputNode == this;
            NodePortContainer portCollection = (input) ? (NodePortContainer)inputPorts : outputPorts;

            portCollection.Remove(edge);//�Ƴ���edge

            // Reset default values of input port: ��������˿�ΪĬ��ֵ
            bool haveConnectedEdges = edge.inputNode.inputPorts.Where(p => p.fieldName == edge.inputFieldName).Any(p => p.GetEdges().Count != 0);//�ýڵ��Ƿ������ӵ�edge
            if (edge.inputNode == this && !haveConnectedEdges && CanResetPort(edge.inputPort))
                edge.inputPort?.ResetToDefault();

            UpdateAllPorts();

            onAfterEdgeDisconnected?.Invoke(edge);
        }

        /// <summary>
        /// *����ͨ��edge��ͨ���ݣ�������صĴ�����
        /// </summary>
		public void OnProcess()
        {
            inputPorts.PullDatas();//��edge��������ȡ���ݵ�����˿�

            ExceptionToLog.Call(() => Process());//�����Զ��崦����

            InvokeOnProcessed();//���ý���ί��

            outputPorts.PushDatas();//������ݵ�edge�Ļ�����
        }

        public void InvokeOnProcessed() => onProcessed?.Invoke();

        /// <summary>
        /// Called when the node is enabled  �����ýڵ�ʱ����
        /// </summary>
        protected virtual void Enable() { }
        /// <summary>
        /// Called when the node is disabled �����ýڵ�ʱ����
        /// </summary>
        protected virtual void Disable() { }
        /// <summary>
        /// Called when the node is removed ���Ƴ��ڵ�ʱ����
        /// </summary>
        protected virtual void Destroy() { }

        /// <summary>
        /// Override this method to implement custom processing ��д�÷���ʵ���Զ��崦��
        /// </summary>
        protected virtual void Process() { }

        #endregion

        #region API and utils

        /// <summary>
        /// Add a port  ���һ���˿�
        /// </summary>
        /// <param name="input">is input port</param>
        /// <param name="fieldName">C# field name</param>
        /// <param name="portData">Data of the port</param>
        public void AddPort(bool input, string fieldName, PortData portData)
        {
            // Fixup port data info if needed: �����Ҫ���޸��˿�������Ϣ��
            if (portData.displayType == null)
                portData.displayType = nodeFields[fieldName].info.FieldType;

            if (input)
                inputPorts.Add(new NodePort(this, fieldName, portData));
            else
                outputPorts.Add(new NodePort(this, fieldName, portData));
        }

        /// <summary>
        /// Remove a port  �Ƴ�һ���˿�
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
        /// Remove port(s) from field name ���ֶ�������ɾ���˿�
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
        /// ��ȡ���ӵ��ýڵ�����˿ڵ����нڵ�
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
        /// ��ȡ���ӵ��ýڵ�����˿ڵ����нڵ�
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
        /// ������ڵ���������е�����ƥ��Ľڵ�
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
                if (depth > 2000)//��ջ�и�������2000������
                    break;

                if (condition(node))//����������������ȥ����
                    return node;

                foreach (var dep in node.GetInputNodes())//δ�ҵ��ͽ��˽ڵ������˵����нڵ�����ջ�н��в���
                    dependencies.Push(dep);
            }
            return null;
        }

        /// <summary>
        /// Get the port from field name and identifier  �����ֶ����ƺͱ�ʶ������ȡ�˿�
        /// </summary>
        /// <param name="fieldName">C# field name</param>
        /// <param name="identifier">Unique port identifier</param>
        /// <returns></returns>
        public NodePort GetPort(string fieldName, string identifier)
        {
            //Concat�������������С� ����inputPorts��outputPorts����������������
            return inputPorts.Concat(outputPorts).FirstOrDefault(p => {
                var bothNull = String.IsNullOrEmpty(identifier) && String.IsNullOrEmpty(p.portData.identifier);
                return p.fieldName == fieldName && (bothNull || identifier == p.portData.identifier);
            });
        }

        /// <summary>
        /// Return all the ports of the node  ���ش˽ڵ�����ж˿ڣ��������������
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
        /// Return all the connected edges of the node  ���ش˽ڵ����������
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SerializableEdge> GetAllEdges()
        {
            foreach (var port in GetAllPorts())
                foreach (var edge in port.GetEdges())
                    yield return edge;
        }

        /// <summary>
        /// Is the port an input    �ж��Ƿ�������˿�
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public bool IsFieldInput(string fieldName) => nodeFields[fieldName].input;

        /// <summary>
        /// Add a message on the node   �ڽڵ��������Ϣ
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
        /// Remove a message on the node  �ӽڵ����Ƴ���Ϣ
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
        /// Remove all messages on the node  ����ڵ��ϵ�������Ϣ
        /// </summary>
        public void ClearMessages()
        {
            foreach (var message in messages)
                onMessageRemoved?.Invoke(message);
            messages.Clear();
        }

        /// <summary>
        /// Set the custom name of the node. This is intended to be used by renamable nodes.
        /// ���ýڵ�� nodeCustomName�����ǹ����������ڵ�ʹ��
        /// This custom name will be serialized inside the node.
        /// ���Զ������ƽ��ڽڵ������л�
        /// </summary>
        /// <param name="customNodeName">New name of the node.</param>
        public void SetCustomName(string customName) => nodeCustomName = customName;

        /// <summary>
        /// Get the name of the node. If the node have a custom name (set using the UI by double clicking on the node title) then it will return this name first, otherwise it returns the value of the name field.
        /// ��ȡ�ڵ�����ơ� ����ڵ����Զ������ƣ�ͨ��˫���ڵ����ʹ�� UI ���ã�����ô�������ȷ��ش����ƣ����򷵻������ֶε�ֵ��
        /// </summary>
        /// <returns>The name of the node as written in the title</returns>
        public string GetCustomName() => String.IsNullOrEmpty(nodeCustomName) ? name : nodeCustomName;

        #endregion
    }
}
