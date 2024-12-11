using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using RDTS.Utility;

namespace RDTS.VisualScripting
{
    [UnitSubtitle("连续目标点驱动行为")]
    [UnitCategory("RDTS NodeLibrary/Drives")]
    public class DriveDestinationMotorNode : Unit
    {

        [DoNotSerialize]
        public ControlInput Set;//设置控制信号

        [DoNotSerialize]
        public ControlOutput SetOutput;//设置之后退出

        [DoNotSerialize]
        public ValueInput Drive;//需要搭载在哪个Drive上
        [DoNotSerialize]
        public ValueInput StartDrive;//设置移动距离
        [DoNotSerialize]
        public ValueInput Destination;//设置加速度
        [DoNotSerialize]
        public ValueInput Acceleration;//设置目标速度
        [DoNotSerialize]
        public ValueInput TargetSpeed;//如果驱动器在目的地位置，信号为真
        [DoNotSerialize]
        public ValueOutput IsAtPosition;//当前驱动速度信号，单位为毫米/秒
        [DoNotSerialize]
        public ValueOutput IsAtSpeed;//当前驱动速度信号，单位为毫米/秒
        [DoNotSerialize]
        public ValueOutput IsAtDestination;//如果驱动器到达目的地则为true
        [DoNotSerialize]
        public ValueOutput IsDriving;//如果Drive当前正在行驶，则信号为true。

        private bool _NewStartDrive;
        private float _NewDestination;
        private float _NewAcceleration;
        private float _NewTargetSpeed;
        private Drive getdrive;


        protected override void Definition()
        {
            Set = ControlInput("SetValue", (flow) => {

                var _Drive = flow.GetValue<GameObject>(Drive);

                getdrive = _Drive.GetComponent<Drive>();
                if (getdrive == null)
                    Debug.LogError("该自定义节点缺少缺少Drive脚本");

                _NewStartDrive = flow.GetValue<bool>(StartDrive);
                _NewDestination = flow.GetValue<float>(Destination);
                _NewAcceleration = flow.GetValue<float>(Acceleration);
                _NewTargetSpeed = flow.GetValue<float>(TargetSpeed);

                getdrive.TargetStartMove = _NewStartDrive;
                getdrive.TargetPosition = _NewDestination;
                getdrive.TargetSpeed = _NewTargetSpeed;
                getdrive.Acceleration = _NewAcceleration;

                return SetOutput;
            });

            SetOutput = ControlOutput("SetOutput");

            //在节点中可视化输入端口
            Drive = ValueInput<GameObject>("Drive");
            StartDrive = ValueInput<bool>("StartDrive",false);
            Destination = ValueInput<float>("Destination",0f);
            Acceleration = ValueInput<float>("Acceleration",100f);
            TargetSpeed = ValueInput<float>("TargetSpeed",100f);


            IsAtPosition = ValueOutput<float>("IsAtPosition",(flow)=>
            {

                float _IsAtPostion = getdrive.CurrentPosition;
                return _IsAtPostion;
            });
            IsAtSpeed = ValueOutput<float>("IsAtSpeed",(flow)=>
            {
                float _IsAtSpeed = getdrive.CurrentSpeed;
                return _IsAtSpeed;
            });
            IsAtDestination = ValueOutput<bool>("IsAtDestination", (flow) => 
            {

                bool _IsAtDestination = getdrive.IsAtTarget;
                return _IsAtDestination;
            });
            IsDriving = ValueOutput<bool>("IsDriving", (flow) => 
            {
                bool _IsDriving = getdrive.IsRunning;
                return _IsDriving;
            });


        }

    }


}
