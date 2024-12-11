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
    /// �ӿ���ص��̷߳����Ļ��࣬�ṩ��ͨ���߳��е����ڡ�ʱ����㷽��
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
        private Thread CommThread;//�߳�
        private DateTime ThreadTime;
        private bool run;
        private float commcyclesum = 0;
        private DateTime last;


        /// <summary>
        /// �߳�����ʱ���õķ���
        /// </summary>
        protected virtual void CommunicationThreadUpdate()
        {
        }

        /// <summary>
        /// �̹߳ر�ʱ���õķ���
        /// </summary>
        protected virtual void CommunicationThreadClose()
        {
        }

        /// <summary>
        /// �����߳�
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
        /// �ر��߳�
        /// </summary>
        public override void CloseInterface()
        {
            run = false;
            if (CommThread != null)
                CommThread.Abort();
        }

        /// <summary>
        /// ������ֵ
        /// </summary>
        private void ResetMeasures()
        {
            CommCycleMeasureNum = 0;
            CommCycleMin = 99999;
            CommCycleMax = 0;
            commcyclesum = 0;
        }


        /// <summary>
        /// ͨ���̣߳�����ͨ��ʱ��������ڡ�ʱ����Ϣ
        /// </summary>
        void CommunicationThread()
        {
            DateTime end;
            bool first = true;
            do
            {
                CommCycleMeasureNum++;
                CommunicationThreadUpdate();//�߳�����ʱ���õķ���
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
            CommunicationThreadClose();//�̹߳ر�ʱ���õķ���

        }

    }
}