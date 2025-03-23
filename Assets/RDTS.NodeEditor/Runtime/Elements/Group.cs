using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// Group the selected node when created 创建时对选定节点进行分组。
    /// [可序列化]包含标题、颜色、位置、尺寸，并记录储存节点的GUID
    /// </summary>
    [System.Serializable]
    public class Group
    {
        public string title;
        public Color color = new Color(0, 0, 0, 0.3f);
        public Rect position;
        public Vector2 size;

        /// <summary>
        /// Store the GUIDs of the node in the group  在group中储存节点的GUID
        /// </summary>
        /// <typeparam name="string">GUID of a node</typeparam>
        /// <returns></returns>
        public List<string> innerNodeGUIDs = new List<string>();

        // For serialization loading 用于序列化加载
        public Group() { }

        /// <summary>
        /// Create a new group with a title and a position
        /// 按给定的标题和位置创建group
        /// </summary>
        /// <param name="title"></param>
        /// <param name="position"></param>
        public Group(string title, Vector2 position)
        {
            this.title = title;
            this.position.position = position;
        }

        /// <summary>
        /// Called when the Group is created 当group被创建时调用
        /// </summary>
        public virtual void OnCreated()
        {
            size = new Vector2(400, 200);
            position.size = size;
        }
    }
}