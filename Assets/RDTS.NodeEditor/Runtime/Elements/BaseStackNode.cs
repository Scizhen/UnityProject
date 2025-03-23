using UnityEngine;
using System.Collections.Generic;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// Data container for the StackNode views StackNode ��ջ�ڵ������������
    ///[�����л�]������λ�ã����⣬�Ƿ�����Ϸţ��Ƿ���ܿո�������Ľڵ�
    /// </summary>
    [System.Serializable]
    public class BaseStackNode
    {
        public Vector2 position;//λ��
        public string title = "New Stack";//����

        /// <summary>
        /// Is the stack accept drag and dropped nodes  �Ƿ�����ϷŽڵ�
        /// </summary>
        public bool acceptDrop;

        /// <summary>
        /// Is the stack accepting node created by pressing space over the stack node
        /// �Ƿ�����ڴ˶�ջ�ڵ��ϰ��ո�������Ľڵ�
        /// </summary>
        public bool acceptNewNode;

        /// <summary>
        /// List of node GUID that are in the stack �ڶ�ջ�ڵ��д�ŵĽڵ��GUID
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <returns></returns>
        public List<string> nodeGUIDs = new List<string>();

        /// <summary>���캯��������λ�ã����⣬�Ƿ�����Ϸţ��Ƿ���ܿո�������Ľڵ�</summary>
        public BaseStackNode(Vector2 position, string title = "Stack", bool acceptDrop = true, bool acceptNewNode = true)
        {
            this.position = position;
            this.title = title;
            this.acceptDrop = acceptDrop;
            this.acceptNewNode = acceptNewNode;
        }
    }
}