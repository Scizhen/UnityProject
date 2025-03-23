using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace RDTS.NodeEditor
{
    [System.Serializable, NodeMenuItem("Common/Prefab")]
    public class PrefabNode : BaseNode
    {
        public override string name => "Prefab";

        [Output(name = "Out"), SerializeField]
        public GameObject output;

        
    }

}