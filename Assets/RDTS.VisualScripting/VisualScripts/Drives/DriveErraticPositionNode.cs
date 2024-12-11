using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using RDTS.Utility;

namespace RDTS.VisualScripting
{
    [UnitSubtitle("������������Ϊ")]
    [UnitCategory("RDTS NodeLibrary/Drives")]
    public class DriveErraticPositionNode : Unit
    {

        [DoNotSerialize]
        public ControlInput Set;//���ÿ����ź�

        [DoNotSerialize]
        public ControlOutput SetOutput;//����֮���˳�

        [DoNotSerialize]
        public ValueInput Drive;//��Ҫ�������ĸ�Drive��
        [DoNotSerialize]
        public ValueInput MinPos;//������С�ƶ�����
        [DoNotSerialize]
        public ValueInput MaxPos;//��������ƶ�����
        [DoNotSerialize]
        public ValueInput Speed;//����Ŀ���ٶ�
        [DoNotSerialize]
        public ValueInput Driving;//�Ƿ�ʼ�˶�

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
                    Debug.LogError("���Զ���ڵ�ȱ��ȱ��Drive�ű�");

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

            //�ڽڵ��п��ӻ�����˿�
            Drive = ValueInput<GameObject>("Drive");
            MinPos = ValueInput<float>("MinPos", 0f);
            MaxPos = ValueInput<float>("MaxPos", 0f);
            Speed = ValueInput<float>("Speed", 100f);
            Driving = ValueInput<bool>("Stop Driving", false);

        }

    }


}
