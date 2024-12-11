using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RDTS;


namespace RDTS.NodeEditor
{
    [System.Serializable, NodeMenuItem("Signal/ValueInputFloat")]
    public class PLCInputFloatNode : BaseNode
    {
        public override string name => "ValueInputFloat";
        public override bool isRenamable => true;//ø…÷ÿ√¸√˚

        [Input("In")]
        public IEnumerable<float> inputs = null;

        [Output("Out")]
        public float output;

        [BindObject]
        public ValueInputFloat signal;


        [Setting("signal")]
        public string Name;


        protected override void Process()
        {
            if (signal != null)
            {
                if (inputs != null)
                {
                    signal.Value = inputs.ToList()[0];
                }
                output = signal.Value;
            }

        

        }

        [CustomPortInput(nameof(inputs), typeof(float), allowCast = true)]
        public void GetInputs(List<SerializableEdge> edges)
        {
            inputs = edges.Select(e => (float)e.passThroughBuffer);
        }


    }

}

