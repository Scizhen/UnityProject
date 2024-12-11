
using System;

namespace RDTS
{
    [System.Serializable]

    //! INT Output Value Object
    public class ValueOutputInt : Value
    {
        public StatusInt Status;
        private float _value;
        public int Value
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

        public override void SetStatusConnected(bool status)
        {
            Status.Connected = status;
        }

        public override bool GetStatusConnected()
        {
            return Status.Connected;
        }

        // When Script is added or reset ist pushed
        private void Reset()
        {
            Settings.Active = true;
            Settings.Override = false;
            Status.Value = 0;
        }

        public override void SetValue(string value)
        {
            if (value != "")
                Status.Value = int.Parse(value);
            else
                Status.Value = 0;


        }

        //! Sets the value as an int
        public void SetValue(int value)
        {
            Value = value;

        }

        //! Sets the value as an int
        public override void SetValue(object value)
        {
            if (value != null)
            {
                Type t = value.GetType();
                try
                {
                    Value = Convert.ToInt32(value);
                }
                catch
                { }
            }

        }

        public override object GetValue()
        {
            return Value;
        }

        public override string GetVisuText()
        {
            return Value.ToString("0");
        }

        //若是output类型，返回2
        public override int GetValueDirection()
        {
            return 2;
        }

        public void Update()
        {
            if (Status.OldValue != Status.Value)
            {
                EventValueChanged.Invoke(this);
                Status.OldValue = Status.Value;
            }
        }
    }
}