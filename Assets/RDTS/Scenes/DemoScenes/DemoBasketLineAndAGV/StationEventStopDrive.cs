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

        //进站调用一次
        public void OnStationWorkStarting(BaseStation station, PathMover pathMover)//改变方向
        {
            stationValue = station;
            pathMoverValue = pathMover;
            var drive = pathMover.GetComponent<Drive>();
            drive.ReverseDirection = !drive.ReverseDirection;
            if (MoveToPlace != null)//进入工作站后给机械臂放就位指令
                MoveToPlace.Value = 1;



        }
        //每帧调用
        public void ControlAGVMove(BaseStation station, PathMover pathMover)
        {
            var drive = pathMover.GetComponent<Drive>();
            RestTime++;
            if (_lastMakePlace != MakePlace.Value && MakePlace.Value == 1)
            {
                _AGVmove = 1;
            }

            if (_AGVmove == 0)//机械臂控制小车停止运行
            {
                drive.Stop();
                WorkStationValue.SetStatus(WorkStation.StationStatus.Working);
            }
            if (_AGVmove == 1)//机械臂控制小车离站 && _LoaderOK == 1
            {
                if ((EndLoad != null)&&(StartLoad.Value==true))//进入工作站后发可卸载指令
                {
                    EndLoad.Value = true;
                    StartLoad.Value = false;
                }
                PlaceSignal.Value = true;
                TimeToMoveAGV++;
                if (TimeToMoveAGV >= 40)//40帧后离站
                {
                    if ((StartLoad != null)&&(StartLoad.Value==false))//进入工作站后发可装载指令
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
                        PlaceObjOk.Value = 1;//离站后给机械臂发确认货物的指令

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

