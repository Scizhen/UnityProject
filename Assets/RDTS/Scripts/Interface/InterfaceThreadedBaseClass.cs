//*************************************************************************
//Thanks for the code reference game4automation provides.                 *
//                                                                        *
//*************************************************************************
using UnityEngine;
using System.Threading;
using System;
using RDTS.Utility;

namespace RDTS
{
    /// <summary>
    /// 接口相关的线程方法的基类，提供了通信线程中的周期、时间计算方法
    /// </summary>
    public class InterfaceThreadedBaseClass : InterfaceBaseClass
    {
        public int MinCommCycleMs = 0;
        public int CommCycleMeasures = 1000;
        [ReadOnly] public int CommCycleNr;
        [ReadOnly] public int CommCycleMs;
        [ReadOnly] public int CommCycleMin;
        [ReadOnly] public float CommCycleMed;
        [ReadOnly] public int CommCycleMax;
        [ReadOnly] public int CommCycleMeasureNum;
        [ReadOnly] public string ThreadStatus;
        private Thread CommThread;//线程
        private DateTime ThreadTime;
        private bool run;
        private float commcyclesum = 0;
        private DateTime last;


        /// <summary>
        /// 线程运行时调用的方法
        /// </summary>
        protected virtual void CommunicationThreadUpdate()
        {
        }

        /// <summary>
        /// 线程关闭时调用的方法
        /// </summary>
        protected virtual void CommunicationThreadClose()
        {
        }

        /// <summary>
        /// 开启线程
        /// </summary>
        public override void OpenInterface()
        {
            ThreadStatus = "running";
            CommThread = new Thread(CommunicationThread);
            CommCycleNr = 0;
            run = true;
            ResetMeasures();
            CommThread.Start();
        }

        /// <summary>
        /// 关闭线程
        /// </summary>
        public override void CloseInterface()
        {
            run = false;
            if (CommThread != null)
                CommThread.Abort();
        }

        /// <summary>
        /// 重置数值
        /// </summary>
        private void ResetMeasures()
        {
            CommCycleMeasureNum = 0;
            CommCycleMin = 99999;
            CommCycleMax = 0;
            commcyclesum = 0;
        }


        /// <summary>
        /// 通信线程，计算通信时的相关周期、时间信息
        /// </summary>
        void CommunicationThread()
        {
            DateTime end;
            bool first = true;
            do
            {
                CommCycleMeasureNum++;
                CommunicationThreadUpdate();//线程运行时调用的方法
                ThreadTime = last;
                CommCycleNr++;
                end = DateTime.Now;
                TimeSpan span = end - last;
                last = DateTime.Now;
                if (!first)
                {
                    CommCycleMs = (int)span.TotalMilliseconds;

                    // Calculate Communication Statistics
                    commcyclesum = commcyclesum + CommCycleMs;
                    if (CommCycleMs > CommCycleMax)
                        CommCycleMax = CommCycleMs;
                    if (CommCycleMs < CommCycleMin)
                        CommCycleMin = CommCycleMs;
                    CommCycleMed = commcyclesum / CommCycleMeasureNum;
                    if (CommCycleMeasureNum > CommCycleMeasures)
                        ResetMeasures();
                }

                first = false;

                if (MinCommCycleMs - CommCycleMs > 0)
                    Thread.Sleep(MinCommCycleMs - CommCycleMs);
            } while (run == true);
            CommunicationThreadClose();//线程关闭时调用的方法

        }

    }
}