using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RDTS.NodeEditor
{
    [System.Serializable, NodeMenuItem("Operation/Or")]
    public class OrNode : BaseNode
    {
        public override string name => "Or£ºX|Y";

        [Input(name = "In X")]
        public bool inX;

        [Input(name = "In Y")]
        public bool inY;

        [Output(name = "Out")]
        public bool result;

        protected override void Process()
        {
            result = inX | inY;
        }

    }

}
