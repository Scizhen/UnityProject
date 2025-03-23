using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using RDTS.Utility;
using System;


namespace RDTS.VisualScripting
{
    [UnitSubtitle("����������Ϊ")]
    [UnitCategory("RDTS NodeLibrary/Drives")]
    public class DriveCylinderNode : Unit
    {

        [DoNotSerialize]
        public ControlInput Set;//���ÿ����ź�

        [DoNotSerialize]
        public ControlOutput SetOutput;//����֮���˳�

        [DoNotSerialize]
        public ValueInput Drive;//��Ҫ�������ĸ�Drive��
        [DoNotSerialize]
        public ValueInput OneBitCylinder;//�Ƿ�ʹ�õ�һ�źſ��ƣ�������ͨ��Out��ֵ����ʵ��������Ϊ
        [DoNotSerialize]
        public ValueInput MinPos;//��ʼλ��
        [DoNotSerialize]
        public ValueInput MaxPos;//����λ��
        [DoNotSerialize]
        public ValueInput TimeOut;//��������ʱ��
        [DoNotSerialize]
        public ValueInput TimeIn;//��������ʱ��
        [DoNotSerialize]
        public ValueInput StopWhenDrivingToMin;//�������жϵ���min
        [DoNotSerialize]
        public ValueInput StopWhenDrivingToMax;//�������жϵ���max
        [DoNotSerialize]
        public ValueInput Out;//����
        [DoNotSerialize]
        public ValueInput In;//����
        [DoNotSerialize]
        public ValueOutput IsOut;//�����������Ŀ�ĵ�λ�ã��ź�Ϊ��
        [DoNotSerialize]
        public ValueOutput IsIn;//�����������Ŀ�ĵ�λ�ã��ź�Ϊ��
        [DoNotSerialize]
        public ValueOutput IsMax;//�������maxλ�ã����ź�Ϊtrue��
        [DoNotSerialize]
        public ValueOutput IsMin;//�������minλ�ã����ź�Ϊtrue��
        [DoNotSerialize]
        public ValueOutput IsMovingOut;//���Drive��ǰ������ʻ�����ź�Ϊtrue��
        [DoNotSerialize]
        public ValueOutput IsMovingIn;//���Drive��ǰ������ʻ�����ź�Ϊtrue��

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

                //��������ʱִ��һ�Σ��൱��Start()
                if (flagStart == true)
                {
                    Cylinder = _Drive.GetComponent<Drive>();
                    if (Cylinder == null)
                        Debug.LogError("���Զ���ڵ�ȱ��ȱ��Drive�ű�");
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
                //���ڴ���������źŵĽ���
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

            //�ڽڵ��п��ӻ�����˿�
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
