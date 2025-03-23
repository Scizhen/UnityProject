using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// ���Nodes��Groups��Edges��JsonElement�б�
    /// </summary>
	[System.Serializable]
    public class CopyPasteHelper
    {
        public List<JsonElement> copiedNodes = new List<JsonElement>();

        public List<JsonElement> copiedGroups = new List<JsonElement>();

        public List<JsonElement> copiedEdges = new List<JsonElement>();
    }
}