using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using RDTS.Utility;
using RDTS;
namespace VisualSpline
{
    //状态机控制器
    public class PieceStateMachine : GEN_Control_Piece_Drive
    {
        [NaughtyAttributes.ReadOnly] public string state_name;
        private PieceState currentState;

        public void ChangeState(PieceState newState)
        {
            if (currentState != null)
            {
                currentState.Exit();
            }

            currentState = newState;
            currentState.SetStateMachine(this);
            state_name = currentState.StateName();
            currentState.Enter();
        }
        private void Start()
        {
            this.ChangeState(new MachineCheckState());//创建状态机实例
            process_max = SchemeList.Count;//从调度函数中找到最大工序数量
        }

        private void Update()
        {
            if(processStep < SchemeList.Count)
                processStepName = SchemeList[processStep].Process_name;
            if (currentState != null)
            {
                currentState.Execute();
            }
        }
    }
    //状态机抽象类，定义每个状态需要做的事情
    public abstract class PieceState
    {
        protected PieceStateMachine stateMachine;
        public void SetStateMachine(PieceStateMachine machine)
        {
            stateMachine = machine;
        }

        public abstract void Enter();
        public abstract void Execute();
        public abstract void Exit();
        public abstract string StateName();
    }
    //询问机器状态
    public class MachineCheckState : PieceState
    {
        private GameObject targetMachine;
        private GEN_Control_Machine_Drive MachineDrive;
        public override void Enter()
        {
            Debug.Log("进入询问机器状态");

            //获取机器状态
            int targetMachineNum = stateMachine.SchemeList[stateMachine.processStep].machine;
            targetMachine = stateMachine.Encoed_Scripts.Machines[targetMachineNum].machineName;//从总控中获取到目标机器
            MachineDrive = targetMachine.GetComponent<GEN_Control_Machine_Drive>();
        }

        public override void Execute()
        {
            Debug.Log("读取机器运行状态...");
            string targetMachineProcessName = MachineDrive.currentProcessNmae;//目标机器当前准备加工的工序
            string currentPieceProcessName = stateMachine.processStepName;//工件当前准备加工的工序
            if (targetMachineProcessName == currentPieceProcessName)
                stateMachine.ChangeState(new AGVCheckState());
        }

        public override void Exit()
        {
            Debug.Log("机器返回加工信号，退出状态");
            MachineDrive.LoadStatus = GEN_Control_Machine_Drive.StationStatus.Entering;
            // 切换到下一个状态
            //stateMachine.ChangeState(new AGVCheckState());
            //stateMachine.Invoke("ChangeToNextState", 5f);
        }
        public override string StateName()
        {
            return "MachineCheckState";
        }
    }
    //询问AGV状态
    public class AGVCheckState : PieceState
    {
        private GameObject targetAGV;
        private GEN_Control_AGV_Drive AGVControl;
        public override void Enter()
        {
            Debug.Log("进入询问AGV状态");
            //获取AGV状态
            int targetAGVNum = stateMachine.SchemeList[stateMachine.processStep].AGV;
            targetAGV = stateMachine.Encoed_Scripts.AGV[targetAGVNum];//从总控中获取到目标AGV
            AGVControl = targetAGV.GetComponent<GEN_Control_AGV_Drive>();
        }

        public override void Execute()
        {
            Debug.Log("读取AGV运行状态...");

            GEN_Control_AGV_Drive.StationStatus targetAGVStatus = AGVControl.AGVStatus;//获取目标AGV的状态
            if (targetAGVStatus == GEN_Control_AGV_Drive.StationStatus.Empty)
            { 
                if(stateMachine.processStep != stateMachine.process_max - 1)
                    stateMachine.ChangeState(new TransportState());
                else
                    stateMachine.ChangeState(new CompleteProcessingState());
            }


        }

        public override void Exit()
        {
            Debug.Log("AGV返回空闲信号，退出状态");
            AGVControl.AGVStatus = GEN_Control_AGV_Drive.StationStatus.Entering;//调整目标AGV状态，当前AGV正在被调用至工件处

        }

        public override string StateName()
        {
            return "AGVCheckState";
        }
    }

    //运输状态
    public class TransportState : PieceState
    {
        private GameObject targetAGV;
        private GEN_Control_AGV_Drive AGVControl;

        private GEN_Control_Machine_Drive MachineDrive;
        private GEN_Control_Machine_Drive CurrentMachineDrive;
        private SplinePoint targetMachineLoadPoint;
        private Detector AGV_detector;
        private float timer = 0f;
        public override void Enter()
        {
            Debug.Log("进入运输状态");
            //获取AGV状态
            int targetAGVNum = stateMachine.SchemeList[stateMachine.processStep].AGV;
            targetAGV = stateMachine.Encoed_Scripts.AGV[targetAGVNum];//从总控中获取到目标AGV
            AGVControl = targetAGV.GetComponent<GEN_Control_AGV_Drive>();
            //获取当前机器
            if (stateMachine.processStep >= 1)
            {
                int currentNum = stateMachine.processStep - 1;
                int currentMachineNum = stateMachine.SchemeList[currentNum].machine;
                GameObject currentMachine = stateMachine.Encoed_Scripts.Machines[currentMachineNum].machineName;
                CurrentMachineDrive = currentMachine.GetComponent<GEN_Control_Machine_Drive>();
            }
            //获取目标机器
            int targetMachineNum = stateMachine.SchemeList[stateMachine.processStep].machine;
            targetMachineLoadPoint = stateMachine.Encoed_Scripts.Machines[targetMachineNum].LoadPoint;//从总控中获取到目标机器装载点
            GameObject targetMachine = stateMachine.Encoed_Scripts.Machines[targetMachineNum].machineName;//从总控中获取到目标机器
            MachineDrive = targetMachine.GetComponent<GEN_Control_Machine_Drive>();

            //配置装载台只装载目标工件
            Transform exhibition = AGVControl.GetComponent<Transform>().Find("exhibition");
            AGV_detector = exhibition.Find("Detector").GetComponent<Detector>();
            AGV_detector.LimitTag.Clear();
            AGV_detector.LimitLayer.Clear();
            AGV_detector.LimitTag.Add(stateMachine.tag);
            

        }

        public override void Execute()
        {
            Debug.Log("AGV调度至工件所在处并运输至目标机器..." + AGVControl.name);
            GEN_Control_AGV_Drive.StationStatus targetAGVStatus = AGVControl.AGVStatus;
            //过程1：AGV来到目标工件处
            if (targetAGVStatus == GEN_Control_AGV_Drive.StationStatus.Entering) {
                Debug.Log("AGV调度至工件所在处并运输至目标机器000"+AGVControl.name);
                if (AGVControl.targetPoint != stateMachine.piecePlace)
                    AGVControl.targetPoint = stateMachine.piecePlace;
            }
            //AGV到达目标工件
            if (targetAGVStatus == GEN_Control_AGV_Drive.StationStatus.Entering && AGVControl.targetAGVDrive.currentLine.endPoint == stateMachine.piecePlace && AGVControl.targetAGVDrive.currentLine.percentage == 1)
            {
                Debug.Log("AGV调度至工件所在处并运输至目标机器111" + AGVControl.name);
                //第一道工序工件刷新给AGV
                if (stateMachine.processStep == 0 && AGV_detector.EffectObjects.Count==0)
                {
                    Transform exhibition = AGVControl.GetComponent<Transform>().Find("exhibition");
                    stateMachine.GetComponent<Transform>().position = exhibition.position;
                    stateMachine.GetComponent<Transform>().rotation = exhibition.rotation;
                    stateMachine.GetComponent<Transform>().Rotate(0, 90, 0);
                }

                if (stateMachine.processStep >= 1)
                {
                    CurrentMachineDrive.UnloadStatus = GEN_Control_Machine_Drive.StationStatus.Waiting;
                }
                if(AGV_detector.EffectObjects.Count > 0)
                    timer += Time.deltaTime;
                if (timer >= 1)
                {

                    AGVControl.GetComponent<Transform>().Find("StartLoad").GetComponent<ValueMiddleBool>().SetValue(true);
                    timer = 0f;
                    AGVControl.AGVStatus = GEN_Control_AGV_Drive.StationStatus.Waiting;
                }
                //stateMachine.GetComponent<Transform>().parent = AGVControl.GetComponent<Transform>().Find("exhibition") ;

            }
            //过程2：AGV将工件运送至目标机器处的装载点
            if (AGVControl.targetPoint == stateMachine.piecePlace && targetAGVStatus == GEN_Control_AGV_Drive.StationStatus.Waiting )//&&(stateMachine.processStep == 0|| CurrentMachineDrive.UnloadStatus == GEN_Control_Machine_Drive.StationStatus.Leaving))
            {
                Debug.Log("AGV调度至工件所在处并运输至目标机器222");
                AGVControl.carriedPiece = stateMachine;//防止AGV初始化时被判断为到达转载点
                AGVControl.AGVStatus = GEN_Control_AGV_Drive.StationStatus.Leaving;
                if (AGVControl.targetPoint != targetMachineLoadPoint)
                    AGVControl.targetPoint = targetMachineLoadPoint;
            }
            //到达目标点后进入等待状态
            if (targetAGVStatus == GEN_Control_AGV_Drive.StationStatus.Leaving && AGVControl.targetAGVDrive.currentLine.endPoint == targetMachineLoadPoint && AGVControl.targetAGVDrive.currentLine.percentage == 1)
            {
                Debug.Log("AGV调度至工件所在处并运输至目标机器333");
                AGVControl.GetComponent<Transform>().Find("StartLoad").GetComponent<ValueMiddleBool>().SetValue(false);
                AGVControl.GetComponent<Transform>().Find("EndLoad").GetComponent<ValueMiddleBool>().SetValue(true);

                //调整位置
                Transform exhibition = AGVControl.GetComponent<Transform>().Find("exhibition");
                stateMachine.GetComponent<Transform>().position = exhibition.position;
                stateMachine.GetComponent<Transform>().rotation = exhibition.rotation;
                stateMachine.GetComponent<Transform>().Rotate(0, 90, 0);

                AGVControl.AGVStatus = GEN_Control_AGV_Drive.StationStatus.Waiting;
                MachineDrive.LoadStatus = GEN_Control_Machine_Drive.StationStatus.Waiting;
                MachineDrive.LoadAGV = AGVControl;
            }
            if (AGVControl.targetAGVDrive.currentLine.endPoint == targetMachineLoadPoint && AGVControl.targetPoint == targetMachineLoadPoint && targetAGVStatus == GEN_Control_AGV_Drive.StationStatus.Waiting)
            {
                Debug.Log("AGV调度至工件所在处并运输至目标机器444");
                if (AGV_detector.EffectObjects.Count == 0)
                    timer += Time.deltaTime;
                if (timer >= 1)
                {
                    AGVControl.GetComponent<Transform>().Find("EndLoad").GetComponent<ValueMiddleBool>().SetValue(false);
                    timer = 0f;
                    stateMachine.ChangeState(new ProcessingState());
                }
            }

        }

        public override void Exit()
        {
            Debug.Log("目标机器返回装载完成信号，退出状态");
            AGVControl.carriedPiece = null;
            AGVControl.AGVStatus = GEN_Control_AGV_Drive.StationStatus.Empty;
        }
        public override string StateName()
        {
            return "TransportState";
        }
    }

    //加工状态
    public class ProcessingState : PieceState
    {
        private GEN_Control_Machine_Drive MachineDrive;
        private SplinePoint targetMachineUnLoadPoint;
        public override void Enter()
        {
            Debug.Log("进入加工状态");
            //获取目标机器
            int targetMachineNum = stateMachine.SchemeList[stateMachine.processStep].machine;
            targetMachineUnLoadPoint = stateMachine.Encoed_Scripts.Machines[targetMachineNum].UnloadPoint;//从总控中获取到目标机器卸载点
            GameObject targetMachine = stateMachine.Encoed_Scripts.Machines[targetMachineNum].machineName;//从总控中获取到目标机器
            MachineDrive = targetMachine.GetComponent<GEN_Control_Machine_Drive>();
            MachineDrive.LoadStatus = GEN_Control_Machine_Drive.StationStatus.Working;
        }

        public override void Execute()
        {
            Debug.Log("进行工件加工...");
            if (MachineDrive.LoadStatus == GEN_Control_Machine_Drive.StationStatus.Empty && MachineDrive.UnloadStatus == GEN_Control_Machine_Drive.StationStatus.Entering)
            {
                stateMachine.piecePlace = targetMachineUnLoadPoint;
                stateMachine.processStep++;
                if (stateMachine.processStep < stateMachine.process_max - 1)
                {
                    stateMachine.ChangeState(new MachineCheckState());
                }
                if (stateMachine.processStep == stateMachine.process_max - 1)
                {
                    stateMachine.ChangeState(new AGVCheckState());
                }
            }
        }

        public override void Exit()
        {
            Debug.Log("目标机器返回加工完成信号，退出状态");
        }
        public override string StateName()
        {
            return "ProcessingState";
        }
    }
    //完工状态
    public class CompleteProcessingState : PieceState
    {
        private GameObject targetAGV;
        private GEN_Control_AGV_Drive AGVControl;

        private SplinePoint EndPoint;
        public override void Enter()
        {
            Debug.Log("进入完工状态");
            //获取目标机器
            //获取AGV状态
            int targetAGVNum = stateMachine.SchemeList[stateMachine.processStep].AGV;
            targetAGV = stateMachine.Encoed_Scripts.AGV[targetAGVNum];//从总控中获取到目标AGV
            AGVControl = targetAGV.GetComponent<GEN_Control_AGV_Drive>();

            EndPoint = stateMachine.pieceEndPlace;
        }

        public override void Execute()
        {
            Debug.Log("进行工件运输...");
            //过程1：AGV来到目标工件处
            GEN_Control_AGV_Drive.StationStatus targetAGVStatus = AGVControl.AGVStatus;
            if (targetAGVStatus == GEN_Control_AGV_Drive.StationStatus.Entering)
            {
                if (AGVControl.targetPoint != stateMachine.piecePlace)
                    AGVControl.targetPoint = stateMachine.piecePlace;
            }
            if (targetAGVStatus == GEN_Control_AGV_Drive.StationStatus.Entering && AGVControl.targetAGVDrive.currentLine.endPoint == stateMachine.piecePlace && AGVControl.targetAGVDrive.currentLine.percentage == 1)
            {
                AGVControl.AGVStatus = GEN_Control_AGV_Drive.StationStatus.Waiting;
            }
            //过程2：AGV将工件运送至目标机器处的装载点
            if (AGVControl.targetPoint == stateMachine.piecePlace && targetAGVStatus == GEN_Control_AGV_Drive.StationStatus.Waiting)
            {
                AGVControl.carriedPiece = stateMachine;//防止AGV初始化时被判断为到达转载点
                AGVControl.AGVStatus = GEN_Control_AGV_Drive.StationStatus.Leaving;
                if (AGVControl.targetPoint != EndPoint)
                    AGVControl.targetPoint = EndPoint;
            }
            //到达目标点后进入等待状态
            if (targetAGVStatus == GEN_Control_AGV_Drive.StationStatus.Leaving && AGVControl.targetAGVDrive.currentLine.endPoint == EndPoint && AGVControl.targetAGVDrive.currentLine.percentage == 1)
            {
                AGVControl.AGVStatus = GEN_Control_AGV_Drive.StationStatus.Waiting;
            }
            if (AGVControl.targetAGVDrive.currentLine.endPoint == EndPoint && AGVControl.targetPoint == EndPoint && targetAGVStatus == GEN_Control_AGV_Drive.StationStatus.Waiting)
                stateMachine.ChangeState(new MachineCheckState());

        }

        public override void Exit()
        {
            Debug.Log("加工结束，返回初始状态");
            AGVControl.AGVStatus = GEN_Control_AGV_Drive.StationStatus.Empty;
            stateMachine.processStep = 0;
            stateMachine.processStepName = stateMachine.SchemeList[0].Process_name;
            stateMachine.piecePlace = stateMachine.pieceStartPlace;
            stateMachine.currentPieceNum++;
            //记录数据-用于画图 
            stateMachine.Encoed_Scripts.Pieces[stateMachine.piece_name].drawDataPieceNum.Add(stateMachine.currentPieceNum);
            stateMachine.Encoed_Scripts.Pieces[stateMachine.piece_name].drawDataPieceTime.Add(stateMachine.Encoed_Scripts.timeTotal);
            stateMachine.Encoed_Scripts.PieceRefreshData(stateMachine.piece_name);
        }
        public override string StateName()
        {
            return "ProcessingState";
        }
    }

}

