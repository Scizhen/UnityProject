using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace RDTS.NodeEditor
{
    [System.Serializable, NodeMenuItem("Operation/Multiply")]
    public class MultiplyNode : BaseNode
    {
        public override string name => "Multiply£ºX*Y";

        [Input(name = "X")]
        public float inputX;
        [Input(name = "Y"),ShowAsDrawer]
        public float inputY;

        [Output(name = "Out")]
        public float output;

       

        protected override void Process()
        {
            output = inputX * inputY;
        }
    }
}