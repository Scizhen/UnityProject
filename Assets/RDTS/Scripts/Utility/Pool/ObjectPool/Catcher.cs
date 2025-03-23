using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;


namespace RDTS.Utility
{
    public class Catcher : BaseWorkEffector
    {
        public enum CatchMode
        {
            Grip,
            Suck
        }


        public Detector detector;//��ϵļ����

        [BoxGroup("Settings")] public CatchMode catchMode = CatchMode.Suck;//��׽ģʽ
        [BoxGroup("Settings")] [ReadOnly] public string CatchStatus = "Idle";//��׽״̬
        [BoxGroup("Settings")] public GameObject catchParent;//ץȡʱ�ĸ�����
        [BoxGroup("Settings")] public GameObject newParent;//���ú�ĸ�����
        [BoxGroup("Settings")] [ReadOnly] public int currentEffectObjectsCount = 0;
        [BoxGroup("Settings")][ReadOnly] public int _lastEffectObjectsCount = 0;
        [ShowIf("catchMode", CatchMode.Grip)] [BoxGroup("Settings")] public ValueOutputBool GripperClose;
        [ShowIf("catchMode", CatchMode.Grip)] [BoxGroup("Settings")] public ValueOutputBool GripperOpen;
        [ShowIf("catchMode", CatchMode.Grip)] [BoxGroup("Settings")] public ValueInputBool GripperIsClose;
        [ShowIf("catchMode", CatchMode.Grip)] [BoxGroup("Settings")] public ValueInputBool GripperIsOpen;

        [BoxGroup("Signal Input")] public ValueOutputBool SignalPick; //ץȡ�ź�
        [BoxGroup("Signal Input")] public ValueOutputBool SignalPlace; //�����ź�
        private bool _lastSignalPick = false;
        private bool _lastSignalPlacee = false;

        [BoxGroup("Signal Output")]
        public ValueMiddleInt PickObjCount; //�����ץȡ�����ź�


        /* �����¼� */
        [Foldout("Events")] public EventEffect EvenCatchStart;
        [Foldout("Events")] public EventEffect EvenCatchEnd;


        private bool _lastGripperIsClose = false;
        private bool _lastGripperIsOpen = false;
       

        // Start is called before the first frame update
        void Start()
        {
            CatchStatus = "Idle";//��ʼ״̬Ϊ��
            if(catchMode == CatchMode.Grip)
                MakeGripperOpen();
            if(SignalPick != null)
                _lastSignalPick = SignalPick.Value;
            if (SignalPlace != null)
                _lastSignalPlacee = SignalPlace.Value;

            if (GripperIsClose != null)
                _lastGripperIsClose = GripperIsClose.Value;
            if (GripperIsOpen != null)
                _lastGripperIsOpen = GripperIsOpen.Value;


        }

        // Update is called once per frame
        void Update()
        {

            

        }


        private void FixedUpdate()
        {
            //����Pick�ź�
            if (SignalPick != null && SignalPick.Value && !_lastSignalPick && CatchStatus == "Idle")
            {
                CatchStatus = "Pick";
                //���ù�����ʼ�ź�
                SetSignalWorkStart(true);
                SetSignalWorkEnd(false);
            }
            //����Place�ź�
            if (SignalPlace != null && SignalPlace.Value && !_lastSignalPlacee && CatchStatus == "Picked")
            {
                CatchStatus = "Place";
            }

            //��׽ģʽ
            if (catchMode == CatchMode.Grip)
                GripperControl();
            else
                SuckerControl();

            //��¼�����ź�ֵ
            if (SignalPick != null)
                _lastSignalPick = SignalPick.Value;
            if (SignalPlace != null)
                _lastSignalPlacee = SignalPlace.Value;
            if (GripperIsClose != null)
                _lastGripperIsClose = GripperIsClose.Value;
            if (GripperIsOpen != null)
                _lastGripperIsOpen = GripperIsOpen.Value;


            if (PickObjCount != null)
                PickObjCount.Value = EffectObjects.Count;
            currentEffectObjectsCount = EffectObjects.Count;
        }



        /// <summary>
        /// ���̹��ߵ���ȡ����
        /// </summary>
        void SuckerControl()
        {

            switch (CatchStatus)
            {
                case "Idle":
                    ResetCatcherSignal();
                    break;
                case "Picked":
                    _lastEffectObjectsCount = EffectObjects.Count;
                    break;
                case "Pick":
                    if (EffectObjects.Count > _lastEffectObjectsCount)
                    {
                        CatchStatus = "Picked";
                    }
                    else
                    {
                        CatcherPick();
                    }

                    break;
                case "Place":
                    if (EffectObjects.Count < _lastEffectObjectsCount)
                    {
                        CatchStatus = "Idle";
                        _lastEffectObjectsCount = EffectObjects.Count;

                        //���ù��������ź�
                        SetSignalWorkStart(false);
                        SetSignalWorkEnd(true);
                    }
                    else
                    {
                        CatcherPlace();
                    }

                    break;


            }


        }


        /// <summary>
        /// ��צ���ߵļ�ȡ����
        /// </summary>
        void GripperControl()
        {
          
            
            switch (CatchStatus)
            {
                case "Idle":
                    ResetCatcherSignal();
                    break;
                case "Picked":
                    _lastEffectObjectsCount = EffectObjects.Count;
                    break;
                case "Pick":
                    if(EffectObjects.Count > _lastEffectObjectsCount)//��ǰ�������� > ֮ǰ��������ʱ��˵��ץȡ�ɹ�
                    {
                        CatchStatus = "Picked";
                    }
                    else
                    {
                        MakeGripperClose();
                        //����צ�պϣ�����ץȡ
                        if (GripperIsClose.Value && !_lastGripperIsClose)
                        {
                            CatcherPick();

                        }
                    }

                    
                    break;
                case "Place":
                    if (EffectObjects.Count < _lastEffectObjectsCount)//��ǰ�������� < ֮ǰ��������ʱ��˵�����óɹ�
                    {
                        CatchStatus = "Idle";
                        _lastEffectObjectsCount = EffectObjects.Count;

                        //���ù��������ź�
                        SetSignalWorkStart(false);
                        SetSignalWorkEnd(true);
                    }
                    else
                    {
                        MakeGripperOpen();
                        //����צ�ſ������з���
                        if (GripperIsOpen.Value && !_lastGripperIsOpen)
                        {
                            CatcherPlace();
                        }
                    }

                    break;


            }



        }


        /// <summary>
        /// ��צץȡ����
        /// </summary>
        void CatcherPick()
        {
            var effectObjs = detector.EffectObjects;
            if (effectObjs.Count == 0 || effectObjs == null)
                return;

            effectObjs.ForEach(obj => {
                obj.transform.SetParent((catchParent == null)? this.transform: catchParent.transform);//ץȡ����ĸ�����Ϊ�գ���Ĭ�Ͻ����ش˽ű��Ķ�����Ϊ������
                detector.effectObjPerGo[obj].rigidbody.isKinematic = true;
                if(!EffectObjects.Contains(obj))
                    EffectObjects.Add(obj);
            });



        }
        /// <summary>
        /// ��צ���ö���
        /// </summary>
        void CatcherPlace()
        {
            var length = EffectObjects.Count;
            if (length == 0)
                return;

            for (int i=0; i< length; i++)
            {
                var obj = EffectObjects[i];
                EffectObjects[i].transform.SetParent((newParent == null)? null:newParent.transform);//����ʱ��δ�������µĸ������򽫶��󸸶�����Ϊ��
                if(detector.effectObjPerGo.ContainsKey(obj))
                detector.effectObjPerGo[obj].rigidbody.isKinematic = false;
                if (EffectObjects.Contains(obj))
                    EffectObjects.Remove(obj);
            }
       


        }



        /// <summary>
        /// �ſ���צ
        /// </summary>
        void MakeGripperOpen()
        {
            GripperClose.Value = false;
            GripperOpen.Value = true;
        }
        /// <summary>
        /// �պϼ�צ
        /// </summary>
        void MakeGripperClose()
        {
            GripperClose.Value = true;
            GripperOpen.Value = false;
        }

        /// <summary>
        /// ����ץȡ/�����ź�
        /// </summary>
        void ResetCatcherSignal()
        {
            SignalPick.Value = false;
            SignalPlace.Value = false;
        }


    }



}
