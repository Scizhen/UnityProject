using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace RDTS.NodeEditor
{
    /// <summary>
    /// �ȿ��Ե���string����ַ�����Ҳ���Ե���ǰһ�ڵ�������ݵ���ʾ��
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
