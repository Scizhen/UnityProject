using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace RDTS.NodeEditor
{
    [System.Serializable, NodeMenuItem("Common/GameObject")]
    public class GameObjectNode : BaseNode, ICreateNodeFrom<GameObject>
    {
        public override string name => "GameObject";

        [Input(name = "In")]
        public GameObject input;

        [Output(name = "Out"), SerializeField]
        public GameObject output;


        public bool InitializeNodeFromObject(GameObject value)
        {
            output = value;
            return true;
        }
    }

}