using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;
using XCharts.Runtime;
using UnityEditorInternal;
namespace VisualSpline
{

    public class GEN_Encoed_result_PSO : GEN_Encoed_result
    {
        public PSO_FunctionTest PSOFuntion;

        [Button("Read PSO Funtion Machines")]
        void ReadPSOFuntionMachines()
        {
            List<GameObject> prefebs = PSOFuntion.prefebs;
            for (int i = 1; i <= prefebs.Count - 1; i++)
            {
                foreach (Transform child in prefebs[i].transform)
                {
                    string childName = child.name;
                    if(childName.Length >= 13)
                    {
                        int machineCount = int.Parse(childName.Substring(12));
                        this.Machines[machineCount - 1].machineName = child.gameObject;
                    }
                }
            }
        }
    }
}

