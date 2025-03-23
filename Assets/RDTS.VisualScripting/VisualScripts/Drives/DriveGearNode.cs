using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using RDTS.Utility;

namespace RDTS.VisualScripting
{
    [UnitSubtitle("����Ŀ���������Ϊ")]
    [UnitCategory("RDTS NodeLibrary/Drives")]
    public class DriveGearNode : Unit
    {

        [DoNotSerialize]
        public ControlInput Set;//���ÿ����ź�

        [DoNotSerialize]
        public ControlOutput SetOutput;//����֮���˳�

        [DoNotSerialize]
        public ValueInput Drive;//��Ҫ�������ĸ�Drive��
        [DoNotSerialize]
        public ValueInput MasterDrive;//���ø���������
        [DoNotSerialize]
        public ValueInput GearFactor;//���÷Ŵ�ϵ��
        [DoNotSerialize]
        public ValueInput Offset;//����ƫ��

        private Drive _NewMasterDrive;
        private float _NewGearFactor;
        private float _NewOffset;
        private Drive getdrive;


        protected override void Definition()
        {
            Set = ControlInput("SetValue", (flow) => {

                var _Drive = flow.GetValue<GameObject>(Drive);

                getdrive = _Drive.GetComponent<Drive>();
                if (getdrive == null)
                    Debug.LogError("���Զ���ڵ�ȱ��ȱ��Drive�ű�");
                var _MasterDrive = flow.GetValue<GameObject>(MasterDrive);

                _NewMasterDrive = _MasterDrive.GetComponent<Drive>();
                if (_NewMasterDrive == null)
                    Debug.LogError("���Զ���ڵ�ȱ��ȱ��MasterDrive�ű�");

                _NewGearFactor = flow.GetValue<float>(GearFactor);
                _NewOffset = flow.GetValue<float>(Offset);

                getdrive.CurrentPosition = _NewMasterDrive.CurrentPosition * _NewGearFactor + _NewOffset;
                getdrive.CurrentSpeed = _NewMasterDrive.CurrentSpeed * _NewGearFactor;


                return SetOutput;
            });

            SetOutput = ControlOutput("SetOutput");

            //�ڽڵ��п��ӻ�����˿�
            Drive = ValueInput<GameObject>("Drive");
            MasterDrive = ValueInput<GameObject>("MasterDrive");
            GearFactor = ValueInput<float>("GearFactor", 1f);
            Offset = ValueInput<float>("Offset", 0f);


        }

    }


}
