using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;

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
        [Foldout("Status")][ReadOnly] public StationStatus LoadStatus = StationStatus.Empty;//װ�ص�״̬
        [Foldout("Status")] [ReadOnly] public StationStatus UnloadStatus = StationStatus.Empty;//ж�ص�״̬
        [ReadOnly] public SplinePoint loadPoint;
        [ReadOnly] public SplinePoint unloadPoint;
        [ReadOnly] public GEN_Control_AGV_Drive LoadAGV;//װ�ص�AGV
        [ReadOnly] public double remainTime = 0;//ʣ��ӹ�ʱ��

        public List<Processing_list> processList = new List<Processing_list>();
        [ReadOnly] public string currentProcessNmae = "";//Ŀǰ�ӹ���������
        [ReadOnly] public int currentProcessStep = 0;//Ŀǰ�ӹ��������

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if(processList.Count != 0 && currentProcessStep < processList.Count)
                currentProcessNmae = processList[currentProcessStep].Process_name;

            //���͹�����װ�ص㣬������ȡ��������
            if (LoadStatus == StationStatus.Waiting && LoadAGV.AGVStatus == GEN_Control_AGV_Drive.StationStatus.Waiting && LoadAGV.targetAGVDrive.currentLine.endPoint == loadPoint)
            {
                LoadAGV.AGVStatus = GEN_Control_AGV_Drive.StationStatus.Empty;//��װ�ص����깤�����ͷ�AGV
                LoadStatus = StationStatus.Working;
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
            }


        }
    }
}

