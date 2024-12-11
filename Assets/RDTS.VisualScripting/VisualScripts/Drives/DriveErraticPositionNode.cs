using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using RDTS.Utility;

namespace RDTS.VisualScripting
{
    [UnitSubtitle("不规律驱动行为")]
    [UnitCategory("RDTS NodeLibrary/Drives")]
    public class DriveErraticPositionNode : Unit
    {

        [DoNotSerialize]
        public ControlInput Set;//设置控制信号

        [DoNotSerialize]
        public ControlOutput SetOutput;//设置之后退出

        [DoNotSerialize]
        public ValueInput Drive;//需要搭载在哪个Drive上
        [DoNotSerialize]
        public ValueInput MinPos;//设置最小移动距离
        [DoNotSerialize]
        public ValueInput MaxPos;//设置最大移动距离
        [DoNotSerialize]
        public ValueInput Speed;//设置目标速度
        [DoNotSerialize]
        public ValueInput Driving;//是否开始运动

        private Drive getdrive;
        private float _NewMinPos;
        private float _NewMaxPos;
        private float _NewSpeed;
        private bool _NewDriving;
        private float _destpos;

        protected override void Definition()
        {
            Set = ControlInput("SetValue", (flow) => {

                var _Drive = flow.GetValue<GameObject>(Drive);

                getdrive = _Drive.GetComponent<Drive>();
                if (getdrive == null)
                    Debug.LogError("该自定义节点缺少缺少Drive脚本");

                _NewMinPos = flow.GetValue<float>(MinPos);
                _NewMaxPos = flow.GetValue<float>(MaxPos);
                _NewSpeed = flow.GetValue<float>(Speed);
                _NewDriving = flow.GetValue<bool>(Driving);

                if (getdrive.UseLimits)
                {
                    _NewMinPos = getdrive.LowerLimit;
                    _NewMaxPos = getdrive.UpperLimit;
                }

                if (_NewDriving && !getdrive.IsRunning && getdrive.CurrentPosition != _destpos)
                {
                    getdrive.TargetPosition = _destpos;
                    getdrive.TargetStartMove = true;
                }

                if (_NewDriving == false)
                {
                    getdrive.TargetSpeed = _NewSpeed;
                    getdrive.TargetPosition = Random.Range(_NewMinPos, _NewMaxPos);
                    getdrive.TargetStartMove = true;
                    _NewDriving = true;
                    _destpos = getdrive.TargetPosition;
                }
                else
                if (getdrive.IsRunning && _NewDriving == true)
                {
                    getdrive.TargetStartMove = false;
                }

                if (getdrive.CurrentPosition == _destpos && _NewDriving == true)
                {
                    _NewDriving = false;
                }

                return SetOutput;
            });

            SetOutput = ControlOutput("SetOutput");

            //在节点中可视化输入端口
            Drive = ValueInput<GameObject>("Drive");
            MinPos = ValueInput<float>("MinPos", 0f);
            MaxPos = ValueInput<float>("MaxPos", 0f);
            Speed = ValueInput<float>("Speed", 100f);
            Driving = ValueInput<bool>("Stop Driving", false);

        }

    }


}
