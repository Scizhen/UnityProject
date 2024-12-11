using UnityEngine;
using System.Collections.Generic;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// Data container for the StackNode views StackNode 堆栈节点的数据容器。
    ///[可序列化]包含：位置，标题，是否接受拖放，是否接受空格键创建的节点
    /// </summary>
    [System.Serializable]
    public class BaseStackNode
    {
        public Vector2 position;//位置
        public string title = "New Stack";//标题

        /// <summary>
        /// Is the stack accept drag and dropped nodes  是否接受拖放节点
        /// </summary>
        public bool acceptDrop;

        /// <summary>
        /// Is the stack accepting node created by pressing space over the stack node
        /// 是否接受在此堆栈节点上按空格键创建的节点
        /// </summary>
        public bool acceptNewNode;

        /// <summary>
        /// List of node GUID that are in the stack 在堆栈节点中存放的节点的GUID
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <returns></returns>
        public List<string> nodeGUIDs = new List<string>();

        /// <summary>构造函数：设置位置，标题，是否接受拖放，是否接受空格键创建的节点</summary>
        public BaseStackNode(Vector2 position, string title = "Stack", bool acceptDrop = true, bool acceptNewNode = true)
        {
            this.position = position;
            this.title = title;
            this.acceptDrop = acceptDrop;
            this.acceptNewNode = acceptNewNode;
        }
    }
}