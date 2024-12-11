using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace RDTS
{
    /// <summary>
    /// 工作效应器基类
    /// </summary>
    public class BaseWorkEffector : BaseEffector
    {

        [BoxGroup("Signal Input")] public ValueMiddleBool SignalStopWork; //停止效应器工作，为true则不会进行任何调整操作


        [BoxGroup("Signal Output")] public ValueMiddleBool SignalWorkStart; //加工开始信号
        [BoxGroup("Signal Output")] public ValueMiddleBool SignalWorkEnd; //加工结束信号

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// 根据信号是否停止效应器的工作，返回true为停止，false为继续进行
        /// </summary>
        /// <returns></returns>
        protected bool StopEffectorWorkBySignal()
        {

            if (this.SignalStopWork != null && this.SignalStopWork.Value == true)
            {
                effectorStatus = status[1];
                return true;
            }
            effectorStatus = status[0];
            return false;
        }


        /// <summary>
        /// 设置工作开始信号
        /// </summary>
        /// <param name="value"></param>
        protected void SetSignalWorkStart(bool value)
        {
            if (SignalWorkStart != null)
                SignalWorkStart.Value = value;
        }


        /// <summary>
        /// 设置工作结束/完成信号
        /// </summary>
        /// <param name="value"></param>
        protected void SetSignalWorkEnd(bool value)
        {
            if (SignalWorkStart != null)
                SignalWorkEnd.Value = value;
        }


        


    }

}