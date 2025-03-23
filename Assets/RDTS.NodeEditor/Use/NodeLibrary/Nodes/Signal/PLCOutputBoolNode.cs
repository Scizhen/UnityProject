using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RDTS;

namespace RDTS.NodeEditor
{
    [System.Serializable, NodeMenuItem("Signal/ValueOutputBool")]
    public class PLCOutputBoolNode : BaseNode
    {
        public override string name => "ValueOutputBool";
        public override bool isRenamable => true;//¿ÉÖØÃüÃû

        [Input("In", true)]
        public bool inputs = false;

        [Output("Out")]
        public bool output;

        [BindObject]
        public ValueOutputBool signal;


        [Setting("signal")]
        public string Name;


        protected override void Process()
        {
            if (signal != null)
            {
                signal.Value = inputs;
                output = signal.Value;
            }


        }

      


    }

}
