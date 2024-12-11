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
    /// ת�������Դ���ײ�м�⣬��һ����⵽�Ķ���ת�����µĶ��������ڼӹ�����װ����װ�ȶ�����̬�仯�ĳ���
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class Converter : BaseWorkEffector
    {
        public ObjectPoolUtility objectPoolBeforeConverter;//������Ҫ���ն���Ķ����
        public ObjectPoolUtility objectPoolAfterConverter;//������Ҫ��������Ķ���� 

        [BoxGroup("Settings")] public bool isConvert = false;//�Ƿ�������
        [BoxGroup("Settings")] public float convertDelay = 0f;//ת�����ӳ�ʱ��

        [BoxGroup("Signal Input")] public ValueMiddleBool SignalStartConvert;//���ܿ�ʼת���ź�
        [BoxGroup("Signal Input")] [ReadOnly] public bool _lastSignalStartConvert = false;

        /* �����¼� */
        [Foldout("Events")] public EventEffect EvenConvertStart;
        [Foldout("Events")] public EventEffect EvenConvertEnd;

        private Coroutine convertCoroutine;///�ñ����洢Э����Ϣ�����Թر�Э��

        // Start is called before the first frame update
        void Start()
        {
            //���ù����ź�
            SetSignalWorkStart(false);
            SetSignalWorkEnd(false);
        }

        // Update is called once per frame
        void Update()
        {
            if (StopEffectorWorkBySignal())
                return;

            //�����źŻ��շ�ʽ�������������ӳ�
            if (SignalStartConvert != null && SignalStartConvert.Value && !_lastSignalStartConvert)//�ź�ֵ��falseתΪtrueʱ
            {
                //���ù�����ʼ�ź�
                SetSignalWorkStart(true);
                SetSignalWorkEnd(false);

                //���б��еĵ�һ������ת�����¶���
                if(EffectObjects.Count > 0)
                    ConvertToAnotherObject(EffectObjects[0], convertDelay);

                EvenConvertStart.Invoke();
            }

            if (SignalStartConvert != null) _lastSignalStartConvert = SignalStartConvert.Value;

        }

        void Reset()
        {
            AddDefaultTagAndLayer();//���Ĭ�ϵ�����Tag��Layer
        }


        private void OnTriggerEnter(Collider other)
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

            if (isConvert)//��ʼת���ı�־λ�Ƿ��
            {
                //����⵽�Ķ���ת�����¶���
                ConvertToAnotherObject(go, convertDelay);
            }
             
           

        }


        private void OnTriggerExit(Collider other)
        {
            if (StopEffectorWorkBySignal())
                return;


        }



        void ConvertToAnotherObject(GameObject go, float delay = 0f)
        {
            convertCoroutine = StartCoroutine(DelayRecycleObject(objectPoolBeforeConverter, go, delay));///����Э��

            //���ù�����ʼ�ź�
            SetSignalWorkStart(true);
            SetSignalWorkEnd(false);

            EvenConvertStart.Invoke();
        }


        protected IEnumerator DelayRecycleObject(ObjectPoolUtility objectPoolUtility, GameObject go, float delay = 0)
        {
            yield return new WaitForSeconds(delay);//�ӳ�ʱ��

            if (objectPoolUtility != null)
            {
                objectPoolUtility.RecycleGivenPoolObject(go);

                //�Ƴ����������Ӧ�Ķ���
                if (EffectObjects.Contains(go))
                {
                    EffectObjects.Remove(go);//���б��Ƴ�
                }

            }

            //�����¶���
            RequestNewObject(objectPoolAfterConverter);
            //���ù�������ź�
            SetSignalWorkStart(false);
            SetSignalWorkEnd(true);

            EvenConvertEnd.Invoke();
            StopCoroutine(convertCoroutine);///�ر�Э��
        }



        /// <summary>
        /// ����һ���µĶ���
        /// </summary>
        /// <param name="objectPoolUtility"></param>
        protected void RequestNewObject(ObjectPoolUtility objectPoolUtility)
        {
            objectPoolUtility.RequestOnePoolObject();
        }


    }



}