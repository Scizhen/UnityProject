using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



namespace RDTS.VisualScripting
{
    [UnitCategory("RDTS NodeLibrary/Values")]
    public class ValueOutputBoolNode : Unit
    {

        [DoNotSerialize]
        public ControlInput Set;//设置值

        [DoNotSerialize]
        public ControlOutput SetOutput;//设置之后退出

        [DoNotSerialize]
        public ValueInput valueObject;//值对象
        [DoNotSerialize]
        public ValueInput newValue;//要设置的新值

        [DoNotSerialize]
        public ValueOutput getValue;//输出值对象的值


        private ValueOutputBool _valueScript;
        private bool _newValue;
        private bool _outputValue;


        protected override void Definition()
        {
            /* visual port and add logic 可视化端口并添加节点逻辑 */

            Set = ControlInput("SetValue", (flow) => {
                _valueScript = flow.GetValue<ValueOutputBool>(valueObject);
                _newValue = flow.GetValue<bool>(newValue);
                _valueScript.SetValue(_newValue);
                _outputValue = _valueScript.Value;

                return SetOutput;
            });

            SetOutput = ControlOutput("SetOutput");

            valueObject = ValueInput<ValueOutputBool>("valueObject");
            newValue = ValueInput<bool>("newValue", false);

            getValue = ValueOutput<bool>("getValue", (flow) =>
            {
                _valueScript = flow.GetValue<ValueOutputBool>(valueObject);
                _outputValue = _valueScript.Value;
                return _outputValue;
            }
            );


        }






    }

}