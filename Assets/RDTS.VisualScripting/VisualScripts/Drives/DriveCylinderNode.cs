using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using RDTS.Utility;
using System;


namespace RDTS.VisualScripting
{
    [UnitSubtitle("气缸驱动行为")]
    [UnitCategory("RDTS NodeLibrary/Drives")]
    public class DriveCylinderNode : Unit
    {

        [DoNotSerialize]
        public ControlInput Set;//设置控制信号

        [DoNotSerialize]
        public ControlOutput SetOutput;//设置之后退出

        [DoNotSerialize]
        public ValueInput Drive;//需要搭载在哪个Drive上
        [DoNotSerialize]
        public ValueInput OneBitCylinder;//是否使用单一信号控制，若是则通过Out的值即可实现驱动行为
        [DoNotSerialize]
        public ValueInput MinPos;//开始位置
        [DoNotSerialize]
        public ValueInput MaxPos;//结束位置
        [DoNotSerialize]
        public ValueInput TimeOut;//向外运行时间
        [DoNotSerialize]
        public ValueInput TimeIn;//向内运行时间
        [DoNotSerialize]
        public ValueInput StopWhenDrivingToMin;//传感器判断到达min
        [DoNotSerialize]
        public ValueInput StopWhenDrivingToMax;//传感器判断到达max
        [DoNotSerialize]
        public ValueInput Out;//向外
        [DoNotSerialize]
        public ValueInput In;//向内
        [DoNotSerialize]
        public ValueOutput IsOut;//如果驱动器在目的地位置，信号为真
        [DoNotSerialize]
        public ValueOutput IsIn;//如果驱动器在目的地位置，信号为真
        [DoNotSerialize]
        public ValueOutput IsMax;//如果到达max位置，则信号为true。
        [DoNotSerialize]
        public ValueOutput IsMin;//如果到达min位置，则信号为true。
        [DoNotSerialize]
        public ValueOutput IsMovingOut;//如果Drive当前正在行驶，则信号为true。
        [DoNotSerialize]
        public ValueOutput IsMovingIn;//如果Drive当前正在行驶，则信号为true。

        public bool _isOut = false; //!< is true when cylinder is out or stopped by Max sensor.
        public bool _isIn = false; //!<  is true when cylinder is in or stopped by Min sensor.
        public bool _movingOut = false; //!<  is true when cylinder is currently moving out
        public bool _movingIn = false; //!<  is true when cylinder is currently moving in
        public bool _isMax = false; //!< is true when cylinder is at maximum position.
        public bool _isMin = false; //!< is true when cylinder is at minimum position.

        private Drive Cylinder;
        private bool flagStart = true;


        protected override void Definition()
        {
            Set = ControlInput("SetValue", (flow) => {

                var _Drive = flow.GetValue<GameObject>(Drive);
                bool _OneBitCylinder = flow.GetValue<bool>(OneBitCylinder);
                float _MinPos = flow.GetValue<float>(MinPos);
                float _MaxPos = flow.GetValue<float>(MaxPos);
                float _TimeOut = flow.GetValue<float>(TimeOut);
                float _TimeIn = flow.GetValue<float>(TimeIn);
                bool _StopWhenDrivingToMin = flow.GetValue<bool>(StopWhenDrivingToMin);
                bool _StopWhenDrivingToMax = flow.GetValue<bool>(StopWhenDrivingToMax);
                bool _Out = flow.GetValue<bool>(Out);
                bool _In = flow.GetValue<bool>(In);

                //程序运行时执行一次，相当于Start()
                if (flagStart == true)
                {
                    Cylinder = _Drive.GetComponent<Drive>();
                    if (Cylinder == null)
                        Debug.LogError("该自定义节点缺少缺少Drive脚本");
                    Cylinder.CurrentPosition = _MinPos;
                    _isOut = false; //!< is true when cylinder is out or stopped by Max sensor.
                    _isIn = false; //!<  is true when cylinder is in or stopped by Min sensor.
                    _movingOut = false; //!<  is true when cylinder is currently moving out
                    _movingIn = false; //!<  is true when cylinder is currently moving in
                    _isMax = false; //!< is true when cylinder is at maximum position.
                    _isMin = false; //!< is true when cylinder is at minimum position.

                    flagStart = false;
                }

                // Moving Stopped at Min or Maxpos
                if (_movingOut && Cylinder.CurrentPosition == _MaxPos)
                {
                    _movingOut = false;
                    _movingIn = false;
                    _isOut = true;
                }
                if (_movingIn && Cylinder.CurrentPosition == _MinPos)
                {
                    _movingIn = false;
                    _movingOut = false;
                    _isIn = true;
                }
                //用于传感器检测信号的接入
                if (_StopWhenDrivingToMin)
                {
                    _movingIn = false;
                    _movingOut = false;
                    _isIn = true;
                    Cylinder.Stop();
                }

                if (_StopWhenDrivingToMax)
                {
                    _movingIn = false;
                    _movingOut = false;
                    _isOut = true;
                    Cylinder.Stop();
                }

                // At Maxpos
                if (Cylinder.CurrentPosition == _MaxPos)
                    _isMax = true;
                else
                    _isMax = false;

                // At Minpos
                if (Cylinder.CurrentPosition == _MinPos)
                    _isMin = true;
                else
                    _isMin = false;

                // Start to Move Cylinder
                if (!(_Out && _In) && !_OneBitCylinder)
                {
                    if (_Out && !_isOut && !_movingOut)
                    {
                        Cylinder.TargetSpeed = Math.Abs(_MaxPos - _MinPos) / _TimeOut;
                        Cylinder.DriveTo(_MaxPos);
                        _movingOut = true;
                        _movingIn = false;
                        _isIn = false;
                    }
                    if (_In && !_isIn && !_movingIn)
                    {
                        Cylinder.TargetSpeed = Math.Abs(_MaxPos - _MinPos) / _TimeIn;
                        Cylinder.DriveTo(_MinPos);
                        _isOut = false;
                        _movingIn = true;
                        _movingOut = false;
                    }
                }
                else
                {
                    if (_Out && !_isOut && !_movingOut)
                    {
                        Cylinder.TargetSpeed = Math.Abs(_MaxPos - _MinPos) / _TimeOut;
                        Cylinder.DriveTo(_MaxPos);
                        _movingOut = true;
                        _movingIn = false;
                        _isIn = false;
                    }

                    if (!_Out && !_isIn && !_movingIn)
                    {
                        Cylinder.TargetSpeed = Math.Abs(_MaxPos - _MinPos) / _TimeIn;
                        Cylinder.DriveTo(_MinPos);
                        _isOut = false;
                        _movingIn = true;
                        _movingOut = false;
                    }
                }
                return SetOutput;
            });

            SetOutput = ControlOutput("SetOutput");

            //在节点中可视化输入端口
            Drive = ValueInput<GameObject>("Drive");
            OneBitCylinder = ValueInput<bool>("OneBitCylinder",false);
            MinPos = ValueInput<float>("MinPos",0f);
            MaxPos = ValueInput<float>("MaxPos",100f);
            TimeOut = ValueInput <float>("TimeOut",1f);
            TimeIn = ValueInput<float>("TimeIn",1f);
            StopWhenDrivingToMin = ValueInput<bool>("StopWhenDrivingToMin",false);
            StopWhenDrivingToMax = ValueInput<bool>("StopWhenDrivingToMax",false);
            Out = ValueInput<bool>("Out",false);
            In = ValueInput<bool>("In",false);

            IsOut = ValueOutput<bool>("IsOut",(flow)=> 
            {
                return _isOut;
            });
            IsIn = ValueOutput<bool>("IsIn",(flow)=> 
            {
                return _isIn;
            });
            IsMax = ValueOutput<bool>("IsMax",(flow)=> 
            {
                return _isMax;
            });
            IsMin = ValueOutput<bool>("IsMin", (flow) =>
            {
                return _isMin;
            });
            IsMovingOut = ValueOutput<bool>("IsMovingOut", (flow) =>
            {
                return _movingOut;
            });
            IsMovingIn = ValueOutput<bool>("IsMovingIn", (flow) =>
            {
                return _movingIn;
            });

        }


    }


}
