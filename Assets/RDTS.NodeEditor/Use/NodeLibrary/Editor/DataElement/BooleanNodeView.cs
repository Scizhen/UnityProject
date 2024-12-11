using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace RDTS.NodeEditor
{
    [NodeCustomEditor(typeof(BooleanNode))]
    public class BooleanNodeView : BaseNodeView
    {
        public override void Enable()
        {
            var booleanNode = nodeTarget as BooleanNode;

            TextField boolField = new TextField
            {
                value = booleanNode.input.ToString()
            };

            booleanNode.onProcessed += () => boolField.value = booleanNode.input.ToString();

            DrawDefaultInspector();
            controlsContainer.Add(boolField);
        }
    }

   
}

