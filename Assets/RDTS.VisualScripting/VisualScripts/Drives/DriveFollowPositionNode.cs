using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using RDTS.Utility;

namespace RDTS.VisualScripting
{
    [UnitSubtitle("位移驱动（无速度）")]
    [UnitCategory("RDTS NodeLibrary/Drives")]
    public class DriveFollowPositionNode : Unit
    {

        [DoNotSerialize]
        public ControlInput Set;//设置控制信号

        [DoNotSerialize]
        public ControlOutput SetOutput;//设置之后退出


        [DoNotSerialize]
        public ValueInput Drive;//需要搭载在哪个Drive上
        [DoNotSerialize]
        public ValueInput Position;//指定位移
        [DoNotSerialize]
        public ValueInput Scale;//位移放大系数
        [DoNotSerialize]
        public ValueInput Offset;//偏移

        [DoNotSerialize]
        public ValueOutput CurrentPosition;//反馈目前位置


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
                    Debug.LogError("该自定义节点缺少缺少Drive脚本");

                _NewPosition = flow.GetValue<float>(Position);
                _NewScale = flow.GetValue<float>(Scale);
                _NewOffset = flow.GetValue<float>(Offset);
                getdrive.CurrentPosition = _NewPosition * _NewScale + _NewOffset;

                _NewCurrentPosition = getdrive.CurrentPosition;

                return SetOutput;
            });

            SetOutput = ControlOutput("SetOutput");

            //在节点中可视化输入端口
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
