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
    public class BaseNodeView : NodeView//NodeView����Node
    {
        public BaseNode nodeTarget;//��Ӧ��Node��

        public List<PortView> inputPortViews = new List<PortView>();//�ڵ������˿��б�
        public List<PortView> outputPortViews = new List<PortView>();//�ڵ������˿��б�

        public BaseGraphView owner { private set; get; }//�ڵ������ͼ��graph

        /// <summary>ÿ���ֶ����Ķ˿�</summary>
		protected Dictionary<string, List<PortView>> portsPerFieldName = new Dictionary<string, List<PortView>>();

        //�Զ��������Ԫ��
        public VisualElement controlsContainer;
        protected VisualElement debugContainer;
        protected VisualElement rightTitleContainer;
        protected VisualElement topPortContainer;
        protected VisualElement bottomPortContainer;
        private VisualElement inputContainerElement;

        //���ð�ť���
        VisualElement settings;
        NodeSettingsView settingsContainer;
        Button settingButton;
        TextField titleTextField;

        /// <summary>����˳����ʾ��ǩ(ѡ�нڵ��Ҽ��󣬵��Debug��ʾ)</summary>
		Label computeOrderLabel = new Label();//����˳���ǩ

        public event Action<PortView> onPortConnected;
        public event Action<PortView> onPortDisconnected;

        protected virtual bool hasSettings { get; set; }//�Ƿ����Setting����

        public bool initializing = false; //Used for applying SetPosition on locked node at init. �����ڳ�ʼ��ʱ�������ڵ���Ӧ�� SetPosition

        readonly string baseNodeStyle = "GraphStyles/BaseNodeView";

        bool settingsExpanded = false;//���ð�ť�Ƿ��

        //IconBadge��ͨ�����ӵ������Ӿ�Ԫ���ϵľ��α�ʾ�� 
        //    ��һ�����ε�ͼ�꣬��������Ϸ�ʱ����ʾ�������ı�
        //    һ����ʵ���ڵ��NodeView�ű�����Ӵ�Ԫ�أ���MessageNode2View
        [System.NonSerialized]
        List<IconBadge> badges = new List<IconBadge>();

        /// <summary>��ѡ�еĽڵ��б�</summary>
		private List<Node> selectedNodes = new List<Node>();
        private float selectedNodesFarLeft;//...��߽���Զ...
        private float selectedNodesNearLeft;//ѡ�еĽڵ��С�����graph��߽�����ľ���ֵ
        private float selectedNodesFarRight;//...�ұ߽���Զ...
        private float selectedNodesNearRight;//...�ұ߽����...
        private float selectedNodesFarTop;//...�ϱ߽���Զ...
        private float selectedNodesNearTop;//...�ϱ߽����...
        private float selectedNodesFarBottom;//...�±߽���Զ...
        private float selectedNodesNearBottom;//...�±߽����...
        private float selectedNodesAvgHorizontal;//����ѡ�нڵ��ˮƽ�м�ֵ
        private float selectedNodesAvgVertical;//����ѡ�нڵ�Ĵ�ֱ�м�ֵ

        #region  Initialization

        /// <summary>
        /// [��BaseGraphView�б�����]�ڵ��ʼ��
        /// ����nodeTarget������ͼ��graph���Ƿ���ɾ������������ί�з������(�˿ڸ��µ�)����ʽ��
        /// ��ʼ���ڵ���ͼ���˿ڡ�debug����˳���ǩ�����ð�ť�����ֶ�
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="node"></param>
        public void Initialize(BaseGraphView owner, BaseNode node)
        {
            nodeTarget = node;//���ù�����Ŀ��ڵ�
            this.owner = owner;//���ù�������ͼ

            if (!node.deletable)///���˽ڵ㲻��ɾ��
				capabilities &= ~Capabilities.Deletable;//ȡ��
                                                        // Note that the Renamable capability is useless right now as it haven't been implemented in Graphview
                                                        //��������ʵ��
            if (node.isRenamable)///�Ƿ���������
				capabilities |= Capabilities.Renamable;

            owner.computeOrderUpdated += ComputeOrderUpdatedCallback;//��ǩ��ʾ
            node.onMessageAdded += AddMessageView;//��ʾ��Ϣ����
            node.onMessageRemoved += RemoveMessageView;//��ʾ��Ϣ�Ƴ�
            node.onPortsUpdated += a => schedule.Execute(_ => UpdatePortsForField(a)).ExecuteLater(0);//�˿ڸ���
            //schedule�������� VisualElement �� IVisualElementScheduler
            //Execute���ƻ��Ժ�ִ�д˲���
            //ExecuteLater��ȡ��֮ǰ���ŵ����д���Ŀ��ִ�в����°��Ŵ���Ŀ���Ժ���Ϊ��λ��

            styleSheets.Add(Resources.Load<StyleSheet>(baseNodeStyle));//��ʽ

            if (!string.IsNullOrEmpty(node.layoutStyle))
                styleSheets.Add(Resources.Load<StyleSheet>(node.layoutStyle));

            InitializeView();//��ʼ���ڵ����ͼԪ��
            InitializePorts();//��ʼ�����ж˿�
            InitializeDebug();//��ʼ��debugContainer�������ʾ����˳��ı�ǩ

            // If the standard Enable method is still overwritten, we call it
            // �����׼�� Enable ������Ȼ�����ǣ�������
            // DeclaringType����ȡ�����ó�Ա����
            if (GetType().GetMethod(nameof(Enable), new Type[] { }).DeclaringType != typeof(BaseNodeView))
                ExceptionToLog.Call(() => Enable());
            else
                ExceptionToLog.Call(() => Enable(false));

            InitializeSettings();//��ʼ�����ð�ť���Լ��ڵ������д���SettingAttribute���Ե��ֶ�

            RefreshExpandedState();//���Զ���Ԫ����ӵ� extensionContainer �󣬵��ô˷�����ʹ��ЩԪ�ر�ÿɼ�

            this.RefreshPorts();//��ȷ��ӵ����ͬ������port��portView����ˢ�¸���inputPortViews��outputPortViews�˿���ͼ

            ///GeometryChangedEvent�����в��ּ����Ԫ�ص�λ�û�ߴ緢���仯ʱ���͵��¼�
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            ///DetachFromPanelEvent����Ԫ�ظ���Ϊ�����ʱ���Ӹ�������Ԫ��ǰ���͵��¼�
            ///��Ԫ�ش�����������������¼���������������ǵݹ�ģ����Ԫ�ص����к��Ҳ������¼�
            RegisterCallback<DetachFromPanelEvent>(e => ExceptionToLog.Call(Disable));
            OnGeometryChanged(null);
        }

        /// <summary>
        /// ��Ӳ���ʼ�����ж˿ڣ���Graph��ͼ�е�BaseEdgeConnectorListener�������˿ڣ�
        /// </summary>
		void InitializePorts()
        {
            var listener = owner.connectorListener;

            foreach (var inputPort in nodeTarget.inputPorts)//��������˿ڣ���Ӷ�Ӧ������˿�
            {
                AddPort(inputPort.fieldInfo, Direction.Input, listener, inputPort.portData);
            }

            foreach (var outputPort in nodeTarget.outputPorts)//��������˿ڣ���Ӷ�Ӧ������˿�
            {
                AddPort(outputPort.fieldInfo, Direction.Output, listener, outputPort.portData);
            }
        }

        /// <summary>
        /// ��ʼ���ڵ����ͼԪ�أ�
        ///     ����Զ��������Ԫ�أ�debug��ʾ�ȣ������ýڵ�ı��⡢λ�á���ɫ����ӽڵ���������������
        /// </summary>
		void InitializeView()
        {
            controlsContainer = new VisualElement { name = "controls" };
            controlsContainer.AddToClassList("NodeControls");
            mainContainer.Add(controlsContainer);///�����������

            rightTitleContainer = new VisualElement { name = "RightTitleContainer" };
            titleContainer.Add(rightTitleContainer);///�ڱ������������

            topPortContainer = new VisualElement { name = "TopPortContainer" };
            this.Insert(0, topPortContainer);///��˽ڵ�� contentContainer ����һ��Ԫ��

            bottomPortContainer = new VisualElement { name = "BottomPortContainer" };
            this.Add(bottomPortContainer);///��˽ڵ�� contentContainer ���һ��Ԫ��

            if (nodeTarget.showControlsOnHover)//��Ҫ��ʾ�ڵ��controlsContainer
            {
                bool mouseOverControls = false;
                controlsContainer.style.display = DisplayStyle.None;
                //MouseOverEvent�������ָ�����ĳһԪ��ʱ���͵��¼������¼�����丵Σ�����ð�ݣ�����ȡ��
                RegisterCallback<MouseOverEvent>(e => {
                    controlsContainer.style.display = DisplayStyle.Flex;
                    mouseOverControls = true;
                    /// Debug.Log($"������ {nodeTarget.name}");
                });
                //MouseOutEvent�������ָ���˳�ĳһԪ��ʱ���͵��¼������¼�����丵Σ�����ð�ݣ�����ȡ��
                RegisterCallback<MouseOutEvent>(e => {
                    var rect = GetPosition();//��ȡ�ڵ㶨λ�����ؾ��ε�λ�úʹ�С
                    var graphMousePosition = owner.contentViewContainer.WorldToLocal(e.mousePosition);//��ȡ���λ��
                    if (rect.Contains(graphMousePosition) || !nodeTarget.showControlsOnHover)//�����λ���ڽڵ�ľ����� �� ���������ͣ����ʾ�ڵ�
                        return;
                    mouseOverControls = false;
                    schedule.Execute(_ => {
                        if (!mouseOverControls)
                            controlsContainer.style.display = DisplayStyle.None;
                    }).ExecuteLater(500);
                    ///  Debug.Log($"����˳� {nodeTarget.name}");
                });
            }

            Undo.undoRedoPerformed += UpdateFieldValues;//ע�᳷��

            debugContainer = new VisualElement { name = "debug" };
            if (nodeTarget.debug)
                mainContainer.Add(debugContainer);///�����������

            initializing = true;

            UpdateTitle();//���ýڵ����
            SetPosition(nodeTarget.position);//���ýڵ�λ��
            SetNodeColor(nodeTarget.color);//���ñ�������ɫ���±�Ե�����ռ�

            AddInputContainer();//����Զ����inputContainerElement

            // Add renaming capability  ��������������
            if ((capabilities & Capabilities.Renamable) != 0)
                SetupRenamableTitle();
        }

        /// <summary>
        /// ���ñ������������������˫����
        /// </summary>
		void SetupRenamableTitle()
        {
            var titleLabel = this.Q("title-label") as Label;

            titleTextField = new TextField { isDelayed = true };//�������isDelayedΪ true�������û��� Enter �����ı��ֶ�ʧȥ����֮ǰ���������ֵ����
            titleTextField.style.display = DisplayStyle.None;//Ԫ�ز��ɼ�
            titleLabel.parent.Insert(0, titleTextField);

            titleLabel.RegisterCallback<MouseDownEvent>(e => {
                if (e.clickCount == 2 && e.button == (int)MouseButton.LeftMouse)//����������������
                    OpenTitleEditor();//����༭�򿪲���
            });

            titleTextField.RegisterValueChangedCallback(e => CloseAndSaveTitleEditor(e.newValue));

            titleTextField.RegisterCallback<MouseDownEvent>(e => {
                if (e.clickCount == 2 && e.button == (int)MouseButton.LeftMouse)
                    CloseAndSaveTitleEditor(titleTextField.value);//�رղ��������༭
            });

            titleTextField.RegisterCallback<FocusOutEvent>(e => CloseAndSaveTitleEditor(titleTextField.value));

            /*�򿪱���༭��
             *    ԭ���Ǳ�ǩ��ʾ���⣬������˫����ʹ��ǩ���ء��ı��ֶοɼ�����ʹ�ı��ֶλ�ý��㣬�ı��ֶ��е��ı���ѡ��
             */
            void OpenTitleEditor()
            {
                // show title textbox
                titleTextField.style.display = DisplayStyle.Flex;//�������ı��ֶ���ʾ�ɼ�
                titleLabel.style.display = DisplayStyle.None;//�����ǩ���ɼ�
                titleTextField.focusable = true;//��ý���

                titleTextField.SetValueWithoutNotify(title);///���ýڵ�ı��⣬�Ҳ�ע��֪ͨ
				titleTextField.Focus();//�����ô�Ԫ�ػ�ý���
                titleTextField.SelectAll();//ѡ�������ı�
            }
            /*�رղ��������༭��
             *    ע�᳷�����������ýڵ���Զ������ƣ������ı��ֶΡ���ʾ��ǩ�ֶΣ������Զ����������ýڵ����
             */
            void CloseAndSaveTitleEditor(string newTitle)
            {
                owner.RegisterCompleteObjectUndo("Renamed node " + newTitle);//ע�᳷��
                nodeTarget.SetCustomName(newTitle);//���ýڵ���Զ�������

                // hide title TextBox 
                titleTextField.style.display = DisplayStyle.None;//���ر����ı��ֶ�
                titleLabel.style.display = DisplayStyle.Flex;//������ʾ��ǩ
                titleTextField.focusable = false;//ȡ������

                UpdateTitle();//���ýڵ����
            }
        }

        /// <summary>���ýڵ�ı���</summary>
		void UpdateTitle()
        {
            //���ýڵ�ı���
            title = (nodeTarget.GetCustomName() == null) ? nodeTarget.GetType().Name : nodeTarget.GetCustomName();
        }

        /// <summary>
        /// ��ʼ�����ð�ť���Լ��ڵ������д���SettingAttribute���Ե��ֶ�
        /// </summary>
		void InitializeSettings()
        {
            // Initialize settings button: ��ʼ�� ���ð�ť
            if (hasSettings)
            {
                CreateSettingButton();//�������ð�ť
                settingsContainer = new NodeSettingsView();
                settingsContainer.visible = false;
                settings = new VisualElement();
                // Add Node type specific settings ��ӽڵ������ض�����
                settings.Add(CreateSettingsView());//����һ���ı�Ϊ��Settings��������Ϊ��header���ı�ǩ�ֶ�
                settingsContainer.Add(settings);//��������������ӱ�ǩ
                Add(settingsContainer);//��ӵ��ڵ���

                var fields = nodeTarget.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                foreach (var field in fields)
                    if (field.GetCustomAttribute(typeof(SettingAttribute)) != null)
                        AddSettingField(field);//��Ӿ���SettingAttribute���Ե��ֶ�
            }
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (settingButton != null)//�����ð�ť
            {
                var settingsButtonLayout = settingButton.ChangeCoordinatesTo(settingsContainer.parent, settingButton.layout);
                settingsContainer.style.top = settingsButtonLayout.yMax - 18f;
                settingsContainer.style.left = settingsButtonLayout.xMin - layout.width + 20f;
            }
        }

        // Workaround for bug in GraphView that makes the node selection border way too big
        // GraphView �е��½ڵ�ѡ��߿�̫��Ĵ���Ľ������
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

        /// <summary>����һ�����ð�ť</summary>
        void CreateSettingButton()
        {
            settingButton = new Button(ToggleSettings) { name = "settings-button" };
            settingButton.Add(new Image { name = "icon", scaleMode = ScaleMode.ScaleToFit });

            titleContainer.Add(settingButton);//�����������
        }

        /// <summary>���ð�ť�Ĺ���</summary>
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
                owner.ClearSelection();//�����ѡ��
                owner.AddToSelection(this);//����ѡ����Ӵ˽ڵ�Ԫ�ء�ѡ�в�����

                settingButton.AddToClassList("clicked");
                settingsContainer.visible = true;//��ʾ�����Ԫ��
                settingsExpanded = true;
            }
            /// Debug.Log("�����ð�ť" + this.nodeTarget.name);
        }

        public void CloseSettings()
        {
            if (settingsContainer != null)
            {
                settingButton.RemoveFromClassList("clicked");
                settingsContainer.visible = false;
                settingsExpanded = false;
            }
            /// Debug.Log("�ر����ð�ť"+ this.nodeTarget.name);
        }

        /// <summary>��ʼ��debugContainer�������ʾ����˳��ı�ǩ</summary>
		void InitializeDebug()
        {
            ComputeOrderUpdatedCallback();//����˳���ǩ���ı�����
            debugContainer.Add(computeOrderLabel);//���Զ����debug��������ӱ�ǩ
        }

        #endregion

        #region API

        /// <summary>�����ֶ�����ȡ�˿�</summary>
        public List<PortView> GetPortViewsFromFieldName(string fieldName)
        {
            List<PortView> ret;

            portsPerFieldName.TryGetValue(fieldName, out ret);

            return ret;
        }
        /// <summary>�����ֶ�����ȡ��һ���˿�</summary>
		public PortView GetFirstPortViewFromFieldName(string fieldName)
        {
            return GetPortViewsFromFieldName(fieldName)?.First();
        }
        /// <summary>�����ֶ����ͱ�ʶ��GUID��ȡһ���˿�</summary>
		public PortView GetPortViewFromFieldName(string fieldName, string identifier)
        {
            return GetPortViewsFromFieldName(fieldName)?.FirstOrDefault(pv => {
                return (pv.portData.identifier == identifier) || (String.IsNullOrEmpty(pv.portData.identifier) && String.IsNullOrEmpty(identifier));
            });
        }

        /// <summary>
        /// ���һ���˿ڣ��Զ˿ڵ�����/������͡��Ƿ�ֱ��ʾ������ز�������ʼ���˿ڲ��������˽ڵ�
        /// </summary>
		public PortView AddPort(FieldInfo fieldInfo, Direction direction, BaseEdgeConnectorListener listener, PortData portData)
        {
            PortView p = CreatePortView(direction, fieldInfo, portData, listener);

            if (p.direction == Direction.Input)//��������˿�
            {
                inputPortViews.Add(p);

                if (portData.vertical)//����ֱ��ʾ
                    topPortContainer.Add(p);
                else
                    inputContainer.Add(p);
            }
            else//����˿�
            {
                outputPortViews.Add(p);

                if (portData.vertical)//����ֱ��ʾ
                    bottomPortContainer.Add(p);
                else
                    outputContainer.Add(p);
            }

            p.Initialize(this, portData?.displayName);//�˿ڵĳ�ʼ���������������˽ڵ�

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

        /// <summary>����һ���˿�</summary>
        protected virtual PortView CreatePortView(Direction direction, FieldInfo fieldInfo, PortData portData, BaseEdgeConnectorListener listener)
            => PortView.CreatePortView(direction, fieldInfo, portData, listener);

        /// <summary>��������/������͡��Ƿ����ֱ��ʾ��������ڶ�Ӧ�˿�������ָ������������ָ���Ķ˿�</summary>
        public void InsertPort(PortView portView, int index)
        {
            if (portView.direction == Direction.Input)//������˿�
            {
                if (portView.portData.vertical)//�Ǵ�ֱ��ʾ
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

        /// <summary>�Ƴ��˶˿ڼ���edges</summary>
		public void RemovePort(PortView p)
        {
            // Remove all connected edges: �Ƴ��˶˿����ӵ����е�edges
            var edgesCopy = p.GetEdges().ToList();
            foreach (var e in edgesCopy)
                owner.Disconnect(e, refreshPorts: false);//�Ͽ�����

            if (p.direction == Direction.Input)
            {
                if (inputPortViews.Remove(p))
                    p.RemoveFromHierarchy();//���˶˿ڴ��丸�㼶��ͼ��ɾ��
            }
            else
            {
                if (outputPortViews.Remove(p))
                    p.RemoveFromHierarchy();//���˶˿ڴ��丸�㼶��ͼ��ɾ��
            }

            List<PortView> ports;
            portsPerFieldName.TryGetValue(p.fieldName, out ports);
            ports.Remove(p);
        }

        /// <summary>
        /// ��������ѡ�еĽڵ㣬����(����ͼ��graph�߽磿)���ҡ��ϡ��µ�������Сֵ���Լ���ˮƽ����ֱ������м�ֵ
        /// </summary>
        private void SetValuesForSelectedNodes()
        {
            selectedNodes = new List<Node>();
            owner.nodes.ForEach(node =>
            {
                if (node.selected) selectedNodes.Add(node);//��ͼ��graph���нڵ㱻ѡ�У��ͼ��뵽selectedNodes�б���
            });

            if (selectedNodes.Count < 2) return; //	No need for any of the calculations below ���ֻ��һ����ѡ�еĽڵ㣬����Ҫ������κμ���

            selectedNodesFarLeft = int.MinValue;
            selectedNodesFarRight = int.MinValue;
            selectedNodesFarTop = int.MinValue;
            selectedNodesFarBottom = int.MinValue;

            selectedNodesNearLeft = int.MaxValue;
            selectedNodesNearRight = int.MaxValue;
            selectedNodesNearTop = int.MaxValue;
            selectedNodesNearBottom = int.MaxValue;

            foreach (var selectedNode in selectedNodes)//������ѡ�нڵ��б�����˽ڵ������ͼ�߽����ҡ��ϡ��µ���С���ֵ
            {
                var nodeStyle = selectedNode.style;
                var nodeWidth = selectedNode.localBound.size.x;//��Գư�Χ�еĿ��
                var nodeHeight = selectedNode.localBound.size.y;//��Գư�Χ�еĸ߶�

                if (nodeStyle.left.value.value > selectedNodesFarLeft) selectedNodesFarLeft = nodeStyle.left.value.value;
                if (nodeStyle.left.value.value + nodeWidth > selectedNodesFarRight) selectedNodesFarRight = nodeStyle.left.value.value + nodeWidth;
                if (nodeStyle.top.value.value > selectedNodesFarTop) selectedNodesFarTop = nodeStyle.top.value.value;
                if (nodeStyle.top.value.value + nodeHeight > selectedNodesFarBottom) selectedNodesFarBottom = nodeStyle.top.value.value + nodeHeight;

                if (nodeStyle.left.value.value < selectedNodesNearLeft) selectedNodesNearLeft = nodeStyle.left.value.value;
                if (nodeStyle.left.value.value + nodeWidth < selectedNodesNearRight) selectedNodesNearRight = nodeStyle.left.value.value + nodeWidth;
                if (nodeStyle.top.value.value < selectedNodesNearTop) selectedNodesNearTop = nodeStyle.top.value.value;
                if (nodeStyle.top.value.value + nodeHeight < selectedNodesNearBottom) selectedNodesNearBottom = nodeStyle.top.value.value + nodeHeight;
            }

            selectedNodesAvgHorizontal = (selectedNodesNearLeft + selectedNodesFarRight) / 2f;//��������ѡ�нڵ��ˮƽ�м�ֵ
            selectedNodesAvgVertical = (selectedNodesNearTop + selectedNodesFarBottom) / 2f;//��������ѡ�нڵ�Ĵ�ֱ�м�ֵ
        }

        /// <summary>���ò���ȡ�˽ڵ��X��Yλ�ã���ȡ���Ⱥ͸߶�</summary>
		public static Rect GetNodeRect(Node node, float left = int.MaxValue, float top = int.MaxValue)
        {
            return new Rect(
                new Vector2(left != int.MaxValue ? left : node.style.left.value.value, top != int.MaxValue ? top : node.style.top.value.value),//X��Y��position
                new Vector2(node.style.width.value.value, node.style.height.value.value)//��Ⱥ͸߶�
            );
        }

        /// <summary>��ѡ�еĽڵ㿿�����(����ѡ������)</summary>
		public void AlignToLeft()
        {
            SetValuesForSelectedNodes();
            if (selectedNodes.Count < 2) return;

            foreach (var selectedNode in selectedNodes)
            {
                selectedNode.SetPosition(GetNodeRect(selectedNode, selectedNodesNearLeft));
            }
        }
        /// <summary>��ѡ�еĽڵ㰴ˮƽ������м����(����ѡ������) ����ͬһ����ֱ����</summary>
		public void AlignToCenter()
        {
            SetValuesForSelectedNodes();
            if (selectedNodes.Count < 2) return;

            foreach (var selectedNode in selectedNodes)
            {
                selectedNode.SetPosition(GetNodeRect(selectedNode, selectedNodesAvgHorizontal - selectedNode.localBound.size.x / 2f));
            }
        }
        /// <summary>��ѡ�еĽڵ㿿�Ҷ���(����ѡ������)</summary>
		public void AlignToRight()
        {
            SetValuesForSelectedNodes();
            if (selectedNodes.Count < 2) return;

            foreach (var selectedNode in selectedNodes)
            {
                selectedNode.SetPosition(GetNodeRect(selectedNode, selectedNodesFarRight - selectedNode.localBound.size.x));
            }
        }
        /// <summary>��ѡ�еĽڵ㿿��������(����ѡ������)</summary>
		public void AlignToTop()
        {
            SetValuesForSelectedNodes();
            if (selectedNodes.Count < 2) return;

            foreach (var selectedNode in selectedNodes)
            {
                selectedNode.SetPosition(GetNodeRect(selectedNode, top: selectedNodesNearTop));
            }
        }
        /// <summary>��ѡ�еĽڵ㰴��ֱ������м����(����ѡ������) ����ͬһˮƽ����</summary>
		public void AlignToMiddle()
        {
            SetValuesForSelectedNodes();
            if (selectedNodes.Count < 2) return;

            foreach (var selectedNode in selectedNodes)
            {
                selectedNode.SetPosition(GetNodeRect(selectedNode, top: selectedNodesAvgVertical - selectedNode.localBound.size.y / 2f));
            }
        }
        /// <summary>��ѡ�еĽڵ㿿�ײ�����(����ѡ������)</summary>
		public void AlignToBottom()
        {
            SetValuesForSelectedNodes();
            if (selectedNodes.Count < 2) return;

            foreach (var selectedNode in selectedNodes)
            {
                selectedNode.SetPosition(GetNodeRect(selectedNode, top: selectedNodesFarBottom - selectedNode.localBound.size.y));
            }
        }

        /// <summary>����Ҽ��ڲ˵����У���NodeViewScript</summary>
		public void OpenNodeViewScript()
        {
            var script = NodeProvider.GetNodeViewScript(GetType());

            if (script != null)
                AssetDatabase.OpenAsset(script.GetInstanceID(), 0, 0);//ʹ�ù�����Ӧ�ó������Դ
        }
        /// <summary>����Ҽ��ڲ˵����У���NodeScript</summary>
		public void OpenNodeScript()
        {
            var script = NodeProvider.GetNodeScript(nodeTarget.GetType());

            if (script != null)
                AssetDatabase.OpenAsset(script.GetInstanceID(), 0, 0);//��ָ�����ʲ��ļ�
        }

        /// <summary>debug���ܵĴ�/�ر�</summary>
		public void ToggleDebug()
        {
            nodeTarget.debug = !nodeTarget.debug;//״̬�л�
            UpdateDebugView();
        }

        //����debug��ǩ����ʾ״̬
        public void UpdateDebugView()
        {
            if (nodeTarget.debug)//Ҫ��ʾdebug(����˳���ǩ)���ʹ����������debugContainer
                mainContainer.Add(debugContainer);
            else//��Ҫ��ʾdebug(����˳���ǩ)���ʹ��������Ƴ�debugContainer
                mainContainer.Remove(debugContainer);
        }

        /// <summary>��ӱ�ʾ��ϢԪ�أ�ָ����ʾ��Ϣ��ͼ�ꡢ��ɫ</summary>
		public void AddMessageView(string message, Texture icon, Color color)
            => AddBadge(new NodeBadgeView(message, icon, color));

        /// <summary>��ӱ�ʾ��ϢԪ�أ�������Ϣ����</summary>
		public void AddMessageView(string message, NodeMessageType messageType)
        {
            IconBadge badge = null;
            switch (messageType)
            {
                case NodeMessageType.Warning:
                    badge = new NodeBadgeView(message, EditorGUIUtility.IconContent("Collab.Warning").image, Color.yellow);//ʹ�ù��캯������Warning��ʾ��Ϣ
                    break;
                case NodeMessageType.Error:
                    badge = IconBadge.CreateError(message);//ʹ�á�error���Ӿ���ʽ���� IconBadge
                    break;
                case NodeMessageType.Info:
                    badge = IconBadge.CreateComment(message);//ʹ�á�comment���Ӿ���ʽ���� IconBadge��
                    break;
                default:
                case NodeMessageType.None:
                    badge = new NodeBadgeView(message, null, Color.grey);////ʹ�ù��캯��������ʾ��Ϣ
                    break;
            }

            AddBadge(badge);
        }

        /// <summary>
        ///  �ڽڵ�����ӱ�ʾԪ�أ�ͬʱ����ʾԪ�ظ��ӵ��ڵ㶥����������Ͻǣ�
        ///  ����badge�б�
        /// </summary>
        /// <param name="badge"></param>
		void AddBadge(IconBadge badge)
        {
            Add(badge);//��˽ڵ�� contentContainer ���IconBadge��ʾԪ��
            badges.Add(badge);//ͬʱ�����б�
            badge.AttachTo(topContainer, SpriteAlignment.TopRight);//���˱�ʾ���ӵ�topContainer(����/��������Ķ�������)����ʾ����λ��ͼ�ξ������Ͻ�
        }

        /// <summary>
        /// �Ƴ�����ƥ�������ı�ʾԪ��
        /// </summary>
        /// <param name="callback"></param>
		void RemoveBadge(Func<IconBadge, bool> callback)
        {
            badges.RemoveAll(b => {
                if (callback(b))
                {
                    b.Detach();//���˱�ʾ����Ŀ���з���
                    b.RemoveFromHierarchy();//����Ԫ�ش��丸�㼶��ͼ��ɾ��
                    return true;
                }
                return false;
            });
        }

        /// <summary>����ʾ�ı����������еı�ʾ��Ϣ�����Ƴ���</summary>
		public void RemoveMessageViewContains(string message) => RemoveBadge(b => b.badgeText.Contains(message));
        /// <summary>������������еı�ʾ��Ϣ��ȵı�ʾ�ı������Ƴ���</summary>
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

        /// <summary>����˳����»ص�����ǩ��ʾ</summary>
        void ComputeOrderUpdatedCallback()
        {
            //Update debug compute order ���µ��Լ���˳��
            computeOrderLabel.text = "Compute order: " + nodeTarget.computeOrder;
        }

        public virtual void Enable(bool fromInspector = false) => DrawDefaultInspector(fromInspector);
        public virtual void Enable() => DrawDefaultInspector(false);

        public virtual void Disable() { }

        /// <summary>
        /// �������ɼ������Ե��ֵ䣺[�漰�����ֶ�]
        ///     ���������ֶε��ֶ�����
        ///     ֵ���������ֶε�ֵ������VisualIf���Ե��ֶΣ�
        /// </summary>
		Dictionary<string, List<(object value, VisualElement target)>> visibleConditions = new Dictionary<string, List<(object value, VisualElement target)>>();
        /// <summary>����ʱ��Ҫ�����ص�Ԫ��</summary>
        Dictionary<string, VisualElement> hideElementIfConnected = new Dictionary<string, VisualElement>();
        /// <summary>�ֵ䣨�Զ����ֶΣ���Ӧ�Ŀؼ���</summary>
        Dictionary<FieldInfo, List<VisualElement>> fieldControlsMap = new Dictionary<FieldInfo, List<VisualElement>>();

        /// <summary>����Զ����inputContainerElement</summary>
		protected void AddInputContainer()
        {
            inputContainerElement = new VisualElement { name = "input-container" };
            mainContainer.parent.Add(inputContainerElement);
            inputContainerElement.SendToBack();//����Ԫ�ط��͵��丸���б�Ŀ�ʼ������Ԫ�ؽ���ʾ�������ص���ͬ��Ԫ��֮��
            inputContainerElement.pickingMode = PickingMode.Ignore;//����ʰȡ
        }

        /// <summary>
        /// ���ƽڵ㣬�����������Ե��ֶκ������Ե��ֶΣ������롱��ʽ���ֶ�(����edge���Ӻ�ᱻ����)
        /// </summary>
        /// <param name="fromInspector"></param>
		protected virtual void DrawDefaultInspector(bool fromInspector = false)
        {
            var fields = nodeTarget.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                // Filter fields from the BaseNode type since we are only interested in user-defined fields
                // (better than BindingFlags.DeclaredOnly because we keep any inherited user-defined fields) 
                // �� BaseNode �����й����ֶΣ���Ϊ����ֻ���û�������ֶθ���Ȥ
                // �� BindingFlags.DeclaredOnly ���ã���Ϊ���Ǳ����κμ̳е��û������ֶ�
                .Where(f => f.DeclaringType != typeof(BaseNode));

            fields = nodeTarget.OverrideFieldOrder(fields).Reverse();//���������ֶκ󣬷�ת������Ԫ�ص�˳��

            foreach (var field in fields)
            {
                //skip if the field is a node setting  ����ֶξ߱�setting����������
                if (field.GetCustomAttribute(typeof(SettingAttribute)) != null)
                {
                    hasSettings = true;
                    continue;
                }

                //skip if the field is not serializable ����ֶβ������л�������
                bool serializeField = field.GetCustomAttribute(typeof(SerializeField)) != null;
                if ((!field.IsPublic && !serializeField) || field.IsNotSerialized)
                {
                    AddEmptyField(field, fromInspector);
                    continue;
                }

                //skip if the field is an input/output and not marked as SerializedField
                //����ֶ�������/������ͣ���δ���Ϊ SerializedField��������
                bool hasInputAttribute = field.GetCustomAttribute(typeof(InputAttribute)) != null;
                bool hasInputOrOutputAttribute = hasInputAttribute || field.GetCustomAttribute(typeof(OutputAttribute)) != null;
                bool showAsDrawer = !fromInspector && field.GetCustomAttribute(typeof(ShowAsDrawer)) != null;
                if (!serializeField && hasInputOrOutputAttribute && !showAsDrawer)
                {
                    AddEmptyField(field, fromInspector);
                    continue;
                }

                //skip if marked with NonSerialized or HideInInspector ������� NonSerialized �� HideInInspector ������
                if (field.GetCustomAttribute(typeof(System.NonSerializedAttribute)) != null || field.GetCustomAttribute(typeof(HideInInspector)) != null)
                {
                    AddEmptyField(field, fromInspector);
                    continue;
                }

                // Hide the field if we want to display in in the inspector ����������ڼ�������ڵ���ͼ������ʾ�������ظ��ֶ�
                var showInInspector = field.GetCustomAttribute<ShowInInspector>();
                if (!serializeField && showInInspector != null && !showInInspector.showInNode && !fromInspector)
                {
                    AddEmptyField(field, fromInspector);
                    continue;
                }

                //�󶨶���
                var bingObject = field.GetCustomAttribute(typeof(BindObjectAttribute)) as BindObjectAttribute;
                if(bingObject != null && field.IsPublic && field.FieldType.IsSubclassOf(typeof(RDTSBehavior)))
                {
                    var objValue = AddBindObjectField(field, bingObject.showName);
                    field.SetValue(nodeTarget, objValue);//���ؼ��еĶ��󸳸����С��󶨶������ԡ����ֶ�
                    continue;
                }

                //showInputDrawer���Ƿ���ʾ����˿ڵġ����롱��ʽ��
                //Ҫ��ʾ�����롱��Ҫ���㣺
                //    ����InputAttribute��(SerializeField��ShowAsDrawer)���ԣ��Ҳ���fromInspector=false���Ҹ��ֶ����͵�ʵ�����ܷ����IList���͵ı�����
                var showInputDrawer = field.GetCustomAttribute(typeof(InputAttribute)) != null && field.GetCustomAttribute(typeof(SerializeField)) != null;
                showInputDrawer |= field.GetCustomAttribute(typeof(InputAttribute)) != null && field.GetCustomAttribute(typeof(ShowAsDrawer)) != null;//��
                showInputDrawer &= !fromInspector; // We can't show a drawer in the inspector ���ǲ����ڼ��������ʾ����
                showInputDrawer &= !typeof(IList).IsAssignableFrom(field.FieldType);//IsAssignableFrom��ȷ��ָ������field.FieldType��ʵ���Ƿ��ܷ������ǰ���͵ı���

                ///ObjectNames������Ϊ���󹹽�����ʾ���Ƶ� Helper ��
                string displayName = ObjectNames.NicifyVariableName(field.Name);//Ϊ������������ʾ�����ƣ��ڴ�д��ĸ֮ǰ����ո񣬲�ɾ������֮ǰ�Ŀ�ѡ m_��_ �� k�������д��ĸ����

                ///InspectorNameAttribute����ö��ֵ����ʹ�ô����Կɸ��� Inspector ����ʾ����ʾ����
                var inspectorNameAttribute = field.GetCustomAttribute<InspectorNameAttribute>();
                if (inspectorNameAttribute != null)
                    displayName = inspectorNameAttribute.displayName;//displayName��Ҫ��Inspector����ʾ������

                //����������/������Ե��ֶλᱻ��ʾ������/���������
                var elem = AddControlField(field, displayName, showInputDrawer);//*��Ӳ��߱����Ե��Զ�����ֶΣ���ö�١�string�ȣ�*

                if (hasInputAttribute)//�������������͵�����
                {
                    hideElementIfConnected[field.Name] = elem;

                    // Hide the field right away if there is already a connection:
                    // ����Ѿ��������ӣ����������ظ��ֶ�
                    if (portsPerFieldName.TryGetValue(field.Name, out var pvs))
                        if (pvs.Any(pv => pv.GetEdges().Count > 0))
                            elem.style.display = DisplayStyle.None;
                }
            }
        }


        /// <summary>���ýڵ��������������ɫ���±�Ե�����ռ�</summary>
		protected virtual void SetNodeColor(Color color)
        {
            //����������
            titleContainer.style.borderBottomColor = new StyleColor(color);//Ԫ���±߿����ɫ
            titleContainer.style.borderBottomWidth = new StyleFloat(color.a > 0 ? 5f : 0f);//�ڲ��ֽ׶�Ϊ�˱߿���±�Ե�����Ŀռ�
        }

        /// <summary>��inputContainerElement���һ�����ֶ�</summary>
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
        /// �����ֶοɼ��ԣ�
        ///     [ VisualIf���������õ������ֶμ�������ֶΣ�����VisualIf���Ե��ֶμ��VisualIf�ֶ� ]
        ///     �������ֶε�ֵ����VisualIf�ֶ���VisualIf���������õĸ������ֶε�ֵʱ��VisualIf�ֶοɼ�����֮�����ɼ�
        /// </summary>
        /// <param name="fieldName">�����ֶε��ֶ���</param>
        /// <param name="newValue">�����ֶε�ֵ</param>
		void UpdateFieldVisibility(string fieldName, object newValue)
        {
            //Debug.Log("�����ֶοɼ���");
            if (newValue == null)//ֵΪ���򷵻�
                return;
            if (visibleConditions.TryGetValue(fieldName, out var list))//������ָ����(fieldName)��Ԫ���򷵻�true����list��ȡ������ָ����(fieldName)���Ӧ��ֵ
            {
                foreach (var elem in list)
                {
                    if (newValue.Equals(elem.value))//������VisualIf���Ե��ֶ�����VisualIf���������õ������ֶε�ֵ ���� �������ֶε�ǰ��ֵ���ͽ��˾���VisualIf���Ե��ֶ���ʾ
                        elem.target.style.display = DisplayStyle.Flex;
                    else//����ȣ����˾���VisualIf���Ե��ֶ�����
                        elem.target.style.display = DisplayStyle.None;
                }
            }
        }

        void UpdateOtherFieldValueSpecific<T>(FieldInfo field, object newValue)
        {
            foreach (var inputField in fieldControlsMap[field])
            {
                //INotifyValueChanged���ؼ��ӿڣ���Щ�ؼ�����ֵ���ҿ������û��������ֵʱ����֪ͨ
                var notify = inputField as INotifyValueChanged<T>;
                if (notify != null)
                    notify.SetValueWithoutNotify((T)newValue);//���ô�ֵnewValue����ʹ��ͬ��Ҳ��ͨ�� ChangeEvent<T> ֪ͨע��ص�
            }
        }

        static MethodInfo specificUpdateOtherFieldValue = typeof(BaseNodeView).GetMethod(nameof(UpdateOtherFieldValueSpecific), BindingFlags.NonPublic | BindingFlags.Instance);
        void UpdateOtherFieldValue(FieldInfo info, object newValue)
        {
            // Warning: Keep in sync with FieldFactory CreateField ���棺�� FieldFactory CreateField ����ͬ��
            var fieldType = info.FieldType.IsSubclassOf(typeof(UnityEngine.Object)) ? typeof(UnityEngine.Object) : info.FieldType;
            var genericUpdate = specificUpdateOtherFieldValue.MakeGenericMethod(fieldType);//���أ�һ�� MethodInfo ���󣬱�ʾͨ������ǰ���ͷ�����������Ͳ����滻Ϊ fieldType ��Ԫ�����ɵĹ��췽��

            genericUpdate.Invoke(this, new object[] { info, newValue });
        }

        /// <summary>
        /// ��fieldControlsMap�ֵ��в����Ƿ��и��������ֶ���Ϣ��Ӧ��VisualElements��
        /// �����д��ڿؼ����ͣ��ͷ��ؿռ䱣���ֵ�����򷵻�null
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
                    if (inputField is INotifyValueChanged<T> notify)//INotifyValueChanged���ؼ��ӿڣ���Щ�ؼ�����ֵ���ҿ������û��������ֵʱ����֪ͨ
                        return notify.value;//�ؼ������ֵ
                }
            }
            return null;
        }

        /// <summary>��BaseNodeView��GetInputFieldValueSpecific����</summary>
		static MethodInfo specificGetValue = typeof(BaseNodeView).GetMethod(nameof(GetInputFieldValueSpecific), BindingFlags.NonPublic | BindingFlags.Instance);
        /// <summary>��BaseNodeView��GetInputFieldValueSpecific����</summary>
        object GetInputFieldValue(FieldInfo info)
        {
            // Warning: Keep in sync with FieldFactory CreateField �� FieldFactory CreateField ����ͬ��
            var fieldType = info.FieldType.IsSubclassOf(typeof(UnityEngine.Object)) ? typeof(UnityEngine.Object) : info.FieldType;
            var genericUpdate = specificGetValue.MakeGenericMethod(fieldType);//MakeGenericMethod�������������Ԫ��fieldType�����ǰ���ͷ�����������Ͳ����������ر�ʾ������췽��

            return genericUpdate.Invoke(this, new object[] { info });
        }

        protected VisualElement AddControlField(string fieldName, string label = null, bool showInputDrawer = false, Action valueChangedCallback = null)
            => AddControlField(nodeTarget.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance), label, showInputDrawer, valueChangedCallback);

        ///Regex����ʾ���ɱ��������ʽ
		Regex s_ReplaceNodeIndexPropertyPath = new Regex(@"(^nodes.Array.data\[)(\d+)(\])");
        //
        internal void SyncSerializedPropertyPathes()
        {
            int nodeIndex = owner.graph.nodes.FindIndex(n => n == nodeTarget);//�ҵ���graph��nodes�б��д˽ڵ������ֵ

            // If the node is not found, then it means that it has been deleted from serialized data.
            // ���δ�ҵ��ýڵ㣬����ζ�����Ѵ����л�������ɾ����
            if (nodeIndex == -1)
                return;

            var nodeIndexString = nodeIndex.ToString();
            foreach (var propertyField in this.Query<PropertyField>().ToList())
            {
                propertyField.Unbind();//�Ͽ��������Եİ�
                                       // The property path look like this: nodes.Array.data[x].fieldName
                                       // And we want to update the value of x with the new node index:
                                       // ����·��������ʾ��nodes.Array.data[x].fieldName ���������µĽڵ��������� x ��ֵ
                                       ///Replace����ָ���������ַ���propertyField.bindingPath�У�ʹ���� MatchEvaluator ί�з��ص��ַ��� [�滻] ��ָ����������ʽƥ��������ַ���
                propertyField.bindingPath = s_ReplaceNodeIndexPropertyPath.Replace(propertyField.bindingPath, m => m.Groups[1].Value + nodeIndexString + m.Groups[3].Value);
                propertyField.Bind(owner.serializedGraph);
            }
        }

        /// <summary>
        /// �����ֶ����������������л�graph�����л�nodes�б��ж�Ӧ�����л��ֶ�����(SerializedProperty)
        /// [ SerializedProperty��ͨ�õ������࣬�������е����л����� ]
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
		protected SerializedProperty FindSerializedProperty(string fieldName)
        {
            int i = owner.graph.nodes.FindIndex(n => n == nodeTarget);//�ҵ��˽ڵ���ͼ��graph�е�����
            ///FindProperty�������Ʋ������л����ԣ������л�graph�в���nodes�б�����л����ԣ�
            ///GetArrayElementAtIndex������������ָ����������Ԫ��
            ///FindPropertyRelative���ӵ�ǰ���Ե����·��(fieldName)���� SerializedProperty
            return owner.serializedGraph.FindProperty("nodes").GetArrayElementAtIndex(i).FindPropertyRelative(fieldName);
        }

        /// <summary>
        /// ���ݰ��ֶ���������Ӷ�Ӧ�İ��ֶεĿؼ�
        /// </summary>
        private RDTSBehavior oldObject;
        protected object AddBindObjectField(FieldInfo field, string showName = null)
        {
           
            Type type = field.FieldType;//�ֶ�����

            ///����һ��Object�ؼ�
            var objField = new ObjectField
            {
                objectType = type,//����
                allowSceneObjects = true,//������scene����

            };
            ///�ڿؼ�����ʱ�����֮ǰ�Ѽ�¼�Ķ���
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

            ///�ص������Ŀؼ���ֵ(����)ʱ����
            objField.RegisterValueChangedCallback(v =>
            {
                var sig = objField.value as RDTSBehavior;
                if (oldObject != sig && oldObject != null)//��������ʱ�����ɶ����Guid���
                    oldObject.Guid = null;


                if (nodeTarget.bindGuid != null && sig != null)//����ؼ���Guid������
                {
                    sig.Guid = nodeTarget.bindGuid;
                }
                else if (nodeTarget.bindGuid == null && sig != null)//���ǵ�һ�ΰ󶨶��󣬾��·���һ��Guid���ؼ�
                {
                    string guid = System.Guid.NewGuid().ToString();
                    sig.Guid = nodeTarget.bindGuid = guid;
                }

                if (sig != null)
                    EditorUtility.SetDirty(sig);//��������Guid

                oldObject = objField.value as RDTSBehavior;//���˶����¼Ϊ�ɵ�
                field.SetValue(nodeTarget, objField.value);//*���ؼ��еĶ��󸳸����С��󶨶������ԡ����ֶ�

            });


            ///��ӿؼ�
            controlsContainer.Add(objField);

            return objField.value;

        }



        /// <summary>
        /// ���һ���ɿ��Ƶ��ֶ�
        /// </summary>
        /// <param name="field">�Զ�����ֶ�</param>
        /// <param name="label">��ʾ����</param>
        /// <param name="showInputDrawer">true�ԡ����롱��ʽ��ʾ���ֶΡ�false������ͨ�ֶ���ʽ��ʾ</param>
        /// <param name="valueChangedCallback"></param>
        /// <returns></returns>
		protected VisualElement AddControlField(FieldInfo field, string label = null, bool showInputDrawer = false, Action valueChangedCallback = null)
        {
            if (field == null)
                return null;

            ///PropertyField������һ�� SerializedProperty ��װ�� VisualElement���ڵ��� Bind() ʱ��ʹ����ȷ�� bindingPath ������ȷ���ֶ�Ԫ��
			var element = new PropertyField(FindSerializedProperty(field.Name), showInputDrawer ? "" : label);
            
            element.Bind(owner.serializedGraph);//�� SerializedObject(��owner.serializedGraph) �󶨵�Ԫ�ز�νṹ�е��ֶ�

#if UNITY_2020_3 // In Unity 2020.3 the empty label on property field doesn't hide it, so we do it manually
			if ((showInputDrawer || String.IsNullOrEmpty(label)) && element != null)
				element.AddToClassList("DrawerField_2020_3");
#endif
            //����б���ֶ�
            if (typeof(IList).IsAssignableFrom(field.FieldType))
                EnableSyncSelectionBorderHeight();//����ͬ��ѡ��߿�߶�

            //ע��˻ص�����ֵ����ʱ���� SerializedPropertyChangeEvent
            element.RegisterValueChangeCallback(e => {
                //GetValue:��������������дʱ�����ظ�������nodeTarget�е��ֶε�ֵ
                UpdateFieldVisibility(field.Name, field.GetValue(nodeTarget));//�����ֶε�ֵ�仯ʱ���Խ����ֶε��������ֶε��ֶν��пɼ���״̬����
                valueChangedCallback?.Invoke();
                NotifyNodeChanged();//��ͼ��graph�����¼�����֪�ýڵ������Ѹ���
                //Debug.Log("element value change");
            });

            // Disallow picking scene objects when the graph is not linked to a scene
            // ��ͼ��graphδ���ӵ�����ʱ��������ѡȡ��������
            if (element != null && !owner.graph.IsLinkedToScene())
            {
                var objectField = element.Q<ObjectField>();//����һ���ɽ����κζ������͵��ֶ�
                if (objectField != null)
                {
                    objectField.allowSceneObjects = false;//������Ϊ���ֶη��䳡������
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
                if (showInputDrawer)//��Ҫ��ʾ��������롱����ʽ
                {
                    var box = new VisualElement { name = field.Name };
                    box.AddToClassList("port-input-element");
                    box.Add(element);
                    inputContainerElement.Add(box);//��ӵ��Զ���inputContainerElement������
                }
                else
                {
                  
                    controlsContainer.Add(element);//��ӵ��Զ���controlsContainer������
                }
                element.name = field.Name;

            }
            else
            {
                // Make sure we create an empty placeholder if FieldFactory can not provide a drawer
                // ��� FieldFactory �޷��ṩ���룬��ȷ�����Ǵ���һ����ռλ��
                if (showInputDrawer) AddEmptyField(field, false);
            }

            var visibleCondition = field.GetCustomAttribute(typeof(VisibleIf)) as VisibleIf;
            if (visibleCondition != null)//�����С������ɼ�������
            {
                ///Debug.Log("visibleCondition is not null");
                // Check if target field exists: ���Ŀ���ֶ��Ƿ����
                //visibleCondition.fieldName�������ֶε��ֶ���
                var conditionField = nodeTarget.GetType().GetField(visibleCondition.fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (conditionField == null)//���˽ڵ㲻����VisibleIf���Ե��ֶ�
                    Debug.LogError($"[VisibleIf] Field {visibleCondition.fieldName} does not exists in node {nodeTarget.GetType()}");
                else//������
                {
                    visibleConditions.TryGetValue(visibleCondition.fieldName, out var list);
                    if (list == null)
                        list = visibleConditions[visibleCondition.fieldName] = new List<(object value, VisualElement target)>();
                    list.Add((visibleCondition.value, element));//�б����ͣ������ֶε�ֵ������VisualIf���Ե��ֶΣ�
                    UpdateFieldVisibility(visibleCondition.fieldName, conditionField.GetValue(nodeTarget));//GetValue(nodeTarget)������nodeTarget֧�ֵ��ֶε�ֵ�������Ｔ�����ֶε�ֵ
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
        /// �����ð�ť������ֵ�Ԫ���У����setting�ֶ�
        /// </summary>
        /// <param name="field"></param>
        protected void AddSettingField(FieldInfo field)
        {
            if (field == null)
                return;

            var label = field.GetCustomAttribute<SettingAttribute>().name;

            var element = new PropertyField(FindSerializedProperty(field.Name));//���Ҵ��� [Setting("Compare Function")] ���Ե��ֶ�
            element.Bind(owner.serializedGraph);

            if (element != null)
            {
                settingsContainer.Add(element);
                element.name = field.Name;//�ֶ�����
            }
        }

        internal void OnPortConnected(PortView port)
        {
            if (port.direction == Direction.Input && inputContainerElement?.Q(port.fieldName) != null)
                inputContainerElement.Q(port.fieldName).AddToClassList("empty");

            if (hideElementIfConnected.TryGetValue(port.fieldName, out var elem))
                elem.style.display = DisplayStyle.None;//����Ԫ���ڲ����е���ʾ��ʽ�� DisplayStyle.None��Ԫ�ز��ɼ����ڲ����в�����

            onPortConnected?.Invoke(port);//���ö˿�����ʱ��ί�з���
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
                        fieldInfo.info.SetValue(nodeTarget, valueBeforeConnection);//��valueBeforeConnectionֵ���õ�nodeTarget
                    }
                }
            }

            if (hideElementIfConnected.TryGetValue(port.fieldName, out var elem))//�Ͽ�����ʱ���������ص�Ԫ����������
                elem.style.display = DisplayStyle.Flex;

            onPortDisconnected?.Invoke(port);//���ö˿ڶϿ�����ʱ��ί�з���
        }

        // TODO: a function to force to reload the custom behavior ports (if we want to do a button to add ports for example)

        public virtual void OnRemoved() { }
        public virtual void OnCreated() { }

        /// <summary>
        /// [����]���ýڵ�λ��
        /// </summary>
        /// <param name="newPos"></param>
		public override void SetPosition(Rect newPos)
        {
            if (initializing || !nodeTarget.isLocked)//����ʼ�� ��δ����
            {
                base.SetPosition(newPos);//���ýڵ㵽�µ�λ��

                if (!initializing)//�����ǳ�ʼ����ע�᳷������
                    owner.RegisterCompleteObjectUndo("Moved graph node");

                nodeTarget.position = newPos;//���½ڵ���Ϣ�е�λ����Ϣ
                initializing = false;
            }
        }

        public override bool expanded//�ڵ��Ƿ�����չ
        {
            get { return base.expanded; }
            set
            {
                base.expanded = value;
                nodeTarget.expanded = value;
            }
        }

        /// <summary>�ı�Lock״̬</summary>
        public void ChangeLockStatus()
        {
            ///ȡ��
            nodeTarget.nodeLock ^= true;//�����ͬΪfalse����ͬΪtrue
        }

        /// <summary>
        /// [����]��ڵ������Ĳ˵���Ӳ˵���
        /// </summary>
        /// <param name="evt">�������Ĳ˵���Ҫ�˵���ʱ���͵��¼�</param>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            ///AppendAction���������˵������һ����ִ�в����Ĳ˵���˲˵�������ڵ�ǰ�˵����б�ĩβ
			BuildAlignMenu(evt);
            evt.menu.AppendAction("Open Node Script", (e) => OpenNodeScript(), OpenNodeScriptStatus);
            evt.menu.AppendAction("Open Node View Script", (e) => OpenNodeViewScript(), OpenNodeViewScriptStatus);
            evt.menu.AppendAction("Debug", (e) => ToggleDebug(), DebugStatus);
            if (nodeTarget.unlockable)
                evt.menu.AppendAction((nodeTarget.isLocked ? "Unlock" : "Lock"), (e) => ChangeLockStatus(), LockStatus);
        }
        //���빦�ܲ˵�
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
            return Status.Normal;//�˵���������ʾ
        }

        Status DebugStatus(DropdownMenuAction action)
        {
            if (nodeTarget.debug)
                return Status.Checked;//�˵�����ʾΪ����ѡ���
            return Status.Normal;
        }

        Status OpenNodeScriptStatus(DropdownMenuAction action)
        {
            if (NodeProvider.GetNodeScript(nodeTarget.GetType()) != null)
                return Status.Normal;
            return Status.Disabled; //�˵����ѽ��ã��û�����ѡ��

        }

        Status OpenNodeViewScriptStatus(DropdownMenuAction action)
        {
            if (NodeProvider.GetNodeViewScript(GetType()) != null)
                return Status.Normal;
            return Status.Disabled;
        }

        /// <summary>
        /// ͬ���˿ںͶ˿���ͼ�ж˿ڵ�����
        /// </summary>
        /// <param name="ports"></param>
        /// <param name="portViews"></param>
        /// <returns></returns>
		IEnumerable<PortView> SyncPortCounts(IEnumerable<NodePort> ports, IEnumerable<PortView> portViews)
        {
            var listener = owner.connectorListener;
            var portViewList = portViews.ToList();

            // Maybe not good to remove ports as edges are still connected :/
            // ���ڱ�Ե��Ȼ���ӣ����ɾ���˿ڿ��ܲ���
            foreach (var pv in portViews.ToList())
            {
                // If the port have disappeared from the node data, we remove the view: ����˿��Ѿ��ӽڵ���������ʧ������ɾ���˿���ͼ
                // We can use the identifier here because this function will only be called when there is a custom port behavior
                // ���ǿ���������ʹ�ñ�ʶ������Ϊֻ�������Զ���˿���Ϊʱ�Ż���ô˺���
                if (!ports.Any(p => p.portData.identifier == pv.portData.identifier))//portViews���е�portsû�У���ɾ��
                {
                    RemovePort(pv);//�Ƴ��˿ڼ���edge
                    portViewList.Remove(pv);//�ӣ��������б����Ƴ�
                }
            }

            foreach (var p in ports)
            {
                // Add missing port views ���ȱ�ٵĶ˿���ͼ
                if (!portViews.Any(pv => p.portData.identifier == pv.portData.identifier))//ports���е�portViews��û�У������
                {
                    Direction portDirection = nodeTarget.IsFieldInput(p.fieldName) ? Direction.Input : Direction.Output;//�ж϶˿�����
                    var pv = AddPort(p.fieldInfo, portDirection, listener, p.portData);
                    portViewList.Add(pv);
                }
            }

            return portViewList;
        }
        /// <summary>
        /// ͬ���˿ںͶ˿���ͼ�ж˿ڵ�˳��
        /// </summary>
        /// <param name="ports"></param>
        /// <param name="portViews"></param>
		void SyncPortOrder(IEnumerable<NodePort> ports, IEnumerable<PortView> portViews)
        {
            var portViewList = portViews.ToList();
            var portsList = ports.ToList();

            // Re-order the port views to match the ports order in case a custom behavior re-ordered the ports
            // ��������˿���ͼ��ƥ��˿�˳���Է��Զ�����Ϊ��������˿�
            for (int i = 0; i < portsList.Count; i++)
            {
                var id = portsList[i].portData.identifier;

                var pv = portViewList.FirstOrDefault(p => p.portData.identifier == id);
                if (pv != null)
                    InsertPort(pv, i);//������/����˿������в���˿�
            }
        }

        /// <summary>
        /// ��ȷ��ӵ����ͬ������port��portView����ˢ�¸���inputPortViews��outputPortViews�˿���ͼ
        /// </summary>
        /// <returns></returns>
		public virtual new bool RefreshPorts()
        {
            // If a port behavior was attached to one port, then
            // the port count might have been updated by the node
            // so we have to refresh the list of port views.
            //����˿���Ϊ���ӵ�һ���˿ڣ���˿ڼ����������ɽڵ���£�������Ǳ���ˢ�¶˿���ͼ�б�
            UpdatePortViewWithPorts(nodeTarget.inputPorts, inputPortViews);//����inputPortViews
            UpdatePortViewWithPorts(nodeTarget.outputPorts, outputPortViews);//����outputPortViews

            ///�ȱ�֤port��portView��������˳����ͬ���ٶԸ����˿���ͼ���и��²���
			void UpdatePortViewWithPorts(NodePortContainer ports, List<PortView> portViews)
            {
                if (ports.Count == 0 && portViews.Count == 0) // Nothing to update ��Ϊ������Ҫˢ��
                    return;

                /* �ȱ�֤port��portView��������ͬ����˳��Ҳ��ͬ */
                // When there is no current portviews, we can't zip the list so we just add all
                //��û�е�ǰ�˿���ͼʱ�������޷�ѹ���б���������ֻ���������
                if (portViews.Count == 0)
                    SyncPortCounts(ports, new PortView[] { });
                else if (ports.Count == 0) // Same when there is no ports
                    SyncPortCounts(new NodePort[] { }, portViews);
                else if (portViews.Count != ports.Count)
                    SyncPortCounts(ports, portViews);
                else
                {
                    ///GroupBy������ָ���ļ�ѡ���������������е�Ԫ�ؽ��з���
                    //��ports��portViews���ն˿ڵ�fieldName���з��飬���صĶ���<string,NodePort>��������(fieldName)�Ͷ�������(NodePort) 
                    var p = ports.GroupBy(n => n.fieldName);
                    var pv = portViews.GroupBy(v => v.fieldName);
                    ///Zip:��ָ������Ӧ�����������еĶ�ӦԪ�أ������ɽ������
					p.Zip(pv, (portPerFieldName, portViewPerFieldName) => {
                        IEnumerable<PortView> portViewsList = portViewPerFieldName;
                        if (portPerFieldName.Count() != portViewPerFieldName.Count())//���������
                            portViewsList = SyncPortCounts(portPerFieldName, portViewPerFieldName);//ͬ������
                        SyncPortOrder(portPerFieldName, portViewsList);//ͬ��˳��
                        // We don't care about the result, we just iterate over port and portView ���ǲ����Ľ��������ֻ�ǵ����˿ںͶ˿���ͼ
                        return "";
                    }).ToList();//ת�����б�
                }

                // Here we're sure that we have the same amount of port and portView
                // so we can update the view with the new port data (if the name of a port have been changed for example)
                //�����������ܹ�ȷ��ӵ����ͬ������port��portView
                //������ǿ���ʹ���µĶ˿����ݸ�����ͼ�����磬����˿������Ѹ��ģ�

                /* �ٸ��¶˿���ͼ */
                for (int i = 0; i < portViews.Count; i++)
                    portViews[i].UpdatePortView(ports[i].portData);
            }

            return base.RefreshPorts();//ˢ�¶˿ڲ���
        }

        /// <summary>
        /// ǿ�Ƹ��¶˿�
        /// </summary>
        public void ForceUpdatePorts()
        {
            nodeTarget.UpdateAllPorts();

            RefreshPorts();
        }

        /// <summary>
        /// ˢ�¶˿���ͼ
        /// </summary>
        /// <param name="fieldName"></param>
		void UpdatePortsForField(string fieldName)
        {
            // TODO: actual code
            RefreshPorts();
        }

        /// <summary>����һ���ı�Ϊ��Settings��������Ϊ��header���ı�ǩ�ֶ�</summary>
		protected virtual VisualElement CreateSettingsView() => new Label("Settings") { name = "header" };

        /// <summary>
        /// Send an event to the graph telling that the content of this node have changed
        /// ��ͼ��graph�����¼�����֪�ýڵ�������Ѹ���
        /// </summary>
        public void NotifyNodeChanged() => owner.graph.NotifyNodeChanged(nodeTarget);

        #endregion
    }
}