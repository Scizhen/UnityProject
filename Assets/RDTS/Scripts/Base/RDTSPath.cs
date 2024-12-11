///********************************************************************************************
///this is part of Robot Digital Twin System.                                                 *
///@Copyright by Shaw.S                                                                       *
///Do not distribute without authorization,thanks!                                            *
///********************************************************************************************
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RDTS
{
    /// <summary>
    /// 存放所需的路径
    /// </summary>
    public static class RDTSPath
    {
        //RDTS-Controller prefab
        public static string path_RDTS_Controller = "Assets/RDTS/Private/Component/RDTS-Controller.prefab";
        //PLC-Interface prefab
        public static string path_PLCInterface = "Assets/RDTS/Private/Component/PLC-Interface.prefab";

        //Data
        public static string path_Data = "Assets/RDTS/Data/";

        //Style
       

        //Material
        public static string path_SensorOccupied = "Materials/Special/SensorOccupiedRed";
        public static string path_SensorNotOccupied = "Materials/Special/SensorNotOccupied";

        //Hierarchy Icons (root)
        public static string path_HierarchyIcos = "Icons/HierarchyIcons/";




    }
}
