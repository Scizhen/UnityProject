using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// 节点的注释标示
    /// </summary>
	public class NodeBadgeView : IconBadge
    {
        Label label;
        Texture icon;
        Color color;
        bool isCustom;

        /// <summary>
        /// 构造函数：依据消息类型创建自定义标示信息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageType"></param>
		public NodeBadgeView(string message, NodeMessageType messageType)
        {
            switch (messageType)
            {
                case NodeMessageType.Warning://Warning消息：从unity内置资源中获取图标，黄色
                    CreateCustom(message, EditorGUIUtility.IconContent("Collab.Warning").image, Color.yellow);
                    break;
                case NodeMessageType.Error:
                    CreateCustom(message, EditorGUIUtility.IconContent("Collab.Warning").image, Color.red);
                    break;
                case NodeMessageType.Info:
                    CreateCustom(message, EditorGUIUtility.IconContent("console.infoicon").image, Color.white);
                    break;
                default:
                case NodeMessageType.None:
                    CreateCustom(message, null, Color.grey);
                    break;
            }
        }

        /// <summary>
        /// 构造函数：按照指定的图标和颜色创建标示信息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="icon"></param>
        /// <param name="color"></param>
		public NodeBadgeView(string message, Texture icon, Color color)
        {
            CreateCustom(message, icon, color);
        }

        /// <summary>
        /// 创建一个自定义的标示（信息，图表，颜色）
        /// </summary>
        /// <param name="message">信息</param>
        /// <param name="icon"></param>
        /// <param name="color"></param>
		void CreateCustom(string message, Texture icon, Color color)
        {
            badgeText = message;//鼠标悬停的标示旁显示的文本
            this.color = color;

            var image = this.Q<Image>("icon");
            image.image = icon;
            image.style.backgroundColor = color;
            style.color = color;
            // This will set a class name containing the hash code of the string 这将设置一个包含字符串哈希码的类名
            // We use this little trick to retrieve the label once it is added to the graph 一旦标签被添加到图中，我们使用这个小技巧来检索标签
            // visualStyle：标示的可见样式。内置值为 "error" 和 "comment"
            visualStyle = badgeText.GetHashCode().ToString();
        }

        /// <summary>
        /// 在事件目标上注册的回调执行后执行逻辑，除非事件已被标记为阻止其默认行为。 EventBase{T}.PreventDefault。
        /// </summary>
        /// <param name="evt"></param>
		protected override void ExecuteDefaultAction(EventBase evt)
        {
            // When the mouse enter the icon, this will add the label to the hierarchy
            //当鼠标进入图标时，这会将标签添加到层次结构中
            base.ExecuteDefaultAction(evt);

            if (evt.eventTypeId == MouseEnterEvent.TypeId())//比较事件类型的ID
            {
                // And then we can fetch it here: 然后我们可以在这里获取它
                GraphView gv = GetFirstAncestorOfType<GraphView>();
                var label = gv.Q<Label>(classes: new string[] { "icon-badge__text--" + badgeText.GetHashCode() });
                if (label != null)
                    label.style.color = color;
            }
        }
    }
}