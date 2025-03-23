using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace RDTS.NodeEditor
{
    [NodeCustomEditor(typeof(OrNode))]
    public class OrNodeView : BaseNodeView
    {
        public override void Enable()
        {
            var orNode = nodeTarget as OrNode;

            TextField boolField = new TextField
            {
                value = orNode.result.ToString()
            };

            nodeTarget.onProcessed += () => boolField.value = orNode.result.ToString();

            controlsContainer.Add(boolField);
        }
    }

}
