using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RDTS
{
    public class StationEventStopDrive : MonoBehaviour
    {
        public ValueInputInt MoveToPlace;
        public ValueMiddleBool StartLoad;
        public ValueMiddleBool EndLoad;
        public ValueOutputInt MakePlace;
        public ValueOutputBool PlaceSignal;
        public ValueInputInt PlaceObjOk;

        public int RestTime = 0;
        private BaseStation stationValue;
        private PathMover pathMoverValue;
        public WorkStation WorkStationValue;
        private int moveAGVFlag = 0;
        private float TimeToMoveAGV = 0;
        private int _lastMakePlace = 0;
        private int _AGVmove = 0;

        //��վ����һ��
        public void OnStationWorkStarting(BaseStation station, PathMover pathMover)//�ı䷽��
        {
            stationValue = station;
            pathMoverValue = pathMover;
            var drive = pathMover.GetComponent<Drive>();
            drive.ReverseDirection = !drive.ReverseDirection;
            if (MoveToPlace != null)//���빤��վ�����е�۷ž�λָ��
                MoveToPlace.Value = 1;



        }
        //ÿ֡����
        public void ControlAGVMove(BaseStation station, PathMover pathMover)
        {
            var drive = pathMover.GetComponent<Drive>();
            RestTime++;
            if (_lastMakePlace != MakePlace.Value && MakePlace.Value == 1)
            {
                _AGVmove = 1;
            }

            if (_AGVmove == 0)//��е�ۿ���С��ֹͣ����
            {
                drive.Stop();
                WorkStationValue.SetStatus(WorkStation.StationStatus.Working);
            }
            if (_AGVmove == 1)//��е�ۿ���С����վ && _LoaderOK == 1
            {
                if ((EndLoad != null)&&(StartLoad.Value==true))//���빤��վ�󷢿�ж��ָ��
                {
                    EndLoad.Value = true;
                    StartLoad.Value = false;
                }
                PlaceSignal.Value = true;
                TimeToMoveAGV++;
                if (TimeToMoveAGV >= 40)//40֡����վ
                {
                    if ((StartLoad != null)&&(StartLoad.Value==false))//���빤��վ�󷢿�װ��ָ��
                    {
                        StartLoad.Value = true;
                        EndLoad.Value = false;
                    }
                    drive.Accelerate();
                    WorkStationValue.SetStatus(WorkStation.StationStatus.Leaving);
                    moveAGVFlag = 0;
                    MoveToPlace.Value = 0;
                    PlaceSignal.Value = false;
                    TimeToMoveAGV = 0;
                    _AGVmove = 0;
                    if (PlaceObjOk != null)
                        PlaceObjOk.Value = 1;//��վ�����е�۷�ȷ�ϻ����ָ��

                }

            }
            _lastMakePlace = MakePlace.Value;


        }
        public void Update()
        {
            var updatestatus = WorkStationValue.StatusToString;
            if (updatestatus == "Working")
            {
                moveAGVFlag = 1;
            }
            if (moveAGVFlag == 1)
            {
                ControlAGVMove(stationValue, pathMoverValue);
            } 

        }
    }

}

