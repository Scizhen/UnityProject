using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using RDTS.Utility;

namespace RDTS.VisualScripting
{
    [UnitSubtitle("����Ŀ���������Ϊ")]
    [UnitCategory("RDTS NodeLibrary/Drives")]
    public class DriveDestinationMotorNode : Unit
    {

        [DoNotSerialize]
        public ControlInput Set;//���ÿ����ź�

        [DoNotSerialize]
        public ControlOutput SetOutput;//����֮���˳�

        [DoNotSerialize]
        public ValueInput Drive;//��Ҫ�������ĸ�Drive��
        [DoNotSerialize]
        public ValueInput StartDrive;//�����ƶ�����
        [DoNotSerialize]
        public ValueInput Destination;//���ü��ٶ�
        [DoNotSerialize]
        public ValueInput Acceleration;//����Ŀ���ٶ�
        [DoNotSerialize]
        public ValueInput TargetSpeed;//�����������Ŀ�ĵ�λ�ã��ź�Ϊ��
        [DoNotSerialize]
        public ValueOutput IsAtPosition;//��ǰ�����ٶ��źţ���λΪ����/��
        [DoNotSerialize]
        public ValueOutput IsAtSpeed;//��ǰ�����ٶ��źţ���λΪ����/��
        [DoNotSerialize]
        public ValueOutput IsAtDestination;//�������������Ŀ�ĵ���Ϊtrue
        [DoNotSerialize]
        public ValueOutput IsDriving;//���Drive��ǰ������ʻ�����ź�Ϊtrue��

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
                    Debug.LogError("���Զ���ڵ�ȱ��ȱ��Drive�ű�");

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

            //�ڽڵ��п��ӻ�����˿�
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
