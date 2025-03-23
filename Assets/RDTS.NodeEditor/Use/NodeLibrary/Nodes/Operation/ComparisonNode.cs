using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

namespace RDTS.NodeEditor
{
    [System.Serializable, NodeMenuItem("Operation/Comparison")]
    public class ComparisonNode : BaseNode
    {
        [Input(name = "In X")]
        public float inX;

        [Input(name = "In Y")]
        public float inY;

        [Output(name = "Out")]
        public bool compared;

        public CompareFunction compareFunction = CompareFunction.LessEqual;

        public override string name => "Comparison";

        protected override void Process()
        {
            switch (compareFunction)
            {
                default:
                case CompareFunction.Disabled:
                case CompareFunction.Never: compared = false; break;
                case CompareFunction.Always: compared = true; break;
                case CompareFunction.Equal: compared = inX == inY; break;
                case CompareFunction.Greater: compared = inX > inY; break;
                case CompareFunction.GreaterEqual: compared = inX >= inY; break;
                case CompareFunction.Less: compared = inX < inY; break;
                case CompareFunction.LessEqual: compared = inX <= inY; break;
                case CompareFunction.NotEqual: compared = inX != inY; break;
            }

            ///Debug.Log($"ComparisonNode Result: {compared}");
        }
    }




}



