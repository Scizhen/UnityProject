using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



namespace RDTS.VisualScripting
{
    [UnitCategory("RDTS NodeLibrary/Values")]
    public class ValueOutputFloatNode : Unit
    {

        [DoNotSerialize]
        public ControlInput Set;//����ֵ

        [DoNotSerialize]
        public ControlOutput SetOutput;//����֮���˳�

        [DoNotSerialize]
        public ValueInput valueObject;//ֵ����
        [DoNotSerialize]
        public ValueInput newValue;//Ҫ���õ���ֵ

        [DoNotSerialize]
        public ValueOutput getValue;//���ֵ�����ֵ


        private ValueOutputFloat _valueScript;
        private float _newValue;
        private float _outputValue;


        protected override void Definition()
        {
            /* visual port and add logic ���ӻ��˿ڲ����ӽڵ��߼� */

            Set = ControlInput("SetValue", (flow) => {
                _valueScript = flow.GetValue<ValueOutputFloat>(valueObject);
                _newValue = flow.GetValue<float>(newValue);
                _valueScript.SetValue(_newValue);
                _outputValue = _valueScript.Value;

                return SetOutput;
            });

            SetOutput = ControlOutput("SetOutput");

            valueObject = ValueInput<ValueOutputFloat>("valueObject");
            newValue = ValueInput<float>("newValue", .0f);

            getValue = ValueOutput<float>("getValue", (flow) =>
            {
                _valueScript = flow.GetValue<ValueOutputFloat>(valueObject);
                _outputValue = _valueScript.Value;
                return _outputValue;
            }
            );


        }





    }

}