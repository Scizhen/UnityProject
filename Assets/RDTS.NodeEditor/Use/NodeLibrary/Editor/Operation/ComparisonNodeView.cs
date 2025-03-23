using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace RDTS.NodeEditor
{
    [NodeCustomEditor(typeof(ComparisonNode))]
    public class ComparisonNodeView : BaseNodeView
    {
        public override void Enable()
        {
            ComparisonNode comparisonNode = nodeTarget as ComparisonNode;
            DrawDefaultInspector();

            var inputX = new FloatField("In X") { value = comparisonNode.inX };
            var inputY = new FloatField("In Y") { value = comparisonNode.inY };
            ///RegisterValueChangedCallback:注册此回调可在值更改时接收 ChangeEvent<T>
            inputX.RegisterValueChangedCallback(v =>
            {
                owner.RegisterCompleteObjectUndo("Change InX value");
                comparisonNode.inX = v.newValue;
            });
            inputY.RegisterValueChangedCallback(v =>
            {
                owner.RegisterCompleteObjectUndo("Change InY value");
                comparisonNode.inY = v.newValue;
            });

            nodeTarget.onAfterEdgeConnected += UpdateVisibleFields;
            nodeTarget.onAfterEdgeDisconnected += UpdateVisibleFields;

            UpdateVisibleFields(null);

            void UpdateVisibleFields(SerializableEdge _)
            {
                var inX = nodeTarget.GetPort(nameof(comparisonNode.inX), null);
                var inY = nodeTarget.GetPort(nameof(comparisonNode.inY), null);

                controlsContainer.Add(inputX);
                controlsContainer.Add(inputY);

                if (inX.GetEdges().Count > 0)
                    controlsContainer.Remove(inputX);
                if (inY.GetEdges().Count > 0)
                    controlsContainer.Remove(inputY);
            }
        }
    }

}