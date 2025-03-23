using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using RDTS.Utility;

namespace RDTS.VisualScripting
{
    [UnitSubtitle("λ�����������ٶȣ�")]
    [UnitCategory("RDTS NodeLibrary/Drives")]
    public class DriveFollowPositionNode : Unit
    {

        [DoNotSerialize]
        public ControlInput Set;//���ÿ����ź�

        [DoNotSerialize]
        public ControlOutput SetOutput;//����֮���˳�


        [DoNotSerialize]
        public ValueInput Drive;//��Ҫ�������ĸ�Drive��
        [DoNotSerialize]
        public ValueInput Position;//ָ��λ��
        [DoNotSerialize]
        public ValueInput Scale;//λ�ƷŴ�ϵ��
        [DoNotSerialize]
        public ValueInput Offset;//ƫ��

        [DoNotSerialize]
        public ValueOutput CurrentPosition;//����Ŀǰλ��


        private Drive getdrive;
        private float _NewPosition;
        private float _NewScale;
        private float _NewOffset;
        private float _NewCurrentPosition;


        protected override void Definition()
        {
            Set = ControlInput("SetValue", (flow) => {

                var _Drive = flow.GetValue<GameObject>(Drive);

                getdrive = _Drive.GetComponent<Drive>();
                if (getdrive == null)
                    Debug.LogError("���Զ���ڵ�ȱ��ȱ��Drive�ű�");

                _NewPosition = flow.GetValue<float>(Position);
                _NewScale = flow.GetValue<float>(Scale);
                _NewOffset = flow.GetValue<float>(Offset);
                getdrive.CurrentPosition = _NewPosition * _NewScale + _NewOffset;

                _NewCurrentPosition = getdrive.CurrentPosition;

                return SetOutput;
            });

            SetOutput = ControlOutput("SetOutput");

            //�ڽڵ��п��ӻ�����˿�
            Drive = ValueInput<GameObject>("Drive");
            Position = ValueInput<float>("Position", 0f);
            Scale = ValueInput<float>("Scale", 1f);
            Offset = ValueInput<float>("Offset", 0f);

            CurrentPosition = ValueOutput<float>("CurrentPosition", (flow) =>
            {
                return _NewCurrentPosition;
            });
        }

    }


}
