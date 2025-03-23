using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RDTS.NodeEditor
{
    [System.Serializable, NodeMenuItem("DataElement/Float")]
    public class FloatNode : BaseNode
    {
        [Output("Out")]
        public float output;

        [Input("In")]
        public float input;

        public override string name => "Float";

        protected override void Process() => output = input;
    }

}