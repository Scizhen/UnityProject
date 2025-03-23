
namespace RDTS
{
    //! FLOAT INPUT Value Object
    public class ValueInputFloat : Value
    {
        public StatusFloat Status;

        public float Value
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

        // When Script is added or reset ist pushed
        private void Reset()
        {
            Settings.Active = true;
            Settings.Override = false;
            Status.Value = 0;

        }

        public override void SetStatusConnected(bool status)
        {
            Status.Connected = status;
        }

        public override bool GetStatusConnected()
        {
            return Status.Connected;
        }


        public override string GetVisuText()
        {
            return Value.ToString("0.0");
        }

        public override bool IsInput()
        {
            return true;
        }

        //若是input类型，返回1
        public override int GetValueDirection()
        {
            return 1;
        }

        public override void SetValue(string value)
        {

            if (value != "")
                Status.Value = float.Parse(value);
            else
                Status.Value = 0;

        }

        //! Sets the value as an int
        public void SetValue(int value)
        {
            Value = value;
        }

        public override void SetValue(object value)
        {
            Value = (float)value;
        }

        public override object GetValue()
        {
            return Value;
        }

        //! Sets the value as a float
        public void SetValue(float value)
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
