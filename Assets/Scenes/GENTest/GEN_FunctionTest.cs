using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;
using OfficeOpenXml;
using System.IO;
using XCharts.Runtime;

public class GEN_FunctionTest : MonoBehaviour
{
    public int iteration = 1000;//迭代次数
    public int gensize = 50;//种群大小
    public int workpieceNumber = 3;//工件数量
    public int machineNum = 3;//机器数量
    public int AGVNum = 3;//AGV数量
    public int processNumTotal = 6;//工序总量
    public int[] drawData;
    public GameObject canvas;

    public int CrossCount_min = 1;
    public int CrossCount_max = 5;
    public double CrossRange = 0.5;
    public double MutationProbability = 0.1;
    public int[] processNum1 = { 2, 2, 2 };//工件对应工序数量
    public int[] processNum = { 0, 0, 1, 1, 2, 2 };//工序展开便于初始化
    public int[] processOptNum = { 3, 3, 3, 3, 3, 2 };//工序可选择的机器数量
    //public int[,] processOptMachine = { { 0, 1, 2 }, { 0, 1, 2 }, { 0, 1, 2 }, { 0, 1, 2 }, { 0, 1, 2 }, { 0, 2, 1 } };//工序选择机器对应机器编号
    public double[,] processOptMachineTime = { { 5, 4, 4 }, { 4, 2, 3 }, { 2, 2, 1 }, { 3, 4, 3 }, { 5, 4, 5 }, { 2, 9999, 2 } };//工序选择机器所需时间
    public double[] machineDistance = {1, 2, 4, 1, 2, 4};//设备间的距离，用于计算AGVworktime
    public float AGVSpeed = 1.5f;


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
                if (Oij[Oi] == 0) //第一道工序,不考虑前置工序
                {
                    Tagv[Ai] = (Tagv[Ai] + AGVworktime) < Tmi[Mi] ? Tmi[Mi] : (Tagv[Ai] + AGVworktime);//AGV运送时间,如果运送时间 小于 机器可加工时间则等机器加工完
                    Tagv_xy = update_AGVxy(Tagv_xy, Ai, Mi);//更新AGV位置

                    Tmi[Mi] = Tagv[Ai] + Tij[k + Oij[Oi]];//机器安排工作 k + Oij[Oi] 工件+工序进度=该工序所需时间，第Mi号机器的进度往前推该工序
                    Tpi[Oi] = Tmi[Mi];//工件时间进度
                }
                else 
                {
                    double Tagv_Tpi = Tagv[Ai] < Tpi[Oi] ? (Tpi[Oi] + AGVworktime) : (Tagv[Ai] + AGVworktime);//该道工序AGV将该工件运送到目标位置所需时间,根据AGV是否空余决定

                    if (Tagv_Tpi < Tmi[Mi])//工件运送到目标机器的时间小于所选机器最早可加工时间，等待机器加工完后直接加工该工件
                    {
                        Tagv[Ai] = Tmi[Mi];//AGV进度等于机器最早开工时间
                        Tagv_xy = update_AGVxy(Tagv_xy, Ai, Mi);
                        Tmi[Mi] += Tij[k + Oij[Oi]];//机器安排工作
                    }
                    else //工件运送到目标机器的时间大于所选机器最早可加工时间，等待工件运到后再放到机器上加工
                    {
                        Tagv[Ai] = Tagv_Tpi;
                        Tagv_xy = update_AGVxy(Tagv_xy, Ai, Mi);
                        Tmi[Mi] = Tagv_Tpi + Tij[k + Oij[Oi]];
                    }

                    Tpi[Oi] = Tmi[Mi];//工件时间进度

                    //if (Tpi[Oi] < Tmi[Mi])  //工件最早可加工时间小于所选机器最早可加工时间，等待机器加工完后直接加工该工件
                    //{
                    //    //贪婪算法，判断机器前置时间是否有空余
                    //    bool MacEmptyTime = false;//避免重复安排进度
                    //    for (int m = 0; m < totalProcessNum * 2; m++)
                    //    {
                    //        //如果工件进度大于等于机器空余时间集合的开始
                    //        if (Tpi[Oi] >= MachineEmptyTime[Mi * 2, m])
                    //        {
                    //            //（如果机器空余时间集合的结束-机器空余时间集合的开始）大于工序所需时间
                    //            if ((MachineEmptyTime[Mi * 2 + 1, m] - Tpi[Oi]) >= Tij[k + Oij[Oi]])
                    //            {

                    //                //安排机器加工
                    //                Tpi[Oi] = Tpi[Oi] + Tij[k + Oij[Oi]];
                    //                MacEmptyTime = true;

                    //                //修改机器空余时间集合
                    //                MachineEmptyTime[Mi * 2, m] = Tpi[Oi];

                    //                break;
                    //            }
                    //        }
                    //    }
                    //    if (MacEmptyTime == false)
                    //    {
                    //        Tmi[Mi] += Tij[k + Oij[Oi]];//机器安排工作
                    //        Tpi[Oi] = Tmi[Mi];//工件时间进度
                    //    }

                    //}
                    ////工件最早可加工时间大于所选机器最早可加工时间，等待工件加工完后再放到机器上加工
                    //else
                    //{
                    //    MachineEmptyTime[Mi * 2, MachineEmptyTime_index[Mi * 2]] = Tmi[Mi];
                    //    MachineEmptyTime_index[Mi * 2] += 1;
                    //    Tmi[Mi] = Tpi[Oi] + Tij[k + Oij[Oi]];
                    //    Tpi[Oi] = Tmi[Mi];
                    //    MachineEmptyTime[Mi * 2 + 1, MachineEmptyTime_index[Mi * 2 + 1]] = Tmi[Mi];
                    //    MachineEmptyTime_index[Mi * 2 + 1] += 1;
                    //}
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
        for (int i = 0; i < gensize / 2; i++)
        {
            //本次交叉的两个基因
            int gensize1 = 2 * i;
            int gensize2 = 2 * i + 1;
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
        }

        gensize_after = childx0;
    }

    public void Mutation(int gensize, int[,] genData, out int[,] gensize_after)
    {
        for (int i = 0; i < gensize; i++)
        {
            //机器码变异
            double index1 = UnityEngine.Random.Range(0.0f, 1.0f);//返回介于 min [含] 与 max [不含] 
            if (index1 <= MutationProbability)
            {
                int index2 = UnityEngine.Random.Range(0, processNum.Length);//本次变异的机器码
                int index3 = 0;//选取用时最少的机器
                for (int j = 1; j < machineNum; j++)
                {
                    if (processOptMachineTime[index2, index3] > processOptMachineTime[index2, j])
                    {
                        index3 = j;
                    }
                }
                genData[i, index2] = index3;
            }

            //工序码变异
            index1 = UnityEngine.Random.Range(0.0f, 1.0f);
            if (index1 <= MutationProbability)
            {
                int index2 = UnityEngine.Random.Range(processNum.Length, processNum.Length * 2 - 1);//本次交换基因的机器码起始
                int index3 = UnityEngine.Random.Range(index2, processNum.Length * 2);//本次交换基因的机器码结束

                int temp = genData[i, index2];
                genData[i, index2] = genData[i, index3];
                genData[i, index3] = temp;
            }
        }
        gensize_after =  genData;
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
        title.text = "调度甘特图,本次得到最短加工时间："+ p_fit;
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
        chart.AddXAxisData("M0");
        chart.AddXAxisData("M1");
        chart.AddXAxisData("M2");
        chart.AddXAxisData("AGV0");
        chart.AddXAxisData("AGV1");
        chart.AddXAxisData("AGV2");
        //0号数据名称用来记录工序安排
        for (int i = 0; i < machineNum + AGVNum; i++)
        {
            chart.AddData(0, 0, 0, 0, 0);
        }
        int count_data = 1;//数据名称,每次累加，避免同名



        int totalProcessNum = processNum.Length;//工序总数
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
            if (Oij[Oi] == 0) //第一道工序,不考虑前置工序
            {
                Tagv[Ai] = (Tagv[Ai] + AGVworktime) < Tmi[Mi] ? Tmi[Mi] : (Tagv[Ai] + AGVworktime);//AGV运送时间,如果运送时间 小于 机器可加工时间则等机器加工完
                Tagv_xy = update_AGVxy(Tagv_xy, Ai, Mi);//更新AGV位置
                serieData1 = Tagv[Ai];
                Tmi[Mi] = Tagv[Ai] + Tij[k + Oij[Oi]];//机器安排工作 k + Oij[Oi] 工件+工序进度=该工序所需时间，第Mi号机器的进度往前推该工序
                Tpi[Oi] = Tmi[Mi];//工件时间进度

                serieData2 = Tmi[Mi];
            }
            else
            {
                double Tagv_Tpi = Tagv[Ai] < Tpi[Oi] ? (Tpi[Oi] + AGVworktime) : (Tagv[Ai] + AGVworktime);//该道工序AGV将该工件运送到目标位置所需时间,根据AGV是否空余决定
                if (Tagv_Tpi < Tmi[Mi])//工件运送到目标机器的时间小于所选机器最早可加工时间，等待机器加工完后直接加工该工件
                {
                    serieData1 = Tmi[Mi];
                    Tagv[Ai] = Tmi[Mi];//AGV进度等于机器最早开工时间
                    Tagv_xy = update_AGVxy(Tagv_xy, Ai, Mi);//更新AGV位置
                    Tmi[Mi] += Tij[k + Oij[Oi]];//机器安排工作
                }
                else //工件运送到目标机器的时间大于所选机器最早可加工时间，等待工件运到后再放到机器上加工
                {
                    serieData1 = Tagv_Tpi;
                    Tagv[Ai] = Tagv_Tpi;
                    Tagv_xy = update_AGVxy(Tagv_xy, Ai, Mi);//更新AGV位置
                    Tmi[Mi] = Tagv_Tpi + Tij[k + Oij[Oi]];
                }
                Tpi[Oi] = Tmi[Mi];//工件时间进度

                serieData2 = Tmi[Mi];


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
            if (Oi == 0)
                itemMacStyle.color = Color.blue;
            if (Oi == 1)
                itemMacStyle.color = Color.yellow;
            if (Oi == 2)
                itemMacStyle.color = Color.red;
            count_data++;

            chart.AddSerie<Candlestick>("Candlestick");
            for (int p = 0; p < Ai + 3; p++)
            {
                chart.AddData(count_data, 0, 0, 0, 0);//给前面的柱状图填空
            }
            var serieMacData1 = chart.AddData(count_data, serieData3, serieData4, serieData3, serieData4);//
            serieMacData1.radius = 10;
            var itemMacStyle1 = serieMacData1.EnsureComponent<ItemStyle>(); //给数据项添加ItemStyle组件
            if (Oi == 0)
                itemMacStyle1.color = Color.blue;
            if (Oi == 1)
                itemMacStyle1.color = Color.yellow;
            if (Oi == 2)
                itemMacStyle1.color = Color.red;
            count_data++;

            var serieData = chart.AddData(0, serieData1, serieData2, serieData1, serieData2);
            chart.AddXAxisData("O" + Oi + Oij[Oi]);
            serieData.radius = 10;
            var itemStyle = serieData.EnsureComponent<ItemStyle>(); //给数据项添加ItemStyle组件
            if (Oi == 0)
                itemStyle.color = Color.blue;
            if (Oi == 1)
                itemStyle.color = Color.yellow;
            if (Oi == 2)
                itemStyle.color = Color.red;
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
    public void StartGENFunction()
    {
        int[,] genData = new int[gensize, processNum.Length * 3];//机器码+工序码+AGV码
        double[] p_fit = new double[gensize];
        drawData = new int[processNum.Length * 3];
        double p_fit_min = 9999999;
        int index = 0;
        int iter_min = 0;
        Initialize(gensize, workpieceNumber, machineNum, processNum, processOptNum,out genData);
        //开始迭代
        for (int i = 0; i < iteration; i++)
        {
            Cross(gensize, genData, processNum, out genData);//交叉
            Mutation(gensize, genData, out genData);
            EnCode(gensize, genData, processNum, out p_fit);//解码

            if (p_fit_min > p_fit.Min())
            {
                iter_min = i;
                p_fit_min = p_fit.Min();
                index = p_fit.ToList().IndexOf(p_fit.Min());    //在个体历史适应度中找到最小值的索引号
                for (int j = 0; j < drawData.Length; j++)
                {
                    drawData[j] = genData[index, j];
                }
            }

        }

        Debug.Log("p_fit_min:" + p_fit.Min());


        //测试贪婪算法
        //drawData[0] = 1;
        //drawData[1] = 1;
        //drawData[2] = 0;
        //drawData[3] = 2;
        //drawData[4] = 0;
        //drawData[5] = 0;
        //drawData[6] = 1;
        //drawData[7] = 2;
        //drawData[8] = 0;
        //drawData[9] = 2;
        //drawData[10] = 0;
        //drawData[11] = 1;
        //drawData[12] = 1;
        //drawData[13] = 0;
        //drawData[14] = 2;
        //drawData[15] = 1;
        //drawData[16] = 2;
        //drawData[17] = 0;


        DrawGante(drawData, index, p_fit_min, iter_min);
        ///迭代结束

    }
    void Awake()
    {
        StartGENFunction();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
