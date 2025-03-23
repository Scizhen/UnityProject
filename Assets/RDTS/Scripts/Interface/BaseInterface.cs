using RDTS.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RDTS.Interface
{
    /// <summary>
    /// ����ʶ��ӿ���Ļ���
    /// </summary>
    public class BaseInterface : RDTSBehavior
    {
        [RDTS.Utility.ReadOnly] public bool ConnectionStatus = false;//�Ƿ�����

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
        public InteractType interactType = InteractType.Single;//�����źŴ���/��������
        public string startAddre;//�����ʼ��ַ
        public int AddreOffset;//��ʼ��ַƫ����
        public bool isExecute = true;//�Ƿ�����źŽ���

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


    /// <summary>��е�۵�Modbus���ݽṹ</summary>
    [System.Serializable]
    public class RobotArmJoints
    {
        public string RobotName;
        public int startAddre;//��ʼ��ַ
        public int AddreOffset;//��ʼ��ַƫ����
        public bool isExecute = true;//�Ƿ�ִ�����ݽ���
        public float lerpSpeed = 1f;//��ֵ
        public List<Drive> Axis = new List<Drive>();
        [RDTS.Utility.ReadOnly]
        public List<float> Values = new List<float>();

        [HideInInspector] public int length => Axis.Count;

    }




}
