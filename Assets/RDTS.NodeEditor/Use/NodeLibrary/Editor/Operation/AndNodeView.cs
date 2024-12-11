using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace RDTS.NodeEditor
{
    [NodeCustomEditor(typeof(AndNode))]
    public class AndNodeView : BaseNodeView
    {
        public override void Enable()
        {
            var andNode = nodeTarget as AndNode;

            TextField boolField = new TextField
            {
                value = andNode.result.ToString()
            };

            nodeTarget.onProcessed += () => boolField.value = andNode.result.ToString();

            controlsContainer.Add(boolField);
        }
    }

}
