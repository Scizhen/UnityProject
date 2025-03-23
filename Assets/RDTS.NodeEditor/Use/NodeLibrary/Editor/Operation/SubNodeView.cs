using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace RDTS.NodeEditor
{
    [NodeCustomEditor(typeof(SubNode))]
    public class SubNodeView : BaseNodeView
    {
        public override void Enable()
        {
            var floatNode = nodeTarget as SubNode;

            DoubleField floatField = new DoubleField
            {
                value = floatNode.output
            };

            // Update the UI value after each processing
            nodeTarget.onProcessed += () => floatField.value = floatNode.output;

            controlsContainer.Add(floatField);
        }
    }

}
