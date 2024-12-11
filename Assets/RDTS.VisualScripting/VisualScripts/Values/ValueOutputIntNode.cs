using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



namespace RDTS.VisualScripting
{
    [UnitCategory("RDTS NodeLibrary/Values")]
    public class ValueOutputIntNode : Unit
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


        private ValueOutputInt _valueScript;
        private int _newValue;
        private int _outputValue;


        protected override void Definition()
        {
            /* visual port and add logic ���ӻ��˿ڲ���ӽڵ��߼� */

            Set = ControlInput("SetValue", (flow) => {
                _valueScript = flow.GetValue<ValueOutputInt>(valueObject);
                _newValue = flow.GetValue<int>(newValue);
                _valueScript.SetValue(_newValue);
                _outputValue = _valueScript.Value;

                return SetOutput;
            });

            SetOutput = ControlOutput("SetOutput");

            valueObject = ValueInput<ValueOutputInt>("valueObject");
            newValue = ValueInput<int>("newValue", 0);

            getValue = ValueOutput<int>("getValue", (flow) =>
            {
                _valueScript = flow.GetValue<ValueOutputInt>(valueObject);
                _outputValue = _valueScript.Value;
                return _outputValue;
            }
            );


        }





    }

}