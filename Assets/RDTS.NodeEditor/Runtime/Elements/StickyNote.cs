using System;
using UnityEngine;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// Serializable Sticky node class  ����ǩ���ڵ�
    /// [�����л�]������λ��(�ߴ�)�����⣬����
    /// </summary>
    [Serializable]
    public class StickyNote
    {
        public Rect position;//λ��
        public string title = "Hello World!";//����
        public string content = "Description";//����

        /// <summary>���ñ����λ��</summary>
        public StickyNote(string title, Vector2 position)
        {
            this.title = title;
            this.position = new Rect(position.x, position.y, 200, 300);
        }
    }
}