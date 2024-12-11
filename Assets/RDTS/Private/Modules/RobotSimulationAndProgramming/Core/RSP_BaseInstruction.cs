using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RDTS.RobotSimulationProgramming
{
    public enum Instruction
    {
        MoveJoint = 0,//关节运动
        MoveCurve,//曲线运动
        MoveStraightLine,//直线运动
        SetParameter,//设置参数
        WaitParameter,//等待参数
        SetSpeed,//设置速度
        PauseTime//暂定时间
    }


    public enum InstructionInput
    {
        Target = 0,//目标点
        Parameter,//参数
        Value//数值
    }



    [Serializable]
    public abstract class BaseInstruction
    {
        [SerializeField] public Texture2D icon;//指令图标
        [SerializeField] public virtual string name => GetType().Name;//指令名称
        [SerializeField] public RSP_RobotProgram program;//所属的程序文件





        public static BaseInstruction CreateFromType(Type instructionType)
        {
            if (!instructionType.IsSubclassOf(typeof(BaseInstruction)))//此节点类型不是BaseNode的子类
                return null;

            //Activator：包含特定的方法，用以在本地或从远程创建对象类型，或获取对现有远程对象的引用。 此类不能被继承
            //CreateInstance：使用类型的无参数构造函数创建指定类型的实例
            var instruction = Activator.CreateInstance(instructionType) as BaseInstruction;//创建nodetype的对象，并返回对其的引用

            return instruction;
        }


        /// <summary>
        /// 重写该方法实现自定义处理
        /// </summary>
        public virtual void Process(RSP_RobotController robotController, out bool isFinish) { isFinish = false; }









    }









}
