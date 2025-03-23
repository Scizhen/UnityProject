using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RDTS
{
    public class ValueMiddleBool : Value
    {

        //! bool���͵�״̬�ṹ
        public StatusBool Status;

        //! ����/��ȡֵ
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

        // ���ű�����ӻ�����ʱ
        private void Reset()
        {
            Settings.Active = true;
            Settings.Override = false;
            Status.Value = false;
            Status.OldValue = false;
        }

        /// <summary>
        /// ��Hierarchy����ж�Value��ǩ�����������
        /// </summary>
        public override void OnToggleHierarchy()
        {
            if (Settings.Override == false)
                Settings.Override = true;
            Status.ValueOverride = !Status.ValueOverride;//ֵ��ת
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

        //! True if value is input ����input���ͣ��򷵻�true
        public override bool IsInput()
        {
            return false;
        }

        //����Middel���ͣ�����3
        public override int GetValueDirection()
        {
            return 3;
        }

        //! ����ֵ���󡱵�ֵ��Ϊstring����
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
