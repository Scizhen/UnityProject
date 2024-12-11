using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace RDTS.NodeEditor
{
    [System.Serializable, NodeMenuItem("Operation/Divide")]
    public class DivideNode : BaseNode
    {
        [Input(name = "X")]
        public float inputX;
        [Input(name = "Y"), ShowAsDrawer]
        public float inputY;

        [Output(name = "Out")]
        public float output;

        public override string name => "Divide£ºX/Y";

        protected override void Process()
        {
            output = inputX / inputY;
        }
    }
}