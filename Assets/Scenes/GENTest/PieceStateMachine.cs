using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using RDTS.Utility;
using RDTS;
namespace VisualSpline
{
    //״̬��������
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
            this.ChangeState(new MachineCheckState());//����״̬��ʵ��
            process_max = SchemeList.Count;//�ӵ��Ⱥ������ҵ����������
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
    //״̬�������࣬����ÿ��״̬��Ҫ��������
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
    //ѯ�ʻ���״̬
    public class MachineCheckState : PieceState
    {
        private GameObject targetMachine;
        private GEN_Control_Machine_Drive MachineDrive;
        public override void Enter()
        {
            Debug.Log("����ѯ�ʻ���״̬");

            //��ȡ����״̬
            int targetMachineNum = stateMachine.SchemeList[stateMachine.processStep].machine;
            targetMachine = stateMachine.Encoed_Scripts.Machines[targetMachineNum].machineName;//���ܿ��л�ȡ��Ŀ�����
            MachineDrive = targetMachine.GetComponent<GEN_Control_Machine_Drive>();
        }

        public override void Execute()
        {
            Debug.Log("��ȡ��������״̬...");
            string targetMachineProcessName = MachineDrive.currentProcessNmae;//Ŀ�������ǰ׼���ӹ��Ĺ���
            string currentPieceProcessName = stateMachine.processStepName;//������ǰ׼���ӹ��Ĺ���
            if (targetMachineProcessName == currentPieceProcessName)
                stateMachine.ChangeState(new AGVCheckState());
        }

        public override void Exit()
        {
            Debug.Log("�������ؼӹ��źţ��˳�״̬");
            MachineDrive.LoadStatus = GEN_Control_Machine_Drive.StationStatus.Entering;
            // �л�����һ��״̬
            //stateMachine.ChangeState(new AGVCheckState());
            //stateMachine.Invoke("ChangeToNextState", 5f);
        }
        public override string StateName()
        {
            return "MachineCheckState";
        }
    }
    //ѯ��AGV״̬
    public class AGVCheckState : PieceState
    {
        private GameObject targetAGV;
        private GEN_Control_AGV_Drive AGVControl;
        public override void Enter()
        {
            Debug.Log("����ѯ��AGV״̬");
            //��ȡAGV״̬
            int targetAGVNum = stateMachine.SchemeList[stateMachine.processStep].AGV;
            targetAGV = stateMachine.Encoed_Scripts.AGV[targetAGVNum];//���ܿ��л�ȡ��Ŀ��AGV
            AGVControl = targetAGV.GetComponent<GEN_Control_AGV_Drive>();
        }

        public override void Execute()
        {
            Debug.Log("��ȡAGV����״̬...");

            GEN_Control_AGV_Drive.StationStatus targetAGVStatus = AGVControl.AGVStatus;//��ȡĿ��AGV��״̬
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
            Debug.Log("AGV���ؿ����źţ��˳�״̬");
            AGVControl.AGVStatus = GEN_Control_AGV_Drive.StationStatus.Entering;//����Ŀ��AGV״̬����ǰAGV���ڱ�������������

        }

        public override string StateName()
        {
            return "AGVCheckState";
        }
    }

    //����״̬
    public class TransportState : PieceState
    {
        private GameObject targetAGV;
        private GEN_Control_AGV_Drive AGVControl;

        private GEN_Control_Machine_Drive MachineDrive;
        private SplinePoint targetMachineLoadPoint;
        private float timer = 0f;
        public override void Enter()
        {
            Debug.Log("��������״̬");
            //��ȡAGV״̬
            int targetAGVNum = stateMachine.SchemeList[stateMachine.processStep].AGV;
            targetAGV = stateMachine.Encoed_Scripts.AGV[targetAGVNum];//���ܿ��л�ȡ��Ŀ��AGV
            AGVControl = targetAGV.GetComponent<GEN_Control_AGV_Drive>();
            //��ȡĿ�����
            int targetMachineNum = stateMachine.SchemeList[stateMachine.processStep].machine;
            targetMachineLoadPoint = stateMachine.Encoed_Scripts.Machines[targetMachineNum].LoadPoint;//���ܿ��л�ȡ��Ŀ�����װ�ص�
            GameObject targetMachine = stateMachine.Encoed_Scripts.Machines[targetMachineNum].machineName;//���ܿ��л�ȡ��Ŀ�����
            MachineDrive = targetMachine.GetComponent<GEN_Control_Machine_Drive>();


            stateMachine.GetComponent<Transform>().position = AGVControl.GetComponent<Transform>().Find("exhibition").position;
            Transform exhibition = AGVControl.GetComponent<Transform>().Find("exhibition");
            Detector detector = exhibition.Find("Detector").GetComponent<Detector>();
            detector.LimitTag.Clear();
            detector.LimitLayer.Clear();
            detector.LimitTag.Add(stateMachine.tag);
            

        }

        public override void Execute()
        {
            Debug.Log("AGV�������������ڴ���������Ŀ�����...");
            GEN_Control_AGV_Drive.StationStatus targetAGVStatus = AGVControl.AGVStatus;
            //����1��AGV����Ŀ�깤����
            if (targetAGVStatus == GEN_Control_AGV_Drive.StationStatus.Entering) {
                if (AGVControl.targetPoint != stateMachine.piecePlace)
                    AGVControl.targetPoint = stateMachine.piecePlace;
            }
            if (targetAGVStatus == GEN_Control_AGV_Drive.StationStatus.Entering && AGVControl.targetAGVDrive.currentLine.endPoint == stateMachine.piecePlace && AGVControl.targetAGVDrive.currentLine.percentage == 1)
            {

                timer += Time.deltaTime;
                if (timer >= 2)
                {
                    AGVControl.GetComponent<Transform>().Find("StartLoad").GetComponent<ValueMiddleBool>().SetValue(true);
                    timer = 0f;
                    AGVControl.AGVStatus = GEN_Control_AGV_Drive.StationStatus.Waiting;
                }
                //stateMachine.GetComponent<Transform>().parent = AGVControl.GetComponent<Transform>().Find("exhibition") ;

            }
            //����2��AGV������������Ŀ���������װ�ص�
            if (AGVControl.targetPoint == stateMachine.piecePlace && targetAGVStatus == GEN_Control_AGV_Drive.StationStatus.Waiting)
            {
                AGVControl.carriedPiece = stateMachine;//��ֹAGV��ʼ��ʱ���ж�Ϊ����ת�ص�
                AGVControl.AGVStatus = GEN_Control_AGV_Drive.StationStatus.Leaving;
                if (AGVControl.targetPoint != targetMachineLoadPoint)
                    AGVControl.targetPoint = targetMachineLoadPoint;
            }
            //����Ŀ�������ȴ�״̬
            if (targetAGVStatus == GEN_Control_AGV_Drive.StationStatus.Leaving && AGVControl.targetAGVDrive.currentLine.endPoint == targetMachineLoadPoint && AGVControl.targetAGVDrive.currentLine.percentage == 1)
            {
                AGVControl.AGVStatus = GEN_Control_AGV_Drive.StationStatus.Waiting;
                AGVControl.GetComponent<Transform>().Find("StartLoad").GetComponent<ValueMiddleBool>().SetValue(false);
                AGVControl.GetComponent<Transform>().Find("EndLoad").GetComponent<ValueMiddleBool>().SetValue(true);
            }
            if (AGVControl.targetAGVDrive.currentLine.endPoint == targetMachineLoadPoint && AGVControl.targetPoint == targetMachineLoadPoint && targetAGVStatus == GEN_Control_AGV_Drive.StationStatus.Waiting) 
                stateMachine.ChangeState(new ProcessingState());




        }

        public override void Exit()
        {
            Debug.Log("Ŀ���������װ������źţ��˳�״̬");
            MachineDrive.LoadStatus = GEN_Control_Machine_Drive.StationStatus.Waiting;
            MachineDrive.LoadAGV = AGVControl;
            AGVControl.carriedPiece = null;
        }
        public override string StateName()
        {
            return "TransportState";
        }
    }

    //�ӹ�״̬
    public class ProcessingState : PieceState
    {
        private GEN_Control_Machine_Drive MachineDrive;
        private SplinePoint targetMachineUnLoadPoint;
        public override void Enter()
        {
            Debug.Log("����ӹ�״̬");
            //��ȡĿ�����
            int targetMachineNum = stateMachine.SchemeList[stateMachine.processStep].machine;
            targetMachineUnLoadPoint = stateMachine.Encoed_Scripts.Machines[targetMachineNum].UnloadPoint;//���ܿ��л�ȡ��Ŀ�����ж�ص�
            GameObject targetMachine = stateMachine.Encoed_Scripts.Machines[targetMachineNum].machineName;//���ܿ��л�ȡ��Ŀ�����
            MachineDrive = targetMachine.GetComponent<GEN_Control_Machine_Drive>();
        }

        public override void Execute()
        {
            Debug.Log("���й����ӹ�...");
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
            Debug.Log("Ŀ��������ؼӹ�����źţ��˳�״̬");
        }
        public override string StateName()
        {
            return "ProcessingState";
        }
    }
    //�깤״̬
    public class CompleteProcessingState : PieceState
    {
        private GameObject targetAGV;
        private GEN_Control_AGV_Drive AGVControl;

        private SplinePoint EndPoint;
        public override void Enter()
        {
            Debug.Log("�����깤״̬");
            //��ȡĿ�����
            //��ȡAGV״̬
            int targetAGVNum = stateMachine.SchemeList[stateMachine.processStep].AGV;
            targetAGV = stateMachine.Encoed_Scripts.AGV[targetAGVNum];//���ܿ��л�ȡ��Ŀ��AGV
            AGVControl = targetAGV.GetComponent<GEN_Control_AGV_Drive>();

            EndPoint = stateMachine.pieceEndPlace;
        }

        public override void Execute()
        {
            Debug.Log("���й�������...");
            //����1��AGV����Ŀ�깤����
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
            //����2��AGV������������Ŀ���������װ�ص�
            if (AGVControl.targetPoint == stateMachine.piecePlace && targetAGVStatus == GEN_Control_AGV_Drive.StationStatus.Waiting)
            {
                AGVControl.carriedPiece = stateMachine;//��ֹAGV��ʼ��ʱ���ж�Ϊ����ת�ص�
                AGVControl.AGVStatus = GEN_Control_AGV_Drive.StationStatus.Leaving;
                if (AGVControl.targetPoint != EndPoint)
                    AGVControl.targetPoint = EndPoint;
            }
            //����Ŀ�������ȴ�״̬
            if (targetAGVStatus == GEN_Control_AGV_Drive.StationStatus.Leaving && AGVControl.targetAGVDrive.currentLine.endPoint == EndPoint && AGVControl.targetAGVDrive.currentLine.percentage == 1)
            {
                AGVControl.AGVStatus = GEN_Control_AGV_Drive.StationStatus.Waiting;
            }
            if (AGVControl.targetAGVDrive.currentLine.endPoint == EndPoint && AGVControl.targetPoint == EndPoint && targetAGVStatus == GEN_Control_AGV_Drive.StationStatus.Waiting)
                stateMachine.ChangeState(new MachineCheckState());

        }

        public override void Exit()
        {
            Debug.Log("�ӹ����������س�ʼ״̬");
            AGVControl.AGVStatus = GEN_Control_AGV_Drive.StationStatus.Empty;
            stateMachine.processStep = 0;
            stateMachine.processStepName = stateMachine.SchemeList[0].Process_name;
            stateMachine.piecePlace = stateMachine.pieceStartPlace;
            stateMachine.currentPieceNum++;
            //��¼����-���ڻ�ͼ 
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

