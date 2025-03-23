using RDTS.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RDTS.Interface
{
    /// <summary>
    /// 用于识别接口类的基类
    /// </summary>
    public class BaseInterface : RDTSBehavior
    {
        [RDTS.Utility.ReadOnly] public bool ConnectionStatus = false;//是否连接

    }

    public enum Status
    {
        Active,
        NotActive
    }

    public enum PLCType
    {
        CPU1500,
        CPU1200
    }

    public enum InteractType
    {
        Single,
        Batch
    }

    public enum SignalType
    {
        InputBool,
        InputInt,
        InputReal,
        OutputBool,
        OutputInt,
        OutputReal
    }


    [System.Serializable]
    public class PLCSignalBlock
    {
        public string blockName = "";
        public InteractType interactType = InteractType.Single;//单个信号处理/批量处理
        public string startAddre;//块的起始地址
        public int AddreOffset;//起始地址偏移量
        public bool isExecute = true;//是否进行信号交互

    }

    [System.Serializable]
    public class InputBoolSignal : PLCSignalBlock
    {
        public List<ValueInputBool> signals;
    }
    [System.Serializable]
    public class InputIntSignal : PLCSignalBlock
    {
        public List<ValueInputInt> signals;
    }
    [System.Serializable]
    public class InputFloatSignal : PLCSignalBlock
    {
        public List<ValueInputFloat> signals;
    }
    [System.Serializable]
    public class OutputBoolSignal : PLCSignalBlock
    {
        public List<ValueOutputBool> signals;
    }
    [System.Serializable]
    public class OutputIntSignal : PLCSignalBlock
    {
        public List<ValueOutputInt> signals;
    }
    [System.Serializable]
    public class OutputFloatSignal : PLCSignalBlock
    {
        public List<ValueOutputFloat> signals;
    }


    /// <summary>机械臂的Modbus数据结构</summary>
    [System.Serializable]
    public class RobotArmJoints
    {
        public string RobotName;
        public int startAddre;//起始地址
        public int AddreOffset;//起始地址偏移量
        public bool isExecute = true;//是否执行数据交互
        public float lerpSpeed = 1f;//插值
        public List<Drive> Axis = new List<Drive>();
        [RDTS.Utility.ReadOnly]
        public List<float> Values = new List<float>();

        [HideInInspector] public int length => Axis.Count;

    }




}
