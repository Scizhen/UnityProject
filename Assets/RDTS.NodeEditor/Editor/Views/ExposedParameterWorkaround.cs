using UnityEngine;
using System;
using System.Collections.Generic;

namespace RDTS.NodeEditor
{
    [Serializable]
    public class ExposedParameterWorkaround : ScriptableObject
    {
        [SerializeReference]
        public List<ExposedParameter> parameters = new List<ExposedParameter>();
        public BaseGraph graph;
    }
}