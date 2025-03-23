using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;


namespace RDTS.NodeEditor
{

    public class GraphAssetCallbacks
    {
        [MenuItem("Assets/Create/Parallel-RDTS/GraphWindowAsset", false, 10)]
        public static void CreateGraphPorcessor()
        {
            var graph = ScriptableObject.CreateInstance<BaseGraph>();
            ProjectWindowUtil.CreateAsset(graph, "GraphWindow.asset");
        }

        [OnOpenAsset(0)]//双击.asset文件打开窗口
        public static bool OnBaseGraphOpened(int instanceID, int line)
        {
            // Debug.Log($"{instanceID} and {line}");

            var asset = EditorUtility.InstanceIDToObject(instanceID) as BaseGraph;

            // Debug.Log($"双击{asset.name}.asset打开窗口");

            if (asset != null )//&& AssetDatabase.GetAssetPath(asset).Contains("GraphAsset"))//在GraphAsset文件夹路径下
            {
                EditorWindow.GetWindow<StandardGraphWindow>().InitializeGraph(asset as BaseGraph);
                return true;
            }
            return false;


        }
    }
}
