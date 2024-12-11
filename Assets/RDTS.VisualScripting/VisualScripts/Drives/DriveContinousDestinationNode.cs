using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using RDTS.Utility;

namespace RDTS.VisualScripting
{
    [UnitSubtitle("����Ŀ���������Ϊ")]
    [UnitCategory("RDTS NodeLibrary/Drives")]
    public class DriveContinousDestinationNode : Unit
    {

        [DoNotSerialize]
        public ControlInput Set;//���ÿ����ź�

        [DoNotSerialize]
        public ControlOutput SetOutput;//����֮���˳�

        [DoNotSerialize]
        public ValueInput Drive;//��Ҫ�������ĸ�Drive��
        [DoNotSerialize]
        public ValueInput Destination;//�����ƶ�����
        [DoNotSerialize]
        public ValueInput Acceleration;//���ü��ٶ�
        [DoNotSerialize]
        public ValueInput TargetSpeed;//����Ŀ���ٶ�
        [DoNotSerialize]
        public ValueOutput IsAtPosition;//�����������Ŀ�ĵ�λ�ã��ź�Ϊ��
        [DoNotSerialize]
        public ValueOutput IsAtSpeed;//��ǰ�����ٶ��źţ���λΪ����/��
        [DoNotSerialize]
        public ValueOutput IsAtDestination;//�������������Ŀ�ĵ���Ϊtrue
        [DoNotSerialize]
        public ValueOutput IsDriving;//���Drive��ǰ������ʻ�����ź�Ϊtrue��

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
                    Debug.LogError("���Զ���ڵ�ȱ��ȱ��Drive�ű�");

                //��ȡPLC�ź�
                //if (Destination != null)
                //    _NewDestination = flow.GetValue<ValueOutputFloat>(Destination).Value;
                //if (Acceleration != null)
                //    _NewAcceleration = flow.GetValue<ValueOutputFloat>(Acceleration).Value;
                //if (TargetSpeed != null)
                //    _NewTargetSpeed = flow.GetValue<ValueOutputFloat>(TargetSpeed).Value;

                //����������ֵ����������
                //if(flow.GetValue<float>(Destination) != 0)
                    _NewDestination = flow.GetValue<float>(Destination);
                //if (flow.GetValue<float>(Acceleration) != 100)
                    _NewAcceleration = flow.GetValue<float>(Acceleration);
                //if (flow.GetValue<float>(TargetSpeed) != 100)
                    _NewTargetSpeed = flow.GetValue<float>(TargetSpeed);

                if (_NewDestination != destinationbefore)
                    getdrive.DriveTo(_NewDestination);//����Drive�˶�
                getdrive.TargetSpeed = _NewTargetSpeed;
                getdrive.Acceleration = _NewAcceleration;


                destinationbefore = _NewDestination;
                return SetOutput;
            });

            SetOutput = ControlOutput("SetOutput");

            //�ڽڵ��п��ӻ�����˿�
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
