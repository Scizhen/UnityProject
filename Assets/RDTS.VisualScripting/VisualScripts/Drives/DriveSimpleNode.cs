using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using RDTS.Utility;

namespace RDTS.VisualScripting
{
    [UnitSubtitle("简易的驱动行为")]
    [UnitCategory("RDTS NodeLibrary/Drives")]
    public class DriveSimpleNode : Unit
    {

        [DoNotSerialize]
        public ControlInput Set;//设置控制信号

        [DoNotSerialize]
        public ControlOutput SetOutput;//设置之后退出


        [DoNotSerialize]
        public ValueInput Drive;//需要搭载在哪个Drive上
        [DoNotSerialize]
        public ValueInput Speed;//速度
        [DoNotSerialize]
        public ValueInput Accelaration;//加速度
        [DoNotSerialize]
        public ValueInput Forward;//前进
        [DoNotSerialize]
        public ValueInput Backward;//后退

        [DoNotSerialize]
        public ValueOutput IsAtPosition;//反馈目前位置
        [DoNotSerialize]
        public ValueOutput IsAtSpeed;//反馈目前速度
        [DoNotSerialize]
        public ValueOutput IsDriving;//反馈目前运行状态


        private Drive getdrive;
        private float _NewSpeed;
        private float _NewAccelaration;
        private bool _NewForward;
        private bool _NewBackward;
        private float _NewIsAtPosition;
        private float _NewIsAtSpeed;
        private bool _NewIsDriving;

        protected override void Definition()
        {
            Set = ControlInput("SetValue", (flow) => {

                var _Drive = flow.GetValue<GameObject>(Drive);

                getdrive = _Drive.GetComponent<Drive>();
                if (getdrive == null)
                    Debug.LogError("该自定义节点缺少缺少Drive脚本");

                _NewSpeed = flow.GetValue<float>(Speed);
                _NewAccelaration = flow.GetValue<float>(Accelaration);
                _NewForward = flow.GetValue<bool>(Forward);
                _NewBackward = flow.GetValue<bool>(Backward);

                getdrive.TargetSpeed = _NewSpeed;
                getdrive.JogForward = _NewForward;
                getdrive.JogBackward = _NewBackward;
                getdrive.Acceleration = _NewAccelaration;

                _NewIsAtPosition = getdrive.CurrentPosition;
                _NewIsAtSpeed = getdrive.CurrentSpeed;
                _NewIsDriving = getdrive.IsRunning;

                return SetOutput;
            });

            SetOutput = ControlOutput("SetOutput");

            //在节点中可视化输入端口
            Drive = ValueInput<GameObject>("Drive");
            Speed = ValueInput<float>("Speed", 100f);
            Accelaration = ValueInput<float>("Accelaration", 100f);
            Forward = ValueInput<bool>("Forward", false);
            Backward = ValueInput<bool>("Backward",false);

            IsAtPosition = ValueOutput<float>("IsAtPosition", (flow) =>
            {
                return _NewIsAtPosition;
            });
            IsAtSpeed = ValueOutput<float>("IsAtSpeed", (flow) =>
            {
                return _NewIsAtSpeed;
            });
            IsDriving = ValueOutput<bool>("IsDriving", (flow) =>
            {
                return _NewIsDriving;
            });
        }

    }


}
