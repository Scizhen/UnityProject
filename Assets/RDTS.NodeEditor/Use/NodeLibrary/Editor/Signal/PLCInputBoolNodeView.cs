using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;


namespace RDTS.NodeEditor
{
    [NodeCustomEditor(typeof(PLCInputBoolNode))]
    public class PLCInputBoolNodeView : BaseNodeView
    {

        public override void Enable()
        {

            DrawDefaultInspector();

        }
    }


}


