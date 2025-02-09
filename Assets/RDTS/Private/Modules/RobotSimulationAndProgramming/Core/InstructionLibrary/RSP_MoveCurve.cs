using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RDTS.RobotSimulationProgramming
{
    [InstructionType(Instruction.MoveCurve)]
    [Serializable]
    public class RSP_MoveCurve : RSP_BaseMoveInstruction
    {
        public override string name => "MoveC";

        public override void Process(RSP_RobotController robotController, out bool isFinish)
        {
            isFinish = false;
            if (robotController == null)
                return;

            /* 设置参数值是否达到指定值 */
            if (RSP_RobotController.MoveToGivenTarget(robotController, inTarget))
            {
                isFinish = true;
            }


        }

    }

}
