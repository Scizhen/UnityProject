using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;



namespace RDTS.RobotSimulationProgramming
{
    public class RSP_ProgramAssetCallbacks
    {
        [MenuItem("Assets/Create/Parallel-RDTS/Create RSP_RobotProgramAsset", false, 10)]
        public static void CreateRobotProgram()
        {
            var program = ScriptableObject.CreateInstance<RSP_RobotProgram>();
            ProjectWindowUtil.CreateAsset(program, "RobotProgram.asset");
        }

        [OnOpenAsset(0)]//双击.asset文件打开窗口
        public static bool OnRobotProgramOpened(int instanceID, int line)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceID) as RSP_RobotProgram;

            // Debug.Log($"双击{asset.name}.asset打开窗口");

            if (asset != null)//&& AssetDatabase.GetAssetPath(asset).Contains("GraphAsset"))//在GraphAsset文件夹路径下
            {
                EditorWindow.GetWindow<RSP_ProgrammingWindow>().InitializeProgram(asset as RSP_RobotProgram);
                return true;
            }
            return false;


        }
    }



}


