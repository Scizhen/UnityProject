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
        [Foldout("Status")][ReadOnly] public StationStatus LoadStatus = StationStatus.Empty;//装载点状态
        [Foldout("Status")] [ReadOnly] public StationStatus UnloadStatus = StationStatus.Empty;//卸载点状态
        [ReadOnly] public SplinePoint loadPoint;
        [ReadOnly] public SplinePoint unloadPoint;
        [ReadOnly] public ValueOutputInt StationStartStatus;
        [ReadOnly] public ValueOutputInt StationEndStatus;
        [ReadOnly] public GEN_Control_AGV_Drive LoadAGV;//装载点AGV
        [ReadOnly] public double remainTime = 0;//剩余加工时间

        public List<Processing_list> processList = new List<Processing_list>();
        [ReadOnly] public string currentProcessNmae = "";//目前加工工序名称
        [ReadOnly] public int currentProcessStep = 0;//目前加工工序进度

        // Start is called before the first frame update
        void Start()
        {
            for (int i = 0; i < StatusTimes.Length; i++)
            {
                StatusTimes[i] = 0;
            }
            //更新装载点和卸载点状态
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

            //运送工件至装载点，进行拿取工件操作
            if (LoadStatus == StationStatus.Working)// && LoadAGV.AGVStatus == GEN_Control_AGV_Drive.StationStatus.Waiting && LoadAGV.targetAGVDrive.currentLine.endPoint == loadPoint)
            {
                //LoadAGV.AGVStatus = GEN_Control_AGV_Drive.StationStatus.Empty;//在装载点拿完工件后释放AGV
                LoadStatus = StationStatus.Empty;
                UnloadStatus = StationStatus.Empty;
                remainTime = processList[currentProcessStep].work_time;
            }

        }
        private void FixedUpdate()
        {
            //加工工件
            if (LoadStatus == StationStatus.Working)
            {
                remainTime -= 0.02f;//每时间帧-0.02s的加工时间
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

