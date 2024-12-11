using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace RDTS
{
    /// <summary>
    /// 检测效应器基类
    /// </summary>
    public class BaseDetectEffector : BaseEffector
    {

        [BoxGroup("Signal Input")] public ValueMiddleBool SignalStopDetect; //停止效应器检测，为true则不会进行任何操作
        [BoxGroup("Signal Input")] public ValueMiddleBool SignalClear; //清空作用对象列表

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }


        /// <summary>
        /// 接收信号是否停止检测
        /// </summary>
        /// <returns></returns>
        protected bool StopEffectorDetectBySignal()
        {

            if (this.SignalStopDetect != null && this.SignalStopDetect.Value == true)
            {
                effectorStatus = status[1];
                return true;
            }
            effectorStatus = status[0];
            return false;
        }


        /// <summary>
        /// 清空作用对象列表
        /// </summary>
        protected void ClearEffectObjects()
        {
            if(EffectObjects != null)
                EffectObjects.Clear();

        }





    }

}
