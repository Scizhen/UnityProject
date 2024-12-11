using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using RDTS.Utility;

namespace RDTS.VisualScripting
{
    [UnitSubtitle("连续目标点驱动行为")]
    [UnitCategory("RDTS NodeLibrary/Drives")]
    public class DriveGearNode : Unit
    {

        [DoNotSerialize]
        public ControlInput Set;//设置控制信号

        [DoNotSerialize]
        public ControlOutput SetOutput;//设置之后退出

        [DoNotSerialize]
        public ValueInput Drive;//需要搭载在哪个Drive上
        [DoNotSerialize]
        public ValueInput MasterDrive;//设置父对象驱动
        [DoNotSerialize]
        public ValueInput GearFactor;//设置放大系数
        [DoNotSerialize]
        public ValueInput Offset;//设置偏移

        private Drive _NewMasterDrive;
        private float _NewGearFactor;
        private float _NewOffset;
        private Drive getdrive;


        protected override void Definition()
        {
            Set = ControlInput("SetValue", (flow) => {

                var _Drive = flow.GetValue<GameObject>(Drive);

                getdrive = _Drive.GetComponent<Drive>();
                if (getdrive == null)
                    Debug.LogError("该自定义节点缺少缺少Drive脚本");
                var _MasterDrive = flow.GetValue<GameObject>(MasterDrive);

                _NewMasterDrive = _MasterDrive.GetComponent<Drive>();
                if (_NewMasterDrive == null)
                    Debug.LogError("该自定义节点缺少缺少MasterDrive脚本");

                _NewGearFactor = flow.GetValue<float>(GearFactor);
                _NewOffset = flow.GetValue<float>(Offset);

                getdrive.CurrentPosition = _NewMasterDrive.CurrentPosition * _NewGearFactor + _NewOffset;
                getdrive.CurrentSpeed = _NewMasterDrive.CurrentSpeed * _NewGearFactor;


                return SetOutput;
            });

            SetOutput = ControlOutput("SetOutput");

            //在节点中可视化输入端口
            Drive = ValueInput<GameObject>("Drive");
            MasterDrive = ValueInput<GameObject>("MasterDrive");
            GearFactor = ValueInput<float>("GearFactor", 1f);
            Offset = ValueInput<float>("Offset", 0f);


        }

    }


}
