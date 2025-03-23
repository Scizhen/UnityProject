///********************************************************************************************
///this is part of Robot Digital Twin System.                                                 *
///@Copyright by Shaw.S                                                                       *
///Thanks for the code reference game4automation provides.                                    *
///Do not distribute without authorization,thanks!                                            *
///********************************************************************************************
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using RDTS;

namespace RDTS.Utility.Editor
{
#if UNITY_EDITOR
    /// <summary>
    /// 实现主菜单栏中“Parallel-RDTS”的菜单项
    /// </summary>
    [InitializeOnLoad]
    public class RDTSToolbar : EditorWindow
    {

        [MenuItem("Parallel-RDTS/Create new RDTS Scene", false, 1)]//创建一个新的场景
        static void CreateNewScene()
        {
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            QM.AddComponentByPath(RDTSPath.path_RDTS_Controller);
        }








        /* Add Competent 添加脚本组件 */

        [MenuItem("Parallel-RDTS/Add Component/Drive", false, 150)]
        static void AddDrive()
        {
            QM.AddScript(typeof(Drive));
        }
        [MenuItem("Parallel-RDTS/Add Component/Drive Behaviour/Drive_Cylinder", false, 150)]
        static void AddDriveCylinder()
        {
            QM.AddScript(typeof(Drive_Cylinder));
        }
        [MenuItem("Parallel-RDTS/Add Component/Drive Behaviour/Drive_DestinationMotor", false, 150)]
        static void AddDriveDestinationMotor()
        {
            QM.AddScript(typeof(Drive_DestinationMotor));
        }
        [MenuItem("Parallel-RDTS/Add Component/Drive Behaviour/Drive_ErraticPosition", false, 150)]
        static void AddDriveErraticPosition()
        {
            QM.AddScript(typeof(Drive_ErraticPosition));
        }
        [MenuItem("Parallel-RDTS/Add Component/Drive Behaviour/Drive_Gear", false, 150)]
        static void AddDriveGear()
        {
            QM.AddScript(typeof(Drive_Gear));
        }
        [MenuItem("Parallel-RDTS/Add Component/Drive Behaviour/Drive_Simple", false, 150)]
        static void AddDriveSimple()
        {
            QM.AddScript(typeof(Drive_Simple));
        }
        [MenuItem("Parallel-RDTS/Add Component/Drive Behaviour/Drive_Speed", false, 150)]
        static void AddDriveSpeed()
        {
            QM.AddScript(typeof(Drive_Speed));
        }
        [MenuItem("Parallel-RDTS/Add Component/Drive Behaviour/Sensor_Standard", false, 150)]
        static void AddSensorStandard()
        {
            QM.AddScript(typeof(Sensor_Standard));
        }

        [MenuItem("Parallel-RDTS/Add Component/Grip", false, 150)]
        static void AddGrip()
        {
            QM.AddScript(typeof(Grip));
        }

        [MenuItem("Parallel-RDTS/Add Component/MU", false, 150)]
        static void AddMU()
        {
            QM.AddScript(typeof(MU)); 
        }

        [MenuItem("Parallel-RDTS/Add Component/RobotArm", false, 150)]
        static void AddRobotArm()
        {
            QM.AddScript(typeof(RobotArm));
        }

        [MenuItem("Parallel-RDTS/Add Component/Sensor", false, 150)]
        static void AddSensor()
        {
            QM.AddScript(typeof(Sensor)); 
        }

        [MenuItem("Parallel-RDTS/Add Component/Source", false, 150)]
        static void AddSourcer()
        {
            QM.AddScript(typeof(Source));
        }

        [MenuItem("Parallel-RDTS/Add Component/Sink", false, 150)]
        static void AddSink()
        {
            QM.AddScript(typeof(Sink));
        }


        [MenuItem("Parallel-RDTS/Add Component/TransportSurface", false, 150)]
        static void AddTransportSurfacek()
        {
            QM.AddScript(typeof(TransportSurface));
        }

        [MenuItem("Parallel-RDTS/Add Component/Value Object/PLC Input Bool", false, 150)]
        static void AddPLCInputBool()
        {
            QM.AddScript(typeof(ValueInputBool));
        }

        [MenuItem("Parallel-RDTS/Add Component/Value Object/PLC Input Float", false, 150)]
        static void AddPLCInputFloat()
        {
            QM.AddScript(typeof(ValueInputFloat));
        }

        [MenuItem("Parallel-RDTS/Add Component/Value Object/PLC Input Int", false, 150)]
        static void AddPLCInputInt()
        {
            QM.AddScript(typeof(ValueInputInt));
        }

        [MenuItem("Parallel-RDTS/Add Component/Value Object/PLC Output Bool", false, 150)]
        static void AddPLCOutputBool()
        {
            QM.AddScript(typeof(ValueOutputBool));
        }

        [MenuItem("Parallel-RDTS/Add Component/Value Object/PLC Output Float", false, 150)]
        static void AddPLCOutputFloat()
        {
            QM.AddScript(typeof(ValueOutputFloat));
        }

        [MenuItem("Parallel-RDTS/Add Component/Value Object/PLC Output Int", false, 150)]
        static void AddPLCOutputInt()
        {
            QM.AddScript(typeof(ValueOutputInt));
        }

        /* Add Object 添加预制体 */
        [MenuItem("Parallel-RDTS/Add Object/RDTS-Controller", false, 151)]
        static void AddRDTSController()
        {
            QM.AddComponentByPath(RDTSPath.path_RDTS_Controller);
        }





        /* Open Demo Scene 打开案例场景 */
        [MenuItem("Parallel-RDTS/Open Demo Scene/DemoGrip", false, 800)]
        static void OpenDemoRobotGrip()
        {
            EditorSceneManager.OpenScene("Assets/RDTS/Scenes/DemoScenes/DemoGrip.unity");
        }
        [MenuItem("Parallel-RDTS/Open Demo Scene/DemoBasketLine", false, 800)]
        static void OpenDemoBasketLine()
        {
            EditorSceneManager.OpenScene("Assets/RDTS/Scenes/DemoScenes/DemoBasketLine.unity");
        }





        /* Documentation 文档 */
        [MenuItem("Parallel-RDTS/Documentation ", false, 801)]
        static void OpenDocumentation()
        {
            //Application.OpenURL("https://game4automation.com/documentation/current/index.html");
            EditorUtility.DisplayDialog("Documentation",
                    "文档还未完成!",
                    "OK");
        }


        
    }
#endif
}
