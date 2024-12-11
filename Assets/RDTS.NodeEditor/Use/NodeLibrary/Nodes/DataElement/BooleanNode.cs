using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RDTS.NodeEditor
{
    [System.Serializable, NodeMenuItem("DataElement/Bool")]
    public class BooleanNode : BaseNode
    {
        public override string name => "Boolean";

        [Input("In")]
        public bool input = false;

        [Output("Out")]
        public bool output;

        //是否需要反转输入的值
        public BooleanFunc choose = BooleanFunc.Inverse;


        protected override void Process()
        {
            switch (choose)
            {
                default:
                case BooleanFunc.Inverse: output = !input; break;
                case BooleanFunc.Equal: output = input; break;
                
            }

           /// Debug.Log($"BooleanNode Result: {output}");
        }
    }

    public enum BooleanFunc
    {
        Inverse,
        Equal
    }

}
