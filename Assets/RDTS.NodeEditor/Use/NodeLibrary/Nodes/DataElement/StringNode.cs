using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace RDTS.NodeEditor
{
    /// <summary>
    /// 既可以当作string输出字符串，也可以当作前一节点输出数据的显示框
    /// </summary>
    [System.Serializable, NodeMenuItem("DataElement/String")]
    public class StringNode : BaseNode
    {
        [Input(name = "In")]
        public object input;

        [Output(name = "Out"), SerializeField]
        public string output;


        public override string name => "String & Print";

        protected override void Process()
        {
            if(input != null)
                output = input?.ToString();
        }

    }
}
