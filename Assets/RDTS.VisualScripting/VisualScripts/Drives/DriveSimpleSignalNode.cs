using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using RDTS.Utility;

namespace RDTS.VisualScripting
{
    [UnitSubtitle("ģ�����������Ľڵ�")]
    [UnitCategory("RDTS NodeLibrary/Drives")]
    public class DriveSimpleSignalNode : Unit
    {

        [DoNotSerialize]
        public ControlInput Set;//���ÿ����ź�

        [DoNotSerialize]
        public ControlOutput SetOutput;//����֮���˳�

        [DoNotSerialize]
        public ValueInput Delay;//���ÿ����źŵ��ӳ�ʱ��
        [DoNotSerialize]
        public ValueInput Out;//�����ź�
        [DoNotSerialize]
        public ValueInput newOutValue;//Ҫ���õ�ֵ
        [DoNotSerialize]
        public ValueInput In;//�����ź�
        [DoNotSerialize]
        public ValueInput newInValue;//Ҫ���õ�ֵ
        [DoNotSerialize]
        public ValueInput IsOut;//�������״̬���ź�
        [DoNotSerialize]
        public ValueInput IsIn;//�������״̬���ź�



        [DoNotSerialize]
        public ValueOutput GetIsOutValue;//�������״̬��ֵ
        [DoNotSerialize]
        public ValueOutput GetIsInValue;//�������״̬��ֵ



        private MonoBehaviour coroutineRunner { get; set; }
        private Coroutine Coro;//�ӳ�ʱ���Э��



        protected override void Definition()
        {
            Set = ControlInput("SetValue", (flow) => {

                float _delay = flow.GetValue<float>(Delay);
                var _Out = flow.GetValue<ValueOutputBool>(Out);
                var _In = flow.GetValue<ValueOutputBool>(In);
                bool _newOutValue = flow.GetValue<bool>(newOutValue);
                bool _newInValue = flow.GetValue<bool>(newInValue);

                if (_Out != null)
                {
                    _Out.SetValue(_newOutValue);
                    /// QM.Log(_newOutValue.ToString());
                }

                if (_In != null)
                    _In.SetValue(_newInValue);



                return SetOutput;
            });

            SetOutput = ControlOutput("SetOutput");

            //�ڽڵ��п��ӻ�����˿�
            Delay = ValueInput<float>("delayTime/s", 0f);
            Out = ValueInput<ValueOutputBool>("Out");
            newOutValue = ValueInput<bool>("newOutValue", false);
            In = ValueInput<ValueOutputBool>("In");
            newInValue = ValueInput<bool>("newInValue", false);

            IsOut = ValueInput<ValueInputBool>("IsOut");
            IsIn = ValueInput<ValueInputBool>("IsIn");

            //���ӻ�����������ź�ֵ
            GetIsOutValue = ValueOutput<bool>("GetIsOutValue", (flow) =>
            {
                var _isOut = flow.GetValue<ValueInputBool>(IsOut);
                bool _value = false;
                if (_isOut != null)
                    _value = _isOut.Value;

                return _value;
            }
            );
            GetIsInValue = ValueOutput<bool>("GetIsInValue", (flow) =>
            {
                var _isIn = flow.GetValue<ValueInputBool>(IsIn);
                bool _value = false;
                if (_isIn != null)
                    _value = _isIn.Value;

                return _value;
            }
           );


        }




    }


}

