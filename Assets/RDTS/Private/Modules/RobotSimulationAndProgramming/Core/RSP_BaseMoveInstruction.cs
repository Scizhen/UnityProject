using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace RDTS.RobotSimulationProgramming
{

    /// <summary>
    /// Move类型指令的基类
    /// </summary>
    [Serializable]
    public abstract class RSP_BaseMoveInstruction : BaseInstruction
    {

        [InstructionInput(InstructionInput.Target)]
        public string inTarget;

        // public override void Process(RSP_RobotController robotController, out bool isFinish)
        // {
        //     isFinish = false;
        //     if (robotController == null)
        //         return;

        //     /* 设置参数值是否达到指定值 */
        //     if (RSP_RobotController.MoveToTarget(robotController, inTarget))
        //     {
        //         isFinish = true;
        //     }


        // }

    }


}