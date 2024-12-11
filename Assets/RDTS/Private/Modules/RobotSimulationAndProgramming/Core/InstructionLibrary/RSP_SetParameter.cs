using System;
using System.Collections;
using System.Collections.Generic;
using RDTS.Utility;
using UnityEngine;


namespace RDTS.RobotSimulationProgramming
{
    [InstructionType(Instruction.SetParameter)]
    [Serializable]
    public class RSP_SetParameter : BaseInstruction
    {
        public override string name => "Set ";

        [InstructionInput(InstructionInput.Parameter)]
        public string inParameter;

        [InstructionInput(InstructionInput.Value)]
        public int inParaValue;


        public RSP_SetParameter()
        {

        }


        public RSP_SetParameter(Texture2D icon, string name, RSP_RobotProgram program)
        {
            this.icon = icon;
            this.program = program;
        }


        public override void Process(RSP_RobotController robotController, out bool isFinish)
        {
            isFinish = false;
            if (robotController == null)
                return;

            /* 设置参数值 */
            RSP_RobotController.SetValueOfOutParameter(robotController, inParameter, inParaValue);
            isFinish = true;


        }


    }


}