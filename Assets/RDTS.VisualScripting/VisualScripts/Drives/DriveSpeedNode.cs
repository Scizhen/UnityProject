using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using RDTS.Utility;

namespace RDTS.VisualScripting
{
    [UnitSubtitle("速度驱动模型")]
    [UnitCategory("RDTS NodeLibrary/Drives")]
    public class DriveSpeedNode : Unit
    {

        [DoNotSerialize]
        public ControlInput Set;//设置控制信号

        [DoNotSerialize]
        public ControlOutput SetOutput;//设置之后退出


        [DoNotSerialize]
        public ValueInput Drive;//需要搭载在哪个Drive上
        [DoNotSerialize]
        public ValueInput TargetSpeed;//目标速度
        [DoNotSerialize]
        public ValueInput Accelaration;//加速度

        [DoNotSerialize]
        public ValueOutput CurrentSpeed;//反馈目前速度
        [DoNotSerialize]
        public ValueOutput CurrentPosition;//反馈目前位置
        [DoNotSerialize]
        public ValueOutput IsDriving;//反馈目前运行状态


        private Drive getdrive;
        private float _NewTargetSpeed;
        private float _NewAccelaration;
        private float _NewCurrentSpeed;
        private float _NewCurrentPosition;
        private bool _NewIsDriving;

        protected override void Definition()
        {
            Set = ControlInput("SetValue", (flow) => {

                var _Drive = flow.GetValue<GameObject>(Drive);

                getdrive = _Drive.GetComponent<Drive>();
                if (getdrive == null)
                    Debug.LogError("该自定义节点缺少Drive脚本");

                _NewTargetSpeed = flow.GetValue<float>(TargetSpeed);
                _NewAccelaration = flow.GetValue<float>(Accelaration);

                getdrive.TargetSpeed = Mathf.Abs(_NewTargetSpeed);

                if (_NewTargetSpeed > 0)
                {
                    getdrive.JogForward = true;
                    getdrive.JogBackward = false;
                }

                if (_NewTargetSpeed == 0)
                {
                    getdrive.JogForward = false;
                    getdrive.JogBackward = false;
                }

                if (_NewTargetSpeed < 0)
                {
                    getdrive.JogForward = false;
                    getdrive.JogBackward = true;
                }
                getdrive.Acceleration = _NewAccelaration;

                _NewCurrentPosition = getdrive.CurrentPosition;
                _NewCurrentSpeed = getdrive.CurrentSpeed;
                _NewIsDriving = getdrive.IsRunning;

                return SetOutput;
            });

            SetOutput = ControlOutput("SetOutput");

            //在节点中可视化输入端口
            Drive = ValueInput<GameObject>("Drive");
            TargetSpeed = ValueInput<float>("Speed", 0f);
            Accelaration = ValueInput<float>("Accelaration", 100f);

            CurrentSpeed = ValueOutput<float>("CurrentSpeed", (flow) =>
            {
                return _NewCurrentSpeed;
            });
            CurrentPosition = ValueOutput<float>("CurrentPosition", (flow) =>
            {
                return _NewCurrentPosition;
            });
            IsDriving = ValueOutput<bool>("IsDriving", (flow) =>
            {
                return _NewIsDriving;
            });
        }

    }


}
