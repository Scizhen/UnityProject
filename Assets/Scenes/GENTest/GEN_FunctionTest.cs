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
    public int iteration = 1000;//��������
    public int gensize = 50;//��Ⱥ��С
    public int workpieceNumber = 3;//��������
    public int machineNum = 3;//��������
    public int AGVNum = 3;//AGV����
    public int processNumTotal = 6;//��������
    public int[] drawData;
    public GameObject canvas;

    public int CrossCount_min = 1;
    public int CrossCount_max = 5;
    public double CrossRange = 0.5;
    public double MutationProbability = 0.1;
    public int[] processNum1 = { 2, 2, 2 };//������Ӧ��������
    public int[] processNum = { 0, 0, 1, 1, 2, 2 };//����չ�����ڳ�ʼ��
    public int[] processOptNum = { 3, 3, 3, 3, 3, 2 };//�����ѡ��Ļ�������
    //public int[,] processOptMachine = { { 0, 1, 2 }, { 0, 1, 2 }, { 0, 1, 2 }, { 0, 1, 2 }, { 0, 1, 2 }, { 0, 2, 1 } };//����ѡ�������Ӧ�������
    public double[,] processOptMachineTime = { { 5, 4, 4 }, { 4, 2, 3 }, { 2, 2, 1 }, { 3, 4, 3 }, { 5, 4, 5 }, { 2, 9999, 2 } };//����ѡ���������ʱ��
    public double[] machineDistance = {1, 2, 4, 1, 2, 4};//�豸��ľ��룬���ڼ���AGVworktime
    public float AGVSpeed = 1.5f;


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
                if (Oij[Oi] == 0) //��һ������,������ǰ�ù���
                {
                    Tagv[Ai] = (Tagv[Ai] + AGVworktime) < Tmi[Mi] ? Tmi[Mi] : (Tagv[Ai] + AGVworktime);//AGV����ʱ��,�������ʱ�� С�� �����ɼӹ�ʱ����Ȼ����ӹ���
                    Tagv_xy = update_AGVxy(Tagv_xy, Ai, Mi);//����AGVλ��

                    Tmi[Mi] = Tagv[Ai] + Tij[k + Oij[Oi]];//�������Ź��� k + Oij[Oi] ����+�������=�ù�������ʱ�䣬��Mi�Ż����Ľ�����ǰ�Ƹù���
                    Tpi[Oi] = Tmi[Mi];//����ʱ�����
                }
                else 
                {
                    double Tagv_Tpi = Tagv[Ai] < Tpi[Oi] ? (Tpi[Oi] + AGVworktime) : (Tagv[Ai] + AGVworktime);//�õ�����AGV���ù������͵�Ŀ��λ������ʱ��,����AGV�Ƿ�������

                    if (Tagv_Tpi < Tmi[Mi])//�������͵�Ŀ�������ʱ��С����ѡ��������ɼӹ�ʱ�䣬�ȴ������ӹ����ֱ�Ӽӹ��ù���
                    {
                        Tagv[Ai] = Tmi[Mi];//AGV���ȵ��ڻ������翪��ʱ��
                        Tagv_xy = update_AGVxy(Tagv_xy, Ai, Mi);
                        Tmi[Mi] += Tij[k + Oij[Oi]];//�������Ź���
                    }
                    else //�������͵�Ŀ�������ʱ�������ѡ��������ɼӹ�ʱ�䣬�ȴ������˵����ٷŵ������ϼӹ�
                    {
                        Tagv[Ai] = Tagv_Tpi;
                        Tagv_xy = update_AGVxy(Tagv_xy, Ai, Mi);
                        Tmi[Mi] = Tagv_Tpi + Tij[k + Oij[Oi]];
                    }

                    Tpi[Oi] = Tmi[Mi];//����ʱ�����

                    //if (Tpi[Oi] < Tmi[Mi])  //��������ɼӹ�ʱ��С����ѡ��������ɼӹ�ʱ�䣬�ȴ������ӹ����ֱ�Ӽӹ��ù���
                    //{
                    //    //̰���㷨���жϻ���ǰ��ʱ���Ƿ��п���
                    //    bool MacEmptyTime = false;//�����ظ����Ž���
                    //    for (int m = 0; m < totalProcessNum * 2; m++)
                    //    {
                    //        //����������ȴ��ڵ��ڻ�������ʱ�伯�ϵĿ�ʼ
                    //        if (Tpi[Oi] >= MachineEmptyTime[Mi * 2, m])
                    //        {
                    //            //�������������ʱ�伯�ϵĽ���-��������ʱ�伯�ϵĿ�ʼ�����ڹ�������ʱ��
                    //            if ((MachineEmptyTime[Mi * 2 + 1, m] - Tpi[Oi]) >= Tij[k + Oij[Oi]])
                    //            {

                    //                //���Ż����ӹ�
                    //                Tpi[Oi] = Tpi[Oi] + Tij[k + Oij[Oi]];
                    //                MacEmptyTime = true;

                    //                //�޸Ļ�������ʱ�伯��
                    //                MachineEmptyTime[Mi * 2, m] = Tpi[Oi];

                    //                break;
                    //            }
                    //        }
                    //    }
                    //    if (MacEmptyTime == false)
                    //    {
                    //        Tmi[Mi] += Tij[k + Oij[Oi]];//�������Ź���
                    //        Tpi[Oi] = Tmi[Mi];//����ʱ�����
                    //    }

                    //}
                    ////��������ɼӹ�ʱ�������ѡ��������ɼӹ�ʱ�䣬�ȴ������ӹ�����ٷŵ������ϼӹ�
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
        for (int i = 0; i < gensize / 2; i++)
        {
            //���ν������������
            int gensize1 = 2 * i;
            int gensize2 = 2 * i + 1;
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
        }

        gensize_after = childx0;
    }

    public void Mutation(int gensize, int[,] genData, out int[,] gensize_after)
    {
        for (int i = 0; i < gensize; i++)
        {
            //���������
            double index1 = UnityEngine.Random.Range(0.0f, 1.0f);//���ؽ��� min [��] �� max [����] 
            if (index1 <= MutationProbability)
            {
                int index2 = UnityEngine.Random.Range(0, processNum.Length);//���α���Ļ�����
                int index3 = 0;//ѡȡ��ʱ���ٵĻ���
                for (int j = 1; j < machineNum; j++)
                {
                    if (processOptMachineTime[index2, index3] > processOptMachineTime[index2, j])
                    {
                        index3 = j;
                    }
                }
                genData[i, index2] = index3;
            }

            //���������
            index1 = UnityEngine.Random.Range(0.0f, 1.0f);
            if (index1 <= MutationProbability)
            {
                int index2 = UnityEngine.Random.Range(processNum.Length, processNum.Length * 2 - 1);//���ν�������Ļ�������ʼ
                int index3 = UnityEngine.Random.Range(index2, processNum.Length * 2);//���ν�������Ļ��������

                int temp = genData[i, index2];
                genData[i, index2] = genData[i, index3];
                genData[i, index3] = temp;
            }
        }
        gensize_after =  genData;
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
        title.text = "���ȸ���ͼ,���εõ���̼ӹ�ʱ�䣺"+ p_fit;
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
        //0����������������¼������
        for (int i = 0; i < machineNum + AGVNum; i++)
        {
            chart.AddData(0, 0, 0, 0, 0);
        }
        int count_data = 1;//��������,ÿ���ۼӣ�����ͬ��



        int totalProcessNum = processNum.Length;//��������
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
            if (Oij[Oi] == 0) //��һ������,������ǰ�ù���
            {
                Tagv[Ai] = (Tagv[Ai] + AGVworktime) < Tmi[Mi] ? Tmi[Mi] : (Tagv[Ai] + AGVworktime);//AGV����ʱ��,�������ʱ�� С�� �����ɼӹ�ʱ����Ȼ����ӹ���
                Tagv_xy = update_AGVxy(Tagv_xy, Ai, Mi);//����AGVλ��
                serieData1 = Tagv[Ai];
                Tmi[Mi] = Tagv[Ai] + Tij[k + Oij[Oi]];//�������Ź��� k + Oij[Oi] ����+�������=�ù�������ʱ�䣬��Mi�Ż����Ľ�����ǰ�Ƹù���
                Tpi[Oi] = Tmi[Mi];//����ʱ�����

                serieData2 = Tmi[Mi];
            }
            else
            {
                double Tagv_Tpi = Tagv[Ai] < Tpi[Oi] ? (Tpi[Oi] + AGVworktime) : (Tagv[Ai] + AGVworktime);//�õ�����AGV���ù������͵�Ŀ��λ������ʱ��,����AGV�Ƿ�������
                if (Tagv_Tpi < Tmi[Mi])//�������͵�Ŀ�������ʱ��С����ѡ��������ɼӹ�ʱ�䣬�ȴ������ӹ����ֱ�Ӽӹ��ù���
                {
                    serieData1 = Tmi[Mi];
                    Tagv[Ai] = Tmi[Mi];//AGV���ȵ��ڻ������翪��ʱ��
                    Tagv_xy = update_AGVxy(Tagv_xy, Ai, Mi);//����AGVλ��
                    Tmi[Mi] += Tij[k + Oij[Oi]];//�������Ź���
                }
                else //�������͵�Ŀ�������ʱ�������ѡ��������ɼӹ�ʱ�䣬�ȴ������˵����ٷŵ������ϼӹ�
                {
                    serieData1 = Tagv_Tpi;
                    Tagv[Ai] = Tagv_Tpi;
                    Tagv_xy = update_AGVxy(Tagv_xy, Ai, Mi);//����AGVλ��
                    Tmi[Mi] = Tagv_Tpi + Tij[k + Oij[Oi]];
                }
                Tpi[Oi] = Tmi[Mi];//����ʱ�����

                serieData2 = Tmi[Mi];


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
            var itemMacStyle = serieMacData.EnsureComponent<ItemStyle>(); //������������ItemStyle���
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
                chart.AddData(count_data, 0, 0, 0, 0);//��ǰ�����״ͼ���
            }
            var serieMacData1 = chart.AddData(count_data, serieData3, serieData4, serieData3, serieData4);//
            serieMacData1.radius = 10;
            var itemMacStyle1 = serieMacData1.EnsureComponent<ItemStyle>(); //������������ItemStyle���
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
            var itemStyle = serieData.EnsureComponent<ItemStyle>(); //������������ItemStyle���
            if (Oi == 0)
                itemStyle.color = Color.blue;
            if (Oi == 1)
                itemStyle.color = Color.yellow;
            if (Oi == 2)
                itemStyle.color = Color.red;
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
    public void StartGENFunction()
    {
        int[,] genData = new int[gensize, processNum.Length * 3];//������+������+AGV��
        double[] p_fit = new double[gensize];
        drawData = new int[processNum.Length * 3];
        double p_fit_min = 9999999;
        int index = 0;
        int iter_min = 0;
        Initialize(gensize, workpieceNumber, machineNum, processNum, processOptNum,out genData);
        //��ʼ����
        for (int i = 0; i < iteration; i++)
        {
            Cross(gensize, genData, processNum, out genData);//����
            Mutation(gensize, genData, out genData);
            EnCode(gensize, genData, processNum, out p_fit);//����

            if (p_fit_min > p_fit.Min())
            {
                iter_min = i;
                p_fit_min = p_fit.Min();
                index = p_fit.ToList().IndexOf(p_fit.Min());    //�ڸ�����ʷ��Ӧ�����ҵ���Сֵ��������
                for (int j = 0; j < drawData.Length; j++)
                {
                    drawData[j] = genData[index, j];
                }
            }

        }

        Debug.Log("p_fit_min:" + p_fit.Min());


        //����̰���㷨
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
        ///��������

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