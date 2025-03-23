using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace RDTS.NodeEditor
{
    [System.Serializable, NodeMenuItem("Operation/Sub")]
    public class SubNode : BaseNode
    {
        public override string name => "Sub£ºX-Y";

        [Input(name = "X")]
        public float inputX;
        [Input(name = "Y")]
        public float inputY;

        [Output(name = "Out")]
        public float output;

       
        protected override void Process()
        {
            output = inputX - inputY;
        }
    }
}
