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
    /// Base class to write a custom view for a node 为节点编写自定义视图的基类（Graph图表的基类）。
    /// 继承此类可实现不同界面风格的图表。主要是对图表的初始化、操作(鼠标操作，复制/粘贴，菜单栏，添加/移除元素等)、序列化/反序列化等方面进行功能实现
    /// </summary>
    public class BaseGraphView : GraphView, IDisposable
    {
        public delegate void ComputeOrderUpdatedDelegate();
        public delegate void NodeDuplicatedDelegate(BaseNode duplicatedNode, BaseNode newNode);

        /// <summary>
        /// Graph that owns of the node 拥有节点的图
        /// </summary>
        public BaseGraph graph;

        /// <summary>
        /// 对应的窗口
        /// </summary>
        public BaseGraphWindow graphWindow;

        /// <summary>
        /// 绑定graph中节点的对象的字典
        /// </summary>
        public Dictionary<string, RDTSBehavior> bindObjPerGuid = new Dictionary<string, RDTSBehavior>();


        /// <summary>
        /// Connector listener that will create the edges between ports 将在端口之间创建边缘的连接器侦听器：具有连线的相关信息
        /// </summary> 
        public BaseEdgeConnectorListener connectorListener;

        /// <summary>
        /// List of all node views in the graph 图表中所有节点视图的列表
        /// </summary>
        /// <typeparam name="BaseNodeView"></typeparam>
        /// <returns></returns>
        public List<BaseNodeView> nodeViews = new List<BaseNodeView>();

        /// <summary>
        /// Dictionary of the node views accessed view the node instance, faster than a Find in the node view list
        /// 访问的节点视图字典查看节点实例，比在节点视图列表中查找更快
        /// </summary>
        /// <typeparam name="BaseNode"></typeparam>
        /// <typeparam name="BaseNodeView"></typeparam>
        /// <returns></returns>
        public Dictionary<BaseNode, BaseNodeView> nodeViewsPerNode = new Dictionary<BaseNode, BaseNodeView>();

        /// <summary>
        /// List of all edge views in the graph 图表中所有连线视图的列表
        /// </summary>
        /// <typeparam name="EdgeView"></typeparam>
        /// <returns></returns>
        public List<EdgeView> edgeViews = new List<EdgeView>();

        /// <summary>
        /// List of all group views in the graph 图表中所有group视图的列表
        /// </summary>
        /// <typeparam name="GroupView"></typeparam>
        /// <returns></returns>
        public List<GroupView> groupViews = new List<GroupView>();

#if UNITY_2020_1_OR_NEWER
        /// <summary>
        /// List of all sticky note views in the graph 图表中所有便笺节点视图的列表
        /// </summary>
        /// <typeparam name="StickyNoteView"></typeparam>
        /// <returns></returns>
        public List<StickyNoteView> stickyNoteViews = new List<StickyNoteView>();
#endif

        /// <summary>
        /// List of all stack node views in the graph 图表中所有堆栈节点视图的列表
        /// </summary>
        /// <typeparam name="BaseStackNodeView"></typeparam>
        /// <returns></returns>
        public List<BaseStackNodeView> stackNodeViews = new List<BaseStackNodeView>();

        /// <summary>字典（PinnedElementView或其子类，PinnedElementView）</summary>
		Dictionary<Type, PinnedElementView> pinnedElements = new Dictionary<Type, PinnedElementView>();

        CreateNodeMenuWindow createNodeMenu;

        /// <summary>
        /// Triggered just after the graph is initialized 在graph被初始化后触发
        /// </summary>
        public event Action initialized;

        /// <summary>
        /// Triggered just after the compute order of the graph is updated 在graph的计算顺序更新后立即触发
        /// </summary>
        public event ComputeOrderUpdatedDelegate computeOrderUpdated;

        // Safe event relay from BaseGraph (safe because you are sure to always point on a valid BaseGraph
        // when one of these events is called), a graph switch can occur between two call tho
        //来自 BaseGraph 的安全事件中继（安全，因为当调用这些事件之一时，您确保始终指向有效的 BaseGraph），
        //可以在两个调用之间发生图形切换
        /// <summary>
        /// Same event than BaseGraph.onExposedParameterListChanged 与 BaseGraph.onExposedParameterListChanged 
        /// Safe event (not triggered in case the graph is null).
        /// 与 BaseGraph.onExposedParameterListChanged 和 BaseGraph.onExposedParameterListChanged 相同的事件
        /// 安全事件（在图表为空的情况下不触发)
        /// </summary>
        public event Action onExposedParameterListChanged;

        /// <summary>
        /// Same event than BaseGraph.onExposedParameterModified 
        /// Safe event (not triggered in case the graph is null).
        /// 与 BaseGraph.onExposedParameterModified 相同的事件
        /// 安全事件（在图表为空的情况下不触发)
        /// </summary>
        public event Action<ExposedParameter> onExposedParameterModified;

        /// <summary>
        /// Triggered when a node is duplicated (crt-d) or copy-pasted (crtl-c/crtl-v)
        /// 在一个节点被重复、复制或粘贴时触发
        /// </summary>
        public event NodeDuplicatedDelegate nodeDuplicated;

        /// <summary>
        /// Object to handle nodes that shows their UI in the inspector. 用于处理在检视面板中显示其 UI 的节点的对象
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
        /// 用于创建公开参数属性字段的解决方法对象
        /// </summary>
        public ExposedParameterFieldFactory exposedParameterFactory { get; private set; }

        /// <summary>序列化graph</summary>
		public SerializedObject serializedGraph { get; private set; }

        Dictionary<Type, (Type nodeType, MethodInfo initalizeNodeFromObject)> nodeTypePerCreateAssetType = new Dictionary<Type, (Type, MethodInfo)>();

        /// <summary>
        /// 构造函数：添加相关操作的委托方法，注册鼠标操作的回调，初始化操控器，鼠标滚轮缩放，搜索窗口初始化
        /// </summary>
        /// <param name="window"></param>
		public BaseGraphView(EditorWindow window)
        {
            ///serializeGraphElements：用于序列化图形元素的委托
            serializeGraphElements = SerializeGraphElementsCallback;//用于序列化图表graph元素，以便实现复制/粘贴和其他操作的回调
            ///canPasteSerializedData：用于查看序列化数据是否可以粘贴的委托
            canPasteSerializedData = CanPasteSerializedDataCallback;//询问序列化数据是否可以粘贴
            ///unserializeAndPaste：用于取消序列化并粘贴元素的委托
            unserializeAndPaste = UnserializeAndPasteCallback;//用于取消序列化图形元素并将其添加到图中的回调
            ///graphViewChanged：用于指示 GraphView 中的更改（通常由操控器执行）的委托
            graphViewChanged = GraphViewChangedCallback;//在图中发生特定更改时使用的回调。请参阅 GraphViewChange
            ///viewTransformChanged：视图变换更改（缩放）委托
            viewTransformChanged = ViewTransformChangedCallback;//视图变换更改回调
            ///elementResized：元素调整大小委托
            elementResized = ElementResizedCallback;//Group元素调整大小回调

            RegisterCallback<KeyDownEvent>(KeyDownCallback);//按键回调函数：保存、快捷对齐节点
            RegisterCallback<DragPerformEvent>(DragPerformedCallback);//拖动行为的回调：可从“黑板”中的ExposedParameterField、或是Hierarchy面板、Project中拖拽对象至graph中，在鼠标位置处创建新的节点
            RegisterCallback<DragUpdatedEvent>(DragUpdatedCallback);//拖动元素更新的回调：鼠标光标的可视指示，更新检视面板
            RegisterCallback<MouseDownEvent>(MouseDownCallback);//鼠标按下回调：左键单击，关闭所有设置窗口；按照当前选中的节点情况是否更新检视面板
            RegisterCallback<MouseUpEvent>(MouseUpCallback);//鼠标抬起回调：按照当前选中的节点情况是否更新检视面板

            InitializeManipulators();//初始化（添加）所需操控器
            ///AddGrid();//网格

            SetupZoom(0.1f, 2f);//缩放（鼠标滚轮）

            Undo.undoRedoPerformed += ReloadView;//“撤销”或“重载”操作：添加重载图表的委托方法

            //搜索树(窗口)
            createNodeMenu = ScriptableObject.CreateInstance<CreateNodeMenuWindow>();
            createNodeMenu.Initialize(this, window);

            this.StretchToParentSize();
        }

        /// <summary>创建节点检视器</summary>
		protected virtual NodeInspectorObject CreateNodeInspectorObject()
        {
            var inspector = ScriptableObject.CreateInstance<NodeInspectorObject>();
            inspector.name = "Node Inspector";//检视器名称
            inspector.hideFlags = HideFlags.HideAndDontSave ^ HideFlags.NotEditable;

            return inspector;
        }



        #region Callbacks 回调

        protected override bool canCopySelection
        {
            get { return selection.Any(e => e is BaseNodeView || e is GroupView); }
        }

        protected override bool canCutSelection
        {
            get { return selection.Any(e => e is BaseNodeView || e is GroupView); }
        }

        /// <summary>
        /// 序列化图表graph元素的回调：返回选中对象的JSON格式数据
        /// </summary>
        /// <param name="elements">要进行序列化的元素：选中的元素</param>
        /// <returns></returns>
		string SerializeGraphElementsCallback(IEnumerable<GraphElement> elements)
        {
            var data = new CopyPasteHelper();

            //在copiedNodes和copiedEdges列表中添加 [选中项] 中对应的Nodes  及其连接的edges
            foreach (BaseNodeView nodeView in elements.Where(e => e is BaseNodeView))//遍历选中的节点
            {
                data.copiedNodes.Add(JsonSerializer.SerializeNode(nodeView.nodeTarget));//将节点序列化成JsonElement类型，并添加到copiedNodes列表中
                foreach (var port in nodeView.nodeTarget.GetAllPorts())//遍历此节点所有的端口
                {
                    if (port.portData.vertical)//若垂直显示
                    {
                        foreach (var edge in port.GetEdges())
                            data.copiedEdges.Add(JsonSerializer.Serialize(edge));//将edge序列化成JsonElement类型，并添加到copiedEdges列表中
                    }
                }
            }
            //在copiedGroups列表中添加 [选中项] 对应的Groups
            foreach (GroupView groupView in elements.Where(e => e is GroupView))//遍历选中的Group
                data.copiedGroups.Add(JsonSerializer.Serialize(groupView.group));//将Group序列化成JsonElement类型，并添加到copiedGroups列表中
            //在copiedEdges列表中添加 [选中项] 对应的Edges
            foreach (EdgeView edgeView in elements.Where(e => e is EdgeView))
                data.copiedEdges.Add(JsonSerializer.Serialize(edgeView.serializedEdge));//将edge序列化成JsonElement类型，并添加到copiedEdges列表中

            ClearSelection();//清除所选项

            return JsonUtility.ToJson(data, true);//JSON格式的对象数据
            ///参数1：要转换为 JSON 形式的对象
            ///参数2：如果为 true，则格式化输出以实现可读性。如果为 false，则格式化输出以实现最小大小。默认为 false
        }

        /// <summary>
        /// 序列化数据是否可以粘贴
        /// </summary>
        /// <param name="serializedData">序列化的图形元素</param>
        /// <returns></returns>
		bool CanPasteSerializedDataCallback(string serializedData)
        {
            try
            {
                return JsonUtility.FromJson(serializedData, typeof(CopyPasteHelper)) != null;//通过JSON表示形式创建对象，返回object对象的实例
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 取消图表元素序列化并将其添加到图中 的回调：分别对节点、Group、Edge三类元素进行反序列化，实现元素的粘贴
        /// </summary>
        /// <param name="operationName">撤销/重做标签的操作名称</param>
        /// <param name="serializedData">序列化数据</param>
		void UnserializeAndPasteCallback(string operationName, string serializedData)
        {
            var data = JsonUtility.FromJson<CopyPasteHelper>(serializedData);//通过JSON表示形式创建对象

            RegisterCompleteObjectUndo(operationName);//注册撤销

            Dictionary<string, BaseNode> copiedNodesMap = new Dictionary<string, BaseNode>();//（GUID， BaseNode）

            var unserializedGroups = data.copiedGroups.Select(g => JsonSerializer.Deserialize<Group>(g)).ToList();//将data中的Group的JSON数据反序列化成Group类型的列表

            foreach (var serializedNode in data.copiedNodes)///遍历JSON形式的节点数据
			{
                var node = JsonSerializer.DeserializeNode(serializedNode);//反序列成BaseNode

                if (node == null)
                    continue;

                string sourceGUID = node.GUID;
                graph.nodesPerGUID.TryGetValue(sourceGUID, out var sourceNode);//通过被反序列化节点的GUID获取到对应的节点
                                                                               //Call OnNodeCreated on the new fresh copied node 当有新的复制节点时调用OnNodeCreated()方法
                node.createdFromDuplication = true;//置位标志位：是从复制/粘贴创建的节点
                node.createdWithinGroup = unserializedGroups.Any(g => g.innerNodeGUIDs.Contains(sourceGUID));//判断：是否是在同一个group内进行复制/粘贴的
                node.OnNodeCreated();
                //And move a bit the new node 当复制/粘贴出新节点时，将其位置与原节点错位一些，便于区别
                node.position.position += new Vector2(20, 20);

                var newNodeView = AddNode(node);//添加一个节点至图表中（在BaseGraph类中添加节点信息，在BaseGraphView中添加可视化的节点元素）

                // If the nodes were copied from another graph, then the source is null  
                // 如果节点是从另一个图表中复制/粘贴过来的，则其(在此图表中的)来源是空的
                if (sourceNode != null)
                    nodeDuplicated?.Invoke(sourceNode, node);
                copiedNodesMap[sourceGUID] = node;//加入字典

                //Select the new node 
                ///AddToSelection：向所选项添加元素
                AddToSelection(nodeViewsPerNode[node]);//选中并高亮！
            }

            foreach (var group in unserializedGroups)///遍历反序列化后的group
            {
                //Same than for node
                group.OnCreated();

                // try to centre the created node in the screen 应该是错位以便于区别
                group.position.position += new Vector2(20, 20);

                var oldGUIDList = group.innerNodeGUIDs.ToList();
                group.innerNodeGUIDs.Clear();
                foreach (var guid in oldGUIDList)
                {
                    graph.nodesPerGUID.TryGetValue(guid, out var node);

                    // In case group was copied from another graph 
                    // 如果group是从另一个图表复制的
                    if (node == null)//从一个graph中复制过来说明在这个graph的nodesPerGUID字典中找不到对应的元素
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

            foreach (var serializedEdge in data.copiedEdges)///遍历JSON形式的edge数据
            {
                var edge = JsonSerializer.Deserialize<SerializableEdge>(serializedEdge);//先反序列化成SerializableEdge

                edge.Deserialize();

                // Find port of new nodes:  找到新节点的端口
                copiedNodesMap.TryGetValue(edge.inputNode.GUID, out var oldInputNode);
                copiedNodesMap.TryGetValue(edge.outputNode.GUID, out var oldOutputNode);

                // We avoid to break the graph by replacing unique connections: 我们避免 通过替换唯一连接来破坏graph
                if (oldInputNode == null && !edge.inputPort.portData.acceptMultipleEdges || !edge.outputPort.portData.acceptMultipleEdges)
                    continue;

                oldInputNode = oldInputNode ?? edge.inputNode;
                oldOutputNode = oldOutputNode ?? edge.outputNode;

                var inputPort = oldInputNode.GetPort(edge.inputPort.fieldName, edge.inputPortIdentifier);//获取edge的输入端口
                var outputPort = oldOutputNode.GetPort(edge.outputPort.fieldName, edge.outputPortIdentifier);//获取edge的输出端口

                var newEdge = SerializableEdge.CreateNewEdge(graph, inputPort, outputPort);//创建一条新SerializableEdge

                if (nodeViewsPerNode.ContainsKey(oldInputNode) && nodeViewsPerNode.ContainsKey(oldOutputNode))//若已包含了此SerializableEdge的输入和输出端口，则创建对应的EdgeView以在graph中画线
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
        /// 用于指示 GraphView 中的更改（通常由操控器执行）的委托。可以按原样返回结构，也可以修改列表以更改 GraphView 将要执行的操作
        /// 在图中发生特定更改时使用的回调：对图表中要删除的元素（node，edge）进行处理
        /// </summary>
        /// <param name="changes">更改结构</param>
        /// <returns></returns>
        GraphViewChange GraphViewChangedCallback(GraphViewChange changes)//GraphViewChange：图中可以拦截的一组更改
        {
            if (changes.elementsToRemove != null)///elementsToRemove：即将删除的元素(列表)
            {
                RegisterCompleteObjectUndo("Remove Graph Elements");

                // Destroy priority of objects 销毁对象的优先级
                // We need nodes to be destroyed first because we can have a destroy operation that uses node connections
                // 我们需要首先销毁节点，因为我们可以进行使用节点连接的销毁操作
                changes.elementsToRemove.Sort((e1, e2) => {
                    int GetPriority(GraphElement e)//获取优先级：BaseNodeView类型元素为0
                    {
                        if (e is BaseNodeView)
                            return 0;
                        else
                            return 1;
                    }
                    ///CompareTo：将此实例与指定的 32 位有符号整数进行比较并返回对其相对值的指示。（-1为小于，0为等于，1为大于）
					return GetPriority(e1).CompareTo(GetPriority(e2));
                });

                //Handle ourselves the edge and node remove 处理所有edge和节点删除
                changes.elementsToRemove.RemoveAll(e => {

                    switch (e)
                    {
                        case EdgeView edge:
                            Disconnect(edge);//断开EdgeView的连接，删除EdgeView，更新计算顺序、端口视图
                            return true;
                        case BaseNodeView nodeView:
                            // For vertical nodes, we need to delete them ourselves as it's not handled by GraphView
                            // 对于垂直节点，我们需要自己删除它们，因为它不是由 GraphView 处理的
                            ///Concat：连接两个序列
                            foreach (var pv in nodeView.inputPortViews.Concat(nodeView.outputPortViews))//遍历 将节点输入、输出端口连接后的序列
                                if (pv.orientation == Orientation.Vertical)//若为垂直节点
                                    foreach (var edge in pv.GetEdges().ToList())
                                        Disconnect(edge);//断开EdgeView的连接，删除EdgeView，更新计算顺序、端口视图

                            nodeInspector.NodeViewRemoved(nodeView);//从自定义的检视器中的列表中移除
                            ExceptionToLog.Call(() => nodeView.OnRemoved());
                            graph.RemoveNode(nodeView.nodeTarget);//从节点相关列表中移除
                            UpdateSerializedProperties();
                            RemoveElement(nodeView);//从graph中移除元素
                            if (Selection.activeObject == nodeInspector)
                                UpdateNodeInspectorSelection();

                            SyncSerializedPropertyPathes();//更新所有序列化的属性绑定
                            return true;
                        case GroupView group:
                            graph.RemoveGroup(group.group);
                            UpdateSerializedProperties();
                            RemoveElement(group);
                            return true;
                        case ExposedParameterFieldView blackboardField://黑板字段
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
        /// graph改变的回调函数
        /// </summary>
        /// <param name="changes"></param>
		void GraphChangesCallback(GraphChanges changes)
        {
            if (changes.removedEdge != null)//若存在要移除的edge，就断开、删除edgeViews列表中对应的元素
            {
                var edge = edgeViews.FirstOrDefault(e => e.serializedEdge == changes.removedEdge);

                DisconnectView(edge);
            }
        }

        /// <summary>
        /// 对图表graph的位置和缩放的设置
        /// </summary>
        /// <param name="view">GraphView 引用</param>
		void ViewTransformChangedCallback(GraphView view)
        {
            if (graph != null)
            {
                ///viewTransform：图表的视图变换
				graph.position = viewTransform.position;//position：此 VisualElement 的变换的位置
                graph.scale = viewTransform.scale;//scale：此 VisualElement 的变换的缩放
            }
        }

        /// <summary>
        /// 设置GroupView元素的尺寸（宽度，高度）
        /// </summary>
        /// <param name="elem">调整大小的元素</param>
        void ElementResizedCallback(VisualElement elem)
        {
            var groupView = elem as GroupView;

            if (groupView != null)//若是GroupView类型元素
                groupView.group.size = groupView.GetPosition().size;//设置GroupView的尺寸（宽度，高度）
        }

        /// <summary>
        /// [重载]获取与给定端口兼容的所有端口。
        /// 要求：不能是同一个节点上的端口，端口方向不同，端口的类型可以在图表上下文连接，没有存在的edge以将端口连接
        /// </summary>
        /// <param name="startPort"></param>
        /// <param name="nodeAdapter"></param>
        /// <returns></returns>
		public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();

            //ports：当前图中所有的端口
            compatiblePorts.AddRange(ports.ToList().Where(p => {
                var portView = p as PortView;

                if (portView.owner == (startPort as PortView).owner)//若为同一个节点
                    return false;

                if (p.direction == startPort.direction)//若端口方向相同
                    return false;

                //Check for type assignability 检查类型可分配性
                if (!BaseGraph.TypesAreConnectable(startPort.portType, p.portType))//若两种类型不可以在图的上下文中连接
                    return false;

                //Check if the edge already exists 检查edge是否已经存在
                if (portView.GetEdges().Any(e => e.input == startPort || e.output == startPort))//若edge已经连接到startPort上
                    return false;

                return true;
            }));

            return compatiblePorts;
        }

        /// <summary>
        /// Build the contextual menu shown when right clicking inside the graph view
        /// 构建当在图表中右击时显示的上下文菜单
        /// </summary>
        /// <param name="evt"></param>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);//向上下文菜单添加菜单项
            BuildGroupContextualMenu(evt, 1);//“Create Group”菜单项
            BuildStackNodeContextualMenu(evt, 2);//"Create Stack"菜单项
            BuildStickyNoteContextualMenu(evt, 3);//"Create Sticky Note"菜单项

            BuildDebugAllContextualMenu(evt);//"Debug All"菜单项：是否显示所有节点的计算顺序
            BuildSelectAssetContextualMenu(evt);//"Select Asset"菜单项：在project中选中当前graph对应的.asset文件
            BuildSaveAssetContextualMenu(evt);//"Save Asset"菜单项：保存.asset文件
            //BuildViewContextualMenu(evt);//"View/Processor"菜单项：是否开启Process panel
            //BuildHelpContextualMenu(evt);//"Help/Reset Pinned Windows"菜单项
        }

        /// <summary>
        /// Add the New Group entry to the context menu  在菜单栏总添加创建group的选项
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
        /// 添加一个Stack
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
            //AppendAction：在下拉菜单中添加一个将执行操作的菜单项。此菜单项添加在当前菜单项列表末尾
            //evt.menu.AppendAction("View/Processor", (e) => ToggleView< ProcessorView >(), (e) => GetPinnedElementStatus< ProcessorView >());
            evt.menu.AppendAction("View/Processor", (e) => ToggleView<ProcessorView>(), DropdownMenuAction.AlwaysEnabled);
        }


        bool isNodesDebugAll = false;//是否所有节点都已Debug
        /// <summary>
        /// 操作图表中所有节点的计算顺序标签的显示
        /// </summary>
        /// <param name="evt"></param>
        protected virtual void BuildDebugAllContextualMenu(ContextualMenuPopulateEvent evt)
        {
            
            evt.menu.AppendAction("Debug All", (e) => SetNodeDebugAll(), DebugAllStatus);
        }

        //判断图表中所有的节点是否都已显示计算顺序
        Status DebugAllStatus(DropdownMenuAction action)
        {
            bool debugAll = (this.graph.nodes.Any(n => n.debug != true)) ? false : true;

            if (debugAll)
                return Status.Checked;//菜单项显示为带复选标记
            return Status.Normal;
        }

        //设置所有节点的debug状态
        void SetNodeDebugAll()
        {
            isNodesDebugAll = !isNodesDebugAll;
            this.graph.nodes.ForEach(n =>{
                if(n.debug != isNodesDebugAll)//只修改不同的节点
                    nodeViewsPerNode[n].ToggleDebug();

            });

        }


        /// <summary>
        /// Add the Select Asset entry to the context menu
        /// </summary>
        /// <param name="evt"></param>
        protected virtual void BuildSelectAssetContextualMenu(ContextualMenuPopulateEvent evt)
        {
            ///PingObject：在场景中对对象执行 Ping 操作，就像在检视面板中单击它一样
			evt.menu.AppendAction("Select Asset", (e) => EditorGUIUtility.PingObject(graph), DropdownMenuAction.AlwaysEnabled);
        }

        /// <summary>
        /// Add the Save Asset entry to the context menu 保存资产
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
        /// Add the Help entry to the context menu 重置黑板元素位置至起始位置
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
        /// 按键回调函数：保存、快捷对齐节点
        ///  [注意：需要修改部分按键，因为和unity、windows的快捷键重合]
        /// </summary>
        /// <param name="e"></param>
		protected virtual void KeyDownCallback(KeyDownEvent e)
        {
            if (e.keyCode == KeyCode.S && e.commandKey)//如果按下了S键 和 Windows/Ctr/ Cmd(Mac转有) 键
            {
                SaveGraphToDisk();
                e.StopPropagation();

            }
            else if (nodeViews.Count > 0 && e.commandKey && e.altKey)
            {
                //	Node Aligning shortcuts 节点对齐快捷键
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
        /// 鼠标抬起回调：按照当前选中的节点情况是否更新检视面板
        /// </summary>
        /// <param name="e"></param>
		void MouseUpCallback(MouseUpEvent e)
        {
            schedule.Execute(() => {
                if (DoesSelectionContainsInspectorNodes())
                    UpdateNodeInspectorSelection();
            }).ExecuteLater(1);//1MS后执行

            mouseUpPos = e.mousePosition;
        }


        Vector2 mouseDownPos;

        /// <summary>
        /// 鼠标按下回调：左键单击，关闭所有设置窗口；按照当前选中的节点情况是否更新检视面板
        /// </summary>
        /// <param name="e"></param>
		void MouseDownCallback(MouseDownEvent e)
        {
            // When left clicking on the graph (not a node or something else) 左键单击图形时（不是节点或其他东西）
            if (e.button == 0)//鼠标左键单击时，关闭所有 设置窗口
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
        /// 若选中的节点中有不是“要显示在检视面板的节点” 或 “要显示在检视面板的节点”没有被全部选中，则返回true；否则返回false
        /// </summary>
        /// <returns></returns>
        bool DoesSelectionContainsInspectorNodes()
        {
            var selectedNodes = selection.Where(s => s is BaseNodeView).ToList();//获取当前选中的所有节点
            ///Except：通过使用默认的相等比较器对值进行比较，生成两个序列的差集
            var selectedNodesNotInInspector = selectedNodes.Except(nodeInspector.selectedNodes).ToList();//返回：在selectedNodes列表中，但不在nodeInspector.selectedNodes中的元素的序列
            var nodeInInspectorWithoutSelectedNodes = nodeInspector.selectedNodes.Except(selectedNodes).ToList();//返回：在nodeInspector.selectedNodes集中，但不在selectedNodes中的元素的序列

            // Debug.Log($"selectedNodes:{selectedNodes.Count} ; nodeInspector.selectedNodes:{nodeInspector.selectedNodes.Count}");
            return selectedNodesNotInInspector.Any() || nodeInInspectorWithoutSelectedNodes.Any();
        }

        /// <summary>
        /// 拖动行为的回调：可从“黑板”中的ExposedParameterField、或是Hierarchy面板、Project中拖拽对象至graph中，在鼠标位置处创建新的节点
        /// </summary>
        /// <param name="e"></param>
		void DragPerformedCallback(DragPerformEvent e)
        {
            ///currentTarget：事件的当前目标。当前路径是传播路径中目前正在为其执行事件处理程序的元素
            var mousePos = (e.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, e.localMousePosition);
            ///GetGenericData：获取与当前拖放操作相关的数据
			var dragData = DragAndDrop.GetGenericData("DragSelection") as List<ISelectable>;

            // Drag and Drop for elements inside the graph  拖放图表内的元素
            // ①对黑板中的ExposedParameterField拖放
            if (dragData != null)
            {
                ///OfType：根据指定类型筛选 IEnumerable 的元素
				var exposedParameterFieldViews = dragData.OfType<ExposedParameterFieldView>();//筛选ExposedParameterFieldView的元素
                if (exposedParameterFieldViews.Any())//若包含任何ExposedParameterFieldView类型的元素
                {
                    foreach (var paramFieldView in exposedParameterFieldViews)
                    {
                        RegisterCompleteObjectUndo("Create Parameter Node");
                        var paramNode = BaseNode.CreateFromType<ParameterNode>(mousePos);//在mousePos位置创建一个类型为ParameterNode的节点
                        paramNode.parameterGUID = paramFieldView.parameter.guid;//对ParameterNode的parameterGUID赋值
                        AddNode(paramNode);
                    }
                }
            }

            // ②External objects drag and drop 外部对象拖放(从Hierarchy面板、Project中拖拽对象至graph中)
            if (DragAndDrop.objectReferences.Length > 0)//objectReferences：对拖动的 objects 的引用
            {
               
                RegisterCompleteObjectUndo("Create Node From Object(s)");
                foreach (var obj in DragAndDrop.objectReferences)
                {
                  
                    var objectType = obj.GetType();

                    foreach (var kp in nodeTypePerCreateAssetType)
                    {
                       
                        if (kp.Key.IsAssignableFrom(objectType))//IsAssignableFrom：确定指定类型objectType的实例是否能分配给当前类型的变量
                        {
                            try
                            {
                                var node = BaseNode.CreateFromType(kp.Value.nodeType, mousePos);
                                
                                if ((bool)kp.Value.initalizeNodeFromObject.Invoke(node, new[] { obj }))
                                {
                                    AddNode(node);
                                    ///Debug.Log("外部对象拖放");
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
        /// 拖动元素更新的回调：鼠标光标的可视指示，更新检视面板
        /// </summary>
        /// <param name="e"></param>
		void DragUpdatedCallback(DragUpdatedEvent e)
        {
            ///GetGenericData：获取与当前拖放操作相关的数据   objectReferences：对拖动的 objects 的引用
            var dragData = DragAndDrop.GetGenericData("DragSelection") as List<ISelectable>;
            var dragObjects = DragAndDrop.objectReferences;
            bool dragging = false;

            if (dragData != null)
            {
                // Handle drag from exposed parameter view 从ExposedParameterFieldView中处理拖动
                if (dragData.OfType<ExposedParameterFieldView>().Any())
                {
                    dragging = true;
                }
            }

            if (dragObjects.Length > 0)
                dragging = true;

            if (dragging)
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;//鼠标光标拖放操作的可视指示

            UpdateNodeInspectorSelection();//更新检视面板
        }

        #endregion

        #region Initialization 初始化

        /// <summary>
        /// 重载视图：对图表graph反序列化，记录下当前选中的节点元素；
        /// 移除所有元素，在重新初始化所有元素；更新计算顺序；将记录的节点元素标为选中并高亮；
        /// 更新节点检视器的选择
        /// </summary>
        void ReloadView()
        {
            // Force the graph to reload his data (Undo have updated the serialized properties of the graph
            // so the one that are not serialized need to be synchronized)
            // 强制图表重新加载他的数据（撤消已经更新了图表的序列化属性，因此未序列化的需要同步）
            graph.Deserialize();

            // Get selected nodes 获取已选中的节点
            var selectedNodeGUIDs = new List<string>();
            foreach (var e in selection)//遍历当前在图表中所有选中的元素
            {
                if (e is BaseNodeView v && this.Contains(v))//若是BaseNodeView类型 且 此元素是此graphView的直接子级
                    selectedNodeGUIDs.Add(v.nodeTarget.GUID);//记录此节点的GUID
            }

            // Remove everything 移除所有的元素
            RemoveNodeViews();
            RemoveEdges();
            RemoveGroups();
#if UNITY_2020_1_OR_NEWER
            RemoveStrickyNotes();
#endif
            RemoveStackNodeViews();

            UpdateSerializedProperties();//更新graph的序列化属性

            // And re-add with new up to date datas 重新添加新的最新数据（重新初始化所有元素）
            InitializeNodeViews();
            InitializeEdgeViews();
            InitializeGroups();
            InitializeStickyNotes();
            InitializeStackNodes();

            Reload();

            UpdateComputeOrder();//更新图中节点的计算顺序，并触发相应事件函数

            // Restore selection after re-creating all views 重新创建所有视图后恢复选择
            // selection = nodeViews.Where(v => selectedNodeGUIDs.Contains(v.nodeTarget.GUID)).Select(v => v as ISelectable).ToList();
            foreach (var guid in selectedNodeGUIDs)
            {
                AddToSelection(nodeViews.FirstOrDefault(n => n.nodeTarget.GUID == guid));//选中并高亮
            }

            UpdateNodeInspectorSelection();//更新节点检视器的选择
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

            //初始化graph和所有类型元素
            InitializeGraphView();//初始化Graph视图，注册事件
            InitializeNodeViews();//根据BaseGraph.nodes初始化所有Node视图
            InitializeEdgeViews();
            InitializeViews();
            InitializeGroups();
            InitializeStickyNotes();
            InitializeStackNodes();

            initialized?.Invoke();
            UpdateComputeOrder();

            InitializeView();

            NodeProvider.LoadGraph(graph);

            // Register the nodes that can be created from assets 注册可以从资产创建的节点
            foreach (var nodeInfo in NodeProvider.GetNodeMenuEntries(graph))
            {
                var interfaces = nodeInfo.type.GetInterfaces();
                var exceptInheritedInterfaces = interfaces.Except(interfaces.SelectMany(t => t.GetInterfaces()));
                foreach (var i in interfaces)
                {
                    if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICreateNodeFrom<>))
                    {
                        var genericArgumentType = i.GetGenericArguments()[0];//GetGenericArguments：返回表示封闭式泛型类型的类型参数或泛型类型定义的类型参数的 Type 对象的数组
                        var initializeFunction = nodeInfo.type.GetMethod(
                            nameof(ICreateNodeFrom<Object>.InitializeNodeFromObject),//找到此方法
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                            null, new Type[] { genericArgumentType }, null
                        );

                        // We only add the type that implements the interface, not it's children
                        // 我们只添加实现接口的类型，而不是子类型
                        if (initializeFunction.DeclaringType == nodeInfo.type)
                            nodeTypePerCreateAssetType[genericArgumentType] = (nodeInfo.type, initializeFunction);
                    }
                }
            }
        }

        /// <summary>清除图表中所有的元素</summary>
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

        /// <summary>更新graph的序列化属性</summary>
		void UpdateSerializedProperties()
        {
            serializedGraph = new SerializedObject(graph);//为检查的对象graph 创建 SerializedObject
        }

        /// <summary>
        /// Allow you to create your own edge connector listener  允许创建自己的EdgeConnectorListener
        /// </summary>
        /// <returns></returns>
        protected virtual BaseEdgeConnectorListener CreateEdgeConnectorListener()
         => new BaseEdgeConnectorListener(this);

        /// <summary>初始化图表视图：添加相关委托，图表位置和缩放，搜索窗口 </summary>
		void InitializeGraphView()
        {
            graph.onExposedParameterListChanged += OnExposedParameterListChanged;
            graph.onExposedParameterModified += (s) => onExposedParameterModified?.Invoke(s);
            graph.onGraphChanges += GraphChangesCallback;
            //图表视图的位置和缩放
            viewTransform.position = graph.position;
            viewTransform.scale = graph.scale;
            ///nodeCreationRequest：在用户请求显示节点创建窗口时使用的回调
            ///NodeCreationContext：此结构表示用户开始创建图形节点时的上下文
            //生成“Create Node”菜单来创建搜索窗口
            nodeCreationRequest = (c) => SearchWindow.Open(new SearchWindowContext(c.screenMousePosition), createNodeMenu);
        }

        /// <summary>公开的参数列表已更改</summary>
		void OnExposedParameterListChanged()
        {
            UpdateSerializedProperties();
            onExposedParameterListChanged?.Invoke();
        }

        /// <summary>初始化节点视图NodeView</summary>
		void InitializeNodeViews()
        {
            graph.nodes.RemoveAll(n => n == null);//清除空的节点

            foreach (var node in graph.nodes)
            {
                
                var v = AddNodeView(node);//添加节点对应的NodeView
            }
        }

        /// <summary>
        /// 初始化edgeView：先清除为空或破损的edge，
        /// </summary>
		void InitializeEdgeViews()
        {
            // Sanitize edges in case a node broke something while loading 清理边缘以防节点在加载时破坏某些东西
            graph.edges.RemoveAll(edge => edge == null || edge.inputNode == null || edge.outputNode == null);//清除为空或破损的edge

            foreach (var serializedEdge in graph.edges)
            {
                nodeViewsPerNode.TryGetValue(serializedEdge.inputNode, out var inputNodeView);//获取edge的输入节点对应的NodeView
                nodeViewsPerNode.TryGetValue(serializedEdge.outputNode, out var outputNodeView);//获取edge的输出节点对应的NodeView
                if (inputNodeView == null || outputNodeView == null)//若均为空则跳过
                    continue;

                //创建一条新的EdgeView，并设置其输入/输出端口、userData(为此serializedEdge)
                var edgeView = CreateEdgeView();
                edgeView.userData = serializedEdge;
                edgeView.input = inputNodeView.GetPortViewFromFieldName(serializedEdge.inputFieldName, serializedEdge.inputPortIdentifier);
                edgeView.output = outputNodeView.GetPortViewFromFieldName(serializedEdge.outputFieldName, serializedEdge.outputPortIdentifier);


                ConnectView(edgeView);//实现edge与输入/输出端口的连接，若端口不支持多重连接则先将原先连接到端口的edge移除；对颜色错误的edge进行修补
            }
        }

        /// <summary>初始化pinnedElement元素：若有打开的pinnedElement就打开</summary>
		void InitializeViews()
        {
            foreach (var pinnedElement in graph.pinnedElements)
            {
                if (pinnedElement.opened)
                    OpenPinned(pinnedElement.editorType.type);
            }
        }

        /// <summary>初始化Group：添加GroupView至图表中</summary>
        void InitializeGroups()
        {
            foreach (var group in graph.groups)
                AddGroupView(group);
        }

        /// <summary>初始化“标签”元素：添加StickyNoteView至图表中</summary>
		void InitializeStickyNotes()
        {
#if UNITY_2020_1_OR_NEWER
            foreach (var group in graph.stickyNotes)
                AddStickyNoteView(group);
#endif
        }

        /// <summary>初始化堆栈元素：添加BaseStackNodeView至图表中</summary>
		void InitializeStackNodes()
        {
            foreach (var stackNode in graph.stackNodes)
                AddStackNodeView(stackNode);
        }

        protected virtual void InitializeManipulators()
        {
            this.AddManipulator(new ContentDragger());//允许鼠标拖动一个或多个元素
            this.AddManipulator(new SelectionDragger());//选项拖动程序操控器
            this.AddManipulator(new RectangleSelector());//矩形选择框
        }


        class AddGridBackground : GridBackground { }//用继承的类才会生成网格
        /// <summary>
        /// 添加网格
        /// </summary>
        private void AddGrid()
        {
            var grid = new AddGridBackground();
            this.Insert(0, grid);
            grid.StretchToParentSize();//给定VisualElement的左右、上下边缘与父元素的边缘对齐
        }

        protected virtual void Reload() { }

        #endregion

        #region Graph content modification 图表内容修改

        /// <summary>
        /// 更新节点检视器的选择
        /// </summary>
        public void UpdateNodeInspectorSelection()
        {
            if (nodeInspector.previouslySelectedObject != Selection.activeObject)//检查器先前选择的对象不是当前选中的对象，则设置为当前选中的对象
                nodeInspector.previouslySelectedObject = Selection.activeObject;

            HashSet<BaseNodeView> selectedNodeViews = new HashSet<BaseNodeView>();
            nodeInspector.selectedNodes.Clear();//清空
            foreach (var e in selection)//遍历当前在graph中选择的元素
            {
                if (e is BaseNodeView v && this.Contains(v) && v.nodeTarget.needsInspector)//若是节点 且 被此graph包含 且 此节点需要在检视面板中可见
                    selectedNodeViews.Add(v);//加入到集中
            }

            nodeInspector.UpdateSelectedNodes(selectedNodeViews);//将selectedNodeViews集复制到nodeInspector对象的selectedNodes集中
            if (Selection.activeObject != nodeInspector && selectedNodeViews.Count > 0)//若当前选择的对象不是要在检视面板中显示的 且 selectedNodeViews存在元素
                Selection.activeObject = nodeInspector;//将nodeInspector确定为当前选择的对象
        }

        /// <summary>
        /// 添加一个节点至图表中（在BaseGraph类中添加节点信息，在BaseGraphView中添加可视化的节点元素）
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
		public BaseNodeView AddNode(BaseNode node)
        {
            // This will initialize the node using the graph instance  使用图表实例初始化节点
            graph.AddNode(node);//增加一个节点到图表中

            UpdateSerializedProperties();//更新图表graph的序列化属性

            var view = AddNodeView(node);//添加节点对应的nodeView到图表中(添加可视化元素至图表)

            // Call create after the node have been initialized  在节点已被初始化后调用
            ExceptionToLog.Call(() => view.OnCreated());

            UpdateComputeOrder();//更新图中节点的计算顺序，并触发相应事件函数

            return view;
        }

        /// <summary>
        /// 添加指定node的nodeView到graphView中（添加可视化元素至图表）
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
		public BaseNodeView AddNodeView(BaseNode node)
        {
            var viewType = NodeProvider.GetNodeViewTypeFromType(node.GetType());//找到指定nodeType对应的nodeViewType，若没有则找其父类的nodeViewType

            if (viewType == null)
                viewType = typeof(BaseNodeView);

            var baseNodeView = Activator.CreateInstance(viewType) as BaseNodeView;
            baseNodeView.Initialize(this, node);//初始化
            AddElement(baseNodeView);//将该元素添加到graph

            nodeViews.Add(baseNodeView);//加入存储的列表
            nodeViewsPerNode[node] = baseNodeView;//加入存储的字典

            return baseNodeView;
        }

        /// <summary>移除一个节点node及其nodeView</summary>
		public void RemoveNode(BaseNode node)
        {
            var view = nodeViewsPerNode[node];
            RemoveNodeView(view);
            graph.RemoveNode(node);
        }
        /// <summary>移除一个nodeView</summary>
		public void RemoveNodeView(BaseNodeView nodeView)
        {
            RemoveElement(nodeView);
            nodeViews.Remove(nodeView);
            nodeViewsPerNode.Remove(nodeView.nodeTarget);
        }

        /// <summary>移除所有的NodeViews</summary>
		void RemoveNodeViews()
        {
            foreach (var nodeView in nodeViews)
                RemoveElement(nodeView);
            nodeViews.Clear();
            nodeViewsPerNode.Clear();
        }
        /// <summary>移除所有的BaseStackNodeView</summary>
		void RemoveStackNodeViews()
        {
            foreach (var stackView in stackNodeViews)
                RemoveElement(stackView);
            stackNodeViews.Clear();
        }
        /// <summary>移除所有的PinnedElementView</summary>
		void RemovePinnedElementViews()
        {
            foreach (var pinnedView in pinnedElements.Values)
            {
                if (Contains(pinnedView))
                    Remove(pinnedView);
            }
            pinnedElements.Clear();
        }

        /// <summary>添加一个group</summary>
        public GroupView AddGroup(Group block)
        {
            graph.AddGroup(block);
            block.OnCreated();
            return AddGroupView(block);
        }
        /// <summary>添加一个group元素至graph中</summary>
		public GroupView AddGroupView(Group block)
        {
            var c = new GroupView();

            c.Initialize(this, block);

            AddElement(c);

            groupViews.Add(c);
            return c;
        }

        /// <summary>添加一个堆栈元素</summary>
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
        /// <summary>移除一个group元素至graph中</summary>
		public void RemoveStackNodeView(BaseStackNodeView stackNodeView)
        {
            stackNodeViews.Remove(stackNodeView);
            RemoveElement(stackNodeView);
        }

        /// <summary>添加一个“便签”节点</summary>
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
        /// <summary>移除一个“便签”节点</summary>
		public void RemoveStickyNoteView(StickyNoteView view)
        {
            stickyNoteViews.Remove(view);
            RemoveElement(view);
        }
        /// <summary>移除所有的“便签”节点</summary>
		public void RemoveStrickyNotes()
        {
            foreach (var stickyNodeView in stickyNoteViews)
                RemoveElement(stickyNodeView);
            stickyNoteViews.Clear();
        }
#endif

        /// <summary>
        /// 将选中的节点添加到指定的group中（已在group中的节点不会被添加）
        /// </summary>
        /// <param name="view"></param>
        public void AddSelectionsToGroup(GroupView view)
        {
            foreach (var selectedNode in selection)
            {
                if (selectedNode is BaseNodeView)
                {
                    ///Exists：确定 List<T> 是否包含与指定谓词定义的条件(委托方法)匹配的元素。 若包含一个或多个与条件匹配的元素，返回true
                    ///ContainsElement：确定作用域是否包含指定的 GraphElement
                    if (groupViews.Exists(x => x.ContainsElement(selectedNode as BaseNodeView)))//若在graph的所有group元素中，存在已包含此节点的group，则跳过
                        continue;

                    view.AddElement(selectedNode as BaseNodeView);//不存在，则将此节点添加到指定的group中
                }
            }
        }

        /// <summary>移除所有的group</summary>
		public void RemoveGroups()
        {
            foreach (var groupView in groupViews)
                RemoveElement(groupView);
            groupViews.Clear();
        }

        /// <summary>
        /// 根据是否存在输入/输出端口、输入输出节点来判断能否进行连线
        /// </summary>
        /// <param name="e"></param>
        /// <param name="autoDisconnectInputs">没用到？</param>
        /// <returns></returns>
		public bool CanConnectEdge(EdgeView e, bool autoDisconnectInputs = true)
        {
            if (e.input == null || e.output == null)
                return false;

            var inputPortView = e.input as PortView;//输入端口
            var outputPortView = e.output as PortView;//输出端口
            var inputNodeView = inputPortView.node as BaseNodeView;//输入节点
            var outputNodeView = outputPortView.node as BaseNodeView;//输出节点

            if (inputNodeView == null || outputNodeView == null)
            {
                Debug.LogError("Connect aborted !");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 实现edge与输入/输出端口的连接(真连接)，并将其加入到graph中；若端口不支持多重连接则先将原先连接到端口的edge移除；刷新端口视图
        /// 对颜色错误的edge进行修补
        /// </summary>
        /// <param name="e"></param>
        /// <param name="autoDisconnectInputs"></param>
        /// <returns></returns>
		public bool ConnectView(EdgeView e, bool autoDisconnectInputs = true)
        {
            if (!CanConnectEdge(e, autoDisconnectInputs))//若不满足连接条件就返回false
                return false;

            var inputPortView = e.input as PortView;//输入端口
            var outputPortView = e.output as PortView;//输出端口
            var inputNodeView = inputPortView.node as BaseNodeView;//输入端口所在节点即edge的输出节点
            var outputNodeView = outputPortView.node as BaseNodeView;//输出端口所在节点即edge的输入节点

            //If the input port does not support multi-connection, we remove them 如果输入端口不支持多条edge连接，则删除原来连接着的edge
            if (autoDisconnectInputs && !(e.input as PortView).portData.acceptMultipleEdges)
            {
                foreach (var edge in edgeViews.Where(ev => ev.input == e.input).ToList())
                {
                    // TODO: do not disconnect them if the connected port is the same than the old connected 如果连接的端口与旧连接的端口相同，请不要断开它们
                    DisconnectView(edge);
                }
            }
            // same for the output port: 对输出端口进行同样的处理
            if (autoDisconnectInputs && !(e.output as PortView).portData.acceptMultipleEdges)
            {
                foreach (var edge in edgeViews.Where(ev => ev.output == e.output).ToList())
                {
                    // TODO: do not disconnect them if the connected port is the same than the old connected
                    DisconnectView(edge);
                }
            }

            AddElement(e);//* 将此edge加入到graph中

            //连接起输入/输出端口
            e.input.Connect(e);//*
            e.output.Connect(e);//*

            // If the input port have been removed by the custom port behavior  
            // we try to find if it's still here
            // 如果输入端口已被自定义端口行为删除，我们试图找出它是否还在这里
            if (e.input == null)
                e.input = inputNodeView.GetPortViewFromFieldName(inputPortView.fieldName, inputPortView.portData.identifier);//
            if (e.output == null)
                e.output = inputNodeView.GetPortViewFromFieldName(outputPortView.fieldName, outputPortView.portData.identifier);

            edgeViews.Add(e);

            //先确定拥有相同数量的port和portView，再刷新更新inputPortViews和outputPortViews端口视图
            inputNodeView.RefreshPorts();
            outputNodeView.RefreshPorts();

            // In certain cases the edge color is wrong so we patch it 在某些情况下，edge颜色是错误的，因此我们对其进行修补
            schedule.Execute(() => {
                e.UpdateEdgeControl();
            }).ExecuteLater(1);

            e.isConnected = true;//* 置位表示连接

            return true;
        }

        /// <summary>
        /// “顶层”的连接方法：(创建新的SerializableEdge和EdgeView)连接参数中的inputPortView和outputPortView，进行相关的操作(连线、记录连线信息、将元素加入到graph、刷新端口视图、更新计算顺序、修补edge颜色等)
        /// </summary>
        /// <param name="inputPortView"></param>
        /// <param name="outputPortView"></param>
        /// <param name="autoDisconnectInputs"></param>
        /// <returns></returns>
		public bool Connect(PortView inputPortView, PortView outputPortView, bool autoDisconnectInputs = true)
        {
            var inputPort = inputPortView.owner.nodeTarget.GetPort(inputPortView.fieldName, inputPortView.portData.identifier);
            var outputPort = outputPortView.owner.nodeTarget.GetPort(outputPortView.fieldName, outputPortView.portData.identifier);

            // Checks that the node we are connecting still exists 检查我们正在连接的节点是否仍然存在，不存在直接返回
            if (inputPortView.owner.parent == null || outputPortView.owner.parent == null)
                return false;

            var newEdge = SerializableEdge.CreateNewEdge(graph, inputPort, outputPort);//返回一个新的edge，此edge具备与graph、inputPort、outputPort的相关连线信息

            var edgeView = CreateEdgeView();//创建一个新的EdgeView
            edgeView.userData = newEdge;
            edgeView.input = inputPortView;
            edgeView.output = outputPortView;


            return Connect(edgeView);
        }

        /// <summary>
        /// 先判断参数edgeView是否满足连线要求，然后在SerializableEdge和EdgeView中分别实现此edge与端口的连接，
        /// 将edgeView加入到graph中，
        /// 更新计算顺序
        /// </summary>
        /// <param name="e"></param>
        /// <param name="autoDisconnectInputs"></param>
        /// <returns></returns>
		public bool Connect(EdgeView e, bool autoDisconnectInputs = true)
        {
            if (!CanConnectEdge(e, autoDisconnectInputs))//先判断能否进行连线（输入/输出端口、输入/输出节点均存在）
                return false;

            var inputPortView = e.input as PortView;//输入端口View
            var outputPortView = e.output as PortView;//输出端口View
            var inputNodeView = inputPortView.node as BaseNodeView;//输入节点View
            var outputNodeView = outputPortView.node as BaseNodeView;//输出节点View
            var inputPort = inputNodeView.nodeTarget.GetPort(inputPortView.fieldName, inputPortView.portData.identifier);//获取对应的输入端口
            var outputPort = outputNodeView.nodeTarget.GetPort(outputPortView.fieldName, outputPortView.portData.identifier);//获取对应的输出端口

            e.userData = graph.Connect(inputPort, outputPort, autoDisconnectInputs);//新建一条(包含相关连线信息的)SerializableEdge来连接两个端口，并将此SerializableEdge记录到graph中的edges列表中

            ConnectView(e, autoDisconnectInputs);//实现edge与输入/输出端口的连接，[并将其加入到graph中]；若端口不支持多重连接则先将原先连接到端口的edge移除；刷新端口视图；修补edge颜色

            UpdateComputeOrder();//更新节点计算顺序

            return true;
        }

        /// <summary>
        /// 将此edge从图表graph中删除，断开edge与输入输出断开的连接，从edgeViews列表中删除，
        /// 按情况更新端口视图
        /// </summary>
        /// <param name="e">被处理的edge</param>
        /// <param name="refreshPorts">是否刷新端口视图</param>
		public void DisconnectView(EdgeView e, bool refreshPorts = true)
        {
            if (e == null)
                return;

            RemoveElement(e);//将此edge从图表graph中删除

            if (e?.input?.node is BaseNodeView inputNodeView)
            {
                e.input.Disconnect(e);///*断开edge与端口的连接
                if (refreshPorts)
                    inputNodeView.RefreshPorts();//先确定拥有相同数量的port和portView，再刷新更新inputPortViews和outputPortViews端口视图
            }
            if (e?.output?.node is BaseNodeView outputNodeView)
            {
                e.output.Disconnect(e);///*断开edge与端口的连接
				if (refreshPorts)
                    outputNodeView.RefreshPorts();
            }

            edgeViews.Remove(e);//也将储存edge的列表中将其删除
        }

        /// <summary>
        /// 断开EdgeView的连接，删除EdgeView，更新计算顺序、端口视图
        /// </summary>
        /// <param name="e"></param>
        /// <param name="refreshPorts"></param>
		public void Disconnect(EdgeView e, bool refreshPorts = true)
        {
            // Remove the serialized edge if there is one 如果有，则删除序列化边缘
            if (e.userData is SerializableEdge serializableEdge)
                graph.Disconnect(serializableEdge.GUID);//断开连线

            DisconnectView(e, refreshPorts);//移除edge，断开与端口连接，刷新端口视图

            UpdateComputeOrder();//更新计算顺序
        }

        /// <summary>将edgeViews列表中的元素全部移除，并清空edgeViews列表</summary>
		public void RemoveEdges()
        {
            foreach (var edge in edgeViews)
                RemoveElement(edge);
            edgeViews.Clear();
        }

        /// <summary>
        /// 更新图中节点的计算顺序，并触发相应事件函数
        /// </summary>
		public void UpdateComputeOrder()
        {
            graph.UpdateComputeOrder();

            computeOrderUpdated?.Invoke();
        }

        /// <summary>
        /// 注册撤销：如果执行了撤销操作，那么在调用这一函数后对对象所做的任何更改都将被撤销，并且该对象将恢复到记录的状态
        /// </summary>
        /// <param name="name"></param>
		public void RegisterCompleteObjectUndo(string name)
        {
            Undo.RegisterCompleteObjectUndo(graph, name);//将对象状态存储在撤销堆栈上
            //如果执行了撤销操作，那么在调用这一函数后对对象所做的任何更改都将被撤销，并且该对象将恢复到记录的状态
        }

        /// <summary>将图表graph存入磁盘</summary>
        public void SaveGraphToDisk()
        {
            if (graph == null)
                return;

            EditorUtility.SetDirty(graph);
        }

        /// <summary>泛型：根据是否窗口打开情况，来打开或关闭窗口</summary>
		public void ToggleView<T>() where T : PinnedElementView//T必须继承PinnedElementView类
        {
            ToggleView(typeof(T));
        }
        /// <summary>PinnedElementView类型的窗口已打开，就关闭；已关闭，就打开</summary>
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
        /// 打开PinnedElement类的元素
        /// </summary>
        /// <param name="type">PinnedElementView或其子类</param>
		public void OpenPinned(Type type)
        {
            PinnedElementView view;

            if (type == null)
                return;

            PinnedElement elem = graph.OpenPinned(type);

            if (!pinnedElements.ContainsKey(type))//若字典中不包含type的键
            {
                view = Activator.CreateInstance(type) as PinnedElementView;//CreateInstance：创建type的对象，并返回对其的引用
                if (view == null)
                    return;
                pinnedElements[type] = view;
                view.InitializeGraphView(elem, this);
            }
            view = pinnedElements[type];

            if (!Contains(view))//若graph中不包含此type对应的PinnedElementView，就添加
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

        /// <summary>重置图表graph的位置的缩放大小</summary>
		public void ResetPositionAndZoom()
        {
            graph.position = Vector3.zero;
            graph.scale = Vector3.one;

            UpdateViewTransform(graph.position, graph.scale);
        }

        /// <summary>打开帮助提示窗口</summary
        public void OpenHelpTipsWindow(GraphTipsWindow tipsWindow)
        {
            Vector2 mousePos = mouseUpPos;
            UnityEditor.PopupWindow.Show(new Rect(mousePos.x, mousePos.y, 0, 0), new GraphTipsWindow());
        }

        /// <summary>关闭对应的窗口</summary
        public void CloseWindow()
        {
            if (EditorUtility.DisplayDialog("Close Window", "是否确定关闭此窗口？", "yes", "no"))
            {
                graphWindow.Close();
            }
        }

        /// <summary>
        /// Deletes the selected content, can be called form an IMGUI container
        /// 删除选中的内容，可以从IMGUI容器中调用
        /// </summary>
        public void DelayedDeleteSelection() => this.schedule.Execute(() => DeleteSelectionOperation("Delete", AskUser.DontAskUser)).ExecuteLater(0);

        protected virtual void InitializeView() { }

        /// <summary>
        /// 创建节点菜单条目的过滤器
        /// </summary>
        /// <returns></returns>
		public virtual IEnumerable<(string path, Type type)> FilterCreateNodeMenuEntries()
        {
            // By default we don't filter anything 默认情况下，我们不过滤任何内容
            foreach (var nodeMenuItem in NodeProvider.GetNodeMenuEntries(graph))
                yield return nodeMenuItem;

            // TODO: add exposed properties to this list 将公开的属性添加到此列表
        }

        /// <summary>
        /// 在指定位置处添加一个中继节点
        /// </summary>
        /// <param name="inputPort"></param>
        /// <param name="outputPort"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public RelayNodeView AddRelayNode(PortView inputPort, PortView outputPort, Vector2 position)
        {
            var relayNode = BaseNode.CreateFromType<RelayNode>(position);
            var view = AddNode(relayNode) as RelayNodeView;

            ///注意：中继节点只有一个输入、一个输出端口
			if (outputPort != null)//若指定的输出端口存在，则将中继节点的输入端口与其连接
                Connect(view.inputPortViews[0], outputPort);
            if (inputPort != null) //若指定的输入端口存在，则将中继节点的输出端口与其连接
                Connect(inputPort, view.outputPortViews[0]);

            return view;
        }

        /// <summary>
        /// Update all the serialized property bindings (in case a node was deleted / added, the property pathes needs to be updated)
        /// 更新所有序列化的属性绑定（如果删除/添加节点，则需要更新属性路径）
        /// </summary>
        public void SyncSerializedPropertyPathes()
        {
            foreach (var nodeView in nodeViews)
                nodeView.SyncSerializedPropertyPathes();
            nodeInspector.RefreshNodes();
        }

        /// <summary>
        /// Call this function when you want to remove this view
        /// 当你想要移除这个视图时，调用这个方法
        /// </summary>
        public void Dispose()
        {
            ClearGraphElements();//清除图表中所有的元素
            RemoveFromHierarchy();//将图表移除
            Undo.undoRedoPerformed -= ReloadView;//删除重载图表的委托方法
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