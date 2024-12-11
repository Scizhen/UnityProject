using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace RDTS.NodeEditor
{
    [System.Serializable, NodeMenuItem("DataElement/Color")]
    public class ColorNode : BaseNode
    {
        [Output(name = "Color"), SerializeField]
        new public Color color;

        public override string name => "Color";
    }
}

