using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// GraphView的Edge元素：是否连接，样式，尺寸，中继节点添加
    /// </summary>
	public class EdgeView : Edge
    {
        public bool isConnected = false;//是否连接

        public SerializableEdge serializedEdge { get { return userData as SerializableEdge; } }//userData：此属性可用于将特定于应用程序的用户数据与此 VisualElement 关联

        readonly string edgeStyle = "GraphStyles/EdgeView";

        protected BaseGraphView owner => ((input ?? output) as PortView).owner.owner;
        //??：null合并运算符，若input为null则赋值为output


        public EdgeView() : base()
        {
            styleSheets.Add(Resources.Load<StyleSheet>(edgeStyle));
            RegisterCallback<MouseDownEvent>(OnMouseDown);//向实例添加事件处理程序OnMouseDown：添加中继节点
        }

        /// <summary>
        /// 在边缘上的端口更改时调用。
        /// </summary>
        /// <param name="isInput">如果输入端口已更改，则为 true。如果输出端口已更改，则为 false</param>
        public override void OnPortChanged(bool isInput)
        {
            base.OnPortChanged(isInput);
            UpdateEdgeSize();
        }

        public void UpdateEdgeSize()
        {
            if (input == null && output == null)
                return;

            PortData inputPortData = (input as PortView)?.portData;
            PortData outputPortData = (output as PortView)?.portData;

            for (int i = 1; i < 20; i++)
                RemoveFromClassList($"edge_{i}");//从元素的类列表中删除一个类。  参数：要删除到列表中的类的名称
            int maxPortSize = Mathf.Max(inputPortData?.sizeInPixel ?? 0, outputPortData?.sizeInPixel ?? 0);//最大端口尺寸
            if (maxPortSize > 0)
                AddToClassList($"edge_{Mathf.Max(1, maxPortSize - 6)}");//将一个类添加到元素的类列表中，以便从 USS 分配样式。
        }

        /// <summary>
        /// 在解析自定义样式属性时调用
        /// </summary>
        /// <param name="styles"></param>
		protected override void OnCustomStyleResolved(ICustomStyle styles)
        {
            base.OnCustomStyleResolved(styles);

            UpdateEdgeControl();//更新边缘的 EdgeControl
        }

        /// <summary>
        /// 鼠标按两下，在光标处创建中继节点
        /// </summary>
        /// <param name="e"></param>
		void OnMouseDown(MouseDownEvent e)
        {
            if (e.clickCount == 2)//按下键的次数
            {
                // Empirical offset: 经验偏移
                var position = e.mousePosition;
                position += new Vector2(-10f, -28);
                //ChangeCoordinatesTo：将一个点从一个元素的局部空间转换到另一个元素的局部空间
                Vector2 mousePos = owner.ChangeCoordinatesTo(owner.contentViewContainer, position);

                owner.AddRelayNode(input as PortView, output as PortView, mousePos);//添加中继节点
                Debug.Log("鼠标点击两下，创建中继节点");
            }
        }
    }
}