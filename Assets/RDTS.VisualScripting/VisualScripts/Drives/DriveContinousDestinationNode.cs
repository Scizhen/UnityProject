using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using RDTS.Utility;

namespace RDTS.VisualScripting
{
    [UnitSubtitle("连续目标点驱动行为")]
    [UnitCategory("RDTS NodeLibrary/Drives")]
    public class DriveContinousDestinationNode : Unit
    {

        [DoNotSerialize]
        public ControlInput Set;//设置控制信号

        [DoNotSerialize]
        public ControlOutput SetOutput;//设置之后退出

        [DoNotSerialize]
        public ValueInput Drive;//需要搭载在哪个Drive上
        [DoNotSerialize]
        public ValueInput Destination;//设置移动距离
        [DoNotSerialize]
        public ValueInput Acceleration;//设置加速度
        [DoNotSerialize]
        public ValueInput TargetSpeed;//设置目标速度
        [DoNotSerialize]
        public ValueOutput IsAtPosition;//如果驱动器在目的地位置，信号为真
        [DoNotSerialize]
        public ValueOutput IsAtSpeed;//当前驱动速度信号，单位为毫米/秒
        [DoNotSerialize]
        public ValueOutput IsAtDestination;//如果驱动器到达目的地则为true
        [DoNotSerialize]
        public ValueOutput IsDriving;//如果Drive当前正在行驶，则信号为true。

        private float destinationbefore;
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

                //读取PLC信号
                //if (Destination != null)
                //    _NewDestination = flow.GetValue<ValueOutputFloat>(Destination).Value;
                //if (Acceleration != null)
                //    _NewAcceleration = flow.GetValue<ValueOutputFloat>(Acceleration).Value;
                //if (TargetSpeed != null)
                //    _NewTargetSpeed = flow.GetValue<ValueOutputFloat>(TargetSpeed).Value;

                //若有输入数值则输入优先
                //if(flow.GetValue<float>(Destination) != 0)
                    _NewDestination = flow.GetValue<float>(Destination);
                //if (flow.GetValue<float>(Acceleration) != 100)
                    _NewAcceleration = flow.GetValue<float>(Acceleration);
                //if (flow.GetValue<float>(TargetSpeed) != 100)
                    _NewTargetSpeed = flow.GetValue<float>(TargetSpeed);

                if (_NewDestination != destinationbefore)
                    getdrive.DriveTo(_NewDestination);//控制Drive运动
                getdrive.TargetSpeed = _NewTargetSpeed;
                getdrive.Acceleration = _NewAcceleration;


                destinationbefore = _NewDestination;
                return SetOutput;
            });

            SetOutput = ControlOutput("SetOutput");

            //在节点中可视化输入端口
            Drive = ValueInput<GameObject>("Drive");
            //Destination = ValueInput<ValueOutputFloat>("Destination");
            Destination = ValueInput<float>("Destination",0f);
            //Acceleration = ValueInput<ValueOutputFloat>("Acceleration");
            Acceleration = ValueInput<float>("Acceleration",100f);
            //TargetSpeed = ValueInput<ValueOutputFloat>("TargetSpeed");
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
