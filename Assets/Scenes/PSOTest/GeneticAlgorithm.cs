using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GeneticAlgorithm : MonoBehaviour
{
    public double[,] GeneticCalcute(double[,] x0, double[] cur_fitness, int gen_size, double Variation_Pro, int equipmentNumber)
    {
        int popsize = cur_fitness.Length;//50
        double[] fitness = new double[cur_fitness.Length];
        Array.Copy(cur_fitness, fitness, cur_fitness.Length);
        int choose_num = gen_size;//10
        //double[] pop_fitness = PopArry(fitness);    //������Ӧ������
        Array.Sort(fitness);
        double losetempCount = fitness[popsize-choose_num ];//40
        double goodtempCount = fitness[choose_num-1];//9
        int[] choose_Count = new int[choose_num];//0-9
        int[] lose_Count = new int[choose_num];//0-9
        int index_good = 0;
        int index_lose = 0;
        for (int i = 0; i < popsize; i++)   //ɸѡ���������
        {
            if ((cur_fitness[i] <= goodtempCount )&&(index_good < choose_num))
            {
                choose_Count[index_good] = i;
                index_good++;
            }
            if ((cur_fitness[i] >= losetempCount) && (index_lose < choose_num))
            {
                lose_Count[index_lose] = i;
                index_lose++;
            }
        }
        //����
        for (int i = 0; i < choose_num / 2; i++)
        {
            int index1 = i * 2;
            int index2 = 2 * i + 1;
            int x_index1 = choose_Count[index1];
            int x_index2 = choose_Count[index2];
            int xlose_index1 = lose_Count[index1];
            int xlose_index2 = lose_Count[index2];
            x0 = CrossAttr(x0, popsize, equipmentNumber, x_index1, x_index2, xlose_index1, xlose_index2);
        }
        //����
        for (int i = 0; i < popsize; i++)   
        {

            int curPop_Probable = UnityEngine.Random.Range(0, 100);//�������ӵı������

            if (curPop_Probable < Variation_Pro * 100)
            {
                int VarEquipment = UnityEngine.Random.Range(0, equipmentNumber - 1);//���α�����豸
                //Debug.Log("����Ļ������Ϊ��" + i + "������豸Ϊ" + VarEquipment);
                double Variation_x = 0.0001 * UnityEngine.Random.Range(-2000, 2000);
                double Variation_y = 0.0001 * UnityEngine.Random.Range(-2000, 2000);
                x0[i, VarEquipment] = x0[i, VarEquipment] + Variation_x;
                x0[i, VarEquipment + equipmentNumber] = x0[i, VarEquipment + equipmentNumber] + Variation_y;

            }

        }
        return x0;
    }

    private double[,] CrossAttr(double[,]x0,int popsize, int length, int x_index1, int x_index2, int xlose_index1, int xlose_index2)
    {
        double[,] childx0 = new double[popsize, 2*length];
        Array.Copy(x0, childx0, x0.Length);
        int count = length;//����ĳ���
        int Cross_count = UnityEngine.Random.Range(1, 4);//��������Ĵ���1~4�����

        for (int j = 0; j < Cross_count; j++)
        {
            int index1 = UnityEngine.Random.Range(0, count - 1);//���ν�������������豸
            int index2 = UnityEngine.Random.Range(0, count - 1);
            //��ʼ����
            double tempx = childx0[x_index1, index1];
            double tempy = childx0[x_index1, index1 + length];
            childx0[x_index1, index1] = childx0[x_index2, index2];
            childx0[x_index1, index1 + length] = childx0[x_index2, index2 + length];
            childx0[x_index2, index2] = tempx;
            childx0[x_index2, index2 + length] = tempy;
        }
        //��̭������������
        for (int i = 0; i < length; i++)
        {
            x0[xlose_index1, i] = childx0[x_index1, i];
            x0[xlose_index1, i + length] = childx0[x_index1, i + length];
            x0[xlose_index2, i] = childx0[x_index2, i];
            x0[xlose_index2, i + length] = childx0[x_index2, i + length];
        }

        return x0;

    }

    public double[] PopArry(double[] cur)
    {
        for (int i = 0; i < cur.Length; i++)
        {
            for (int n = 0; n < cur.Length - 1; n++)
            {
                //�����n�����ȵ�n+1���������Ǿ�Ҫ����λ��
                if (cur[n] > cur[n + 1])
                {
                    //����λ��
                    double temp = cur[n];
                    cur[n] = cur[n + 1];
                    cur[n + 1] = temp;

                }
            }
        }
        return cur;
    }
}
