///********************************************************************************************
///this is part of Robot Digital Twin System.                                                 *
///@Copyright by Shaw.S , 2023                                                                *
///Do not distribute without authorization,thanks!                                            *
///********************************************************************************************
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;


namespace RDTS.Utility
{
    /// <summary>
    /// �ӹ������Դ���ײ�м�⡣��⵽����ʱ����ӹ���ʼ״̬���������뿪��ײ�м�ⷶΧ�����ӹ����״̬��Ӧ�����ڼӹ�����������ͷ�ȡ�
    /// �����ṩ�ӹ���ɵĶ�������������Ҫ�����ⲿ�ź�����ռӹ���ɶ�����б�
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class Processor : BaseWorkEffector
    {
        [NaughtyAttributes.ReadOnly] public List<GameObject> ProcessObjects;//�ӹ���ɶ����б�


        [BoxGroup("Signal Input")]
        public ValueMiddleBool SignalClear; //�б�����źţ����ź�ֵ��falseתΪtrueʱ����ռӹ���ɶ�����б�

        [BoxGroup("Signal Output")] public ValueMiddleInt SignalProcessNumber; //�ӹ����������ź�

        /* �����¼� */
        [Foldout("Events")] public EventEffect EvenProcessStart;
        [Foldout("Events")] public EventEffect EvenProcessEnd;



        private BoxCollider _boxCollider;//�˽ű����ڵ���ײ��
        private bool _lastSignalClear = false;

        // Start is called before the first frame update
        void Start()
        {
            //������ײ��Ϊ������ʽ
            _boxCollider = GetComponent<BoxCollider>();
            _boxCollider.isTrigger = true;

            //���ù����ź�
            SetSignalWorkStart(false);
            SetSignalWorkEnd(false);
        }

        // Update is called once per frame
        void Update()
        {
            if (StopEffectorWorkBySignal())
                return;

            //�����ⲿ�źţ���ռӹ���ɶ����б�
            if (SignalClear != null && SignalClear.Value && !_lastSignalClear)//�ź�ֵ��falseתΪtrueʱ
            {
                ProcessObjects.Clear();
            }

            if(SignalClear != null) _lastSignalClear = SignalClear.Value;
            if (SignalProcessNumber != null) SignalProcessNumber.Value = ProcessObjects.Count;//����ӹ���ɶ�������
        }


        void Reset()
        {
            AddDefaultTagAndLayer();//���Ĭ�ϵ�����Tag��Layer
        }




        protected void OnTriggerEnter(Collider other)
        {
            if (StopEffectorWorkBySignal())
                return;

            GameObject go = other.gameObject;
            //�ж���ײ�����Ķ����Ƿ����Tag��Layer������
            if (!SusGameObjectLimitTagAndlayer(go))
                return;

            if (!EffectObjects.Contains(go))
            {
                EffectObjects.Add(go);//���뵽�б�
            }


            //���ù�����ʼ�ź�
            SetSignalWorkStart(true);
            SetSignalWorkEnd(false);


            EvenProcessStart.Invoke();

        }


        protected void OnTriggerExit(Collider other)
        {
            if (StopEffectorWorkBySignal())
                return;

            GameObject go = other.gameObject;

            if (EffectObjects.Contains(go))
            {
                EffectObjects.Remove(go);//���б��Ƴ�
                if(!ProcessObjects.Contains(go))
                    ProcessObjects.Add(go);//����ӹ���ɶ����б�
            }

            //���ù�������ź�
            SetSignalWorkStart(false);
            SetSignalWorkEnd(true);

            EvenProcessEnd.Invoke();

        }







    }


}
