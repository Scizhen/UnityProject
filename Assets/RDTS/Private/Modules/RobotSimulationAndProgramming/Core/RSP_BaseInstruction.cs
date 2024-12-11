using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RDTS.RobotSimulationProgramming
{
    public enum Instruction
    {
        MoveJoint = 0,//�ؽ��˶�
        MoveCurve,//�����˶�
        MoveStraightLine,//ֱ���˶�
        SetParameter,//���ò���
        WaitParameter,//�ȴ�����
        SetSpeed,//�����ٶ�
        PauseTime//�ݶ�ʱ��
    }


    public enum InstructionInput
    {
        Target = 0,//Ŀ���
        Parameter,//����
        Value//��ֵ
    }



    [Serializable]
    public abstract class BaseInstruction
    {
        [SerializeField] public Texture2D icon;//ָ��ͼ��
        [SerializeField] public virtual string name => GetType().Name;//ָ������
        [SerializeField] public RSP_RobotProgram program;//�����ĳ����ļ�





        public static BaseInstruction CreateFromType(Type instructionType)
        {
            if (!instructionType.IsSubclassOf(typeof(BaseInstruction)))//�˽ڵ����Ͳ���BaseNode������
                return null;

            //Activator�������ض��ķ����������ڱ��ػ��Զ�̴����������ͣ����ȡ������Զ�̶�������á� ���಻�ܱ��̳�
            //CreateInstance��ʹ�����͵��޲������캯������ָ�����͵�ʵ��
            var instruction = Activator.CreateInstance(instructionType) as BaseInstruction;//����nodetype�Ķ��󣬲����ض��������

            return instruction;
        }


        /// <summary>
        /// ��д�÷���ʵ���Զ��崦��
        /// </summary>
        public virtual void Process(RSP_RobotController robotController, out bool isFinish) { isFinish = false; }









    }









}
