using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEngine;
using System;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// “黑板”元素PinnedElement视图的基类，确定元素样式、基础功能(可拖动，可重新确定尺寸大小等)、标题、初始化
    /// </summary>
	public abstract class PinnedElementView : GraphElement
    {
        protected PinnedElement pinnedElement;//[可序列化]像黑板一样覆盖图形的元素
        protected VisualElement root;
        protected VisualElement content;
        protected VisualElement header;

        protected event Action onResized;

        VisualElement main;
        Label titleLabel;
        bool _scrollable;
        ScrollView scrollView;//ScrollView：在可滚动框架内显示其内容

        static readonly string pinnedElementStyle = "GraphStyles/PinnedElementView";//uss文件：确定样式
        static readonly string pinnedElementTree = "GraphUXML/PinnedElement";//uxml文件：定义元素的界面

        /// <summary>
        /// [重载]标题：用于获取或设置titleLabel
        /// </summary>
        public override string title
        {
            get { return titleLabel.text; }
            set { titleLabel.text = value; }
        }

        protected bool scrollable
        {
            get
            {
                return _scrollable;
            }
            set
            {
                if (_scrollable == value)//若已等于要设置的值，就直接返回
                    return;

                _scrollable = value;

                style.position = Position.Absolute;//Absolute：元素的放置与其父容器框相关，并且不再影响到布局
                if (_scrollable)//若需要滚动显示：添加scrollView元素并将content元素作为其子级
                {
                    content.RemoveFromHierarchy();//从父级从将此content元素移除
                    root.Add(scrollView);//添加滚动框架
                    scrollView.Add(content);//在滚动框架中添加此content
                    AddToClassList("scrollable");
                }
                else//若无需滚动显示：将scrollView元素移除，并将content元素作为root的子级
                {
                    scrollView.RemoveFromHierarchy();//从父级从将此scrollView元素移除
                    content.RemoveFromHierarchy();
                    root.Add(content);
                    RemoveFromClassList("scrollable");
                }
            }
        }

        /// <summary>
        /// 构造函数：样式，相关设置，可拖动且不会被拖出边缘，可重新确定尺寸大小
        /// </summary>
		public PinnedElementView()
        {
            var tpl = Resources.Load<VisualTreeAsset>(pinnedElementTree);//引用到Asset中设计的元素界面即PinnedElement.uxml
            styleSheets.Add(Resources.Load<StyleSheet>(pinnedElementStyle));

            main = tpl.CloneTree();
            main.AddToClassList("mainContainer");
            scrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);

            root = main.Q("content");//PinnedElement.uxml的根容器

            header = main.Q("header");//PinnedElement.uxml的标头

            titleLabel = main.Q<Label>(name: "titleLabel");//PinnedElement.uxml的标题标签
            content = main.Q<VisualElement>(name: "contentContainer");//PinnedElement.uxml的内容容器

            ///hierarchy：访问此元素物理层级视图
            hierarchy.Add(main);

            capabilities |= Capabilities.Movable | Capabilities.Resizable;//可移动，可重新确定大小
            ///overflow：容器在内容溢出容器框时的行为
            ///Overflow：定义当内容溢出元素边界时该如何处理
            style.overflow = Overflow.Hidden;//Hidden：溢出的内容会被剪掉，其余内容将不可见

            ClearClassList();
            AddToClassList("pinnedElement");

            ///Dragger：用于使用鼠标拖动元素的基本操控器
            //使得此元素能够被拖动，且不会被拖出graph的边缘
            this.AddManipulator(new Dragger { clampToParentEdges = true });//clampToParentEdges如果为 true，它将不允许拖动的元素退出父元素的边缘

            scrollable = false;

            ///Resizer：大小调整程序操控器元素
            //使得此元素能够被自定义尺寸大小
            hierarchy.Add(new Resizer(() => onResized?.Invoke()));

            ///DragUpdatedEvent：拖动的元素进入可能的放置目标时发送的事件
            RegisterCallback<DragUpdatedEvent>(e =>
            {
                e.StopPropagation();
            });

            title = "PinnedElementView";
        }

        /// <summary>
        /// 初始化PinnedElement元素视图
        /// </summary>
        /// <param name="pinnedElement"></param>
        /// <param name="graphView"></param>
		public void InitializeGraphView(PinnedElement pinnedElement, BaseGraphView graphView)
        {
            this.pinnedElement = pinnedElement;
            SetPosition(pinnedElement.position);//设置位置和大小

            //为委托方法添加调用方法
            onResized += () => {
                pinnedElement.position.size = layout.size;
                ///Debug.Log("Resizer pinnedElement");//当改变元素大小且鼠标松开时打印
            };
            //注册回调
            RegisterCallback<MouseUpEvent>(e => {
                pinnedElement.position.position = layout.position;
                ///Debug.Log("Set position pinnedElement");//当点击元素(包括拖动元素)且鼠标按键抬起时打印
            });

            Initialize(graphView);//初始化与关联的图表graph的相关设置
        }

        /// <summary>重置元素位置和尺寸大小</summary>
		public void ResetPosition()
        {
            pinnedElement.position = new Rect(Vector2.zero, PinnedElement.defaultSize);
            SetPosition(pinnedElement.position);
        }

        /// <summary>
        /// 初始化与关联的图表graph的相关设置：在继承的子类中实现此方法，实现具体的功能
        /// </summary>
        /// <param name="graphView">正在操作的图表graph</param>
		protected abstract void Initialize(BaseGraphView graphView);

        ~PinnedElementView()
        {
            Destroy();
        }

        protected virtual void Destroy() { }
    }
}
