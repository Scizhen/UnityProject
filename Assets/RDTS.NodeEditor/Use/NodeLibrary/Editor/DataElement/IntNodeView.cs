using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace RDTS.NodeEditor
{
    [NodeCustomEditor(typeof(IntNode))]
    public class IntNodeView : BaseNodeView
    {
        public override void Enable()
        {
            var intNode = nodeTarget as IntNode;

            IntegerField intField = new IntegerField
            {
                value = intNode.input
            };

            intNode.onProcessed += () => intField.value = intNode.input;

            intField.RegisterValueChangedCallback((v) =>
            {
                owner.RegisterCompleteObjectUndo("Updated intNode input");
                intNode.input = (int)v.newValue;
            });

            controlsContainer.Add(intField);//Ìí¼ÓÔªËØ
        }
    }
}

