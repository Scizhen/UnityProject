using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RDTS.RobotSimulationProgramming
{
    [InstructionType(Instruction.SetSpeed)]
    [Serializable]
    public class RSP_SetSpeed : BaseInstruction
    {
        public override string name => "Speed ";

        [InstructionInput(InstructionInput.Value)]
        public float inValue;


        public override void Process(RSP_RobotController robotController, out bool isFinish)
        {
            isFinish = false;
            if (robotController == null)
                return;

            /* 设置速度值 */
            RSP_RobotController.SetValueOfSpeed(robotController, inValue);
            isFinish = true;


        }
    }

}
