using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;
using XCharts.Runtime;
namespace VisualSpline
{
    [Serializable]
    public class SingalMachine
    {
        public GameObject machineName;
        public SplinePoint LoadPoint;
        public SplinePoint UnloadPoint;

    }
    [Serializable]
    public class SingalPiece
    {
        public PieceStateMachine stateMachine;
        public int currentPieceNum;
        public List<double> drawDataPieceTime;
        public List<int> drawDataPieceNum;
        public Material exhibition;
        
    }
    public class GEN_Encoed_result : MonoBehaviour
    {
        public GEN_FunctionTest GEN_Function;//ѵ����������ȡ����
        public Spline Map;//�������������ڵ�ͼ
        public GameObject AGVPrefeb;//AGVԤ����
        public SplinePoint AGVStart;//AGV��ʼ��
        public SplinePoint AGVEnd;//AGV�յ�

        [Header("Work Control")]
        public bool startWork = false;
        [Range(0, 100)] public float timeSpeed = 1.0f;//����
        [ReadOnly] public double timeTotal = 0;//�ۼ���ʱ

        [Header("Moving line analysis")]
        public bool openLineAnalysis = false;
        public GameObject LinePrefeb;
        public float LineWidth = 0.002f;
        public float LineWidthRandom = 0.001f;
        public List<Color> LineColor;

        [Header("Draw Chart")]
        public GameObject pieceLineCanvas;
        private LineChart PieceLineChart;
        private int workPieceNum;

        [Header("Information")]
        public List<SingalMachine> Machines;//�ӹ�������λ��Ϣ
        public List<SingalPiece> Pieces;//������Ϣ


        private int[] genResult;//���Ƚ������
        public GameObject[] AGV;//AGVʵ��
        private GameObject[] Piece;//����ʵ��
        private GameObject[] Lines;//����������ʵ��



        private float lastTimeSpeed = 1.0f;
        void TimeSpeedControl()
        {
            if (lastTimeSpeed != timeSpeed)
                Time.timeScale = timeSpeed;

            lastTimeSpeed = timeSpeed;
        }
        // Start is called before the first frame update
        public void StartGENEncoed()
        {
            genResult = GEN_Function.drawData;

            //����AGVʵ������ʼ��
            AGV = new GameObject[GEN_Function.AGVNum];
            Lines = new GameObject[GEN_Function.AGVNum];
            for (int i = 0; i < GEN_Function.AGVNum; i++)
            {
                //����AGVʵ��
                AGV[i] = Instantiate(AGVPrefeb, this.transform, true);
                AGV[i].transform.position = AGVStart.transform.position;
                AGV[i].transform.localScale = AGV[i].transform.localScale * this.transform.localScale.x;//���ݸ���������
                AGV[i].name = "AGV"+i;
                AGVSplineDrive agvDrive = AGV[i].GetComponent<AGVSplineDrive>();
                agvDrive.associatedSpline = Map;
                agvDrive.motionPath.Clear();//��ʼ��·��
                agvDrive.currentLine.startPoint = AGVStart;//���ó�ʼ��λ
                agvDrive.currentLine.endPoint = AGVStart;
                agvDrive.speed = 50 * GEN_Function.AGVSpeed;
                AGV[i].GetComponent<GEN_Control_AGV_Drive>().targetPoint = AGVStart;//��ʼ��Ŀ��������λΪ��
                agvDrive.currentLine.percentage = 1;

                //�������߷���ʵ��
                Lines[i] = Instantiate(LinePrefeb, AGV[i].transform.position, AGV[i].transform.rotation, AGV[i].transform) ;
                LineRenderer lR = Lines[i].GetComponent<LineRenderer>();
                lR.startWidth = LineWidth;
                if (LineColor.Count > i)
                {
                    lR.startColor = LineColor[i];
                    lR.endColor = LineColor[i];
                }
                lR.positionCount = 2;
                lR.SetPosition(0,AGVStart.transform.position);
                lR.SetPosition(1, AGVStart.transform.position);
                lR.enabled = openLineAnalysis;
                Lines[i].GetComponent<LineMarkControl>().random = LineWidthRandom;
            }

            //Ϊ������ӿ��ƽű�
            foreach (var m in Machines)
            {
                int index = Machines.IndexOf(m);

                if (Machines[index].machineName.GetComponent<GEN_Control_Machine_Drive>() == null)
                    Machines[index].machineName.AddComponent<GEN_Control_Machine_Drive>();
                GEN_Control_Machine_Drive machineDrive = Machines[index].machineName.GetComponent<GEN_Control_Machine_Drive>();
                machineDrive.loadPoint = m.LoadPoint;
                machineDrive.unloadPoint = m.UnloadPoint;
            }


            //��������ʵ������ʼ��
            Piece = new GameObject[GEN_Function.workpieceNumber];
            for (int i = 0; i < GEN_Function.workpieceNumber; i++)
            {
                Piece[i] = new GameObject("Piece" + i);
                Piece[i].transform.parent = this.transform;
                Piece[i].AddComponent<PieceStateMachine>();
                Piece[i].GetComponent<PieceStateMachine>().Encoed_Scripts = this;
                Piece[i].GetComponent<PieceStateMachine>().piece_name = i;
                Piece[i].GetComponent<PieceStateMachine>().piecePlace = AGVStart;
                Piece[i].GetComponent<PieceStateMachine>().pieceStartPlace = AGVStart;
                Piece[i].GetComponent<PieceStateMachine>().pieceEndPlace = AGVEnd;
                int step = 0;
                //Ϊ�����ͻ�����Ӽӹ�����
                for (int j = GEN_Function.processNumTotal; j < GEN_Function.processNumTotal * 2; j++)
                {

                    if (genResult[j] == i)
                    {
                        //Ϊ������Ӽӹ�����
                        GEN_Control_Piece_Drive.Processing_scheme scheme = new GEN_Control_Piece_Drive.Processing_scheme();
                        scheme.Process_name = "O" + i + (step + 1);                        
                        scheme.step = j - GEN_Function.processNumTotal;
                        scheme.machine = genResult[2 * i + step];
                        scheme.AGV = genResult[j + GEN_Function.processNumTotal];
                        Piece[i].GetComponent<PieceStateMachine>().SchemeList.Add(scheme);

                        //Ϊ������Ӽӹ��б�
                        GEN_Control_Machine_Drive.Processing_list processing = new GEN_Control_Machine_Drive.Processing_list();
                        processing.Process_name = scheme.Process_name;
                        processing.step = j - GEN_Function.processNumTotal;
                        processing.work_time = GEN_Function.processOptMachineTime[2 * i + step, genResult[2 * i + step]];
                        Machines[scheme.machine].machineName.GetComponent<GEN_Control_Machine_Drive>().processList.Add(processing);
                        step++;
                    }
                }
                GEN_Control_Piece_Drive.Processing_scheme lastScheme = new GEN_Control_Piece_Drive.Processing_scheme();
                lastScheme.Process_name = "O999999999";
                lastScheme.step = 999;
                lastScheme.machine = -1;
                lastScheme.AGV = Piece[i].GetComponent<PieceStateMachine>().SchemeList[Piece[i].GetComponent<PieceStateMachine>().SchemeList.Count - 1].AGV;
                Piece[i].GetComponent<PieceStateMachine>().SchemeList.Add(lastScheme);

                SingalPiece sP = new SingalPiece();
                sP.stateMachine = Piece[i].GetComponent<PieceStateMachine>();
                sP.drawDataPieceNum = new List<int>();
                sP.drawDataPieceTime = new List<double>();
                Pieces.Add(sP);
            }

            foreach (var m in Machines)
            {
                m.machineName.GetComponent<GEN_Control_Machine_Drive>().processList.Sort((x,y)=> { return x.step.CompareTo(y.step); });//����step����
            }

            //ͼ���ʼ��
            PieceLineChart = pieceLineCanvas.AddComponent<LineChart>();
            PieceLineChart.Init();
            PieceLineChart.EnsureChartComponent<Title>().text = "Piece";
            PieceLineChart.EnsureChartComponent<Tooltip>().show = true;
            PieceLineChart.EnsureChartComponent<Legend>().show = false;

            var xAxis = PieceLineChart.EnsureChartComponent<XAxis>();
            xAxis.splitNumber = 10;
            xAxis.boundaryGap = true;
            xAxis.type = Axis.AxisType.Time;
            xAxis.splitNumber = 5;

            var yAxis = PieceLineChart.EnsureChartComponent<YAxis>();
            yAxis.type = Axis.AxisType.Value;

            workPieceNum = GEN_Function.workpieceNumber;
            PieceLineChart.RemoveData();
            for (int i = 0; i < GEN_Function.workpieceNumber; i++)
            {
                PieceLineChart.AddSerie<XCharts.Runtime.Line>("Piece" + i);
                PieceLineChart.AddData(i, 0, 0);
            }

            //PieceLineChart.CheckChartInit(); 

        }

        public void PieceRefreshData(int piece)
        {
            if (Pieces[piece].drawDataPieceTime.Count != 0)
            {
                var time = Math.Round(Pieces[piece].drawDataPieceTime[Pieces[piece].drawDataPieceTime.Count - 1], 1);
                var num = Pieces[piece].drawDataPieceNum[Pieces[piece].drawDataPieceNum.Count - 1];
                PieceLineChart.AddData(piece, time, num);
            }
        }

        bool lastOpenLineAnalysis;
        float lastLineWidth;
        // Update is called once per frame
        void Update()
        {
            if (startWork)
                TimeSpeedControl();//�����ٶȱ任
            else
            {
                timeSpeed = 0;
                Time.timeScale = timeSpeed; 
            }
            //�ӹ���¼����
            foreach (var i in Pieces)
            {
                i.currentPieceNum = i.stateMachine.currentPieceNum;
            }

            if (openLineAnalysis != lastOpenLineAnalysis)
            {
                foreach (var i in Lines)
                {
                    i.GetComponent<LineRenderer>().enabled = openLineAnalysis;
                }
            }
            if (LineWidth != lastLineWidth)
            {
                foreach (var i in Lines)
                {
                    i.GetComponent<LineRenderer>().startWidth = LineWidth;
                }
            }
            lastOpenLineAnalysis = openLineAnalysis;
            lastLineWidth = LineWidth;
        }
        private void FixedUpdate()
        {
            if(startWork)
                timeTotal += 0.02;
        }
    }
}

