//*************************************************************************
//Thanks for the code reference game4automation provides.                 *
//                                                                        *
//*************************************************************************
using System.Collections.Generic;
using UnityEngine;

namespace RDTS
{
    /// <summary>
    /// （play mode 下）在RuntimeInspector中显示自定义的Inspector
    /// </summary>
    public class Inspector : MonoBehaviour
    {
        public bool ShowInInspector = true;
        public bool ShowOnlyMarkedAttributes = false;
        public bool ShowOnlyDefinedComponents=true;
        public bool HideDefinedElements=false;
    
        public string HierarchyName;
        public string ComponentName;

        public List<Component> Elements;
        private RDTSController RDTSController;
    
        // Start is called before the first frame update
        void Start()
        {
            var rdts = GameObject.Find("RDTS-Controller");
            if (rdts != null)
                RDTSController = rdts.GetComponent<RDTSController>();

            if (ShowInInspector)
                InitInspector();
        }

        void InitInspector()
        {

            RDTSController.InspectorController.Add(this);
        }
  
    }


}

