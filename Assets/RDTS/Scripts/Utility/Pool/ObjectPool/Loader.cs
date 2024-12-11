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
    /// װ��������������ЧӦ��ʹ�ã�һ���Ǽ����������������ͨ�������źŶ��б��ж��������µĸ�������������Ҫ��������а��ˡ��̶��ȳ��ϣ�
    /// ������װ�ض����������
    /// </summary>
    public class Loader : BaseWorkEffector
    {
        public Detector detector;//���ڵļ����

        [BoxGroup("Setting")]public int loadNumber = 1;//װ������
        [BoxGroup("Setting")] public GameObject parentLoad;//װ�ظ�������Ϊ����Ĭ�Ͻ��˽ű����صĶ�����Ϊ������
        [BoxGroup("Setting")] public GameObject parentAfterLoad;//װ����ɺ���¸����󣬿���

        [BoxGroup("Signal Input")] public ValueMiddleBool SignalStartLoad; //��ʼװ���ź�
        [BoxGroup("Signal Input")] public ValueMiddleBool SignalEndLoad; //����װ���ź�
        [BoxGroup("Signal Input")] [ReadOnly] public bool _lastSignalStartLoad = false;
        [BoxGroup("Signal Input")] [ReadOnly] public bool _lastSignalEndLoad = false;

        /* �����¼� */
        [Foldout("Events")] public EventEffect EvenLoadStart;
        [Foldout("Events")] public EventEffect EvenLoadEnd;



        // Start is called before the first frame update
        void Start()
        {
            SignalStartLoad.Value = false;
            SignalEndLoad.Value = false;
            _lastSignalStartLoad = false;
            _lastSignalEndLoad = false;

            //���ù����ź�
            SetSignalWorkStart(false);
            SetSignalWorkEnd(false);
        }

        // Update is called once per frame
        void Update()
        {

            if (StopEffectorWorkBySignal())
                return;

            //��ȡҪװ�صĶ���
            GetLoadedObject();

            ///����װ���źţ�����װ��
            if (SignalStartLoad != null && SignalStartLoad.Value && !_lastSignalStartLoad)//�ź�ֵ��falseתΪtrueʱ
            {
                //���ù�����ʼ�ź�
                SetSignalWorkStart(true);
                SetSignalWorkEnd(false);
                LoadObject();
                EvenLoadStart.Invoke();
            }


            ///����ж���źţ�����װ��
            if (SignalEndLoad != null && SignalEndLoad.Value && !_lastSignalEndLoad)//�ź�ֵ��falseתΪtrueʱ
            {
                UnloadObject();

                //���ù�������ź�
                SetSignalWorkStart(false);
                SetSignalWorkEnd(true);

                EvenLoadEnd.Invoke();
            }

            if(SignalStartLoad != null) _lastSignalStartLoad = SignalStartLoad.Value;//��¼��Ϊ��һ��ֵ
            if (SignalEndLoad != null) _lastSignalEndLoad = SignalEndLoad.Value;//��¼��Ϊ��һ��ֵ

        }


        /// <summary>
        /// ��ȡҪ��װ�صĶ���
        /// </summary>
        void GetLoadedObject()
        {
            if (detector == null || detector.EffectObjects == null || detector.EffectObjects.Count == 0)
                return;

            if (detector.EffectObjects.Count < loadNumber)
                return;

            EffectObjects = detector.EffectObjects.GetRange(0, loadNumber);//��ȡָ��������װ�ض���
        }


        /// <summary>��LoadObjects�б��еĶ������װ�� </summary>
        void LoadObject()
        {
            //���ô˽ű����صĶ���Ϊ�����󣨼����ض���װ�����˶����ϣ�
            EffectObjects.ForEach(lobj => {
                lobj.transform.SetParent((parentLoad != null)? parentLoad.transform : this.gameObject.transform);
            });
        }
        /// <summary>��LoadObjects�б��еĶ������ж��</summary>
        void UnloadObject()
        {
            EffectObjects.ForEach(lobj => {
                lobj.transform.SetParent((parentAfterLoad != null) ? parentAfterLoad.transform : null);
            });

            EffectObjects.Clear();//���װ���б�
        }




    }




}
