using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace RDTS.NodeEditor
{
    [System.Serializable, NodeMenuItem("Operation/Add")]
    public class MultiAddNode : BaseNode
    {
        public override string name => "Add"; 

        [Input]
        public IEnumerable<float> inputs = null;

        [Output]
        public float output;


        protected override void Process()
        {
            output = 0;

            if (inputs == null)
                return;

            foreach (float input in inputs)//将输入数据相加
                output += input;

        }

        [CustomPortBehavior(nameof(inputs))]
        IEnumerable<PortData> GetPortsForInputs(List<SerializableEdge> edges)
        {
            yield return new PortData { displayName = "In ", displayType = typeof(float), acceptMultipleEdges = true };
        }

        [CustomPortInput(nameof(inputs), typeof(float), allowCast = true)]
        public void GetInputs(List<SerializableEdge> edges)
        {
            inputs = edges.Select(e => (float)e.passThroughBuffer);
        }
    }

}
