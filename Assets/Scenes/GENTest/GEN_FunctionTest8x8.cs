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
    public string DataExportPath = "/Scenes/GENTest/Data/";  //实验数据文件夹保存路径
    public string DataExportName = "nof";             //实验数据文件夹名称
    public bool SaveTotalData = false;                  //保存每一次迭代数据
    public bool SaveEveryFitMinData = true;         //是否保存每一次迭代适应度最低值
    public bool SaveEveryExperimentsData = true;         //是否保存每一次迭代适应度最低值

    public bool open_greed = true;
    public bool open_improve = true;

    public int ExperimentsNum = 1;
    public int iteration = 1000;//迭代次数
    public int gensize = 50;//种群大小
    public int workpieceNumber = 8;//3;//工件数量
    public int machineNum = 8;//3;//机器数量
    public int AGVNum = 2;//3;//AGV数量
    public int processNumTotal = 27;//6;//工序总量
    public float AGVSpeed = 1f;
    public int[] drawData;
    public Color[] drawColor;
    public GameObject canvas;
    private double Punishment_set = 999;


    public int CrossCount_min = 1;
    public int CrossCount_max = 5;
    public double CrossRange = 0.5;
    public double ReserveRange = 0.2; //保留数量
    public double MutationProbability = 0.1;
    private int[] processNum1 = { 3,4,3,3,4,3,3,4};//{ 2, 2, 2 };//工件对应工序数量
    private int[] processNum;//= { 0, 0, 1, 1, 2, 2 };//工序展开便于初始化
    private int[] processOptNum;// = { 3, 3, 3, 3, 3, 2 };//工序可选择的机器数量
    //public int[,] processOptMachine = { { 0, 1, 2 }, { 0, 1, 2 }, { 0, 1, 2 }, { 0, 1, 2 }, { 0, 1, 2 }, { 0, 2, 1 } };//工序选择机器对应机器编号
    public double[,] processOptMachineTime= { { 5, 3, 5, 3, 3, 9999, 10, 9 }, { 10, 9999, 5, 8, 3, 9, 9, 6 }, { 9999, 10, 9999, 5, 6, 2, 4, 5 }, { 5, 7,3,9,8,9999,9,9999 },{ 9999,8,5,2,6,7,10,9},{ 9999,10,9999,5,6,4,1,7},{ 10,8,9,6,4,7,9999,9999},{ 10,9999,9999,7,6,5,2,4},{ 9999,10,6,4,8,9,10,9999},{ 1,4,5,6,9999,10,9999,7},{ 3,1,6,5,9,7,8,4},{ 12,11,7,8,10,5,6,9},{ 4,6,2,10,3,9,5,7},{ 3,6,7,8,9,9999,10,9999},{ 10,7,9999,4,9,8,6,9999},{ 9999,9,8,7,4,2,7,9999},{ 11,9,9999,6,7,5,3,6},{ 6,7,1,4,6,9,9999,9999},{ 11,9999,9,9,9,7,8,10},{ 10,5,9,10,11,9999,10,4},{ 5,4,2,6,7,9999,10,9999},{ 9999,9,9999,9,11,9,10,5},{ 9999,8,9,3,8,6,9999,10},{ 2,8,5,9,9999,4,9999,8},{ 7,4,7,8,9,9999,10,9999},{ 9,9,9999,8,5,6,7,1},{ 9,9999,3,7,1,5,8,9999} };//工序选择机器所需时间
    private double[] machineDistance;//= {1, 2, 4, 1, 2, 4};//设备间的距离，用于计算AGVworktime
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
        double distance = Math.Abs(AGV_x - machineDistance[endMachine_num]) + Math.Abs(AGV_y - machineDistance[endMachine_num + machineNum]);//计算设备间需要走的距离
        double output = distance / AGVSpeed;
        return output;
    }
    /// <summary>
    /// 更新agv位置
    /// </summary>
    /// <param name="Tagv_xy">agv数据</param>
    /// <param name="Ai">agv编号</param>
    /// <param name="Mi">机器编号</param>
    /// <returns></returns>
    public double[] update_AGVxy(double[] Tagv_xy, int Ai, int Mi)
    {
        Tagv_xy[Ai] = machineDistance[Mi];
        Tagv_xy[Ai + AGVNum] = machineDistance[Mi + machineNum];
        return Tagv_xy;
    }

    /// <summary>
    /// 解码
    /// </summary>
    /// <param name="gensize"></param>
    /// <param name="genData"></param>
    /// <param name="processNum"></param>
    /// <param name="p_fit"></param>
    public void EnCode(int gensize, int[,] genData, int[] processNum, out double[] p_fit)
    {
        double[] p_fitness = new double[gensize];
        int totalProcessNum = processNum.Length;//工序总数

        for (int i = 0; i < gensize; i++)
        {
            int[] Mij = new int[totalProcessNum];//工序对应加工机器集合
            double[] Tij = new double[totalProcessNum];//工序对应机器加工时间集合
            double[,] MachineEmptyTime = new double[machineNum * 2, totalProcessNum * 2];//机器加工空余时间【机器1空余时间起始，机器1空余时间结束.....】，用于检测是否符合贪婪算法
            int[] MachineEmptyTime_index = new int[machineNum * 2];
            for (int j = 0; j < totalProcessNum; j++)
            {
                Mij[j] =  genData[i, j];//工序对应机器            
                Tij[j] = processOptMachineTime[j, Mij[j]];//每一道工序对应时间
            }
            int[] Oij = new int[workpieceNumber];//工件加工工序进度
            for (int k = 0; k < Oij.Length; k++)
            {
                Oij[k] = 0;
            }
            double[] Tmi = new double[machineNum];//机器时间进度
            for (int k = 0; k < Tmi.Length; k++)
            {
                Tmi[k] = 0;
            }
            double[] Tpi = new double[workpieceNumber];//工件加工时间进度,用来安排工序的起始时间
            for (int k = 0; k < Tpi.Length; k++)
            {
                Tpi[k] = 0;
            }
            double[] Tagv = new double[AGVNum];//AGV调度时间进度记录
            for (int k = 0; k < Tagv.Length; k++)
            {
                Tagv[k] = 0;
            }
            double[] Tagv_xy = new double[AGVNum * 2];//当前AGV所在地点x+y
            for (int k = 0; k < Tagv_xy.Length; k++)
            {
                Tagv_xy[k] = 0;
            }

            //开始安排加工流程
            for (int j = totalProcessNum; j < totalProcessNum * 2; j++)
            {
                //工序码
                int Oi = genData[i, j];//当前选择的工件

                //机器码
                int k = 0;//工件起始索引
                for (int m = 0; m < Oi; m++)
                {
                    k = k + processNum1[m];
                }                
                int Mi = Mij[k + Oij[Oi]];//工序索引到的机器编号,0,1,2

                //AGV码
                int Ai = genData[i, totalProcessNum + j];

                double AGVworktime = cal_AGVworktime(Tagv_xy[Ai], Tagv_xy[Ai + AGVNum], Mi);//AGV运行时间

                //根据工序安排加工时间
                if (false) //第一道工序,不考虑前置工序
                {
                    //Tagv[Ai] = (Tagv[Ai] + AGVworktime) < Tmi[Mi] ? Tmi[Mi] : (Tagv[Ai] + AGVworktime);//AGV运送时间,如果运送时间 小于 机器可加工时间则等机器加工完
                    //Tagv_xy = update_AGVxy(Tagv_xy, Ai, Mi);//更新AGV位置

                    //Tmi[Mi] = Tagv[Ai] + Tij[k + Oij[Oi]];//机器安排工作 k + Oij[Oi] 工件+工序进度=该工序所需时间，第Mi号机器的进度往前推该工序
                    //Tpi[Oi] = Tmi[Mi];//工件时间进度
                }
                else
                {
                    double Tagv_Tpi = Tagv[Ai] < Tpi[Oi] ? (Tpi[Oi] + AGVworktime) : (Tagv[Ai] + AGVworktime);//该道工序AGV将该工件运送到目标位置所需时间,根据AGV是否空余决定
                    if (Tagv_Tpi < Tmi[Mi])//工件运送到目标机器的时间小于所选机器最早可加工时间，等待机器加工完后直接加工该工件
                    {
                        bool is_use_greed_time = false;
                        //----判断是否符合贪婪方法
                        double greed_time = Tagv_Tpi + Tij[k + Oij[Oi]];
                        if (open_greed && greed_time < Tmi[Mi])//初步校验，送达时间＋加工时间是否小于机器起始时间
                        {
                            for (int met = 0; met < MachineEmptyTime_index[Mi]; met++)
                            {
                                if (greed_time < MachineEmptyTime[Mi, met * 2 + 1] && is_use_greed_time == false)//贪婪时间小于空闲时间结束
                                {
                                    double cal = MachineEmptyTime[Mi, met * 2 + 1] - Tij[k + Oij[Oi]]; //空闲结束时间-加工时间
                                    if (cal >= MachineEmptyTime[Mi, met * 2] && Tagv_Tpi <= cal)//确保有充足的时间加工,并且加工之前工件已送到并待命
                                    {
                                        if (MachineEmptyTime[Mi, met * 2] <= Tagv_Tpi)//如果空余起始时间小于送达时间，以送达时间开始加工
                                        {
                                            Tagv[Ai] = Tagv_Tpi;

                                            //更新空余时间
                                            MachineEmptyTime[Mi, MachineEmptyTime_index[Mi] * 2] = Tagv_Tpi + Tij[k + Oij[Oi]];
                                            MachineEmptyTime[Mi, MachineEmptyTime_index[Mi] * 2 + 1] = MachineEmptyTime[Mi, met * 2 + 1];
                                            MachineEmptyTime_index[Mi]++;

                                            MachineEmptyTime[Mi, met * 2 + 1] = Tagv_Tpi;


                                        }
                                        else//不然则用空余起始时间开始加工
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
                        //---结束贪婪
                        if (is_use_greed_time == false)
                        {
                            Tagv[Ai] = Tmi[Mi];//AGV进度等于机器最早开工时间
                            Tagv_xy = update_AGVxy(Tagv_xy, Ai, Mi);//更新AGV位置
                            Tmi[Mi] += Tij[k + Oij[Oi]];//机器安排工作
                        }
                    }
                    else //工件运送到目标机器的时间大于所选机器最早可加工时间，等待工件运到后再放到机器上加工
                    {
                        MachineEmptyTime[Mi, 2 * MachineEmptyTime_index[Mi]] = Tmi[Mi];
                        Tagv[Ai] = Tagv_Tpi;
                        Tagv_xy = update_AGVxy(Tagv_xy, Ai, Mi);//更新AGV位置
                        MachineEmptyTime[Mi, 2 * MachineEmptyTime_index[Mi] + 1] = Tagv_Tpi;
                        MachineEmptyTime_index[Mi]++;
                        Tmi[Mi] = Tagv_Tpi + Tij[k + Oij[Oi]];
                    }
                    Tpi[Oi] = Tmi[Mi];//工件时间进度

                }
                Oij[Oi] += 1;
            }
            p_fitness[i] = Tmi.Max();
            //Debug.Log("本次结果：" + p_fitness[i]);
        }
        p_fit = p_fitness;
    }
    /// <summary>
    /// 交叉
    /// </summary>
    /// <param name="gensize">种群大小</param>
    /// <param name="genData">数据</param>
    /// <param name="processNum">工序</param>
    /// <param name="gensize_after">输出数据</param>
    public void Cross(int gensize, int[,] genData, int[] processNum, out int[,] gensize_after)
    {
        int[,] childx0 = new int[gensize, processNum.Length * 3];
        Array.Copy(genData, childx0, genData.Length);

        int cross_count = (int)(gensize * CrossRange);//交叉总数
        int alternate_position = 0;

        for (int i = 0; i < cross_count/2; i++)
        {
            //本次交叉的两个基因
            int gensize1 = UnityEngine.Random.Range(0, gensize - 1);
            int gensize2 = UnityEngine.Random.Range(gensize1, gensize - 1);
            //机器码交叉
            int index1 = UnityEngine.Random.Range(0, processNum.Length - 1);//本次交换基因的机器码起始
            int index2 = UnityEngine.Random.Range(index1, processNum.Length);//本次交换基因的机器码结束
            for (int j = index1; j <= index2; j++)
            {
                int temp = childx0[gensize1, j];
                childx0[gensize1, j] = childx0[gensize2, j];
                childx0[gensize2, j] = temp;
            }
            //工序码交叉
            int[] code1 = new int[workpieceNumber];//J1选择出的工件
            int[] code2 = new int[workpieceNumber];//J2选择出的工件
            for (int k = 0; k < workpieceNumber; k++)
            {
                code1[k] = k;
            }
            int index3 = UnityEngine.Random.Range(1, workpieceNumber);//交叉个数
            Array.Resize(ref code2, index3);
            for (int k = 0; k < index3; k++)//选择交叉的工件
            {
                int index4 = UnityEngine.Random.Range(0, code1.Length);//交叉个数
                code2[k] = code1[index4];
                code1[index4] = code1[code1.Length - 1];
                Array.Resize(ref code1, code1.Length - 1);
            }

            //进行交叉
            int[] parent1 = new int[processNum.Length];//父代P1
            int[] parent2 = new int[processNum.Length];//父代CP2
            int[] child1 = new int[processNum.Length];//子代C1
            int[] child2 = new int[processNum.Length];//子代C2
            //子代、父代赋值
            for (int k = 0; k < processNum.Length; k++)
            {
                parent1[k] = genData[gensize1, processNum.Length + k];
                parent2[k] = genData[gensize2, processNum.Length + k];
            }
            Array.Fill(child1, -1);
            Array.Fill(child2, -1);

            //复制p1中属于工件集J1 中工件的工序到c1，复制p2中属于工件集J2中工件的工序到c2，保留它们的位置；
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
            //复制p1中属于工件集J2 中工件的工序到c2，复制p2中属于工件集J2 中工件的工序到c1，保留它们的顺序。
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

            //工序码赋值
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
            //机器码变异
            double index1 = UnityEngine.Random.Range(0.0f, 1.0f);//返回介于 min [含] 与 max [不含] 
            if (index1 <= MutationProbability)
            {
                int index2 = UnityEngine.Random.Range(0, processNumTotal);//本次变异的机器码
                int index3 = 0;


                //随机数优化变异
                int[] effect_machine;
                for (int j = 1; j < machineNum; j++)
                {
                    if (processOptMachineTime[index2, index3] > processOptMachineTime[index2, j])//选取用时最少的机器
                    {
                        index3 = j;
                    }
                }

                //选择变异
                if(!open_improve)
                    index3 = UnityEngine.Random.Range(0, machineNum);

                genData[i, index2] = index3;
            }

            //工序码变异
            index1 = UnityEngine.Random.Range(0.0f, 1.0f);
            if (index1 <= MutationProbability)
            {
                int index2 = UnityEngine.Random.Range(processNumTotal, processNumTotal * 2 - 1);//本次交换基因的机器码起始
                int index3 = UnityEngine.Random.Range(index2, processNumTotal * 2);//本次交换基因的机器码结束

                int temp = genData[i, index2];
                genData[i, index2] = genData[i, index3];
                genData[i, index3] = temp;
            }
        }
        gensize_after =  genData;
    }

    public void Reserve(int gensize, int[,] genData, out int[,] gensize_after,int index_min,double[] p_fit)
    {

        //精英保留，替换掉最后一条染色体
        for (int i = 0; i < processNumTotal * 3; i++)
        {
            genData[gensize - 1, i] = genData[index_min, i];
        }

        //竞标赛保留
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
    /// 绘制甘特图
    /// </summary>
    /// <param name="drawData">机器码+工序码数据</param>
    /// <param name="p_fit">适应度值</param>
    public void DrawGante(int[] drawData, int index, double p_fit,int iter_min)
    {
        //图表显示初始化
        var chart = canvas.GetComponent<BarChart>();
        if (chart == null)
        {
            chart = canvas.AddComponent<BarChart>();
            chart.Init();
        }
        //chart.SetSize(1500, 600);
        var title = chart.EnsureChartComponent<Title>();
        title.text = "调度甘特图";//:,本次得到最短加工时间：" + p_fit;
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


        //0号数据名称用来记录工序安排
        for (int i = 0; i < machineNum + AGVNum; i++)
        {
            chart.AddData(0, 0, 0, 0, 0);
        }
        int count_data = 1;//数据名称,每次累加，避免同名



        int totalProcessNum = processNumTotal;//工序总数
        int[] Mij = new int[totalProcessNum];//工序对应加工机器集合
        double[] Tij = new double[totalProcessNum];//工序对应机器加工时间集合
        double[,] MachineEmptyTime = new double[machineNum * 2, totalProcessNum * 2];//机器加工空余时间【机器1空余时间起始，机器1空余时间结束.....】，用于检测是否符合贪婪算法
        int[] MachineEmptyTime_index = new int[machineNum * 2];
        for (int j = 0; j < totalProcessNum; j++)
        {
            Mij[j] = drawData[j];//工序对应机器            
            Tij[j] = processOptMachineTime[j, Mij[j]];//每一道工序对应时间
        }
        int[] Oij = new int[workpieceNumber];//工件加工工序进度
        for (int k = 0; k < Oij.Length; k++)
        {
            Oij[k] = 0;
        }
        double[] Tmi = new double[machineNum];//机器时间进度
        for (int k = 0; k < Tmi.Length; k++)
        {
            Tmi[k] = 0;
        }
        double[] Tpi = new double[workpieceNumber];//工件加工时间进度,用来安排工序的起始时间
        for (int k = 0; k < Tpi.Length; k++)
        {
            Tpi[k] = 0;
        }
        double[] Tagv = new double[AGVNum];//AGV调度时间进度记录
        for (int k = 0; k < Tagv.Length; k++)
        {
            Tagv[k] = 0;
        }
        double[] Tagv_xy = new double[AGVNum * 2];//当前AGV所在地点x+y
        for (int k = 0; k < Tagv_xy.Length; k++)
        {
            Tagv_xy[k] = 0;
        }
        //开始安排加工流程
        for (int j = totalProcessNum; j < totalProcessNum * 2; j++)
        {
            //工序码
            int Oi = drawData[j];//当前选择的工件

            //机器码
            int k = 0;//工件起始索引
            for (int m = 0; m < Oi; m++)
            {
                k = k + processNum1[m];
            }
            int Mi = Mij[k + Oij[Oi]];//工序索引到的机器编号,0,1,2

            //AGV码
            int Ai = drawData[totalProcessNum + j];

            double AGVworktime = cal_AGVworktime(Tagv_xy[Ai], Tagv_xy[Ai + AGVNum], Mi);//AGV运行时间

            var serieData1 = Tmi[Mi];//画图记录数据1
            var serieData2 = Tmi[Mi];//画图记录数据2
            var serieData3 = Tpi[Oi] < Tagv[Ai] ? Tagv[Ai] : Tpi[Oi];//画图记录数据3

            //根据工序安排加工时间
            if (false)//(Oij[Oi] == 0) //第一道工序,不考虑前置工序
            {
                //Tagv[Ai] = (Tagv[Ai] + AGVworktime) < Tmi[Mi] ? Tmi[Mi] : (Tagv[Ai] + AGVworktime);//AGV运送时间,如果运送时间 小于 机器可加工时间 则等机器加工完
                //Tagv_xy = update_AGVxy(Tagv_xy, Ai, Mi);//更新AGV位置
                //serieData1 = Tagv[Ai];
                //Tmi[Mi] = Tagv[Ai] + Tij[k + Oij[Oi]];//机器安排工作 k + Oij[Oi] 工件+工序进度=该工序所需时间，第Mi号机器的进度往前推该工序
                //Tpi[Oi] = Tmi[Mi];//工件时间进度

                //serieData2 = Tmi[Mi];
            }
            else
            {
                double Tagv_Tpi = Tagv[Ai] < Tpi[Oi] ? (Tpi[Oi] + AGVworktime) : (Tagv[Ai] + AGVworktime);//该道工序AGV将该工件运送到目标位置所需时间,根据AGV是否空余决定
                if (Tagv_Tpi < Tmi[Mi])//工件运送到目标机器的时间小于所选机器最早可加工时间，等待机器加工完后直接加工该工件
                {
                    bool is_use_greed_time = false;
                    //----判断是否符合贪婪方法
                    double greed_time = Tagv_Tpi + Tij[k + Oij[Oi]];
                    if (open_greed && greed_time < Tmi[Mi])//初步校验，送达时间＋加工时间是否小于机器起始时间
                    {
                        for (int met = 0; met < MachineEmptyTime_index[Mi]; met++)
                        {
                            if(greed_time < MachineEmptyTime[Mi, met * 2 + 1] && is_use_greed_time == false)//贪婪时间小于空闲时间结束
                            {
                                double cal = MachineEmptyTime[Mi, met * 2 + 1] - Tij[k + Oij[Oi]]; //空闲结束时间-加工时间
                                if (cal >= MachineEmptyTime[Mi, met * 2] && Tagv_Tpi <= cal)//确保有充足的时间加工,并且加工之前工件已送到并待命
                                {
                                    if (MachineEmptyTime[Mi, met * 2] <= Tagv_Tpi)//如果空余起始时间小于送达时间，以送达时间开始加工
                                    {
                                        serieData1 = Tagv_Tpi;
                                        Tagv[Ai] = Tagv_Tpi;
                                        serieData2 = Tagv_Tpi + Tij[k + Oij[Oi]];

                                        //更新空余时间
                                        MachineEmptyTime[Mi, MachineEmptyTime_index[Mi] * 2] = Tagv_Tpi + Tij[k + Oij[Oi]];
                                        MachineEmptyTime[Mi, MachineEmptyTime_index[Mi] * 2 + 1] = MachineEmptyTime[Mi, met * 2 + 1];
                                        MachineEmptyTime_index[Mi]++;

                                        MachineEmptyTime[Mi, met * 2 + 1] = Tagv_Tpi;


                                    }
                                    else//不然则用空余起始时间开始加工
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
                    //---结束贪婪
                    if (is_use_greed_time == false)
                    {
                        serieData1 = Tmi[Mi];
                        Tagv[Ai] = Tmi[Mi];//AGV进度等于机器最早开工时间
                        Tagv_xy = update_AGVxy(Tagv_xy, Ai, Mi);//更新AGV位置
                        Tmi[Mi] += Tij[k + Oij[Oi]];//机器安排工作
                        serieData2 = Tmi[Mi];
                    }
                }
                else //工件运送到目标机器的时间大于所选机器最早可加工时间，等待工件运到后再放到机器上加工
                {
                    MachineEmptyTime[Mi,2 * MachineEmptyTime_index[Mi]] = Tmi[Mi];
                    serieData1 = Tagv_Tpi;
                    Tagv[Ai] = Tagv_Tpi;
                    Tagv_xy = update_AGVxy(Tagv_xy, Ai, Mi);//更新AGV位置
                    MachineEmptyTime[Mi, 2 * MachineEmptyTime_index[Mi] + 1] = Tagv_Tpi;
                    MachineEmptyTime_index[Mi]++;
                    Tmi[Mi] = Tagv_Tpi + Tij[k + Oij[Oi]];
                    serieData2 = Tmi[Mi];
                }
                Tpi[Oi] = Tmi[Mi];//工件时间进度



            }
            var serieData4 = Tagv[Ai];//画图记录数据4
            Oij[Oi] += 1;

            chart.AddSerie<Candlestick>("Candlestick");
            for (int p = 0; p < Mi; p++)
            {
                chart.AddData(count_data, 0, 0, 0, 0);//给前面的柱状图填空,j - totalProcessNum + 1
            }
            var serieMacData = chart.AddData(count_data, serieData1, serieData2, serieData1, serieData2);//
            serieMacData.radius = 10;
            var itemMacStyle = serieMacData.EnsureComponent<ItemStyle>(); //给数据项添加ItemStyle组件
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
                chart.AddData(count_data, 0, 0, 0, 0);//给前面的柱状图填空
            }
            var serieMacData1 = chart.AddData(count_data, serieData3, serieData4, serieData3, serieData4);//
            serieMacData1.radius = 10;
            var itemMacStyle1 = serieMacData1.EnsureComponent<ItemStyle>(); //给数据项添加ItemStyle组件
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
            //var itemStyle = serieData.EnsureComponent<ItemStyle>(); //给数据项添加ItemStyle组件
            //if (Oi == 0)
            //    itemStyle.color = Color.blue;
            //if (Oi == 1)
            //    itemStyle.color = Color.yellow;
            //if (Oi == 2)
            //    itemStyle.color = Color.red;
        }
    }
    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="gensize">种群大小</param>
    /// <param name="workpieceNumber">工件数量</param>
    ///  <param name="machineNum">机器数量</param>
    /// <param name="processNum">工件对应工序数量</param>
    public void Initialize(int gensize, int workpieceNumber,int machineNum, int[] processNum,int[] processOptNum,out int[,] genData)
    {
        System.Random rand = new System.Random();
        int totalProcessNum = processNum.Length;//工序总数
        int[,] x0 = new int[gensize, totalProcessNum * 3];

        for (int i = 0; i < gensize; i++)
        {
            int[] process = new int[totalProcessNum];
            Array.Copy(processNum, process, processNum.Length);
            for (int j = 0; j < totalProcessNum * 3; j++)
            {
                if (j < totalProcessNum)//机器码
                {
                    x0[i, j] = rand.Next(0, processOptNum[j]);
                }
                if (j >= totalProcessNum && j < totalProcessNum * 2)//工序码
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
        int[,] genData = new int[gensize, processNumTotal * 3];//机器码+工序码+AGV码
        double[] p_fit = new double[gensize];
        drawData = new int[processNumTotal * 3];
        double p_fit_min = 9999999;
        double[] p_fit_min_total = new double[iteration];
        int index = 0;
        int iter_min = 0;
        Initialize(gensize, workpieceNumber, machineNum, processNum, processOptNum,out genData);
        //开始迭代
        for (int i = 0; i < iteration; i++)
        {
            Reserve(gensize, genData, out genData,index,p_fit);//保留
            Cross(gensize, genData, processNum, out genData);//交叉
            Mutation(gensize, genData, out genData);//变异
            EnCode(gensize, genData, processNum, out p_fit);//解码
            index = p_fit.ToList().IndexOf(p_fit.Min());    //在个体历史适应度中找到最小值的索引号
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


        ////测试
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
        ///迭代结束

    }
    void InitGen()
    {
        //初始化赋值
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
                    processOptMachineTime[i,j] = Punishment_set;//工序对应加工时间初始化
            }
        }
        //for (int i = 0; i < pieceMachineTime.Count(); i++)
        //{
        //    int startNum = 0;//工序起始编号
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
            //删除旧文件，并创建一个新的 excel 文件。
            _excelName.Delete();
            _excelName = new FileInfo(path);
        }
        using (ExcelPackage package = new ExcelPackage(_excelName))
        {
            //在 excel 空文件添加新 sheet，并设置名称

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

                worksheet0.Cells[2, 1].Value = "迭代/实验次数";
                worksheet0.Cells[2, 2].Value = "本次最佳染色体编号";
                worksheet0.Cells[2, 3].Value = "本次最佳适应度";
                worksheet0.Cells[2, 4].Value = "历史最佳适应度";
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
            //在 excel 空文件添加新 sheet，并设置名称
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
            //在 excel 空文件添加新 sheet，并设置名称
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
            //在 excel 空文件添加新 sheet，并设置名称
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
            Excelpath = Generate_Experimental_Tables(DataExportPath, DataExportName, iteration, gensize, workpieceNumber, machineNum, AGVNum, processNumTotal, AGVSpeed, CrossCount_min, CrossCount_max, CrossRange, MutationProbability);  //生成实验数据文件
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
