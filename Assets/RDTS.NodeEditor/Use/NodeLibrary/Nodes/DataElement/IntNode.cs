using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RDTS.NodeEditor
{
    [System.Serializable, NodeMenuItem("DataElement/Int")]
    public class IntNode : BaseNode
    {
        [Output("Out")]
        public int output;

        [Input("In")]
        public int input;

        public override string name => "Int";

        protected override void Process() => output = input;
    }

}