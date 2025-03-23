using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace RDTS.NodeEditor
{
    [NodeCustomEditor(typeof(ColorNode))]
    public class ColorNodeView : BaseNodeView
    {
        public override void Enable()
        {
            AddControlField(nameof(ColorNode.color));
            // AddControlField(nameof(ColorNode.color), null, true);
            style.width = 200;
        }
    }
}