using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace RDTS.RobotSimulationProgramming
{

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class InstructionTypeAttribute : Attribute
    {
        public Instruction instruction;//指令类型

        public InstructionTypeAttribute(Instruction instruction)
        {
            this.instruction = instruction;
        }


    }


    public class InstructionInputAttribute : Attribute
    {
        public InstructionInput input;//输入类型

        public InstructionInputAttribute(InstructionInput input)
        {
            this.input = input;
        }
    }



}