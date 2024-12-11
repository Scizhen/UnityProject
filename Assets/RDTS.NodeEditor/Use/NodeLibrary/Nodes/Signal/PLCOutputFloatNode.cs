using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RDTS;

namespace RDTS.NodeEditor
{
    [System.Serializable, NodeMenuItem("Signal/ValueOutputFloat")]
    public class PLCOutputFloatNode : BaseNode
    {
        public override string name => "ValueOutputFloat";
        public override bool isRenamable => true;//¿ÉÖØÃüÃû

        [Input("In", true)]
        public float inputs = 0;

        [Output("Out")]
        public float output;

        [BindObject]
        public ValueOutputFloat signal;


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

