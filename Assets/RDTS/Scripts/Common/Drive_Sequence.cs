// Game4Automation (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// (c) 2019 in2Sight GmbH - Usage of this source code only allowed based on License conditions see https://game4automation.com/lizenz  

using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace RDTS
{

    //! Defines sequentially movement of drives which can set signals or be started by signals 定义可以设置信号或由信号启动的驱动器的顺序运动
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

        void NextStep()//移动前准备
        {
            waitafterstep = false;
            CurrentStep++;
            waitforsignal = false;
            if (CurrentStep > Sequence.Count - 1)//超出步骤数就置位
            {
                CurrentStep = 0;
            }

            CurrentDrive = Sequence[CurrentStep].Drive;//获取该步骤的Drive

            if (CurrentDrive == null)
                CurrentDrive = this.GetComponent<Drive>();//若该步骤无指定Drive默认获取挂载在该组件上的Drive

            CurrentDestination = Sequence[CurrentStep].Destination;//该步骤需要执行的位移

            if (Sequence[CurrentStep].SignalToFalseOnStart != null)
            {
                Sequence[CurrentStep].SignalToFalseOnStart.SetValue(false);//若有信号则进行置位
            }

            if (Sequence[CurrentStep].FinishedSignal != null)
            {
                Sequence[CurrentStep].FinishedSignal.SetValue(false);//若有信号则进行置位
            }


            if (Sequence[CurrentStep].WaitForSignal != null)
            {
                waitforsignal = true;
                CurrentWaitForSignal = Sequence[CurrentStep].WaitForSignal;//若有信号则进行置位
            }
            else
            {
                StartDrive();//开始运动
                if (Sequence[CurrentStep].NoWait)
                    StepFinished();
            }


        }

        /// <summary>
        /// 以设定的速度前往设定的目标点
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
        /// 当前步骤完成，进行下一步：分自动进行和手动按钮
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

                if (ReferenceEquals(CurrentDrive, null))//确定指定的 Object 实例是否是相同的实例
                {
                    if (!waitafterstep)
                    {
                        var wait = Sequence[CurrentStep].WaitAfterStep;
                        waitafterstep = true;
                        if (Sequence[CurrentStep].FinishedSignal != null)
                            Sequence[CurrentStep].FinishedSignal.SetValue(true);
                        Invoke("StepFinished", wait);//设置wait的值来等待延迟时间处理
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

