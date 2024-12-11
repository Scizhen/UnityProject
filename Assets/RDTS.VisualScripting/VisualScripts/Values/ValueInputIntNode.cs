using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



namespace RDTS.VisualScripting
{
    [UnitCategory("RDTS NodeLibrary/Values")]
    public class ValueInputIntNode : Unit
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


        private ValueInputInt _valueScript;
        private int _newValue;
        private int _outputValue;


        protected override void Definition()
        {
            /* visual port and add logic 可视化端口并添加节点逻辑 */

            Set = ControlInput("SetValue", (flow) => {
                _valueScript = flow.GetValue<ValueInputInt>(valueObject);
                _newValue = flow.GetValue<int>(newValue);
                _valueScript.SetValue(_newValue);
                _outputValue = _valueScript.Value;

                return SetOutput;
            });

            SetOutput = ControlOutput("SetOutput");

            valueObject = ValueInput<ValueInputInt>("valueObject");
            newValue = ValueInput<int>("newValue", 0);

            getValue = ValueOutput<int>("getValue", (flow) =>
            {
                _valueScript = flow.GetValue<ValueInputInt>(valueObject);
                _outputValue = _valueScript.Value;
                return _outputValue;
            }
            );


        }






    }

}