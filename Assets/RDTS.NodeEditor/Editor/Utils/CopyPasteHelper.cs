using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// 针对Nodes、Groups、Edges的JsonElement列表
    /// </summary>
	[System.Serializable]
    public class CopyPasteHelper
    {
        public List<JsonElement> copiedNodes = new List<JsonElement>();

        public List<JsonElement> copiedGroups = new List<JsonElement>();

        public List<JsonElement> copiedEdges = new List<JsonElement>();
    }
}