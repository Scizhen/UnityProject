using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace RDTS
{
    /// <summary>
    /// ����ЧӦ������
    /// </summary>
    public class BaseWorkEffector : BaseEffector
    {

        [BoxGroup("Signal Input")] public ValueMiddleBool SignalStopWork; //ֹͣЧӦ��������Ϊtrue�򲻻�����κε�������


        [BoxGroup("Signal Output")] public ValueMiddleBool SignalWorkStart; //�ӹ���ʼ�ź�
        [BoxGroup("Signal Output")] public ValueMiddleBool SignalWorkEnd; //�ӹ������ź�

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// �����ź��Ƿ�ֹͣЧӦ���Ĺ���������trueΪֹͣ��falseΪ��������
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
        /// ���ù�����ʼ�ź�
        /// </summary>
        /// <param name="value"></param>
        protected void SetSignalWorkStart(bool value)
        {
            if (SignalWorkStart != null)
                SignalWorkStart.Value = value;
        }


        /// <summary>
        /// ���ù�������/����ź�
        /// </summary>
        /// <param name="value"></param>
        protected void SetSignalWorkEnd(bool value)
        {
            if (SignalWorkStart != null)
                SignalWorkEnd.Value = value;
        }


        


    }

}