using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// Group the selected node when created ����ʱ��ѡ���ڵ���з��顣
    /// [�����л�]�������⡢��ɫ��λ�á��ߴ磬����¼����ڵ��GUID
    /// </summary>
    [System.Serializable]
    public class Group
    {
        public string title;
        public Color color = new Color(0, 0, 0, 0.3f);
        public Rect position;
        public Vector2 size;

        /// <summary>
        /// Store the GUIDs of the node in the group  ��group�д���ڵ��GUID
        /// </summary>
        /// <typeparam name="string">GUID of a node</typeparam>
        /// <returns></returns>
        public List<string> innerNodeGUIDs = new List<string>();

        // For serialization loading �������л�����
        public Group() { }

        /// <summary>
        /// Create a new group with a title and a position
        /// �������ı����λ�ô���group
        /// </summary>
        /// <param name="title"></param>
        /// <param name="position"></param>
        public Group(string title, Vector2 position)
        {
            this.title = title;
            this.position.position = position;
        }

        /// <summary>
        /// Called when the Group is created ��group������ʱ����
        /// </summary>
        public virtual void OnCreated()
        {
            size = new Vector2(400, 200);
            position.size = size;
        }
    }
}