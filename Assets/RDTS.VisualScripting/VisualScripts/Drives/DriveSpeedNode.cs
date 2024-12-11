using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using RDTS.Utility;

namespace RDTS.VisualScripting
{
    [UnitSubtitle("�ٶ�����ģ��")]
    [UnitCategory("RDTS NodeLibrary/Drives")]
    public class DriveSpeedNode : Unit
    {

        [DoNotSerialize]
        public ControlInput Set;//���ÿ����ź�

        [DoNotSerialize]
        public ControlOutput SetOutput;//����֮���˳�


        [DoNotSerialize]
        public ValueInput Drive;//��Ҫ�������ĸ�Drive��
        [DoNotSerialize]
        public ValueInput TargetSpeed;//Ŀ���ٶ�
        [DoNotSerialize]
        public ValueInput Accelaration;//���ٶ�

        [DoNotSerialize]
        public ValueOutput CurrentSpeed;//����Ŀǰ�ٶ�
        [DoNotSerialize]
        public ValueOutput CurrentPosition;//����Ŀǰλ��
        [DoNotSerialize]
        public ValueOutput IsDriving;//����Ŀǰ����״̬


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
                    Debug.LogError("���Զ���ڵ�ȱ��Drive�ű�");

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

            //�ڽڵ��п��ӻ�����˿�
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
