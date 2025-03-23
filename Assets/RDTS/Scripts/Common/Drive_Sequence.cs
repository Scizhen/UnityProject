// Game4Automation (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// (c) 2019 in2Sight GmbH - Usage of this source code only allowed based on License conditions see https://game4automation.com/lizenz  

using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace RDTS
{

    //! Defines sequentially movement of drives which can set signals or be started by signals ������������źŻ����ź���������������˳���˶�
    public class Drive_Sequence : BehaviorInterface
    {
        [System.Serializable]
        public class DriveSequence
        {
            public string Description;
            public Value SignalToFalseOnStart;
            public Value WaitForSignal;
            public Drive Drive;
            public float Destination;
            public bool NoWait;
            public float Speed;
            public float WaitAfterStep;
            public Value FinishedSignal;

        }

        public bool StartAtBeginning = true;
        public bool ResetWaitForSignals = true;
        public bool StopAfterEachStep = false;

        [SerializeField]
        public List<DriveSequence> Sequence = new List<DriveSequence>();

        [ReadOnly] public int CurrentStep = -1;
        [ReadOnly] public Drive CurrentDrive;
        [ReadOnly] public float CurrentDestination;
        [ReadOnly] public Value CurrentWaitForSignal;

        private bool waitforsignal = false;
        private bool waitfornextstepbutton = false;
        private bool waitafterstep = false;

        // Start is called before the first frame update
        void StartSequzence()
        {
            CurrentStep = -1;
            NextStep();
        }

        void NextStep()//�ƶ�ǰ׼��
        {
            waitafterstep = false;
            CurrentStep++;
            waitforsignal = false;
            if (CurrentStep > Sequence.Count - 1)//��������������λ
            {
                CurrentStep = 0;
            }

            CurrentDrive = Sequence[CurrentStep].Drive;//��ȡ�ò����Drive

            if (CurrentDrive == null)
                CurrentDrive = this.GetComponent<Drive>();//���ò�����ָ��DriveĬ�ϻ�ȡ�����ڸ�����ϵ�Drive

            CurrentDestination = Sequence[CurrentStep].Destination;//�ò�����Ҫִ�е�λ��

            if (Sequence[CurrentStep].SignalToFalseOnStart != null)
            {
                Sequence[CurrentStep].SignalToFalseOnStart.SetValue(false);//�����ź��������λ
            }

            if (Sequence[CurrentStep].FinishedSignal != null)
            {
                Sequence[CurrentStep].FinishedSignal.SetValue(false);//�����ź��������λ
            }


            if (Sequence[CurrentStep].WaitForSignal != null)
            {
                waitforsignal = true;
                CurrentWaitForSignal = Sequence[CurrentStep].WaitForSignal;//�����ź��������λ
            }
            else
            {
                StartDrive();//��ʼ�˶�
                if (Sequence[CurrentStep].NoWait)
                    StepFinished();
            }


        }

        /// <summary>
        /// ���趨���ٶ�ǰ���趨��Ŀ���
        /// </summary>
        void StartDrive()
        {
            if (Sequence[CurrentStep].Speed != 0)
                CurrentDrive.TargetSpeed = Sequence[CurrentStep].Speed;
            if (!(CurrentDestination == 0 && Sequence[CurrentStep].Speed == 0))
                CurrentDrive.DriveTo(CurrentDestination);
        }

        void Start()
        {
            if (StartAtBeginning)
                StartSequzence();
        }

        [Button("Next Step")]
        void ButtonNextStep()
        {
            if (waitfornextstepbutton)
            {
                NextStep();
                waitfornextstepbutton = false;
            }

        }

        /// <summary>
        /// ��ǰ������ɣ�������һ�������Զ����к��ֶ���ť
        /// </summary>
        void StepFinished()
        {
            if (StopAfterEachStep != true)
            {
                NextStep();
            }
            else
                waitfornextstepbutton = true;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (CurrentStep > -1)
            {
                if (waitforsignal)
                {
                    if ((bool)CurrentWaitForSignal.GetValue() == true)
                    {
                        waitforsignal = false;
                        StartDrive();
                        if (ResetWaitForSignals)
                            CurrentWaitForSignal.SetValue(false);
                        return;
                    }
                }

                if (ReferenceEquals(CurrentDrive, null))//ȷ��ָ���� Object ʵ���Ƿ�����ͬ��ʵ��
                {
                    if (!waitafterstep)
                    {
                        var wait = Sequence[CurrentStep].WaitAfterStep;
                        waitafterstep = true;
                        if (Sequence[CurrentStep].FinishedSignal != null)
                            Sequence[CurrentStep].FinishedSignal.SetValue(true);
                        Invoke("StepFinished", wait);//����wait��ֵ���ȴ��ӳ�ʱ�䴦��
                    }
                }
                else
                {
                    if (CurrentDrive.CurrentPosition == CurrentDestination && !waitafterstep)
                    {
                        var wait = Sequence[CurrentStep].WaitAfterStep;
                        if (Sequence[CurrentStep].FinishedSignal != null)
                            Sequence[CurrentStep].FinishedSignal.SetValue(true);
                        waitafterstep = true;
                        Invoke("StepFinished", wait);
                    }
                }

            }
        }
    }
}

