using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;
using System.Reflection;
using System;
using System.Collections;
using System.Linq;
using UnityEditor.UIElements;
using System.Text.RegularExpressions;

using Status = UnityEngine.UIElements.DropdownMenuAction.Status;
using NodeView = UnityEditor.Experimental.GraphView.Node;

namespace RDTS.NodeEditor
{
    [NodeCustomEditor(typeof(BaseNode))]
    public class BaseNodeView : NodeView//NodeView就是Node
    {
        public BaseNode nodeTarget;//对应的Node类

        public List<PortView> inputPortViews = new List<PortView>();//节点的输入端口列表
        public List<PortView> outputPortViews = new List<PortView>();//节点的输出端口列表

        public BaseGraphView owner { private set; get; }//节点关联的图表graph

        /// <summary>每个字段名的端口</summary>
		protected Dictionary<string, List<PortView>> portsPerFieldName = new Dictionary<string, List<PortView>>();

        //自定义的容器元素
        public VisualElement controlsContainer;
        protected VisualElement debugContainer;
        protected VisualElement rightTitleContainer;
        protected VisualElement topPortContainer;
        protected VisualElement bottomPortContainer;
        private VisualElement inputContainerElement;

        //设置按钮相关
        VisualElement settings;
        NodeSettingsView settingsContainer;
        Button settingButton;
        TextField titleTextField;

        /// <summary>计算顺序显示标签(选中节点右键后，点击Debug显示)</summary>
		Label computeOrderLabel = new Label();//计算顺序标签

        public event Action<PortView> onPortConnected;
        public event Action<PortView> onPortDisconnected;

        protected virtual bool hasSettings { get; set; }//是否具有Setting特性

        public bool initializing = false; //Used for applying SetPosition on locked node at init. 用于在初始化时在锁定节点上应用 SetPosition

        readonly string baseNodeStyle = "GraphStyles/BaseNodeView";

        bool settingsExpanded = false;//设置按钮是否打开

        //IconBadge：通常附加到其他视觉元素上的矩形标示。 
        //    是一个矩形的图标，鼠标在其上方时会显示给定的文本
        //    一般在实例节点的NodeView脚本中添加此元素，如MessageNode2View
        [System.NonSerialized]
        List<IconBadge> badges = new List<IconBadge>();

        /// <summary>被选中的节点列表</summary>
		private List<Node> selectedNodes = new List<Node>();
        private float selectedNodesFarLeft;//...左边界最远...
        private float selectedNodesNearLeft;//选中的节点中。距离graph左边界最近的距离值
        private float selectedNodesFarRight;//...右边界最远...
        private float selectedNodesNearRight;//...右边界最近...
        private float selectedNodesFarTop;//...上边界最远...
        private float selectedNodesNearTop;//...上边界最近...
        private float selectedNodesFarBottom;//...下边界最远...
        private float selectedNodesNearBottom;//...下边界最近...
        private float selectedNodesAvgHorizontal;//所有选中节点的水平中间值
        private float selectedNodesAvgVertical;//所有选中节点的垂直中间值

        #region  Initialization

        /// <summary>
        /// [在BaseGraphView中被调用]节点初始化
        /// 设置nodeTarget，关联图表graph，是否能删除、重命名，委托方法添加(端口更新等)，样式，
        /// 初始化节点视图、端口、debug计算顺序标签、设置按钮及其字段
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="node"></param>
        public void Initialize(BaseGraphView owner, BaseNode node)
        {
            nodeTarget = node;//设置关联的目标节点
            this.owner = owner;//设置关联的视图

            if (!node.deletable)///若此节点不能删除
				capabilities &= ~Capabilities.Deletable;//取反
                                                        // Note that the Renamable capability is useless right now as it haven't been implemented in Graphview
                                                        //重命名已实现
            if (node.isRenamable)///是否能重命名
				capabilities |= Capabilities.Renamable;

            owner.computeOrderUpdated += ComputeOrderUpdatedCallback;//标签显示
            node.onMessageAdded += AddMessageView;//标示信息增加
            node.onMessageRemoved += RemoveMessageView;//标示信息移除
            node.onPortsUpdated += a => schedule.Execute(_ => UpdatePortsForField(a)).ExecuteLater(0);//端口更新
            //schedule：检索此 VisualElement 的 IVisualElementScheduler
            //Execute：计划稍后执行此操作
            //ExecuteLater：取消之前安排的所有此项目的执行并重新安排此项目（以毫秒为单位）

            styleSheets.Add(Resources.Load<StyleSheet>(baseNodeStyle));//样式

            if (!string.IsNullOrEmpty(node.layoutStyle))
                styleSheets.Add(Resources.Load<StyleSheet>(node.layoutStyle));

            InitializeView();//初始化节点的视图元素
            InitializePorts();//初始化所有端口
            InitializeDebug();//初始化debugContainer，添加显示计算顺序的标签

            // If the standard Enable method is still overwritten, we call it
            // 如果标准的 Enable 方法仍然被覆盖，调用它
            // DeclaringType：获取声明该成员的类
            if (GetType().GetMethod(nameof(Enable), new Type[] { }).DeclaringType != typeof(BaseNodeView))
                ExceptionToLog.Call(() => Enable());
            else
                ExceptionToLog.Call(() => Enable(false));

            InitializeSettings();//初始化设置按钮，以及节点中其中带有SettingAttribute特性的字段

            RefreshExpandedState();//将自定义元素添加到 extensionContainer 后，调用此方法以使这些元素变得可见

            this.RefreshPorts();//先确定拥有相同数量的port和portView，再刷新更新inputPortViews和outputPortViews端口视图

            ///GeometryChangedEvent：进行布局计算后当元素的位置或尺寸发生变化时发送的事件
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            ///DetachFromPanelEvent：当元素父级为面板后代时，从父级分离元素前发送的事件
            ///当元素从面板分离后立即发送事件，由于设置面板是递归的，因此元素的所有后代也会接收事件
            RegisterCallback<DetachFromPanelEvent>(e => ExceptionToLog.Call(Disable));
            OnGeometryChanged(null);
        }

        /// <summary>
        /// 添加并初始化所有端口，将Graph视图中的BaseEdgeConnectorListener关联到端口，
        /// </summary>
		void InitializePorts()
        {
            var listener = owner.connectorListener;

            foreach (var inputPort in nodeTarget.inputPorts)//遍历输入端口，添加对应的输入端口
            {
                AddPort(inputPort.fieldInfo, Direction.Input, listener, inputPort.portData);
            }

            foreach (var outputPort in nodeTarget.outputPorts)//遍历输出端口，添加对应的输出端口
            {
                AddPort(outputPort.fieldInfo, Direction.Output, listener, outputPort.portData);
            }
        }

        /// <summary>
        /// 初始化节点的视图元素：
        ///     添加自定义的容器元素（debug显示等），设置节点的标题、位置、颜色，添加节点标题的重命名功能
        /// </summary>
		void InitializeView()
        {
            controlsContainer = new VisualElement { name = "controls" };
            controlsContainer.AddToClassList("NodeControls");
            mainContainer.Add(controlsContainer);///在主容器添加

            rightTitleContainer = new VisualElement { name = "RightTitleContainer" };
            titleContainer.Add(rightTitleContainer);///在标题栏容器添加

            topPortContainer = new VisualElement { name = "TopPortContainer" };
            this.Insert(0, topPortContainer);///向此节点的 contentContainer 插入一个元素

            bottomPortContainer = new VisualElement { name = "BottomPortContainer" };
            this.Add(bottomPortContainer);///向此节点的 contentContainer 添加一个元素

            if (nodeTarget.showControlsOnHover)//若要显示节点的controlsContainer
            {
                bool mouseOverControls = false;
                controlsContainer.style.display = DisplayStyle.None;
                //MouseOverEvent：当鼠标指针进入某一元素时发送的事件。此事件发生涓滴，发生冒泡，并可取消
                RegisterCallback<MouseOverEvent>(e => {
                    controlsContainer.style.display = DisplayStyle.Flex;
                    mouseOverControls = true;
                    /// Debug.Log($"鼠标进入 {nodeTarget.name}");
                });
                //MouseOutEvent：当鼠标指针退出某一元素时发送的事件。此事件发生涓滴，发生冒泡，并可取消
                RegisterCallback<MouseOutEvent>(e => {
                    var rect = GetPosition();//获取节点定位，返回矩形的位置和大小
                    var graphMousePosition = owner.contentViewContainer.WorldToLocal(e.mousePosition);//获取鼠标位置
                    if (rect.Contains(graphMousePosition) || !nodeTarget.showControlsOnHover)//若鼠标位置在节点的矩形内 或 无需鼠标悬停才显示节点
                        return;
                    mouseOverControls = false;
                    schedule.Execute(_ => {
                        if (!mouseOverControls)
                            controlsContainer.style.display = DisplayStyle.None;
                    }).ExecuteLater(500);
                    ///  Debug.Log($"鼠标退出 {nodeTarget.name}");
                });
            }

            Undo.undoRedoPerformed += UpdateFieldValues;//注册撤销

            debugContainer = new VisualElement { name = "debug" };
            if (nodeTarget.debug)
                mainContainer.Add(debugContainer);///在主容器添加

            initializing = true;

            UpdateTitle();//设置节点标题
            SetPosition(nodeTarget.position);//设置节点位置
            SetNodeColor(nodeTarget.color);//设置标题栏颜色和下边缘保留空间

            AddInputContainer();//添加自定义的inputContainerElement

            // Add renaming capability  增加重命名功能
            if ((capabilities & Capabilities.Renamable) != 0)
                SetupRenamableTitle();
        }

        /// <summary>
        /// 设置标题的重命名（鼠标左键双击）
        /// </summary>
		void SetupRenamableTitle()
        {
            var titleLabel = this.Q("title-label") as Label;

            titleTextField = new TextField { isDelayed = true };//如果设置isDelayed为 true，则在用户按 Enter 键或文本字段失去焦点之前，不会更新值属性
            titleTextField.style.display = DisplayStyle.None;//元素不可见
            titleLabel.parent.Insert(0, titleTextField);

            titleLabel.RegisterCallback<MouseDownEvent>(e => {
                if (e.clickCount == 2 && e.button == (int)MouseButton.LeftMouse)//若鼠标左键连按两下
                    OpenTitleEditor();//标题编辑打开操作
            });

            titleTextField.RegisterValueChangedCallback(e => CloseAndSaveTitleEditor(e.newValue));

            titleTextField.RegisterCallback<MouseDownEvent>(e => {
                if (e.clickCount == 2 && e.button == (int)MouseButton.LeftMouse)
                    CloseAndSaveTitleEditor(titleTextField.value);//关闭并保存标题编辑
            });

            titleTextField.RegisterCallback<FocusOutEvent>(e => CloseAndSaveTitleEditor(titleTextField.value));

            /*打开标题编辑：
             *    原本是标签显示标题，鼠标左键双击后使标签隐藏、文本字段可见，并使文本字段获得焦点，文本字段中的文本被选中
             */
            void OpenTitleEditor()
            {
                // show title textbox
                titleTextField.style.display = DisplayStyle.Flex;//将标题文本字段显示可见
                titleLabel.style.display = DisplayStyle.None;//标题标签不可见
                titleTextField.focusable = true;//获得焦点

                titleTextField.SetValueWithoutNotify(title);///设置节点的标题，且不注册通知
				titleTextField.Focus();//尝试让此元素获得焦点
                titleTextField.SelectAll();//选择所有文本
            }
            /*关闭并保存标题编辑：
             *    注册撤销操作，设置节点的自定义名称，隐藏文本字段、显示标签字段，根据自定义名称设置节点标题
             */
            void CloseAndSaveTitleEditor(string newTitle)
            {
                owner.RegisterCompleteObjectUndo("Renamed node " + newTitle);//注册撤销
                nodeTarget.SetCustomName(newTitle);//设置节点的自定义名称

                // hide title TextBox 
                titleTextField.style.display = DisplayStyle.None;//隐藏标题文本字段
                titleLabel.style.display = DisplayStyle.Flex;//重新显示标签
                titleTextField.focusable = false;//取消焦点

                UpdateTitle();//设置节点标题
            }
        }

        /// <summary>设置节点的标题</summary>
		void UpdateTitle()
        {
            //设置节点的标题
            title = (nodeTarget.GetCustomName() == null) ? nodeTarget.GetType().Name : nodeTarget.GetCustomName();
        }

        /// <summary>
        /// 初始化设置按钮，以及节点中其中带有SettingAttribute特性的字段
        /// </summary>
		void InitializeSettings()
        {
            // Initialize settings button: 初始化 设置按钮
            if (hasSettings)
            {
                CreateSettingButton();//创建设置按钮
                settingsContainer = new NodeSettingsView();
                settingsContainer.visible = false;
                settings = new VisualElement();
                // Add Node type specific settings 添加节点类型特定设置
                settings.Add(CreateSettingsView());//创建一个文本为“Settings”，名称为“header”的标签字段
                settingsContainer.Add(settings);//在设置容器中添加标签
                Add(settingsContainer);//添加到节点中

                var fields = nodeTarget.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                foreach (var field in fields)
                    if (field.GetCustomAttribute(typeof(SettingAttribute)) != null)
                        AddSettingField(field);//添加具有SettingAttribute特性的字段
            }
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (settingButton != null)//有设置按钮
            {
                var settingsButtonLayout = settingButton.ChangeCoordinatesTo(settingsContainer.parent, settingButton.layout);
                settingsContainer.style.top = settingsButtonLayout.yMax - 18f;
                settingsContainer.style.left = settingsButtonLayout.xMin - layout.width + 20f;
            }
        }

        // Workaround for bug in GraphView that makes the node selection border way too big
        // GraphView 中导致节点选择边框太大的错误的解决方法
        VisualElement selectionBorder, nodeBorder;
        internal void EnableSyncSelectionBorderHeight()
        {
            if (selectionBorder == null || nodeBorder == null)
            {
                selectionBorder = this.Q("selection-border");
                nodeBorder = this.Q("node-border");

                schedule.Execute(() => {
                    selectionBorder.style.height = nodeBorder.localBound.height;
                }).Every(17);
            }
        }

        /// <summary>创建一个设置按钮</summary>
        void CreateSettingButton()
        {
            settingButton = new Button(ToggleSettings) { name = "settings-button" };
            settingButton.Add(new Image { name = "icon", scaleMode = ScaleMode.ScaleToFit });

            titleContainer.Add(settingButton);//标题容器添加
        }

        /// <summary>设置按钮的功能</summary>
		void ToggleSettings()
        {
            settingsExpanded = !settingsExpanded;
            if (settingsExpanded)
                OpenSettings();
            else
                CloseSettings();
        }

        public void OpenSettings()
        {
            if (settingsContainer != null)
            {
                owner.ClearSelection();//清除所选项
                owner.AddToSelection(this);//向所选项添加此节点元素—选中并高亮

                settingButton.AddToClassList("clicked");
                settingsContainer.visible = true;//显示点击的元素
                settingsExpanded = true;
            }
            /// Debug.Log("打开设置按钮" + this.nodeTarget.name);
        }

        public void CloseSettings()
        {
            if (settingsContainer != null)
            {
                settingButton.RemoveFromClassList("clicked");
                settingsContainer.visible = false;
                settingsExpanded = false;
            }
            /// Debug.Log("关闭设置按钮"+ this.nodeTarget.name);
        }

        /// <summary>初始化debugContainer，添加显示计算顺序的标签</summary>
		void InitializeDebug()
        {
            ComputeOrderUpdatedCallback();//计算顺序标签的文本设置
            debugContainer.Add(computeOrderLabel);//在自定义的debug容器中添加标签
        }

        #endregion

        #region API

        /// <summary>根据字段名获取端口</summary>
        public List<PortView> GetPortViewsFromFieldName(string fieldName)
        {
            List<PortView> ret;

            portsPerFieldName.TryGetValue(fieldName, out ret);

            return ret;
        }
        /// <summary>根据字段名获取第一个端口</summary>
		public PortView GetFirstPortViewFromFieldName(string fieldName)
        {
            return GetPortViewsFromFieldName(fieldName)?.First();
        }
        /// <summary>根据字段名和标识符GUID获取一个端口</summary>
		public PortView GetPortViewFromFieldName(string fieldName, string identifier)
        {
            return GetPortViewsFromFieldName(fieldName)?.FirstOrDefault(pv => {
                return (pv.portData.identifier == identifier) || (String.IsNullOrEmpty(pv.portData.identifier) && String.IsNullOrEmpty(identifier));
            });
        }

        /// <summary>
        /// 添加一个端口，对端口的输入/输出类型、是否垂直显示进行相关操作，初始化端口并关联到此节点
        /// </summary>
		public PortView AddPort(FieldInfo fieldInfo, Direction direction, BaseEdgeConnectorListener listener, PortData portData)
        {
            PortView p = CreatePortView(direction, fieldInfo, portData, listener);

            if (p.direction == Direction.Input)//若是输入端口
            {
                inputPortViews.Add(p);

                if (portData.vertical)//若垂直显示
                    topPortContainer.Add(p);
                else
                    inputContainer.Add(p);
            }
            else//输出端口
            {
                outputPortViews.Add(p);

                if (portData.vertical)//若垂直显示
                    bottomPortContainer.Add(p);
                else
                    outputContainer.Add(p);
            }

            p.Initialize(this, portData?.displayName);//端口的初始化操作，关联到此节点

            List<PortView> ports;
            portsPerFieldName.TryGetValue(p.fieldName, out ports);
            if (ports == null)
            {
                ports = new List<PortView>();
                portsPerFieldName[p.fieldName] = ports;
            }
            ports.Add(p);

            return p;
        }

        /// <summary>创建一个端口</summary>
        protected virtual PortView CreatePortView(Direction direction, FieldInfo fieldInfo, PortData portData, BaseEdgeConnectorListener listener)
            => PortView.CreatePortView(direction, fieldInfo, portData, listener);

        /// <summary>按照输入/输出类型、是否出垂直显示的情况，在对应端口容器的指定索引处插入指定的端口</summary>
        public void InsertPort(PortView portView, int index)
        {
            if (portView.direction == Direction.Input)//是输入端口
            {
                if (portView.portData.vertical)//是垂直显示
                    topPortContainer.Insert(index, portView);
                else
                    inputContainer.Insert(index, portView);
            }
            else
            {
                if (portView.portData.vertical)
                    bottomPortContainer.Insert(index, portView);
                else
                    outputContainer.Insert(index, portView);
            }
        }

        /// <summary>移除此端口及其edges</summary>
		public void RemovePort(PortView p)
        {
            // Remove all connected edges: 移除此端口连接的所有的edges
            var edgesCopy = p.GetEdges().ToList();
            foreach (var e in edgesCopy)
                owner.Disconnect(e, refreshPorts: false);//断开连接

            if (p.direction == Direction.Input)
            {
                if (inputPortViews.Remove(p))
                    p.RemoveFromHierarchy();//将此端口从其父层级视图中删除
            }
            else
            {
                if (outputPortViews.Remove(p))
                    p.RemoveFromHierarchy();//将此端口从其父层级视图中删除
            }

            List<PortView> ports;
            portsPerFieldName.TryGetValue(p.fieldName, out ports);
            ports.Remove(p);
        }

        /// <summary>
        /// 对于所有选中的节点，计算(距离图表graph边界？)左、右、上、下的最大和最小值，以及在水平、垂直方向的中间值
        /// </summary>
        private void SetValuesForSelectedNodes()
        {
            selectedNodes = new List<Node>();
            owner.nodes.ForEach(node =>
            {
                if (node.selected) selectedNodes.Add(node);//若图表graph中有节点被选中，就加入到selectedNodes列表中
            });

            if (selectedNodes.Count < 2) return; //	No need for any of the calculations below 如果只有一个被选中的节点，不需要下面的任何计算

            selectedNodesFarLeft = int.MinValue;
            selectedNodesFarRight = int.MinValue;
            selectedNodesFarTop = int.MinValue;
            selectedNodesFarBottom = int.MinValue;

            selectedNodesNearLeft = int.MaxValue;
            selectedNodesNearRight = int.MaxValue;
            selectedNodesNearTop = int.MaxValue;
            selectedNodesNearBottom = int.MaxValue;

            foreach (var selectedNode in selectedNodes)//遍历被选中节点列表，计算此节点距离视图边界左、右、上、下的最小最大值
            {
                var nodeStyle = selectedNode.style;
                var nodeWidth = selectedNode.localBound.size.x;//轴对称包围盒的宽度
                var nodeHeight = selectedNode.localBound.size.y;//轴对称包围盒的高度

                if (nodeStyle.left.value.value > selectedNodesFarLeft) selectedNodesFarLeft = nodeStyle.left.value.value;
                if (nodeStyle.left.value.value + nodeWidth > selectedNodesFarRight) selectedNodesFarRight = nodeStyle.left.value.value + nodeWidth;
                if (nodeStyle.top.value.value > selectedNodesFarTop) selectedNodesFarTop = nodeStyle.top.value.value;
                if (nodeStyle.top.value.value + nodeHeight > selectedNodesFarBottom) selectedNodesFarBottom = nodeStyle.top.value.value + nodeHeight;

                if (nodeStyle.left.value.value < selectedNodesNearLeft) selectedNodesNearLeft = nodeStyle.left.value.value;
                if (nodeStyle.left.value.value + nodeWidth < selectedNodesNearRight) selectedNodesNearRight = nodeStyle.left.value.value + nodeWidth;
                if (nodeStyle.top.value.value < selectedNodesNearTop) selectedNodesNearTop = nodeStyle.top.value.value;
                if (nodeStyle.top.value.value + nodeHeight < selectedNodesNearBottom) selectedNodesNearBottom = nodeStyle.top.value.value + nodeHeight;
            }

            selectedNodesAvgHorizontal = (selectedNodesNearLeft + selectedNodesFarRight) / 2f;//计算所有选中节点的水平中间值
            selectedNodesAvgVertical = (selectedNodesNearTop + selectedNodesFarBottom) / 2f;//计算所有选中节点的垂直中间值
        }

        /// <summary>设置并获取此节点的X、Y位置，获取其宽度和高度</summary>
		public static Rect GetNodeRect(Node node, float left = int.MaxValue, float top = int.MaxValue)
        {
            return new Rect(
                new Vector2(left != int.MaxValue ? left : node.style.left.value.value, top != int.MaxValue ? top : node.style.top.value.value),//X和Y的position
                new Vector2(node.style.width.value.value, node.style.height.value.value)//宽度和高度
            );
        }

        /// <summary>将选中的节点靠左对齐(至少选中两个)</summary>
		public void AlignToLeft()
        {
            SetValuesForSelectedNodes();
            if (selectedNodes.Count < 2) return;

            foreach (var selectedNode in selectedNodes)
            {
                selectedNode.SetPosition(GetNodeRect(selectedNode, selectedNodesNearLeft));
            }
        }
        /// <summary>将选中的节点按水平方向的中间对齐(至少选中两个) 即在同一条垂直线上</summary>
		public void AlignToCenter()
        {
            SetValuesForSelectedNodes();
            if (selectedNodes.Count < 2) return;

            foreach (var selectedNode in selectedNodes)
            {
                selectedNode.SetPosition(GetNodeRect(selectedNode, selectedNodesAvgHorizontal - selectedNode.localBound.size.x / 2f));
            }
        }
        /// <summary>将选中的节点靠右对齐(至少选中两个)</summary>
		public void AlignToRight()
        {
            SetValuesForSelectedNodes();
            if (selectedNodes.Count < 2) return;

            foreach (var selectedNode in selectedNodes)
            {
                selectedNode.SetPosition(GetNodeRect(selectedNode, selectedNodesFarRight - selectedNode.localBound.size.x));
            }
        }
        /// <summary>将选中的节点靠顶部对齐(至少选中两个)</summary>
		public void AlignToTop()
        {
            SetValuesForSelectedNodes();
            if (selectedNodes.Count < 2) return;

            foreach (var selectedNode in selectedNodes)
            {
                selectedNode.SetPosition(GetNodeRect(selectedNode, top: selectedNodesNearTop));
            }
        }
        /// <summary>将选中的节点按垂直方向的中间对齐(至少选中两个) 即在同一水平线上</summary>
		public void AlignToMiddle()
        {
            SetValuesForSelectedNodes();
            if (selectedNodes.Count < 2) return;

            foreach (var selectedNode in selectedNodes)
            {
                selectedNode.SetPosition(GetNodeRect(selectedNode, top: selectedNodesAvgVertical - selectedNode.localBound.size.y / 2f));
            }
        }
        /// <summary>将选中的节点靠底部对齐(至少选中两个)</summary>
		public void AlignToBottom()
        {
            SetValuesForSelectedNodes();
            if (selectedNodes.Count < 2) return;

            foreach (var selectedNode in selectedNodes)
            {
                selectedNode.SetPosition(GetNodeRect(selectedNode, top: selectedNodesFarBottom - selectedNode.localBound.size.y));
            }
        }

        /// <summary>鼠标右键在菜单栏中，打开NodeViewScript</summary>
		public void OpenNodeViewScript()
        {
            var script = NodeProvider.GetNodeViewScript(GetType());

            if (script != null)
                AssetDatabase.OpenAsset(script.GetInstanceID(), 0, 0);//使用关联的应用程序打开资源
        }
        /// <summary>鼠标右键在菜单栏中，打开NodeScript</summary>
		public void OpenNodeScript()
        {
            var script = NodeProvider.GetNodeScript(nodeTarget.GetType());

            if (script != null)
                AssetDatabase.OpenAsset(script.GetInstanceID(), 0, 0);//打开指定的资产文件
        }

        /// <summary>debug功能的打开/关闭</summary>
		public void ToggleDebug()
        {
            nodeTarget.debug = !nodeTarget.debug;//状态切换
            UpdateDebugView();
        }

        //更新debug标签的显示状态
        public void UpdateDebugView()
        {
            if (nodeTarget.debug)//要显示debug(计算顺序标签)，就从主容器添加debugContainer
                mainContainer.Add(debugContainer);
            else//不要显示debug(计算顺序标签)，就从主容器移除debugContainer
                mainContainer.Remove(debugContainer);
        }

        /// <summary>添加标示信息元素：指定标示信息、图标、颜色</summary>
		public void AddMessageView(string message, Texture icon, Color color)
            => AddBadge(new NodeBadgeView(message, icon, color));

        /// <summary>添加标示信息元素：按照信息类型</summary>
		public void AddMessageView(string message, NodeMessageType messageType)
        {
            IconBadge badge = null;
            switch (messageType)
            {
                case NodeMessageType.Warning:
                    badge = new NodeBadgeView(message, EditorGUIUtility.IconContent("Collab.Warning").image, Color.yellow);//使用构造函数创建Warning标示信息
                    break;
                case NodeMessageType.Error:
                    badge = IconBadge.CreateError(message);//使用“error”视觉样式创建 IconBadge
                    break;
                case NodeMessageType.Info:
                    badge = IconBadge.CreateComment(message);//使用“comment”视觉样式创建 IconBadge。
                    break;
                default:
                case NodeMessageType.None:
                    badge = new NodeBadgeView(message, null, Color.grey);////使用构造函数创建标示信息
                    break;
            }

            AddBadge(badge);
        }

        /// <summary>
        ///  在节点中添加标示元素，同时将标示元素附加到节点顶部区域的右上角；
        ///  加入badge列表
        /// </summary>
        /// <param name="badge"></param>
		void AddBadge(IconBadge badge)
        {
            Add(badge);//向此节点的 contentContainer 添加IconBadge标示元素
            badges.Add(badge);//同时加入列表
            badge.AttachTo(topContainer, SpriteAlignment.TopRight);//将此标示附加到topContainer(输入/输出容器的顶部区域)，标示轴心位于图形矩形右上角
        }

        /// <summary>
        /// 移除满足匹配条件的标示元素
        /// </summary>
        /// <param name="callback"></param>
		void RemoveBadge(Func<IconBadge, bool> callback)
        {
            badges.RemoveAll(b => {
                if (callback(b))
                {
                    b.Detach();//将此标示从其目标中分离
                    b.RemoveFromHierarchy();//将此元素从其父层级视图中删除
                    return true;
                }
                return false;
            });
        }

        /// <summary>若标示文本包含参数中的标示信息，就移除它</summary>
		public void RemoveMessageViewContains(string message) => RemoveBadge(b => b.badgeText.Contains(message));
        /// <summary>若包含与参数中的标示信息相等的标示文本，就移除它</summary>
        public void RemoveMessageView(string message) => RemoveBadge(b => b.badgeText == message);

        public void Highlight()
        {
            AddToClassList("Highlight");
        }

        public void UnHighlight()
        {
            RemoveFromClassList("Highlight");
        }

        #endregion

        #region Callbacks & Overrides

        /// <summary>计算顺序更新回调：标签显示</summary>
        void ComputeOrderUpdatedCallback()
        {
            //Update debug compute order 更新调试计算顺序
            computeOrderLabel.text = "Compute order: " + nodeTarget.computeOrder;
        }

        public virtual void Enable(bool fromInspector = false) => DrawDefaultInspector(fromInspector);
        public virtual void Enable() => DrawDefaultInspector(false);

        public virtual void Disable() { }

        /// <summary>
        /// “条件可见”特性的字典：[涉及两个字段]
        ///     键：条件字段的字段名；
        ///     值：（条件字段的值，具有VisualIf特性的字段）
        /// </summary>
		Dictionary<string, List<(object value, VisualElement target)>> visibleConditions = new Dictionary<string, List<(object value, VisualElement target)>>();
        /// <summary>连接时需要被隐藏的元素</summary>
        Dictionary<string, VisualElement> hideElementIfConnected = new Dictionary<string, VisualElement>();
        /// <summary>字典（自定义字段，对应的控件）</summary>
        Dictionary<FieldInfo, List<VisualElement>> fieldControlsMap = new Dictionary<FieldInfo, List<VisualElement>>();

        /// <summary>添加自定义的inputContainerElement</summary>
		protected void AddInputContainer()
        {
            inputContainerElement = new VisualElement { name = "input-container" };
            mainContainer.parent.Add(inputContainerElement);
            inputContainerElement.SendToBack();//将此元素发送到其父子列表的开始处。此元素将显示在所有重叠的同级元素之后
            inputContainerElement.pickingMode = PickingMode.Ignore;//禁用拾取
        }

        /// <summary>
        /// 绘制节点，包含具有特性的字段和无特性的字段，“抽屉”形式的字段(在右edge连接后会被隐藏)
        /// </summary>
        /// <param name="fromInspector"></param>
		protected virtual void DrawDefaultInspector(bool fromInspector = false)
        {
            var fields = nodeTarget.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                // Filter fields from the BaseNode type since we are only interested in user-defined fields
                // (better than BindingFlags.DeclaredOnly because we keep any inherited user-defined fields) 
                // 从 BaseNode 类型中过滤字段，因为我们只对用户定义的字段感兴趣
                // 比 BindingFlags.DeclaredOnly 更好，因为我们保留任何继承的用户定义字段
                .Where(f => f.DeclaringType != typeof(BaseNode));

            fields = nodeTarget.OverrideFieldOrder(fields).Reverse();//重新排列字段后，反转序列中元素的顺序

            foreach (var field in fields)
            {
                //skip if the field is a node setting  如果字段具备setting特性则跳过
                if (field.GetCustomAttribute(typeof(SettingAttribute)) != null)
                {
                    hasSettings = true;
                    continue;
                }

                //skip if the field is not serializable 如果字段不可序列化则跳过
                bool serializeField = field.GetCustomAttribute(typeof(SerializeField)) != null;
                if ((!field.IsPublic && !serializeField) || field.IsNotSerialized)
                {
                    AddEmptyField(field, fromInspector);
                    continue;
                }

                //skip if the field is an input/output and not marked as SerializedField
                //如果字段是输入/输出类型，且未标记为 SerializedField，则跳过
                bool hasInputAttribute = field.GetCustomAttribute(typeof(InputAttribute)) != null;
                bool hasInputOrOutputAttribute = hasInputAttribute || field.GetCustomAttribute(typeof(OutputAttribute)) != null;
                bool showAsDrawer = !fromInspector && field.GetCustomAttribute(typeof(ShowAsDrawer)) != null;
                if (!serializeField && hasInputOrOutputAttribute && !showAsDrawer)
                {
                    AddEmptyField(field, fromInspector);
                    continue;
                }

                //skip if marked with NonSerialized or HideInInspector 如果标有 NonSerialized 或 HideInInspector 则跳过
                if (field.GetCustomAttribute(typeof(System.NonSerializedAttribute)) != null || field.GetCustomAttribute(typeof(HideInInspector)) != null)
                {
                    AddEmptyField(field, fromInspector);
                    continue;
                }

                // Hide the field if we want to display in in the inspector 如果我们想在检查器（节点视图）中显示，请隐藏该字段
                var showInInspector = field.GetCustomAttribute<ShowInInspector>();
                if (!serializeField && showInInspector != null && !showInInspector.showInNode && !fromInspector)
                {
                    AddEmptyField(field, fromInspector);
                    continue;
                }

                //绑定对象
                var bingObject = field.GetCustomAttribute(typeof(BindObjectAttribute)) as BindObjectAttribute;
                if(bingObject != null && field.IsPublic && field.FieldType.IsSubclassOf(typeof(RDTSBehavior)))
                {
                    var objValue = AddBindObjectField(field, bingObject.showName);
                    field.SetValue(nodeTarget, objValue);//将控件中的对象赋给具有“绑定对象特性”的字段
                    continue;
                }

                //showInputDrawer：是否显示输入端口的“抽屉”形式。
                //要显示“抽屉”，要满足：
                //    具有InputAttribute、(SerializeField或ShowAsDrawer)特性，且参数fromInspector=false，且该字段类型的实例不能分配给IList类型的变量。
                var showInputDrawer = field.GetCustomAttribute(typeof(InputAttribute)) != null && field.GetCustomAttribute(typeof(SerializeField)) != null;
                showInputDrawer |= field.GetCustomAttribute(typeof(InputAttribute)) != null && field.GetCustomAttribute(typeof(ShowAsDrawer)) != null;//或
                showInputDrawer &= !fromInspector; // We can't show a drawer in the inspector 我们不能在检查器中显示抽屉
                showInputDrawer &= !typeof(IList).IsAssignableFrom(field.FieldType);//IsAssignableFrom：确定指定类型field.FieldType的实例是否能分配给当前类型的变量

                ///ObjectNames：用于为对象构建可显示名称的 Helper 类
                string displayName = ObjectNames.NicifyVariableName(field.Name);//为变量创建可显示的名称（在大写字母之前插入空格，并删除名称之前的可选 m_、_ 或 k（后跟大写字母））

                ///InspectorNameAttribute：对枚举值声明使用此属性可更改 Inspector 中显示的显示名称
                var inspectorNameAttribute = field.GetCustomAttribute<InspectorNameAttribute>();
                if (inspectorNameAttribute != null)
                    displayName = inspectorNameAttribute.displayName;//displayName：要在Inspector中显示的名称

                //（具有输入/输出特性的字段会被显示在输入/输出容器）
                var elem = AddControlField(field, displayName, showInputDrawer);//*添加不具备特性的自定义的字段（如枚举、string等）*

                if (hasInputAttribute)//若具有输入类型的特性
                {
                    hideElementIfConnected[field.Name] = elem;

                    // Hide the field right away if there is already a connection:
                    // 如果已经存在连接，请立即隐藏该字段
                    if (portsPerFieldName.TryGetValue(field.Name, out var pvs))
                        if (pvs.Any(pv => pv.GetEdges().Count > 0))
                            elem.style.display = DisplayStyle.None;
                }
            }
        }


        /// <summary>设置节点标题栏容器的颜色和下边缘保留空间</summary>
		protected virtual void SetNodeColor(Color color)
        {
            //标题栏容器
            titleContainer.style.borderBottomColor = new StyleColor(color);//元素下边框的颜色
            titleContainer.style.borderBottomWidth = new StyleFloat(color.a > 0 ? 5f : 0f);//在布局阶段为此边框的下边缘保留的空间
        }

        /// <summary>向inputContainerElement添加一个空字段</summary>
        private void AddEmptyField(FieldInfo field, bool fromInspector)
        {
            if (field.GetCustomAttribute(typeof(InputAttribute)) == null || fromInspector)
                return;

            if (field.GetCustomAttribute<VerticalAttribute>() != null)
                return;

            var box = new VisualElement { name = field.Name };
            box.AddToClassList("port-input-element");
            box.AddToClassList("empty");
            inputContainerElement.Add(box);
        }

        /// <summary>
        /// 更新字段可见性：
        ///     [ VisualIf特性中设置的条件字段简称条件字段，具有VisualIf特性的字段简称VisualIf字段 ]
        ///     当条件字段的值等于VisualIf字段在VisualIf特性中设置的该条件字段的值时，VisualIf字段可见；反之，不可见
        /// </summary>
        /// <param name="fieldName">条件字段的字段名</param>
        /// <param name="newValue">条件字段的值</param>
		void UpdateFieldVisibility(string fieldName, object newValue)
        {
            //Debug.Log("更新字段可见性");
            if (newValue == null)//值为空则返回
                return;
            if (visibleConditions.TryGetValue(fieldName, out var list))//若包含指定键(fieldName)的元素则返回true，且list获取返回与指定键(fieldName)相对应的值
            {
                foreach (var elem in list)
                {
                    if (newValue.Equals(elem.value))//若具有VisualIf特性的字段中在VisualIf特性中设置的条件字段的值 等于 其条件字段当前的值，就将此具有VisualIf特性的字段显示
                        elem.target.style.display = DisplayStyle.Flex;
                    else//不相等，将此具有VisualIf特性的字段隐藏
                        elem.target.style.display = DisplayStyle.None;
                }
            }
        }

        void UpdateOtherFieldValueSpecific<T>(FieldInfo field, object newValue)
        {
            foreach (var inputField in fieldControlsMap[field])
            {
                //INotifyValueChanged：控件接口，这些控件保存值并且可以在用户输入更改值时发出通知
                var notify = inputField as INotifyValueChanged<T>;
                if (notify != null)
                    notify.SetValueWithoutNotify((T)newValue);//设置此值newValue，即使不同，也不通过 ChangeEvent<T> 通知注册回调
            }
        }

        static MethodInfo specificUpdateOtherFieldValue = typeof(BaseNodeView).GetMethod(nameof(UpdateOtherFieldValueSpecific), BindingFlags.NonPublic | BindingFlags.Instance);
        void UpdateOtherFieldValue(FieldInfo info, object newValue)
        {
            // Warning: Keep in sync with FieldFactory CreateField 警告：与 FieldFactory CreateField 保持同步
            var fieldType = info.FieldType.IsSubclassOf(typeof(UnityEngine.Object)) ? typeof(UnityEngine.Object) : info.FieldType;
            var genericUpdate = specificUpdateOtherFieldValue.MakeGenericMethod(fieldType);//返回：一个 MethodInfo 对象，表示通过将当前泛型方法定义的类型参数替换为 fieldType 的元素生成的构造方法

            genericUpdate.Invoke(this, new object[] { info, newValue });
        }

        /// <summary>
        /// 在fieldControlsMap字典中查找是否有给定参数字段信息对应的VisualElements，
        /// 若其中存在控件类型，就返回空间保存的值，否则返回null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <returns></returns>
		object GetInputFieldValueSpecific<T>(FieldInfo field)
        {
            if (fieldControlsMap.TryGetValue(field, out var list))
            {
                foreach (var inputField in list)
                {
                    if (inputField is INotifyValueChanged<T> notify)//INotifyValueChanged：控件接口，这些控件保存值并且可以在用户输入更改值时发出通知
                        return notify.value;//控件保存的值
                }
            }
            return null;
        }

        /// <summary>即BaseNodeView的GetInputFieldValueSpecific方法</summary>
		static MethodInfo specificGetValue = typeof(BaseNodeView).GetMethod(nameof(GetInputFieldValueSpecific), BindingFlags.NonPublic | BindingFlags.Instance);
        /// <summary>即BaseNodeView的GetInputFieldValueSpecific方法</summary>
        object GetInputFieldValue(FieldInfo info)
        {
            // Warning: Keep in sync with FieldFactory CreateField 与 FieldFactory CreateField 保持同步
            var fieldType = info.FieldType.IsSubclassOf(typeof(UnityEngine.Object)) ? typeof(UnityEngine.Object) : info.FieldType;
            var genericUpdate = specificGetValue.MakeGenericMethod(fieldType);//MakeGenericMethod：用类型数组的元素fieldType替代当前泛型方法定义的类型参数，并返回表示结果构造方法

            return genericUpdate.Invoke(this, new object[] { info });
        }

        protected VisualElement AddControlField(string fieldName, string label = null, bool showInputDrawer = false, Action valueChangedCallback = null)
            => AddControlField(nodeTarget.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance), label, showInputDrawer, valueChangedCallback);

        ///Regex：表示不可变的正则表达式
		Regex s_ReplaceNodeIndexPropertyPath = new Regex(@"(^nodes.Array.data\[)(\d+)(\])");
        //
        internal void SyncSerializedPropertyPathes()
        {
            int nodeIndex = owner.graph.nodes.FindIndex(n => n == nodeTarget);//找到在graph的nodes列表中此节点的索引值

            // If the node is not found, then it means that it has been deleted from serialized data.
            // 如果未找到该节点，则意味着它已从序列化数据中删除。
            if (nodeIndex == -1)
                return;

            var nodeIndexString = nodeIndex.ToString();
            foreach (var propertyField in this.Query<PropertyField>().ToList())
            {
                propertyField.Unbind();//断开所有属性的绑定
                                       // The property path look like this: nodes.Array.data[x].fieldName
                                       // And we want to update the value of x with the new node index:
                                       // 属性路径如下所示：nodes.Array.data[x].fieldName 我们想用新的节点索引更新 x 的值
                                       ///Replace：在指定的输入字符串propertyField.bindingPath中，使用由 MatchEvaluator 委托返回的字符串 [替换] 与指定的正则表达式匹配的所有字符串
                propertyField.bindingPath = s_ReplaceNodeIndexPropertyPath.Replace(propertyField.bindingPath, m => m.Groups[1].Value + nodeIndexString + m.Groups[3].Value);
                propertyField.Bind(owner.serializedGraph);
            }
        }

        /// <summary>
        /// 根据字段名查找其所在序列化graph的序列化nodes列表中对应的序列化字段属性(SerializedProperty)
        /// [ SerializedProperty：通用的属性类，用于所有的序列化对象 ]
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
		protected SerializedProperty FindSerializedProperty(string fieldName)
        {
            int i = owner.graph.nodes.FindIndex(n => n == nodeTarget);//找到此节点在图表graph中的索引
            ///FindProperty：按名称查找序列化属性（再序列化graph中查找nodes列表的序列化属性）
            ///GetArrayElementAtIndex：返回数组中指定索引处的元素
            ///FindPropertyRelative：从当前属性的相关路径(fieldName)检索 SerializedProperty
            return owner.serializedGraph.FindProperty("nodes").GetArrayElementAtIndex(i).FindPropertyRelative(fieldName);
        }

        /// <summary>
        /// 根据绑定字段特性来添加对应的绑定字段的控件
        /// </summary>
        private RDTSBehavior oldObject;
        protected object AddBindObjectField(FieldInfo field, string showName = null)
        {
           
            Type type = field.FieldType;//字段类型

            ///创建一个Object控件
            var objField = new ObjectField
            {
                objectType = type,//类型
                allowSceneObjects = true,//允许是scene对象

            };
            ///在控件启用时，添加之前已记录的对象
            if (nodeTarget.bindGuid != null)
            {
                string guid = nodeTarget.bindGuid;
                var bindObjPerGuid = this.owner.bindObjPerGuid;
                if (bindObjPerGuid != null)
                {
                    if (bindObjPerGuid.ContainsKey(guid))
                    {
                        var obj = this.owner.bindObjPerGuid[nodeTarget.bindGuid];
                        if (obj != null)
                        {
                            objField.value = obj;
                            oldObject = objField.value as RDTSBehavior;
                        }

                    }
                }

            }

            ///回调：更改控件的值(对象)时调用
            objField.RegisterValueChangedCallback(v =>
            {
                var sig = objField.value as RDTSBehavior;
                if (oldObject != sig && oldObject != null)//更换对象时，将旧对象的Guid清空
                    oldObject.Guid = null;


                if (nodeTarget.bindGuid != null && sig != null)//分配控件的Guid给对象
                {
                    sig.Guid = nodeTarget.bindGuid;
                }
                else if (nodeTarget.bindGuid == null && sig != null)//若是第一次绑定对象，就新分配一个Guid给控件
                {
                    string guid = System.Guid.NewGuid().ToString();
                    sig.Guid = nodeTarget.bindGuid = guid;
                }

                if (sig != null)
                    EditorUtility.SetDirty(sig);//保存对象的Guid

                oldObject = objField.value as RDTSBehavior;//将此对象记录为旧的
                field.SetValue(nodeTarget, objField.value);//*将控件中的对象赋给具有“绑定对象特性”的字段

            });


            ///添加控件
            controlsContainer.Add(objField);

            return objField.value;

        }



        /// <summary>
        /// 添加一个可控制的字段
        /// </summary>
        /// <param name="field">自定义的字段</param>
        /// <param name="label">显示名称</param>
        /// <param name="showInputDrawer">true以“抽屉”形式显示该字段。false则以普通字段形式显示</param>
        /// <param name="valueChangedCallback"></param>
        /// <returns></returns>
		protected VisualElement AddControlField(FieldInfo field, string label = null, bool showInputDrawer = false, Action valueChangedCallback = null)
        {
            if (field == null)
                return null;

            ///PropertyField：这是一个 SerializedProperty 封装器 VisualElement，在调用 Bind() 时将使用正确的 bindingPath 生成正确的字段元素
			var element = new PropertyField(FindSerializedProperty(field.Name), showInputDrawer ? "" : label);
            
            element.Bind(owner.serializedGraph);//将 SerializedObject(即owner.serializedGraph) 绑定到元素层次结构中的字段

#if UNITY_2020_3 // In Unity 2020.3 the empty label on property field doesn't hide it, so we do it manually
			if ((showInputDrawer || String.IsNullOrEmpty(label)) && element != null)
				element.AddToClassList("DrawerField_2020_3");
#endif
            //针对列表的字段
            if (typeof(IList).IsAssignableFrom(field.FieldType))
                EnableSyncSelectionBorderHeight();//启用同步选择边框高度

            //注册此回调以在值更改时接收 SerializedPropertyChangeEvent
            element.RegisterValueChangeCallback(e => {
                //GetValue:当在派生类中重写时，返回给定对象nodeTarget中的字段的值
                UpdateFieldVisibility(field.Name, field.GetValue(nodeTarget));//（此字段的值变化时）对将此字段当作条件字段的字段进行可见性状态设置
                valueChangedCallback?.Invoke();
                NotifyNodeChanged();//向图表graph发送事件，告知该节点内容已更改
                //Debug.Log("element value change");
            });

            // Disallow picking scene objects when the graph is not linked to a scene
            // 当图表graph未链接到场景时，不允许选取场景对象
            if (element != null && !owner.graph.IsLinkedToScene())
            {
                var objectField = element.Q<ObjectField>();//查找一个可接收任何对象类型的字段
                if (objectField != null)
                {
                    objectField.allowSceneObjects = false;//不允许为此字段分配场景对象
                }
                else
                {
                   // Debug.Log("objectField is null");
                }

                var textField = element.Q<TextField>();
                if(textField != null)
                {
                    textField.value = "test";
                }
                else
                {
                   // Debug.Log("textField is null");
                }
                    
            }

            if (!fieldControlsMap.TryGetValue(field, out var inputFieldList))
                inputFieldList = fieldControlsMap[field] = new List<VisualElement>();
            inputFieldList.Add(element);///

            if (element != null)
            {
                if (showInputDrawer)//若要显示“输入抽屉”的形式
                {
                    var box = new VisualElement { name = field.Name };
                    box.AddToClassList("port-input-element");
                    box.Add(element);
                    inputContainerElement.Add(box);//添加到自定义inputContainerElement容器中
                }
                else
                {
                  
                    controlsContainer.Add(element);//添加到自定义controlsContainer容器中
                }
                element.name = field.Name;

            }
            else
            {
                // Make sure we create an empty placeholder if FieldFactory can not provide a drawer
                // 如果 FieldFactory 无法提供抽屉，请确保我们创建一个空占位符
                if (showInputDrawer) AddEmptyField(field, false);
            }

            var visibleCondition = field.GetCustomAttribute(typeof(VisibleIf)) as VisibleIf;
            if (visibleCondition != null)//若具有“条件可见”特性
            {
                ///Debug.Log("visibleCondition is not null");
                // Check if target field exists: 检查目标字段是否存在
                //visibleCondition.fieldName：条件字段的字段名
                var conditionField = nodeTarget.GetType().GetField(visibleCondition.fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (conditionField == null)//若此节点不存在VisibleIf特性的字段
                    Debug.LogError($"[VisibleIf] Field {visibleCondition.fieldName} does not exists in node {nodeTarget.GetType()}");
                else//若存在
                {
                    visibleConditions.TryGetValue(visibleCondition.fieldName, out var list);
                    if (list == null)
                        list = visibleConditions[visibleCondition.fieldName] = new List<(object value, VisualElement target)>();
                    list.Add((visibleCondition.value, element));//列表类型（条件字段的值，具有VisualIf特性的字段）
                    UpdateFieldVisibility(visibleCondition.fieldName, conditionField.GetValue(nodeTarget));//GetValue(nodeTarget)：返回nodeTarget支持的字段的值。在这里即条件字段的值
                    ///Debug.Log("conditionField is not null");
				}
            }


            return element;
        }

        void UpdateFieldValues()
        {
            foreach (var kp in fieldControlsMap)
                UpdateOtherFieldValue(kp.Key, kp.Key.GetValue(nodeTarget));
        }

        /// <summary>
        /// 在设置按钮点击出现的元素中，添加setting字段
        /// </summary>
        /// <param name="field"></param>
        protected void AddSettingField(FieldInfo field)
        {
            if (field == null)
                return;

            var label = field.GetCustomAttribute<SettingAttribute>().name;

            var element = new PropertyField(FindSerializedProperty(field.Name));//查找带有 [Setting("Compare Function")] 特性的字段
            element.Bind(owner.serializedGraph);

            if (element != null)
            {
                settingsContainer.Add(element);
                element.name = field.Name;//字段名称
            }
        }

        internal void OnPortConnected(PortView port)
        {
            if (port.direction == Direction.Input && inputContainerElement?.Q(port.fieldName) != null)
                inputContainerElement.Q(port.fieldName).AddToClassList("empty");

            if (hideElementIfConnected.TryGetValue(port.fieldName, out var elem))
                elem.style.display = DisplayStyle.None;//定义元素在布局中的显示方式。 DisplayStyle.None：元素不可见，在布局中不存在

            onPortConnected?.Invoke(port);//调用端口连接时的委托方法
        }

        internal void OnPortDisconnected(PortView port)
        {
            if (port.direction == Direction.Input && inputContainerElement?.Q(port.fieldName) != null)
            {
                inputContainerElement.Q(port.fieldName).RemoveFromClassList("empty");

                if (nodeTarget.nodeFields.TryGetValue(port.fieldName, out var fieldInfo))
                {
                    var valueBeforeConnection = GetInputFieldValue(fieldInfo.info);

                    if (valueBeforeConnection != null)
                    {
                        fieldInfo.info.SetValue(nodeTarget, valueBeforeConnection);//将valueBeforeConnection值设置到nodeTarget
                    }
                }
            }

            if (hideElementIfConnected.TryGetValue(port.fieldName, out var elem))//断开连接时，将被隐藏的元素重新显现
                elem.style.display = DisplayStyle.Flex;

            onPortDisconnected?.Invoke(port);//调用端口断开连接时的委托方法
        }

        // TODO: a function to force to reload the custom behavior ports (if we want to do a button to add ports for example)

        public virtual void OnRemoved() { }
        public virtual void OnCreated() { }

        /// <summary>
        /// [重载]设置节点位置
        /// </summary>
        /// <param name="newPos"></param>
		public override void SetPosition(Rect newPos)
        {
            if (initializing || !nodeTarget.isLocked)//若初始化 或未锁定
            {
                base.SetPosition(newPos);//设置节点到新的位置

                if (!initializing)//若不是初始化，注册撤销操作
                    owner.RegisterCompleteObjectUndo("Moved graph node");

                nodeTarget.position = newPos;//更新节点信息中的位置信息
                initializing = false;
            }
        }

        public override bool expanded//节点是否已扩展
        {
            get { return base.expanded; }
            set
            {
                base.expanded = value;
                nodeTarget.expanded = value;
            }
        }

        /// <summary>改变Lock状态</summary>
        public void ChangeLockStatus()
        {
            ///取反
            nodeTarget.nodeLock ^= true;//异或：相同为false，不同为true
        }

        /// <summary>
        /// [重载]向节点上下文菜单添加菜单项
        /// </summary>
        /// <param name="evt">当上下文菜单需要菜单项时发送的事件</param>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            ///AppendAction：在下拉菜单中添加一个将执行操作的菜单项。此菜单项添加在当前菜单项列表末尾
			BuildAlignMenu(evt);
            evt.menu.AppendAction("Open Node Script", (e) => OpenNodeScript(), OpenNodeScriptStatus);
            evt.menu.AppendAction("Open Node View Script", (e) => OpenNodeViewScript(), OpenNodeViewScriptStatus);
            evt.menu.AppendAction("Debug", (e) => ToggleDebug(), DebugStatus);
            if (nodeTarget.unlockable)
                evt.menu.AppendAction((nodeTarget.isLocked ? "Unlock" : "Lock"), (e) => ChangeLockStatus(), LockStatus);
        }
        //对齐功能菜单
        protected void BuildAlignMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Align/To Left", (e) => AlignToLeft());
            evt.menu.AppendAction("Align/To Center", (e) => AlignToCenter());
            evt.menu.AppendAction("Align/To Right", (e) => AlignToRight());
            evt.menu.AppendSeparator("Align/");
            evt.menu.AppendAction("Align/To Top", (e) => AlignToTop());
            evt.menu.AppendAction("Align/To Middle", (e) => AlignToMiddle());
            evt.menu.AppendAction("Align/To Bottom", (e) => AlignToBottom());
            evt.menu.AppendSeparator();
        }

        Status LockStatus(DropdownMenuAction action)
        {
            return Status.Normal;//菜单项正常显示
        }

        Status DebugStatus(DropdownMenuAction action)
        {
            if (nodeTarget.debug)
                return Status.Checked;//菜单项显示为带复选标记
            return Status.Normal;
        }

        Status OpenNodeScriptStatus(DropdownMenuAction action)
        {
            if (NodeProvider.GetNodeScript(nodeTarget.GetType()) != null)
                return Status.Normal;
            return Status.Disabled; //菜单项已禁用，用户不能选择

        }

        Status OpenNodeViewScriptStatus(DropdownMenuAction action)
        {
            if (NodeProvider.GetNodeViewScript(GetType()) != null)
                return Status.Normal;
            return Status.Disabled;
        }

        /// <summary>
        /// 同步端口和端口视图中端口的数量
        /// </summary>
        /// <param name="ports"></param>
        /// <param name="portViews"></param>
        /// <returns></returns>
		IEnumerable<PortView> SyncPortCounts(IEnumerable<NodePort> ports, IEnumerable<PortView> portViews)
        {
            var listener = owner.connectorListener;
            var portViewList = portViews.ToList();

            // Maybe not good to remove ports as edges are still connected :/
            // 由于边缘仍然连接，因此删除端口可能不好
            foreach (var pv in portViews.ToList())
            {
                // If the port have disappeared from the node data, we remove the view: 如果端口已经从节点数据中消失，我们删除端口视图
                // We can use the identifier here because this function will only be called when there is a custom port behavior
                // 我们可以在这里使用标识符，因为只有在有自定义端口行为时才会调用此函数
                if (!ports.Any(p => p.portData.identifier == pv.portData.identifier))//portViews中有但ports没有，就删除
                {
                    RemovePort(pv);//移除端口及其edge
                    portViewList.Remove(pv);//从（参数）列表中移除
                }
            }

            foreach (var p in ports)
            {
                // Add missing port views 添加缺少的端口视图
                if (!portViews.Any(pv => p.portData.identifier == pv.portData.identifier))//ports中有但portViews中没有，就添加
                {
                    Direction portDirection = nodeTarget.IsFieldInput(p.fieldName) ? Direction.Input : Direction.Output;//判断端口类型
                    var pv = AddPort(p.fieldInfo, portDirection, listener, p.portData);
                    portViewList.Add(pv);
                }
            }

            return portViewList;
        }
        /// <summary>
        /// 同步端口和端口视图中端口的顺序
        /// </summary>
        /// <param name="ports"></param>
        /// <param name="portViews"></param>
		void SyncPortOrder(IEnumerable<NodePort> ports, IEnumerable<PortView> portViews)
        {
            var portViewList = portViews.ToList();
            var portsList = ports.ToList();

            // Re-order the port views to match the ports order in case a custom behavior re-ordered the ports
            // 重新排序端口视图以匹配端口顺序，以防自定义行为重新排序端口
            for (int i = 0; i < portsList.Count; i++)
            {
                var id = portsList[i].portData.identifier;

                var pv = portViewList.FirstOrDefault(p => p.portData.identifier == id);
                if (pv != null)
                    InsertPort(pv, i);//在输入/输出端口容器中插入端口
            }
        }

        /// <summary>
        /// 先确定拥有相同数量的port和portView，再刷新更新inputPortViews和outputPortViews端口视图
        /// </summary>
        /// <returns></returns>
		public virtual new bool RefreshPorts()
        {
            // If a port behavior was attached to one port, then
            // the port count might have been updated by the node
            // so we have to refresh the list of port views.
            //如果端口行为附加到一个端口，则端口计数可能已由节点更新，因此我们必须刷新端口视图列表。
            UpdatePortViewWithPorts(nodeTarget.inputPorts, inputPortViews);//更新inputPortViews
            UpdatePortViewWithPorts(nodeTarget.outputPorts, outputPortViews);//更新outputPortViews

            ///先保证port和portView的数量和顺序相同，再对各个端口视图进行更新操作
			void UpdatePortViewWithPorts(NodePortContainer ports, List<PortView> portViews)
            {
                if (ports.Count == 0 && portViews.Count == 0) // Nothing to update 都为空则不需要刷新
                    return;

                /* 先保证port和portView的数量相同，且顺序也相同 */
                // When there is no current portviews, we can't zip the list so we just add all
                //当没有当前端口视图时，我们无法压缩列表，所以我们只需添加所有
                if (portViews.Count == 0)
                    SyncPortCounts(ports, new PortView[] { });
                else if (ports.Count == 0) // Same when there is no ports
                    SyncPortCounts(new NodePort[] { }, portViews);
                else if (portViews.Count != ports.Count)
                    SyncPortCounts(ports, portViews);
                else
                {
                    ///GroupBy：根据指定的键选择器函数对序列中的元素进行分组
                    //对ports和portViews按照端口的fieldName进行分组，返回的对象<string,NodePort>均包含键(fieldName)和对象序列(NodePort) 
                    var p = ports.GroupBy(n => n.fieldName);
                    var pv = portViews.GroupBy(v => v.fieldName);
                    ///Zip:将指定函数应用于两个序列的对应元素，以生成结果序列
					p.Zip(pv, (portPerFieldName, portViewPerFieldName) => {
                        IEnumerable<PortView> portViewsList = portViewPerFieldName;
                        if (portPerFieldName.Count() != portViewPerFieldName.Count())//数量不相等
                            portViewsList = SyncPortCounts(portPerFieldName, portViewPerFieldName);//同步数量
                        SyncPortOrder(portPerFieldName, portViewsList);//同步顺序
                        // We don't care about the result, we just iterate over port and portView 我们不关心结果，我们只是迭代端口和端口视图
                        return "";
                    }).ToList();//转换成列表
                }

                // Here we're sure that we have the same amount of port and portView
                // so we can update the view with the new port data (if the name of a port have been changed for example)
                //到这里我们能够确定拥有相同数量的port和portView
                //因此我们可以使用新的端口数据更新视图（例如，如果端口名称已更改）

                /* 再更新端口视图 */
                for (int i = 0; i < portViews.Count; i++)
                    portViews[i].UpdatePortView(ports[i].portData);
            }

            return base.RefreshPorts();//刷新端口布局
        }

        /// <summary>
        /// 强制更新端口
        /// </summary>
        public void ForceUpdatePorts()
        {
            nodeTarget.UpdateAllPorts();

            RefreshPorts();
        }

        /// <summary>
        /// 刷新端口视图
        /// </summary>
        /// <param name="fieldName"></param>
		void UpdatePortsForField(string fieldName)
        {
            // TODO: actual code
            RefreshPorts();
        }

        /// <summary>创建一个文本为“Settings”，名称为“header”的标签字段</summary>
		protected virtual VisualElement CreateSettingsView() => new Label("Settings") { name = "header" };

        /// <summary>
        /// Send an event to the graph telling that the content of this node have changed
        /// 向图表graph发送事件，告知该节点的内容已更改
        /// </summary>
        public void NotifyNodeChanged() => owner.graph.NotifyNodeChanged(nodeTarget);

        #endregion
    }
}