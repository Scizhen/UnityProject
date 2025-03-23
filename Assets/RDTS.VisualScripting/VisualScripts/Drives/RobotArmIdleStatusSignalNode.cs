using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
namespace RDTS.VisualScripting
{
    [UnitSubtitle("机械臂等待状态")]
    [UnitCategory("RDTS NodeLibrary/Drives")]
    public class RobotArmIdleStatusSignalNode : Unit
    {
        [DoNotSerialize]
        public ControlInput Set;//设置控制信号

        [DoNotSerialize]
        public ControlOutput SetOutput;//设置之后退出

        [DoNotSerialize]
        public ValueInput StartToPick;//设置控制信号的延迟时间
        [DoNotSerialize]
        public ValueInput MoveToPick;//控制信号
        [DoNotSerialize]
        public ValueInput PickFinish;//要设置的值
        [DoNotSerialize]
        public ValueInput PlaceFinish;//控制信号
        [DoNotSerialize]
        public ValueOutput NextStatus;//要设置的值

        private bool __NextStatusFlag = false;

        protected override void Definition()
        {
            Set = ControlInput("SetValue", (flow) => {

                bool _StartToPick = flow.GetValue<bool>(StartToPick);
                var _MoveToPick = flow.GetValue<ValueInputInt>(MoveToPick);
                var _PickFinish = flow.GetValue<ValueOutputBool>(PickFinish);
                var _PlaceFinish = flow.GetValue<ValueOutputBool>(PlaceFinish);

                if (_StartToPick == true)
                {
                    _MoveToPick.Value = 1;
                    _PickFinish.Value = false;
                    _PlaceFinish.Value = false;
                    __NextStatusFlag = true;
                }

                return SetOutput;
            });

            SetOutput = ControlOutput("SetOutput");
            //在节点中可视化输入端口
            StartToPick = ValueInput<bool>("StartToPick",false);
            MoveToPick = ValueInput<ValueInputInt>("MoveToPick");
            PickFinish = ValueInput<ValueOutputBool>("PickFinish");
            PlaceFinish = ValueInput<ValueOutputBool>("PlaceFinish");
            NextStatus = ValueOutput<string>("NextStatus", (flow) =>
            {
                    string _NextStatus = "Idle";
                    if (__NextStatusFlag == true)
                    {
                        _NextStatus = "PickObject";

                    }

                    return _NextStatus;
            });
            
        }
    }
}
