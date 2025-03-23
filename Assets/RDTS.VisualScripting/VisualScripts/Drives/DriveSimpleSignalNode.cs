using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using RDTS.Utility;

namespace RDTS.VisualScripting
{
    [UnitSubtitle("模拟气缸驱动的节点")]
    [UnitCategory("RDTS NodeLibrary/Drives")]
    public class DriveSimpleSignalNode : Unit
    {

        [DoNotSerialize]
        public ControlInput Set;//设置控制信号

        [DoNotSerialize]
        public ControlOutput SetOutput;//设置之后退出

        [DoNotSerialize]
        public ValueInput Delay;//设置控制信号的延迟时间
        [DoNotSerialize]
        public ValueInput Out;//控制信号
        [DoNotSerialize]
        public ValueInput newOutValue;//要设置的值
        [DoNotSerialize]
        public ValueInput In;//控制信号
        [DoNotSerialize]
        public ValueInput newInValue;//要设置的值
        [DoNotSerialize]
        public ValueInput IsOut;//输出控制状态的信号
        [DoNotSerialize]
        public ValueInput IsIn;//输出控制状态的信号



        [DoNotSerialize]
        public ValueOutput GetIsOutValue;//输出控制状态的值
        [DoNotSerialize]
        public ValueOutput GetIsInValue;//输出控制状态的值



        private MonoBehaviour coroutineRunner { get; set; }
        private Coroutine Coro;//延迟时间的协程



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

            //在节点中可视化输入端口
            Delay = ValueInput<float>("delayTime/s", 0f);
            Out = ValueInput<ValueOutputBool>("Out");
            newOutValue = ValueInput<bool>("newOutValue", false);
            In = ValueInput<ValueOutputBool>("In");
            newInValue = ValueInput<bool>("newInValue", false);

            IsOut = ValueInput<ValueInputBool>("IsOut");
            IsIn = ValueInput<ValueInputBool>("IsIn");

            //可视化并输出两个信号值
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

