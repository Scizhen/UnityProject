using UnityEngine.UIElements;
using UnityEngine;
using System;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// Element that overlays the graph like the blackboard 
    /// [�����л�]��ڰ�һ������ͼ�ε�Ԫ��
    /// </summary>
    [System.Serializable]
    public class PinnedElement
    {
        public static readonly Vector2 defaultSize = new Vector2(150, 200);//Ĭ�ϳߴ�

        public Rect position = new Rect(Vector2.zero, defaultSize);//λ��
        public bool opened = true;
        public SerializableType editorType;

        public PinnedElement(Type editorType)
        {
            this.editorType = new SerializableType(editorType);
        }
    }
}