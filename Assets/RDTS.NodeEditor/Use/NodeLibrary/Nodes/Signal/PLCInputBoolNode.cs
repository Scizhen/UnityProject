using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RDTS;

namespace RDTS.NodeEditor
{
    [System.Serializable, NodeMenuItem("Signal/ValueInputBool")]
    public class PLCInputBoolNode : BaseNode
    {
        public override string name => "ValueInputBool";
        public override bool isRenamable => true;//ø…÷ÿ√¸√˚

        [Input("In")]
        public IEnumerable<bool> inputs = null;

        [Output("Out")]
        public bool output;

        [BindObject]
        public ValueInputBool signal;


        [Setting("signal")]
        public string Name;


        protected override void Process()
        {
            if (signal != null)
            {
                if(inputs!=null)
                {
                    foreach(bool input in inputs)
                        signal.Value |= input;
                }
                output = signal.Value;
            }


        }

        [CustomPortInput(nameof(inputs), typeof(bool), allowCast = true)]
        public void GetInputs(List<SerializableEdge> edges)
        {
            inputs = edges.Select(e => (bool)e.passThroughBuffer);
        }




    }

}
