using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace RDTS.NodeEditor
{
    [System.Serializable, NodeMenuItem("DataElement/Vector3")]
    public class Vector3Node : BaseNode
    {
        [Output(name = "Out")]
        public Vector3 output;

        [Input(name = "In"), SerializeField]
        public Vector3 input;

        public override string name => "Vector3";

        protected override void Process()
        {
            output = input;
        }
    }
}
