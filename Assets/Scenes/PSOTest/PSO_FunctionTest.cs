using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;
using OfficeOpenXml;
using System.IO;
using NaughtyAttributes;

public class PSO_FunctionTest : MonoBehaviour
{
    public double[] xlimit = new double[2] { 0, 50 };       //λ�ñ߽�����
    public double[] ylimit = new double[2] { 0, 40 };
    public double[] vlimit = new double[2] { -2, 2 };       //�ٶȱ߽�����
    public int experimentNumber= 1;
    public int generation = 100;             //��������
    public int popsize = 100;                //��Ⱥ����
    public int equipmentNumber = 12;       //�豸����
    public bool OpenLspInit = true;
    public bool OpenGeneral = true;
    public string DataExportPath = "/Scenes/PSOTest/Data";  //ʵ�������ļ��б���·��
    public string DataExportName = "nof";             //ʵ�������ļ�������
    public bool SaveTotalData = false;                  
    public bool SaveEveryGenerationData = false;         //�Ƿ񱣴�ÿһ�ε�������
    public bool SaveEveryPopsizeData = false;           //�Ƿ񱣴�ÿһ���������ݡ���������ʱ������ʱ��Ͼã�������ѡ��

    public GameObject prefeb;
    public List<GameObject> prefebs =  new List<GameObject>();
    public List<GameObject> machineGameObject = new List<GameObject>();

    public double Wmax = 0.95;             //����Ȩ������
    public double Wmin = 0.45;
    public double C1 = 2;                //����ѧϰ����
    public double C2 = 2;                //Ⱥ��ѧϰ����
    public double T1 = 10;                //���ͷ�ϵ��
    public double T2 = 1000;            //�����ͷ�ϵ��

    public int gen_size = 10;
    public double Variation_Pro = 0.2f;//������ʣ�0-1��

    private static double[] Frequent_data =
                                     {0,1,0,0,0,0,0,1,0,0,0,0,
                                      1,0,1,0,0,0,0,0,0,0,0,0,
                                      0,1,0,1,0,0,0,0,0,0,0,0,
                                      0,0,1,0,1,0,0,0,0,0,0,0,
                                      0,0,0,1,0,1,0,0,0,0,0,0,
                                      0,0,0,0,1,0,1,0,0,0,0,0,
                                      0,0,0,0,0,1,0,0,1,1,0,1,
                                      1,0,0,0,0,0,0,0,1,0,0,0,
                                      0,0,0,0,0,0,1,1,0,0,0,0,
                                      0,0,0,0,0,0,1,0,0,0,1,0,
                                      0,0,0,0,0,0,0,0,0,1,0,1,
                                      0,0,0,0,0,0,1,0,0,0,1,0};//��������Ƶ��
    private static double Cost_per_meter = 1;//����ÿ�׻���
    private double[] Equipment_Length =
        {5,7.18,6.73,7.89,3.23,3.05,3.45,3.8,4.85,2.45,3.45,5};//�豸����
    private double[] Equipment_Width =
        {5,9,6.32,5.12,6.44,6.11,8.39,6.55,4.25,3.85,3.65,5};//�豸���
    private double Equipment_Min_Length = 1.5;//�豸����С���� Hij
    private double Equipment_Min_Width = 1.2;//�豸����С��� Vij
    private double Wall_Min_Length = 1.5;//�豸����С����  H0i
    private double Wall_Min_Width = 1.2;//�豸����С���   V0i

    public double[] Lsp_Init = { 7.54,19.9,19.8,31.1,41.2,30.7,29.3,7.39,19.9,36,43.3,41,21,21.4,32.8,32,31.7,23,11.8,11.4,8.71,6.42,8.78,17.6 };
    private double[] Lsp_Init_4030 = { 4.91, 13.65, 13.55, 22.8, 31.97, 23.47, 23.63, 5.03, 13.96, 29.95, 35.17, 34.21, 20.6, 15.56, 24.7, 24.98, 22.04, 16.87, 7.25, 7.87, 5.71, 5.38, 5.5, 13.21 };
    private int fitness_generation_500 = -1;

    void Main(string totalpath,int totalnum)
    {
        fitness_generation_500 = -1;
        string Excelpath = "";
        if(SaveEveryGenerationData|| SaveEveryPopsizeData)
            Excelpath = Generate_Experimental_Tables(DataExportPath, DataExportName,Wmax,Wmin,C1,C2,xlimit,ylimit,vlimit,T1,T2,Equipment_Min_Length,Equipment_Min_Width,Wall_Min_Length,Wall_Min_Width);  //����ʵ�������ļ�
        System.Random rand = new System.Random();

        int equipmentNumber_init = 2 * equipmentNumber;//ÿ���豸��x��y�ᣬ[x1��x2��...,xn,y1,y2...,yn]
        //������ʼ��           
        double[,] x0 = new double[popsize, equipmentNumber_init];  //��Ⱥλ�ó�ʼ��
        double[,] V = new double[popsize, equipmentNumber_init];               //�����ٶȳ�ʼ��
        double[,] x_pbest = new double[popsize, equipmentNumber_init];         //������ʷ���λ��
        double[] p_fitness = new double[popsize];           //������ʷ�����Ӧ��
        double[] x_gbest = new double[equipmentNumber_init];    //Ⱥ����ʷ���λ��
        double g_fitness = new double();                     //Ⱥ����ʷ�����Ӧ��
        Initialize(popsize,equipmentNumber_init, out x0, out x_pbest, out p_fitness, out x_gbest, out g_fitness, out V);

        double[] cur_fitness = new double[popsize];
        //Thread.Sleep(3000);                                            //��ʱ
        //�㷨����
        for (int i = 0; i < generation; i++)
        {
            for (int j = 0; j < popsize; j++)
            {
                double[] equipment_fitness = new double[equipmentNumber];
                for (int k = 0; k < equipmentNumber; k++)
                {
                    equipment_fitness[k] = func(j, k, x0,T1, T2);  //����ÿ���豸����Ӧ��
                }

                double fitness = Calute_gfitness(equipment_fitness);//���㱾�����Ӹ�����Ӧ��
                cur_fitness[j] = fitness;//�洢���εĸ�����Ӧ��ֵ
                if (fitness < p_fitness[j])         //����ǰ������Ӧ���������ʷ�����Ӧ�ȱȽϣ������������滻
                {
                    p_fitness[j] = fitness;         //���¸�����ʷ�����Ӧ��
                    for (int k = 0; k < equipmentNumber; k++)   //���¸�����ʷ���λ��
                    {
                        x_pbest[j,k] = x0[j,k];              //�����豸x���Ӧλ��
                        x_pbest[j,k+equipmentNumber] = x0[j,k+equipmentNumber];   //�����豸y���Ӧλ��
                    }
                }
                //Debug.Log("��ǰ���ӱ��Ϊ��" + j + "   " + " ������Ӧ��Ϊ��" + fitness.ToString("f5") + "   " + "���������Ӧ��Ϊ��" + p_fitness[j].ToString("f5"));
                if(SaveEveryPopsizeData ==true)
                    Save_Every_Popsize_Data(Excelpath, DataExportName, i,j,fitness,p_fitness[j],x0);  //����ÿһ�����ӵ����ݣ��������ϴ�ʱ���ã����ܻ���Ҫ�ȽϾõ�ʱ�䣡
            }
            if (g_fitness > p_fitness.Min())
            {

                g_fitness = p_fitness.Min();            //����ȫ������ֵ
                int index = p_fitness.ToList().IndexOf(p_fitness.Min());    //�ڸ�����ʷ��Ӧ�����ҵ���Сֵ��������
                for (int k = 0; k < equipmentNumber; k++)   //����ȫ����ʷ���λ��
                {
                    x_gbest[k] = x0[index,k];              
                    x_gbest[k + equipmentNumber] = x0[index,k + equipmentNumber];
                    //Debug.Log("��ǰ����ȫ������λ�ã��豸" + k + "��x��Ϊ��"+x0[index, k].ToString("f5") + " ��y��Ϊ��" + x0[index, k + equipmentNumber].ToString("f5"));
                }
            }
            if(SaveEveryGenerationData == true)
                Save_Every_Generation_Data(Excelpath, i, p_fitness.ToList().IndexOf(p_fitness.Min()), cur_fitness.Min(), g_fitness,x0);//����ÿһ�ε����������
            if ((g_fitness < 500) && (fitness_generation_500 < 0))
                fitness_generation_500 = i;
            Debug.Log("��ǰ��������Ϊ��" + i + "   " + " ȫ������ֵΪ��" + g_fitness.ToString("f5"));

            //�Ŵ��㷨��ʼ
            if (OpenGeneral)
            {
                GeneticAlgorithm gen = new GeneticAlgorithm();
                x0 = gen.GeneticCalcute(x0, cur_fitness, gen_size, Variation_Pro, equipmentNumber);
                Debug.Log("OpenGeneral");
            }
            //�Ŵ��㷨����

            double W = new double();
            W = Wmax - (Wmax - Wmin) * i / generation;     //���Եݼ�����Ȩ��

            for (int j = 0; j < popsize; j++) 
            {
                //�ٶȸ���
                for (int k = 0; k < 2 * equipmentNumber; k++)
                {
                    V[j,k] = W * V[j,k] + C1 * rand.NextDouble() * (x_pbest[j,k] - x0[j,k]) + C2 * rand.NextDouble() * (x_gbest[k] - x0[j,k]);

                    //�ٶ�����
                    if (V[j,k] > vlimit[1])
                    {
                        V[j,k] = vlimit[1];
                    }
                    if (V[j,k] < vlimit[0])
                    {
                        V[j,k] = vlimit[0];
                    }

                    //λ�ø���
                    x0[j,k] = x0[j,k] + V[j,k];

                    //λ������
                    if (k < equipmentNumber)
                    {
                        
                        if (x0[j,k] > xlimit[1])
                        {
                            x0[j,k] = xlimit[1];
                        }
                        if (x0[j,k] < xlimit[0])
                        {
                            x0[j,k] = xlimit[0];
                        }
                    }
                    if (k >= equipmentNumber)
                    {
                        if (x0[j,k] > ylimit[1])
                        {
                            x0[j,k] = ylimit[1];
                        }
                        if (x0[j,k] < ylimit[0])
                        {
                            x0[j,k] = ylimit[0];
                        }
                    }
                }

            }

        }
        if (SaveTotalData == true)
            Save_Every_Generation_Data(totalpath, totalnum, p_fitness.ToList().IndexOf(p_fitness.Min()), cur_fitness.Min(), g_fitness, x0);//����ÿһ�ε����������
        Generate_Layout_Plan(x_gbest);
        //Debug.Log("Ŀ�꺯����ȫ����Сֵ������:" + g_fit.ToList().IndexOf(g_fit.Min()) + "   " +g_fit.Min().ToString("f5"));

    }

    private void Generate_Layout_Plan(double[] x_gbest) 
    {
        if (prefebs.Count() < equipmentNumber)
        {
            for (int i = 0; i < prefebs.Count(); i++)
            {
                DestroyImmediate(prefebs[i], true);
            }
            prefebs.Clear();
            for (int i = 0; i < equipmentNumber; i++)
            {
                GameObject obj;
                Vector3 pos = new Vector3(((float)x_gbest[i]), 0, ((float)x_gbest[equipmentNumber + i]));
                Vector3 scale = new Vector3(((float)Equipment_Length[i]), 1, ((float)Equipment_Width[i]));
                if (machineGameObject[i] == null)
                {
                    prefeb = Resources.Load("Cube") as GameObject;
                    obj = Instantiate(prefeb, pos, Quaternion.identity);
                    obj.transform.localScale = scale;
                }
                else
                {
                    prefeb = machineGameObject[i];
                    obj = Instantiate(prefeb, pos, Quaternion.identity);
                }
                if (obj.GetComponent<MeshRenderer>() != null)
                    obj.GetComponent<MeshRenderer>().material.color = UnityEngine.Random.ColorHSV();
                obj.name = i.ToString();
                prefebs.Add(obj);
            }
        }
        else
        {
            for (int i = 0; i < equipmentNumber; i++)
            {
                Vector3 pos = new Vector3(((float)x_gbest[i]), 0, ((float)x_gbest[equipmentNumber + i]));
                Vector3 scale = new Vector3(((float)Equipment_Length[i]), 1, ((float)Equipment_Width[i]));
                prefebs[i].transform.position = pos;
            }
        }

    }
    private string  Generate_Experimental_Tables(string path,string name,double Wmax,double Wmin,double C1,double C2,double[] xlimit,double[] ylimit,double[] vlimit,double T1,double T2,double eq_Min_Length,double eq_Min_Width,double wall_Min_length,double wall_Min_width) 
    {
        path = Application.dataPath+ path + name + DateTime.UtcNow.Ticks + ".xlsx";
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
            ExcelWorksheet worksheet0 = package.Workbook.Worksheets.Add("Every_Generation");
            worksheet0.Cells[1, 1].Value = "��������";
            worksheet0.Cells[1, 2].Value = "����������ӱ��";
            worksheet0.Cells[1, 3].Value = "���������Ӧ��";
            worksheet0.Cells[1, 4].Value = "��ʷ�����Ӧ��";
            for (int i = 0; i < equipmentNumber; i++)
            {
                worksheet0.Cells[1, 5 + i].Value = i + "x";
                worksheet0.Cells[1, 5 + equipmentNumber + i].Value = i + "y";
            }
            worksheet0.Cells[1, 5 + 2 * equipmentNumber].Value = "generation:" + generation;
            worksheet0.Cells[1, 6 + 2 * equipmentNumber].Value = "popsize:" + popsize;
            worksheet0.Cells[1, 7 + 2 * equipmentNumber].Value = "Wmax:" + Wmax;
            worksheet0.Cells[1, 8 + 2 * equipmentNumber].Value = "Wmin:" + Wmin;
            worksheet0.Cells[1, 9 + 2 * equipmentNumber].Value = "C1:" + C1;
            worksheet0.Cells[1, 10 + 2 * equipmentNumber].Value = "C2:" + C2;
            worksheet0.Cells[1, 11 + 2 * equipmentNumber].Value = "xlimit:" + xlimit[0] + "," + xlimit[1];
            worksheet0.Cells[1, 12 + 2 * equipmentNumber].Value = "ylimit:" + ylimit[0] + "," + ylimit[1];
            worksheet0.Cells[1, 13 + 2 * equipmentNumber].Value = "vlimit:" + vlimit[0] + "," + vlimit[1];
            worksheet0.Cells[1, 14 + 2 * equipmentNumber].Value = "T1:" + T1;
            worksheet0.Cells[1, 15 + 2 * equipmentNumber].Value = "T2:" + T2;
            worksheet0.Cells[1, 16 + 2 * equipmentNumber].Value = "eq_Min_Length:" + eq_Min_Length;
            worksheet0.Cells[1, 17 + 2 * equipmentNumber].Value = "eq_Min_Width:" + eq_Min_Width;
            worksheet0.Cells[1, 18 + 2 * equipmentNumber].Value = "wall_Min_length:" + wall_Min_length;
            worksheet0.Cells[1, 19 + 2 * equipmentNumber].Value = "wall_Min_width:" + wall_Min_width;

            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Every_Popsize");

            worksheet.Cells[1, 1].Value = "��������";
            worksheet.Cells[1, 2].Value = "��������";
            worksheet.Cells[1, 3].Value = "������Ӧ��";
            worksheet.Cells[1, 4].Value = "���������Ӧ��";
            for (int i = 0; i < equipmentNumber; i++)
            {
                worksheet.Cells[1, 5 + i].Value = i+"x";
                worksheet.Cells[1, 5 + equipmentNumber +  i].Value = i + "y";
            }
            package.Save();
        }
        return path;
    }
    private string Generate_TotalExperimental_Tables(string path)
    {
        path = Application.dataPath + path + "Total" + DateTime.UtcNow.Ticks + ".xlsx";
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
            ExcelWorksheet worksheet0 = package.Workbook.Worksheets.Add("Every_Generation");
            worksheet0.Cells[1, 1].Value = "ʵ�����";
            worksheet0.Cells[1, 2].Value = "����������ӱ��";
            worksheet0.Cells[1, 3].Value = "���������Ӧ��";
            worksheet0.Cells[1, 4].Value = "��ʷ�����Ӧ��";
            for (int i = 0; i < equipmentNumber; i++)
            {
                worksheet0.Cells[1, 5 + i].Value = i + "x";
                worksheet0.Cells[1, 5 + equipmentNumber + i].Value = i + "y";
            }
            package.Save();
        }
        return path;
    }

    private void Save_Every_Popsize_Data(string path,string name,int i, int j, double fitness, double p_fit, double[,] x0) 
    {
        FileInfo _excelName = new FileInfo(path);
        using (ExcelPackage package = new ExcelPackage(_excelName))
        {
            ExcelWorksheet worksheet = package.Workbook.Worksheets["Every_Popsize"];

            worksheet.Cells[2 + popsize * i + j, 1].Value = i;
            worksheet.Cells[2 + popsize * i + j, 2].Value = j;
            worksheet.Cells[2 + popsize * i + j, 3].Value = fitness;
            worksheet.Cells[2 + popsize * i + j, 4].Value = p_fit;
            for (int k = 0; k < equipmentNumber; k++)
            {
                worksheet.Cells[2 + popsize * i + j, 5 + k].Value = x0[j,k].ToString("f4");
                worksheet.Cells[2 + popsize * i + j, 5 + equipmentNumber + k].Value = x0[j,k+equipmentNumber].ToString("f4");
            }
            package.Save();
        }

    }
    private void Save_Every_Generation_Data(string path,int i,int j,double fitness,double g_fit,double[,] x0)
    {
        FileInfo _excelName = new FileInfo(path);
        using (ExcelPackage package = new ExcelPackage(_excelName))
        {
            //�� excel ���ļ������ sheet������������
            ExcelWorksheet worksheet = package.Workbook.Worksheets["Every_Generation"];

            worksheet.Cells[2 + i, 1].Value = i;
            worksheet.Cells[2 + i, 2].Value = j;
            worksheet.Cells[2 + i, 3].Value = fitness;
            worksheet.Cells[2 + i, 4].Value = g_fit;
            for (int k = 0; k < equipmentNumber; k++)
            {
                worksheet.Cells[2 + i, 5 + k].Value = x0[j, k].ToString("f4");
                worksheet.Cells[2 + i, 5 + equipmentNumber + k].Value = x0[j, k + equipmentNumber].ToString("f4");
            }
            worksheet.Cells[2 + i, 30].Value = fitness_generation_500; 
            package.Save();
        }
    }
    public double Calute_gfitness(double[] fit) //�����������ӵĸ�����Ӧ��ֵ��
    {
        double fit_all = 0;
        for (int i = 0; i < equipmentNumber; i++)
        {
            fit_all += fit[i];
        }

        return fit_all;
    }
    /// <summary>
    /// ����ÿһ���豸������ֵ
    /// </summary>
    /// <param name="index1">���ӱ��</param>
    /// <param name="index2">�豸</param>
    /// 
    /// <returns>����ֵ</returns>
    public double func(int index1 ,int index2, double[,] variable, double T1, double T2)
    {
        double[,] x = variable;
        double f = 0;
        double Pe = 0;
        double Pe_h = 0;
        double Pe_g = 0;
        for (int i = 0; i < equipmentNumber; i++)
        {
            double xi = x[index1, index2];
            double yi = x[index1, index2 + equipmentNumber];
            double xj = x[index1, i];
            double yj = x[index1, i + equipmentNumber];
            double li = Equipment_Length[index2];
            double lj = Equipment_Length[i];
            double wi = Equipment_Width[index2];
            double wj = Equipment_Width[i];

            int fre_index = i * equipmentNumber + index2;
            if (Frequent_data[fre_index] > 0)
            {
                //Debug.Log(index + "��ǰ���ӵ�Ƶ������Ϊ��" + fre_index + "ֵΪ:"+ Frequent_data[fre_index]);
                double distance = Math.Abs(xi - xj) + Math.Abs(yi - yj);
                f =f + Frequent_data[fre_index] * distance * Cost_per_meter;//�豸�������ɱ�
            }
            if (i != index2)
            {
                double h = -Math.Abs(xi-xj) + 0.5*(li + lj) + Equipment_Min_Length -Math.Abs(yi-yj) +0.5*(wi+wj)+Equipment_Min_Width;
                Pe_h += Math.Abs(h) + h;//�豸����ײ�ͷ�����
                //Debug.Log("��ǰ�豸Ϊ��" + index2 + "���豸" + i + "֮��ĳͷ�Ϊ:"+h);
            }
            if (i == 0)
            {
                double g1 = 0.5 * li + Wall_Min_Length - xi;
                double g2 = 0.5 * wi + Wall_Min_Width - yi;
                double g3 = xi - (xlimit[1] - 0.5 * li - Wall_Min_Length);
                double g4 = yi - (ylimit[1] - 0.5 * wi - Wall_Min_Width);
                Pe_g =Math.Abs(g1) + g1 + Math.Abs(g2) + g2 + Math.Abs(g3) + g3 + Math.Abs(g4) + g4;//�豸��߽���ײ�ͷ�����
                //Debug.Log("��ǰ�豸Ϊ��" + index2 + "��ǽ֮��ĳͷ�Ϊ:" + Pe_g);
            }
        }
        Pe = 0.5 * (T1 * Pe_h + T2 * Pe_g);

        return f + Pe;
    }

    //��ʼ������
    public void Initialize(int popsize,int equipmentNumber, out double[,] x0, out double[,] x_pbest, out double[] p_fitness, out double[] x_gbest ,out double g_fitness, out double[,] V)
    {
        System.Random rand = new System.Random();
        double[,] x = new double[popsize,equipmentNumber];             //ÿ���豸��λ��
        double[,] v0 = new double[popsize, equipmentNumber];
            //{ 
            //    new double[13],
            //    new double[13]
            //};            //ÿ���豸���ٶ�
        //double[][] pbest = new double[popsize][];      //����λ����ʷ���λ��
        double[] p_fit = new double[popsize];             //������Ӧ����ʷ����ֵ
        double[] gbest = new double[equipmentNumber];     //Ⱥ����ʷ���λ��
                                                          
        int y_begin = equipmentNumber / 2;

        for (int i = 0; i < popsize; i++)
        {
            p_fit[i] = double.PositiveInfinity;           //������ʷ�����Ӧ�ȳ�ʼ��ѡȡ�������ע�⣡������

            for (int j = 0; j < equipmentNumber; j++)
            {
                v0[i,j] = rand.NextDouble();     //ÿ���豸�ٶȳ�ʼ��

                if (j < y_begin)
                {
                    x[i,j] = xlimit[0] + rand.NextDouble() * (xlimit[1] - xlimit[0]);          //Random.NextDouble:����һ�����ڻ���� 0.0 ��С�� 1.0 �����������
                    gbest[j] = xlimit[0] + rand.NextDouble() * (xlimit[1] - xlimit[0]);         //ȫ����ʷ���λ�ó�ʼ��
                }
                else
                {
                    x[i,j] = ylimit[0] + rand.NextDouble() * (ylimit[1] - ylimit[0]);          //y
                    gbest[j] = ylimit[0] + rand.NextDouble() * (ylimit[1] - ylimit[0]);           
                }
            }
        }
        if (OpenLspInit)
        {
             for (int j = 0; j < equipmentNumber; j++)
            {
                x[0, j] = Lsp_Init[j];
            }
        }

        x0 = x;
        x_pbest = x;
        p_fitness = p_fit;
        x_gbest = gbest;
        g_fitness = double.PositiveInfinity;
        V = v0;

    }

    // Start is called before the first frame update
    public void StartPSOFunction()
    {
        //string path = Generate_TotalExperimental_Tables(DataExportPath);
        for (int i = experimentNumber; i > 0; i--)
        {
            //Main(path,i);
            Main("", i);
        }
        //Debug.Log("Ŀ�꺯����ȫ����Сֵ��");
    }
    private void Start()
    {
        StartPSOFunction();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [Button("Start PSO Function")]
    void ButtonStartPSOFunction()
    {
        StartPSOFunction();
    }
}
