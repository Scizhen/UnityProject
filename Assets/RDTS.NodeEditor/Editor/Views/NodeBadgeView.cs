using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// �ڵ��ע�ͱ�ʾ
    /// </summary>
	public class NodeBadgeView : IconBadge
    {
        Label label;
        Texture icon;
        Color color;
        bool isCustom;

        /// <summary>
        /// ���캯����������Ϣ���ʹ����Զ����ʾ��Ϣ
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageType"></param>
		public NodeBadgeView(string message, NodeMessageType messageType)
        {
            switch (messageType)
            {
                case NodeMessageType.Warning://Warning��Ϣ����unity������Դ�л�ȡͼ�꣬��ɫ
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
        /// ���캯��������ָ����ͼ�����ɫ������ʾ��Ϣ
        /// </summary>
        /// <param name="message"></param>
        /// <param name="icon"></param>
        /// <param name="color"></param>
		public NodeBadgeView(string message, Texture icon, Color color)
        {
            CreateCustom(message, icon, color);
        }

        /// <summary>
        /// ����һ���Զ���ı�ʾ����Ϣ��ͼ����ɫ��
        /// </summary>
        /// <param name="message">��Ϣ</param>
        /// <param name="icon"></param>
        /// <param name="color"></param>
		void CreateCustom(string message, Texture icon, Color color)
        {
            badgeText = message;//�����ͣ�ı�ʾ����ʾ���ı�
            this.color = color;

            var image = this.Q<Image>("icon");
            image.image = icon;
            image.style.backgroundColor = color;
            style.color = color;
            // This will set a class name containing the hash code of the string �⽫����һ�������ַ�����ϣ�������
            // We use this little trick to retrieve the label once it is added to the graph һ����ǩ����ӵ�ͼ�У�����ʹ�����С������������ǩ
            // visualStyle����ʾ�Ŀɼ���ʽ������ֵΪ "error" �� "comment"
            visualStyle = badgeText.GetHashCode().ToString();
        }

        /// <summary>
        /// ���¼�Ŀ����ע��Ļص�ִ�к�ִ���߼��������¼��ѱ����Ϊ��ֹ��Ĭ����Ϊ�� EventBase{T}.PreventDefault��
        /// </summary>
        /// <param name="evt"></param>
		protected override void ExecuteDefaultAction(EventBase evt)
        {
            // When the mouse enter the icon, this will add the label to the hierarchy
            //��������ͼ��ʱ����Ὣ��ǩ��ӵ���νṹ��
            base.ExecuteDefaultAction(evt);

            if (evt.eventTypeId == MouseEnterEvent.TypeId())//�Ƚ��¼����͵�ID
            {
                // And then we can fetch it here: Ȼ�����ǿ����������ȡ��
                GraphView gv = GetFirstAncestorOfType<GraphView>();
                var label = gv.Q<Label>(classes: new string[] { "icon-badge__text--" + badgeText.GetHashCode() });
                if (label != null)
                    label.style.color = color;
            }
        }
    }
}