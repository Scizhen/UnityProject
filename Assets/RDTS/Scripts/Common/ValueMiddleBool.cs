using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RDTS
{
    public class ValueMiddleBool : Value
    {

        //! bool类型的状态结构
        public StatusBool Status;

        //! 设置/获取值
        public bool Value
        {
            get
            {
                if (Settings.Override)
                {
                    return Status.ValueOverride;
                }
                else
                {
                    return Status.Value;
                }
            }
            set
            {
                var oldvalue = Status.Value;
                Status.Value = value;
                if (oldvalue != value)
                    ValueChangedEvent(this);
            }
        }

        // 当脚本被添加或重载时
        private void Reset()
        {
            Settings.Active = true;
            Settings.Override = false;
            Status.Value = false;
            Status.OldValue = false;
        }

        /// <summary>
        /// 在Hierarchy面板中对Value标签的鼠标点击操作
        /// </summary>
        public override void OnToggleHierarchy()
        {
            if (Settings.Override == false)
                Settings.Override = true;
            Status.ValueOverride = !Status.ValueOverride;//值反转
            EventValueChanged.Invoke(this);
            ValueChangedEvent(this);
        }

        //! Sets the Status connected
        public override void SetStatusConnected(bool status)
        {
            Status.Connected = status;
        }

        //! Gets the status connected
        public override bool GetStatusConnected()
        {
            return Status.Connected;
        }

        //! Gets the text for displaying it in the hierarchy view
        public override string GetVisuText()
        {
            return Value.ToString();
        }

        //! True if value is input 若是input类型，则返回true
        public override bool IsInput()
        {
            return false;
        }

        //若是Middel类型，返回3
        public override int GetValueDirection()
        {
            return 3;
        }

        //! 将“值对象”的值设为string类型
        public override void SetValue(string value)
        {
            if (value != "")
            {
                if (value == "0")
                {
                    Value = false;
                    return;
                }

                if (value == "1")
                {
                    Value = true;
                    return;
                }
                Value = bool.Parse(value);
            }
            else
                Value = false;
        }

        public override void SetValue(object value)
        {
            if (value != null)
                Value = (bool)value;
        }

        public override object GetValue()
        {
            return Value;
        }

        //! Sets the Value as a bool
        public void SetValue(bool value)
        {
            Value = value;
        }

        public void Update()
        {
            if (Status.OldValue != Status.Value)
            {
                if (EventValueChanged != null)
                    EventValueChanged.Invoke(this);
                Status.OldValue = Status.Value;
            }
        }

    }

}
