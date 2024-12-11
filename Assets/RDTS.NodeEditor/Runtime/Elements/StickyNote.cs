using System;
using UnityEngine;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// Serializable Sticky node class  “便签”节点
    /// [可序列化]包含：位置(尺寸)，标题，内容
    /// </summary>
    [Serializable]
    public class StickyNote
    {
        public Rect position;//位置
        public string title = "Hello World!";//标题
        public string content = "Description";//内容

        /// <summary>设置标题和位置</summary>
        public StickyNote(string title, Vector2 position)
        {
            this.title = title;
            this.position = new Rect(position.x, position.y, 200, 300);
        }
    }
}