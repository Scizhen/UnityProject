using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RDTS;

namespace RDTS.NodeEditor
{
    [System.Serializable, NodeMenuItem("Signal/ValueOutputInt")]
    public class PLCOutputIntNode : BaseNode
    {
        public override string name => "ValueOutputInt";
        public override bool isRenamable => true;//¿ÉÖØÃüÃû

        [Input("In")]
        public IEnumerable<int> inputs = null;

        [Output("Out")]
        public int output;

        [BindObject]
        public ValueOutputInt signal;


        [Setting("signal")]
        public string Name;


        protected override void Process()
        {
            if (signal != null)
            {
                var list = inputs.ToList();
                if (inputs != null && list.Count>0)
                {
                    signal.Value = list[0];
                }
                output = signal.Value;
            }


        }

        [CustomPortInput(nameof(inputs), typeof(int), allowCast = true)]
        public void GetInputs(List<SerializableEdge> edges)
        {
            inputs = edges.Select(e => (int)e.passThroughBuffer);
        }


    }

}


