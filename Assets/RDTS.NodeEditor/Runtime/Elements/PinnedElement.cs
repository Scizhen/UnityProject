using UnityEngine.UIElements;
using UnityEngine;
using System;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// Element that overlays the graph like the blackboard 
    /// [可序列化]像黑板一样覆盖图形的元素
    /// </summary>
    [System.Serializable]
    public class PinnedElement
    {
        public static readonly Vector2 defaultSize = new Vector2(150, 200);//默认尺寸

        public Rect position = new Rect(Vector2.zero, defaultSize);//位置
        public bool opened = true;
        public SerializableType editorType;

        public PinnedElement(Type editorType)
        {
            this.editorType = new SerializableType(editorType);
        }
    }
}