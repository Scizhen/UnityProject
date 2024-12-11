using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RDTS.RobotSimulationProgramming
{
    [InstructionType(Instruction.PauseTime)]
    [Serializable]
    public class RSP_PauseTime : BaseInstruction
    {
        public override string name => "Pause ";

        [InstructionInput(InstructionInput.Value)]
        public float inValue;


        public override void Process(RSP_RobotController robotController, out bool isFinish)
        {
            isFinish = false;
            if (robotController == null)
                return;

            /* 设置速度值 */
            if (RSP_RobotController.SetValueOfPauseTime(robotController, inValue))
            {
                isFinish = true;
            }


        }
    }

}
