using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace RDTS.RobotSimulationProgramming
{
    [InstructionType(Instruction.MoveJoint)]
    [Serializable]
    public class RSP_MoveJoint : RSP_BaseMoveInstruction
    {
        public override string name => "MoveJ";



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