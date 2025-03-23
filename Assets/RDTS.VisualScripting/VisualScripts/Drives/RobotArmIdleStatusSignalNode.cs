using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
namespace RDTS.VisualScripting
{
    [UnitSubtitle("��е�۵ȴ�״̬")]
    [UnitCategory("RDTS NodeLibrary/Drives")]
    public class RobotArmIdleStatusSignalNode : Unit
    {
        [DoNotSerialize]
        public ControlInput Set;//���ÿ����ź�

        [DoNotSerialize]
        public ControlOutput SetOutput;//����֮���˳�

        [DoNotSerialize]
        public ValueInput StartToPick;//���ÿ����źŵ��ӳ�ʱ��
        [DoNotSerialize]
        public ValueInput MoveToPick;//�����ź�
        [DoNotSerialize]
        public ValueInput PickFinish;//Ҫ���õ�ֵ
        [DoNotSerialize]
        public ValueInput PlaceFinish;//�����ź�
        [DoNotSerialize]
        public ValueOutput NextStatus;//Ҫ���õ�ֵ

        private bool __NextStatusFlag = false;

        protected override void Definition()
        {
            Set = ControlInput("SetValue", (flow) => {

                bool _StartToPick = flow.GetValue<bool>(StartToPick);
                var _MoveToPick = flow.GetValue<ValueInputInt>(MoveToPick);
                var _PickFinish = flow.GetValue<ValueOutputBool>(PickFinish);
                var _PlaceFinish = flow.GetValue<ValueOutputBool>(PlaceFinish);

                if (_StartToPick == true)
                {
                    _MoveToPick.Value = 1;
                    _PickFinish.Value = false;
                    _PlaceFinish.Value = false;
                    __NextStatusFlag = true;
                }

                return SetOutput;
            });

            SetOutput = ControlOutput("SetOutput");
            //�ڽڵ��п��ӻ�����˿�
            StartToPick = ValueInput<bool>("StartToPick",false);
            MoveToPick = ValueInput<ValueInputInt>("MoveToPick");
            PickFinish = ValueInput<ValueOutputBool>("PickFinish");
            PlaceFinish = ValueInput<ValueOutputBool>("PlaceFinish");
            NextStatus = ValueOutput<string>("NextStatus", (flow) =>
            {
                    string _NextStatus = "Idle";
                    if (__NextStatusFlag == true)
                    {
                        _NextStatus = "PickObject";

                    }

                    return _NextStatus;
            });
            
        }
    }
}
