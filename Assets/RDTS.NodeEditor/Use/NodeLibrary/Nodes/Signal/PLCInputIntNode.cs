using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RDTS;

namespace RDTS.NodeEditor
{
    [System.Serializable, NodeMenuItem("Signal/ValueInputInt")]
    public class PLCInputIntNode : BaseNode
    {
        public override string name => "ValueInputInt";
        public override bool isRenamable => true;//¿ÉÖØÃüÃû

        [Input("In", true)]
        public int inputs = 0;

        [Output("Out")]
        public int output;

        [BindObject]
        public ValueInputInt signal;


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

