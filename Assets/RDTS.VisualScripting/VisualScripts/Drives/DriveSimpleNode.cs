using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using RDTS.Utility;

namespace RDTS.VisualScripting
{
    [UnitSubtitle("���׵�������Ϊ")]
    [UnitCategory("RDTS NodeLibrary/Drives")]
    public class DriveSimpleNode : Unit
    {

        [DoNotSerialize]
        public ControlInput Set;//���ÿ����ź�

        [DoNotSerialize]
        public ControlOutput SetOutput;//����֮���˳�


        [DoNotSerialize]
        public ValueInput Drive;//��Ҫ�������ĸ�Drive��
        [DoNotSerialize]
        public ValueInput Speed;//�ٶ�
        [DoNotSerialize]
        public ValueInput Accelaration;//���ٶ�
        [DoNotSerialize]
        public ValueInput Forward;//ǰ��
        [DoNotSerialize]
        public ValueInput Backward;//����

        [DoNotSerialize]
        public ValueOutput IsAtPosition;//����Ŀǰλ��
        [DoNotSerialize]
        public ValueOutput IsAtSpeed;//����Ŀǰ�ٶ�
        [DoNotSerialize]
        public ValueOutput IsDriving;//����Ŀǰ����״̬


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
                    Debug.LogError("���Զ���ڵ�ȱ��ȱ��Drive�ű�");

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

            //�ڽڵ��п��ӻ�����˿�
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
