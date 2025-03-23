using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;


namespace RDTS.NodeEditor
{
    [NodeCustomEditor(typeof(PLCOutputBoolNode))]
    public class PLCOutputBoolNodeView : BaseNodeView
    {

        public override void Enable()
        {
           
            DrawDefaultInspector();

        }
    }


}


