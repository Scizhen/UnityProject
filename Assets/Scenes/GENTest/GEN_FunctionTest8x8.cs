using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;
using OfficeOpenXml;
using System.IO;
using XCharts.Runtime;

public class GEN_FunctionTest8x8 : MonoBehaviour
{
    private string Excelpath = "";
    private FileInfo _excelName;
    public string DataExportPath = "/Scenes/GENTest/Data/";  //ʵ�������ļ��б���·��
    public string DataExportName = "nof";             //ʵ�������ļ�������
    public bool SaveTotalData = false;                  //����ÿһ�ε�������
    public bool SaveEveryFitMinData = true;         //�Ƿ񱣴�ÿһ�ε�����Ӧ�����ֵ
    public bool SaveEveryExperimentsData = true;         //�Ƿ񱣴�ÿһ�ε�����Ӧ�����ֵ

    public bool open_greed = true;
    public bool open_improve = true;

    public int ExperimentsNum = 1;
    public int iteration = 1000;//��������
    public int gensize = 50;//��Ⱥ��С
    public int workpieceNumber = 8;//3;//��������
    public int machineNum = 8;//3;//��������
    public int AGVNum = 2;//3;//AGV����
    public int processNumTotal = 27;//6;//��������
    public float AGVSpeed = 1f;
    public int[] drawData;
    public Color[] drawColor;
    public GameObject canvas;
    private double Punishment_set = 999;


    public int CrossCount_min = 1;
    public int CrossCount_max = 5;
    public double CrossRange = 0.5;
    public double ReserveRange = 0.2; //��������
    public double MutationProbability = 0.1;
    private int[] processNum1 = { 3,4,3,3,4,3,3,4};//{ 2, 2, 2 };//������Ӧ��������
    private int[] processNum;//= { 0, 0, 1, 1, 2, 2 };//����չ�����ڳ�ʼ��
    private int[] processOptNum;// = { 3, 3, 3, 3, 3, 2 };//�����ѡ��Ļ�������
    //public int[,] processOptMachine = { { 0, 1, 2 }, { 0, 1, 2 }, { 0, 1, 2 }, { 0, 1, 2 }, { 0, 1, 2 }, { 0, 2, 1 } };//����ѡ�������Ӧ�������
    public double[,] processOptMachineTime= { { 5, 3, 5, 3, 3, 9999, 10, 9 }, { 10, 9999, 5, 8, 3, 9, 9, 6 }, { 9999, 10, 9999, 5, 6, 2, 4, 5 }, { 5, 7,3,9,8,9999,9,9999 },{ 9999,8,5,2,6,7,10,9},{ 9999,10,9999,5,6,4,1,7},{ 10,8,9,6,4,7,9999,9999},{ 10,9999,9999,7,6,5,2,4},{ 9999,10,6,4,8,9,10,9999},{ 1,4,5,6,9999,10,9999,7},{ 3,1,6,5,9,7,8,4},{ 12,11,7,8,10,5,6,9},{ 4,6,2,10,3,9,5,7},{ 3,6,7,8,9,9999,10,9999},{ 10,7,9999,4,9,8,6,9999},{ 9999,9,8,7,4,2,7,9999},{ 11,9,9999,6,7,5,3,6},{ 6,7,1,4,6,9,9999,9999},{ 11,9999,9,9,9,7,8,10},{ 10,5,9,10,11,9999,10,4},{ 5,4,2,6,7,9999,10,9999},{ 9999,9,9999,9,11,9,10,5},{ 9999,8,9,3,8,6,9999,10},{ 2,8,5,9,9999,4,9999,8},{ 7,4,7,8,9,9999,10,9999},{ 9,9,9999,8,5,6,7,1},{ 9,9999,3,7,1,5,8,9999} };//����ѡ���������ʱ��
    private double[] machineDistance;//= {1, 2, 4, 1, 2, 4};//�豸��ľ��룬���ڼ���AGVworktime
    [Serializable]
    public class PieceMachineTime
    {
        public List<PieceProcess> pieceProcess;
    }
    [Serializable]
    public class PieceProcess
    {
        public List<MachineTimes> machineTimes;
    }
    [Serializable]
    public class MachineTimes
    {
        public int machineNum;
        public double workTime;
    }

    public List<GameObject> machinesPlaceInformation;
    public List<PieceMachineTime> pieceMachineTime;


    public double cal_AGVworktime(double AGV_x,double AGV_y, int endMachine_num)
    {
        double distance = Math.Abs(AGV_x - machineDistance[endMachine_num]) + Math.Abs(AGV_y - machineDistance[endMachine_num + machineNum]);//�����豸����Ҫ�ߵľ���
        double output = distance / AGVSpeed;
        return output;
    }
    /// <summary>
    /// ����agvλ��
    /// </summary>
    /// <param name="Tagv_xy">agv����</param>
    /// <param name="Ai">agv���</param>
    /// <param name="Mi">�������</param>
    /// <returns></returns>
    public double[] update_AGVxy(double[] Tagv_xy, int Ai, int Mi)
    {
        Tagv_xy[Ai] = machineDistance[Mi];
        Tagv_xy[Ai + AGVNum] = machineDistance[Mi + machineNum];
        return Tagv_xy;
    }

    /// <summary>
    /// ����
    /// </summary>
    /// <param name="gensize"></param>
    /// <param name="genData"></param>
    /// <param name="processNum"></param>
    /// <param name="p_fit"></param>
    public void EnCode(int gensize, int[,] genData, int[] processNum, out double[] p_fit)
    {
        double[] p_fitness = new double[gensize];
        int totalProcessNum = processNum.Length;//��������

        for (int i = 0; i < gensize; i++)
        {
            int[] Mij = new int[totalProcessNum];//�����Ӧ�ӹ���������
            double[] Tij = new double[totalProcessNum];//�����Ӧ�����ӹ�ʱ�伯��
            double[,] MachineEmptyTime = new double[machineNum * 2, totalProcessNum * 2];//�����ӹ�����ʱ�䡾����1����ʱ����ʼ������1����ʱ�����.....�������ڼ���Ƿ����̰���㷨
            int[] MachineEmptyTime_index = new int[machineNum * 2];
            for (int j = 0; j < totalProcessNum; j++)
            {
                Mij[j] =  genData[i, j];//�����Ӧ����            
                Tij[j] = processOptMachineTime[j, Mij[j]];//ÿһ�������Ӧʱ��
            }
            int[] Oij = new int[workpieceNumber];//�����ӹ��������
            for (int k = 0; k < Oij.Length; k++)
            {
                Oij[k] = 0;
            }
            double[] Tmi = new double[machineNum];//����ʱ�����
            for (int k = 0; k < Tmi.Length; k++)
            {
                Tmi[k] = 0;
            }
            double[] Tpi = new double[workpieceNumber];//�����ӹ�ʱ�����,�������Ź������ʼʱ��
            for (int k = 0; k < Tpi.Length; k++)
            {
                Tpi[k] = 0;
            }
            double[] Tagv = new double[AGVNum];//AGV����ʱ����ȼ�¼
            for (int k = 0; k < Tagv.Length; k++)
            {
                Tagv[k] = 0;
            }
            double[] Tagv_xy = new double[AGVNum * 2];//��ǰAGV���ڵص�x+y
            for (int k = 0; k < Tagv_xy.Length; k++)
            {
                Tagv_xy[k] = 0;
            }

            //��ʼ���żӹ�����
            for (int j = totalProcessNum; j < totalProcessNum * 2; j++)
            {
                //������
                int Oi = genData[i, j];//��ǰѡ��Ĺ���

                //������
                int k = 0;//������ʼ����
                for (int m = 0; m < Oi; m++)
                {
                    k = k + processNum1[m];
                }                
                int Mi = Mij[k + Oij[Oi]];//�����������Ļ������,0,1,2

                //AGV��
                int Ai = genData[i, totalProcessNum + j];

                double AGVworktime = cal_AGVworktime(Tagv_xy[Ai], Tagv_xy[Ai + AGVNum], Mi);//AGV����ʱ��

                //���ݹ����żӹ�ʱ��
                if (false) //��һ������,������ǰ�ù���
                {
                    //Tagv[Ai] = (Tagv[Ai] + AGVworktime) < Tmi[Mi] ? Tmi[Mi] : (Tagv[Ai] + AGVworktime);//AGV����ʱ��,�������ʱ�� С�� �����ɼӹ�ʱ����Ȼ����ӹ���
                    //Tagv_xy = update_AGVxy(Tagv_xy, Ai, Mi);//����AGVλ��

                    //Tmi[Mi] = Tagv[Ai] + Tij[k + Oij[Oi]];//�������Ź��� k + Oij[Oi] ����+�������=�ù�������ʱ�䣬��Mi�Ż����Ľ�����ǰ�Ƹù���
                    //Tpi[Oi] = Tmi[Mi];//����ʱ�����
                }
                else
                {
                    double Tagv_Tpi = Tagv[Ai] < Tpi[Oi] ? (Tpi[Oi] + AGVworktime) : (Tagv[Ai] + AGVworktime);//�õ�����AGV���ù������͵�Ŀ��λ������ʱ��,����AGV�Ƿ�������
                    if (Tagv_Tpi < Tmi[Mi])//�������͵�Ŀ�������ʱ��С����ѡ��������ɼӹ�ʱ�䣬�ȴ������ӹ����ֱ�Ӽӹ��ù���
                    {
                        bool is_use_greed_time = false;
                        //----�ж��Ƿ����̰������
                        double greed_time = Tagv_Tpi + Tij[k + Oij[Oi]];
                        if (open_greed && greed_time < Tmi[Mi])//����У�飬�ʹ�ʱ�䣫�ӹ�ʱ���Ƿ�С�ڻ�����ʼʱ��
                        {
                            for (int met = 0; met < MachineEmptyTime_index[Mi]; met++)
                            {
                                if (greed_time < MachineEmptyTime[Mi, met * 2 + 1] && is_use_greed_time == false)//̰��ʱ��С�ڿ���ʱ�����
                                {
                                    double cal = MachineEmptyTime[Mi, met * 2 + 1] - Tij[k + Oij[Oi]]; //���н���ʱ��-�ӹ�ʱ��
                                    if (cal >= MachineEmptyTime[Mi, met * 2] && Tagv_Tpi <= cal)//ȷ���г����ʱ��ӹ�,���Ҽӹ�֮ǰ�������͵�������
                                    {
                                        if (MachineEmptyTime[Mi, met * 2] <= Tagv_Tpi)//���������ʼʱ��С���ʹ�ʱ�䣬���ʹ�ʱ�俪ʼ�ӹ�
                                        {
                                            Tagv[Ai] = Tagv_Tpi;

                                            //���¿���ʱ��
                                            MachineEmptyTime[Mi, MachineEmptyTime_index[Mi] * 2] = Tagv_Tpi + Tij[k + Oij[Oi]];
                                            MachineEmptyTime[Mi, MachineEmptyTime_index[Mi] * 2 + 1] = MachineEmptyTime[Mi, met * 2 + 1];
                                            MachineEmptyTime_index[Mi]++;

                                            MachineEmptyTime[Mi, met * 2 + 1] = Tagv_Tpi;


                                        }
                                        else//��Ȼ���ÿ�����ʼʱ�俪ʼ�ӹ�
                                        {
                                            Tagv[Ai] = MachineEmptyTime[Mi, met * 2];

                                            MachineEmptyTime[Mi, MachineEmptyTime_index[Mi] * 2] = MachineEmptyTime[Mi, met * 2] + Tij[k + Oij[Oi]];
                                            MachineEmptyTime[Mi, MachineEmptyTime_index[Mi] * 2 + 1] = MachineEmptyTime[Mi, met * 2 + 1];
                                            MachineEmptyTime_index[Mi]++;

                                            MachineEmptyTime[Mi, met * 2 + 1] = MachineEmptyTime[Mi, met * 2];
                                        }
                                        is_use_greed_time = true;
                                    }
                                }
                            }
                        }
                        //---����̰��
                        if (is_use_greed_time == false)
                        {
                            Tagv[Ai] = Tmi[Mi];//AGV���ȵ��ڻ������翪��ʱ��
                            Tagv_xy = update_AGVxy(Tagv_xy, Ai, Mi);//����AGVλ��
                            Tmi[Mi] += Tij[k + Oij[Oi]];//�������Ź���
                        }
                    }
                    else //�������͵�Ŀ�������ʱ�������ѡ��������ɼӹ�ʱ�䣬�ȴ������˵����ٷŵ������ϼӹ�
                    {
                        MachineEmptyTime[Mi, 2 * MachineEmptyTime_index[Mi]] = Tmi[Mi];
                        Tagv[Ai] = Tagv_Tpi;
                        Tagv_xy = update_AGVxy(Tagv_xy, Ai, Mi);//����AGVλ��
                        MachineEmptyTime[Mi, 2 * MachineEmptyTime_index[Mi] + 1] = Tagv_Tpi;
                        MachineEmptyTime_index[Mi]++;
                        Tmi[Mi] = Tagv_Tpi + Tij[k + Oij[Oi]];
                    }
                    Tpi[Oi] = Tmi[Mi];//����ʱ�����

                }
                Oij[Oi] += 1;
            }
            p_fitness[i] = Tmi.Max();
            //Debug.Log("���ν����" + p_fitness[i]);
        }
        p_fit = p_fitness;
    }
    /// <summary>
    /// ����
    /// </summary>
    /// <param name="gensize">��Ⱥ��С</param>
    /// <param name="genData">����</param>
    /// <param name="processNum">����</param>
    /// <param name="gensize_after">�������</param>
    public void Cross(int gensize, int[,] genData, int[] processNum, out int[,] gensize_after)
    {
        int[,] childx0 = new int[gensize, processNum.Length * 3];
        Array.Copy(genData, childx0, genData.Length);

        int cross_count = (int)(gensize * CrossRange);//��������
        int alternate_position = 0;

        for (int i = 0; i < cross_count/2; i++)
        {
            //���ν������������
            int gensize1 = UnityEngine.Random.Range(0, gensize - 1);
            int gensize2 = UnityEngine.Random.Range(gensize1, gensize - 1);
            //�����뽻��
            int index1 = UnityEngine.Random.Range(0, processNum.Length - 1);//���ν�������Ļ�������ʼ
            int index2 = UnityEngine.Random.Range(index1, processNum.Length);//���ν�������Ļ��������
            for (int j = index1; j <= index2; j++)
            {
                int temp = childx0[gensize1, j];
                childx0[gensize1, j] = childx0[gensize2, j];
                childx0[gensize2, j] = temp;
            }
            //�����뽻��
            int[] code1 = new int[workpieceNumber];//J1ѡ����Ĺ���
            int[] code2 = new int[workpieceNumber];//J2ѡ����Ĺ���
            for (int k = 0; k < workpieceNumber; k++)
            {
                code1[k] = k;
            }
            int index3 = UnityEngine.Random.Range(1, workpieceNumber);//�������
            Array.Resize(ref code2, index3);
            for (int k = 0; k < index3; k++)//ѡ�񽻲�Ĺ���
            {
                int index4 = UnityEngine.Random.Range(0, code1.Length);//�������
                code2[k] = code1[index4];
                code1[index4] = code1[code1.Length - 1];
                Array.Resize(ref code1, code1.Length - 1);
            }

            //���н���
            int[] parent1 = new int[processNum.Length];//����P1
            int[] parent2 = new int[processNum.Length];//����CP2
            int[] child1 = new int[processNum.Length];//�Ӵ�C1
            int[] child2 = new int[processNum.Length];//�Ӵ�C2
            //�Ӵ���������ֵ
            for (int k = 0; k < processNum.Length; k++)
            {
                parent1[k] = genData[gensize1, processNum.Length + k];
                parent2[k] = genData[gensize2, processNum.Length + k];
            }
            Array.Fill(child1, -1);
            Array.Fill(child2, -1);

            //����p1�����ڹ�����J1 �й����Ĺ���c1������p2�����ڹ�����J2�й����Ĺ���c2���������ǵ�λ�ã�
            for (int k = 0; k < processNum.Length; k++)
            { 
                for(int m = 0; m < code1.Length; m++)
                {
                   if(parent1[k] == code1[m])
                   {
                        child1[k] = parent1[k];
                        break;
                   }
                }
                for (int m = 0; m < code2.Length; m++)
                {
                    if (parent2[k] == code2[m])
                    {
                        child2[k] = parent2[k];
                        break;
                    }
                }
            }
            //����p1�����ڹ�����J2 �й����Ĺ���c2������p2�����ڹ�����J2 �й����Ĺ���c1���������ǵ�˳��
            int index_p1 = 0;
            int index_p2 = 0;
            for (int k = 0; k < processNum.Length; k++)
            {
                if (child1[k] < 0)
                {
                    for (int m = index_p1; m < processNum.Length; m++)
                    {
                        bool break_falg = false;
                        for (int j = 0; j < code2.Length; j++)
                        { 
                            if(parent2[m] == code2[j])
                            {
                                child1[k] = parent2[m];
                                index_p1 = m + 1;
                                break_falg = true;
                                break;
                            }
                        }
                        if(break_falg == true)
                            break;
                    }
                }
                if (child2[k] < 0)
                {
                    for (int m = index_p2; m < processNum.Length; m++)
                    {
                        bool break_falg = false;
                        for (int j = 0; j < code1.Length; j++)
                        {
                            if (parent1[m] == code1[j])
                            {
                                child2[k] = parent1[m];
                                index_p2 = m + 1;
                                break_falg = true;
                                break;
                            }
                        }
                        if (break_falg == true)
                            break;
                    }
                }
            }

            //�����븳ֵ
            for (int k = 0; k < processNum.Length; k++)
            {
                childx0[gensize1, processNum.Length + k] = child1[k];
                childx0[gensize2, processNum.Length + k] = child2[k];
            }

            for (int k = 0; k < processNumTotal * 3; k++)
            {
                genData[alternate_position,k] = childx0[gensize1,k];
                genData[alternate_position+1,k] = childx0[gensize2, k];
            }

            alternate_position += 2;
        }

        gensize_after = genData;
    }

    public void Mutation(int gensize, int[,] genData, out int[,] gensize_after)
    {
        for (int i = 0; i < gensize - 1; i++)
        {
            //���������
            double index1 = UnityEngine.Random.Range(0.0f, 1.0f);//���ؽ��� min [��] �� max [����] 
            if (index1 <= MutationProbability)
            {
                int index2 = UnityEngine.Random.Range(0, processNumTotal);//���α���Ļ�����
                int index3 = 0;


                //������Ż�����
                int[] effect_machine;
                for (int j = 1; j < machineNum; j++)
                {
                    if (processOptMachineTime[index2, index3] > processOptMachineTime[index2, j])//ѡȡ��ʱ���ٵĻ���
                    {
                        index3 = j;
                    }
                }

                //ѡ�����
                if(!open_improve)
                    index3 = UnityEngine.Random.Range(0, machineNum);

                genData[i, index2] = index3;
            }

            //���������
            index1 = UnityEngine.Random.Range(0.0f, 1.0f);
            if (index1 <= MutationProbability)
            {
                int index2 = UnityEngine.Random.Range(processNumTotal, processNumTotal * 2 - 1);//���ν�������Ļ�������ʼ
                int index3 = UnityEngine.Random.Range(index2, processNumTotal * 2);//���ν�������Ļ��������

                int temp = genData[i, index2];
                genData[i, index2] = genData[i, index3];
                genData[i, index3] = temp;
            }
        }
        gensize_after =  genData;
    }

    public void Reserve(int gensize, int[,] genData, out int[,] gensize_after,int index_min,double[] p_fit)
    {

        //��Ӣ�������滻�����һ��Ⱦɫ��
        for (int i = 0; i < processNumTotal * 3; i++)
        {
            genData[gensize - 1, i] = genData[index_min, i];
        }

        //����������
        int Reserve_count = (int)(gensize * ReserveRange - 1);
        int Every_choonse_num = (int)(gensize / Reserve_count);
        double[] fit = new double[Every_choonse_num]; 
        int alternate_position = 1;
        for (int i = 0; i < Reserve_count; i++)
        {
            for (int j = 0; j < Every_choonse_num; j++)
            {
                fit[j] = p_fit[i * Every_choonse_num + j];
            }
            int fit_min = fit.ToList().IndexOf(fit.Min());
            int reserveNum = i * Every_choonse_num + fit_min;

            for (int j = 0; j < processNumTotal * 3; j++)
            {
                genData[gensize - 1 - alternate_position, j] = genData[reserveNum, j];
            }
            alternate_position++;
        }

        gensize_after = genData;
    }
    /// <summary>
    /// ���Ƹ���ͼ
    /// </summary>
    /// <param name="drawData">������+����������</param>
    /// <param name="p_fit">��Ӧ��ֵ</param>
    public void DrawGante(int[] drawData, int index, double p_fit,int iter_min)
    {
        //ͼ����ʾ��ʼ��
        var chart = canvas.GetComponent<BarChart>();
        if (chart == null)
        {
            chart = canvas.AddComponent<BarChart>();
            chart.Init();
        }
        //chart.SetSize(1500, 600);
        var title = chart.EnsureChartComponent<Title>();
        title.text = "���ȸ���ͼ";//:,���εõ���̼ӹ�ʱ�䣺" + p_fit;
        var tooltip = chart.EnsureChartComponent<Tooltip>();
        tooltip.show = true;

        var legend = chart.EnsureChartComponent<Legend>();
        legend.show = false;
        var xAxis = chart.EnsureChartComponent<XAxis>();
        xAxis.splitNumber = 0;
        xAxis.boundaryGap = true;
        xAxis.type = Axis.AxisType.Category;

        var yAxis = chart.EnsureChartComponent<YAxis>();
        yAxis.type = Axis.AxisType.Value;
        chart.RemoveData();
        chart.AddSerie<Candlestick>("Candlestick");
        for (int i = 0; i < machineNum; i++)
        {
            chart.AddXAxisData("M" + i);
        }
        for (int i = 0; i < AGVNum; i++)
        {
            chart.AddXAxisData("AGV"+i);
        }


        //0����������������¼������
        for (int i = 0; i < machineNum + AGVNum; i++)
        {
            chart.AddData(0, 0, 0, 0, 0);
        }
        int count_data = 1;//��������,ÿ���ۼӣ�����ͬ��



        int totalProcessNum = processNumTotal;//��������
        int[] Mij = new int[totalProcessNum];//�����Ӧ�ӹ���������
        double[] Tij = new double[totalProcessNum];//�����Ӧ�����ӹ�ʱ�伯��
        double[,] MachineEmptyTime = new double[machineNum * 2, totalProcessNum * 2];//�����ӹ�����ʱ�䡾����1����ʱ����ʼ������1����ʱ�����.....�������ڼ���Ƿ����̰���㷨
        int[] MachineEmptyTime_index = new int[machineNum * 2];
        for (int j = 0; j < totalProcessNum; j++)
        {
            Mij[j] = drawData[j];//�����Ӧ����            
            Tij[j] = processOptMachineTime[j, Mij[j]];//ÿһ�������Ӧʱ��
        }
        int[] Oij = new int[workpieceNumber];//�����ӹ��������
        for (int k = 0; k < Oij.Length; k++)
        {
            Oij[k] = 0;
        }
        double[] Tmi = new double[machineNum];//����ʱ�����
        for (int k = 0; k < Tmi.Length; k++)
        {
            Tmi[k] = 0;
        }
        double[] Tpi = new double[workpieceNumber];//�����ӹ�ʱ�����,�������Ź������ʼʱ��
        for (int k = 0; k < Tpi.Length; k++)
        {
            Tpi[k] = 0;
        }
        double[] Tagv = new double[AGVNum];//AGV����ʱ����ȼ�¼
        for (int k = 0; k < Tagv.Length; k++)
        {
            Tagv[k] = 0;
        }
        double[] Tagv_xy = new double[AGVNum * 2];//��ǰAGV���ڵص�x+y
        for (int k = 0; k < Tagv_xy.Length; k++)
        {
            Tagv_xy[k] = 0;
        }
        //��ʼ���żӹ�����
        for (int j = totalProcessNum; j < totalProcessNum * 2; j++)
        {
            //������
            int Oi = drawData[j];//��ǰѡ��Ĺ���

            //������
            int k = 0;//������ʼ����
            for (int m = 0; m < Oi; m++)
            {
                k = k + processNum1[m];
            }
            int Mi = Mij[k + Oij[Oi]];//�����������Ļ������,0,1,2

            //AGV��
            int Ai = drawData[totalProcessNum + j];

            double AGVworktime = cal_AGVworktime(Tagv_xy[Ai], Tagv_xy[Ai + AGVNum], Mi);//AGV����ʱ��

            var serieData1 = Tmi[Mi];//��ͼ��¼����1
            var serieData2 = Tmi[Mi];//��ͼ��¼����2
            var serieData3 = Tpi[Oi] < Tagv[Ai] ? Tagv[Ai] : Tpi[Oi];//��ͼ��¼����3

            //���ݹ����żӹ�ʱ��
            if (false)//(Oij[Oi] == 0) //��һ������,������ǰ�ù���
            {
                //Tagv[Ai] = (Tagv[Ai] + AGVworktime) < Tmi[Mi] ? Tmi[Mi] : (Tagv[Ai] + AGVworktime);//AGV����ʱ��,�������ʱ�� С�� �����ɼӹ�ʱ�� ��Ȼ����ӹ���
                //Tagv_xy = update_AGVxy(Tagv_xy, Ai, Mi);//����AGVλ��
                //serieData1 = Tagv[Ai];
                //Tmi[Mi] = Tagv[Ai] + Tij[k + Oij[Oi]];//�������Ź��� k + Oij[Oi] ����+�������=�ù�������ʱ�䣬��Mi�Ż����Ľ�����ǰ�Ƹù���
                //Tpi[Oi] = Tmi[Mi];//����ʱ�����

                //serieData2 = Tmi[Mi];
            }
            else
            {
                double Tagv_Tpi = Tagv[Ai] < Tpi[Oi] ? (Tpi[Oi] + AGVworktime) : (Tagv[Ai] + AGVworktime);//�õ�����AGV���ù������͵�Ŀ��λ������ʱ��,����AGV�Ƿ�������
                if (Tagv_Tpi < Tmi[Mi])//�������͵�Ŀ�������ʱ��С����ѡ��������ɼӹ�ʱ�䣬�ȴ������ӹ����ֱ�Ӽӹ��ù���
                {
                    bool is_use_greed_time = false;
                    //----�ж��Ƿ����̰������
                    double greed_time = Tagv_Tpi + Tij[k + Oij[Oi]];
                    if (open_greed && greed_time < Tmi[Mi])//����У�飬�ʹ�ʱ�䣫�ӹ�ʱ���Ƿ�С�ڻ�����ʼʱ��
                    {
                        for (int met = 0; met < MachineEmptyTime_index[Mi]; met++)
                        {
                            if(greed_time < MachineEmptyTime[Mi, met * 2 + 1] && is_use_greed_time == false)//̰��ʱ��С�ڿ���ʱ�����
                            {
                                double cal = MachineEmptyTime[Mi, met * 2 + 1] - Tij[k + Oij[Oi]]; //���н���ʱ��-�ӹ�ʱ��
                                if (cal >= MachineEmptyTime[Mi, met * 2] && Tagv_Tpi <= cal)//ȷ���г����ʱ��ӹ�,���Ҽӹ�֮ǰ�������͵�������
                                {
                                    if (MachineEmptyTime[Mi, met * 2] <= Tagv_Tpi)//���������ʼʱ��С���ʹ�ʱ�䣬���ʹ�ʱ�俪ʼ�ӹ�
                                    {
                                        serieData1 = Tagv_Tpi;
                                        Tagv[Ai] = Tagv_Tpi;
                                        serieData2 = Tagv_Tpi + Tij[k + Oij[Oi]];

                                        //���¿���ʱ��
                                        MachineEmptyTime[Mi, MachineEmptyTime_index[Mi] * 2] = Tagv_Tpi + Tij[k + Oij[Oi]];
                                        MachineEmptyTime[Mi, MachineEmptyTime_index[Mi] * 2 + 1] = MachineEmptyTime[Mi, met * 2 + 1];
                                        MachineEmptyTime_index[Mi]++;

                                        MachineEmptyTime[Mi, met * 2 + 1] = Tagv_Tpi;


                                    }
                                    else//��Ȼ���ÿ�����ʼʱ�俪ʼ�ӹ�
                                    {
                                        serieData1 = MachineEmptyTime[Mi, met * 2];
                                        Tagv[Ai] = MachineEmptyTime[Mi, met * 2];
                                        serieData2 = MachineEmptyTime[Mi, met * 2] + Tij[k + Oij[Oi]];

                                        MachineEmptyTime[Mi, MachineEmptyTime_index[Mi] * 2] = MachineEmptyTime[Mi, met * 2] + Tij[k + Oij[Oi]];
                                        MachineEmptyTime[Mi, MachineEmptyTime_index[Mi] * 2 + 1] = MachineEmptyTime[Mi, met * 2 + 1];
                                        MachineEmptyTime_index[Mi]++;

                                        MachineEmptyTime[Mi, met * 2 + 1] = MachineEmptyTime[Mi, met * 2];
                                    }
                                    is_use_greed_time = true;
                                }
                            }
                        }
                    }
                    //---����̰��
                    if (is_use_greed_time == false)
                    {
                        serieData1 = Tmi[Mi];
                        Tagv[Ai] = Tmi[Mi];//AGV���ȵ��ڻ������翪��ʱ��
                        Tagv_xy = update_AGVxy(Tagv_xy, Ai, Mi);//����AGVλ��
                        Tmi[Mi] += Tij[k + Oij[Oi]];//�������Ź���
                        serieData2 = Tmi[Mi];
                    }
                }
                else //�������͵�Ŀ�������ʱ�������ѡ��������ɼӹ�ʱ�䣬�ȴ������˵����ٷŵ������ϼӹ�
                {
                    MachineEmptyTime[Mi,2 * MachineEmptyTime_index[Mi]] = Tmi[Mi];
                    serieData1 = Tagv_Tpi;
                    Tagv[Ai] = Tagv_Tpi;
                    Tagv_xy = update_AGVxy(Tagv_xy, Ai, Mi);//����AGVλ��
                    MachineEmptyTime[Mi, 2 * MachineEmptyTime_index[Mi] + 1] = Tagv_Tpi;
                    MachineEmptyTime_index[Mi]++;
                    Tmi[Mi] = Tagv_Tpi + Tij[k + Oij[Oi]];
                    serieData2 = Tmi[Mi];
                }
                Tpi[Oi] = Tmi[Mi];//����ʱ�����



            }
            var serieData4 = Tagv[Ai];//��ͼ��¼����4
            Oij[Oi] += 1;

            chart.AddSerie<Candlestick>("Candlestick");
            for (int p = 0; p < Mi; p++)
            {
                chart.AddData(count_data, 0, 0, 0, 0);//��ǰ�����״ͼ���,j - totalProcessNum + 1
            }
            var serieMacData = chart.AddData(count_data, serieData1, serieData2, serieData1, serieData2);//
            serieMacData.radius = 10;
            var itemMacStyle = serieMacData.EnsureComponent<ItemStyle>(); //�����������ItemStyle���
            itemMacStyle.color = drawColor[Oi];
            itemMacStyle.borderColor = Color.black;
            //if (Oi == 0)
            //    itemMacStyle.color = Color.blue;
            //if (Oi == 1)
            //    itemMacStyle.color = Color.yellow;
            //if (Oi == 2)
            //    itemMacStyle.color = Color.red;
            count_data++;

            chart.AddSerie<Candlestick>("Candlestick");
            for (int p = 0; p < Ai +machineNum; p++)
            {
                chart.AddData(count_data, 0, 0, 0, 0);//��ǰ�����״ͼ���
            }
            var serieMacData1 = chart.AddData(count_data, serieData3, serieData4, serieData3, serieData4);//
            serieMacData1.radius = 10;
            var itemMacStyle1 = serieMacData1.EnsureComponent<ItemStyle>(); //�����������ItemStyle���
            itemMacStyle1.color = drawColor[Oi];
            itemMacStyle1.borderColor = Color.white;
            //if (Oi == 0)
            //    itemMacStyle1.color = Color.blue;
            //if (Oi == 1)
            //    itemMacStyle1.color = Color.yellow;
            //if (Oi == 2)
            //    itemMacStyle1.color = Color.red;
            count_data++;

            //var serieData = chart.AddData(0, serieData1, serieData2, serieData1, serieData2);
            //chart.AddXAxisData("O" + Oi + Oij[Oi]);
            //serieData.radius = 10;
            //var itemStyle = serieData.EnsureComponent<ItemStyle>(); //�����������ItemStyle���
            //if (Oi == 0)
            //    itemStyle.color = Color.blue;
            //if (Oi == 1)
            //    itemStyle.color = Color.yellow;
            //if (Oi == 2)
            //    itemStyle.color = Color.red;
        }
    }
    /// <summary>
    /// ��ʼ��
    /// </summary>
    /// <param name="gensize">��Ⱥ��С</param>
    /// <param name="workpieceNumber">��������</param>
    ///  <param name="machineNum">��������</param>
    /// <param name="processNum">������Ӧ��������</param>
    public void Initialize(int gensize, int workpieceNumber,int machineNum, int[] processNum,int[] processOptNum,out int[,] genData)
    {
        System.Random rand = new System.Random();
        int totalProcessNum = processNum.Length;//��������
        int[,] x0 = new int[gensize, totalProcessNum * 3];

        for (int i = 0; i < gensize; i++)
        {
            int[] process = new int[totalProcessNum];
            Array.Copy(processNum, process, processNum.Length);
            for (int j = 0; j < totalProcessNum * 3; j++)
            {
                if (j < totalProcessNum)//������
                {
                    x0[i, j] = rand.Next(0, processOptNum[j]);
                }
                if (j >= totalProcessNum && j < totalProcessNum * 2)//������
                {
                    int randNum = rand.Next(0, process.Length - 1);
                    x0[i, j] = process[randNum];
                    process[randNum] = process[process.Length - 1];
                    Array.Resize(ref process, process.Length - 1);
                }
                if(j >= totalProcessNum * 2)
                {
                    x0[i, j] = rand.Next(0, AGVNum);
                }
            }
        }
        genData = x0;
    }

    // Start is called before the first frame update
    public void StartGENFunction(int expNum)
    {
        InitGen();
        int[,] genData = new int[gensize, processNumTotal * 3];//������+������+AGV��
        double[] p_fit = new double[gensize];
        drawData = new int[processNumTotal * 3];
        double p_fit_min = 9999999;
        double[] p_fit_min_total = new double[iteration];
        int index = 0;
        int iter_min = 0;
        Initialize(gensize, workpieceNumber, machineNum, processNum, processOptNum,out genData);
        //��ʼ����
        for (int i = 0; i < iteration; i++)
        {
            Reserve(gensize, genData, out genData,index,p_fit);//����
            Cross(gensize, genData, processNum, out genData);//����
            Mutation(gensize, genData, out genData);//����
            EnCode(gensize, genData, processNum, out p_fit);//����
            index = p_fit.ToList().IndexOf(p_fit.Min());    //�ڸ�����ʷ��Ӧ�����ҵ���Сֵ��������
            if (p_fit_min > p_fit.Min())
            {
                iter_min = i;
                p_fit_min = p_fit.Min();
                for (int j = 0; j < drawData.Length; j++)
                {
                    drawData[j] = genData[index, j];
                }
            }
            p_fit_min_total[i] = p_fit.Min();
            if (SaveTotalData)
                Save_Every_Generation_Data(i,index, p_fit.Min(),p_fit_min, genData);
        }
        if (SaveEveryFitMinData)
            Save_Every_Fit_Min_Data(p_fit_min_total);
        if (SaveEveryExperimentsData)
            Save_Every_Experiments_Data(expNum,index, p_fit.Min(), p_fit_min, genData);
        Debug.Log("p_fit_min:" + p_fit.Min());


        ////����
        //for (int i = 0; i < 36; i++)
        //{
        //    int k = i % 6;
        //    switch (k)
        //    {
        //        case 0:
        //            drawData[i] = 0;
        //            break;
        //        case 1:
        //            drawData[i] = 2;
        //            break;
        //        case 2:
        //            drawData[i] = 4;
        //            break;
        //        case 3:
        //            drawData[i] = 6;
        //            break;
        //        case 4:
        //            drawData[i] = 9;
        //            break;
        //        case 5:
        //            drawData[i] = 12;
        //            break;
        //    }

        //}
        //for (int i = 36; i < 61; i++)
        //{
        //    int k = (i-36) % 5;
        //    switch (k)
        //    {
        //        case 0:
        //            drawData[i] = 15;
        //            break;
        //        case 1:
        //            drawData[i] = 18;
        //            break;
        //        case 2:
        //            drawData[i] = 13;
        //            break;
        //        case 3:
        //            drawData[i] = 21;
        //            break;
        //        case 4:
        //            drawData[i] = 22;
        //            break;
        //    }

        //}

        //int[] drawdatatest = { 1, 0, 5, 2, 5, 6, 0, 4, 2, 5, 4, 5, 2, 1, 5, 4, 6, 2, 3, 7, 2, 7, 4, 3, 1, 4, 4, 4, 0, 6, 7, 5, 4, 7, 6, 3, 2, 5, 4, 1, 1, 3, 0, 1, 5, 4, 0, 7, 2, 1, 6, 2, 3, 7, 5, 3, 0, 0, 2, 0, 7, 2, 2, 1, 7, 4, 5, 5, 0, 3, 3, 4, 3, 1, 0, 5, 4, 6, 0, 7, 7 };
        //for (int i = 0; i < 81; i++)
        //{
        //    drawData[i] = drawdatatest[i];
        //}
        string shortarrayString = "drawdata:";
        for (int i = 0; i < 81; i++)
        {
            shortarrayString  = shortarrayString +  drawData[i] + ",";
        }

        print(shortarrayString);
        DrawGante(drawData, index, p_fit_min, iter_min);
        ///��������

    }
    void InitGen()
    {
        //��ʼ����ֵ
        machineDistance = new double[machineNum*2];
        for (int i = 0; i < machinesPlaceInformation.Count(); i++)
        {
            machineDistance[i] = machinesPlaceInformation[i].transform.position.x;
            machineDistance[machineNum + i] = machinesPlaceInformation[i].transform.position.z;
        }
        processNum = new int[processNumTotal];
        int num = 0;
        for (int i = 0; i < processNum1.Length; i++)
        {
            for (int j = 0; j < processNum1[i]; j++)
            {
                processNum[num] = i;
                num++;
            }
        }
        processOptNum = new int[processNumTotal];
        for (int i = 0; i < processOptNum.Length; i++)
        {
            processOptNum[i] = machineNum;
        }
        //processOptMachineTime = new double[processNumTotal, machineNum];
        for (int i = 0; i < processNumTotal; i++)
        {
            for (int j = 0; j < machineNum; j++)
            {
                if(processOptMachineTime[i, j] == 9999)
                    processOptMachineTime[i,j] = Punishment_set;//�����Ӧ�ӹ�ʱ���ʼ��
            }
        }
        //for (int i = 0; i < pieceMachineTime.Count(); i++)
        //{
        //    int startNum = 0;//������ʼ���
        //    for (int j = 0; j < i; j++)
        //    {
        //        startNum += processNum1[j];
        //    }
        //    for (int k = 0; k < pieceMachineTime[i].pieceProcess.Count(); k++)
        //    {
        //        int endNum = startNum +k;
        //        for (int m = 0; m < pieceMachineTime[i].pieceProcess[k].machineTimes.Count(); m++)
        //        {
        //            int machineNum = pieceMachineTime[i].pieceProcess[k].machineTimes[m].machineNum;
        //            double workTime = pieceMachineTime[i].pieceProcess[k].machineTimes[m].workTime;
        //            processOptMachineTime[endNum, machineNum] = workTime;
        //        }
        //    }
        //}
    }
    private string Generate_Experimental_Tables(string path, string name, int iteration, int gensize, int workpieceNumber, int machineNum, int AGVNum, int processNumTotal, float AGVSpeed, int CrossCount_min, int CrossCount_max,double CrossRange,double MutationProbability)
    {

        path = Application.dataPath + path + name + DateTime.UtcNow.Ticks + ".xlsx";
        //path = path + name + DateTime.UtcNow.Ticks + ".xls";
        Debug.Log(path);
        FileInfo _excelName = new FileInfo(path);
        if (_excelName.Exists)
        {
            //ɾ�����ļ���������һ���µ� excel �ļ���
            _excelName.Delete();
            _excelName = new FileInfo(path);
        }
        using (ExcelPackage package = new ExcelPackage(_excelName))
        {
            //�� excel ���ļ������ sheet������������

                ExcelWorksheet worksheet0 = package.Workbook.Worksheets.Add("GEN");
                worksheet0.Cells[2, 1].Value = "iteration:" + iteration;
                worksheet0.Cells[2, 1].Value = "gensize:" + gensize;
                worksheet0.Cells[2, 1].Value = "workpieceNumber:" + workpieceNumber;
                worksheet0.Cells[2, 1].Value = "machineNum:" + machineNum;
                worksheet0.Cells[2, 1].Value = "AGVNum:" + AGVNum;
                worksheet0.Cells[2, 1].Value = "processNumTotal:" + processNumTotal;
                worksheet0.Cells[2, 1].Value = "AGVSpeed:" + AGVSpeed;
                worksheet0.Cells[2, 1].Value = "CrossCount_min:" + CrossCount_min;
                worksheet0.Cells[2, 1].Value = "CrossCount_max:" + CrossCount_max;
                worksheet0.Cells[2, 1].Value = "CrossRange:" + CrossRange;
                worksheet0.Cells[2, 1].Value = "MutationProbability:" + MutationProbability;

                worksheet0.Cells[2, 1].Value = "����/ʵ�����";
                worksheet0.Cells[2, 2].Value = "�������Ⱦɫ����";
                worksheet0.Cells[2, 3].Value = "���������Ӧ��";
                worksheet0.Cells[2, 4].Value = "��ʷ�����Ӧ��";
                for (int i = 0; i < processNumTotal * 3; i++)
                {
                    worksheet0.Cells[2, 5 + i].Value = i;
                }
            

            package.Save();
        }

        return path;
    }
    private void Save_Every_Experiments_Data(int i, int index, double p_fit_min, double p_fit_min_all, int[,] data)
    {
        using (ExcelPackage package = new ExcelPackage(_excelName))
        {
            //�� excel ���ļ������ sheet������������
            ExcelWorksheet worksheet = package.Workbook.Worksheets["GEN"];

            worksheet.Cells[3 + i, 1].Value = i;
            worksheet.Cells[3 + i, 2].Value = index;
            worksheet.Cells[3 + i, 3].Value = p_fit_min;
            worksheet.Cells[3 + i, 4].Value = p_fit_min_all;
            for (int k = 0; k < processNumTotal * 3; k++)
            {
                worksheet.Cells[3 + i, 5 + k].Value = data[index, k];
            }
            package.Save();
        }
    }
    private void Save_Every_Generation_Data(int i,int index,double p_fit_min ,double p_fit_min_all,int[,] data)
    {
        using (ExcelPackage package = new ExcelPackage(_excelName))
        {
            //�� excel ���ļ������ sheet������������
            ExcelWorksheet worksheet = package.Workbook.Worksheets["GEN"];

            worksheet.Cells[3 + i, 1].Value = i;
            worksheet.Cells[3 + i, 2].Value = index;
            worksheet.Cells[3 + i, 3].Value = p_fit_min;
            worksheet.Cells[3 + i, 4].Value = p_fit_min_all;
            for (int k = 0; k < processNumTotal * 3; k++)
            {
                worksheet.Cells[3 + i, 5 + k].Value = data[index, k];
            }
            package.Save();
        }
    }
    private void Save_Every_Fit_Min_Data(double[] fit_min)
    {
        using (ExcelPackage package = new ExcelPackage(_excelName))
        {
            //�� excel ���ļ������ sheet������������
            ExcelWorksheet worksheet = package.Workbook.Worksheets["GEN"];

            for (int i = 0; i < iteration; i++)
            {
                worksheet.Cells[3+i, 3].Value =fit_min[i].ToString("f4");
            }
            package.Save();
        }
    }

    void Start()
    {
        if (SaveTotalData || SaveEveryFitMinData || SaveEveryExperimentsData)
        {
            Excelpath = Generate_Experimental_Tables(DataExportPath, DataExportName, iteration, gensize, workpieceNumber, machineNum, AGVNum, processNumTotal, AGVSpeed, CrossCount_min, CrossCount_max, CrossRange, MutationProbability);  //����ʵ�������ļ�
            _excelName = new FileInfo(Excelpath);
        }
        for (int i = 0; i < ExperimentsNum; i++)
        {
            StartGENFunction(i);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
