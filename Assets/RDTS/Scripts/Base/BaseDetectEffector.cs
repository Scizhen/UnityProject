using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace RDTS
{
    /// <summary>
    /// ���ЧӦ������
    /// </summary>
    public class BaseDetectEffector : BaseEffector
    {

        [BoxGroup("Signal Input")] public ValueMiddleBool SignalStopDetect; //ֹͣЧӦ����⣬Ϊtrue�򲻻�����κβ���
        [BoxGroup("Signal Input")] public ValueMiddleBool SignalClear; //������ö����б�

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }


        /// <summary>
        /// �����ź��Ƿ�ֹͣ���
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
        /// ������ö����б�
        /// </summary>
        protected void ClearEffectObjects()
        {
            if(EffectObjects != null)
                EffectObjects.Clear();

        }





    }

}
