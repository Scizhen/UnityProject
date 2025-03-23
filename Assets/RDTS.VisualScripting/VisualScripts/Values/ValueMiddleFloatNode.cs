using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



namespace RDTS.VisualScripting
{
    [UnitCategory("RDTS NodeLibrary/Values")]
    public class ValueMiddleFloatNode : Unit
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


        private ValueMiddleFloat _valueScript;
        private float _newValue;
        private float _outputValue;


        protected override void Definition()
        {
            /* visual port and add logic 可视化端口并添加节点逻辑 */

            Set = ControlInput("SetValue", (flow) => {
                _valueScript = flow.GetValue<ValueMiddleFloat>(valueObject);
                _newValue = flow.GetValue<float>(newValue);
                _valueScript.SetValue(_newValue);
                _outputValue = _valueScript.Value;

                return SetOutput;
            });

            SetOutput = ControlOutput("SetOutput");

            valueObject = ValueInput<ValueMiddleFloat>("valueObject");
            newValue = ValueInput<float>("newValue", .0f);

            getValue = ValueOutput<float>("getValue", (flow) =>
            {
                _valueScript = flow.GetValue<ValueMiddleFloat>(valueObject);
                _outputValue = _valueScript.Value;
                return _outputValue;
            }
            );


        }






    }

}