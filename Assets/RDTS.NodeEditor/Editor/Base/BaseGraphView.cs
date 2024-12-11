using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Linq;
using System;
using UnityEditor.SceneManagement;
using System.Reflection;
using RDTS;

using Status = UnityEngine.UIElements.DropdownMenuAction.Status;
using Object = UnityEngine.Object;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// Base class to write a custom view for a node Ϊ�ڵ��д�Զ�����ͼ�Ļ��ࣨGraphͼ��Ļ��ࣩ��
    /// �̳д����ʵ�ֲ�ͬ�������ͼ����Ҫ�Ƕ�ͼ��ĳ�ʼ��������(������������/ճ�����˵��������/�Ƴ�Ԫ�ص�)�����л�/�����л��ȷ�����й���ʵ��
    /// </summary>
    public class BaseGraphView : GraphView, IDisposable
    {
        public delegate void ComputeOrderUpdatedDelegate();
        public delegate void NodeDuplicatedDelegate(BaseNode duplicatedNode, BaseNode newNode);

        /// <summary>
        /// Graph that owns of the node ӵ�нڵ��ͼ
        /// </summary>
        public BaseGraph graph;

        /// <summary>
        /// ��Ӧ�Ĵ���
        /// </summary>
        public BaseGraphWindow graphWindow;

        /// <summary>
        /// ��graph�нڵ�Ķ�����ֵ�
        /// </summary>
        public Dictionary<string, RDTSBehavior> bindObjPerGuid = new Dictionary<string, RDTSBehavior>();


        /// <summary>
        /// Connector listener that will create the edges between ports ���ڶ˿�֮�䴴����Ե�����������������������ߵ������Ϣ
        /// </summary> 
        public BaseEdgeConnectorListener connectorListener;

        /// <summary>
        /// List of all node views in the graph ͼ�������нڵ���ͼ���б�
        /// </summary>
        /// <typeparam name="BaseNodeView"></typeparam>
        /// <returns></returns>
        public List<BaseNodeView> nodeViews = new List<BaseNodeView>();

        /// <summary>
        /// Dictionary of the node views accessed view the node instance, faster than a Find in the node view list
        /// ���ʵĽڵ���ͼ�ֵ�鿴�ڵ�ʵ�������ڽڵ���ͼ�б��в��Ҹ���
        /// </summary>
        /// <typeparam name="BaseNode"></typeparam>
        /// <typeparam name="BaseNodeView"></typeparam>
        /// <returns></returns>
        public Dictionary<BaseNode, BaseNodeView> nodeViewsPerNode = new Dictionary<BaseNode, BaseNodeView>();

        /// <summary>
        /// List of all edge views in the graph ͼ��������������ͼ���б�
        /// </summary>
        /// <typeparam name="EdgeView"></typeparam>
        /// <returns></returns>
        public List<EdgeView> edgeViews = new List<EdgeView>();

        /// <summary>
        /// List of all group views in the graph ͼ��������group��ͼ���б�
        /// </summary>
        /// <typeparam name="GroupView"></typeparam>
        /// <returns></returns>
        public List<GroupView> groupViews = new List<GroupView>();

#if UNITY_2020_1_OR_NEWER
        /// <summary>
        /// List of all sticky note views in the graph ͼ�������б��ڵ���ͼ���б�
        /// </summary>
        /// <typeparam name="StickyNoteView"></typeparam>
        /// <returns></returns>
        public List<StickyNoteView> stickyNoteViews = new List<StickyNoteView>();
#endif

        /// <summary>
        /// List of all stack node views in the graph ͼ�������ж�ջ�ڵ���ͼ���б�
        /// </summary>
        /// <typeparam name="BaseStackNodeView"></typeparam>
        /// <returns></returns>
        public List<BaseStackNodeView> stackNodeViews = new List<BaseStackNodeView>();

        /// <summary>�ֵ䣨PinnedElementView�������࣬PinnedElementView��</summary>
		Dictionary<Type, PinnedElementView> pinnedElements = new Dictionary<Type, PinnedElementView>();

        CreateNodeMenuWindow createNodeMenu;

        /// <summary>
        /// Triggered just after the graph is initialized ��graph����ʼ���󴥷�
        /// </summary>
        public event Action initialized;

        /// <summary>
        /// Triggered just after the compute order of the graph is updated ��graph�ļ���˳����º���������
        /// </summary>
        public event ComputeOrderUpdatedDelegate computeOrderUpdated;

        // Safe event relay from BaseGraph (safe because you are sure to always point on a valid BaseGraph
        // when one of these events is called), a graph switch can occur between two call tho
        //���� BaseGraph �İ�ȫ�¼��м̣���ȫ����Ϊ��������Щ�¼�֮һʱ����ȷ��ʼ��ָ����Ч�� BaseGraph����
        //��������������֮�䷢��ͼ���л�
        /// <summary>
        /// Same event than BaseGraph.onExposedParameterListChanged �� BaseGraph.onExposedParameterListChanged 
        /// Safe event (not triggered in case the graph is null).
        /// �� BaseGraph.onExposedParameterListChanged �� BaseGraph.onExposedParameterListChanged ��ͬ���¼�
        /// ��ȫ�¼�����ͼ��Ϊ�յ�����²�����)
        /// </summary>
        public event Action onExposedParameterListChanged;

        /// <summary>
        /// Same event than BaseGraph.onExposedParameterModified 
        /// Safe event (not triggered in case the graph is null).
        /// �� BaseGraph.onExposedParameterModified ��ͬ���¼�
        /// ��ȫ�¼�����ͼ��Ϊ�յ�����²�����)
        /// </summary>
        public event Action<ExposedParameter> onExposedParameterModified;

        /// <summary>
        /// Triggered when a node is duplicated (crt-d) or copy-pasted (crtl-c/crtl-v)
        /// ��һ���ڵ㱻�ظ������ƻ�ճ��ʱ����
        /// </summary>
        public event NodeDuplicatedDelegate nodeDuplicated;

        /// <summary>
        /// Object to handle nodes that shows their UI in the inspector. ���ڴ����ڼ����������ʾ�� UI �Ľڵ�Ķ���
        /// </summary>
        [SerializeField]
        protected NodeInspectorObject nodeInspector
        {
            get
            {

                if (graph.nodeInspectorReference == null)
                    graph.nodeInspectorReference = CreateNodeInspectorObject();
                return graph.nodeInspectorReference as NodeInspectorObject;
            }
        }

        /// <summary>
        /// Workaround object for creating exposed parameter property fields.
        /// ���ڴ����������������ֶεĽ����������
        /// </summary>
        public ExposedParameterFieldFactory exposedParameterFactory { get; private set; }

        /// <summary>���л�graph</summary>
		public SerializedObject serializedGraph { get; private set; }

        Dictionary<Type, (Type nodeType, MethodInfo initalizeNodeFromObject)> nodeTypePerCreateAssetType = new Dictionary<Type, (Type, MethodInfo)>();

        /// <summary>
        /// ���캯���������ز�����ί�з�����ע���������Ļص�����ʼ���ٿ��������������ţ��������ڳ�ʼ��
        /// </summary>
        /// <param name="window"></param>
		public BaseGraphView(EditorWindow window)
        {
            ///serializeGraphElements���������л�ͼ��Ԫ�ص�ί��
            serializeGraphElements = SerializeGraphElementsCallback;//�������л�ͼ��graphԪ�أ��Ա�ʵ�ָ���/ճ�������������Ļص�
            ///canPasteSerializedData�����ڲ鿴���л������Ƿ����ճ����ί��
            canPasteSerializedData = CanPasteSerializedDataCallback;//ѯ�����л������Ƿ����ճ��
            ///unserializeAndPaste������ȡ�����л���ճ��Ԫ�ص�ί��
            unserializeAndPaste = UnserializeAndPasteCallback;//����ȡ�����л�ͼ��Ԫ�ز�������ӵ�ͼ�еĻص�
            ///graphViewChanged������ָʾ GraphView �еĸ��ģ�ͨ���ɲٿ���ִ�У���ί��
            graphViewChanged = GraphViewChangedCallback;//��ͼ�з����ض�����ʱʹ�õĻص�������� GraphViewChange
            ///viewTransformChanged����ͼ�任���ģ����ţ�ί��
            viewTransformChanged = ViewTransformChangedCallback;//��ͼ�任���Ļص�
            ///elementResized��Ԫ�ص�����Сί��
            elementResized = ElementResizedCallback;//GroupԪ�ص�����С�ص�

            RegisterCallback<KeyDownEvent>(KeyDownCallback);//�����ص����������桢��ݶ���ڵ�
            RegisterCallback<DragPerformEvent>(DragPerformedCallback);//�϶���Ϊ�Ļص����ɴӡ��ڰ塱�е�ExposedParameterField������Hierarchy��塢Project����ק������graph�У������λ�ô������µĽڵ�
            RegisterCallback<DragUpdatedEvent>(DragUpdatedCallback);//�϶�Ԫ�ظ��µĻص��������Ŀ���ָʾ�����¼������
            RegisterCallback<MouseDownEvent>(MouseDownCallback);//��갴�»ص�������������ر��������ô��ڣ����յ�ǰѡ�еĽڵ�����Ƿ���¼������
            RegisterCallback<MouseUpEvent>(MouseUpCallback);//���̧��ص������յ�ǰѡ�еĽڵ�����Ƿ���¼������

            InitializeManipulators();//��ʼ������ӣ�����ٿ���
            ///AddGrid();//����

            SetupZoom(0.1f, 2f);//���ţ������֣�

            Undo.undoRedoPerformed += ReloadView;//�������������ء��������������ͼ���ί�з���

            //������(����)
            createNodeMenu = ScriptableObject.CreateInstance<CreateNodeMenuWindow>();
            createNodeMenu.Initialize(this, window);

            this.StretchToParentSize();
        }

        /// <summary>�����ڵ������</summary>
		protected virtual NodeInspectorObject CreateNodeInspectorObject()
        {
            var inspector = ScriptableObject.CreateInstance<NodeInspectorObject>();
            inspector.name = "Node Inspector";//����������
            inspector.hideFlags = HideFlags.HideAndDontSave ^ HideFlags.NotEditable;

            return inspector;
        }



        #region Callbacks �ص�

        protected override bool canCopySelection
        {
            get { return selection.Any(e => e is BaseNodeView || e is GroupView); }
        }

        protected override bool canCutSelection
        {
            get { return selection.Any(e => e is BaseNodeView || e is GroupView); }
        }

        /// <summary>
        /// ���л�ͼ��graphԪ�صĻص�������ѡ�ж����JSON��ʽ����
        /// </summary>
        /// <param name="elements">Ҫ�������л���Ԫ�أ�ѡ�е�Ԫ��</param>
        /// <returns></returns>
		string SerializeGraphElementsCallback(IEnumerable<GraphElement> elements)
        {
            var data = new CopyPasteHelper();

            //��copiedNodes��copiedEdges�б������ [ѡ����] �ж�Ӧ��Nodes  �������ӵ�edges
            foreach (BaseNodeView nodeView in elements.Where(e => e is BaseNodeView))//����ѡ�еĽڵ�
            {
                data.copiedNodes.Add(JsonSerializer.SerializeNode(nodeView.nodeTarget));//���ڵ����л���JsonElement���ͣ�����ӵ�copiedNodes�б���
                foreach (var port in nodeView.nodeTarget.GetAllPorts())//�����˽ڵ����еĶ˿�
                {
                    if (port.portData.vertical)//����ֱ��ʾ
                    {
                        foreach (var edge in port.GetEdges())
                            data.copiedEdges.Add(JsonSerializer.Serialize(edge));//��edge���л���JsonElement���ͣ�����ӵ�copiedEdges�б���
                    }
                }
            }
            //��copiedGroups�б������ [ѡ����] ��Ӧ��Groups
            foreach (GroupView groupView in elements.Where(e => e is GroupView))//����ѡ�е�Group
                data.copiedGroups.Add(JsonSerializer.Serialize(groupView.group));//��Group���л���JsonElement���ͣ�����ӵ�copiedGroups�б���
            //��copiedEdges�б������ [ѡ����] ��Ӧ��Edges
            foreach (EdgeView edgeView in elements.Where(e => e is EdgeView))
                data.copiedEdges.Add(JsonSerializer.Serialize(edgeView.serializedEdge));//��edge���л���JsonElement���ͣ�����ӵ�copiedEdges�б���

            ClearSelection();//�����ѡ��

            return JsonUtility.ToJson(data, true);//JSON��ʽ�Ķ�������
            ///����1��Ҫת��Ϊ JSON ��ʽ�Ķ���
            ///����2�����Ϊ true�����ʽ�������ʵ�ֿɶ��ԡ����Ϊ false�����ʽ�������ʵ����С��С��Ĭ��Ϊ false
        }

        /// <summary>
        /// ���л������Ƿ����ճ��
        /// </summary>
        /// <param name="serializedData">���л���ͼ��Ԫ��</param>
        /// <returns></returns>
		bool CanPasteSerializedDataCallback(string serializedData)
        {
            try
            {
                return JsonUtility.FromJson(serializedData, typeof(CopyPasteHelper)) != null;//ͨ��JSON��ʾ��ʽ�������󣬷���object�����ʵ��
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// ȡ��ͼ��Ԫ�����л���������ӵ�ͼ�� �Ļص����ֱ�Խڵ㡢Group��Edge����Ԫ�ؽ��з����л���ʵ��Ԫ�ص�ճ��
        /// </summary>
        /// <param name="operationName">����/������ǩ�Ĳ�������</param>
        /// <param name="serializedData">���л�����</param>
		void UnserializeAndPasteCallback(string operationName, string serializedData)
        {
            var data = JsonUtility.FromJson<CopyPasteHelper>(serializedData);//ͨ��JSON��ʾ��ʽ��������

            RegisterCompleteObjectUndo(operationName);//ע�᳷��

            Dictionary<string, BaseNode> copiedNodesMap = new Dictionary<string, BaseNode>();//��GUID�� BaseNode��

            var unserializedGroups = data.copiedGroups.Select(g => JsonSerializer.Deserialize<Group>(g)).ToList();//��data�е�Group��JSON���ݷ����л���Group���͵��б�

            foreach (var serializedNode in data.copiedNodes)///����JSON��ʽ�Ľڵ�����
			{
                var node = JsonSerializer.DeserializeNode(serializedNode);//�����г�BaseNode

                if (node == null)
                    continue;

                string sourceGUID = node.GUID;
                graph.nodesPerGUID.TryGetValue(sourceGUID, out var sourceNode);//ͨ���������л��ڵ��GUID��ȡ����Ӧ�Ľڵ�
                                                                               //Call OnNodeCreated on the new fresh copied node �����µĸ��ƽڵ�ʱ����OnNodeCreated()����
                node.createdFromDuplication = true;//��λ��־λ���ǴӸ���/ճ�������Ľڵ�
                node.createdWithinGroup = unserializedGroups.Any(g => g.innerNodeGUIDs.Contains(sourceGUID));//�жϣ��Ƿ�����ͬһ��group�ڽ��и���/ճ����
                node.OnNodeCreated();
                //And move a bit the new node ������/ճ�����½ڵ�ʱ������λ����ԭ�ڵ��λһЩ����������
                node.position.position += new Vector2(20, 20);

                var newNodeView = AddNode(node);//���һ���ڵ���ͼ���У���BaseGraph������ӽڵ���Ϣ����BaseGraphView����ӿ��ӻ��Ľڵ�Ԫ�أ�

                // If the nodes were copied from another graph, then the source is null  
                // ����ڵ��Ǵ���һ��ͼ���и���/ճ�������ģ�����(�ڴ�ͼ���е�)��Դ�ǿյ�
                if (sourceNode != null)
                    nodeDuplicated?.Invoke(sourceNode, node);
                copiedNodesMap[sourceGUID] = node;//�����ֵ�

                //Select the new node 
                ///AddToSelection������ѡ�����Ԫ��
                AddToSelection(nodeViewsPerNode[node]);//ѡ�в�������
            }

            foreach (var group in unserializedGroups)///���������л����group
            {
                //Same than for node
                group.OnCreated();

                // try to centre the created node in the screen Ӧ���Ǵ�λ�Ա�������
                group.position.position += new Vector2(20, 20);

                var oldGUIDList = group.innerNodeGUIDs.ToList();
                group.innerNodeGUIDs.Clear();
                foreach (var guid in oldGUIDList)
                {
                    graph.nodesPerGUID.TryGetValue(guid, out var node);

                    // In case group was copied from another graph 
                    // ���group�Ǵ���һ��ͼ���Ƶ�
                    if (node == null)//��һ��graph�и��ƹ���˵�������graph��nodesPerGUID�ֵ����Ҳ�����Ӧ��Ԫ��
                    {
                        copiedNodesMap.TryGetValue(guid, out node);
                        group.innerNodeGUIDs.Add(node.GUID);
                    }
                    else
                    {
                        group.innerNodeGUIDs.Add(copiedNodesMap[guid].GUID);
                    }
                }

                AddGroup(group);
            }

            foreach (var serializedEdge in data.copiedEdges)///����JSON��ʽ��edge����
            {
                var edge = JsonSerializer.Deserialize<SerializableEdge>(serializedEdge);//�ȷ����л���SerializableEdge

                edge.Deserialize();

                // Find port of new nodes:  �ҵ��½ڵ�Ķ˿�
                copiedNodesMap.TryGetValue(edge.inputNode.GUID, out var oldInputNode);
                copiedNodesMap.TryGetValue(edge.outputNode.GUID, out var oldOutputNode);

                // We avoid to break the graph by replacing unique connections: ���Ǳ��� ͨ���滻Ψһ�������ƻ�graph
                if (oldInputNode == null && !edge.inputPort.portData.acceptMultipleEdges || !edge.outputPort.portData.acceptMultipleEdges)
                    continue;

                oldInputNode = oldInputNode ?? edge.inputNode;
                oldOutputNode = oldOutputNode ?? edge.outputNode;

                var inputPort = oldInputNode.GetPort(edge.inputPort.fieldName, edge.inputPortIdentifier);//��ȡedge������˿�
                var outputPort = oldOutputNode.GetPort(edge.outputPort.fieldName, edge.outputPortIdentifier);//��ȡedge������˿�

                var newEdge = SerializableEdge.CreateNewEdge(graph, inputPort, outputPort);//����һ����SerializableEdge

                if (nodeViewsPerNode.ContainsKey(oldInputNode) && nodeViewsPerNode.ContainsKey(oldOutputNode))//���Ѱ����˴�SerializableEdge�����������˿ڣ��򴴽���Ӧ��EdgeView����graph�л���
                {
                    var edgeView = CreateEdgeView();
                    edgeView.userData = newEdge;
                    edgeView.input = nodeViewsPerNode[oldInputNode].GetPortViewFromFieldName(newEdge.inputFieldName, newEdge.inputPortIdentifier);
                    edgeView.output = nodeViewsPerNode[oldOutputNode].GetPortViewFromFieldName(newEdge.outputFieldName, newEdge.outputPortIdentifier);

                    Connect(edgeView);
                }
            }
        }

        public virtual EdgeView CreateEdgeView()
        {
            return new EdgeView();
        }

        /// <summary>
        /// ����ָʾ GraphView �еĸ��ģ�ͨ���ɲٿ���ִ�У���ί�С����԰�ԭ�����ؽṹ��Ҳ�����޸��б��Ը��� GraphView ��Ҫִ�еĲ���
        /// ��ͼ�з����ض�����ʱʹ�õĻص�����ͼ����Ҫɾ����Ԫ�أ�node��edge�����д���
        /// </summary>
        /// <param name="changes">���Ľṹ</param>
        /// <returns></returns>
        GraphViewChange GraphViewChangedCallback(GraphViewChange changes)//GraphViewChange��ͼ�п������ص�һ�����
        {
            if (changes.elementsToRemove != null)///elementsToRemove������ɾ����Ԫ��(�б�)
            {
                RegisterCompleteObjectUndo("Remove Graph Elements");

                // Destroy priority of objects ���ٶ�������ȼ�
                // We need nodes to be destroyed first because we can have a destroy operation that uses node connections
                // ������Ҫ�������ٽڵ㣬��Ϊ���ǿ��Խ���ʹ�ýڵ����ӵ����ٲ���
                changes.elementsToRemove.Sort((e1, e2) => {
                    int GetPriority(GraphElement e)//��ȡ���ȼ���BaseNodeView����Ԫ��Ϊ0
                    {
                        if (e is BaseNodeView)
                            return 0;
                        else
                            return 1;
                    }
                    ///CompareTo������ʵ����ָ���� 32 λ�з����������бȽϲ����ض������ֵ��ָʾ����-1ΪС�ڣ�0Ϊ���ڣ�1Ϊ���ڣ�
					return GetPriority(e1).CompareTo(GetPriority(e2));
                });

                //Handle ourselves the edge and node remove ��������edge�ͽڵ�ɾ��
                changes.elementsToRemove.RemoveAll(e => {

                    switch (e)
                    {
                        case EdgeView edge:
                            Disconnect(edge);//�Ͽ�EdgeView�����ӣ�ɾ��EdgeView�����¼���˳�򡢶˿���ͼ
                            return true;
                        case BaseNodeView nodeView:
                            // For vertical nodes, we need to delete them ourselves as it's not handled by GraphView
                            // ���ڴ�ֱ�ڵ㣬������Ҫ�Լ�ɾ�����ǣ���Ϊ�������� GraphView �����
                            ///Concat��������������
                            foreach (var pv in nodeView.inputPortViews.Concat(nodeView.outputPortViews))//���� ���ڵ����롢����˿����Ӻ������
                                if (pv.orientation == Orientation.Vertical)//��Ϊ��ֱ�ڵ�
                                    foreach (var edge in pv.GetEdges().ToList())
                                        Disconnect(edge);//�Ͽ�EdgeView�����ӣ�ɾ��EdgeView�����¼���˳�򡢶˿���ͼ

                            nodeInspector.NodeViewRemoved(nodeView);//���Զ���ļ������е��б����Ƴ�
                            ExceptionToLog.Call(() => nodeView.OnRemoved());
                            graph.RemoveNode(nodeView.nodeTarget);//�ӽڵ�����б����Ƴ�
                            UpdateSerializedProperties();
                            RemoveElement(nodeView);//��graph���Ƴ�Ԫ��
                            if (Selection.activeObject == nodeInspector)
                                UpdateNodeInspectorSelection();

                            SyncSerializedPropertyPathes();//�����������л������԰�
                            return true;
                        case GroupView group:
                            graph.RemoveGroup(group.group);
                            UpdateSerializedProperties();
                            RemoveElement(group);
                            return true;
                        case ExposedParameterFieldView blackboardField://�ڰ��ֶ�
                            graph.RemoveExposedParameter(blackboardField.parameter);
                            UpdateSerializedProperties();
                            return true;
                        case BaseStackNodeView stackNodeView:
                            graph.RemoveStackNode(stackNodeView.stackNode);
                            UpdateSerializedProperties();
                            RemoveElement(stackNodeView);
                            return true;
#if UNITY_2020_1_OR_NEWER
                        case StickyNoteView stickyNoteView:
                            graph.RemoveStickyNote(stickyNoteView.note);
                            UpdateSerializedProperties();
                            RemoveElement(stickyNoteView);
                            return true;
#endif
                    }

                    return false;
                });
            }

            return changes;
        }

        /// <summary>
        /// graph�ı�Ļص�����
        /// </summary>
        /// <param name="changes"></param>
		void GraphChangesCallback(GraphChanges changes)
        {
            if (changes.removedEdge != null)//������Ҫ�Ƴ���edge���ͶϿ���ɾ��edgeViews�б��ж�Ӧ��Ԫ��
            {
                var edge = edgeViews.FirstOrDefault(e => e.serializedEdge == changes.removedEdge);

                DisconnectView(edge);
            }
        }

        /// <summary>
        /// ��ͼ��graph��λ�ú����ŵ�����
        /// </summary>
        /// <param name="view">GraphView ����</param>
		void ViewTransformChangedCallback(GraphView view)
        {
            if (graph != null)
            {
                ///viewTransform��ͼ�����ͼ�任
				graph.position = viewTransform.position;//position���� VisualElement �ı任��λ��
                graph.scale = viewTransform.scale;//scale���� VisualElement �ı任������
            }
        }

        /// <summary>
        /// ����GroupViewԪ�صĳߴ磨��ȣ��߶ȣ�
        /// </summary>
        /// <param name="elem">������С��Ԫ��</param>
        void ElementResizedCallback(VisualElement elem)
        {
            var groupView = elem as GroupView;

            if (groupView != null)//����GroupView����Ԫ��
                groupView.group.size = groupView.GetPosition().size;//����GroupView�ĳߴ磨��ȣ��߶ȣ�
        }

        /// <summary>
        /// [����]��ȡ������˿ڼ��ݵ����ж˿ڡ�
        /// Ҫ�󣺲�����ͬһ���ڵ��ϵĶ˿ڣ��˿ڷ���ͬ���˿ڵ����Ϳ�����ͼ�����������ӣ�û�д��ڵ�edge�Խ��˿�����
        /// </summary>
        /// <param name="startPort"></param>
        /// <param name="nodeAdapter"></param>
        /// <returns></returns>
		public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();

            //ports����ǰͼ�����еĶ˿�
            compatiblePorts.AddRange(ports.ToList().Where(p => {
                var portView = p as PortView;

                if (portView.owner == (startPort as PortView).owner)//��Ϊͬһ���ڵ�
                    return false;

                if (p.direction == startPort.direction)//���˿ڷ�����ͬ
                    return false;

                //Check for type assignability ������Ϳɷ�����
                if (!BaseGraph.TypesAreConnectable(startPort.portType, p.portType))//���������Ͳ�������ͼ��������������
                    return false;

                //Check if the edge already exists ���edge�Ƿ��Ѿ�����
                if (portView.GetEdges().Any(e => e.input == startPort || e.output == startPort))//��edge�Ѿ����ӵ�startPort��
                    return false;

                return true;
            }));

            return compatiblePorts;
        }

        /// <summary>
        /// Build the contextual menu shown when right clicking inside the graph view
        /// ��������ͼ�����һ�ʱ��ʾ�������Ĳ˵�
        /// </summary>
        /// <param name="evt"></param>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);//�������Ĳ˵���Ӳ˵���
            BuildGroupContextualMenu(evt, 1);//��Create Group���˵���
            BuildStackNodeContextualMenu(evt, 2);//"Create Stack"�˵���
            BuildStickyNoteContextualMenu(evt, 3);//"Create Sticky Note"�˵���

            BuildDebugAllContextualMenu(evt);//"Debug All"�˵���Ƿ���ʾ���нڵ�ļ���˳��
            BuildSelectAssetContextualMenu(evt);//"Select Asset"�˵����project��ѡ�е�ǰgraph��Ӧ��.asset�ļ�
            BuildSaveAssetContextualMenu(evt);//"Save Asset"�˵������.asset�ļ�
            //BuildViewContextualMenu(evt);//"View/Processor"�˵���Ƿ���Process panel
            //BuildHelpContextualMenu(evt);//"Help/Reset Pinned Windows"�˵���
        }

        /// <summary>
        /// Add the New Group entry to the context menu  �ڲ˵�������Ӵ���group��ѡ��
        /// </summary>
        /// <param name="evt"></param>
        protected virtual void BuildGroupContextualMenu(ContextualMenuPopulateEvent evt, int menuPosition = -1)
        {
            if (menuPosition == -1)
                menuPosition = evt.menu.MenuItems().Count;
            Vector2 position = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
            evt.menu.InsertAction(menuPosition, "Create Group", (e) => AddSelectionsToGroup(AddGroup(new Group("New Group", position))), DropdownMenuAction.AlwaysEnabled);
        }

        /// <summary>
        /// Add the New Sticky Note entry to the context menu
        /// </summary>
        /// <param name="evt"></param>
        protected virtual void BuildStickyNoteContextualMenu(ContextualMenuPopulateEvent evt, int menuPosition = -1)
        {
            if (menuPosition == -1)
                menuPosition = evt.menu.MenuItems().Count;
#if UNITY_2020_1_OR_NEWER
            Vector2 position = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
            evt.menu.InsertAction(menuPosition, "Create Sticky Note", (e) => AddStickyNote(new StickyNote("New StickyNote", position)), DropdownMenuAction.AlwaysEnabled);
#endif
        }

        /// <summary>
        /// ���һ��Stack
        /// </summary>
        /// <param name="evt"></param>
        protected void BuildStackNodeContextualMenu(ContextualMenuPopulateEvent evt, int menuPosition = -1)
        {
            if (menuPosition == -1)
                menuPosition = evt.menu.MenuItems().Count;
            Vector2 position = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
            evt.menu.InsertAction(menuPosition, "Create Stack", (e) => AddStackNode(new BaseStackNode(position)), DropdownMenuAction.AlwaysEnabled);
        }


        /// <summary>
        /// Add the View entry to the context menu
        /// </summary>
        /// <param name="evt"></param>
        protected virtual void BuildViewContextualMenu(ContextualMenuPopulateEvent evt)
        {
            //AppendAction���������˵������һ����ִ�в����Ĳ˵���˲˵�������ڵ�ǰ�˵����б�ĩβ
            //evt.menu.AppendAction("View/Processor", (e) => ToggleView< ProcessorView >(), (e) => GetPinnedElementStatus< ProcessorView >());
            evt.menu.AppendAction("View/Processor", (e) => ToggleView<ProcessorView>(), DropdownMenuAction.AlwaysEnabled);
        }


        bool isNodesDebugAll = false;//�Ƿ����нڵ㶼��Debug
        /// <summary>
        /// ����ͼ�������нڵ�ļ���˳���ǩ����ʾ
        /// </summary>
        /// <param name="evt"></param>
        protected virtual void BuildDebugAllContextualMenu(ContextualMenuPopulateEvent evt)
        {
            
            evt.menu.AppendAction("Debug All", (e) => SetNodeDebugAll(), DebugAllStatus);
        }

        //�ж�ͼ�������еĽڵ��Ƿ�����ʾ����˳��
        Status DebugAllStatus(DropdownMenuAction action)
        {
            bool debugAll = (this.graph.nodes.Any(n => n.debug != true)) ? false : true;

            if (debugAll)
                return Status.Checked;//�˵�����ʾΪ����ѡ���
            return Status.Normal;
        }

        //�������нڵ��debug״̬
        void SetNodeDebugAll()
        {
            isNodesDebugAll = !isNodesDebugAll;
            this.graph.nodes.ForEach(n =>{
                if(n.debug != isNodesDebugAll)//ֻ�޸Ĳ�ͬ�Ľڵ�
                    nodeViewsPerNode[n].ToggleDebug();

            });

        }


        /// <summary>
        /// Add the Select Asset entry to the context menu
        /// </summary>
        /// <param name="evt"></param>
        protected virtual void BuildSelectAssetContextualMenu(ContextualMenuPopulateEvent evt)
        {
            ///PingObject���ڳ����жԶ���ִ�� Ping �����������ڼ�������е�����һ��
			evt.menu.AppendAction("Select Asset", (e) => EditorGUIUtility.PingObject(graph), DropdownMenuAction.AlwaysEnabled);
        }

        /// <summary>
        /// Add the Save Asset entry to the context menu �����ʲ�
        /// </summary>
        /// <param name="evt"></param>
        protected virtual void BuildSaveAssetContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Save Asset", (e) => {
                EditorUtility.SetDirty(graph);
                AssetDatabase.SaveAssets();
            }, DropdownMenuAction.AlwaysEnabled);
        }

        /// <summary>
        /// Add the Help entry to the context menu ���úڰ�Ԫ��λ������ʼλ��
        /// </summary>
        /// <param name="evt"></param>
        protected void BuildHelpContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Help/Reset Pinned Windows", e => {
                foreach (var kp in pinnedElements)
                    kp.Value.ResetPosition();
            });
        }

        /// <summary>
        /// �����ص����������桢��ݶ���ڵ�
        ///  [ע�⣺��Ҫ�޸Ĳ��ְ�������Ϊ��unity��windows�Ŀ�ݼ��غ�]
        /// </summary>
        /// <param name="e"></param>
		protected virtual void KeyDownCallback(KeyDownEvent e)
        {
            if (e.keyCode == KeyCode.S && e.commandKey)//���������S�� �� Windows/Ctr/ Cmd(Macת��) ��
            {
                SaveGraphToDisk();
                e.StopPropagation();

            }
            else if (nodeViews.Count > 0 && e.commandKey && e.altKey)
            {
                //	Node Aligning shortcuts �ڵ�����ݼ�
                switch (e.keyCode)
                {
                    case KeyCode.LeftArrow:
                        nodeViews[0].AlignToLeft();
                        e.StopPropagation();
                        break;
                    case KeyCode.RightArrow:
                        nodeViews[0].AlignToRight();
                        e.StopPropagation();
                        break;
                    case KeyCode.UpArrow:
                        nodeViews[0].AlignToTop();
                        e.StopPropagation();
                        break;
                    case KeyCode.DownArrow:
                        nodeViews[0].AlignToBottom();
                        e.StopPropagation();
                        break;
                    case KeyCode.C:
                        nodeViews[0].AlignToCenter();
                        e.StopPropagation();
                        break;
                    case KeyCode.M:
                        nodeViews[0].AlignToMiddle();
                        e.StopPropagation();
                        break;
                }
            }
        }

        Vector2 mouseUpPos;

        /// <summary>
        /// ���̧��ص������յ�ǰѡ�еĽڵ�����Ƿ���¼������
        /// </summary>
        /// <param name="e"></param>
		void MouseUpCallback(MouseUpEvent e)
        {
            schedule.Execute(() => {
                if (DoesSelectionContainsInspectorNodes())
                    UpdateNodeInspectorSelection();
            }).ExecuteLater(1);//1MS��ִ��

            mouseUpPos = e.mousePosition;
        }


        Vector2 mouseDownPos;

        /// <summary>
        /// ��갴�»ص�������������ر��������ô��ڣ����յ�ǰѡ�еĽڵ�����Ƿ���¼������
        /// </summary>
        /// <param name="e"></param>
		void MouseDownCallback(MouseDownEvent e)
        {
            // When left clicking on the graph (not a node or something else) �������ͼ��ʱ�����ǽڵ������������
            if (e.button == 0)//����������ʱ���ر����� ���ô���
            {
                // Close all settings windows:
                nodeViews.ForEach(v => v.CloseSettings());
            }

            if (DoesSelectionContainsInspectorNodes())
                UpdateNodeInspectorSelection();

            /*
            if(DoesSelectionContainsInspectorNodes())
                Debug.Log("DoesSelectionContainsInspectorNodes true");
            else
                Debug.Log("DoesSelectionContainsInspectorNodes false");
            */

            mouseDownPos = e.mousePosition;
        }

        /// <summary>
        /// ��ѡ�еĽڵ����в��ǡ�Ҫ��ʾ�ڼ������Ľڵ㡱 �� ��Ҫ��ʾ�ڼ������Ľڵ㡱û�б�ȫ��ѡ�У��򷵻�true�����򷵻�false
        /// </summary>
        /// <returns></returns>
        bool DoesSelectionContainsInspectorNodes()
        {
            var selectedNodes = selection.Where(s => s is BaseNodeView).ToList();//��ȡ��ǰѡ�е����нڵ�
            ///Except��ͨ��ʹ��Ĭ�ϵ���ȱȽ�����ֵ���бȽϣ������������еĲ
            var selectedNodesNotInInspector = selectedNodes.Except(nodeInspector.selectedNodes).ToList();//���أ���selectedNodes�б��У�������nodeInspector.selectedNodes�е�Ԫ�ص�����
            var nodeInInspectorWithoutSelectedNodes = nodeInspector.selectedNodes.Except(selectedNodes).ToList();//���أ���nodeInspector.selectedNodes���У�������selectedNodes�е�Ԫ�ص�����

            // Debug.Log($"selectedNodes:{selectedNodes.Count} ; nodeInspector.selectedNodes:{nodeInspector.selectedNodes.Count}");
            return selectedNodesNotInInspector.Any() || nodeInInspectorWithoutSelectedNodes.Any();
        }

        /// <summary>
        /// �϶���Ϊ�Ļص����ɴӡ��ڰ塱�е�ExposedParameterField������Hierarchy��塢Project����ק������graph�У������λ�ô������µĽڵ�
        /// </summary>
        /// <param name="e"></param>
		void DragPerformedCallback(DragPerformEvent e)
        {
            ///currentTarget���¼��ĵ�ǰĿ�ꡣ��ǰ·���Ǵ���·����Ŀǰ����Ϊ��ִ���¼���������Ԫ��
            var mousePos = (e.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, e.localMousePosition);
            ///GetGenericData����ȡ�뵱ǰ�ϷŲ�����ص�����
			var dragData = DragAndDrop.GetGenericData("DragSelection") as List<ISelectable>;

            // Drag and Drop for elements inside the graph  �Ϸ�ͼ���ڵ�Ԫ��
            // �ٶԺڰ��е�ExposedParameterField�Ϸ�
            if (dragData != null)
            {
                ///OfType������ָ������ɸѡ IEnumerable ��Ԫ��
				var exposedParameterFieldViews = dragData.OfType<ExposedParameterFieldView>();//ɸѡExposedParameterFieldView��Ԫ��
                if (exposedParameterFieldViews.Any())//�������κ�ExposedParameterFieldView���͵�Ԫ��
                {
                    foreach (var paramFieldView in exposedParameterFieldViews)
                    {
                        RegisterCompleteObjectUndo("Create Parameter Node");
                        var paramNode = BaseNode.CreateFromType<ParameterNode>(mousePos);//��mousePosλ�ô���һ������ΪParameterNode�Ľڵ�
                        paramNode.parameterGUID = paramFieldView.parameter.guid;//��ParameterNode��parameterGUID��ֵ
                        AddNode(paramNode);
                    }
                }
            }

            // ��External objects drag and drop �ⲿ�����Ϸ�(��Hierarchy��塢Project����ק������graph��)
            if (DragAndDrop.objectReferences.Length > 0)//objectReferences�����϶��� objects ������
            {
               
                RegisterCompleteObjectUndo("Create Node From Object(s)");
                foreach (var obj in DragAndDrop.objectReferences)
                {
                  
                    var objectType = obj.GetType();

                    foreach (var kp in nodeTypePerCreateAssetType)
                    {
                       
                        if (kp.Key.IsAssignableFrom(objectType))//IsAssignableFrom��ȷ��ָ������objectType��ʵ���Ƿ��ܷ������ǰ���͵ı���
                        {
                            try
                            {
                                var node = BaseNode.CreateFromType(kp.Value.nodeType, mousePos);
                                
                                if ((bool)kp.Value.initalizeNodeFromObject.Invoke(node, new[] { obj }))
                                {
                                    AddNode(node);
                                    ///Debug.Log("�ⲿ�����Ϸ�");
									break;
                                }
                            }
                            catch (Exception exception)
                            {
                                Debug.LogException(exception);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// �϶�Ԫ�ظ��µĻص��������Ŀ���ָʾ�����¼������
        /// </summary>
        /// <param name="e"></param>
		void DragUpdatedCallback(DragUpdatedEvent e)
        {
            ///GetGenericData����ȡ�뵱ǰ�ϷŲ�����ص�����   objectReferences�����϶��� objects ������
            var dragData = DragAndDrop.GetGenericData("DragSelection") as List<ISelectable>;
            var dragObjects = DragAndDrop.objectReferences;
            bool dragging = false;

            if (dragData != null)
            {
                // Handle drag from exposed parameter view ��ExposedParameterFieldView�д����϶�
                if (dragData.OfType<ExposedParameterFieldView>().Any())
                {
                    dragging = true;
                }
            }

            if (dragObjects.Length > 0)
                dragging = true;

            if (dragging)
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;//������ϷŲ����Ŀ���ָʾ

            UpdateNodeInspectorSelection();//���¼������
        }

        #endregion

        #region Initialization ��ʼ��

        /// <summary>
        /// ������ͼ����ͼ��graph�����л�����¼�µ�ǰѡ�еĽڵ�Ԫ�أ�
        /// �Ƴ�����Ԫ�أ������³�ʼ������Ԫ�أ����¼���˳�򣻽���¼�Ľڵ�Ԫ�ر�Ϊѡ�в�������
        /// ���½ڵ��������ѡ��
        /// </summary>
        void ReloadView()
        {
            // Force the graph to reload his data (Undo have updated the serialized properties of the graph
            // so the one that are not serialized need to be synchronized)
            // ǿ��ͼ�����¼����������ݣ������Ѿ�������ͼ������л����ԣ����δ���л�����Ҫͬ����
            graph.Deserialize();

            // Get selected nodes ��ȡ��ѡ�еĽڵ�
            var selectedNodeGUIDs = new List<string>();
            foreach (var e in selection)//������ǰ��ͼ��������ѡ�е�Ԫ��
            {
                if (e is BaseNodeView v && this.Contains(v))//����BaseNodeView���� �� ��Ԫ���Ǵ�graphView��ֱ���Ӽ�
                    selectedNodeGUIDs.Add(v.nodeTarget.GUID);//��¼�˽ڵ��GUID
            }

            // Remove everything �Ƴ����е�Ԫ��
            RemoveNodeViews();
            RemoveEdges();
            RemoveGroups();
#if UNITY_2020_1_OR_NEWER
            RemoveStrickyNotes();
#endif
            RemoveStackNodeViews();

            UpdateSerializedProperties();//����graph�����л�����

            // And re-add with new up to date datas ��������µ��������ݣ����³�ʼ������Ԫ�أ�
            InitializeNodeViews();
            InitializeEdgeViews();
            InitializeGroups();
            InitializeStickyNotes();
            InitializeStackNodes();

            Reload();

            UpdateComputeOrder();//����ͼ�нڵ�ļ���˳�򣬲�������Ӧ�¼�����

            // Restore selection after re-creating all views ���´���������ͼ��ָ�ѡ��
            // selection = nodeViews.Where(v => selectedNodeGUIDs.Contains(v.nodeTarget.GUID)).Select(v => v as ISelectable).ToList();
            foreach (var guid in selectedNodeGUIDs)
            {
                AddToSelection(nodeViews.FirstOrDefault(n => n.nodeTarget.GUID == guid));//ѡ�в�����
            }

            UpdateNodeInspectorSelection();//���½ڵ��������ѡ��
        }

        public void Initialize(BaseGraph graph, BaseGraphWindow graphWindow, Dictionary<string, RDTSBehavior> bindObjPerGuid = null)
        {
            if (this.graph != null)
            {
                SaveGraphToDisk();
                // Close pinned windows from old graph:
                ClearGraphElements();
                NodeProvider.UnloadGraph(graph);
            }

            this.graph = graph;
            this.graphWindow = graphWindow;
            this.bindObjPerGuid = bindObjPerGuid;

            exposedParameterFactory = new ExposedParameterFieldFactory(graph);

            UpdateSerializedProperties();

            connectorListener = CreateEdgeConnectorListener();

            // When pressing ctrl-s, we save the graph
            EditorSceneManager.sceneSaved += _ => SaveGraphToDisk();
            RegisterCallback<KeyDownEvent>(e => {
                if (e.keyCode == KeyCode.S && e.actionKey)
                    SaveGraphToDisk();
            });

            ClearGraphElements();

            //��ʼ��graph����������Ԫ��
            InitializeGraphView();//��ʼ��Graph��ͼ��ע���¼�
            InitializeNodeViews();//����BaseGraph.nodes��ʼ������Node��ͼ
            InitializeEdgeViews();
            InitializeViews();
            InitializeGroups();
            InitializeStickyNotes();
            InitializeStackNodes();

            initialized?.Invoke();
            UpdateComputeOrder();

            InitializeView();

            NodeProvider.LoadGraph(graph);

            // Register the nodes that can be created from assets ע����Դ��ʲ������Ľڵ�
            foreach (var nodeInfo in NodeProvider.GetNodeMenuEntries(graph))
            {
                var interfaces = nodeInfo.type.GetInterfaces();
                var exceptInheritedInterfaces = interfaces.Except(interfaces.SelectMany(t => t.GetInterfaces()));
                foreach (var i in interfaces)
                {
                    if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICreateNodeFrom<>))
                    {
                        var genericArgumentType = i.GetGenericArguments()[0];//GetGenericArguments�����ر�ʾ���ʽ�������͵����Ͳ����������Ͷ�������Ͳ����� Type ���������
                        var initializeFunction = nodeInfo.type.GetMethod(
                            nameof(ICreateNodeFrom<Object>.InitializeNodeFromObject),//�ҵ��˷���
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                            null, new Type[] { genericArgumentType }, null
                        );

                        // We only add the type that implements the interface, not it's children
                        // ����ֻ���ʵ�ֽӿڵ����ͣ�������������
                        if (initializeFunction.DeclaringType == nodeInfo.type)
                            nodeTypePerCreateAssetType[genericArgumentType] = (nodeInfo.type, initializeFunction);
                    }
                }
            }
        }

        /// <summary>���ͼ�������е�Ԫ��</summary>
		public void ClearGraphElements()
        {
            RemoveGroups();
            RemoveNodeViews();
            RemoveEdges();
            RemoveStackNodeViews();
            RemovePinnedElementViews();
#if UNITY_2020_1_OR_NEWER
            RemoveStrickyNotes();
#endif
        }

        /// <summary>����graph�����л�����</summary>
		void UpdateSerializedProperties()
        {
            serializedGraph = new SerializedObject(graph);//Ϊ���Ķ���graph ���� SerializedObject
        }

        /// <summary>
        /// Allow you to create your own edge connector listener  �������Լ���EdgeConnectorListener
        /// </summary>
        /// <returns></returns>
        protected virtual BaseEdgeConnectorListener CreateEdgeConnectorListener()
         => new BaseEdgeConnectorListener(this);

        /// <summary>��ʼ��ͼ����ͼ��������ί�У�ͼ��λ�ú����ţ��������� </summary>
		void InitializeGraphView()
        {
            graph.onExposedParameterListChanged += OnExposedParameterListChanged;
            graph.onExposedParameterModified += (s) => onExposedParameterModified?.Invoke(s);
            graph.onGraphChanges += GraphChangesCallback;
            //ͼ����ͼ��λ�ú�����
            viewTransform.position = graph.position;
            viewTransform.scale = graph.scale;
            ///nodeCreationRequest�����û�������ʾ�ڵ㴴������ʱʹ�õĻص�
            ///NodeCreationContext���˽ṹ��ʾ�û���ʼ����ͼ�νڵ�ʱ��������
            //���ɡ�Create Node���˵���������������
            nodeCreationRequest = (c) => SearchWindow.Open(new SearchWindowContext(c.screenMousePosition), createNodeMenu);
        }

        /// <summary>�����Ĳ����б��Ѹ���</summary>
		void OnExposedParameterListChanged()
        {
            UpdateSerializedProperties();
            onExposedParameterListChanged?.Invoke();
        }

        /// <summary>��ʼ���ڵ���ͼNodeView</summary>
		void InitializeNodeViews()
        {
            graph.nodes.RemoveAll(n => n == null);//����յĽڵ�

            foreach (var node in graph.nodes)
            {
                
                var v = AddNodeView(node);//��ӽڵ��Ӧ��NodeView
            }
        }

        /// <summary>
        /// ��ʼ��edgeView�������Ϊ�ջ������edge��
        /// </summary>
		void InitializeEdgeViews()
        {
            // Sanitize edges in case a node broke something while loading �����Ե�Է��ڵ��ڼ���ʱ�ƻ�ĳЩ����
            graph.edges.RemoveAll(edge => edge == null || edge.inputNode == null || edge.outputNode == null);//���Ϊ�ջ������edge

            foreach (var serializedEdge in graph.edges)
            {
                nodeViewsPerNode.TryGetValue(serializedEdge.inputNode, out var inputNodeView);//��ȡedge������ڵ��Ӧ��NodeView
                nodeViewsPerNode.TryGetValue(serializedEdge.outputNode, out var outputNodeView);//��ȡedge������ڵ��Ӧ��NodeView
                if (inputNodeView == null || outputNodeView == null)//����Ϊ��������
                    continue;

                //����һ���µ�EdgeView��������������/����˿ڡ�userData(Ϊ��serializedEdge)
                var edgeView = CreateEdgeView();
                edgeView.userData = serializedEdge;
                edgeView.input = inputNodeView.GetPortViewFromFieldName(serializedEdge.inputFieldName, serializedEdge.inputPortIdentifier);
                edgeView.output = outputNodeView.GetPortViewFromFieldName(serializedEdge.outputFieldName, serializedEdge.outputPortIdentifier);


                ConnectView(edgeView);//ʵ��edge������/����˿ڵ����ӣ����˿ڲ�֧�ֶ����������Ƚ�ԭ�����ӵ��˿ڵ�edge�Ƴ�������ɫ�����edge�����޲�
            }
        }

        /// <summary>��ʼ��pinnedElementԪ�أ����д򿪵�pinnedElement�ʹ�</summary>
		void InitializeViews()
        {
            foreach (var pinnedElement in graph.pinnedElements)
            {
                if (pinnedElement.opened)
                    OpenPinned(pinnedElement.editorType.type);
            }
        }

        /// <summary>��ʼ��Group�����GroupView��ͼ����</summary>
        void InitializeGroups()
        {
            foreach (var group in graph.groups)
                AddGroupView(group);
        }

        /// <summary>��ʼ������ǩ��Ԫ�أ����StickyNoteView��ͼ����</summary>
		void InitializeStickyNotes()
        {
#if UNITY_2020_1_OR_NEWER
            foreach (var group in graph.stickyNotes)
                AddStickyNoteView(group);
#endif
        }

        /// <summary>��ʼ����ջԪ�أ����BaseStackNodeView��ͼ����</summary>
		void InitializeStackNodes()
        {
            foreach (var stackNode in graph.stackNodes)
                AddStackNodeView(stackNode);
        }

        protected virtual void InitializeManipulators()
        {
            this.AddManipulator(new ContentDragger());//��������϶�һ������Ԫ��
            this.AddManipulator(new SelectionDragger());//ѡ���϶�����ٿ���
            this.AddManipulator(new RectangleSelector());//����ѡ���
        }


        class AddGridBackground : GridBackground { }//�ü̳е���Ż���������
        /// <summary>
        /// �������
        /// </summary>
        private void AddGrid()
        {
            var grid = new AddGridBackground();
            this.Insert(0, grid);
            grid.StretchToParentSize();//����VisualElement�����ҡ����±�Ե�븸Ԫ�صı�Ե����
        }

        protected virtual void Reload() { }

        #endregion

        #region Graph content modification ͼ�������޸�

        /// <summary>
        /// ���½ڵ��������ѡ��
        /// </summary>
        public void UpdateNodeInspectorSelection()
        {
            if (nodeInspector.previouslySelectedObject != Selection.activeObject)//�������ǰѡ��Ķ����ǵ�ǰѡ�еĶ���������Ϊ��ǰѡ�еĶ���
                nodeInspector.previouslySelectedObject = Selection.activeObject;

            HashSet<BaseNodeView> selectedNodeViews = new HashSet<BaseNodeView>();
            nodeInspector.selectedNodes.Clear();//���
            foreach (var e in selection)//������ǰ��graph��ѡ���Ԫ��
            {
                if (e is BaseNodeView v && this.Contains(v) && v.nodeTarget.needsInspector)//���ǽڵ� �� ����graph���� �� �˽ڵ���Ҫ�ڼ�������пɼ�
                    selectedNodeViews.Add(v);//���뵽����
            }

            nodeInspector.UpdateSelectedNodes(selectedNodeViews);//��selectedNodeViews�����Ƶ�nodeInspector�����selectedNodes����
            if (Selection.activeObject != nodeInspector && selectedNodeViews.Count > 0)//����ǰѡ��Ķ�����Ҫ�ڼ����������ʾ�� �� selectedNodeViews����Ԫ��
                Selection.activeObject = nodeInspector;//��nodeInspectorȷ��Ϊ��ǰѡ��Ķ���
        }

        /// <summary>
        /// ���һ���ڵ���ͼ���У���BaseGraph������ӽڵ���Ϣ����BaseGraphView����ӿ��ӻ��Ľڵ�Ԫ�أ�
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
		public BaseNodeView AddNode(BaseNode node)
        {
            // This will initialize the node using the graph instance  ʹ��ͼ��ʵ����ʼ���ڵ�
            graph.AddNode(node);//����һ���ڵ㵽ͼ����

            UpdateSerializedProperties();//����ͼ��graph�����л�����

            var view = AddNodeView(node);//��ӽڵ��Ӧ��nodeView��ͼ����(��ӿ��ӻ�Ԫ����ͼ��)

            // Call create after the node have been initialized  �ڽڵ��ѱ���ʼ�������
            ExceptionToLog.Call(() => view.OnCreated());

            UpdateComputeOrder();//����ͼ�нڵ�ļ���˳�򣬲�������Ӧ�¼�����

            return view;
        }

        /// <summary>
        /// ���ָ��node��nodeView��graphView�У���ӿ��ӻ�Ԫ����ͼ��
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
		public BaseNodeView AddNodeView(BaseNode node)
        {
            var viewType = NodeProvider.GetNodeViewTypeFromType(node.GetType());//�ҵ�ָ��nodeType��Ӧ��nodeViewType����û�������丸���nodeViewType

            if (viewType == null)
                viewType = typeof(BaseNodeView);

            var baseNodeView = Activator.CreateInstance(viewType) as BaseNodeView;
            baseNodeView.Initialize(this, node);//��ʼ��
            AddElement(baseNodeView);//����Ԫ����ӵ�graph

            nodeViews.Add(baseNodeView);//����洢���б�
            nodeViewsPerNode[node] = baseNodeView;//����洢���ֵ�

            return baseNodeView;
        }

        /// <summary>�Ƴ�һ���ڵ�node����nodeView</summary>
		public void RemoveNode(BaseNode node)
        {
            var view = nodeViewsPerNode[node];
            RemoveNodeView(view);
            graph.RemoveNode(node);
        }
        /// <summary>�Ƴ�һ��nodeView</summary>
		public void RemoveNodeView(BaseNodeView nodeView)
        {
            RemoveElement(nodeView);
            nodeViews.Remove(nodeView);
            nodeViewsPerNode.Remove(nodeView.nodeTarget);
        }

        /// <summary>�Ƴ����е�NodeViews</summary>
		void RemoveNodeViews()
        {
            foreach (var nodeView in nodeViews)
                RemoveElement(nodeView);
            nodeViews.Clear();
            nodeViewsPerNode.Clear();
        }
        /// <summary>�Ƴ����е�BaseStackNodeView</summary>
		void RemoveStackNodeViews()
        {
            foreach (var stackView in stackNodeViews)
                RemoveElement(stackView);
            stackNodeViews.Clear();
        }
        /// <summary>�Ƴ����е�PinnedElementView</summary>
		void RemovePinnedElementViews()
        {
            foreach (var pinnedView in pinnedElements.Values)
            {
                if (Contains(pinnedView))
                    Remove(pinnedView);
            }
            pinnedElements.Clear();
        }

        /// <summary>���һ��group</summary>
        public GroupView AddGroup(Group block)
        {
            graph.AddGroup(block);
            block.OnCreated();
            return AddGroupView(block);
        }
        /// <summary>���һ��groupԪ����graph��</summary>
		public GroupView AddGroupView(Group block)
        {
            var c = new GroupView();

            c.Initialize(this, block);

            AddElement(c);

            groupViews.Add(c);
            return c;
        }

        /// <summary>���һ����ջԪ��</summary>
		public BaseStackNodeView AddStackNode(BaseStackNode stackNode)
        {
            graph.AddStackNode(stackNode);
            return AddStackNodeView(stackNode);
        }

        public BaseStackNodeView AddStackNodeView(BaseStackNode stackNode)
        {
            var viewType = StackNodeViewProvider.GetStackNodeCustomViewType(stackNode.GetType()) ?? typeof(BaseStackNodeView);
            var stackView = Activator.CreateInstance(viewType, stackNode) as BaseStackNodeView;

            AddElement(stackView);
            stackNodeViews.Add(stackView);

            stackView.Initialize(this);

            return stackView;
        }
        /// <summary>�Ƴ�һ��groupԪ����graph��</summary>
		public void RemoveStackNodeView(BaseStackNodeView stackNodeView)
        {
            stackNodeViews.Remove(stackNodeView);
            RemoveElement(stackNodeView);
        }

        /// <summary>���һ������ǩ���ڵ�</summary>
#if UNITY_2020_1_OR_NEWER
        public StickyNoteView AddStickyNote(StickyNote note)
        {
            graph.AddStickyNote(note);
            return AddStickyNoteView(note);
        }

        public StickyNoteView AddStickyNoteView(StickyNote note)
        {
            var c = new StickyNoteView();

            c.Initialize(this, note);

            AddElement(c);

            stickyNoteViews.Add(c);
            return c;
        }
        /// <summary>�Ƴ�һ������ǩ���ڵ�</summary>
		public void RemoveStickyNoteView(StickyNoteView view)
        {
            stickyNoteViews.Remove(view);
            RemoveElement(view);
        }
        /// <summary>�Ƴ����еġ���ǩ���ڵ�</summary>
		public void RemoveStrickyNotes()
        {
            foreach (var stickyNodeView in stickyNoteViews)
                RemoveElement(stickyNodeView);
            stickyNoteViews.Clear();
        }
#endif

        /// <summary>
        /// ��ѡ�еĽڵ���ӵ�ָ����group�У�����group�еĽڵ㲻�ᱻ��ӣ�
        /// </summary>
        /// <param name="view"></param>
        public void AddSelectionsToGroup(GroupView view)
        {
            foreach (var selectedNode in selection)
            {
                if (selectedNode is BaseNodeView)
                {
                    ///Exists��ȷ�� List<T> �Ƿ������ָ��ν�ʶ��������(ί�з���)ƥ���Ԫ�ء� ������һ������������ƥ���Ԫ�أ�����true
                    ///ContainsElement��ȷ���������Ƿ����ָ���� GraphElement
                    if (groupViews.Exists(x => x.ContainsElement(selectedNode as BaseNodeView)))//����graph������groupԪ���У������Ѱ����˽ڵ��group��������
                        continue;

                    view.AddElement(selectedNode as BaseNodeView);//�����ڣ��򽫴˽ڵ���ӵ�ָ����group��
                }
            }
        }

        /// <summary>�Ƴ����е�group</summary>
		public void RemoveGroups()
        {
            foreach (var groupView in groupViews)
                RemoveElement(groupView);
            groupViews.Clear();
        }

        /// <summary>
        /// �����Ƿ��������/����˿ڡ���������ڵ����ж��ܷ��������
        /// </summary>
        /// <param name="e"></param>
        /// <param name="autoDisconnectInputs">û�õ���</param>
        /// <returns></returns>
		public bool CanConnectEdge(EdgeView e, bool autoDisconnectInputs = true)
        {
            if (e.input == null || e.output == null)
                return false;

            var inputPortView = e.input as PortView;//����˿�
            var outputPortView = e.output as PortView;//����˿�
            var inputNodeView = inputPortView.node as BaseNodeView;//����ڵ�
            var outputNodeView = outputPortView.node as BaseNodeView;//����ڵ�

            if (inputNodeView == null || outputNodeView == null)
            {
                Debug.LogError("Connect aborted !");
                return false;
            }

            return true;
        }

        /// <summary>
        /// ʵ��edge������/����˿ڵ�����(������)����������뵽graph�У����˿ڲ�֧�ֶ����������Ƚ�ԭ�����ӵ��˿ڵ�edge�Ƴ���ˢ�¶˿���ͼ
        /// ����ɫ�����edge�����޲�
        /// </summary>
        /// <param name="e"></param>
        /// <param name="autoDisconnectInputs"></param>
        /// <returns></returns>
		public bool ConnectView(EdgeView e, bool autoDisconnectInputs = true)
        {
            if (!CanConnectEdge(e, autoDisconnectInputs))//�����������������ͷ���false
                return false;

            var inputPortView = e.input as PortView;//����˿�
            var outputPortView = e.output as PortView;//����˿�
            var inputNodeView = inputPortView.node as BaseNodeView;//����˿����ڽڵ㼴edge������ڵ�
            var outputNodeView = outputPortView.node as BaseNodeView;//����˿����ڽڵ㼴edge������ڵ�

            //If the input port does not support multi-connection, we remove them �������˿ڲ�֧�ֶ���edge���ӣ���ɾ��ԭ�������ŵ�edge
            if (autoDisconnectInputs && !(e.input as PortView).portData.acceptMultipleEdges)
            {
                foreach (var edge in edgeViews.Where(ev => ev.input == e.input).ToList())
                {
                    // TODO: do not disconnect them if the connected port is the same than the old connected ������ӵĶ˿�������ӵĶ˿���ͬ���벻Ҫ�Ͽ�����
                    DisconnectView(edge);
                }
            }
            // same for the output port: ������˿ڽ���ͬ���Ĵ���
            if (autoDisconnectInputs && !(e.output as PortView).portData.acceptMultipleEdges)
            {
                foreach (var edge in edgeViews.Where(ev => ev.output == e.output).ToList())
                {
                    // TODO: do not disconnect them if the connected port is the same than the old connected
                    DisconnectView(edge);
                }
            }

            AddElement(e);//* ����edge���뵽graph��

            //����������/����˿�
            e.input.Connect(e);//*
            e.output.Connect(e);//*

            // If the input port have been removed by the custom port behavior  
            // we try to find if it's still here
            // �������˿��ѱ��Զ���˿���Ϊɾ����������ͼ�ҳ����Ƿ�������
            if (e.input == null)
                e.input = inputNodeView.GetPortViewFromFieldName(inputPortView.fieldName, inputPortView.portData.identifier);//
            if (e.output == null)
                e.output = inputNodeView.GetPortViewFromFieldName(outputPortView.fieldName, outputPortView.portData.identifier);

            edgeViews.Add(e);

            //��ȷ��ӵ����ͬ������port��portView����ˢ�¸���inputPortViews��outputPortViews�˿���ͼ
            inputNodeView.RefreshPorts();
            outputNodeView.RefreshPorts();

            // In certain cases the edge color is wrong so we patch it ��ĳЩ����£�edge��ɫ�Ǵ���ģ�������Ƕ�������޲�
            schedule.Execute(() => {
                e.UpdateEdgeControl();
            }).ExecuteLater(1);

            e.isConnected = true;//* ��λ��ʾ����

            return true;
        }

        /// <summary>
        /// �����㡱�����ӷ�����(�����µ�SerializableEdge��EdgeView)���Ӳ����е�inputPortView��outputPortView��������صĲ���(���ߡ���¼������Ϣ����Ԫ�ؼ��뵽graph��ˢ�¶˿���ͼ�����¼���˳���޲�edge��ɫ��)
        /// </summary>
        /// <param name="inputPortView"></param>
        /// <param name="outputPortView"></param>
        /// <param name="autoDisconnectInputs"></param>
        /// <returns></returns>
		public bool Connect(PortView inputPortView, PortView outputPortView, bool autoDisconnectInputs = true)
        {
            var inputPort = inputPortView.owner.nodeTarget.GetPort(inputPortView.fieldName, inputPortView.portData.identifier);
            var outputPort = outputPortView.owner.nodeTarget.GetPort(outputPortView.fieldName, outputPortView.portData.identifier);

            // Checks that the node we are connecting still exists ��������������ӵĽڵ��Ƿ���Ȼ���ڣ�������ֱ�ӷ���
            if (inputPortView.owner.parent == null || outputPortView.owner.parent == null)
                return false;

            var newEdge = SerializableEdge.CreateNewEdge(graph, inputPort, outputPort);//����һ���µ�edge����edge�߱���graph��inputPort��outputPort�����������Ϣ

            var edgeView = CreateEdgeView();//����һ���µ�EdgeView
            edgeView.userData = newEdge;
            edgeView.input = inputPortView;
            edgeView.output = outputPortView;


            return Connect(edgeView);
        }

        /// <summary>
        /// ���жϲ���edgeView�Ƿ���������Ҫ��Ȼ����SerializableEdge��EdgeView�зֱ�ʵ�ִ�edge��˿ڵ����ӣ�
        /// ��edgeView���뵽graph�У�
        /// ���¼���˳��
        /// </summary>
        /// <param name="e"></param>
        /// <param name="autoDisconnectInputs"></param>
        /// <returns></returns>
		public bool Connect(EdgeView e, bool autoDisconnectInputs = true)
        {
            if (!CanConnectEdge(e, autoDisconnectInputs))//���ж��ܷ�������ߣ�����/����˿ڡ�����/����ڵ�����ڣ�
                return false;

            var inputPortView = e.input as PortView;//����˿�View
            var outputPortView = e.output as PortView;//����˿�View
            var inputNodeView = inputPortView.node as BaseNodeView;//����ڵ�View
            var outputNodeView = outputPortView.node as BaseNodeView;//����ڵ�View
            var inputPort = inputNodeView.nodeTarget.GetPort(inputPortView.fieldName, inputPortView.portData.identifier);//��ȡ��Ӧ������˿�
            var outputPort = outputNodeView.nodeTarget.GetPort(outputPortView.fieldName, outputPortView.portData.identifier);//��ȡ��Ӧ������˿�

            e.userData = graph.Connect(inputPort, outputPort, autoDisconnectInputs);//�½�һ��(�������������Ϣ��)SerializableEdge�����������˿ڣ�������SerializableEdge��¼��graph�е�edges�б���

            ConnectView(e, autoDisconnectInputs);//ʵ��edge������/����˿ڵ����ӣ�[��������뵽graph��]�����˿ڲ�֧�ֶ����������Ƚ�ԭ�����ӵ��˿ڵ�edge�Ƴ���ˢ�¶˿���ͼ���޲�edge��ɫ

            UpdateComputeOrder();//���½ڵ����˳��

            return true;
        }

        /// <summary>
        /// ����edge��ͼ��graph��ɾ�����Ͽ�edge����������Ͽ������ӣ���edgeViews�б���ɾ����
        /// ��������¶˿���ͼ
        /// </summary>
        /// <param name="e">�������edge</param>
        /// <param name="refreshPorts">�Ƿ�ˢ�¶˿���ͼ</param>
		public void DisconnectView(EdgeView e, bool refreshPorts = true)
        {
            if (e == null)
                return;

            RemoveElement(e);//����edge��ͼ��graph��ɾ��

            if (e?.input?.node is BaseNodeView inputNodeView)
            {
                e.input.Disconnect(e);///*�Ͽ�edge��˿ڵ�����
                if (refreshPorts)
                    inputNodeView.RefreshPorts();//��ȷ��ӵ����ͬ������port��portView����ˢ�¸���inputPortViews��outputPortViews�˿���ͼ
            }
            if (e?.output?.node is BaseNodeView outputNodeView)
            {
                e.output.Disconnect(e);///*�Ͽ�edge��˿ڵ�����
				if (refreshPorts)
                    outputNodeView.RefreshPorts();
            }

            edgeViews.Remove(e);//Ҳ������edge���б��н���ɾ��
        }

        /// <summary>
        /// �Ͽ�EdgeView�����ӣ�ɾ��EdgeView�����¼���˳�򡢶˿���ͼ
        /// </summary>
        /// <param name="e"></param>
        /// <param name="refreshPorts"></param>
		public void Disconnect(EdgeView e, bool refreshPorts = true)
        {
            // Remove the serialized edge if there is one ����У���ɾ�����л���Ե
            if (e.userData is SerializableEdge serializableEdge)
                graph.Disconnect(serializableEdge.GUID);//�Ͽ�����

            DisconnectView(e, refreshPorts);//�Ƴ�edge���Ͽ���˿����ӣ�ˢ�¶˿���ͼ

            UpdateComputeOrder();//���¼���˳��
        }

        /// <summary>��edgeViews�б��е�Ԫ��ȫ���Ƴ��������edgeViews�б�</summary>
		public void RemoveEdges()
        {
            foreach (var edge in edgeViews)
                RemoveElement(edge);
            edgeViews.Clear();
        }

        /// <summary>
        /// ����ͼ�нڵ�ļ���˳�򣬲�������Ӧ�¼�����
        /// </summary>
		public void UpdateComputeOrder()
        {
            graph.UpdateComputeOrder();

            computeOrderUpdated?.Invoke();
        }

        /// <summary>
        /// ע�᳷�������ִ���˳�����������ô�ڵ�����һ������Զ����������κθ��Ķ��������������Ҹö��󽫻ָ�����¼��״̬
        /// </summary>
        /// <param name="name"></param>
		public void RegisterCompleteObjectUndo(string name)
        {
            Undo.RegisterCompleteObjectUndo(graph, name);//������״̬�洢�ڳ�����ջ��
            //���ִ���˳�����������ô�ڵ�����һ������Զ����������κθ��Ķ��������������Ҹö��󽫻ָ�����¼��״̬
        }

        /// <summary>��ͼ��graph�������</summary>
        public void SaveGraphToDisk()
        {
            if (graph == null)
                return;

            EditorUtility.SetDirty(graph);
        }

        /// <summary>���ͣ������Ƿ񴰿ڴ���������򿪻�رմ���</summary>
		public void ToggleView<T>() where T : PinnedElementView//T����̳�PinnedElementView��
        {
            ToggleView(typeof(T));
        }
        /// <summary>PinnedElementView���͵Ĵ����Ѵ򿪣��͹رգ��ѹرգ��ʹ�</summary>
		public void ToggleView(Type type)
        {
            PinnedElementView view;
            pinnedElements.TryGetValue(type, out view);

            if (view == null)
                OpenPinned(type);
            else
                ClosePinned(type, view);
        }

        public void OpenPinned<T>() where T : PinnedElementView
        {
            OpenPinned(typeof(T));
        }

        /// <summary>
        /// ��PinnedElement���Ԫ��
        /// </summary>
        /// <param name="type">PinnedElementView��������</param>
		public void OpenPinned(Type type)
        {
            PinnedElementView view;

            if (type == null)
                return;

            PinnedElement elem = graph.OpenPinned(type);

            if (!pinnedElements.ContainsKey(type))//���ֵ��в�����type�ļ�
            {
                view = Activator.CreateInstance(type) as PinnedElementView;//CreateInstance������type�Ķ��󣬲����ض��������
                if (view == null)
                    return;
                pinnedElements[type] = view;
                view.InitializeGraphView(elem, this);
            }
            view = pinnedElements[type];

            if (!Contains(view))//��graph�в�������type��Ӧ��PinnedElementView�������
                Add(view);
        }

        public void ClosePinned<T>(PinnedElementView view) where T : PinnedElementView
        {
            ClosePinned(typeof(T), view);
        }

        public void ClosePinned(Type type, PinnedElementView elem)
        {
            pinnedElements.Remove(type);
            Remove(elem);
            graph.ClosePinned(type);
        }

        public Status GetPinnedElementStatus<T>() where T : PinnedElementView
        {
            return GetPinnedElementStatus(typeof(T));
        }

        public Status GetPinnedElementStatus(Type type)
        {
            var pinned = graph.pinnedElements.Find(p => p.editorType.type == type);

            if (pinned != null && pinned.opened)
                return Status.Normal;
            else
                return Status.Hidden;
        }

        /// <summary>����ͼ��graph��λ�õ����Ŵ�С</summary>
		public void ResetPositionAndZoom()
        {
            graph.position = Vector3.zero;
            graph.scale = Vector3.one;

            UpdateViewTransform(graph.position, graph.scale);
        }

        /// <summary>�򿪰�����ʾ����</summary
        public void OpenHelpTipsWindow(GraphTipsWindow tipsWindow)
        {
            Vector2 mousePos = mouseUpPos;
            UnityEditor.PopupWindow.Show(new Rect(mousePos.x, mousePos.y, 0, 0), new GraphTipsWindow());
        }

        /// <summary>�رն�Ӧ�Ĵ���</summary
        public void CloseWindow()
        {
            if (EditorUtility.DisplayDialog("Close Window", "�Ƿ�ȷ���رմ˴��ڣ�", "yes", "no"))
            {
                graphWindow.Close();
            }
        }

        /// <summary>
        /// Deletes the selected content, can be called form an IMGUI container
        /// ɾ��ѡ�е����ݣ����Դ�IMGUI�����е���
        /// </summary>
        public void DelayedDeleteSelection() => this.schedule.Execute(() => DeleteSelectionOperation("Delete", AskUser.DontAskUser)).ExecuteLater(0);

        protected virtual void InitializeView() { }

        /// <summary>
        /// �����ڵ�˵���Ŀ�Ĺ�����
        /// </summary>
        /// <returns></returns>
		public virtual IEnumerable<(string path, Type type)> FilterCreateNodeMenuEntries()
        {
            // By default we don't filter anything Ĭ������£����ǲ������κ�����
            foreach (var nodeMenuItem in NodeProvider.GetNodeMenuEntries(graph))
                yield return nodeMenuItem;

            // TODO: add exposed properties to this list ��������������ӵ����б�
        }

        /// <summary>
        /// ��ָ��λ�ô����һ���м̽ڵ�
        /// </summary>
        /// <param name="inputPort"></param>
        /// <param name="outputPort"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public RelayNodeView AddRelayNode(PortView inputPort, PortView outputPort, Vector2 position)
        {
            var relayNode = BaseNode.CreateFromType<RelayNode>(position);
            var view = AddNode(relayNode) as RelayNodeView;

            ///ע�⣺�м̽ڵ�ֻ��һ�����롢һ������˿�
			if (outputPort != null)//��ָ��������˿ڴ��ڣ����м̽ڵ������˿���������
                Connect(view.inputPortViews[0], outputPort);
            if (inputPort != null) //��ָ��������˿ڴ��ڣ����м̽ڵ������˿���������
                Connect(inputPort, view.outputPortViews[0]);

            return view;
        }

        /// <summary>
        /// Update all the serialized property bindings (in case a node was deleted / added, the property pathes needs to be updated)
        /// �����������л������԰󶨣����ɾ��/��ӽڵ㣬����Ҫ��������·����
        /// </summary>
        public void SyncSerializedPropertyPathes()
        {
            foreach (var nodeView in nodeViews)
                nodeView.SyncSerializedPropertyPathes();
            nodeInspector.RefreshNodes();
        }

        /// <summary>
        /// Call this function when you want to remove this view
        /// ������Ҫ�Ƴ������ͼʱ�������������
        /// </summary>
        public void Dispose()
        {
            ClearGraphElements();//���ͼ�������е�Ԫ��
            RemoveFromHierarchy();//��ͼ���Ƴ�
            Undo.undoRedoPerformed -= ReloadView;//ɾ������ͼ���ί�з���
            Object.DestroyImmediate(nodeInspector);
            NodeProvider.UnloadGraph(graph);
            exposedParameterFactory.Dispose();
            exposedParameterFactory = null;

            graph.onExposedParameterListChanged -= OnExposedParameterListChanged;
            graph.onExposedParameterModified += (s) => onExposedParameterModified?.Invoke(s);
            graph.onGraphChanges -= GraphChangesCallback;
        }

        #endregion
    }
}