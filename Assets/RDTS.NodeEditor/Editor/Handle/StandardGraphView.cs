using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace RDTS.NodeEditor
{


    public class StandardGraphView : BaseGraphView
    {
        
        public StandardGraphView(EditorWindow window) : base(window)
        {

        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

        }


    }

}
