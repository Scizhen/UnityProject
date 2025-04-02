 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;
using RDTS;

namespace VisualSpline
{
    public class GEN_Control_Machine_Drive : MonoBehaviour
    {
        [Serializable]
        public class Processing_list
        {
            public int step;
            public string Process_name;
            public double work_time;
        }

        public enum StationStatus { Entering, Working, Leaving, Empty, Failure, Waiting };
        public int machineIndex;
        public double[] StatusTimes = new double[6];
        [Foldout("Status")][ReadOnly] public StationStatus LoadStatus = StationStatus.Empty;//װ�ص�״̬
        [Foldout("Status")] [ReadOnly] public StationStatus UnloadStatus = StationStatus.Empty;//ж�ص�״̬
        [ReadOnly] public SplinePoint loadPoint;
        [ReadOnly] public SplinePoint unloadPoint;
        [ReadOnly] public ValueOutputInt StationStartStatus;
        [ReadOnly] public ValueOutputInt StationEndStatus;
        [ReadOnly] public GEN_Control_AGV_Drive LoadAGV;//װ�ص�AGV
        [ReadOnly] public double remainTime = 0;//ʣ��ӹ�ʱ��

        public List<Processing_list> processList = new List<Processing_list>();
        [ReadOnly] public string currentProcessNmae = "";//Ŀǰ�ӹ���������
        [ReadOnly] public int currentProcessStep = 0;//Ŀǰ�ӹ��������

        // Start is called before the first frame update
        void Start()
        {
            for (int i = 0; i < StatusTimes.Length; i++)
            {
                StatusTimes[i] = 0;
            }
            //����װ�ص��ж�ص�״̬
            int machineIndexAddOne = machineIndex + 1;
            string StationStartStatus_name = machineIndexAddOne + "StationStartStatus";
            string StationEndStatus_name = machineIndexAddOne + "StationEndStatus";
            if (this.GetComponent<Transform>().Find("Signals") != null)
            {
                Transform signals = this.GetComponent<Transform>().Find("Signals");
                this.StationStartStatus = signals.Find(StationStartStatus_name).GetComponent<ValueOutputInt>();
                this.StationEndStatus = signals.Find(StationEndStatus_name).GetComponent<ValueOutputInt>();
            }

        }

        int StationStatus2num(StationStatus ss)
        {
            int num = -1;
            switch (ss) {
                case StationStatus.Entering:
                    num = 0;
                    break;
                case StationStatus.Working:
                    num = 1;
                    break;
                case StationStatus.Leaving:
                    num = 2;
                    break;
                case StationStatus.Empty:
                    num = 3;
                    break;
                case StationStatus.Failure:
                    num = 4;
                    break;
                case StationStatus.Waiting:
                    num = 5;
                    break;
            }
            return num;
        }
        // Update is called once per frame
        void Update()
        {
            if (StationStartStatus is not null)
                StationStartStatus.SetValue(StationStatus2num(LoadStatus));
            if (StationEndStatus is not null)
                StationEndStatus.SetValue(StationStatus2num(UnloadStatus));

            if (processList.Count != 0 && currentProcessStep < processList.Count)
                currentProcessNmae = processList[currentProcessStep].Process_name;

            //���͹�����װ�ص㣬������ȡ��������
            if (LoadStatus == StationStatus.Working)// && LoadAGV.AGVStatus == GEN_Control_AGV_Drive.StationStatus.Waiting && LoadAGV.targetAGVDrive.currentLine.endPoint == loadPoint)
            {
                //LoadAGV.AGVStatus = GEN_Control_AGV_Drive.StationStatus.Empty;//��װ�ص����깤�����ͷ�AGV
                LoadStatus = StationStatus.Empty;
                UnloadStatus = StationStatus.Empty;
                remainTime = processList[currentProcessStep].work_time;
            }

        }
        private void FixedUpdate()
        {
            //�ӹ�����
            if (LoadStatus == StationStatus.Working)
            {
                remainTime -= 0.02f;//ÿʱ��֡-0.02s�ļӹ�ʱ��
                if (remainTime < 0)
                {
                    LoadStatus = StationStatus.Empty;
                    UnloadStatus = StationStatus.Entering;
                    currentProcessStep++;
                    if (currentProcessStep >= processList.Count)
                        currentProcessStep = 0;
                }
                StatusTimes[1] += 0.02f;
            }
            else
            {
                StatusTimes[3] += 0.02f;
            }


        }
    }
}

