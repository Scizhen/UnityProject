using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RDTS.RobotSimulationProgramming
{
    [InstructionType(Instruction.WaitParameter)]
    [Serializable]
    public class RSP_WaitParameter : BaseInstruction
    {
        public override string name => "Wait ";

        [InstructionInput(InstructionInput.Parameter)]
        public string inParameter;

        [InstructionInput(InstructionInput.Value)]
        public int inParaValue;


        public override void Process(RSP_RobotController robotController, out bool isFinish)
        {
            isFinish = false;
            if (robotController == null)
                return;

            /* 设置参数值是否达到指定值 */
            if (RSP_RobotController.WaitValueOfOutParameter(robotController, inParameter, inParaValue))
            {
                isFinish = true;
            }


        }





    }


}