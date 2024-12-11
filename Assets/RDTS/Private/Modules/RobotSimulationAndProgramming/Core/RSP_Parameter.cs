using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RDTS.RobotSimulationProgramming
{
    [Serializable]
    /// <summary>
    /// 参数类
    /// </summary>
    public class RSP_Parameter
    {


    }

    [Serializable]
    /// <summary>
    /// 输入类型参数
    /// </summary>
    public class RSP_InputParameter : RSP_Parameter
    {
        [SerializeField]
        List<ValueInputInt> inputs = new List<ValueInputInt>();
    }

    [Serializable]
    /// <summary>
    /// 输出类型参数
    /// </summary>
    public class RSP_OutputParameter : RSP_Parameter
    {
        [SerializeField]
        List<ValueOutputInt> outputs = new List<ValueOutputInt>();
    }




}
