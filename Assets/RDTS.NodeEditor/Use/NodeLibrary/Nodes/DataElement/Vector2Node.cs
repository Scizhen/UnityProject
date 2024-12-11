using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace RDTS.NodeEditor
{
    [System.Serializable, NodeMenuItem("DataElement/Vector2")]
    public class Vector2Node : BaseNode
    {
        [Output(name = "Out")]
        public Vector2 output;

        [Input(name = "In"), SerializeField]
        public Vector2 input;

        public override string name => "Vector2";

        protected override void Process()
        {
            output = input;
        }
    }
}